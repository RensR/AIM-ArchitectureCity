namespace Framework.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Framework.Models.Nodes;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using Neo4jClient;

    using Newtonsoft.Json.Serialization;

    public class Neo4JContext : DbContext
    {
        private const int MaxWriteCount = 10_000;

        public GraphClient Client { get; }

        private readonly ILogger logger;

        public Neo4JContext(ILogger logger)
        {
            this.logger = logger;
            Client = new GraphClient(new Uri("http://localhost:7474/db/data"), "neo4j", "memphis")
            {
                JsonContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            try
            {
                Client.Connect();
            }
            catch (System.Net.Http.HttpRequestException e)
            {
                this.logger.LogError($"Database Neo4j not found, please check if it is online. {e.InnerException}");
            }
        }

        public async Task InsertAiAsync(SimpleNodeModel newNode)
        {
            await Client.Cypher
                .Merge("(id: UniqueId {name: 'node'})")
                .OnCreate().Set("id.count = 1")
                .OnMatch().Set("id.count = id.count + 1")
                .With("id.str + id.count AS uid")
                .Create("(p: node{ newNode})").Set("(p.id = uid) ").WithParam("newNode", newNode)
                .ExecuteWithoutResultsAsync();
        }

        public long CreateNode(List<SimpleNodeModel> nodes)
        {
            long lastInsert = 0;
            for (var i = 0; i < nodes.Count; i += MaxWriteCount)
            {
                var sublist = nodes.GetRange(i, Math.Min(nodes.Count - i, MaxWriteCount));
                
                foreach (var simpleNode in sublist)
                {
                    simpleNode.ID = AutoIncrement();
                    lastInsert = simpleNode.ID;
                }

                Client.Cypher
                    .Unwind(sublist, "node")
                    .Merge("(n:Node { id: node.id })")
                    .OnCreate()
                    .Set("n = node")
                    .ExecuteWithoutResults();
            }

            return lastInsert;
        }

        public void CreateNodeByName(List<SimpleNodeModel> nodes)
        {
            for (var i = 0; i < nodes.Count; i += MaxWriteCount)
            {
                var sublist = nodes.GetRange(i, Math.Min(nodes.Count - i, MaxWriteCount));

                Client.Cypher
                    .Unwind(sublist, "node")
                    .Merge("(n:Node { label: node.label })")
                    .OnCreate()
                    .Set("n = node")
                    .ExecuteWithoutResults();
            }
        }

        public SimpleNodeModel Get(int id)
        {
            var query = Client.Cypher
                .Match("(n:Node)")
                .Where((SimpleNodeModel n) => n.ID == id)
                .Return(n => n.As<SimpleNodeModel>()).Results;

            return query.FirstOrDefault();
        }

        public SimpleNodeModel Get(string label)
        {
            var query = Client.Cypher
                .Match("(n:Node)")
                .Where((SimpleNodeModel n) => n.Label == label)
                .Return(n => n.As<SimpleNodeModel>()).Results;

            return query.FirstOrDefault();
        }

        public IEnumerable<SimpleNodeModel> GetAll()
        {
            return Client.Cypher
                    .Match("(n:Node)")
                    .Where((SimpleNodeModel n) => true)
                    .Return(n => n.As<SimpleNodeModel>()).Results;
        }

        public async Task UpdateAsync(SimpleNodeModel node)
        {
            await Client.Cypher
                    .WithParams(new
                    {
                        id = node.ID, node
                    })
                    .Merge("(n:Node { id: {id} })")
                    .OnCreate()
                    .Set("n = {node}")
                    .ExecuteWithoutResultsAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await Client.Cypher
                    .Match("(n:Node)")
                    .Where((SimpleNodeModel n) => n.ID == id)
                    .DetachDelete("(n)")
                    .ExecuteWithoutResultsAsync();
        }

        public async Task InsertEdge(int nodeIDa, int nodeIDb, string relType, int weight = 0, bool twoWay = false)
        {
            var query = Client.Cypher
                .Match("(p1:Node)", "(p2:Node)")
                .Where((SimpleNodeModel p1) => p1.ID == nodeIDa)
                .AndWhere((SimpleNodeModel p2) => p2.ID == nodeIDb)
                .CreateUnique($"(p1)-[: {relType}{{weight: {weight }}}]->(p2)");

            if (twoWay)
                query = query.CreateUnique("(p1)<-[:" + relType + "]-(p2)");

            await query.ExecuteWithoutResultsAsync();
        }

        public async Task InsertEdgeByName(string nodeNamea, string nodeNameb, string relType, int weight = 0,  bool twoWay = false)
        {
            var query = Client.Cypher
                .Match("(p1:Node)", "(p2:Node)")
                .Where((SimpleNodeModel p1) => p1.Label == nodeNamea)
                .AndWhere((SimpleNodeModel p2) => p2.Label == nodeNameb)
                .CreateUnique($"(p1)-[: {relType}{{weight: {weight }}}]->(p2)");

            if (twoWay)
                query = query.CreateUnique("(p1)<-[:" + relType + "]-(p2)");

            await query.ExecuteWithoutResultsAsync();
            Client.EndTransaction();
        }

        public async Task InsertEdgeByName(SimpleNodeModel nodea, SimpleNodeModel nodeb, string relType, int weight = 0, bool twoWay = false)
        {
            await InsertEdgeByName(nodea.Label, nodeb.Label, relType, weight, twoWay);
        }

        public async Task CsvInsertNodes(List<SimpleNodeModel> nodes, string type = "Node")
        {
            using (var sw = new StreamWriter(File.Open($"temp-{type}.csv", FileMode.Create)))
            {
                sw.Write(SimpleNodeModel.Header + "\n");
                foreach (var simpleNodeModel in nodes)
                {
                    sw.Write(simpleNodeModel.ToCsvWithoutId() + "source\n");
                }
            }

            var f = new FileInfo($"temp-{type}.csv");
            Client.Cypher
                .Create("INDEX ON :Node(label);")
                .ExecuteWithoutResults();

            await Client.Cypher
                .LoadCsv(new Uri("file://" + f.FullName), "csvNode", true, periodicCommit: 5000)
                .Merge($"(n:{type} {{label:csvNode.label, source:csvNode.source, name:csvNode.name}})")
                .ExecuteWithoutResultsAsync();

            Client.Dispose();
        }

        public async Task InsertEdgesByID(
            List<int> nodeListA, 
            List<int> nodeListB,
            List<int> weightList, 
            string type)
        {
            using (var sw = new StreamWriter(File.Open($"tempEdge-{type}.csv", FileMode.Create)))
            {
                sw.Write("From,To,Weight\n");
                for (var j = 0; j < nodeListA.Count; j++)
                {
                    sw.Write($"{nodeListA[j]}," +
                             $"{nodeListB[j]}," +
                             $"{weightList[j]}" +
                             $"\n");
                }
            }

            var f = new FileInfo($"tempEdge-{type}.csv");

            await Client.Cypher
                .LoadCsv(new Uri("file://" + f.FullName), "rels", true, periodicCommit: 500)
                .Match("(from {label: rels.From}), (to {label: rels.To})")
                .Create($"(from)-[:{type} {{weight: rels.Weight}}]->(to);")
                .ExecuteWithoutResultsAsync();
            Client.Dispose();
        }

        public void ClearDb()
        {
            try
            {
                Client.Cypher
                    .Match("(n)")
                    .DetachDelete("n")
                    .ExecuteWithoutResults();
            }
            catch (InvalidOperationException ioe)
            {
                this.logger.LogCritical($"Database server most likely not running. Please start the Neo4j Client. Error: {ioe.InnerException}");
                throw;
            }
        }

        private long AutoIncrement()
        {
            return Client.Cypher
                .Merge("(id: AutoIncrement {name: 'node'})")
                .OnCreate().Set("id.AICount = 1")
                .OnMatch().Set("id.AICount = id.AICount + 1")
                .With("id.AICount as AI")
                .Return(ai => ai.As<long>())
                .Results
                .First();
        }
    }

    public class AutoIncrement
    {
        public string Name { get; set; }

        public int AiCount { get; set; }
    }
}

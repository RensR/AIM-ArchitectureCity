namespace Framework.Data
{
    using Models;

    using Microsoft.EntityFrameworkCore;

    public class PluginContext : DbContext
    {
        public PluginContext(DbContextOptions<PluginContext> options)
            : base(options)
        {
        }

        public DbSet<PluginDescription> PluginDescription { get; set; }

        public DbSet<RmrFileTypeModel> RmrFileTypeModel { get; set; }

        public DbSet<Event> Event { get; set; }
    }
}

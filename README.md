## ArchitectureCity

We propose ArchitectureCity, a variation on CodeCity built on dynamic aspects of the software system. ArchitectureCity allows for a clear, high-level overview of a systems dynamic aspects e.g. call counts and application flow. It was constructed using existing software architecture reconstruction techniques as well as new clustering algorithms to aid in the visualization. ArchitectureCity does not require any source code to run, as it is supplied solely log data. This means all information is completely based on true system performance.


### Thesis
This is part of the Master Thesis of Rens Rooimans. It was produces in 2017 under guidance of J.M.E.M van der Werf and J. Hage of Utrecht University.
Minimal updates have been performed since to keep the project up-to-date.


## AIM - Architectural Intelligence Mining Framework
AIM can run on any operating system as the underlying .NET Core framework is platform independent. As of now it does require Visual Studio 2017 to run the source code as earlier versions do not support the .NET Core framework. 

### General Dependencies

The project requires Dotnet Core 2.0 to run, this can be installed from https://www.microsoft.com/net/download/windows. For a barebones version of AIM we only need to install the NPM packages, and the NuGet packages as specified in the project files. Gulp takes care of the merging and transportation of the .js and .css files to the wwwroot. 


Either Neo4j or OpenSCAD and Graphviz are needed to produce any visual output. We highly recommend installing them both to make use of the full feature suite of AIM.

#### Neo4j
Neo4j is needed for the workflow net. The system will not run the visualization without a properly configured Neo4J database
~~~
uri = http://localhost:7474/db/data
login = neo4j
pass = memphis
~~~

We highly recommend you change these defaults in the Neo4jContext.cs file. There is an entire wrapper included in the Neo4jContext so that more functionallity can be added with minimal effort. 

#### OpenSCAD
OpenSCAD is needed for visualizing he .scad files that ArchitectureCity produces. The system will run without any trouble and create the .scad files without OpenScad installed. It will also display the contents of the .scad file on screen after each run for easy downloading.

#### Graphviz 
Graphviz should be able to be called from the command line. Graphviz is needed when using the ArchitectureCity visualization as it calculates the position of the buildings and the roads. When Graphviz is nog properly installed, there will be an error message telling the user to download the correct software and add it to the path.
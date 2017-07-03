## ArchitectureCity

We propose ArchitectureCity, a variation on CodeCity built on dynamic aspects of the software system. ArchitectureCity allows for a clear, high-level overview of a systems dynamic aspects e.g. call counts and application flow. It was constructed using existing software architecture reconstruction techniques as well as new clustering algorithms to aid in the visualization. ArchitectureCity does not require any source code to run, as it is supplied solely log data. This means all information is completely based on true system performance.


### Thesis
This is part of the Master Thesis of Rens Rooimans. It was produces in 2017 under guidance of J.M.E.M van der Werf and J. Hage of Utrecht University.



## AIM - Architectural Intelligence Mining Framework
AIM can run on any operating system as the underlying Dotnet Core framework is platform independent. As of now you do require Visual Studio 2017 to run the source code. 

### General Dependencies
Either Neo4j or OpenSCAD and Graphviz are needed to produce any visual output. We highly recommend installing them all to make use of the full feature suite of AIM.

#### Neo4j
Neo4j for the workflow net. The system will not run the visualization without a properly configured Neo4J database
~~~
uri = http://localhost:7474/db/data
login = neo4j
pass = memphis
~~~

We highly recommend you change these defaults in the Neo4jContext.cs file.

#### OpenSCAD
OpenSCAD for visualizing .scad files (ArchitectureCity). The system will run without any trouble and create the .scad files without OpenScad installed.

#### Graphviz 
Graphviz should be able to be called from CLI. Graphviz is only needed when using the ArchitectureCity visualization, but it will silently fail when not properly installed. When the ArchitectureCity yields an empty file you should check your Graphviz configuration.


#### System
Install Dotnet Core 1.0

Use Visual Studio 2017

Install the NuGet packages

~~~~
bower install
~~~~
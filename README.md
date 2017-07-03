## AIM - Architectural Intelligence Mining Framework
AIM can run on any operating system as the underlying Dotnet Core framework is platform independent. As of now you do require Visual Studio 2017 to run the source code. 

### General Dependencies
Graphviz in path (callable from a CLI)

For visualizations
Neo4j (Workflow Net)
OpenSCAD for visualizing .scad files (ArchitectureCity)


#### Windows
Install Dotnet Core 1.0

Use Visual Studio 2017

Install the NuGet packages

~~~~
bower install
~~~~


### Database


NuGet console

~~~~
cd to the folder with .csproj
 dotnet ef migrations add _MigrationName_ --context _MigrationContext_
 dotnet ef database update --context _MigrationContext_

 e.g.
 dotnet ef migrations add RMRMigration --context PluginContext
 dotnet ef database update --context PluginContext


Update-Database -context PluginContext
~~~~

Linux cli
~~~~
Add-Migration MyFirstMigration --context PluginContext

Update-Database --context PluginContext
~~~~

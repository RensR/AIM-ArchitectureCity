# PluginDescription JSON

A plugindescription file should be a JSON formatted file including, but not limited to, the following fields. The file serves as an identifier for the DLL file.

### Name
The name for the plugin. At this time there can only be a singele unique pluging with a name, so if you want to develop a suite please use the following format.

~~~
MyOwnSoftwareParser
MyOwnSoftwareAnalyser
MyOwnSoftwareVisualiser
~~~

### Type
The type determines what kind of plugin it is:
~~~
0: Parser
1: Analyser
2: Visualiser
~~~

### Description
The description is a short text stating what the plugin does. This is visible in the plugin selection screen.

### Version
The current version of the plugin installed. Descriptions will only update on version changed. This can be any string.

### Author
The author of the plugin.

### Other Fields
Other fields can be added, as they are ignored by the software. A longer description or a readme can be included in the file. A readme can also be included in the folder itself in the form of a markdown, pdf or web based format.

## Example
~~~
{
	"Name":"UltiParse",
	"Type":0,
	"Description":"Why use anything less? UltimParse is the ultimate parser",
	"Version":"0.13.3",
	"Author":"D.A. Haar"
}
~~~

# Plugin DLL

The DLL should be C# classes the implement an existing interface of Framework. 
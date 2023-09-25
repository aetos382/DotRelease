using System.CommandLine;

using DotRelease;

var rootCommand = new MainCommand();
var commandLine = new CliConfiguration(rootCommand);

return commandLine.Invoke(args);

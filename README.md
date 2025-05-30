# CmdStarter

[![Build and Test](https://github.com/erinloy/CmdStarter/actions/workflows/build.yml/badge.svg)](https://github.com/erinloy/CmdStarter/actions/workflows/build.yml)
[![Create Release](https://github.com/erinloy/CmdStarter/actions/workflows/release.yml/badge.svg)](https://github.com/erinloy/CmdStarter/actions/workflows/release.yml)

This library is a layer over [System.CommandLine](https://github.com/dotnet/command-line-api) to ease integration 
into existing projects. Currently, this dependency is still in beta version, hence this library's version will stay 
in beta too.

## Links

- Article: https://www.codeproject.com/Articles/5361921/Integrate-System-CommandLine-Easily-with-CmdStarte
- Nuget package: https://www.nuget.org/packages/cints.CmdStarter
- Jira: https://cyberinternauts.atlassian.net/browse/CMD

## Features
- Implement commands using an abstract class or an interface
- Filter classes to use in current execution by namespaces or by full class names
- Classes using dependency injection are supported
- Mark classes as global options container
- Easy access to the global options inside the executing method
- Lonely command can be rooted
- Autowiring properties to System.CommandLine command options
- Autowiring executing method parameters to System.CommandLine command arguments
- Alias, Hidden, Description and AutoComplete attributes are offered to set command options/arguments properties
- Automatic commands tree loading via namespaces or Parent|Children attributes
- Resilient to assembly load errors, allowing for graceful degradation when malformed assemblies are encountered

## Usage

- Import the [nuget package **cints.CmdStarter**](https://www.nuget.org/packages/cints.CmdStarter). 
  > Ensure to check *Prerelease* checkbox
- Command integration (Choose one):
  - Create a new class inheriting from `StarterCommand`.
  - Add `IStarterCommand` interface to an existing class having a constructor without parameter.
     > For dependency injection, see below.
- Create the Program class below.

```
internal class Program
{
    public static async Task Main(string[] args)
    {
        var starter = new CmdStarter.Lib.Starter();
        await starter.Start(args);
    }
}
```

### Dependency injection

Those methods allow classes with a constructor having parameters.
- `IStarterCommand.GetInstance` method can be overridden
- `Starter.SetFactory` can be used to change the default behavior of instantiation
- `(new Starter()).Start(IServiceManager, string[])` can be used having an object implementing `IServiceManager`

Any of your preferred library can be used. This repository includes an example with Simple Injector.

### Assembly Load Error Handling

CmdStarter is designed to be resilient when loading assemblies that might be malformed or have issues. The `AssemblyLoadErrorHandler` class provides control over how these errors are handled:

```csharp
// Access the error handler through the Starter instance
var starter = new CmdStarter.Lib.Starter();

// Configure the error handling mode
starter.AssemblyLoadErrorHandler.Mode = AssemblyLoadErrorHandler.ErrorHandlingMode.RaiseEvent;

// Subscribe to error events if needed
starter.AssemblyLoadErrorHandler.AssemblyLoadError += (sender, args) => 
{
    Console.WriteLine($"Assembly load error: {args.Exception.Message}");
};

starter.AssemblyLoadErrorHandler.TypeLoadError += (sender, args) => 
{
    Console.WriteLine($"Type load error in assembly {args.Assembly.FullName}: {args.Exception.Message}");
};
```

Available error handling modes:
- `Silent` (default): Silently ignore errors and continue
- `RaiseEvent`: Raise events but don't throw exceptions
- `Throw`: Throw exceptions immediately

## Releases

Binary releases are automatically created for each new commit to the main branch.
These releases contain the compiled library and dependencies, packaged in a zip file.
You can find the latest release in the [Releases section](https://github.com/erinloy/CmdStarter/releases)
of the repository.

## Versioning

This project uses [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) to manage version numbers.
The version is automatically derived from git history and configured in the `version.json` file in the root of the repository.

Key benefits of this approach:
- Every build produces a unique, deterministic version number
- Version numbers are compatible with SemVer 2.0
- Version is visible in assemblies, NuGet packages, and GitHub releases
- Version is based on the git commit history and tags

For more information on how versioning works, see the [Nerdbank.GitVersioning documentation](https://github.com/dotnet/Nerdbank.GitVersioning/blob/master/doc/index.md).

## Participation | Submit issues, ideas

The project uses a free open source licence of Jira to manage its development.

https://cyberinternauts.atlassian.net/browse/CMD

If you want to participate, there are two options:
- Fork this repository and submit a pull request
- Ask to join the team

## License

MIT License. See [LICENSE.txt](https://github.com/CyberInternauts/CmdStarter/blob/master/LICENSE.txt)

## Collaborators

- [Jonathan Boivin](https://github.com/djon2003): Project leader, main developer, reviewer
- [Norbert Ormándi](https://github.com/DeszkaCodes): Developer, reviewer. A special thank to him to have believed in this project!

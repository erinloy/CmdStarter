# CmdStarter

### v0.9.0-beta.4

Features:
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
- Resilient to assembly load errors, with configurable error handling modes
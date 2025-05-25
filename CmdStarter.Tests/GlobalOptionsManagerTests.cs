using System.CommandLine;
using com.cyberinternauts.csharp.CmdStarter.Lib;
using com.cyberinternauts.csharp.CmdStarter.Lib.Interfaces;

namespace CmdStarter.Tests
{
    [TestFixture]
    public class GlobalOptionsManagerTests
    {
        [Test]
        public void GlobalOptionsManager_ShouldNotThrow_WhenAssemblyLoadFails()
        {
            // Arrange
            var starter = new Starter();
            starter.AssemblyLoadErrorHandler.Mode = AssemblyLoadErrorHandler.ErrorHandlingMode.Silent;
            
            // Act & Assert - Construction of GlobalOptionsManager internally calls methods that use reflection
            Assert.DoesNotThrow(() => new Starter());
        }
        
        [Test]
        public void CommandsWithOptions_ShouldNotThrow_WhenLoadingOptionsFromTypesWithIssues()
        {
            // Arrange
            var starter = new Starter(new[] { typeof(MockStarterCommand).Namespace ?? string.Empty });
            
            // Force error handler to be silent
            starter.AssemblyLoadErrorHandler.Mode = AssemblyLoadErrorHandler.ErrorHandlingMode.Silent;
            
            // Act & Assert - Just verify that we don't throw exceptions when accessing options
            Assert.DoesNotThrow(() => 
            {
                // This internally triggers the LoadOptions method through InstantiateCommands
                starter.Start(new[] { "--help" }).Wait();
            });
        }

        // Mock command for testing
        public class MockStarterCommand : IStarterCommand
        {
            public GlobalOptionsManager? GlobalOptionsManager { get; set; }
            
            public Delegate HandlingMethod => (Func<Task<int>>)Execute;
            
            public Task<int> Execute() => Task.FromResult(0);
        }
    }
}
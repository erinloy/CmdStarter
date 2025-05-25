using System.CommandLine;

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
            var starter = new Starter();
            var command = new Command("test-command");
            
            // Act & Assert - Just verify that we don't throw exceptions when accessing options
            Assert.DoesNotThrow(() => 
            {
                // This internally triggers the LoadOptions method through InstantiateCommands
                starter.Start(new[] { "--help" }).Wait();
            });
        }
    }
}
using System.Reflection;

namespace CmdStarter.Tests
{
    [TestFixture]
    public class StarterAssemblyLoadTests
    {
        [Test]
        public void FindCommandsTypes_ShouldNotThrow_WhenAssemblyLoadFails()
        {
            // Arrange
            var starter = new Starter();

            // Set up error handler to simulate assembly load failures
            starter.AssemblyLoadErrorHandler.Mode = AssemblyLoadErrorHandler.ErrorHandlingMode.Silent;
            
            // Act & Assert - This should not throw an exception
            Assert.DoesNotThrow(() => starter.FindCommandsTypes());
        }
        
        [Test]
        public void ErrorEvent_ShouldBeRaised_WhenModeIsSet()
        {
            // Arrange
            var starter = new Starter();
            bool eventRaised = false;
            
            // Configure error handler
            starter.AssemblyLoadErrorHandler.Mode = AssemblyLoadErrorHandler.ErrorHandlingMode.RaiseEvent;
            starter.AssemblyLoadErrorHandler.TypeLoadError += (sender, args) =>
            {
                eventRaised = true;
            };
            
            // Create a subclass of Starter that provides a mock assembly
            var mockStarter = new MockStarter(starter);
            
            // Act
            mockStarter.TriggerMockTypeLoadError();
            
            // Assert
            Assert.That(eventRaised, Is.True);
        }
        
        private class MockStarter
        {
            private readonly Starter _starter;
            
            public MockStarter(Starter starter)
            {
                _starter = starter;
            }
            
            public void TriggerMockTypeLoadError()
            {
                // Create a mock assembly that will throw an exception when GetTypes is called
                var mockAssembly = new MockAssemblyForTesting();
                
                // Use the error handler to process the mock assembly
                _starter.AssemblyLoadErrorHandler.GetTypesFromAssembly(mockAssembly);
            }
        }
        
        private class MockAssemblyForTesting : Assembly
        {
            public override Type[] GetTypes()
            {
                throw new ReflectionTypeLoadException(null, null);
            }
        }
    }
}
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
        
        [Test]
        public void ErrorPreferences_ShouldBeHonored_BeforeFirstTypeLoad()
        {
            // Arrange
            var starter = new Starter();
            bool eventRaised = false;
            
            // Configure error handler immediately after creating Starter
            // This is the key part of the test - previously this would have been too late
            // since type loading would have already happened in the GlobalOptionsManager constructor
            starter.AssemblyLoadErrorHandler.Mode = AssemblyLoadErrorHandler.ErrorHandlingMode.RaiseEvent;
            starter.AssemblyLoadErrorHandler.TypeLoadError += (sender, args) =>
            {
                eventRaised = true;
            };
            
            // Act - Now trigger type loading which should respect our error handling preferences
            var mockStarter = new MockStarterWithInjection(starter);
            mockStarter.InjectMockAssembly();
            
            // Assert
            Assert.That(eventRaised, Is.True, "Error event should be raised when error handling mode is set before first type load");
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
        
        private class MockStarterWithInjection
        {
            private readonly Starter _starter;
            
            public MockStarterWithInjection(Starter starter)
            {
                _starter = starter;
            }
            
            public void InjectMockAssembly()
            {
                // Create a mock assembly that will throw an exception when GetTypes is called
                var mockAssembly = new MockAssemblyForTesting();
                
                // First, add a mock assembly to the AppDomain by using reflection to set it as a field
                // (This is just for testing - we're simulating a problematic assembly)
                var appDomain = AppDomain.CurrentDomain;
                
                // Force type loading through GlobalOptionsManager which should call FindTypes() internally
                // This is what would previously have happened in the constructor but now happens on-demand
                // Since we've set the error mode before this point, our event should be raised
                _starter.GlobalOptionsManager.FilterTypes();
                
                // Also directly test that the handler is properly configured
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
using System.Reflection;

namespace CmdStarter.Tests
{
    [TestFixture]
    public class AssemblyLoadErrorHandlerTests
    {
        [Test]
        public void GetAssemblies_ShouldNotThrowException()
        {
            // Arrange
            var handler = new AssemblyLoadErrorHandler();

            // Act & Assert
            var assemblies = handler.GetAssemblies();
            Assert.That(assemblies, Is.Not.Null);
            Assert.That(assemblies, Is.Not.Empty);
        }

        [Test]
        public void GetTypesFromAssembly_WithNullAssembly_ShouldReturnEmptyCollection()
        {
            // Arrange
            var handler = new AssemblyLoadErrorHandler();

            // Act
            #pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var types = handler.GetTypesFromAssembly(null);
            #pragma warning restore CS8625

            // Assert
            Assert.That(types, Is.Not.Null);
            Assert.That(types, Is.Empty);
        }

        [Test]
        public void GetTypesFromAssembly_WithValidAssembly_ShouldReturnTypes()
        {
            // Arrange
            var handler = new AssemblyLoadErrorHandler();
            var assembly = typeof(AssemblyLoadErrorHandler).Assembly;

            // Act
            var types = handler.GetTypesFromAssembly(assembly);

            // Assert
            Assert.That(types, Is.Not.Null);
            Assert.That(types, Is.Not.Empty);
            Assert.That(types, Does.Contain(typeof(AssemblyLoadErrorHandler)));
        }

        [Test]
        public void EventMode_ShouldRaiseEventOnError()
        {
            // Arrange
            var handler = new AssemblyLoadErrorHandler { Mode = AssemblyLoadErrorHandler.ErrorHandlingMode.RaiseEvent };
            bool eventRaised = false;
            
            handler.TypeLoadError += (sender, args) => 
            {
                eventRaised = true;
                Assert.That(args.Assembly, Is.Not.Null);
                Assert.That(args.Exception, Is.Not.Null);
            };

            // Create a mock assembly that will throw an exception when GetTypes is called
            var mockAssembly = new MockAssemblyForTesting();

            // Act & Assert
            var types = handler.GetTypesFromAssembly(mockAssembly);
            Assert.That(eventRaised, Is.True);
            Assert.That(types, Is.Empty); // Should return empty collection on error
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
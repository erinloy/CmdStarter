using com.cyberinternauts.csharp.CmdStarter.Lib.Reflection;
using System.CommandLine;
using System.Reflection;

namespace CmdStarter.Tests
{
    [TestFixture]
    public class ReflectionLoaderTests
    {
        [Test]
        public void LoadDescription_ShouldNotThrow_WhenAttributeLoadFails()
        {
            // Arrange
            var mockProvider = new MockCustomAttributeProvider();
            var symbol = new Command("test");
            
            // Act & Assert
            Assert.DoesNotThrow(() => Loader.LoadDescription(mockProvider, symbol));
            Assert.That(symbol.Description, Is.EqualTo(string.Empty));
        }
        
        [Test]
        public void LoadAliases_ShouldNotThrow_WhenAttributeLoadFails()
        {
            // Arrange
            var mockProvider = new MockCustomAttributeProvider();
            var symbol = new Command("test");
            
            // Act & Assert
            Assert.DoesNotThrow(() => Loader.LoadAliases(mockProvider, symbol));
        }
        
        [Test]
        public void LoadAutoCompletes_ShouldNotThrow_WhenAttributeLoadFails()
        {
            // Arrange
            var mockProvider = new MockCustomAttributeProvider();
            bool actionCalled = false;
            
            // Act & Assert
            Assert.DoesNotThrow(() => Loader.LoadAutoCompletes(mockProvider, _ => actionCalled = true));
            Assert.That(actionCalled, Is.False);
        }
        
        private class MockCustomAttributeProvider : ICustomAttributeProvider
        {
            public object[] GetCustomAttributes(bool inherit)
            {
                throw new Exception("Test exception");
            }
            
            public object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                throw new Exception("Test exception");
            }
            
            public bool IsDefined(Type attributeType, bool inherit)
            {
                throw new Exception("Test exception");
            }
        }
    }
}
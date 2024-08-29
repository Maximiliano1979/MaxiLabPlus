using System;
using iLabPlus.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iLabPlus.Tests
{
    public class CloneAndModifyTests
    {

        // Clase para usar en la prueba
        public class TestClass
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Fact]
        public void CloneAndModify_ShouldCreateNewInstance()
        {
            // Arrange
            var original = new TestClass { Id = 1, Name = "Original" };

            // Act
            var clone = original.CloneAndModify(c => { });

            // Assert
            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
        }

        [Fact]
        public void CloneAndModify_ShouldCopyAllProperties()
        {
            // Arrange
            var original = new TestClass { Id = 1, Name = "Original" };

            // Act
            var clone = original.CloneAndModify(c => { });

            // Assert
            Assert.Equal(original.Id, clone.Id);
            Assert.Equal(original.Name, clone.Name);
        }

        [Fact]
        public void CloneAndModify_ShouldModifyClone()
        {
            // Arrange
            var original = new TestClass { Id = 1, Name = "Original" };

            // Act
            var clone = original.CloneAndModify(c => c.Name = "Modified");

            // Assert
            Assert.Equal("Modified", clone.Name);
            Assert.NotEqual(original.Name, clone.Name);
            Assert.Equal(original.Id, clone.Id);
        }

    }
}

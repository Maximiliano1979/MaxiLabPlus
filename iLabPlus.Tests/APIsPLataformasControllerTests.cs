using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using iLabPlus.Controllers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Helpers;
using iLabPlus.Tests.TestHelpers;
using System.Collections.Generic;
using iLabPlus.Models.Clases;

namespace iLabPlus.Tests
{
    public class APIsPlataformasControllerTests
    {
        [Fact]
        public void Index_ReturnsViewWithCorrectData()
        {
            // Arrange
            var mockContext = MockDbContextFactory.CreateMockContext();
            var mockHttpContextAccessor = MockHttpContextAccessorFactory.CreateMockHttpContextAccessor("TestEmpresa", "TestUser", "ADMIN");
            var mockConfiguration = new Mock<IConfiguration>();

            var expectedMenuAccesos = new List<MenuUser> { new MenuUser("TestMenu", "TestMenu", new List<MenuUserAccesos>(), "fa-test") };

            var mockFunctionsBBDD = new Mock<FunctionsBBDD>(mockContext, mockHttpContextAccessor.Object);
            mockFunctionsBBDD.Setup(f => f.GetMenuAccesos()).Returns(expectedMenuAccesos);
            mockFunctionsBBDD.Setup(f => f.GetClaims()).Returns(new GrupoClaims
            {
                SessionEmpresa = "TestEmpresa",
                SessionUsuario = "TestUser",
                SessionUsuarioTipo = "ADMIN"
            });
            mockFunctionsBBDD.Setup(f => f.GetColumnsLayout(It.IsAny<string>())).Returns(new GrupoColumnsLayout());

            var controller = new APIsPlataformasController(mockContext, mockFunctionsBBDD.Object, mockConfiguration.Object);

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("APIsPlataformas", viewResult.ViewName);

            Assert.NotNull(viewResult.ViewData["MenuUserList"]);
            var menuUserList = Assert.IsType<List<MenuUser>>(viewResult.ViewData["MenuUserList"]);
            Assert.Equal(expectedMenuAccesos, menuUserList);
        }
    }
}
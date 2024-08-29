using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace iLabPlus.Tests.TestHelpers
{
    public static class MockHttpContextAccessorFactory
    {
        public static Mock<IHttpContextAccessor> CreateMockHttpContextAccessor(string empresa, string usuario, string usuarioTipo)
        {
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var claims = new List<Claim>
        {
            new Claim("Empresa", empresa),
            new Claim("Usuario", usuario),
            new Claim("UsuarioTipo", usuarioTipo)
        };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = claimsPrincipal };
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(httpContext);
            return mockHttpContextAccessor;
        }
    }
}

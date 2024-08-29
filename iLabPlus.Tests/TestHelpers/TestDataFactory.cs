using iLabPlus.Models.BDiLabPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iLabPlus.Tests.TestHelpers
{
    public static class TestDataFactory
    {
        public static List<Articulos> CreateTestArticulos()
        {
            return new List<Articulos>
        {
            new Articulos { Guid = Guid.NewGuid(), Articulo = "ART001", Activo = true, Empresa = "TestEmpresa" },
            new Articulos { Guid = Guid.NewGuid(), Articulo = "ART002", Activo = false, Empresa = "TestEmpresa" }
        };
        }

        public static List<Usuarios> CreateTestUsuarios()
        {
            return new List<Usuarios>
        {
            new Usuarios { Guid = Guid.NewGuid(), UsuarioNombre = "Usuario 1", Empresa = "TestEmpresa" },
            new Usuarios { Guid = Guid.NewGuid(), UsuarioNombre = "Usuario 2", Empresa = "TestEmpresa" }
        };
        }

        public static List<Calendario> CreateTestCalendario()
        {
            return new List<Calendario>
        {
            new Calendario { Guid = Guid.NewGuid(), Titulo = "Evento 1", Fecha = DateTime.Now, Empresa = "TestEmpresa" },
            new Calendario { Guid = Guid.NewGuid(), Titulo = "Evento 2", Fecha = DateTime.Now.AddDays(1), Empresa = "TestEmpresa" }
        };
        }

        public static List<Doctores> CreateTestDoctores()
        {
            return new List<Doctores>
        {
            new Doctores { Guid = Guid.NewGuid(), Doctor = "DOC001", Nombre = "Doctor 1", Activo = true, Empresa = "TestEmpresa" },
            new Doctores { Guid = Guid.NewGuid(), Doctor = "DOC002", Nombre = "Doctor 2", Activo = false, Empresa = "TestEmpresa" },
            new Doctores { Guid = Guid.NewGuid(), Doctor = "DOC003", Nombre = "Doctor 3", Activo = true, Empresa = "TestEmpresa" }
        };
        }
    }
}

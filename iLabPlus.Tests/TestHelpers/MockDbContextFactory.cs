using iLabPlus.Models.BDiLabPlus;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iLabPlus.Tests.TestHelpers
{
    public static class MockDbContextFactory
    {
        public static DbContextiLabPlus CreateMockContext(
                  List<Articulos> articulos = null,
                  List<Usuarios> usuarios = null,
                  List<Calendario> calendarios = null,
                  List<Clientes> clientes = null,
                  List<Doctores> doctores = null)
        {
            var options = new DbContextOptionsBuilder<DbContextiLabPlus>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new DbContextiLabPlus(options);

            if (articulos != null) context.Articulos.AddRange(articulos);
            if (usuarios != null) context.Usuarios.AddRange(usuarios);
            if (calendarios != null) context.Calendario.AddRange(calendarios);
            if (clientes != null) context.Clientes.AddRange(clientes);
            if (doctores != null) context.Doctores.AddRange(doctores);

            context.SaveChanges();

            return context;
        }
    }
}

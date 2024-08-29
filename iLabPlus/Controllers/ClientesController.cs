using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Nemesis365.Controllers;
using Xunit.Abstractions;
using Newtonsoft.Json;


namespace iLabPlus.Controllers
{
    [Authorize]
    public class ClientesController : Controller
    {

        private readonly DbContextiLabPlus              ctxDB;
        private readonly ILogger<ClientesController>    _logger;
        private readonly FunctionsBBDD                  FunctionsBBDD;
        private readonly GrupoClaims                    GrupoClaims;
        private readonly GrupoColumnsLayout             GrupoColumnsLayout;
        private readonly ITestOutputHelper _testOutputHelper;


        public ClientesController(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD, ILogger<ClientesController> logger, ITestOutputHelper testOutputHelper = null)
        {
            ctxDB               = Context;
            _logger             = logger;
            FunctionsBBDD       = _FunctionsBBDD;
            GrupoClaims         = FunctionsBBDD.GetClaims();
            GrupoColumnsLayout  = FunctionsBBDD.GetColumnsLayout("gridClientes");
            _testOutputHelper = testOutputHelper;

        }

        private void Log(string message)
        {
            _logger.LogInformation(message);
            _testOutputHelper?.WriteLine(message);
        }


        public IActionResult Index()
        {

            try
            {
                ViewBag.MenuUserList = FunctionsBBDD.GetMenuAccesos();
                ViewBag.ColumnsLayoutUser = GrupoColumnsLayout.ColumnsLayoutUser;
                ViewBag.ColumnsPinnedUser = GrupoColumnsLayout.ColumnsPinnedUser;


                var Clientes = ctxDB.Clientes
                    .Where(x => x.Empresa == GrupoClaims.SessionEmpresa)
                    .OrderBy(x => x.Cliente)
                    .ToList();


                return View("Clientes", Clientes);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IActionResult DialogCliente(Guid Guid)
        {

            var Cliente = ctxDB.Clientes.Where(x => x.Guid == Guid).FirstOrDefault();
            if (Cliente == null)
            {
                Cliente = new Clientes();
            }
            else
            {
                Log($"Cliente encontrado: {Cliente.CliNombre}");
            }

            var FacturaTipos = new List<string> { "", "Etiqueta", "Hechura", "Peso", "PesoHechura" };
            ViewBag.ListFacturaTipos = FacturaTipos;

            var TarifaVacia = new TarifasVenta { Tarifa = "", TarDescripcion = "" };
            var Tarifas = ctxDB.TarifasVenta.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).OrderBy(x => x.Tarifa).ToList();
            Tarifas.Insert(0, TarifaVacia);
            ViewBag.ListTarifas = Tarifas;

            var VendedorVacio = new Vendedores { Vendedor = "", VenNombre = "" };
            var Vendedores = ctxDB.Vendedores.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).OrderBy(x => x.Vendedor).ToList();
            Vendedores.Insert(0, VendedorVacio);
            ViewBag.ListVendedores = Vendedores;

            var ExpNac = new List<string> { "", "Nac", "Nac-Exp" };
            ViewBag.ListExpNac = ExpNac;

            var Idiomas = new List<string> { "Español", "Ingles" };
            ViewBag.ListIdiomas = Idiomas;

            var Divisas = ctxDB.Divisas.Where(x => x.Empresa == GrupoClaims.SessionEmpresa).OrderBy(x => x.Divisa).ToList();
            ViewBag.ListDivisas = Divisas;

            var ValsysVacio = new ValSys();
            var ValsysEmpresas = ctxDB.ValSys.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Indice == "Empresas").OrderBy(x => x.Indice).ThenBy(x => x.Clave).ToList();
            ValsysEmpresas.Insert(0, ValsysVacio);
            ViewBag.ValsysEmpresas = ValsysEmpresas;

            return PartialView("_DialogCliente", Cliente);
        }

        /********************************************************************************************************************************/
        /*                                                  FUNCIONES CREATE, EDIT, DELETE...                                           */
        /********************************************************************************************************************************/


        [HttpPost]
        public async Task<IActionResult> CreateEdit(Clientes Customer)
        {
            bool resultProcess = false;
            Log($"Iniciando CreateEdit para Cliente: {Customer.Cliente}, Guid: {Customer.Guid}");

            try
            {
                if (Customer.Guid == Guid.Empty)
                {
                    Log("Creando nuevo cliente");
                    var maxCliente = ctxDB.Clientes
                       .Where(x => x.Empresa == GrupoClaims.SessionEmpresa)
                       .ToList()
                       .Where(x => IsNumeric(x.Cliente))
                       .OrderByDescending(x => int.Parse(x.Cliente))
                       .Select(x => x.Cliente)
                       .FirstOrDefault();

                    Log($"Máximo cliente encontrado: {maxCliente}");

                    int nextNumber = 1;
                    if (!string.IsNullOrEmpty(maxCliente))
                    {
                        nextNumber = int.Parse(maxCliente) + 1;
                    }

                    Customer.Cliente = nextNumber.ToString("D6");
                    Log($"Nuevo número de cliente generado: {Customer.Cliente}");

                    var FindClient = ctxDB.Clientes
                        .Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.Cliente == Customer.Cliente)
                        .FirstOrDefault();

                    if (FindClient == null)
                    {
                        Log("Cliente no existe, procediendo a crear");
                        Customer.Guid = Guid.NewGuid();
                        Log($"Nuevo GUID generado: {Customer.Guid}");
                        Customer.Empresa = GrupoClaims.SessionEmpresa;
                        Customer.IsoUser = GrupoClaims.SessionUsuarioNombre;
                        Customer.IsoFecAlt = DateTime.Now;
                        Customer.IsoFecMod = DateTime.Now;

                        ctxDB.Clientes.Add(Customer);
                        await ctxDB.SaveChangesAsync();
                        resultProcess = true;
                        Log($"Cliente creado con éxito. GUID: {Customer.Guid}, Número: {Customer.Cliente}");
                    }
                    else
                    {
                        Log("Cliente ya existe, retornando EXIST");
                        return StatusCode(200, "EXIST");
                    }
                }
                else
                {
                    Log($"Actualizando cliente existente. GUID: {Customer.Guid}");
                    var existingCustomer = await ctxDB.Clientes.FirstOrDefaultAsync(c => c.Guid == Customer.Guid);
                    Log($"Resultado de búsqueda: {(existingCustomer != null ? "Cliente encontrado" : "Cliente no encontrado")}");

                    if (existingCustomer != null)
                    {
                        Log($"Actualizando campos del cliente. Nombre anterior: {existingCustomer.CliNombre}, Nuevo nombre: {Customer.CliNombre}");
                        existingCustomer.CliNombre = Customer.CliNombre;
                        existingCustomer.IsoUser = GrupoClaims.SessionUsuarioNombre;
                        existingCustomer.IsoFecMod = DateTime.Now;

                        ctxDB.Clientes.Update(existingCustomer);
                        await ctxDB.SaveChangesAsync();
                        Log($"Cliente actualizado con éxito. GUID: {existingCustomer.Guid}");
                        Customer = existingCustomer; // Asegurarse de que Customer tenga todos los campos actualizados
                        resultProcess = true;
                    }
                    else
                    {
                        Log($"No se encontró el cliente con GUID: {Customer.Guid}");
                        return StatusCode(400, $"No se encontró el cliente con GUID: {Customer.Guid}");

                    }
                }
            }
            catch (Exception e)
            {
                Log($"Error en CreateEdit: {e.Message}");
                Log($"StackTrace: {e.StackTrace}");
                return StatusCode(400, e.Message);
            }

            //var Result = ctxDB.Clientes.Where(x => x.Guid == Customer.Guid).FirstOrDefault();
            //Log($"Resultado final: {(Result != null ? $"Cliente encontrado. GUID: {Result.Guid}, Número: {Result.Cliente}" : "Cliente no encontrado")}");

            if (resultProcess)
            {
                return StatusCode(200, Customer);
            }
            else
            {
                return StatusCode(400, null);
            }
        }


        internal bool IsNumeric(string str)
        {
            return !string.IsNullOrWhiteSpace(str) && str.All(char.IsDigit);
        }

        public string GenerateNextClientNumber()
        {
            var maxCliente = ctxDB.Clientes
                .Where(x => x.Empresa == GrupoClaims.SessionEmpresa)
                .ToList()
                .Where(x => IsNumeric(x.Cliente))
                .OrderByDescending(x => int.Parse(x.Cliente))
                .Select(x => x.Cliente)
                .FirstOrDefault();

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(maxCliente))
            {
                nextNumber = int.Parse(maxCliente) + 1;
            }

            return nextNumber.ToString("D6");
        }



        [HttpPost]
        public async Task<IActionResult> Delete_CLIENTE(Guid Guid)
        {
            var ProFind = ctxDB.Clientes.Where(x => x.Guid == Guid).FirstOrDefault();
            if (ProFind != null)
            {
                try
                {
                    ctxDB.Clientes.Remove(ProFind);
                    await ctxDB.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    return StatusCode(400, e.Message);
                }

                return StatusCode(200, "OK");
            }

            return StatusCode(200, null);
        }


        /********************************************************************************************************************************/
        /*                                                  FUNCIONES GENERAL CLIENTES                                                  */
        /********************************************************************************************************************************/
        [HttpPost]
        public ActionResult GetClientesLazing(string query, int max)
        {
            query = query ?? string.Empty;
            var FindQueryCli = ctxDB.Clientes
                .Where(x => x.Empresa == GrupoClaims.SessionEmpresa &&
                            (x.CliNombre.StartsWith(query) || x.Cliente.StartsWith(query)))
                .Select(c => new Clientes
                {
                    Guid = c.Guid,
                    Cliente = c.Cliente,
                    CliNombre = c.CliNombre,
                    Empresa = c.Empresa,
                    CalcClienteNombre = c.Cliente + "  :  " + c.CliNombre
                })
                .Take(max)
                .ToList();
            return Json(FindQueryCli);
        }


        [HttpGet]
        public async Task<IActionResult> GetDoctoresPorClinica(string nombreClinica)
        {
            Log("Iniciando GetDoctoresPorClinica()");
            try
            {
                var doctores = await ctxDB.DoctoresClinicas
                    .Where(dc => dc.Clinica == nombreClinica)
                    .Join(ctxDB.Doctores,  // Une con la tabla Doctores
                        dc => dc.Doctor,   // Clave en DoctoresClinicas que representa el doctor
                        d => d.Doctor,     // Clave en Doctores que es la referencia del doctor
                        (dc, d) => new { DoctoresClinicas = dc, Doctor = d }) // Resultado de la unión
                    .Where(joined => joined.Doctor.Activo == true) // Filtrar por doctores activos
                    .Select(joined => new {
                        Guid = joined.DoctoresClinicas.Guid,
                        Nombre = joined.Doctor.Nombre,
                        NumColegiado = joined.Doctor.NumColegiado,
                        Mail = joined.Doctor.Mail,
                        Telefono1 = joined.Doctor.Telefono1
                    })
                    .ToListAsync();

                Log($"GetDoctoresPorCloinica(): Se encontraron {doctores.Count} doctores por Clinica");


                return Json(new { success = true, doctores = doctores });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Error al obtener los datos de los doctores." });
            }
        }

        public IActionResult GetDoctores()
        {
            Log("Iniciando GetDoctores()");
            try
            {
                var doctores = ctxDB.Doctores
                    .Where(d => d.Activo == true)
                    .Select(d => new
                    {
                        d.Guid,
                        d.Nombre,
                        d.Doctor,
                        d.NumColegiado,
                        d.Mail,
                        d.Telefono1
                    })
                    .ToList();

                Log($"GetDoctores(): Se encontraron {doctores.Count} doctores activos");
                foreach (var doctor in doctores)
                {
                    Log($"Doctor: {doctor.Nombre}, ID: {doctor.Doctor}");
                }

                return Json(new { success = true, doctores = doctores });
            }
            catch (Exception e)
            {
                Log($"Error en GetDoctores(): {e.Message}");
                return Json(new { success = false, message = "Error al obtener los datos de los doctores." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Cliente_Copiar(string ClienteOld, string ClienteNew)
        {
           
            // Verificar si el nuevo cliente ya existe
            var findClienteNew = ctxDB.Clientes.FirstOrDefault(c => c.Empresa == GrupoClaims.SessionEmpresa && c.Cliente == ClienteNew);
            if (findClienteNew != null)
            {
                return Json(new { success = false, message = "El cliente nuevo ya existe." });
            }

            // Buscar el cliente original
            var clienteOriginal = ctxDB.Clientes.FirstOrDefault(c => c.Empresa == GrupoClaims.SessionEmpresa && c.Cliente == ClienteOld);
            if (clienteOriginal == null)
            {
                return Json(new { success = false, message = "Cliente original no encontrado." });
            }

            try
            {
                // Crear una copia del cliente original
                var nuevoCliente = clienteOriginal.CloneAndModify(v =>
                {
                 // Asumiendo que tienes un método Clone en tu modelo Clientes
                    v.Guid = Guid.NewGuid();
                    v.Cliente = ClienteNew;
                    v.IsoUser = GrupoClaims.SessionUsuarioNombre;
                    v.IsoFecAlt = DateTime.Now;
                    v.IsoFecMod = DateTime.Now;
                });

                ctxDB.Clientes.Add(nuevoCliente);
                await ctxDB.SaveChangesAsync();

                return Json(new { success = true, data = nuevoCliente }); // Devolver el nuevo cliente creado
            }
            catch (Exception e)
            {
                // Manejo de errores (puedes personalizar esto)
                return Json(new { success = false, message = "Ocurrió un error durante la copia del cliente." });
            }
        }

        public IActionResult _DialogClienteCopiar(string Cliente)
        {
            ViewBag.ClienteOld = Cliente;

            return PartialView("_DialogClienteCopiar");
        }



        [HttpPost]
        public async Task<IActionResult> Cliente_Renombrar(string ClienteOld, string ClienteNew)
        {
            _logger.LogInformation($"Iniciando Cliente_Renombrar. ClienteOld: {ClienteOld}, ClienteNew: {ClienteNew}");

            // Verificar si el nuevo cliente ya existe
            var findClienteNew = ctxDB.Clientes.FirstOrDefault(c => c.Empresa == GrupoClaims.SessionEmpresa && c.Cliente == ClienteNew);
            if (findClienteNew != null)
            {
                _logger.LogWarning($"El cliente nuevo ya existe: {ClienteNew}");
                return Json(new { success = false, message = "El cliente nuevo ya existe." });
            }

            // Buscar el cliente original
            var clienteOriginal = ctxDB.Clientes.FirstOrDefault(c => c.Empresa == GrupoClaims.SessionEmpresa && c.Cliente == ClienteOld);
            if (clienteOriginal == null)
            {
                _logger.LogWarning($"Cliente original no encontrado: {ClienteOld}");
                return Json(new { success = false, message = "Cliente original no encontrado." });
            }

            _logger.LogInformation($"Cliente original encontrado: {JsonConvert.SerializeObject(clienteOriginal)}");

            try
            {
                // Actualizar el nombre del cliente
                clienteOriginal.Cliente = ClienteNew;
                clienteOriginal.IsoUser = GrupoClaims.SessionUsuarioNombre;
                clienteOriginal.IsoFecMod = DateTime.Now;

                _logger.LogInformation($"Actualizando cliente: {JsonConvert.SerializeObject(clienteOriginal)}");

                ctxDB.Clientes.Update(clienteOriginal);
                await ctxDB.SaveChangesAsync();

                _logger.LogInformation($"Cliente actualizado con éxito: {JsonConvert.SerializeObject(clienteOriginal)}");

                return Json(new { success = true, data = clienteOriginal }); // Devolver el cliente actualizado
            }
            catch (Exception e)
            {
                _logger.LogError($"Error al renombrar cliente: {e.Message}");
                return Json(new { success = false, message = "Ocurrió un error durante el cambio de nombre del cliente." });
            }
        }


        public IActionResult _DialogClienteRenombrar(string Cliente)
        {
            ViewBag.ClienteOld = Cliente;

            return PartialView("_DialogClienteRenombrar");
        }







    }

}
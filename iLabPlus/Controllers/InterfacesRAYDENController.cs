using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace iLabPlusV.Controllers
{
    [Authorize]
    public class InterfacesRAYDENController : Controller
    {
        private readonly DbContextiLabPlus  ctxDB;
        private readonly FunctionsBBDD      FunctionsBBDD;
        private readonly GrupoClaims        GrupoClaims;
        private readonly GrupoColumnsLayout GrupoColumnsLayout;

        private int DiasRestaToday;

        public InterfacesRAYDENController( DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD)
        {
            ctxDB   = Context;

            FunctionsBBDD       = _FunctionsBBDD;
            GrupoClaims         = FunctionsBBDD.GetClaims();
            GrupoColumnsLayout  = FunctionsBBDD.GetColumnsLayout("gridInterface");
        }


        public IActionResult Index()
        {
            ViewBag.MenuUserList        = FunctionsBBDD.GetMenuAccesos();

            var Empresas            = ctxDB.Empresas.ToList();
            ViewBag.Empresas        = Empresas;

            var ultimaEmpresa       = ctxDB.Empresas.OrderBy(x => x.Empresa).LastOrDefault();
            if (ultimaEmpresa != null)
            {
                ViewBag.UltimaEmpresaId = ultimaEmpresa.Empresa;
            }


            return View();
        }

        public async Task<ActionResult> ProcesareExcelRAYDEN(string KEYEMP, IFormFile FicExcel)
        {
            DiasRestaToday = -4;

            if (FicExcel != null && FicExcel.Length > 0)
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var stream = FicExcel.OpenReadStream())
                {
                    using (var package = new ExcelPackage(stream))
                    {
                        // Accede a la hoja de trabajo (worksheet) por índice o nombre
                        //var worksheet = package.Workbook.Worksheets[0]; 
                        var ws_users        = package.Workbook.Worksheets["USUARIOS"];
                        var ws_articulos    = package.Workbook.Worksheets["ARTICULOS"];
                        var ws_clientes     = package.Workbook.Worksheets["CLIENTES"];
                        var ws_proveedores  = package.Workbook.Worksheets["PROVEEDORES"];
                        var ws_pedidosCAB   = package.Workbook.Worksheets["PEDIDOS"];
                        //var ws_pedidosLIN   = package.Workbook.Worksheets["PEDIDOSLIN"];
                        var ws_AlbaranesCAB = package.Workbook.Worksheets["ALBARANES"];
                        var ws_FacturasCAB  = package.Workbook.Worksheets["FACTURAS"];

                        DateTime FechaImportacion   = new DateTime(2024, 4, 4);
                        DateTime FechaRecogidaDatos = new DateTime(2024, 4, 8);

                        //*************************************************************************
                        // Valors fijos
                        //*************************************************************************
                        await Generar_Tarifas           (KEYEMP, FechaImportacion); 
                        await Generar_Divisas           (KEYEMP, FechaImportacion); 
                        await Generar_LogsVersiones     (KEYEMP, FechaImportacion); 
                        //*************************************************************************

                        await Generar_Usuarios      (KEYEMP, ws_users,          FechaImportacion);
                        await Generar_Articulos     (KEYEMP, ws_articulos,      FechaImportacion);
                        await Generar_Clientes      (KEYEMP, ws_clientes,       FechaImportacion);
                        await Generar_Proveedores   (KEYEMP, ws_proveedores,    FechaImportacion);

                        await Generar_PedidosCAB    (KEYEMP, ws_pedidosCAB,     FechaImportacion);
                        //await Generar_AlbaranesCAB  (KEYEMP, ws_AlbaranesCAB,   FechaImportacion);
                        await Generar_FacturasCAB   (KEYEMP, ws_FacturasCAB,    FechaImportacion);

                        await Generar_LogsAccesos   (KEYEMP, FechaRecogidaDatos);
                    }
                }

            }

            return StatusCode(200, "Ok");
        }

        private async Task Generar_FacturasCAB(string KEYEMP, ExcelWorksheet WS, DateTime FechaImportacion)
        {
            // SE ELIMINAN PREVIAMENTE LOS Proveedores EXISTENTES DE LA EMPRESA 
            var ListFacturasCAB = ctxDB.FacturasCAB.Where(x => x.Empresa == KEYEMP).ToList();
            ctxDB.RemoveRange(ListFacturasCAB);
            await ctxDB.SaveChangesAsync();

            var ListFacturasLIN = ctxDB.FacturasLIN.Where(x => x.Empresa == KEYEMP).ToList();
            ctxDB.RemoveRange(ListFacturasLIN);
            await ctxDB.SaveChangesAsync();

            try
            {

                for (int row = 3; row <= WS.Dimension.Rows + 1; row++)
                {
                    var Cliente = WS.Cells[row, 4].Text;
                    if (Cliente != null && Cliente != "")
                    {
                        var Factura = WS.Cells[row, 2].Text;
                        var Fecha = DateTime.ParseExact(WS.Cells[row, 3].Text, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                        var FactBruto   = decimal.Parse(WS.Cells[row, 8].Text);
                        var FactDTO     = decimal.Parse(WS.Cells[row, 9].Text);
                        var FactBI      = decimal.Parse(WS.Cells[row, 10].Text);
                        var FactIva     = decimal.Parse(WS.Cells[row, 11].Text);
                        var FactTotal   = decimal.Parse(WS.Cells[row, 12].Text);

                        var NewRow = new FacturasCAB();
                        NewRow.Empresa = KEYEMP;
                        NewRow.Cliente = Cliente;
                        NewRow.Factura = Factura;
                        NewRow.FacTarifaVenta = "T1";

                        NewRow.FacFecha = Fecha;

                        NewRow.FacDTOCial = 0;
                        NewRow.FacDTOPpago = 0;
                        NewRow.FacDTORappel = 0;
                        NewRow.FacIVA = FactIva;


                        var FindCli = ctxDB.Clientes.Where(x => x.Empresa == KEYEMP && x.Cliente == Cliente).FirstOrDefault();
                        if (FindCli != null)
                        {
                            NewRow.FacDirFacDireccion = FindCli.CliDirFacDireccion;
                            NewRow.FacDirFacDP = FindCli.CliDirFacDP;
                            NewRow.FacDirFacPoblacion = FindCli.CliDirFacPoblacion;
                            NewRow.FacDirFacPais = FindCli.CliDirFacPais;

                            NewRow.FacDirMerDireccion = FindCli.CliDirMerDireccion;
                            NewRow.FacDirMerDP = FindCli.CliDirMerDP;
                            NewRow.FacDirMerPoblacion = FindCli.CliDirMerPoblacion;
                            NewRow.FacDirMerPais = FindCli.CliDirMerPais;
                        }

                        NewRow.FacEstado = "";
                        NewRow.FacTipo = "NAC";

                        NewRow.FacDivisa = "Euro";
                        NewRow.FacIdioma = "Español";

                        NewRow.TotalFac     = FactTotal;
                        NewRow.TotalFacBI   = FactBI;
                        NewRow.TotalDtoCial = FactDTO;


                        NewRow.IsoUser = "Interface";
                        NewRow.IsoFecAlt = FechaImportacion;
                        NewRow.IsoFecMod = FechaImportacion;

                        ctxDB.FacturasCAB.Add(NewRow);
                        await ctxDB.SaveChangesAsync();

                        // SE CREA LA LINEA
                        var NewLin = new FacturasLIN();
                        NewLin.Empresa = KEYEMP;
                        NewLin.Cliente = Cliente;
                        NewLin.Factura = Factura;
                        NewLin.Pedido = "";
                        NewLin.Albaran = "";
                        NewLin.FacLinea = 1;
                        NewLin.FacArt = "*";
                        NewLin.FacDesc = "*";

                        NewLin.FacQty = 1;
                        NewLin.FacPrecio = FactBI;
                        NewLin.FacPrecioTotal = FactBI;

                        ctxDB.FacturasLIN.Add(NewLin);
                        await ctxDB.SaveChangesAsync();
                    }
                    

                }

            }
            catch (Exception e)
            {
                var errorV = e.Message;
            }

        }

        //private async Task Generar_AlbaranesCAB(string KEYEMP, ExcelWorksheet WS, DateTime FechaImportacion)
        //{
        //    // SE ELIMINAN PREVIAMENTE LOS Proveedores EXISTENTES DE LA EMPRESA 
        //    var ListAlbaranesCAB = ctxDB.AlbaranesCAB.Where(x => x.Empresa == KEYEMP).ToList();
        //    ctxDB.RemoveRange(ListAlbaranesCAB);
        //    await ctxDB.SaveChangesAsync();

        //    var ListAlbaranesLIN = ctxDB.AlbaranesLIN.Where(x => x.Empresa == KEYEMP).ToList();
        //    ctxDB.RemoveRange(ListAlbaranesLIN);
        //    await ctxDB.SaveChangesAsync();

        //    try
        //    {

        //        for (int row = 3; row <= WS.Dimension.Rows + 1; row++)
        //        {
        //            var Cliente = WS.Cells[row, 2].Text;
        //            if (Cliente != null && Cliente != "")
        //            {
        //                var Albaran = WS.Cells[row, 4].Text;
        //                var Fecha = DateTime.ParseExact(WS.Cells[row, 5].Text, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        //                var Pedido = WS.Cells[row, 6].Text;
        //                var Factura = WS.Cells[row, 7].Text;

        //                var AlbValor = decimal.Parse(WS.Cells[row, 8].Text);
        //                var AlbTotal = decimal.Parse(WS.Cells[row, 14].Text);
        //                var AlbIva = decimal.Parse(WS.Cells[row, 13].Text);

        //                var NewRow = new AlbaranesCAB();
        //                NewRow.Empresa = KEYEMP;
        //                NewRow.Cliente = Cliente;
        //                NewRow.Albaran = Albaran;
        //                NewRow.AlbTarifaVenta = "T1";

        //                NewRow.AlbFecha = Fecha;

        //                NewRow.AlbDTOCial = 0;
        //                NewRow.AlbDTOPpago = 0;
        //                NewRow.AlbDTORappel = 0;
        //                NewRow.AlbIVA = AlbIva;


        //                var FindCli = ctxDB.Clientes.Where(x => x.Empresa == KEYEMP && x.Cliente == Cliente).FirstOrDefault();
        //                if (FindCli != null)
        //                {
        //                    NewRow.AlbDirFacDireccion = FindCli.CliDirFacDireccion;
        //                    NewRow.AlbDirFacDP = FindCli.CliDirFacDP;
        //                    NewRow.AlbDirFacPoblacion = FindCli.CliDirFacPoblacion;
        //                    NewRow.AlbDirFacPais = FindCli.CliDirFacPais;

        //                    NewRow.AlbDirMerDireccion = FindCli.CliDirMerDireccion;
        //                    NewRow.AlbDirMerDP = FindCli.CliDirMerDP;
        //                    NewRow.AlbDirMerPoblacion = FindCli.CliDirMerPoblacion;
        //                    NewRow.AlbDirMerPais = FindCli.CliDirMerPais;
        //                }

        //                NewRow.AlbEstado = "";
        //                NewRow.AlbTipo = "NAC";

        //                NewRow.AlbDivisa = "Euro";
        //                NewRow.AlbIdioma = "Español";

        //                NewRow.IsoUser = "Interface";
        //                NewRow.IsoFecAlt = FechaImportacion;
        //                NewRow.IsoFecMod = FechaImportacion;

        //                ctxDB.AlbaranesCAB.Add(NewRow);
        //                await ctxDB.SaveChangesAsync();

        //                // SE CREA LA LINEA
        //                var NewLin = new AlbaranesLIN();
        //                NewLin.Empresa = KEYEMP;
        //                NewLin.Cliente = Cliente;
        //                NewLin.Albaran = Albaran;
        //                NewLin.Pedido = Pedido;
        //                NewLin.PedLinea = 1;
        //                NewLin.AlbArt = "*";
        //                NewLin.AlbDesc = "*";

        //                NewLin.AlbQty = 1;
        //                NewLin.AlbPrecio = AlbValor;
        //                NewLin.AlbPrecioTotal = AlbValor;

        //                ctxDB.AlbaranesLIN.Add(NewLin);
        //                await ctxDB.SaveChangesAsync();
        //            }
                        

        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        var errorV = e.Message;
        //    }

        //}


        private async Task Generar_LogsAccesos(string KEYEMP, DateTime FechaRecogidaDatos)
        {
            // SE ELIMINAN PREVIAMENTE LOS Logs_Versiones EXISTENTES DE LA EMPRESA 
            var ListLogs_Accesos = ctxDB.Logs_Accesos.Where(x => x.Empresa == KEYEMP).ToList();
            ctxDB.RemoveRange(ListLogs_Accesos);
            await ctxDB.SaveChangesAsync();


            try
            {
                var fechaActual = FechaRecogidaDatos.AddDays(+1);
                var fechaInicio = fechaActual.AddDays(-5); // Retroceder 1 semanas

                Random random = new Random();

                string ip = await GetPublicIpAddress();

                var usuarios = ctxDB.Usuarios.Where(u => u.Empresa == KEYEMP).ToList();

                // Definir la cadena de tipos con pesos proporcionales
                //string[] TipoRecurso = { "Pedidos", "Articulos", "Albaranes", "Facturas", "LogsAcceso" };
                string[] TipoRecurso = { "Pedidos", "Pedidos", "Pedidos", "Pedidos", "Pedidos", "Articulos", "Articulos", "Albaranes", "Facturas", "LogsAcceso" };

                // Calcular la suma de los pesos
                int sumaPesos = TipoRecurso.Length;

                string[] TipoSistema = { "Windows", "Windows", "Windows", "Windows", "Windows", "Windows", "Windows", "Windows", "Android", "Apple", };
                int sumaPesosSistema = TipoSistema.Length;

                for (int i = 0; i < 40; i++)
                {
                    DateTime fechaGenerada;
                    do
                    {
                        fechaGenerada = fechaInicio.AddSeconds(random.NextDouble() * (fechaActual - fechaInicio).TotalSeconds);
                    }
                    while (fechaGenerada.DayOfWeek == DayOfWeek.Saturday || fechaGenerada.DayOfWeek == DayOfWeek.Sunday);

                    // Generar horas, minutos y segundos aleatorios dentro del rango
                    TimeSpan horasAleatorias = new TimeSpan(0, random.Next(8, 15), random.Next(0, 60), random.Next(0, 60));

                    fechaGenerada = fechaGenerada.Date.Add(horasAleatorias);

                    var usuarioAleatorio = usuarios[random.Next(usuarios.Count)];

                    // Calcular un índice ponderado aleatorio
                    int indicePonderado = random.Next(sumaPesos);

                    // Obtener aleatoriamente un tipo de recurso con pesos proporcionales
                    var tipoRecursoAleatorio = TipoRecurso[indicePonderado];

                    int indicePonderadoSistema = random.Next(sumaPesosSistema);
                    var tipoSistemaAleatorio = TipoSistema[indicePonderadoSistema];

                    string dispositivo = (tipoSistemaAleatorio == "Windows") ? "Computer" : "Tablet";


                    var NewRow = new Logs_Accesos
                    {
                        //Guid = Guid.NewGuid(),
                        Empresa     = KEYEMP,
                        IpLocal     = null,
                        IpPublica   = ip,

                        Recurso     = tipoRecursoAleatorio,
                        Sistema     = tipoSistemaAleatorio,
                        Dispositivo = dispositivo,

                        IsoUser     = usuarioAleatorio.Usuario,
                        UserNombre  = usuarioAleatorio.UsuarioNombre,

                        IsoFecAlt   = fechaGenerada
                    };

                    ctxDB.Logs_Accesos.Add(NewRow);
                }

                await ctxDB.SaveChangesAsync();


            }
            catch (Exception e)
            {
                var errorV = e.Message;
            }

        }

        static async Task<string> GetPublicIpAddress()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync("https://httpbin.org/ip");

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    // Analizar el contenido JSON para obtener la dirección IP
                    // Este ejemplo asume que el servicio httpbin devuelve la dirección IP en un formato específico.
                    // Puedes ajustar esto según la respuesta real del servicio que utilices.
                    // Si el servicio devuelve JSON, puedes usar una biblioteca como Newtonsoft.Json para analizarlo.
                    return content.Split('"')[3];
                }
                else
                {
                    throw new HttpRequestException("Error en la solicitud HTTP");
                }
            }
        }

        private async Task Generar_LogsVersiones(string KEYEMP, DateTime FechaImportacion)
        {
            // SE ELIMINAN PREVIAMENTE LOS Logs_Versiones EXISTENTES DE LA EMPRESA 
            var ListLogs_Versiones = ctxDB.Logs_Versiones.Where(x => x.Empresa == KEYEMP).ToList();
            ctxDB.RemoveRange(ListLogs_Versiones);
            await ctxDB.SaveChangesAsync();


            try
            {
                var NewRow = new Logs_Versiones();
                NewRow.Empresa = KEYEMP;
                NewRow.Version = 1;
                NewRow.VersionSub = 1;
                NewRow.Fecha = FechaImportacion;
                NewRow.Descripcion = "Parametrizaciones iniciales";

                NewRow.IsoUser = "Interface";
                NewRow.IsoFecAlt = FechaImportacion;
                NewRow.IsoFecMod = FechaImportacion;

                ctxDB.Logs_Versiones.Add(NewRow);
                await ctxDB.SaveChangesAsync();


                var NewRow2 = new Logs_Versiones();
                NewRow2.Empresa = KEYEMP;
                NewRow2.Version = 1;
                NewRow2.VersionSub = 2;
                NewRow2.Fecha = FechaImportacion;
                NewRow2.Descripcion = "Traspasados datos de interface";

                NewRow2.IsoUser = "Interface";
                NewRow2.IsoFecAlt = FechaImportacion;
                NewRow2.IsoFecMod = FechaImportacion;

                ctxDB.Logs_Versiones.Add(NewRow2);
                await ctxDB.SaveChangesAsync();

            }
            catch (Exception e)
            {
                var errorV = e.Message;
            }

        }

        private async Task Generar_Divisas(string KEYEMP, DateTime FechaImportacion)
        {
            // SE ELIMINAN PREVIAMENTE LOS TarifasVenta EXISTENTES DE LA EMPRESA 
            var ListTarifasVenta = ctxDB.Divisas.Where(x => x.Empresa == KEYEMP).ToList();
            ctxDB.RemoveRange(ListTarifasVenta);
            await ctxDB.SaveChangesAsync();


            try
            {
                var NewRow          = new Divisas();
                NewRow.Empresa      = KEYEMP;
                NewRow.Divisa       = "Euro";
                NewRow.DivNombre    = "Euro";
                NewRow.DivFecha     = FechaImportacion;
                NewRow.DivSimbolo   = "€";
                NewRow.DivCambio    = 1;

                NewRow.IsoUser      = "Interface";
                NewRow.IsoFecAlt    = FechaImportacion;
                NewRow.IsoFecMod    = FechaImportacion;

                ctxDB.Divisas.Add(NewRow);
                await ctxDB.SaveChangesAsync();

                var NewRow2          = new Divisas();
                NewRow2.Empresa      = KEYEMP;
                NewRow2.Divisa       = "Dolar";
                NewRow2.DivNombre    = "Dolar";
                NewRow2.DivFecha     = FechaImportacion;
                NewRow2.DivSimbolo   = "$";
                NewRow2.DivCambio    = 0.9271M;

                NewRow2.IsoUser = "Interface";
                NewRow2.IsoFecAlt = FechaImportacion;
                NewRow2.IsoFecMod = FechaImportacion;

                ctxDB.Divisas.Add(NewRow2);
                await ctxDB.SaveChangesAsync();
            }
            catch (Exception e)
            {
                var errorV = e.Message;
            }

        }

        private async Task Generar_PedidosCAB(string KEYEMP, ExcelWorksheet WS, DateTime FechaImportacion)
        {

            try
            {
                // SE ELIMINAN PREVIAMENTE LOS Proveedores EXISTENTES DE LA EMPRESA 
                var ListPedidosCAB = ctxDB.PedidosCAB.Where(x => x.Empresa == KEYEMP).ToList();
                ctxDB.RemoveRange(ListPedidosCAB);
                await ctxDB.SaveChangesAsync();

                var ListPedidosLIN = ctxDB.PedidosLIN.Where(x => x.Empresa == KEYEMP).ToList();
                ctxDB.RemoveRange(ListPedidosLIN);
                await ctxDB.SaveChangesAsync();


                for (int row = 3; row <= WS.Dimension.Rows + 1; row++)
                {
                    var Cliente     = WS.Cells[row, 2].Text;
                    if (Cliente != null && Cliente != "")
                    {
                        var Pedido = WS.Cells[row, 4].Text;
                        var Fecha = DateTime.ParseExact(WS.Cells[row, 5].Text, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        DateTime? FechaEnt = null;
                        if (WS.Cells[row, 6].Text != "")
                        {
                            FechaEnt = DateTime.ParseExact(WS.Cells[row, 6].Text, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        }


                        var PedValor    = decimal.Parse(WS.Cells[row, 12].Text);                        
                        var PedIva      = decimal.Parse(WS.Cells[row, 13].Text);
                        var PedTotal    = decimal.Parse(WS.Cells[row, 14].Text);

                        Boolean?    PedAlbaran      = null;
                        DateTime?   PedAlbaranFecha = null;

                        if (Boolean.Parse(WS.Cells[row, 18].Text) == true)
                        {
                            PedAlbaran = true;

                            if (!string.IsNullOrEmpty(WS.Cells[row, 19].Text))
                            {
                                PedAlbaranFecha = DateTime.Parse(WS.Cells[row, 19].Text);
                            }
                        }

                        //PedAlbaran 18 
                        //PedAlbaranFecha 19

                        var NewRow              = new PedidosCAB();
                        NewRow.Empresa          = KEYEMP;
                        NewRow.Cliente          = Cliente;
                        NewRow.Pedido           = Pedido;
                        NewRow.PedTarifaVenta   = "T1";

                        NewRow.PedFecha         = Fecha;
                        NewRow.PedFechaEnt      = FechaEnt;
                        NewRow.PedDTOCial       = 0;
                        NewRow.PedDTOPpago      = 0;
                        NewRow.PedDTORappel     = 0;
                        NewRow.PedIVA           = PedIva;

                        NewRow.PedAlbaran       = PedAlbaran;
                        NewRow.PedAlbaranFecha  = PedAlbaranFecha;

                        var FindCli = ctxDB.Clientes.Where(x => x.Empresa == KEYEMP && x.Cliente == Cliente).FirstOrDefault();
                        if (FindCli != null)
                        {
                            NewRow.PedDirFacDireccion = FindCli.CliDirFacDireccion;
                            NewRow.PedDirFacDP = FindCli.CliDirFacDP;
                            NewRow.PedDirFacPoblacion = FindCli.CliDirFacPoblacion;
                            NewRow.PedDirFacPais = FindCli.CliDirFacPais;

                            NewRow.PedDirMerDireccion = FindCli.CliDirMerDireccion;
                            NewRow.PedDirMerDP = FindCli.CliDirMerDP;
                            NewRow.PedDirMerPoblacion = FindCli.CliDirMerPoblacion;
                            NewRow.PedDirMerPais = FindCli.CliDirMerPais;
                        }

                        NewRow.PedEstado    = WS.Cells[row, 10].Text;
                        NewRow.PedTipo      = WS.Cells[row, 11].Text;

                        NewRow.PedDivisa    = "Euro";
                        NewRow.PedIdioma    = "Español";

                        NewRow.IsoUser      = "Interface";
                        NewRow.IsoFecAlt    = FechaImportacion;
                        NewRow.IsoFecMod    = FechaImportacion;

                        ctxDB.PedidosCAB.Add(NewRow);
                        await ctxDB.SaveChangesAsync();

                        var KEYART = "*";
                        // SE BUSCA ARTICULO CON ESA DESCRIPCION
                        var DESCRIPCION = WS.Cells[row, 17].Text;
                        var FindArt = ctxDB.Articulos.Where(x => x.Empresa == GrupoClaims.SessionEmpresa && x.ArtDes == DESCRIPCION).FirstOrDefault();
                        if (FindArt != null)
                        {
                            KEYART = FindArt.Articulo;
                        }

                        // SE CREA LA LINEA
                        var NewLin      = new PedidosLIN();
                        NewLin.Empresa  = KEYEMP;
                        NewLin.Cliente  = Cliente;
                        NewLin.Pedido   = Pedido;
                        NewLin.PedLinea = 1;
                        NewLin.PedArt   = KEYART;
                        NewLin.PedDesc  = DESCRIPCION;

                        NewLin.PedQty = 1;
                        NewLin.PedPrecio = PedValor;
                        NewLin.PedPrecioTotal = PedValor;

                        ctxDB.PedidosLIN.Add(NewLin);
                        await ctxDB.SaveChangesAsync();
                    }



                }

            }
            catch (Exception e)
            {
                var errorV = e.Message;
            }

        }

        private async Task Generar_Tarifas(string KEYEMP, DateTime FechaImportacion)
        {
            // SE ELIMINAN PREVIAMENTE LOS TarifasVenta EXISTENTES DE LA EMPRESA 
            var ListTarifasVenta = ctxDB.TarifasVenta.Where(x => x.Empresa == KEYEMP).ToList();
            ctxDB.RemoveRange(ListTarifasVenta);
            await ctxDB.SaveChangesAsync();


            try
            {
                var NewRow              = new TarifasVenta();
                NewRow.Empresa          = KEYEMP;
                NewRow.Tarifa           = "T1";
                NewRow.TarDescripcion   = "Tarifa General";
                NewRow.TarEtiqueta      = 75;

                NewRow.IsoUser      = "Interface";
                NewRow.IsoFecAlt    = FechaImportacion;
                NewRow.IsoFecMod    = FechaImportacion;
                //NewRow.IsoFecAlt    = DateTime.Now.AddDays(DiasRestaToday);
                //NewRow.IsoFecMod    = DateTime.Now.AddDays(DiasRestaToday);

                ctxDB.TarifasVenta.Add(NewRow);
                await ctxDB.SaveChangesAsync();

                var NewRow2 = new TarifasVenta();
                NewRow2.Empresa          = KEYEMP;
                NewRow2.Tarifa           = "T2";
                NewRow2.TarDescripcion   = "Tarifa Especial";
                NewRow2.TarEtiqueta      = 60;

                NewRow2.IsoUser      = "Interface";
                NewRow2.IsoFecAlt    = FechaImportacion;
                NewRow2.IsoFecMod    = FechaImportacion;

                ctxDB.TarifasVenta.Add(NewRow2);
                await ctxDB.SaveChangesAsync();
            }
            catch (Exception e)
            {
                var errorV = e.Message;
            }

        }

        private async Task Generar_Proveedores(string KEYEMP, ExcelWorksheet WS, DateTime FechaImportacion)
        {
            // SE ELIMINAN PREVIAMENTE LOS Proveedores EXISTENTES DE LA EMPRESA 
            var ListProveedores = ctxDB.Proveedores.Where(x => x.Empresa == KEYEMP).ToList();
            ctxDB.RemoveRange(ListProveedores);
            await ctxDB.SaveChangesAsync();


            try
            {

                for (int row = 3; row <= WS.Dimension.Rows + 1; row++)
                {
                    var Proveedor       = WS.Cells[row, 2].Text;
                    if (Proveedor != null && Proveedor != "")
                    {
                        var Nombre = WS.Cells[row, 3].Text;
                        var RazonSocial = WS.Cells[row, 4].Text;

                        var NewRow = new Proveedores();
                        NewRow.Empresa = KEYEMP;
                        NewRow.Proveedor = Proveedor;
                        NewRow.ProNombre = Nombre;
                        NewRow.ProRazon = RazonSocial;

                        NewRow.ProDirFacDireccion = WS.Cells[row, 5].Text;
                        NewRow.ProDirFacDP = WS.Cells[row, 6].Text;
                        NewRow.ProDirFacPoblacion = WS.Cells[row, 7].Text;
                        NewRow.ProDirFacPais = WS.Cells[row, 8].Text;

                        NewRow.ProDirMerDireccion = WS.Cells[row, 5].Text;
                        NewRow.ProDirMerDP = WS.Cells[row, 6].Text;
                        NewRow.ProDirMerPoblacion = WS.Cells[row, 7].Text;
                        NewRow.ProDirMerPais = WS.Cells[row, 8].Text;


                        NewRow.ProNIF = WS.Cells[row, 9].Text;
                        NewRow.ProTelefono1 = WS.Cells[row, 10].Text;
                        NewRow.ProMail = WS.Cells[row, 11].Text;

                        NewRow.IsoUser = "Interface";
                        NewRow.IsoFecAlt = FechaImportacion;
                        NewRow.IsoFecMod = FechaImportacion;

                        ctxDB.Proveedores.Add(NewRow);
                        await ctxDB.SaveChangesAsync();
                    }


                }

            }
            catch (Exception e)
            {
                var errorV = e.Message;
            }

        }

        private async Task Generar_Clientes(string KEYEMP, ExcelWorksheet WS, DateTime FechaImportacion)
        {
            // SE ELIMINAN PREVIAMENTE LOS ARTICULOS EXISTENTES DE LA EMPRESA 
            var ListClientes = ctxDB.Clientes.Where(x => x.Empresa == KEYEMP).ToList();
            ctxDB.RemoveRange(ListClientes);
            await ctxDB.SaveChangesAsync();


            try
            {

                for (int row = 3; row <= WS.Dimension.Rows + 1; row++)
                {
                    var Cliente     = WS.Cells[row, 2].Text;
                    if (Cliente != null && Cliente != "")
                    {

                        var Nombre = WS.Cells[row, 3].Text;
                        var RazonSocial = WS.Cells[row, 4].Text;

                        var NewRow = new Clientes();
                        NewRow.Empresa = KEYEMP;
                        NewRow.Cliente = Cliente;
                        NewRow.CliNombre = Nombre;
                        NewRow.CliRazon = RazonSocial;

                        string[] dir6 = WS.Cells[row, 6].Text.Split(' ');
                        NewRow.CliDirFacDireccion = WS.Cells[row, 5].Text;

                        if (dir6.Length > 1)
                        {
                            NewRow.CliDirFacDP = dir6[0];
                            NewRow.CliDirFacPoblacion = dir6[1];
                        }
                        else
                        {
                            NewRow.CliDirFacPoblacion = dir6[0];
                        }
                        NewRow.CliDirFacPais = "España";

                        NewRow.CliDirMerDireccion = WS.Cells[row, 5].Text;

                        if (dir6.Length > 1)
                        {
                            NewRow.CliDirMerDP = dir6[0];
                            NewRow.CliDirMerPoblacion = dir6[1];
                        }
                        else
                        {
                            NewRow.CliDirMerPoblacion = dir6[0];
                        }
                        NewRow.CliDirMerPais = "España";


                        NewRow.CliNIF = WS.Cells[row, 7].Text;
                        NewRow.CliTelefono1 = WS.Cells[row, 8].Text;
                        NewRow.CliTelefono2 = WS.Cells[row, 9].Text;
                        NewRow.CliMail = WS.Cells[row, 11].Text;

                        NewRow.CliTarifaVenta = WS.Cells[row, 21].Text;

                        //NewRow.activo

                        NewRow.CliDivisa = "Euro";
                        NewRow.IsoUser = "Interface";
                        NewRow.IsoFecAlt = FechaImportacion;
                        NewRow.IsoFecMod = FechaImportacion;

                        ctxDB.Clientes.Add(NewRow);
                        await ctxDB.SaveChangesAsync();
                    }

                }

            }
            catch (Exception e)
            {
                var errorV = e.Message;
            }

        }

        private async Task Generar_Articulos(string KEYEMP, ExcelWorksheet WS, DateTime FechaImportacion)
        {



            try
            {
                // SE ELIMINAN PREVIAMENTE LOS ARTICULOS EXISTENTES DE LA EMPRESA 
                var ListArticulos = ctxDB.Articulos.Where(x => x.Empresa == KEYEMP).ToList();
                ctxDB.RemoveRange(ListArticulos);
                await ctxDB.SaveChangesAsync();


                // SE CPMPRUEBA SI TIENE FIXIND DADO DE ALTA, NECESARIO PARA LUEGO NO PETE AL SIMULADOR DE PRECIOS
                var FindFixing = ctxDB.Fixing.Where(x => x.Empresa == KEYEMP).FirstOrDefault();
                if (FindFixing == null)
                {
                    var NewMetal        = new Fixing();
                    NewMetal.Empresa    = KEYEMP;
                    NewMetal.Tipo       = "ORO";
                    NewMetal.Metal      = "Oro";

                    NewMetal.K18Ley         = 100;
                    NewMetal.K18Milesimas   = 100;

                    NewMetal.Fecha      = FechaImportacion;
                    NewMetal.Observaciones = "";

                    ctxDB.Fixing.Add(NewMetal);
                    await ctxDB.SaveChangesAsync();
                }


                for (int row = 3; row <= WS.Dimension.Rows + 1; row++)
                {
                    var Art     = WS.Cells[row, 2].Text;
                    if (Art != null && Art != "")
                    {
                        var Desc = WS.Cells[row, 3].Text;
                        var Fam = WS.Cells[row, 10].Text;
                        var Pvp = decimal.Parse(WS.Cells[row, 4].Text);
                        var Tar1 = decimal.Parse(WS.Cells[row, 6].Text);
                        //var Art = WS.Cells[row, 2].Text;

                        var NewRow = new Articulos();
                        NewRow.Empresa = KEYEMP;
                        NewRow.MultiEmpresa = "";
                        NewRow.Articulo = Art;
                        NewRow.ArtDes = Desc;
                        NewRow.ArtFamilia = Fam;

                        NewRow.ArtTipo = "FAB";

                        if (Pvp > 0)
                        {
                            NewRow.ArtPrecioPVP = Pvp;
                        }
                        else
                        {
                            NewRow.ArtPrecioPVP = Tar1;
                        }

                        NewRow.Activo = true;
                        NewRow.IsoUser = "Interface";
                        NewRow.IsoFecAlt = FechaImportacion;
                        NewRow.IsoFecMod = FechaImportacion;

                        ctxDB.Articulos.Add(NewRow);
                        await ctxDB.SaveChangesAsync();
                    }
                        
                }

            }
            catch (Exception e)
            {
                var errorV = e.Message;
            }

        }

        public async Task Generar_Usuarios (string KEYEMP, ExcelWorksheet WS, DateTime FechaImportacion)
        {
            // SE ELIMINAN PREVIAMENTE LOS USUARIOS EXISTENTES DE LA EMPRESA 
            var ListUsers = ctxDB.Usuarios.Where(x=>x.Empresa == KEYEMP).ToList();
            ctxDB.RemoveRange(ListUsers);
            await ctxDB.SaveChangesAsync();

            try
            {
                for (int row = 3; row <= WS.Dimension.Rows + 1; row++)
                {
                    var UserMail    = WS.Cells[row, 2].Text;
                    if (UserMail != null && UserMail != "")
                    {
                        var Nombre = WS.Cells[row, 3].Text;
                        var Tipo = WS.Cells[row, 4].Text;
                        var Password = WS.Cells[row, 5].Text;

                        var NewUser = new Usuarios();
                        NewUser.Empresa = KEYEMP;
                        NewUser.Usuario = UserMail;
                        NewUser.UsuarioNombre = Nombre;
                        NewUser.UsuarioTipo = Tipo;
                        NewUser.Password = Password;
                        NewUser.ControllerInit = "Clientes";

                        var MenuStd = JsonSerializer.Serialize(FunctionsBBDD.GetMenuAccesosStandar());
                        NewUser.Menus = MenuStd;

                        NewUser.IsoUser = "Interface";
                        NewUser.IsoFecAlt = FechaImportacion;
                        NewUser.IsoFecMod = FechaImportacion;

                        ctxDB.Usuarios.Add(NewUser);
                        await ctxDB.SaveChangesAsync();
                    }
                        
                }


            }
            catch (Exception e)
            {
                var errorV = e.Message;
            }


        }

    }
}

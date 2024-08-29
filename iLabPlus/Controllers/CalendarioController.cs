using iLabPlus.Helpers;
using iLabPlus.Models.BDiLabPlus;
using iLabPlus.Models.Clases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace iLabPlus.Controllers
{
    [Authorize]
    public class CalendarioController : Controller
    {
        private readonly DbContextiLabPlus ctxDB;
        private readonly FunctionsBBDD FunctionsBBDD;
        private readonly GrupoClaims GrupoClaims;
        private readonly ITestOutputHelper _output;


        public CalendarioController(DbContextiLabPlus Context, FunctionsBBDD _FunctionsBBDD, ITestOutputHelper output = null)
        {
            ctxDB = Context;
            FunctionsBBDD = _FunctionsBBDD;
            GrupoClaims = FunctionsBBDD.GetClaims();
            _output = output;
        }

        public IActionResult Index()
        {
            ViewBag.MenuUserList = FunctionsBBDD.GetMenuAccesos();

            return View();
        }



        [HttpGet]
        public IActionResult ObtenerEmpresaActual()
        {
            return Ok(new { Empresa = GrupoClaims.SessionEmpresa });
        }



        [HttpGet]
        public async Task<IActionResult> ObtenerUsuarios()
        {
            // Obtener usuarios
            var usuarios = await ctxDB.Usuarios
                                .Where(u => u.Empresa == GrupoClaims.SessionEmpresa)
                                .Select(u => new
                                {
                                    Guid = u.Guid,
                                    NombreCompleto = u.UsuarioNombre
                                }).ToListAsync();

            return Ok(usuarios);
        }



        [HttpGet]

        public IActionResult ObtenerEventos()
        {

            ViewBag.MenuUserList = FunctionsBBDD.GetMenuAccesos();

            // Obtengo los eventos de Solo Yo
            var alcanceSoloYo = GetEventosAlcanceSoloYo();

            // Obtener eventos de "Todos"
            var alcanceTodos = GetEventosAlcanceTodos();

            // Obtener eventos de "Usuarios Múltiples"
            var alcanceUsuarioMultiples = GetEventosAlcanceUsuarioMultiples();

            // Con esto obtengo eventos de AlcanceObservador!
            var alcanceObservador = GetEventosAlcanceObservador();

            // Combino todos los eventos
            var eventos = alcanceSoloYo.Concat(alcanceTodos).Concat(alcanceUsuarioMultiples).Concat(alcanceObservador).ToList();

            return Json(eventos);

        }

        protected virtual List<object> GetEventosAlcanceObservador()
        {
            var eventos = ctxDB.Calendario
                               .Where(e => e.Empresa == GrupoClaims.SessionEmpresa && e.Usuario == GrupoClaims.SessionUsuario && e.Alcance == "Observador")
                               .Select(e => CrearObjetoEvento(e, "#C0C3C7"))
                               .ToList();
            return eventos;
        }

        // Nuevo método público para pruebas
        public List<object> GetEventosAlcanceObservadorForTesting()
        {
            return GetEventosAlcanceObservador();
        }

        protected virtual List<object> GetEventosAlcanceSoloYo()
        {
            var eventos = ctxDB.Calendario
                               .Where(e => e.Usuario == GrupoClaims.SessionUsuario && e.Empresa == GrupoClaims.SessionEmpresa && e.Alcance == "Solo Yo")
                               .Select(e => CrearObjetoEvento(e, "#FF0000"))
                               .ToList();
            return eventos;
        }

        public List<object> GetEventosAlcanceSoloYoForTesting()
        {
            return GetEventosAlcanceSoloYo();
        }


        protected virtual List<object> GetEventosAlcanceTodos()
        {
            var eventos = ctxDB.Calendario
                               .Where(e => e.Empresa == GrupoClaims.SessionEmpresa && e.Alcance == "Todos" && (GrupoClaims.SessionUsuarioTipo == "ADMIN" || GrupoClaims.SessionUsuarioTipo == "USUARIO"))
                               .Select(e => CrearObjetoEvento(e, "#3CB371"))
                               .ToList();
            return eventos;
        }

        public List<object> GetEventosAlcanceTodosForTesting()
        {
            return GetEventosAlcanceTodos();
        }

        protected virtual List<object> GetEventosAlcanceUsuarioMultiples()
        {
            try
            {
                var eventos = ctxDB.Calendario
                                    .Where(e => e.Empresa == GrupoClaims.SessionEmpresa &&
                                                ctxDB.CalendarioMultiUsuarios.Any(eu => eu.GuidEvento == e.Guid && eu.Usuario == GrupoClaims.SessionUsuario))
                                    .Select(e => CrearObjetoEvento(e, "#1B6EC2"))
                                    .ToList();
                return eventos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener eventos de usuarios múltiples: " + ex.Message);
            }
        }

        public List<object> GetEventosAlcanceUsuarioMultiplesForTesting()
        {
            return GetEventosAlcanceUsuarioMultiples();
        }



        //private static object CrearObjetoEvento(Calendario e, string color)
        //{
        //    var startDateTime = e.Fecha.Add(e.HoraInicio).ToString("yyyy-MM-ddTHH:mm:ss");
        //    var endDateTime = e.Fecha.Add(e.HoraFin).ToString("yyyy-MM-ddTHH:mm:ss");
        //    var allDayStart = e.Fecha.ToString("yyyy-MM-dd");
        //    var allDayEnd = e.Fecha.ToString("yyyy-MM-dd");

        //    return new
        //    {
        //        id = e.Guid,
        //        title = e.HoraInicio.ToString(@"hh\:mm") + " a " + e.HoraFin.ToString(@"hh\:mm") + " " + e.Titulo,
        //        start = startDateTime,
        //        end = endDateTime,
        //        allDayStart = allDayStart, // Fecha sin hora para vista de mes
        //        allDayEnd = allDayEnd,     // Fecha sin hora para vista de mes
        //        color = color,
        //        textColor = "#ffffff"
        //    };
        //}

        private static object CrearObjetoEvento(Calendario e, string color)
        {
            return new
            {
                id = e.Guid,
                title = e.HoraInicio.ToString(@"hh\:mm") + " a " + e.HoraFin.ToString(@"hh\:mm") + " " + e.Titulo,
                start = e.Fecha.Add(e.HoraInicio).ToString("yyyy-MM-ddTHH:mm:ss"),
                end = e.Fecha.Add(e.HoraFin).ToString("yyyy-MM-ddTHH:mm:ss"),
                allDayStart = e.Fecha.ToString("yyyy-MM-dd"),
                allDayEnd = e.Fecha.ToString("yyyy-MM-dd"),
                color = color,
                textColor = "#ffffff"
            };
        }


        public IActionResult ObtenerDetallesEvento(string guidEvento)
        {
            var evento = ctxDB.Calendario
                .Where(e => e.Guid == new Guid(guidEvento) && e.Usuario == GrupoClaims.SessionUsuario)
                .FirstOrDefault();

            if (evento == null)
            {
                return NotFound();
            }

            return Json(new Dictionary<string, object>
            {
                ["Titulo"] = evento.Titulo,
                ["Fecha"] = evento.Fecha.ToString("yyyy-MM-dd"),
                ["Detalle"] = evento.Detalle,
                ["HoraInicio"] = evento.HoraInicio.ToString(@"hh\:mm"),
                ["HoraFin"] = evento.HoraFin.ToString(@"hh\:mm"),
                ["Alcance"] = evento.Alcance,
            });
        }



        [HttpPost]
        public async Task<IActionResult> AgregarEvento([FromBody] CalendarioEventoDto CalendarioEventoDto)
        {
            try
            {
                var nuevoEvento = new Calendario
                {
                    Usuario = GrupoClaims.SessionUsuario,
                    Empresa = GrupoClaims.SessionEmpresa,
                    Titulo = CalendarioEventoDto.Titulo,
                    Fecha = DateTime.Parse(CalendarioEventoDto.Fecha),
                    Detalle = CalendarioEventoDto.Detalle,
                    HoraInicio = TimeSpan.Parse(CalendarioEventoDto.HoraInicio),
                    HoraFin = TimeSpan.Parse(CalendarioEventoDto.HoraFin),
                    Alcance = CalendarioEventoDto.Alcance,
                    UsuarioEspecifico = CalendarioEventoDto.Alcance == "Todos" ? "Todos" : CalendarioEventoDto.UsuarioEspecifico,
                    RecibirMail = CalendarioEventoDto.RecibirMail,
                };

                if (CalendarioEventoDto.Alcance == "Solo yo" || CalendarioEventoDto.Alcance == "Observador")
                {
                    nuevoEvento.UsuarioEspecifico = GrupoClaims.SessionUsuario;
                }
                else if (CalendarioEventoDto.Alcance == "Varios usuarios")
                {
                    var usuarioSesion = await ctxDB.Usuarios.FindAsync(GrupoClaims.SessionUsuario);

                    if (usuarioSesion != null && usuarioSesion.Empresa == GrupoClaims.SessionEmpresa)
                    {
                        if (CalendarioEventoDto.Usuarios == null)
                        {
                            CalendarioEventoDto.Usuarios = new List<string>();
                        }
                        if (!CalendarioEventoDto.Usuarios.Contains(usuarioSesion.Guid.ToString()))
                        {
                            CalendarioEventoDto.Usuarios.Add(usuarioSesion.Guid.ToString());
                        }
                        _output?.WriteLine($"Usuario de sesión agregado: {usuarioSesion.Usuario}");
                    }
                }

                if (GrupoClaims.SessionUsuarioTipo == "USUARIO")
                {
                    nuevoEvento.Alcance = "Solo Yo";
                }

                await ctxDB.Calendario.AddAsync(nuevoEvento);

                if ((CalendarioEventoDto.Alcance == "Varios usuarios" || CalendarioEventoDto.Alcance == "Observador") && CalendarioEventoDto.Usuarios != null)
                {
                    foreach (var usuarioGuid in CalendarioEventoDto.Usuarios)
                    {
                        var usuarioInfo = await ctxDB.Usuarios.FindAsync(Guid.Parse(usuarioGuid));
                        if (usuarioInfo != null)
                        {
                            var eventoUsuario = new CalendarioMultiUsuarios
                            {
                                GuidEvento = nuevoEvento.Guid,
                                Usuario = usuarioInfo.Usuario,
                                Empresa = nuevoEvento.Empresa
                            };
                            await ctxDB.CalendarioMultiUsuarios.AddAsync(eventoUsuario);
                            _output?.WriteLine($"Usuario agregado a CalendarioMultiUsuarios: {usuarioInfo.Usuario}");
                        }
                    }
                }

                await ctxDB.SaveChangesAsync();

                return StatusCode(200, nuevoEvento);
            }
            catch (Exception ex)
            {
                _output?.WriteLine($"Error en AgregarEvento: {ex.Message}");
                _output?.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(400, ex.Message);
            }
        }


        [HttpPost]
        public async Task<IActionResult> ActualizarEvento([FromBody] CalendarioEventoDto CalendarioEventoDto)
        {
            try
            {
                _output?.WriteLine("Iniciando ActualizarEvento");

                var eventoExistente = ctxDB.Calendario.FirstOrDefault(e => e.Guid == new Guid(CalendarioEventoDto.Guid) && e.Usuario == GrupoClaims.SessionUsuario);

                if (eventoExistente == null)
                {
                    return NotFound("Evento no encontrado");
                }

                // Logging para depuración
                _output?.WriteLine($"Evento encontrado: {eventoExistente.Guid}");

                eventoExistente.Titulo = CalendarioEventoDto.Titulo;
                eventoExistente.Fecha = DateTime.Parse(CalendarioEventoDto.Fecha);
                eventoExistente.Detalle = CalendarioEventoDto.Detalle;
                eventoExistente.HoraInicio = TimeSpan.Parse(CalendarioEventoDto.HoraInicio);
                eventoExistente.HoraFin = TimeSpan.Parse(CalendarioEventoDto.HoraFin);
                eventoExistente.Alcance = CalendarioEventoDto.Alcance;
                eventoExistente.RecibirMail = CalendarioEventoDto.RecibirMail;

                if (CalendarioEventoDto.Alcance == "Solo yo" || CalendarioEventoDto.Alcance == "Observador")
                {
                    eventoExistente.UsuarioEspecifico = GrupoClaims.SessionUsuario;
                }
                else if (CalendarioEventoDto.Alcance != "Varios usuarios")
                {
                    eventoExistente.UsuarioEspecifico = CalendarioEventoDto.UsuarioEspecifico;
                }

                if (GrupoClaims.SessionUsuarioTipo == "USUARIO")
                {
                    eventoExistente.Alcance = "Solo Yo";
                }

                // Logging para depuración
                _output?.WriteLine($"Evento actualizado: {eventoExistente.Titulo}, Alcance: {eventoExistente.Alcance}");

                _output?.WriteLine("Buscando usuarios antiguos");
                var usuariosAntiguos = ctxDB.CalendarioMultiUsuarios.AsEnumerable()
                    .Where(eu => eu.GuidEvento == eventoExistente.Guid)
                    .ToList();
                _output?.WriteLine($"Encontrados {usuariosAntiguos.Count} usuarios antiguos");

                if (usuariosAntiguos.Any())
                {
                    ctxDB.CalendarioMultiUsuarios.RemoveRange(usuariosAntiguos);
                }


                if (CalendarioEventoDto.Alcance == "Varios usuarios" && CalendarioEventoDto.Usuarios != null)
                {
                    foreach (var usuario in CalendarioEventoDto.Usuarios)
                    {
                        var eventoUsuario = new CalendarioMultiUsuarios
                        {
                            GuidEvento = eventoExistente.Guid,
                            Usuario = usuario,
                            Empresa = eventoExistente.Empresa
                        };
                        await ctxDB.CalendarioMultiUsuarios.AddAsync(eventoUsuario);
                    }
                }

                ctxDB.Calendario.Update(eventoExistente);

                _output?.WriteLine("Llamando a SaveChangesAsync");
                await ctxDB.SaveChangesAsync();


                _output?.WriteLine("SaveChangesAsync completado");
                return Ok();
            }
            catch (Exception ex)
            {
                _output?.WriteLine($"Excepción en ActualizarEvento: {ex.GetType().Name}");
                _output?.WriteLine($"Mensaje: {ex.Message}");
                _output?.WriteLine($"StackTrace: {ex.StackTrace}");
                return BadRequest("Ha habido un problema al actualizar evento: " + ex.Message);
            }
        }


        public class GuidDTO
        {
            public Guid Guid { get; set; }
        }


        [HttpPost]

        public async Task<IActionResult> EliminarEvento([FromBody] GuidDTO guidDTO)
        {
            var eventoExistente = await ctxDB.Calendario.FindAsync(guidDTO.Guid);
            if (eventoExistente == null)
            {
                return NotFound("Evento a eliminar no encontrado");
            }

            ctxDB.Calendario.Remove(eventoExistente);
            await ctxDB.SaveChangesAsync();

            return Ok(); // Devuelvo un 200 OK para confirmar que todo fue joya
        }

        public IActionResult _ModalEvento()
        {

            return PartialView();

        }

    }
}
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TP05_Martinez_Loufer.Models;

namespace TP05_Martinez_Loufer.Controllers;

/* plan: Vamos a añadir un forma de recuperar tu contraseña si te la olvidaste, para esto, primero vamos a crear un  nuevo parametro en usuario que sea email, luego, creearemos una nueva view a llamada recuperarContrasenia, en esta uno tendra que ingresar el email y , si este existe en la base de datos, el usuario podra cambiar su contraseña
plan creado: Plan: Recuperación de Contraseña con Email
TL;DR: Agregar email como campo obligatorio en el Registro, crear una nueva vista RecuperarContrasenia donde el usuario ingresa su email y, si existe, genera un token temporal que se almacena en BD. Con ese token, el usuario puede cambiar su contraseña. Como no hay SMTP configurado, se mostrará el enlace/código en pantalla o vía tabla temporal.

Decisión importante: Dado que elegiste validación local sin SMTP real, usaremos un token temporal almacenado en la BD que expira en 30 minutos.

Steps
Actualizar modelo Usuario.cs

Agregar propiedades: Email (string), Id (int)
Validar formato email con regex
Actualizar tabla BD en BD.sql

Agregar columna Email (VARCHAR(100), NOT NULL, UNIQUE)
Agregar tabla TokensRecuperacion con campos: TokenId, UsuarioId, Token, FechaExpiracion
Actualizar clase BD.cs

Agregar método BuscarPorEmail(string email) - retorna Usuario si existe
Agregar método GenerarTokenRecuperacion(int usuarioId) - crea token con expiración
Agregar método ValidarYObtenerTokenRecuperacion(string token) - valida token y retorna usuarioId
Agregar método ActualizarPassword(int usuarioId, string nuevaPassword) - cambia contraseña
Actualizar Registro.cshtml

Agregar campo Email con validación
Actualizar validación JavaScript
Actualizar HomeController.cs

Actualizar action RegistroUsuario() para guardar email
Agregar action RecuperarContrasenia() (GET) - muestra formulario
Agregar action EnviarLinkRecuperacion() (POST) - valida email y genera token
Agregar action ValidarToken() (GET) - verifica token y muestra formulario de cambio
Agregar action CambiarPassword() (POST) - actualiza contraseña con token
Crear vista Views/Home/RecuperarContrasenia.cshtml

Formulario para ingresar email
Mensaje con token/enlace si es exitoso
Crear vista Views/Home/CambiarPassword.cshtml

Formulario para ingresar nueva contraseña
Campo oculto con token
Validación de contraseña
Verificación
Registrar nuevo usuario con email
Ir a "Recuperar Contraseña" e ingresar email inexistente (debe mostrar error)
Ingresar email existente (debe generar token)
Usar token para cambiar contraseña
Intentar usar token expirado (debe rechazar)
Intentar usar token inválido (debe rechazar)
Hacer login con nueva contraseña
Decisiones
Token temporal: 30 minutos de expiración, almacenado en BD
Email único: No se permiten dos usuarios con el mismo email
Validación email: Formato básico (usuario@dominio.com)
Sin SMTP: Token se muestra en pantalla (puedes agregar SMTP después)
*/



//pregunta: Que es: Guid.NewGuid().ToString("N"), GETDATE(), ViewBag.EnlaceRecuperacion = $"/Home/ValidarToken?token={token}";
/*respuesta: 
Explicación de los Componentes
1. Guid.NewGuid().ToString("N")
Genera un token único e impredecible para la recuperación de contraseña.

Guid.NewGuid()              // Crea un GUID único (ej: 550e8400-e29b-41d4-a716-446655440000)
            .ToString("N")  // Lo convierte a string SIN guiones (ej: 550e8400e29b41d4a716446655440000)

Ventaja: Casi imposible adivinar. Alguien tendría que probar billones de combinaciones.

2. GETDATE()
Función de SQL Server que obtiene la fecha y hora actual.

3. ViewBag.EnlaceRecuperacion = $"/Home/ValidarToken?token={token}";
Construye un URL que el usuario puede usar para cambiar su contraseña.

Ejemplo práctico:
Si token = "a7f3d9e2c1b5k8m2"

Resultado:
ViewBag.EnlaceRecuperacion = "/Home/ValidarToken?token=a7f3d9e2c1b5k8m2"

En la vista HTML se vería:
<a href="/Home/ValidarToken?token=a7f3d9e2c1b5k8m2">Cambiar Contraseña</a>

URL final en navegador:
https://localhost:7074/Home/ValidarToken?token=a7f3d9e2c1b5k8m2
                                                      ↑ QueryString (parámetro en URL)
*/



public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }
    
    public IActionResult Index()
    {
        
        if (HttpContext.Session.GetString("id") == null)
        {
            return RedirectToAction("Login");
        }
        int id = int.Parse(HttpContext.Session.GetString("id"));
        BD bd = new BD();
        ViewBag.usuario = bd.ObtenerUsuarioPorId(id);
        return View();
    }
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
    public IActionResult RegistroUsuario(string nombreUsuario, string password, string nombre, string apellido, string tipoUsuario, string email)
    {
        BD bd = new BD();
        if(bd.RegistrarUsuario(nombreUsuario, password, nombre, apellido, tipoUsuario, email))
        {
            return RedirectToAction("Login");
        }
        return RedirectToAction("Registro", new { mensajeError = "Ese usuario ya existe" });
    }
    [HttpPost]
    public IActionResult VerificarSesion(string nombreUsuario, string password)
    {
        BD bd = new BD();
        int id = bd.ValidarUsuario(nombreUsuario, password);
        if (id == 0)
        {
           return RedirectToAction("Login", new { mensajeError = "Usuario o contraseña incorrectos" }); 
        }
        HttpContext.Session.SetString("id", id.ToString());
        return RedirectToAction("Index");
    }

    public IActionResult Login(string mensajeError, string mensajeExito)
    {
        ViewBag.MensajeError = mensajeError;
        ViewBag.MensajeExito = mensajeExito;
        return View();
    }

    public IActionResult Registro(string mensajeError)
    {
        ViewBag.MensajeError = mensajeError;
        return View();
    }

    public IActionResult RecuperarContrasenia(string mensajeError)
    {
        ViewBag.MensajeError = mensajeError;
        return View();
    }

    [HttpPost]
    public IActionResult EnviarLinkRecuperacion(string email)
    {
        BD bd = new BD();
        Usuario usuario = bd.BuscarPorEmail(email);
        
        if (usuario == null)
        {
            return RedirectToAction("RecuperarContrasenia", new { mensajeError = "El email no existe en el sistema" });
        }

        string token = bd.GenerarTokenRecuperacion(usuario.Id);
        ViewBag.Token = token;
        ViewBag.Email = email;
        ViewBag.EnlaceRecuperacion = $"/Home/ValidarToken?token={token}";
        
        return View("TokenGenerado");
    }

    public IActionResult ValidarToken(string token)
    {
        BD bd = new BD();
        int usuarioId = bd.ValidarTokenRecuperacion(token);
        
        if (usuarioId == 0)
        {
            return RedirectToAction("RecuperarContrasenia", new { mensajeError = "El token es inválido o ha expirado" });
        }

        ViewBag.UsuarioId = usuarioId;
        return View("CambiarPassword");
    }

    [HttpPost]
    public IActionResult CambiarPassword(int usuarioId, string nuevoPassword)
    {
        BD bd = new BD();
        if (bd.ActualizarPassword(usuarioId, nuevoPassword))
        {
            return RedirectToAction("Login", new { mensajeExito = "Contraseña actualizada correctamente. Inicia sesión." });
        }
        ViewBag.UsuarioId = usuarioId;
        ViewBag.MensajeError = "Error al actualizar la contraseña";
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

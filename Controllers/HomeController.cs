using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TP05_Martinez_Loufer.Models;

namespace TP05_Martinez_Loufer.Controllers;

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
    public IActionResult RegistroUsuario(string nombreUsuario, string password, string nombre, string apellido, string tipoUsuario)
    {
        BD bd = new BD();
        if(bd.RegistrarUsuario(nombreUsuario, password, nombre, apellido, tipoUsuario))
        {
            return RedirectToAction("Login");
        }
        return RedirectToAction("Registro");
    }
    [HttpPost]
    public IActionResult VerificarSesion(string nombreUsuario, string password)
    {
        BD bd = new BD();
        int id = bd.ValidarUsuario(nombreUsuario, password);
        if (id == 0)
        {
           return RedirectToAction("Login"); 
        }
        HttpContext.Session.SetString("id", id.ToString());
        return RedirectToAction("Index");
    }

    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Registro()
    {
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

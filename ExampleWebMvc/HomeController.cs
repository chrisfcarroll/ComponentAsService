using Microsoft.AspNetCore.Mvc;

namespace ExampleWebApp
{
    
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Content("<body><h1>ExampleWebApp HomeController/Index Method","text/html");
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace ExampleWebMvc
{
    
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Content("<body><h1>ExampleWebApp HomeController/Index Method","text/html");
        }
    }
}

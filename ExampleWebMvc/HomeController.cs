using Microsoft.AspNetCore.Mvc;

namespace ExampleWebMvc
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}

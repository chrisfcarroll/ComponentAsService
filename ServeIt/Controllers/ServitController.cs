using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Example;
using Microsoft.AspNetCore.Mvc;

namespace ServeIt.Controllers
{
    public class ServeItController<I> : Controller
    {
        
    }

    public class ExampleController : ServeItController<IExemplify>{}
}

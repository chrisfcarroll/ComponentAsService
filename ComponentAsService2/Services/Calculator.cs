using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ComponentAsService2.Services
{
    public class Calculator
    {
        public int Add(int a, int b, int c)=> a+b+c;
        public float Add(float a, float b, float c) => a +b +c;
    }
}

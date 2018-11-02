using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Component.As.Service.UseComponentAsService;
using Extensions.Logging.ListOfString;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using TestBase;
using Xunit;

namespace Component.As.Service.Specs.FinerGrainedActionSelection
{
    public class SelectActionByParameterNameAndConvertibilitySpecs : BaseForCreatingActionDescriptors
    {
        [Theory]
        [InlineData( typeof(bool)  ,     5 - 2000)]
        [InlineData( typeof(float) ,   903 - 2000)]
        [InlineData( typeof(string),  1001 - 2000 )]
        [InlineData( typeof(int)   ,  1005 - 2000 )]
        public void ScoreGivenRouteValuesA1BBCC_IsAsPerAlgorithm(Type type, int expectedScore)
        {
            var incomingValues = new RouteValueDictionary(new {a = 1, b="b", c="c"});
            var rc = CreateRouteContext("POST");
            var action = new ActionDescriptor
            {
                Parameters = new[]{new ParameterDescriptor {Name = "a", ParameterType = type}}
            };

            ScoreByParameterNameAndConvertibility.Score(incomingValues, rc,action).ShouldBe( expectedScore );
        }


        [Theory]
        [InlineData(typeof(bool)  ,typeof(bool) ,      0 + 10 -    0)]
        [InlineData(typeof(string),typeof(string),  2000 +  2 -    0)]
        [InlineData(typeof(int)   ,typeof(int) ,    1900 + 10 -    0)]
        [InlineData(typeof(float),typeof(float) ,   1800 +  6 -    0)]
        [InlineData(typeof(double),typeof(double) , 1900 +  4 -    0 )]
        public void ScoreGivenRouteValuesA1point0B1_IsAsPerAlgorithm(Type typeA, Type typeB, int expectedScore)
        {
            var incomingValues = new RouteValueDictionary(new {a = 1.0, b=1});
            var rc = CreateRouteContext("POST");
            var action = new ActionDescriptor
            {
                Parameters = new[]
                {
                    new ParameterDescriptor {Name = "a", ParameterType = typeA},
                    new ParameterDescriptor {Name = "b", ParameterType = typeB},
                }
            };

            ScoreByParameterNameAndConvertibility
                .Score(incomingValues,rc, action)
                .ShouldBe( expectedScore );
        }

        [Fact]
        public void DoesNotCountUnconvertibleParametersAsAMatch()
        {
            // Arrange
            var loglines= new List<string>();
            var loggerFactory = new LoggerFactory().AddStringListLogger(loglines);

            var incomingValues= new RouteValueDictionary(new{a=1, b="b", c="c"});
            var routeContext = CreateRouteContext("POST",incomingValues);

            var actions = new[]
            {
                new ControllerActionDescriptor
                { 
                    ControllerName = "Controller",
                    ControllerTypeInfo = typeof(AController).GetTypeInfo(),
                    MethodInfo = typeof(AController).GetMethod("A1", new[]{typeof(int),typeof(int),typeof(int),}),
                    DisplayName = "A1", 
                    Parameters = new[]
                    {
                        new ParameterDescriptor{Name="a", ParameterType = typeof(int)},
                        new ParameterDescriptor{Name="b", ParameterType = typeof(int)},
                        new ParameterDescriptor{Name="c", ParameterType = typeof(int)},
                    }
                },
                new ControllerActionDescriptor
                {
                    ControllerName = "Controller",
                    ControllerTypeInfo = typeof(AController).GetTypeInfo(),
                    MethodInfo = typeof(AController).GetMethod("A2", new[]{typeof(int),typeof(string)}),
                    DisplayName = "A2", 
                    Parameters = new[]
                    {
                        new ParameterDescriptor {Name="a", ParameterType = typeof(int)},
                        new ParameterDescriptor {Name="b", ParameterType = typeof(string)},
                    }
                }
            };
            var selector = CreateFinerGrainedActionSelector(actions,loggerFactory);

            // Act

            selector
                .SelectBestCandidate(routeContext, actions)
                .ShouldNotBeNull()
                .ShouldBe(actions[1]);
        }
        [Fact]
        public void ChoosesActionWith2MatchingParametersOverActionWith1MatchingParameter()
        {
            // Arrange
            var loglines= new List<string>();
            var loggerFactory = new LoggerFactory().AddStringListLogger(loglines);

            var incomingValues= new RouteValueDictionary(new{a=1, b=2});
            var routeContext = CreateRouteContext("POST", incomingValues);
            var actions = new[]
            {
                new ControllerActionDescriptor
                { 
                    ControllerName = "Controller",
                    ControllerTypeInfo = typeof(AController).GetTypeInfo(),
                    MethodInfo = typeof(AController).GetMethod("A1", new[]{typeof(int)}),
                    DisplayName = "A1", 
                    Parameters = new []
                    {
                        new ParameterDescriptor{Name="a", ParameterType = typeof(int)}
                    }
                },
                new ControllerActionDescriptor
                {
                    ControllerName = "Controller",
                    ControllerTypeInfo = typeof(AController).GetTypeInfo(),
                    MethodInfo = typeof(AController).GetMethod("A2", new[]{typeof(int),typeof(float)}),
                    DisplayName = "A2", 
                    Parameters = new []
                    {
                        new ParameterDescriptor {Name="a", ParameterType = typeof(int)},
                        new ParameterDescriptor {Name="b", ParameterType = typeof(float)},
                    }
                },
                new ControllerActionDescriptor
                {
                    ControllerName = "Controller",
                    ControllerTypeInfo = typeof(AController).GetTypeInfo(),
                    MethodInfo = typeof(AController).GetMethod("A3", new[]{typeof(int),typeof(int)}),
                    DisplayName = "A3", 
                    Parameters = new []
                    {
                        new ParameterDescriptor {Name="a", ParameterType = typeof(int)},
                        new ParameterDescriptor {Name="b", ParameterType = typeof(int)},
                    }
                }
            };

            // Act
            actions
                .OrderByDescending(a => ScoreByParameterNameAndConvertibility.Score(incomingValues, routeContext, a))
                .Select(a=>a.DisplayName)               
                .ShouldEqualByValue(new[] {"A3", "A2", "A1"});
        }

        public class AController
        {
            public IActionResult A1(int a) => new ObjectResult(a);
            public IActionResult A1(int a, int b, int c) => new ObjectResult(new {a,b,c});
            public IActionResult A1(int a, int b, string c) => new ObjectResult(new {a,b,c});

            public IActionResult A2(int a, int b) => new ObjectResult(new {a,b});
            public IActionResult A2(int a, string b) => new ObjectResult(new {a,b});
            public IActionResult A2(int a, float b) => new ObjectResult(new {a,b});

            public IActionResult A3(int a, int b) => new ObjectResult(new {a,b});

        }
    }
}
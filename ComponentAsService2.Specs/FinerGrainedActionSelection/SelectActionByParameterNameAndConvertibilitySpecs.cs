using System;
using System.Collections.Generic;
using ComponentAsService2.Specs.FinerGrainedActionSelection.Tests.Microsoft.AspNetCore.Mvc.Infrastructure;
using ComponentAsService2.UseComponentAsService;
using Extensions.Logging.ListOfString;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.Logging;
using TestBase;
using Xunit;

namespace ComponentAsService2.Specs.FinerGrainedActionSelection
{
    public class SelectActionByParameterNameAndConvertibilitySpecs : BaseForCreatingActionDescriptors
    {
        [NotYetATheory("Need access to actual values")]
        [InlineData(typeof(bool)  ,typeof(bool) ,  0 - 3 + 0 - 2 )]
        [InlineData(typeof(string),typeof(string), 2 - 3 + 0 - 2 )]
        [InlineData(typeof(int)   ,typeof(int) ,   1 - 3 + 1 - 2)]
        [InlineData(typeof(float) ,typeof(float) , 2 - 3 + 4 - 2 )]
        public void ScoreGivenRouteValuesA1point0B1_IsAsPerAlgorithm(Type typeA, Type typeB, int expectedScore)
        {
            var action = new ActionDescriptor
            {
                Parameters = new[]
                {
                    new ParameterDescriptor {Name = "a", ParameterType = typeA},
                    new ParameterDescriptor {Name = "b", ParameterType = typeB},
                }
            };

            SelectActionByParameterNameAndConvertibility
                .Score(action)
                .ShouldBe( expectedScore );
        }
        [NotYetATheory("Need access to actual values")]
        [InlineData("a", typeof(bool) ,  0 - 3 + 0 - 1 )]
        [InlineData("a", typeof(float) , 1 - 3 + 2 - 1 )]
        [InlineData("a", typeof(string), 1 - 3 + 0 - 1 )]
        [InlineData("a", typeof(int) ,   1 - 3 + 1 - 1 )]
        public void ScoreGivenRouteValuesA1BBCC_IsAsPerAlgorithm(string name, Type type, int expectedScore)
        {
            var action = new ActionDescriptor
            {
                Parameters = new[]{new ParameterDescriptor {Name = name, ParameterType = type}}
            };

            SelectActionByParameterNameAndConvertibility
                .Score(action)
                .ShouldBe( expectedScore );
        }

        [Fact]
        public void DoesNotCountUnconvertibleParametersAsAMatch()
        {
            // Arrange
            var loglines= new List<string>();
            var loggerFactory = new LoggerFactory().AddStringListLogger(loglines);
            var routeContext = CreateRouteContext("POST");

            var actions = new[]
            {
                new ActionDescriptor
                { 
                    DisplayName = "A1", 
                    Parameters = new[]
                    {
                        new ParameterDescriptor{Name="a", ParameterType = typeof(int)},
                        new ParameterDescriptor{Name="b", ParameterType = typeof(int)},
                        new ParameterDescriptor{Name="c", ParameterType = typeof(int)},
                    }
                },
                new ActionDescriptor
                {
                    DisplayName = "A2", 
                    RouteValues = {{"a","1"},{"b","b"}},
                    Parameters = new[]
                    {
                        new ParameterDescriptor {Name="a", ParameterType = typeof(int)},
                        new ParameterDescriptor {Name="b", ParameterType = typeof(string)},
                    }
                }
            };
            var selector = CreateSelector(actions,loggerFactory);

            // Act
            selector.SelectActionStrategy
                = SelectActionByParameterNameAndConvertibility.Apply;

            selector
                .SelectBestCandidate(routeContext, actions)
                .ShouldNotBeNull()
                .ShouldBe(actions[1]);
        }
        [NotYetAFact()]
        public void ChoosesActionWith2MatchingParametersOverActionWith1MatchingParameter()
        {
            // Arrange
            var loglines= new List<string>();
            var loggerFactory = new LoggerFactory().AddStringListLogger(loglines);
            var routeContext = CreateRouteContext("POST");

            var actions = new[]
            {
                new ActionDescriptor
                { 
                    DisplayName = "A1", 
                    Parameters = new []
                    {
                        new ParameterDescriptor{Name="a", ParameterType = typeof(int)}
                    }
                },
                new ActionDescriptor
                {
                    DisplayName = "A2", 
                    Parameters = new []
                    {
                        new ParameterDescriptor {Name="a", ParameterType = typeof(int)},
                        new ParameterDescriptor {Name="b", ParameterType = typeof(int)},
                    }
                }
            };
            var selector = CreateSelector(actions,loggerFactory);

            // Act
            selector.SelectActionStrategy = SelectActionByParameterNameAndConvertibility.Apply;

            selector
                .SelectBestCandidate(routeContext, actions)
                .ShouldNotBeNull()
                .ShouldBe(actions[1]);
        }
    }
}
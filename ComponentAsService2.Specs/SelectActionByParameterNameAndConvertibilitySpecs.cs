using System;
using System.Collections.Generic;
using ComponentAsService2.UseComponentAsService;
using Extensions.Logging.ListOfString;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.Logging;
using TestBase;
using Xunit;

namespace ComponentAsService2.Specs
{
    public class SelectActionByParameterNameAndConvertibilitySpecs : BaseForCreatingActionDescriptors
    {
        [Theory]
        [InlineData(typeof(bool)  ,typeof(bool) ,  5 - 2*0 )]
        [InlineData(typeof(float) ,typeof(float) , 5 - 2*2 )]
        [InlineData(typeof(string),typeof(string) , 5 - 2*2 )]
        [InlineData(typeof(int)   ,typeof(int) ,   5 - 2*1 )]
        public void 
            ScoreGivenRouteValuesA1point0B1_IsRvCountPlusAPCountMinusTwoTimesMismatches(Type typeA, Type typeB, int expectedMismatches)
        {
            var routeValues = new Dictionary<string,string>{{"a", "1.0"}, {"b", "1"}, {"c", "c"}};
            var action = new ActionDescriptor
            {
                RouteValues = routeValues,
                Parameters = new[]
                {
                    new ParameterDescriptor {Name = "a", ParameterType = typeA},
                    new ParameterDescriptor {Name = "b", ParameterType = typeB},
                }
            };

            SelectActionByParameterNameAndConvertibility
                .Score(action)
                .ShouldBe( - expectedMismatches );
        }
        [Theory]
        [InlineData("a", typeof(bool) ,  4 - 2*0 )]
        [InlineData("a", typeof(float) , 4 - 2*1 )]
        [InlineData("a", typeof(string), 4 - 2*1 )]
        [InlineData("a", typeof(int) ,   4 - 2*1 )]
        public void ScoreGivenRouteValuesA1BBCC_IsRvCountPlusAPCountMinusTwoTimesMismatches(string name, Type type, int expectedMismatches)
        {
            var routeValues = new Dictionary<string,string>{{"a", "1"}, {"b", "b"}, {"c", "c"}};
            var action = new ActionDescriptor
            {
                RouteValues = routeValues,
                Parameters = new[]{new ParameterDescriptor {Name = name, ParameterType = type}}
            };

            SelectActionByParameterNameAndConvertibility
                .Score(action)
                .ShouldBe( - expectedMismatches );
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
                    RouteValues = {{"a","1"},{"b","b"},{"c","c"}},
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
        [Fact]
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
                    RouteValues = {{"a","1"},{"b","2"}},
                    Parameters = new []
                    {
                        new ParameterDescriptor{Name="a", ParameterType = typeof(int)}
                    }
                },
                new ActionDescriptor
                {
                    DisplayName = "A2", 
                    RouteValues = {{"a","1"},{"b","2"}},
                    Parameters = new []
                    {
                        new ParameterDescriptor {Name="a", ParameterType = typeof(int)},
                        new ParameterDescriptor {Name="b", ParameterType = typeof(int)},
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
    }
}
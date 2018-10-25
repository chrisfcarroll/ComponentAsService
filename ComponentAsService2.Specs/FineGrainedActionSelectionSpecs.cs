using System.Linq;
using Extensions.Logging.ListOfString;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.Logging;
using TestBase;
using Xunit;

namespace ComponentAsService2.Specs
{
    public class FinerGrainedActionSelectorSpecs : BaseForCreatingActionDescriptors
    {
        [Fact]
        public void SelectBestOneOfSeveralActionsStrategy_CanBeOverridden()
        {
            // Arrange
            StringListLogger logger=new StringListLogger();
            var loggerFactory = new LoggerFactory().AddStringListLogger(logger);
            var actions = new[]
            {
                new ActionDescriptor { DisplayName = "A1" },
                new ActionDescriptor { DisplayName = "A2" }
            };
            var routeContext = CreateRouteContext("POST");
            var selector = CreateSelector(actions,loggerFactory);

            // Act
            selector.SelectActionStrategy = (l, a) =>
            {
                l.Log(LogLevel.Information, "Here!");
                return a.FirstOrDefault();
            };

            selector
                .SelectBestCandidate(routeContext, actions)
                .ShouldNotBeNull()
                .ShouldBeOneOf(actions);

            logger.LoggedLines.SingleOrAssertFail("Should have logged 1 line").ShouldContain("Here!");
        }


        [Fact]
        public void SelectBestCandidate_DefaultImplementationChoosesOne_GivenAmbiguousActions()
        {
            // Arrange
            var actions = new[]
            {
                new ActionDescriptor { DisplayName = "A1" },
                new ActionDescriptor { DisplayName = "A2" }
            };
            var selector = CreateSelector(actions);
            var routeContext = CreateRouteContext("POST");

            // Act
            selector
                .SelectBestCandidate(routeContext, actions)
                .ShouldNotBeNull()
                .ShouldBeOneOf(actions);
        }
        
        [Fact]
        public void SelectBestCandidate_DefaultImplementationChoosesOne_GivenAmbiguousActionsB()
        {
            // Arrange
            var actions = new[]
            {
                CreateAction(area: null, controller: "Store", action: "Buy"),
                CreateAction(area: null, controller: "Store", action: "Buy")
            };
            actions[0].DisplayName = "Ambiguous1";
            actions[1].DisplayName = "Ambiguous2";
            var selector = CreateSelector(actions);
            var context = CreateRouteContext("GET");
            context.RouteData.Values.Add("controller", "Store");
            context.RouteData.Values.Add("action", "Buy");

            // Act
            selector
                .SelectBestCandidate(context, actions)
                .ShouldNotBeNull()
                .ShouldBeOneOf(actions);
        }
    }
}

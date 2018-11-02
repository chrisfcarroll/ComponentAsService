using Microsoft.AspNetCore.Mvc.Abstractions;
using TestBase;
using Xunit;

namespace Component.As.Service.Specs.FinerGrainedActionSelection
{
    public class FinerGrainedActionSelectorSpecs : BaseForCreatingActionDescriptors
    {
        [Fact]
        public void SelectBestCandidate_DefaultImplementationChoosesOne_GivenAmbiguousActions()
        {
            // Arrange
            var actions = new[]
            {
                new ActionDescriptor { DisplayName = "A1" },
                new ActionDescriptor { DisplayName = "A2" }
            };
            var selector = CreateFinerGrainedActionSelector(actions);
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
            var selector = CreateFinerGrainedActionSelector(actions);
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

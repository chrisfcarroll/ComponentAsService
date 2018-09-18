using Microsoft.AspNetCore.Mvc;

namespace ComponentAsService
{
    public class SpecialNames
    {
        public static readonly SpecialNames DefaultValues = new SpecialNames();
        
        /// <summary>Effect: This is the controller name by which Asp.Net Routing can find a <see cref="ComponentAsServiceController"/>
        /// Change this if you want to subclass or replace <see cref="ComponentAsServiceController"/></summary>
        public string ComponentAsServiceControllerName { get; private set; } = "ComponentAsService";
        
        /// <summary>Effect: This is the action name by which Asp.Net Routing can find <see cref="ComponentAsServiceController.Serve"/>
        /// Change this if you want to replace <see cref="ComponentAsServiceController"/></summary>
        public string ComponentAsServiceControllerActionName { get; private set; } = "Serve";

        /// <summary>Effect: This is the identifier used in ComponentAsService's route constraint to identify the service to invoke</summary>
        public string RouteValueServiceName { get; private set; } = "service";
        
        /// <summary>Effect: This is the identifier used in ComponentAsService's route constraint to identify the method to invoke on the service being used</summary>
        public string RouteValueMethodName { get; private set; } = "method";
        
        /// <summary>Effect: if you invoke a method which has a single parameter of this name, then ComponentAsService will populated it
        /// with all Values in <see cref="ActionContext.RouteData"/>.Values</summary>
        public string SpecialParameterNameForAllMvcRouteValues { get; private set; } = "allMvcRouteValues";
        
        public SpecialNames(string specialParameterNameForAllMvcRouteValues="allMvcRouteValues"){}

        protected bool Equals(SpecialNames other)
        {
            return string.Equals(ComponentAsServiceControllerName, other.ComponentAsServiceControllerName)
                && string.Equals(ComponentAsServiceControllerActionName, other.ComponentAsServiceControllerActionName)
                && string.Equals(RouteValueServiceName, other.RouteValueServiceName)
                && string.Equals(RouteValueMethodName, other.RouteValueMethodName)
                && string.Equals(SpecialParameterNameForAllMvcRouteValues, other.SpecialParameterNameForAllMvcRouteValues);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SpecialNames) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ComponentAsServiceControllerName != null ? ComponentAsServiceControllerName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ComponentAsServiceControllerActionName                        != null ? ComponentAsServiceControllerActionName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RouteValueServiceName                    != null ? RouteValueServiceName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RouteValueMethodName                     != null ? RouteValueMethodName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SpecialParameterNameForAllMvcRouteValues != null ? SpecialParameterNameForAllMvcRouteValues.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(SpecialNames left, SpecialNames right) { return Equals(left, right); }
        public static bool operator !=(SpecialNames left, SpecialNames right) { return !Equals(left, right); }
    }
}
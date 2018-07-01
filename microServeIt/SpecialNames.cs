using microServeIt.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace microServeIt
{
    public class SpecialNames
    {
        public static readonly SpecialNames DefaultValues = new SpecialNames();
        
        /// <summary>Effect: This is the controller name by which Asp.Net Routing can find a <see cref="ServeItController"/>
        /// Change this if you want to subclass or replace <see cref="ServeItController"/></summary>
        public string ServeItControllerName { get; private set; } = "ServeIt";
        
        /// <summary>Effect: This is the action name by which Asp.Net Routing can find <see cref="ServeItController.Serve"/>
        /// Change this if you want to replace <see cref="ServeItController"/></summary>
        public string ServeItActionName { get; private set; } = "Serve";

        /// <summary>Effect: This is the identifier used in ServeIt's route constraint to identify the service to invoke</summary>
        public string RouteValueServiceName { get; private set; } = "service";
        
        /// <summary>Effect: This is the identifier used in ServeIt's route constraint to identify the method to invoke on the service being used</summary>
        public string RouteValueMethodName { get; private set; } = "method";
        
        /// <summary>Effect: if you invoke a method which has a single parameter of this name, then ServeIt will populated it
        /// with all Values in <see cref="ActionContext.RouteData"/>.Values</summary>
        public string SpecialParameterNameForAllMvcRouteValues { get; private set; } = "allMvcRouteValues";
        
        public SpecialNames(string specialParameterNameForAllMvcRouteValues="allMvcRouteValues"){}

        protected bool Equals(SpecialNames other)
        {
            return string.Equals(ServeItControllerName, other.ServeItControllerName)
                && string.Equals(ServeItActionName, other.ServeItActionName)
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
                var hashCode = (ServeItControllerName != null ? ServeItControllerName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ServeItActionName                        != null ? ServeItActionName.GetHashCode() : 0);
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace microServeIt
{
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Find methods on <paramref name="type"/> which match <paramref name="methodName"/>
        /// and for which we can find parameters in <paramref name="args"/> which match on name and instance assignability.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<MethodInfo> 
            GetMethodsByNameAndParameterAssignability(this Type type, string methodName, Dictionary<string, object> args)
        {
            var methods = type
                         .GetMethods()
                         .Where(
                                m => m.Name == methodName
                                  && m.GetParameters()
                                      .All(
                                           p => args.ContainsKey(p.Name)
                                             && p.ParameterType.IsInstanceOfTypeEvenIfNull(args[p.Name])))
                         .OrderByDescending(m => m.GetParameters().Count());
            return methods;
        }

        public static IEnumerable<MethodInfo> GetMethodsWithOneDictionaryParameter(this Type serviceType, string methodName, Dictionary<string, object> args)
        {
            var methods = serviceType
                         .GetMethods()
                         .Where(
                                m => m.Name == methodName
                                  && m.GetParameters().Length ==1
                                  && m.GetParameters()[0].ParameterType.IsInstanceOfType(args));
            return methods;
        }

        public static IEnumerable<MethodInfo> 
            GetMethodsWithOneObjectArrayParameter(this Type serviceType, string methodName, object[] args)
        {
            var methods = serviceType
                         .GetMethods()
                         .Where(
                                m => m.Name == methodName
                                  && m.GetParameters().Length ==1
                                  && m.GetParameters()[0].ParameterType.IsInstanceOfType(args));
            return methods;
        }

        /// <returns>true if <code>type.IsInstanceOfType(instance) || (!type.IsValueType && instance == null)</code></returns>
        public static bool IsInstanceOfTypeEvenIfNull(this Type type, object instance)
        {
            return type.IsInstanceOfType(instance) || (!type.IsValueType && instance == null);
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;

namespace ComponentAsService2.UseComponentAsService
{
    public static class ScoreByParameterNameAndConvertibility
    {
        public static int Score(IDictionary<string, object> actualValues, RouteContext rContext, ActionDescriptor action)
        {

            var expectedParameters = action.Parameters?.Select(p => new {p.Name, p.ParameterType}).ToArray();
            
            var convertibleMatches =
                (expectedParameters == null
                    ? new []{ new {Key="",ParameterType=typeof(string),Value=null as object} }
                    : actualValues
                        .Join(expectedParameters,
                            kv => kv.Key,
                            nt => nt.Name,
                            (a, e) => new
                            {
                                a.Key,
                                e.ParameterType,
                                Value = TryConvert(e.ParameterType, a.Value),
                            })
                ).Where(x => x.Value != null).ToArray();

            var score = convertibleMatches.Count() - actualValues.Count
                        + convertibleMatches.Sum(m=>TypePreferenceScore(m.ParameterType)) 
                        - (expectedParameters?.Length??0);
            
            return score;
        }

        static int TypePreferenceScore(Type type)
        {
            if (type == typeof(string))
                return 0;
            else if (type.IsPrimitive )
                return PrimitiveTypePreferences.ContainsKey(type) ? PrimitiveTypePreferences[type] : 1;
            else
            {
                return 5 + type.GetProperties().Length;
            }
        }

        static object TryConvert(Type toType, object routeValue)
        {
            try
            {
                return TypeDescriptor.GetConverter(toType).ConvertFrom(routeValue);
            }
            catch{ return null; }
        }

        static readonly Dictionary<Type,int> PrimitiveTypePreferences=new Dictionary<Type, int>
        {
            {typeof(float),2},
            {typeof(long),2},
            {typeof(double),3},
            {typeof(decimal),4},
        };

    }
}
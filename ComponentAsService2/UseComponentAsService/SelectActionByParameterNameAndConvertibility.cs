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

            var score = 10*(convertibleMatches.Count() 
                            - actualValues.Count
                            - (expectedParameters?.Length??0)
                            )
                        + convertibleMatches.Sum(m=>TypePreferenceScore(m.ParameterType)) ;
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
                var convertor = TypeDescriptor.GetConverter(toType);
                if (routeValue is string)
                {
                    return convertor.ConvertFromString(routeValue as string);
                }
                else if(convertor.CanConvertFrom(routeValue.GetType()))
                {
                    return convertor.ConvertFrom(routeValue);
                }
                else
                {
                    return convertor.ConvertFromString(routeValue.ToString());
                }
            }
            catch (Exception e){return null;}
        }

        static readonly Dictionary<Type,int> PrimitiveTypePreferences=new Dictionary<Type, int>
        {
            {typeof(double),2},
            {typeof(float),3},
            {typeof(decimal),4},
            {typeof(long),5},
            {typeof(int), 6},
        };
    }
}
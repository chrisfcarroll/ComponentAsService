using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore.Internal;

namespace ComponentAsService2.UseComponentAsService
{
    public static class ScoreByParameterNameAndConvertibility
    {
        public static int Score(IDictionary<string, object> actualValues, RouteContext rContext, ActionDescriptor action)
        {

            var expectedParameters = action.Parameters?.Select(p => new {p.Name, p.ParameterType}).ToArray();
            
            var convertibleMatches =
                (expectedParameters == null
                    ? new []{ new {Key="",ParameterType=typeof(string),Value=0} }
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
                ).ToArray();

            var score = convertibleMatches.Sum(m=>m.Value)
                        - 1000 * Math.Max(actualValues.Count, (expectedParameters?.Length??0))
                        + convertibleMatches.Sum(m=>TypePreferenceScore(m.ParameterType)) ;
            return score;
        }

        static int TypePreferenceScore(Type type)
        {
            if (type == typeof(string))
                return 1;
            else if (type.IsEnum)
                return 10;
            else if (type.IsPrimitive )
                return PrimitiveTypePreferences.ContainsKey(type) ? PrimitiveTypePreferences[type] : 1;
            else
            {
                return 10;
            }
        }

        static int TryConvert(Type toType, object routeValue)
        {
            try
            {
                var convertor = TypeDescriptor.GetConverter(toType);
                if (routeValue.GetType() == toType)
                {
                    return 1000;
                }
                if (routeValue is string s)
                {
                    return convertor.ConvertFromString(s) != null ? 900 : 0;
                }
                else if(convertor.CanConvertFrom(routeValue.GetType()))
                {
                    return convertor.ConvertFrom(routeValue) != null ? 900 : 0;
                }
                else
                {
                    return convertor.ConvertFromString(routeValue.ToString()) != null ? 900 : 0;
                }
            }
            catch (Exception e){return 0;}
        }

        static readonly Dictionary<Type,int> PrimitiveTypePreferences=new Dictionary<Type, int>
        {
            {typeof(double),2},
            {typeof(float),3},
            {typeof(decimal),4},
            {typeof(long),4},
            {typeof(int), 5},
            {typeof(bool), 5}
        };
    }
}
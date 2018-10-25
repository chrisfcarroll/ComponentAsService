using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.Logging;

namespace ComponentAsService2.UseComponentAsService
{
    /// <summary>Provides <see cref="Apply"/> as a Delegate suitable for use by
    /// <see cref="FinerGrainedActionSelector.SelectActionStrategy"/>
    /// </summary>
    public static class SelectActionByParameterNameAndConvertibility
    {
        /// <summary>Selects a best match Action by scoring each candidate action on 
        /// (1) How many RouteValues match the Actions' parameters by name and Type-convertibility
        /// (2) A preference for "bigger" or more complex types, so that float is preferred int,
        /// user defined types are preferred to primitive types, and types with more properties
        /// are preferred to types with fewer properties</summary>
        public static ActionDescriptor Apply(ILogger logger, IReadOnlyList<ActionDescriptor> actions)
        {
            logger.LogDebug(actions.ToJson());
            return actions.OrderByDescending(Score).First();
        }

        public static int Score(ActionDescriptor action)
        {
            //TODO: how to get hold of action values.
            var actualValues = new Dictionary<string, string>();

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

        static object TryConvert(Type toType, string fromString)
        {
            try
            {
                return TypeDescriptor.GetConverter(toType).ConvertFromString(fromString);
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
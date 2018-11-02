using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace Component.As.Service.Pieces
{
    /// <summary>
    /// Score an attempted binding of parameter values to the parameters of possible method calls,
    /// in order to pick a best match.
    ///
    /// When using this to score RouteValues, ModelBinding must be done first to produce the <c>actualValues</c>
    /// parameter to pass to <see cref="Score"/>.
    /// </summary>
    public static class ScoreByParameterNameAndConvertibility
    {
        /// <summary>
        /// Score how well we can map the 
        /// <paramref name="actualValues"/> to the parameters of <see cref="action"/>.
        /// </summary>
        /// <param name="actualValues"></param>
        /// <param name="action"></param>
        /// <returns>
        /// A integer score. Higher than zero suggests the action can accept these parameters.
        /// Lower than zero means either missing parameters or excess parameters.
        /// 
        /// The score goes up by around 1000 for each item in <paramref name="actualValues"/> that matches
        /// a parameter of <paramref name="action"/> by exact name, and for which the value is, or is
        /// convertible to, the required <see cref="Type"/> of the <see cref="ActionDescriptor.Parameters"/>.
        ///
        /// The score goes up by a small value of <see cref="PrimitiveTypePreferences"/> for each
        /// primitive type match, so that, for instance, a method having an <see cref="Int32"/> parameter is considered
        /// a better match for an <see cref="Int32"/> <c>actualValue</c>, over a method having a <see cref="Single"/>parameter,
        /// even though the <see cref="Int32"/> <c>actualValue</c> is perfectly good for a method with a <see cref="Single"/>
        /// parameter.
        ///
        /// The score goes down by 1000 for each 'spare' <paramref name="actualValues"/> or
        /// <see cref="ActionDescriptor.Parameters"/> that has no match by name.
        /// </returns>
        public static int Score(IDictionary<string, object> actualValues, ActionDescriptor action)
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

            var matchScore = convertibleMatches.Sum(m=>m.Value + TypePreferenceScore(m.ParameterType)) ;
            var mismatchScore = 
                1000 * (Math.Max(actualValues.Count, (expectedParameters?.Length ?? 0)) - convertibleMatches.Length);
            return matchScore - mismatchScore;
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
                if (routeValue.GetType() == toType || toType==typeof(string) )
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
            catch (Exception){return 0;}
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
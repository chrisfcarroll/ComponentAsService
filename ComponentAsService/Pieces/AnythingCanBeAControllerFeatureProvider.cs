﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;

namespace Component.As.Service.Pieces
{
    public class AnythingCanBeAControllerFeatureProvider : ControllerFeatureProvider, ICollection<TypeInfo>
    {
        /// <summary>Determines if a given <paramref name="typeInfo" /> is a controller.</summary>
        /// <param name="typeInfo">The <see cref="T:System.Reflection.TypeInfo" /> candidate.</param>
        /// <returns><code>true</code> if the type is in <see cref="moreControllerTypes"/>
        /// or if <see cref="ControllerFeatureProvider.IsController"/> says so;
        /// otherwise <code>false</code>.</returns>
        protected override bool IsController(TypeInfo typeInfo)
        {
            var isMine = typeInfo.IsIn(moreControllerTypes);
            var isBases = base.IsController(typeInfo);
            if(moreControllerTypes.Length==0){log.LogWarning("AnythingCanBeAControllerFeatureProvider.IsController was called when moreControllerTypes is Empty.");}
            if(isMine)log.LogInformation("AnythingCanBeAController added {controller}",typeInfo.FullName);
            if(isBases)log.LogInformation("BaseControllerFeatureProvider added {controller}",typeInfo.FullName);
            return isMine || isBases;
        }

        public AnythingCanBeAControllerFeatureProvider(ILogger<AnythingCanBeAControllerFeatureProvider> log, params TypeInfo[] moreControllerTypes)
        {
            this.log = log;
            this.moreControllerTypes = moreControllerTypes??new TypeInfo[0];
        }

        readonly ILogger<AnythingCanBeAControllerFeatureProvider> log;
        TypeInfo[] moreControllerTypes;


        /// <summary>
        /// Add <paramref name="typeInfos"/> to the list of types to be served as controllers.
        /// </summary>
        /// <param name="typeInfos"></param>
        public void Add(IEnumerable<TypeInfo> typeInfos) => moreControllerTypes = moreControllerTypes.Union( typeInfos ).ToArray();

        /// <inheritdoc />
        /// <summary>
        /// Add <paramref name="type" /> to the list of types to be served as controllers.
        /// </summary>
        /// <param name="type"></param>
        public void Add(TypeInfo type) => moreControllerTypes = moreControllerTypes.Union(new[] {type}).ToArray();

        public void Clear() => moreControllerTypes = new TypeInfo[0];

        public bool Contains(TypeInfo item) => moreControllerTypes.Contains(item);

        public void CopyTo(TypeInfo[] array, int arrayIndex) => moreControllerTypes.CopyTo(array, arrayIndex);

        public bool Remove(TypeInfo item)
        {
            moreControllerTypes = moreControllerTypes.Where(t => t != item).ToArray();
            return true;
        }

        public int Count => moreControllerTypes.Length;

        public bool IsReadOnly => moreControllerTypes.IsReadOnly;
        
        public IEnumerator<TypeInfo> GetEnumerator() => moreControllerTypes.GetEnumerator() as IEnumerator<TypeInfo>;
        
        IEnumerator IEnumerable.GetEnumerator() => moreControllerTypes.GetEnumerator();
    }
}
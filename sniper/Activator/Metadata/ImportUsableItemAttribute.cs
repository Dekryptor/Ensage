// <copyright file="ImportUsableItemAttribute.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper.Activator.Metadata
{
    using System;
    using System.ComponentModel.Composition;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class ImportUsableItemAttribute : ImportAttribute
    {
        public ImportUsableItemAttribute()
            : base(typeof(IUsableItem))
        {
        }
    }
}
// <copyright file="ExportUsableItemAttribute.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper.Activator.Metadata
{
    using System;
    using System.ComponentModel.Composition;

    using Ensage.Common.Enums;

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public class ExportUsableItemAttribute : ExportAttribute, IUsableItemMetadata
    {
        public ExportUsableItemAttribute(ItemId id)
            : base(typeof(IUsableItem))
        {
            this.Id = id;
        }

        public ItemId Id { get; }
    }
}
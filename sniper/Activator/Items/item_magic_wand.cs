// <copyright file="item_magic_wand.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper.Activator.Items
{
    using System.ComponentModel.Composition;

    using Ensage.Common.Enums;
    using Ensage.SDK.Service;

    using Sniper.Activator.Metadata;

    [ExportUsableItem(ItemId.item_magic_wand)]
    public class item_magic_wand : UsableItem
    {
        [ImportingConstructor]
        public item_magic_wand([Import] IServiceContext context, [Import] ActivatorConfig.ItemsConfig parent)
            : base(context, ItemId.item_magic_wand)
        {
        }
    }
}
// <copyright file="item_dagon_5.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper.Activator.Items
{
    using System.ComponentModel.Composition;
    using System.Linq;

    using Ensage.Common.Enums;
    using Ensage.SDK.Service;

    using Sniper.Activator.Metadata;

    [ExportUsableItem(ItemId.item_dagon_5)]
    public class item_dagon_5 : item_dagon
    {
        [ImportingConstructor]
        public item_dagon_5([Import] IServiceContext context, [Import] ActivatorConfig config)
            : base(context, config, ItemId.item_dagon_5)
        {
        }

        protected override float GetRawDamage()
        {
            return this.Item.AbilitySpecialData.First(x => x.Name == "damage").GetValue(4);
        }
    }
}
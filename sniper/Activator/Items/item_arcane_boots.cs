// <copyright file="item_arcane_boots.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper.Activator.Items
{
    using System.ComponentModel.Composition;

    using Ensage.Common.Enums;
    using Ensage.SDK.Menu;
    using Ensage.SDK.Service;

    using Sniper.Activator.Metadata;

    [ExportUsableItem(ItemId.item_arcane_boots)]
    public class item_arcane_boots : UsableItem
    {
        [ImportingConstructor]
        public item_arcane_boots([Import] IServiceContext context, [Import] ActivatorConfig config)
            : base(context, ItemId.item_arcane_boots)
        {
            this.UseBoots = config.Items.Factory.Item("Arcane Boots", true);
        }

        public MenuItem<bool> UseBoots { get; }

        protected override bool CanUse()
        {
            return this.UseBoots.Value && base.CanUse() && (this.Owner.Mana + 135) < this.Owner.MaximumMana;
        }
    }
}
// <copyright file="item_phase_boots.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper.Activator.Items
{
    using System.ComponentModel.Composition;

    using Ensage.Common.Enums;
    using Ensage.SDK.Menu;
    using Ensage.SDK.Service;

    using Sniper.Activator.Metadata;

    [ExportUsableItem(ItemId.item_phase_boots)]
    public class item_phase_boots : UsableItem
    {
        [ImportingConstructor]
        public item_phase_boots([Import] IServiceContext context, [Import] ActivatorConfig config)
            : base(context, ItemId.item_phase_boots)
        {
            this.UseBoots = config.Items.Factory.Item("Phase Boots", true);
        }

        public MenuItem<bool> UseBoots { get; }

        protected override bool CanUse()
        {
            return this.UseBoots.Value && base.CanUse();
        }
    }
}
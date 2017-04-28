// <copyright file="UsableItem.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper.Activator
{
    using System.Linq;
    using System.Reflection;

    using Ensage;
    using Ensage.Common.Enums;
    using Ensage.Common.Extensions;
    using Ensage.SDK.Helpers;
    using Ensage.SDK.Service;

    using log4net;

    using PlaySharp.Toolkit.Logging;

    public abstract class UsableItem : IUsableItem
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private Item item;

        protected UsableItem(IServiceContext context, ItemId itemId)
        {
            this.Id = itemId;
            this.Owner = context.Owner;

            Log.Debug($"{this.Owner.HeroId}@{itemId} ({this.Item.AbilityBehavior})");
        }

        public ItemId Id { get; }

        public virtual bool IsImportant { get; } = false;

        public Item Item
        {
            get
            {
                if (this.item == null || !this.item.IsValid)
                {
                    this.item = this.Owner.GetItemById(this.Id);
                }

                return this.item;
            }
        }

        protected Hero Owner { get; set; }

        public void Use()
        {
            if (!this.CanUse())
            {
                return;
            }

            Log.Debug($"Item[{this.Id}]");
            this.UseItem();
        }

        protected virtual bool CanUse()
        {
            return !Game.IsPaused && this.Owner.IsAlive && this.Item.CanBeCasted();
        }

        protected virtual void UseItem()
        {
            if (this.Item.AbilityBehavior.HasFlag(AbilityBehavior.UnitTarget))
            {
                var target = EntityManager<Hero>.Entities.FirstOrDefault();

                if (!this.Item.CanBeCasted(target) || !this.Item.CanHit(target))
                {
                    return;
                }

                this.Item.UseAbility(target);
            }
            else if (this.Item.AbilityBehavior.HasFlag(AbilityBehavior.NoTarget))
            {
                if (!this.Item.CanBeCasted())
                {
                    return;
                }

                this.Item.UseAbility();
            }
        }
    }
}
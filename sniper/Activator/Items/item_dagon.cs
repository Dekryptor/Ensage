// <copyright file="item_dagon.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper.Activator.Items
{
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Reflection;

    using Ensage;
    using Ensage.Common.Enums;
    using Ensage.Common.Extensions;
    using Ensage.SDK.Extensions;
    using Ensage.SDK.Helpers;
    using Ensage.SDK.Menu;
    using Ensage.SDK.Renderer.Particle;
    using Ensage.SDK.Renderer.Particle.Metadata;
    using Ensage.SDK.Service;

    using log4net;

    using PlaySharp.Toolkit.Logging;

    using Sniper.Activator.Metadata;

    [ExportUsableItem(ItemId.item_dagon)]
    public class item_dagon : UsableItem
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private float rawDamage;

        [ImportingConstructor]
        public item_dagon([Import] IServiceContext context, [Import] ActivatorConfig config)
            : this(context, config, ItemId.item_dagon)
        {
        }

        public item_dagon(IServiceContext context, ActivatorConfig config, ItemId id = ItemId.item_dagon)
            : base(context, id)
        {
            this.UseDagon = config.Items.Factory.Item($"Dagon", true);
        }

        public override bool IsImportant => true;

        public float RawDamage
        {
            get
            {
                if (this.rawDamage == 0)
                {
                    this.rawDamage = this.GetRawDamage();
                }

                return this.rawDamage;
            }
        }

        public MenuItem<bool> UseDagon { get; }

        [ImportParticleManager]
        protected Lazy<IParticleManager> ParticleManager { get; set; }

        protected override bool CanUse()
        {
            return this.UseDagon.Value && base.CanUse() && this.HasTarget();
        }

        protected virtual float GetRawDamage()
        {
            return this.Item.AbilitySpecialData.First(x => x.Name == "damage").GetValue(0);
        }

        protected override void UseItem()
        {
            var target = this.GetTarget();

            if (target != null)
            {
                var dmg = this.Owner.CalculateSpellDamage(target, DamageType.Magical, this.RawDamage);
                Log.Debug($"{dmg}|{target.Health}");

                this.Item.UseAbility(target);
            }
        }

        private bool Filter(Hero target)
        {
            if (target.Team == this.Owner.Team)
            {
                return false;
            }

            if (target.IsIllusion || !target.IsAlive || target.Health == 0)
            {
                return false;
            }

            if (!this.Item.CanHit(target))
            {
                return false;
            }

            if (this.Owner.CalculateSpellDamage(target, DamageType.Magical, this.RawDamage) > target.Health)
            {
                return true;
            }

            return false;
        }

        private Hero GetTarget()
        {
            return EntityManager<Hero>.Entities.FirstOrDefault(this.Filter);
        }

        private bool HasTarget()
        {
            return this.GetTarget() != null;
        }
    }
}
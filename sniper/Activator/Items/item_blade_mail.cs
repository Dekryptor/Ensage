// <copyright file="item_blade_mail.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper.Activator.Items
{
    using System;
    using System.ComponentModel.Composition;
    using System.Reflection;

    using Ensage;
    using Ensage.Common.Enums;
    using Ensage.Common.Menu;
    using Ensage.SDK.Extensions;
    using Ensage.SDK.Menu;
    using Ensage.SDK.Service;

    using log4net;

    using PlaySharp.Toolkit.Logging;

    using Sniper.Activator.Metadata;

    [ExportUsableItem(ItemId.item_blade_mail)]
    public class item_blade_mail : UsableItem, IDisposable
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool disposed;

        [ImportingConstructor]
        public item_blade_mail([Import] IServiceContext context, [Import] ActivatorConfig config)
            : base(context, ItemId.item_blade_mail)
        {
            this.Factory = config.Items.Factory.Menu("Blade Mail");
            this.UseBladeMail = this.Factory.Item("Blade Mail", true);
            this.MinDamage = this.Factory.Item("Received Damage", new Slider(150, 1, 1000));
            this.MinHealth = this.Factory.Item("Low Health", new Slider(60, 1, 100));

            Entity.OnInt32PropertyChange += this.OnPropertyChange;
        }

        public MenuFactory Factory { get; }

        public MenuItem<Slider> MinDamage { get; }

        public MenuItem<Slider> MinHealth { get; }

        public MenuItem<bool> UseBladeMail { get; }

        private TimeoutTrigger Trigger { get; } = new TimeoutTrigger(TimeSpan.FromSeconds(2));

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override bool CanUse()
        {
            return this.UseBladeMail.Value && base.CanUse() && this.Trigger.IsTriggered;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                Entity.OnInt32PropertyChange -= this.OnPropertyChange;
            }

            this.disposed = true;
        }

        protected override void UseItem()
        {
            base.UseItem();
            this.Trigger.Deactivate();
        }

        private void OnPropertyChange(Entity sender, Int32PropertyChangeEventArgs args)
        {
            if (sender != this.Owner || !base.CanUse())
            {
                return;
            }

            if (args.PropertyName == "m_iHealth" && args.OldValue > args.NewValue)
            {
                var diff = args.OldValue - args.NewValue;
                if (diff > this.MinDamage.Value.Value)
                {
                    Log.Debug($"MinDamage trigger {diff}");
                    this.Trigger.Activate();
                }

                var perc = this.Owner.HealthPercent() * 100;
                if (perc < this.MinHealth.Value.Value)
                {
                    Log.Debug($"MinHealth trigger {perc}");
                    this.Trigger.Activate();
                }
            }
        }
    }
}
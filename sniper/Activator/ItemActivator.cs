// <copyright file="ItemActivator.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper.Activator
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Reflection;

    using Ensage;
    using Ensage.SDK.Helpers;
    using Ensage.SDK.Inventory;
    using Ensage.SDK.Inventory.Metadata;
    using Ensage.SDK.Service;
    using Ensage.SDK.Service.Metadata;

    using log4net;

    using PlaySharp.Toolkit.Logging;

    using Sniper.Activator.Items;
    using Sniper.Activator.Metadata;

    [ExportAssembly("Item Activator")]
    public class ItemActivator : IAssemblyLoader
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [ImportingConstructor]
        public ItemActivator([Import] IServiceContext context)
        {
            this.Owner = context.Owner;
            this.Config = new ActivatorConfig();
        }

        [Export]
        public ActivatorConfig Config { get; private set; }

        [ImportInventoryManager]
        protected Lazy<IInventoryManager> Inventory { get; set; }

        protected List<IUsableItem> Items { get; } = new List<IUsableItem>();

        protected Hero Owner { get; set; }

        [ImportMany(typeof(IUsableItem))]
        private IEnumerable<Lazy<IUsableItem, IUsableItemMetadata>> ItemPlugins { get; set; }

        public void Activate()
        {
            try
            {
                UpdateManager.Subscribe(this.ItemUse, 500);
                UpdateManager.Subscribe(this.ItemUseImportant);
                this.Inventory.Value.CollectionChanged += this.OnInventoryChanged;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public void Deactivate()
        {
            UpdateManager.Unsubscribe(this.ItemUse);
            UpdateManager.Unsubscribe(this.ItemUseImportant);
            this.Inventory.Value.CollectionChanged -= this.OnInventoryChanged;
        }

        private void ItemUse()
        {
            foreach (var item in this.Items.Where(i => !i.IsImportant))
            {
                item.Use();
            }
        }

        private void ItemUseImportant()
        {
            foreach (var item in this.Items.Where(i => i.IsImportant))
            {
                item.Use();
            }
        }

        private void OnInventoryChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            try
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var item in args.NewItems.Cast<InventoryItem>())
                        {
                            var newItem = this.ItemPlugins.FirstOrDefault(i => i.Metadata.Id == item.Id);
                            if (newItem?.Value != null)
                            {
                                Log.Debug($"Activate {item.Id}");
                                this.Items.Add(newItem.Value);
                            }
                        }

                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in args.OldItems.Cast<InventoryItem>())
                        {
                            var newItem = this.Items.FirstOrDefault(i => i.Id == item.Id);
                            if (newItem != null)
                            {
                                Log.Debug($"Deactivate {newItem.Id}");
                                this.Items.Remove(newItem);
                            }
                        }

                        break;
                }
            }
            catch (CompositionException e)
            {
                Log.Error(e);

                foreach (var error in e.Errors)
                {
                    Log.Error(error);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}
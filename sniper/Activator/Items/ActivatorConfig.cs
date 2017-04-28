// <copyright file="ActivatorConfig.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper.Activator.Items
{
    using Ensage.SDK.Menu;

    public class ActivatorConfig
    {
        public ActivatorConfig()
        {
            this.Factory = MenuFactory.Create("Activator");
            this.Items = new ItemsConfig(this.Factory);
        }

        public MenuFactory Factory { get; }

        public ItemsConfig Items { get; }

        public class ItemsConfig
        {
            public ItemsConfig(MenuFactory parent)
            {
                this.Factory = parent.Menu("Items");
            }

            public MenuFactory Factory { get; }
        }
    }
}
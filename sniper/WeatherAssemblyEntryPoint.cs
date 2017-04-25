// <copyright file="WeatherAssemblyEntryPoint.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper
{
    using System;
    using System.Linq;
    using System.Reflection;

    using Ensage;
    using Ensage.Common.Extensions;
    using Ensage.SDK.Extensions;
    using Ensage.SDK.Menu;
    using Ensage.SDK.Service;

    using log4net;

    using PlaySharp.Toolkit.Logging;

    [ExportAssembly("Dagon")]
    public class DagonEntryPoint : IAssemblyLoader
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public DagonEntryPoint()
        {
            this.Hero = ObjectManager.LocalHero;
            this.Factory = MenuFactory.Create("Dagon");
            this.Use = this.Factory.Item("Enable Dagon killsteal", false);
        }

        public MenuFactory Factory { get; }

        public Hero Hero { get; }

        public MenuItem<bool> Use { get; }

        public void Activate()
        {
            Game.OnIngameUpdate += this.OnUpdate;
        }

        public void Deactivate()
        {
            Game.OnIngameUpdate -= this.OnUpdate;
            this.Factory.Parent.RemoveFromMainMenu();
        }

        public Item GetDagon()
        {
            return this.Hero.Inventory.Items.FirstOrDefault(x => x.Name.StartsWith("item_dagon"));
        }

        private float GetDamage(Item item, Unit target)
        {
            var index = item.Name.Length == 10 ? 0 : uint.Parse(item.Name.Substring(11)) - 1;
            var damage = item.AbilitySpecialData.First(x => x.Name == "damage").GetValue(index);
            return this.Hero.CalculateSpellDamage(target, DamageType.Magical, damage);
        }

        private void OnUpdate(EventArgs args)
        {
            if (!this.Hero.IsAlive || Game.IsPaused)
            {
                return;
            }

            var dagon = this.GetDagon();

            if (dagon == null || !dagon.CanBeCasted())
            {
                return;
            }

            var target = ObjectManager.GetEntitiesFast<Hero>()
                                      .FirstOrDefault(
                                          h => this.Hero.CanAttack(h) &&
                                               dagon.CanBeCasted(h) && dagon.CanHit(h) &&
                                               this.GetDamage(dagon, h) > h.Health);

            if (target != null)
            {
                dagon.UseAbility(target);
            }
        }
    }
}
// <copyright file="CreepManager.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Ensage;
    using Ensage.SDK.Extensions;

    internal class CreepManager
    {
        private static CreepManager _Instance;

        private static List<Creep> Creeps = new List<Creep>();

        private static float LastUpdateTime = 0f;

        public CreepManager()
        {
            Game.OnIngameUpdate += this.GameDispatcherOnOnIngameUpdate;
        }

        public Hero Hero
        {
            get
            {
                return ObjectManager.LocalHero;
            }
        }

        public static CreepManager Instance()
        {
            if (_Instance == null)
            {
                _Instance = new CreepManager();
            }

            return _Instance;
        }

        public List<Creep> GetCreeps()
        {
            return Creeps;
        }

        private void GameDispatcherOnOnIngameUpdate(EventArgs args)
        {
            var now = Game.RawGameTime;
            if ((now - LastUpdateTime) < 0.25f)
            {
                return;
            }

            LastUpdateTime = now;
            var heroPosition = this.Hero.Position;
            Creeps = ObjectManager.GetEntitiesFast<Creep>().Where(unit => unit.IsValid && unit.IsAlive && unit.IsSpawned && heroPosition.IsInRange(unit, 3000f)).ToList();
        }
    }
}
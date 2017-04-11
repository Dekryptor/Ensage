// <copyright file="Program.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper
{
    using System;

    using Ensage;

    using Sniper.Orbwalking;

    class Program
    {
        public static void Main()
        {
            Game.OnIngameUpdate += OnLoad;
        }

        private static void OnLoad(EventArgs args)
        {
            if (ObjectManager.LocalHero == null)
            {
                return;
            }

            Game.OnIngameUpdate -= OnLoad;
            Orbwalker.Instance().Load();
        }
    }
}
// <copyright file="Program.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper
{
    using Ensage;
    using Ensage.SDK.Helpers;

    using Sniper.Orbwalking;

    internal class Program
    {
        public static void Main()
        {
            UpdateManager.Subscribe(OnLoad);
        }

        private static void OnLoad()
        {
            if (ObjectManager.LocalHero == null)
            {
                return;
            }

            UpdateManager.Unsubscribe(OnLoad);
            Orbwalker.Instance().Load();
        }
    }
}
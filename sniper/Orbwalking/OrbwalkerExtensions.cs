// <copyright file="OrbwalkerExtensions.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper.Orbwalking
{
    using Ensage;

    public static class OrbwalkerExtensions
    {
        /// <summary>
        /// returns the health percentage from 0 to 1
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static float HealthPercent(this Unit unit)
        {
            return unit.Health / unit.MaximumHealth;
        }
    }
}
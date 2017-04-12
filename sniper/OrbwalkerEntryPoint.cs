// <copyright file="OrbwalkerEntryPoint.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper
{
    using Ensage.SDK.Orbwalker;
    using Ensage.SDK.Service;

    [ExportAssembly("Orbwalker")]
    public class OrbwalkerEntryPoint : IAssemblyLoader
    {
        public void Activate()
        {
            Orbwalker.Instance().Load();
        }

        public void Deactivate()
        {
        }
    }
}
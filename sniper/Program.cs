// <copyright file="Program.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper
{
    using Ensage.SDK.Orbwalker;
    using Ensage.SDK.Service;

    [ExportAssembly("Sniper")]
    public class EntryPoint : IAssemblyLoader
    {
        public void Activate()
        {
            Orbwalker.Instance().Load();
        }

        public void Deactivate()
        {
        }
    }

    internal class Program
    {
        public static void Main()
        {
        }
    }
}
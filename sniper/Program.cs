// <copyright file="Program.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper
{
    using System.Reflection;

    using Ensage.SDK.Service;

    using log4net;

    using PlaySharp.Toolkit.Logging;

    using Sniper.Orbwalking;

    [ExportAssembly("Sniper")]
    public class EntryPoint : IAssemblyLoader
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
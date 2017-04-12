// <copyright file="WeatherAssemblyEntryPoint.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper
{
    using System;

    using Ensage;
    using Ensage.Common.Menu;
    using Ensage.SDK.Menu;
    using Ensage.SDK.Service;

    [ExportAssembly("Weather Assembly")]
    public class WeatherAssemblyEntryPoint : IAssemblyLoader
    {
        public WeatherAssemblyEntryPoint()
        {
            this.Factory = MenuFactory.Create("Weather Assembly");
            this.SelectedWeather = this.Factory.Item("Weather", new StringList(Enum.GetNames(typeof(WeatherType))));
        }

        public MenuFactory Factory { get; }

        public MenuItem<StringList> SelectedWeather { get; }

        public int Weather
        {
            get
            {
                var var = Game.GetConsoleVar("cl_weather");
                var.RemoveFlags(ConVarFlags.Cheat);
                return var.GetInt();
            }

            set
            {
                var var = Game.GetConsoleVar("cl_weather");
                var.RemoveFlags(ConVarFlags.Cheat);
                var.SetValue(value);
            }
        }

        public void Activate()
        {
            this.Weather = this.SelectedWeather.Value.SelectedIndex;
            this.SelectedWeather.Item.ValueChanged += this.OnValueChanged;
        }

        public void Deactivate()
        {
            this.SelectedWeather.Item.ValueChanged -= this.OnValueChanged;
        }

        private void OnValueChanged(object sender, OnValueChangeEventArgs args)
        {
            this.Weather = this.SelectedWeather.Value.SelectedIndex;
        }
    }
}
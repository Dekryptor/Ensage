// <copyright file="TimeoutTrigger.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper.Activator
{
    using System;

    public class TimeoutTrigger
    {
        public TimeoutTrigger(TimeSpan timeout)
        {
            this.Timeout = timeout;
        }

        public bool IsTriggered
        {
            get
            {
                if (this.TriggerTime == default(DateTime))
                {
                    return false;
                }

                var diff = this.TriggerTime - DateTime.Now;
                if (diff > this.Timeout)
                {
                    return false;
                }

                return true;
            }
        }

        private TimeSpan Timeout { get; }

        private DateTime TriggerTime { get; set; }

        public void Activate()
        {
            this.TriggerTime = DateTime.Now;
        }

        public void Deactivate()
        {
            this.TriggerTime = default(DateTime);
        }
    }
}
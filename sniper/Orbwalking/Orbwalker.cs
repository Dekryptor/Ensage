// <copyright file="Orbwalker.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper.Orbwalking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Ensage;
    using Ensage.Common.Menu;
    using Ensage.SDK.Extensions;
    using Ensage.SDK.Helpers;
    using Ensage.SDK.Menu;

    using SharpDX;

    using Sniper.Managers;

    internal class Orbwalker
    {
        private static Orbwalker _Instance;

        public OrbwalkingMode Mode = OrbwalkingMode.None;

        private readonly HashSet<NetworkActivity> attackActivityList = new HashSet<NetworkActivity>
                                                                       {
                                                                           NetworkActivity.Attack,
                                                                           NetworkActivity.Attack2,
                                                                           NetworkActivity.AttackEvent
                                                                       };

        public Orbwalker(Unit owner)
        {
            this.Owner = owner;

            this.Factory = MenuFactory.Create("Orbwalker");
            this.AttackMenu = this.Factory.Item("Attack", true);
            this.MoveMenu = this.Factory.Item("Move", true);

            this.MoveDelay = this.Factory.Item("Move Delay", new Slider(5, 0, 250));
            this.AttackDelay = this.Factory.Item("Attack Delay", new Slider(5, 0, 250));

            this.Combo = this.Factory.Item("Combo", new KeyBind(32, KeyBindType.Press));
            this.LaneClear = this.Factory.Item("LaneClear", new KeyBind(0, KeyBindType.Press));
            this.LastHit = this.Factory.Item("LastHit", new KeyBind(0, KeyBindType.Press));
            this.Mixed = this.Factory.Item("Mixed", new KeyBind(0, KeyBindType.Press));
        }

        public MenuItem<Slider> AttackDelay { get; set; }

        public MenuItem<bool> AttackMenu { get; set; }

        public MenuItem<KeyBind> Combo { get; set; }

        public MenuFactory Factory { get; set; }

        public MenuItem<KeyBind> LaneClear { get; set; }

        public bool LaneClearRateLimitResult { get; set; }

        public float LaneClearRateLimitTime { get; set; }

        public float LastAttackOrderIssuedTime { get; set; }

        public float LastAttackTime { get; set; }

        public MenuItem<KeyBind> LastHit { get; set; }

        public float LastMoveOrderIssuedTime { get; set; }

        public Unit LastTarget { get; set; }

        public MenuItem<KeyBind> Mixed { get; set; }

        public MenuItem<Slider> MoveDelay { get; set; }

        public MenuItem<bool> MoveMenu { get; set; }

        public Unit Owner { get; set; }

        public float TurnEndTime { get; set; }

        private ParticleEffectManager EffectManager { get; set; }

        private bool Initialized { get; set; }

        public static Orbwalker Instance()
        {
            if (_Instance == null)
            {
                _Instance = new Orbwalker(ObjectManager.LocalHero);
            }

            return _Instance;
        }

        public bool Attack(Unit unit)
        {
            if (!this.AttackMenu.Value)
            {
                return false;
            }

            var time = Game.RawGameTime;
            if ((time - this.LastAttackOrderIssuedTime) < (this.AttackDelay.Value.Value / 1000f))
            {
                return false;
            }

            this.TurnEndTime = Game.RawGameTime + (Game.Ping / 2000f) + (float)this.Owner.TurnTime(unit.NetworkPosition) + 0.1f;
            this.Owner.Attack(unit);
            return true;
        }

        public bool CanAttack(Unit target)
        {
            var rotationTime = this.Owner.TurnTime(target.NetworkPosition);
            return this.Owner.CanAttack() && ((Game.RawGameTime + 0.1f + rotationTime + (Game.Ping / 2000f)) - this.LastAttackTime) > (1f / this.Owner.AttacksPerSecond);
        }

        public bool CanMove()
        {
            return (((Game.RawGameTime - 0.1f) + (Game.Ping / 2000f)) - this.LastAttackTime) > this.Owner.AttackPoint();
        }

        public Unit GetTarget()
        {
            try
            {
                if (this.Mode != OrbwalkingMode.LastHit)
                {
                    var targetHero = ObjectManager.GetEntitiesFast<Hero>()
                                                  .FirstOrDefault(unit => unit.IsValid && unit.Team != this.Owner.Team && this.Owner.IsValidOrbwalkingTarget(unit));
                    if (targetHero != null)
                    {
                        return targetHero;
                    }
                }

                if (this.Mode == OrbwalkingMode.LaneClear || this.Mode == OrbwalkingMode.LastHit || this.Mode == OrbwalkingMode.Mixed)
                {
                    // killable creeps
                    var killableCreep =
                        CreepManager.Instance().GetCreeps()
                                    .FirstOrDefault(
                                        unit =>
                                            unit.IsValid && unit.Team != this.Owner.Team && this.Owner.IsValidOrbwalkingTarget(unit)
                                            && HealthPrediction.Instance().GetPredictedHealth(unit, this.Owner.GetAutoAttackArrivalTime(unit) + (Game.Ping / 2000f))
                                            < this.Owner.GetAttackDamage(unit, true));

                    if (killableCreep != null)
                    {
                        return killableCreep;
                    }
                }

                if (this.Mode == OrbwalkingMode.LaneClear || this.Mode == OrbwalkingMode.LastHit || this.Mode == OrbwalkingMode.Mixed)
                {
                    // check if we should wait because there is a creep that will be killable soon
                    if ((Game.RawGameTime - this.LaneClearRateLimitTime) > 0.25f)
                    {
                        this.LaneClearRateLimitResult = this.ShouldWait();
                        this.LaneClearRateLimitTime = Game.RawGameTime;
                    }

                    if (this.LaneClearRateLimitResult)
                    {
                        return null;
                    }

                    // denyCreep creeps
                    var denyCreep =
                        CreepManager.Instance().GetCreeps()
                                    .FirstOrDefault(
                                        unit =>
                                            unit.IsValid && unit.Team == this.Owner.Team && this.Owner.IsValidOrbwalkingTarget(unit) && unit.HealthPercent() < 0.5f
                                            && HealthPrediction.Instance().GetPredictedHealth(unit, this.Owner.GetAutoAttackArrivalTime(unit) + (Game.Ping / 2000f))
                                            < this.Owner.GetAttackDamage(unit, true));

                    if (denyCreep != null)
                    {
                        return denyCreep;
                    }
                }

                if (this.Mode == OrbwalkingMode.LaneClear)
                {
                    // towers // barracks shrines
                    var barracks =
                        ObjectManager.GetEntitiesFast<Building>()
                                     .FirstOrDefault(unit => unit.IsValid && unit.Team != this.Owner.Team && this.Owner.IsValidOrbwalkingTarget(unit));

                    if (barracks != null)
                    {
                        return barracks;
                    }

                    var tower =
                        ObjectManager.GetEntitiesFast<Tower>()
                                     .FirstOrDefault(unit => unit.IsValid && unit.Team != this.Owner.Team && this.Owner.IsValidOrbwalkingTarget(unit));

                    if (tower != null)
                    {
                        return tower;
                    }

                    // jungle mobs
                    var jungleMob =
                        CreepManager.Instance().GetCreeps()
                                    .FirstOrDefault(unit => unit.IsValid && unit.IsNeutral && this.Owner.IsValidOrbwalkingTarget(unit));

                    if (jungleMob != null)
                    {
                        return jungleMob;
                    }

                    // creeps
                    var creep =
                        CreepManager.Instance().GetCreeps()
                                    .FirstOrDefault(unit => unit.IsValid && unit.Team != this.Owner.Team && this.Owner.IsValidOrbwalkingTarget(unit));

                    if (creep != null)
                    {
                        return creep;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return null;
        }

        public bool Load()
        {
            if (this.Initialized)
            {
                return false;
            }

            this.Initialized = true;

            UpdateManager.Subscribe(this.OnDrawingsUpdate, 1000);
            this.EffectManager = new ParticleEffectManager();

            HealthPrediction.Instance().Load();
            Game.OnIngameUpdate += this.GameDispatcherOnOnIngameUpdate;
            Entity.OnInt32PropertyChange += this.Hero_OnInt32PropertyChange;

            return true;
        }

        public bool Move(Vector3 position)
        {
            if (!this.MoveMenu.Value)
            {
                return false;
            }

            var time = Game.RawGameTime;
            if ((time - this.LastMoveOrderIssuedTime) < (this.MoveDelay.Value.Value / 1000f))
            {
                // 0.005f
                return false;
            }

            this.LastMoveOrderIssuedTime = Game.RawGameTime;

            this.Owner.Move(position);
            return true;
        }

        public bool Unload()
        {
            if (!this.Initialized)
            {
                return false;
            }

            this.Initialized = false;

            UpdateManager.Unsubscribe(this.OnDrawingsUpdate);
            HealthPrediction.Instance().Unload();
            Game.OnIngameUpdate -= this.GameDispatcherOnOnIngameUpdate;
            Entity.OnInt32PropertyChange -= this.Hero_OnInt32PropertyChange;

            this.EffectManager?.Dispose();

            return true;
        }

        private void GameDispatcherOnOnIngameUpdate(EventArgs args)
        {
            this.Mode = OrbwalkingMode.None;

            // no spamerino
            if (Game.IsPaused || Game.IsChatOpen)
            {
                return;
            }

            if (this.Combo.Value.Active)
            {
                this.Mode = OrbwalkingMode.Combo;
            }
            else if (this.LaneClear.Value.Active)
            {
                this.Mode = OrbwalkingMode.LaneClear;
            }
            else if (this.LastHit.Value.Active)
            {
                this.Mode = OrbwalkingMode.LastHit;
            }
            else if (this.Mixed.Value.Active)
            {
                this.Mode = OrbwalkingMode.Mixed;
            }

            if (this.Mode == OrbwalkingMode.None)
            {
                return;
            }

            // turning
            if (this.TurnEndTime > Game.RawGameTime)
            {
                return;
            }

            var target = this.GetTarget();

            if ((target == null || !this.CanAttack(target)) && this.CanMove())
            {
                this.Move(Game.MousePosition);
                return;
            }

            if (target != null && this.CanAttack(target))
            {
                this.LastTarget = target;
                this.Attack(target);
            }

            if (this.LastTarget != null && this.LastTarget.IsValid && this.LastTarget.IsAlive)
            {
                this.EffectManager.DrawRange(this.LastTarget, "attackTarget", 60, Color.Red);
            }
            else
            {
                this.EffectManager.Remove("attackTarget");
            }
        }

        private void Hero_OnInt32PropertyChange(Entity sender, Int32PropertyChangeEventArgs args)
        {
            if (sender != this.Owner)
            {
                return;
            }

            if (!args.PropertyName.Equals("m_networkactivity", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            var newNetworkActivity = (NetworkActivity)args.NewValue;

            if (this.attackActivityList.Contains(newNetworkActivity))
            {
                var diff = Game.RawGameTime - this.LastAttackTime;
                this.LastAttackTime = Game.RawGameTime - (Game.Ping / 2000f);
            }
        }

        private void OnDrawingsUpdate()
        {
            this.EffectManager.DrawRange(ObjectManager.LocalHero, "attackRange", this.Owner.AttackRange(this.Owner), Color.LimeGreen);
        }

        private bool ShouldWait()
        {
            var t = 2f;
            return CreepManager.Instance().GetCreeps()
                               .Any(
                                   unit =>
                                       unit.IsValid && unit.Team != this.Owner.Team && this.Owner.IsValidOrbwalkingTarget(unit)
                                       && HealthPrediction.Instance().GetPredictedHealth(unit, t / this.Owner.AttacksPerSecond) < this.Owner.GetAttackDamage(unit, true));
        }
    }
}
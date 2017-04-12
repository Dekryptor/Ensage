// <copyright file="HealthPrediction.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Sniper.Orbwalking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Ensage;
    using Ensage.SDK.Extensions;

    using SharpDX;

    using Sniper.Managers;

    internal class CreepStatus
    {
        private float _attackPoint;

        private bool _isMelee;

        private bool _isMeleeCached;

        private bool _isSiege;

        private bool _isSiegeCached;

        private bool _isTower;

        private bool _isTowerCached;

        private bool _isValid = true;

        private Creep _lastPossibleTarget;

        private float _missileSpeed;

        private Creep _target;

        private Team _team = Team.Undefined;

        private float _timeBetweenAttacks;

        public CreepStatus(Unit source)
        {
            this.Source = source;
        }

        public float AttackPoint
        {
            get
            {
                if (this._attackPoint == 0f)
                {
                    this._attackPoint = (float)this.Source.AttackPoint();
                }

                return this._attackPoint;
            }
        }

        public bool IsMelee
        {
            get
            {
                if (!this._isMeleeCached)
                {
                    this._isMelee = this.Source.IsMelee;
                    this._isMeleeCached = true;
                }

                return this._isMelee;
            }
        }

        public bool IsSiege
        {
            get
            {
                if (!this._isSiegeCached)
                {
                    this._isSiege = this.Source.Name.Contains("siege");
                    this._isSiegeCached = true;
                }

                return this._isSiege;
            }
        }

        public bool IsTower
        {
            get
            {
                if (!this._isTowerCached)
                {
                    this._isTower = this.Source as Tower != null;
                    this._isTowerCached = true;
                }

                return this._isTower;
            }
        }

        public bool IsValid
        {
            get
            {
                if (!this._isValid)
                {
                    return false;
                }

                if (!this.Source.IsValid || !this.Source.IsAlive)
                {
                    return false;
                }

                return this._isValid;
            }
        }

        public float LastAttackAnimationTime { get; set; }

        public float MissileSpeed
        {
            get
            {
                if (this._missileSpeed == 0f)
                {
                    if (this.IsMelee)
                    {
                        this._missileSpeed = float.MaxValue;
                    }
                    else
                    {
                        this._missileSpeed = (float)this.Source.ProjectileSpeed();
                    }
                }

                return this._missileSpeed;
            }
        }

        public Unit Source { get; set; }

        public Creep Target
        {
            get
            {
                if (this._target != null && this._target.IsValid && this.Source.IsValidOrbwalkingTarget(this._target))
                {
                    return this._target;
                }

                if (this._lastPossibleTarget != null && this._lastPossibleTarget.IsValid && this.Source.IsDirectlyFacing(this._lastPossibleTarget)
                    && this.Source.IsValidOrbwalkingTarget(this._lastPossibleTarget))
                {
                    return this._lastPossibleTarget;
                }

                var possibleTarget = CreepManager.Instance().GetCreeps()
                                                 .FirstOrDefault(
                                                     unit =>
                                                         unit.IsValid && unit.Team != this.Team && this.Source.IsDirectlyFacing(unit) && this.Source.IsValidOrbwalkingTarget(unit));

                if (possibleTarget != null)
                {
                    this._lastPossibleTarget = possibleTarget;
                    return possibleTarget;
                }

                return null;
            }

            set
            {
                this._target = value;
            }
        }

        public Team Team
        {
            get
            {
                if (this._team == Team.Undefined)
                {
                    this._team = this.Source.Team;
                }

                return this._team;
            }
        }

        public float TimeBetweenAttacks
        {
            get
            {
                if (this._timeBetweenAttacks == 0f)
                {
                    this._timeBetweenAttacks = 1 / this.Source.AttacksPerSecond;
                }

                return this._timeBetweenAttacks;
            }
        }

        public float GetAutoAttackArrivalTime(Unit target)
        {
            var result = this.Source.GetProjectileArrivalTime(target, this.AttackPoint, this.MissileSpeed, !this.IsTower);

            if (!this.IsTower && !this.IsSiege)
            {
                result -= 0.10f;
            }

            return result;
        }
    }

    public class HealthPrediction
    {
        private static HealthPrediction _Instance;

        private float _LastUpdateTime;

        private Dictionary<uint, CreepStatus> CreepStatuses = new Dictionary<uint, CreepStatus>();

        public Hero Hero
        {
            get
            {
                return ObjectManager.LocalHero;
            }
        }

        private bool _Initialized { get; set; }

        public static HealthPrediction Instance()
        {
            if (_Instance == null)
            {
                _Instance = new HealthPrediction();
            }

            return _Instance;
        }

        public float GetPredictedHealth(Unit unit, float untilTime)
        {
            var now = Game.RawGameTime;
            var health = (float)unit.Health;
            untilTime = Math.Max(0f, untilTime);
            untilTime = now + untilTime;

            var team = unit.Team;
            foreach (var creepStatusValuePair in this.CreepStatuses)
            {
                var creepStatus = creepStatusValuePair.Value;

                if (!creepStatus.IsValid)
                {
                    continue;
                }

                if (creepStatus.Team == team)
                {
                    continue;
                }

                var targetCreep = creepStatus.Target;
                if (targetCreep == null || targetCreep.Handle != unit.Handle)
                {
                    continue;
                }

                var damage = creepStatus.Source.GetAttackDamage(unit);

                float attackHitTime;

                if (creepStatus.LastAttackAnimationTime == 0f || (now - creepStatus.LastAttackAnimationTime) > (creepStatus.TimeBetweenAttacks + 0.2))
                {
                    continue;
                }

                // handle melee creeps
                if (creepStatus.IsMelee)
                {
                    attackHitTime = creepStatus.LastAttackAnimationTime + creepStatus.AttackPoint;
                }

                // ranged creeps
                else
                {
                    attackHitTime = (creepStatus.LastAttackAnimationTime - creepStatus.TimeBetweenAttacks) + creepStatus.GetAutoAttackArrivalTime(unit);
                }

                var i = 0;
                while (attackHitTime <= untilTime)
                {
                    if (attackHitTime > now)
                    {
                        health -= damage;
                    }

                    attackHitTime += creepStatus.TimeBetweenAttacks;
                    i++;
                }
            }

            if (health > 0f)
            {
                // towers
                var closestTower =
                    ObjectManager.GetEntitiesFast<Tower>().OrderBy(tower => tower.IsValid ? tower.Distance2D(this.Hero) : float.MaxValue).FirstOrDefault(t => t.IsValid);
                if (closestTower != null)
                {
                    var towerTarget = closestTower.AttackTarget;
                    if (towerTarget != null && towerTarget == unit)
                    {
                        var creepStatus = this.GetCreepStatusEntry(closestTower);
                        var damage = closestTower.GetAttackDamage(unit);
                        var attackHitTime = (creepStatus.LastAttackAnimationTime - creepStatus.TimeBetweenAttacks) + creepStatus.GetAutoAttackArrivalTime(unit);

                        while (attackHitTime <= untilTime)
                        {
                            if (attackHitTime > now)
                            {
                                health -= damage;
                            }

                            attackHitTime += creepStatus.TimeBetweenAttacks;
                        }
                    }
                }
            }

            return health;
        }

        public bool Load()
        {
            if (this._Initialized)
            {
                return false;
            }

            this._Initialized = true;

            ObjectManager.OnAddTrackingProjectile += this.ObjectManagerOnOnAddTrackingProjectile;
            Game.OnIngameUpdate += this.Game_OnIngameUpdate;
            Drawing.OnDraw += this.DrawingOnOnDraw;
            Entity.OnAnimationChanged += this.Unit_OnAnimationChanged;
            Entity.OnHandlePropertyChange += this.Tower_OnHandlePropertyChange;
            return true;
        }

        public bool Unload()
        {
            if (!this._Initialized)
            {
                return false;
            }

            this._Initialized = false;
            ObjectManager.OnAddTrackingProjectile -= this.ObjectManagerOnOnAddTrackingProjectile;
            Game.OnIngameUpdate -= this.Game_OnIngameUpdate;
            Entity.OnAnimationChanged -= this.Unit_OnAnimationChanged;
            return true;
        }

        private void DrawingOnOnDraw(EventArgs args)
        {
            return;
            var pTeam = this.Hero.Team;
            foreach (var senderCreep in CreepManager.Instance().GetCreeps()
                                                    .Where(unit => unit.Team == pTeam && unit.Distance2D(this.Hero) < 3000f))
            {
                var creepStatus = this.GetCreepStatusEntry(senderCreep);
                var targetCreep = creepStatus.Target;

                if (!creepStatus.IsValid)
                {
                    // continue;
                }

                if (creepStatus.LastAttackAnimationTime == 0f)
                {
                    // continue;
                }

                var position = Drawing.WorldToScreen(senderCreep.Position);
                var text = "Status: " + senderCreep.Handle + " HasTarget: " + (targetCreep != null) + " TimeBetweenAttacks: " + creepStatus.TimeBetweenAttacks + " Animation: "
                           + (Game.RawGameTime - creepStatus.LastAttackAnimationTime) + " MissileSpeed: " + creepStatus.MissileSpeed;

                Drawing.DrawText(text, position, Color.White, FontFlags.AntiAlias);

                if (targetCreep != null)
                {
                    var position2 = Drawing.WorldToScreen(targetCreep.Position) + new Vector2(0, 10 * (senderCreep.Handle % 3));
                    var time = this.Hero.GetAutoAttackArrivalTime(targetCreep);
                    Drawing.DrawText(
                        "Target of " + senderCreep.Handle + " predicted health: " + this.GetPredictedHealth(targetCreep, time) + " vs " + targetCreep.Health + "(" + time + ") "
                        + "inrange: " + this.Hero.IsInAttackRange(targetCreep) + " dis: " + (this.Hero.HullRadius + targetCreep.HullRadius + this.Hero.Distance2D(targetCreep))
                        + " range: " + this.Hero.AttackRange(targetCreep),
                        position2,
                        Color.White,
                        FontFlags.AntiAlias);

                    Drawing.DrawLine(position, position2, Color.White);
                }
            }

            var closestTower = ObjectManager.GetEntitiesFast<Tower>().OrderBy(tower => tower.IsValid ? tower.Distance2D(this.Hero) : float.MaxValue).FirstOrDefault(t => t.IsValid);
            if (closestTower != null)
            {
                var towerTarget = closestTower.AttackTarget;
                if (towerTarget != null)
                {
                    var creepStatus = this.GetCreepStatusEntry(closestTower);
                    var position = Drawing.WorldToScreen(closestTower.Position);
                    var text = "Status: " + closestTower.Handle + " HasTarget: " + (towerTarget != null) + " TimeBetweenAttacks: " + creepStatus.TimeBetweenAttacks + " Animation: "
                               + (Game.RawGameTime - creepStatus.LastAttackAnimationTime) + " Attackpoint: " + creepStatus.AttackPoint;

                    Drawing.DrawText(text, position, Color.White, FontFlags.AntiAlias);

                    var position2 = Drawing.WorldToScreen(towerTarget.Position) + new Vector2(0, 10 * (towerTarget.Handle % 3));
                    var time = this.Hero.GetAutoAttackArrivalTime(towerTarget);
                    Drawing.DrawText(
                        "Target of " + towerTarget.Handle + " predicted health: " + this.GetPredictedHealth(towerTarget, time) + " vs " + towerTarget.Health + "(" + time + ")",
                        position2,
                        Color.White,
                        FontFlags.AntiAlias);

                    Drawing.DrawLine(position, position2, Color.White);
                }
            }
        }

        private void Game_OnIngameUpdate(EventArgs args)
        {
            var now = Game.RawGameTime;
            if ((now - this._LastUpdateTime) < 1)
            {
                return;
            }

            this._LastUpdateTime = now;
            var toRemove = this.CreepStatuses.Where(pair => !pair.Value.IsValid || this.Hero.Distance2D(pair.Value.Source) > 4000).ToList();
            foreach (var remove in toRemove)
            {
                this.CreepStatuses.Remove(remove.Key);
            }
        }

        private CreepStatus GetCreepStatusEntry(Unit source)
        {
            var handle = source.Handle;
            if (!this.CreepStatuses.ContainsKey(handle))
            {
                this.CreepStatuses.Add(handle, new CreepStatus(source));
            }

            return this.CreepStatuses[handle];
        }

        private void ObjectManagerOnOnAddTrackingProjectile(TrackingProjectileEventArgs args)
        {
            var sourceCreep = args.Projectile.Source as Creep;
            var sourceTower = args.Projectile.Source as Tower;

            if (sourceCreep != null)
            {
                if (this.Hero.Distance2D(sourceCreep) > 3000)
                {
                    return;
                }

                if (sourceCreep.IsNeutral)
                {
                    return;
                }

                var creepStatus = this.GetCreepStatusEntry(sourceCreep);
                creepStatus.Target = args.Projectile.Target as Creep;
            }
            else if (sourceTower != null)
            {
                if (this.Hero.Distance2D(sourceTower) > 3000)
                {
                    return;
                }

                var creepStatus = this.GetCreepStatusEntry(sourceTower);
                creepStatus.LastAttackAnimationTime = Game.RawGameTime - creepStatus.AttackPoint - (Game.Ping / 2000f);
            }
        }

        private void Tower_OnHandlePropertyChange(Entity sender, HandlePropertyChangeEventArgs args)
        {
            if (!(sender is Tower))
            {
                return;
            }

            if (!args.PropertyName.Equals("m_htowerattacktarget", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            if (this.Hero.Distance2D(sender) > 3000)
            {
                return;
            }

            var tower = sender as Tower;
            var creepStatus = this.GetCreepStatusEntry(tower);
            creepStatus.LastAttackAnimationTime = Game.RawGameTime - (Game.Ping / 2000f);
        }

        private void Unit_OnAnimationChanged(Entity sender, EventArgs args)
        {
            if (!(sender is Creep))
            {
                return;
            }

            if (this.Hero.Distance2D(sender) > 3000)
            {
                return;
            }

            var senderCreep = sender as Creep;

            if (senderCreep.IsNeutral)
            {
                return;
            }

            if (!senderCreep.Animation.Name.ToLowerInvariant().Contains("attack"))
            {
                return;
            }

            var creepStatus = this.GetCreepStatusEntry(senderCreep);
            creepStatus.LastAttackAnimationTime = Game.RawGameTime - (Game.Ping / 2000f);
        }
    }
}
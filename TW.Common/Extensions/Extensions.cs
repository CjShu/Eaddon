namespace TW.Common.Extensions
{
    using System;
    using System.IO;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization.Json;
    using SharpDX;
    using EloBuddy;
    using Extension = EloBuddy.SDK.Extensions;
    using Distance;

    public static class Extensions
    {
        #region Enums

        public enum FountainType
        {
            OwnFountain,

            EnemyFountain
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Returns the unit's ability power
        /// </summary>
        [Obsolete("Use TotalMagicalDamage attribute.", false)]
        public static float AbilityPower(this Obj_AI_Base @base)
        {
            return @base.FlatMagicDamageMod + (@base.PercentMagicDamageMod * @base.FlatMagicDamageMod);
        }

        [Obsolete("Use CountEnemiesInRange", false)]
        public static int CountEnemysInRange(this Obj_AI_Base unit, float range)
        {
            return unit.ServerPosition.CountEnemiesInRange(range);
        }

        [Obsolete("Use CountEnemiesInRange", false)]
        public static int CountEnemysInRange(this Vector3 point, float range)
        {
            return point.CountEnemiesInRange(range);
        }

        [Obsolete("Use HealthPercent attribute.", false)]
        public static float HealthPercentage(this Obj_AI_Base unit)
        {
            return unit.HealthPercent;
        }

        [Obsolete("Use TotalAttackDamage attribute from LeagueSharp.Core", false)]
        public static float TotalAttackDamage(this AIHeroClient target)
        {
            return target.TotalAttackDamage;
        }

        [Obsolete("Use TotalMagicalDamage from Leaguesharp.Core.", false)]
        public static float TotalMagicalDamage(this AIHeroClient target)
        {
            return target.TotalMagicalDamage;
        }

        /// <summary>
        ///     Returns the unit's mana percentage (From 0 to 100).
        /// </summary>
        [Obsolete("Use ManaPercent attribute.", false)]
        public static float ManaPercentage(this Obj_AI_Base unit)
        {
            return unit.ManaPercent;
        }

        #endregion

        /// <summary>
        ///     Returns if the spell is ready to use.
        /// </summary>
        public static bool IsReady(this SpellDataInst spell, int t = 0)
        {
            return spell != null && spell.Slot != SpellSlot.Unknown && t == 0
                       ? spell.State == SpellState.Ready
                       : (spell.State == SpellState.Ready
                          || (spell.State == SpellState.Cooldown && (spell.CooldownExpires - Game.Time) <= t / 1000f));
        }

        public static bool IsReady(this SpellSlot slot, int t = 0)
        {
            var s = HeroManager.Player.Spellbook.GetSpell(slot);
            return s != null && IsReady(s, t);
        }

        public static bool IsReady(this Spell spell, int t = 0)
        {
            return IsReady(spell.Instance, t);
        }

        /// <summary>
        ///     Returns true if the buff is active and didn't expire.
        /// </summary>
        public static bool IsValidBuff(this BuffInstance buff)
        {
            return buff.IsActive && buff.EndTime - Game.Time > 0;
        }

        /// <summary>
        ///     Returns if the SpellSlot of the InventorySlot is valid
        /// </summary>
        public static bool IsValidSlot(this InventorySlot slot)
        {
            return slot != null && slot.SpellSlot != SpellSlot.Unknown;
        }

        /// <summary>
        ///     Checks if the unit casting recall
        /// </summary>
        public static bool IsRecalling(this AIHeroClient unit)
        {
            return unit.Buffs.Any(buff => buff.Name.ToLower().Contains("recall") && buff.Type == BuffType.Aura);
        }

        public static bool UnderAllyTurret(this Obj_AI_Base unit)
        {
            return UnderAllyTurret(unit.Position);
        }

        public static bool UnderAllyTurret(this Vector3 position)
        {
            return
                ObjectManager.Get<Obj_AI_Turret>()
                    .Any(turret => turret.IsValidTarget(950, false, position) && turret.IsAlly);
        }

        /// <summary>
        ///     Returns true if the unit is under tower range.
        /// </summary>
        public static bool UnderTurret(this Obj_AI_Base unit)
        {
            return UnderTurret(unit.Position, true);
        }

        /// <summary>
        ///     Returns true if the unit is under turret range.
        /// </summary>
        public static bool UnderTurret(this Obj_AI_Base unit, bool enemyTurretsOnly)
        {
            return UnderTurret(unit.Position, enemyTurretsOnly);
        }

        public static bool UnderTurret(this Vector3 position, bool enemyTurretsOnly)
        {
            return
                ObjectManager.Get<Obj_AI_Turret>().Any(turret => turret.IsValidTarget(950, enemyTurretsOnly, position));
        }

        public static bool IsMovingInSameDirection(Obj_AI_Base source, Obj_AI_Base target)
        {
            var sourceLW = source.GetWaypoints().Last().To3D();

            if (sourceLW == source.Position || !source.IsMoving)
                return false;

            var targetLW = target.GetWaypoints().Last().To3D();

            if (targetLW == target.Position || !target.IsMoving)
                return false;

            var pos1 = sourceLW.To2D() - source.Position.To2D();
            var pos2 = targetLW.To2D() - target.Position.To2D();
            var getAngle = pos1.AngleBetween(pos2);

            if (getAngle < 25)
                return true;
            else
                return false;
        }

        public static List<Vector3> CirclePoints(float CircleLineSegmentN, float radius, Vector3 position)
        {
            var points = new List<Vector3>();

            for (var i = 1; i <= CircleLineSegmentN; i++)
            {
                var angle = i * 2 * Math.PI / CircleLineSegmentN;
                var point = new Vector3(position.X + radius * (float)Math.Cos(angle), position.Y + radius * (float)Math.Sin(angle), position.Z);
                points.Add(point);
            }
            return points;
        }

        public static bool IsValidTarget(
            this AttackableUnit unit,
            float range = float.MaxValue,
            bool checkTeam = true,
            Vector3 from = default(Vector3))
        {
            if (unit == null || !unit.IsValid || !unit.IsVisible || unit.IsDead || !unit.IsTargetable
                || unit.IsInvulnerable)
            {
                return false;
            }

            if (checkTeam && unit.Team == HeroManager.Player.Team)
            {
                return false;
            }

            var obj = unit as Obj_AI_Base;
            if (obj != null && !obj.IsHPBarRendered)
            {
                return false;
            }

            if (unit.Name == "WardCorpse")
            {
                return false;
            }

            var @base = unit as Obj_AI_Base;

            return !(range < float.MaxValue)
                   || !(Vector2.DistanceSquared(
                            (@from.To2D().IsValid() ? @from : HeroManager.Player.ServerPosition).To2D(),
                            (@base != null ? @base.ServerPosition : unit.Position).To2D()) > range * range);
        }

        public static bool IsValidTarget1(this AttackableUnit target,
            float? range = null,
            bool onlyEnemyTeam = true,
            Vector3? rangeCheckFrom = null)
        {
            if (target == null || !target.IsValid || target.IsDead || !target.IsVisible || !target.IsTargetable || target.IsInvulnerable)
            {
                return false;
            }
            if (onlyEnemyTeam && Player.Instance.Team == target.Team)
            {
                return false;
            }
            var obj = target as Obj_AI_Base;
            if (obj != null && !obj.IsHPBarRendered)
            {
                return false;
            }
            if (!range.HasValue)
            {
                return true;
            }
            range = new float?(range.Value.Pow());

            var pos = (obj != null) ? obj.ServerPosition : target.Position;

            if (!rangeCheckFrom.HasValue)
            {
                float num = Player.Instance.ServerPosition.DistanceSquared(pos);
                float? num2 = range;
                return num < num2.GetValueOrDefault() && num2.HasValue;
            }
            float num3 = rangeCheckFrom.Value.Distance(pos, true);
            float? num4 = range;
            return num3 < num4.GetValueOrDefault() && num4.HasValue;
        }

        public static bool IsValidTargetCommon(
            this AttackableUnit target,
            float range = float.MaxValue,
            bool onlyEnemyTeam = true,
            Vector3 from = default(Vector3))
        {
            return IsValidTarget(target, range, onlyEnemyTeam, from);
        }

        public static float Pow(this float number)
        {
            return number * number;
        }

        public static List<Vector2> CutPath(this List<Vector2> path, float distance)
        {
            var result = new List<Vector2>();
            var Distance = distance;
            if (distance < 0)
            {
                path[0] = path[0] + distance * (path[1] - path[0]).Normalized();
                return path;
            }

            for (var i = 0; i < path.Count - 1; i++)
            {
                var dist = path[i].Distance(path[i + 1]);
                if (dist > Distance)
                {
                    result.Add(path[i] + Distance * (path[i + 1] - path[i]).Normalized());
                    for (var j = i + 1; j < path.Count; j++)
                    {
                        result.Add(path[j]);
                    }

                    break;
                }
                Distance -= dist;
            }
            return result.Count > 0 ? result : new List<Vector2> { path.Last() };
        }

        #region 範圍

        // Use same interface as CountEnemiesInRange
        /// <summary>
        ///     Count the allies in range of the Player.
        /// </summary>
        public static int CountAlliesInRange(float range)
        {
            return HeroManager.Player.CountAlliesInRange(range);
        }

        /// <summary>
        ///     Counts the allies in range of the Unit.
        /// </summary>
        public static int CountAlliesInRange(this Obj_AI_Base unit, float range)
        {
            return unit.ServerPosition.CountAlliesInRange(range, unit);
        }

        /// <summary>
        ///     Counts the allies in the range of the Point.
        /// </summary>
        public static int CountAlliesInRange(this Vector3 point, float range, Obj_AI_Base originalunit = null)
        {
            if (originalunit != null)
            {
                return
                    HeroManager.Allies.Count(
                        x => x.NetworkId != originalunit.NetworkId && x.IsValidTarget(range, false, point));
            }
            return HeroManager.Allies.Count(x => x.IsValidTarget(range, false, point));
        }

        /// <summary>
        ///     Counts the enemies in range of Player.
        /// </summary>
        public static int CountEnemiesInRange(float range)
        {
            return HeroManager.Player.CountEnemiesInRange(range);
        }

        /// <summary>
        ///     Counts the enemies in range of Unit.
        /// </summary>
        public static int CountEnemiesInRange(this Obj_AI_Base unit, float range)
        {
            return unit.ServerPosition.CountEnemiesInRange(range);
        }

        /// <summary>
        ///     Counts the enemies in range of point.
        /// </summary>
        public static int CountEnemiesInRange(this Vector3 point, float range)
        {
            return HeroManager.Enemies.Count(h => h.IsValidTarget(range, true, point));
        }

        public static List<AIHeroClient> GetAlliesInRange(this Obj_AI_Base unit, float range)
        {
            return GetAlliesInRange(unit.ServerPosition, range, unit);
        }

        public static List<AIHeroClient> GetAlliesInRange(
            this Vector3 point,
            float range,
            Obj_AI_Base originalunit = null)
        {
            if (originalunit != null)
            {
                return
                    HeroManager.Allies.FindAll(
                        x =>
                            x.NetworkId != originalunit.NetworkId && point.Distance(x.ServerPosition, true) <= range * range);
            }
            return HeroManager.Allies.FindAll(x => point.Distance(x.ServerPosition, true) <= range * range);
        }

        public static List<AIHeroClient> GetEnemiesInRange(this Obj_AI_Base unit, float range)
        {
            return GetEnemiesInRange(unit.ServerPosition, range);
        }

        public static List<AIHeroClient> GetEnemiesInRange(this Vector3 point, float range)
        {
            return HeroManager.Enemies.FindAll(x => point.Distance(x.ServerPosition, true) <= range * range);
        }

        #endregion

        #region T GetObjects

        public static List<T> GetObjects<T>(this Vector3 position, float range) where T : GameObject, new()
        {
            return ObjectManager.Get<T>().Where(x => position.Distance(x.Position, true) < range * range).ToList();
        }

        public static List<T> GetObjects<T>(string objectName, float range, Vector3 rangeCheckFrom = new Vector3())
            where T : GameObject, new()
        {
            if (rangeCheckFrom.Equals(Vector3.Zero))
            {
                rangeCheckFrom = HeroManager.Player.ServerPosition;
            }

            return ObjectManager.Get<T>().Where(x => rangeCheckFrom.Distance(x.Position, true) < range * range).ToList();
        }

        #endregion

        public static short GetPacketId(this GamePacketEventArgs gamePacketEventArgs)
        {
            var packetData = gamePacketEventArgs.PacketData;
            if (packetData.Length < 2)
            {
                return 0;
            }
            return (short)(packetData[0] + packetData[1] * 256);
        }

        #region 回成時間 與技能

        /// <summary>
        ///     Returns the recall duration
        /// </summary>
        /*
        public static int GetRecallTime(AIHeroClient obj)
        {
            return GetRecallTime(obj.Spellbook.GetSpell(SpellSlot.Recall).Name);
        }
        //*/

        public static int GetRecallTime(string recallName)
        {
            var duration = 0;

            switch (recallName.ToLower())
            {
                case "recall":
                    duration = 8000;
                    break;
                case "recallimproved":
                    duration = 7000;
                    break;
                case "odinrecall":
                    duration = 4500;
                    break;
                case "odinrecallimproved":
                    duration = 4000;
                    break;
                case "superrecall":
                    duration = 4000;
                    break;
                case "superrecallimproved":
                    duration = 4000;
                    break;
            }
            return duration;
        }

        public static int GetRecallDuration(GameObjectTeleportEventArgs args)
        {
            string key;
            switch (key = args.RecallType.ToLower())
            {
                case "recall":
                    return 8000;
                case "recallimproved":
                    return 7000;
                case "odinrecall":
                    return 4500;
                case "odinrecallimproved":
                    return 4000;
                case "superrecall":
                    return 4000;
                case "superrecallimproved":
                    return 4000;
            }
            return 8000;
        }

        public static SpellDataInst GetSpell(this AIHeroClient hero, SpellSlot slot)
        {
            return hero.Spellbook.GetSpell(slot);
        }

        /// <summary>
        ///     Will return real time spell cooldown
        /// </summary>
        /// <param name="hero"></param>
        /// <param name="spell"></param>
        /// <returns></returns>
        public static float GetSpellCooldownEx(this AIHeroClient hero, SpellSlot spell)
        {
            var expire = hero.Spellbook.GetSpell(spell).CooldownExpires;
            var cd = (expire - (Game.Time - 1));

            return cd <= 0 ? 0 : cd;
        }

        /// <summary>
        ///     Returns the spell slot with the name.
        /// </summary>
        public static SpellSlot GetSpellSlot(this AIHeroClient unit, string name)
        {
            foreach (var spell in
                unit.Spellbook.Spells.Where(
                    spell => String.Equals(spell.Name, name, StringComparison.CurrentCultureIgnoreCase)))
            {
                return spell.Slot;
            }

            return SpellSlot.Unknown;
        }

        /// <summary>
        ///     Checks if this spell is an autoattack
        /// </summary>
        public static bool IsAutoAttack(this SpellData spellData)
        {
            return Orbwalking.IsAutoAttack(spellData.Name);
        }

        /// <summary>
        ///     Checks if this spell is an autoattack
        /// </summary>
        public static bool IsAutoAttack(this SpellDataInst spellData)
        {
            return Orbwalking.IsAutoAttack(spellData.Name);
        }


        /// <summary>
        /// Returns the actual path of a unit.
        /// </summary>
        /// <param name="unit"> The unit.</param>
        public static Vector3[] GetRealPath(Obj_AI_Base unit)
        {
            const int tolerance = 50;
            var path = unit.Path.ToList();

            for (var i = path.Count - 1; i > 0; i--)
            {
                var start = path[i].To2D();
                var end = path[i - 1].To2D();

                if (unit.ServerPosition.Distance(start, end, true) <= Extension.Pow(tolerance))
                {
                    path.RemoveRange(0, i);
                    break;
                }
            }

            return new[] { unit.Position }.Concat(path).ToArray();
        }

        /// <summary> 
        /// Returns the path the unit is taking.
        /// </summary>
        public static Vector3[] RealPath(this Obj_AI_Base unit)
        {
            return GetRealPath(unit);
        }

        #endregion

        /// <summary>
        ///     Returns the path of the unit appending the ServerPosition at the start, works even if the unit just entered fow.
        /// </summary>
        public static List<Vector2> GetWaypoints(this Obj_AI_Base unit)
        {
            var result = new List<Vector2>();

            if (unit.IsVisible)
            {
                result.Add(unit.ServerPosition.To2D());
                var path = unit.Path;
                if (path.Length > 0)
                {
                    var first = path[0].To2D();
                    if (first.Distance(result[0], true) > 40)
                    {
                        result.Add(first);
                    }

                    for (var i = 1; i < path.Length; i++)
                    {
                        result.Add(path[i].To2D());
                    }
                }
            }
            else if (WaypointTracker.StoredPaths.ContainsKey(unit.NetworkId))
            {
                var path = WaypointTracker.StoredPaths[unit.NetworkId];
                var timePassed = (Utils.TickCount - WaypointTracker.StoredTick[unit.NetworkId]) / 1000f;
                if (path.PathLength() >= unit.MoveSpeed * timePassed)
                {
                    result = CutPath(path, (int)(unit.MoveSpeed * timePassed));
                }
            }

            return result;
        }

        public static List<Vector2Time> GetWaypointsWithTime(this Obj_AI_Base unit)
        {
            var wp = unit.GetWaypoints();

            if (wp.Count < 1)
            {
                return null;
            }

            var result = new List<Vector2Time>();
            var speed = unit.MoveSpeed;
            var lastPoint = wp[0];
            var time = 0f;

            foreach (var point in wp)
            {
                time += point.Distance(lastPoint) / speed;
                result.Add(new Vector2Time(point, time));
                lastPoint = point;
            }

            return result;
        }

        /// <summary>
        ///     Returns if the unit has the buff and it is active
        /// </summary>
        [Obsolete("Use Obj_AI_Base.HasBuff")]
        public static bool HasBuff(
            this Obj_AI_Base unit,
            string buffName,
            bool dontUseDisplayName = false,
            bool kappa = true)
        {
            return
                unit.Buffs.Any(
                    buff =>
                        ((dontUseDisplayName
                          && String.Equals(buff.Name, buffName, StringComparison.CurrentCultureIgnoreCase))
                         || (!dontUseDisplayName
                             && String.Equals(buff.DisplayName, buffName, StringComparison.CurrentCultureIgnoreCase)))
                        && buff.IsValidBuff());
        }

        /// <summary>
        ///     Returns if the unit has the specified buff in the indicated amount of time
        /// </summary>
        public static bool HasBuffIn(
            this Obj_AI_Base unit,
            string displayName,
            float tickCount,
            bool includePing = true)
        {
            return
                unit.Buffs.Any(
                    buff =>
                        buff.IsValid && buff.DisplayName == displayName
                        && buff.EndTime - Game.Time > tickCount - (includePing ? (Game.Ping / 2000f) : 0));
        }

        /// <summary>
        ///     Returns true if unit is in fountain range (range in which fountain heals).
        ///     The second optional parameter allows you to indicate which fountain you want to check against.
        /// </summary>
        public static bool InFountain(this Obj_AI_Base unit, FountainType ftype = FountainType.OwnFountain)
        {
            float fountainRange = 562500; //750 * 750
            var map = TW.Common.Utility.Map.GetMap();
            if (map != null && map.Type == TW.Common.Utility.Map.MapType.SummonersRift)
            {
                fountainRange = 1210000; //1100 * 1100
            }

            var fpos = new Vector3();

            if (ftype == FountainType.OwnFountain)
            {
                fpos = unit.Team == HeroManager.Player.Team ? MiniCache.AllyFountain : MiniCache.EnemyFountain;
            }
            if (ftype == FountainType.EnemyFountain)
            {
                fpos = unit.Team == HeroManager.Player.Team ? MiniCache.EnemyFountain : MiniCache.AllyFountain;
            }

            return unit.IsVisible && unit.Distance(fpos, true) <= fountainRange;
        }

        public static bool IsInFountainRange(this Obj_AI_Base hero, bool enemyFountain = false)
        {
            if (!hero.IsHPBarRendered || !enemyFountain)
            {
                return ObjectManager.Get<Obj_SpawnPoint>().Any((Obj_SpawnPoint s) => s.Team == hero.Team && hero.Distance(s.Position, true) < 1562500f);
            }
            return ObjectManager.Get<Obj_SpawnPoint>().Any((Obj_SpawnPoint s) => s.Team != hero.Team && hero.Distance(s.Position, true) < 1562500f);
        }

        /// <summary>
        ///     Checks a point to see if it is in an ally or enemy fountain
        /// </summary>
        public static bool InFountain(this Vector3 position, FountainType fountain)
        {
            return position.To2D().InFountain(fountain);
        }

        /// <summary>
        ///     Checks a point to see if it is in an ally or enemy fountain
        /// </summary>
        public static bool InFountain(this Vector2 position, FountainType fountain)
        {
            float fountainRange = 562500; //750 * 750
            var map = TW.Common.Utility.Map.GetMap();
            if (map != null && map.Type == TW.Common.Utility.Map.MapType.SummonersRift)
            {
                fountainRange = 1210000; //1100 * 1100
            }
            var fpos = fountain == FountainType.OwnFountain ? MiniCache.AllyFountain : MiniCache.EnemyFountain;
            return position.Distance(fpos, true) <= fountainRange;
        }

        /// <summary>
        ///     Returns if both source and target are Facing Themselves.
        /// </summary>
        public static bool IsBothFacing(Obj_AI_Base source, Obj_AI_Base target)
        {
            return source.IsFacing(target) && target.IsFacing(source);
        }

        /// <summary>
        ///     Returns true if unit is in shop range (range in which the shopping is allowed).
        /// </summary>
        /// <returns></returns>
        public static bool InShop(this Obj_AI_Base unit)
        {
            float fountainRange = 562500; //750 * 750
            var map = TW.Common.Utility.Map.GetMap();
            if (map != null && map.Type == TW.Common.Utility.Map.MapType.SummonersRift)
            {
                fountainRange = 1000000; //1000 * 1000
            }
            var fpos = unit.Team == HeroManager.Player.Team ? MiniCache.AllyFountain : MiniCache.EnemyFountain;
            return unit.IsVisible && unit.Distance(fpos, true) <= fountainRange;
        }

        /// <summary>
        ///     Returns if the source is facing the target.
        /// </summary>
        public static bool IsFacing(this Obj_AI_Base source, Obj_AI_Base target)
        {
            if (source == null || target == null)
            {
                return false;
            }

            const float angle = 90;
            return source.Direction.To2D().Perpendicular().AngleBetween((target.Position - source.Position).To2D())
                   < angle;
        }

        /// <summary>
        ///     Checks if CastState is SuccessfullyCasted
        /// </summary>
        public static bool IsCasted(this Spell.CastStates state)
        {
            return state == Spell.CastStates.SuccessfullyCasted;
        }

        /// <summary>
        ///     Checks if the unit is a Hero or Champion
        /// </summary>
        public static bool IsChampion(this Obj_AI_Base unit)
        {
            var hero = unit as AIHeroClient;
            return hero != null && hero.IsValid;
        }

        /// <summary>
        ///     Checks if this unit is the same as the given champion name
        /// </summary>
        public static bool IsChampion(this Obj_AI_Base unit, string championName)
        {
            var hero = unit as AIHeroClient;
            return hero != null && hero.IsValid && hero.ChampionName.Equals(championName);
        }
        
        public static bool Compare1(this GameObject gameObject, GameObject @object)
        {
            return gameObject != null && gameObject.IsValid && @object != null && @object.IsValid &&
                   gameObject.NetworkId == @object.NetworkId;
        }
        
        /// <summary>
        ///     Returns if the unit's movement is impaired (Slows, Taunts, Charms, Taunts, Snares, Fear)
        /// </summary>
        public static bool IsMovementImpaired(this AIHeroClient hero)
        {
            return hero.HasBuffOfType(BuffType.Flee) || hero.HasBuffOfType(BuffType.Charm)
                   || hero.HasBuffOfType(BuffType.Slow) || hero.HasBuffOfType(BuffType.Snare)
                   || hero.HasBuffOfType(BuffType.Stun) || hero.HasBuffOfType(BuffType.Taunt);
        }

        public static float DistanceToPlayer(this Obj_AI_Base source)
        {
            return ObjectManager.Player.Distance(source);
        }

        public static float DistanceToPlayer(this Vector3 position)
        {
            return position.To2D().DistanceToPlayer();
        }

        public static float DistanceToPlayer(this Vector2 position)
        {
            return ObjectManager.Player.Distance(position);
        }

        /// <summary>
        ///     Checks if this position is a wall using NavMesh
        /// </summary>
        public static bool IsWall(this Vector3 position)
        {
            return NavMesh.GetCollisionFlags(position).HasFlag(CollisionFlags.Wall);
        }

        /// <summary>
        ///     Checks if this position is a wall using NavMesh
        /// </summary>
        public static bool IsWall(this Vector2 position)
        {
            return position.To3D().IsWall();
        }

        /// <summary>
        ///     Levels up a spell
        /// </summary>
        public static void LevelUpSpell(this Spellbook book, SpellSlot slot, bool evolve = false)
        {
            book.LevelSpell(slot);
        }

        public static void ProcessAsPacket(this byte[] packetData, PacketChannel channel = PacketChannel.S2C)
        {
            Game.ProcessPacket(packetData, channel);
        }

        /// <summary>
        ///     Randomizes the position with the supplied min/max
        /// </summary>
        public static Vector3 Randomize(this Vector3 position, int min, int max)
        {
            var ran = new Random(Utils.TickCount);
            return position + new Vector2(ran.Next(min, max), ran.Next(min, max)).To3D();
        }

        /// <summary>
        ///     Randomizes the position with the supplied min/max
        /// </summary>
        public static Vector2 Randomize(this Vector2 position, int min, int max)
        {
            return position.To3D().Randomize(min, max).To2D();
        }

        public static void SendAsPacket(
            this byte[] packetData,
            PacketChannel channel = PacketChannel.C2S,
            PacketProtocolFlags protocolFlags = PacketProtocolFlags.Reliable)
        {
            Game.SendPacket(packetData, channel, protocolFlags);
        }

        public static NavMeshCell ToNavMeshCell(this Vector3 position)
        {
            var nav = NavMesh.WorldToGrid(position.X, position.Y);
            return NavMesh.GetCell((short)nav.X, (short)nav.Y);
        }

        /// <summary>
        ///     Internal class used to get the waypoints even when the enemy enters the fow of war.
        /// </summary>
        internal static class WaypointTracker
        {
            #region Static Fields

            public static readonly Dictionary<int, List<Vector2>> StoredPaths = new Dictionary<int, List<Vector2>>();

            public static readonly Dictionary<int, int> StoredTick = new Dictionary<int, int>();

            #endregion
        }

        public class MiniCache
        {
            #region Static Fields

            private static VectorHolder _allySpawn, _enemySpawn;

            #endregion

            #region Public Properties

            public static Vector3 AllyFountain
            {
                get
                {
                    if (_allySpawn != null) return _allySpawn.position;
                    _allySpawn = new VectorHolder(ObjectManager.Get<Obj_SpawnPoint>().Find(x => x.IsAlly).Position);
                    return _allySpawn.position;
                }
            }

            public static Vector3 EnemyFountain
            {
                get
                {
                    if (_enemySpawn != null) return _enemySpawn.position;
                    _enemySpawn = new VectorHolder(ObjectManager.Get<Obj_SpawnPoint>().Find(x => x.IsEnemy).Position);
                    return _enemySpawn.position;
                }
            }

            #endregion

            private class VectorHolder
            {
                #region Fields

                public Vector3 position;

                #endregion

                #region Constructors and Destructors

                public VectorHolder(Vector3 position)
                {
                    this.position = position;
                }

                #endregion
            }
        }
    }
}
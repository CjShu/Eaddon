namespace TW.Common
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;
    using EloBuddy;
    using SharpDX;
    using Extensions;
    using Color = System.Drawing.Color;
    using Extensions.Distance;

    public static class Orbwalking
    {
        #region Static Fields

        public static bool Attack = true;
        public static bool DisableNextAttack;
        public static int LastAATick;
        public static int LastAttackCommandT;
        public static Vector3 LastMoveCommandPosition = Vector3.Zero;
        public static int LastMoveCommandT;
        public static bool Move = true;
        private static readonly Random _random = new Random(DateTime.Now.Millisecond);
        private static readonly string[] AttackResets =
            {
                "dariusnoxiantacticsonh", "fiorae", "garenq", "gravesmove", "hecarimrapidslash", "jaxempowertwo",
                "leonashieldofdaybreak", "luciane", "monkeykingdoubleattack", "mordekaisermaceofspades", "nasusq",
                "nautiluspiercinggaze", "netherblade", "gangplankqwrapper", "powerfist", "renektonpreexecute",
                "rengarq", "shyvanadoubleattack", "sivirw", "takedown", "talonnoxiandiplomacy", "trundletrollsmash",
                "vaynetumble", "vie", "volibearq", "xenzhaocombotarget", "yorickspectral", "reksaiq",
                "itemtitanichydracleave", "masochism", "illaoiw", "elisespiderw", "fiorae", "meditate",
                "sejuaninorthernwinds", "camilleq", "camilleq2", "asheq"
            };
        private static readonly string[] Attacks =
            {
                "caitlynheadshotmissile", "frostarrow", "garenslash2",
                "kennenmegaproc", "masteryidoublestrike", "quinnwenhanced",
                "renektonexecute", "renektonsuperexecute",
                "rengarnewpassivebuffdash", "trundleq", "xenzhaothrust",
                "xenzhaothrust2", "xenzhaothrust3", "viktorqbuff",
                "lucianpassiveshot"
            };
        private static readonly string[] NoAttacks =
            {
                "volleyattack", "volleyattackwithsound", "jarvanivcataclysmattack", "monkeykingdoubleattack",
                "shyvanadoubleattack", "shyvanadoubleattackdragon", "zyragraspingplantattack",
                "zyragraspingplantattack2", "zyragraspingplantattackfire", "zyragraspingplantattack2fire",
                "viktorpowertransfer", "sivirwattackbounce", "asheqattacknoonhit", "elisespiderlingbasicattack",
                "heimertyellowbasicattack", "heimertyellowbasicattack2", "heimertbluebasicattack",
                "annietibbersbasicattack", "annietibbersbasicattack2", "yorickdecayedghoulbasicattack",
                "yorickravenousghoulbasicattack", "yorickspectralghoulbasicattack", "malzaharvoidlingbasicattack",
                "malzaharvoidlingbasicattack2", "malzaharvoidlingbasicattack3", "kindredwolfbasicattack",
                "gravesautoattackrecoil"
            };
        private static readonly string[] NoCancelChamps = { "Kalista" };
        private static readonly AIHeroClient Player;
        private static int _autoattackCounter;
        private static int _delay;
        private static AttackableUnit _lastTarget;
        private static float _minDistance = 400;
        private static bool _missileLaunched;
        public static int LastMove;
        public static int NextMovementDelay;
        public static Vector3 LastMovementPosition = Vector3.Zero;
        private static bool DisableAttackIfCastSpell = true;
        public static int DelayOnFire;
        public static int DelayOnFireId;
        public static int BrainFarmInt = -100;
        private static List<Obj_AI_Base> MinionListAA = new List<Obj_AI_Base>();

        #endregion

        static Orbwalking()
        {
            Player = ObjectManager.Player;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Spellbook.OnStopCast += SpellbookOnStopCast;
            Obj_AI_Base.OnSpellCast += Obj_AI_Base_OnDoCast;
            Obj_AI_Base.OnBasicAttack += OnBasicAttack;
            Obj_AI_Base.OnPlayAnimation += OnPlayAnimation;
        }

        private static void OnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Rengar")
            {
                return;
            }

            if (sender.IsMe && args.Animation == "Spell5")
            {
                var t = 0;

                if (_lastTarget != null && _lastTarget.IsValid)
                {
                    t += (int)Math.Min(ObjectManager.Player.Distance(_lastTarget) / 1.5f, 0.6f);
                }

                LastAATick = Utils.GameTimeTickCount - Game.Ping / 2 + t;
            }
        }

        private static void OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.Target is Obj_AI_Base || args.Target is Obj_BarracksDampener || args.Target is Obj_HQ)
                {
                    LastAATick = Utils.GameTimeTickCount - Game.Ping / 2;
                    _missileLaunched = false;
                    LastMoveCommandT = 0;
                    _autoattackCounter++;

                    var spell = args.Target as Obj_AI_Base;

                    if (spell != null)
                    {
                        var target = spell;

                        if (target.IsValid)
                        {
                            FireOnTargetSwitch(target);
                            _lastTarget = target;
                        }
                    }               
                }
            }
            FireOnAttack(sender, _lastTarget);
        }

        internal static readonly Dictionary<int, Obj_AI_Base> LastTargetTurrets = new Dictionary<int, Obj_AI_Base>();

        private static void Obj_AI_Base_OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                var ping = Game.Ping;
                if (ping <= 30)
                {
                    Utility.DelayAction.Add(30 - ping, () => Obj_AI_Base_OnDoCast_Delayed(sender, args));
                    return;
                }

                Obj_AI_Base_OnDoCast_Delayed(sender, args);
            }
        }

        private static void Obj_AI_Base_OnDoCast_Delayed(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (IsAutoAttackReset(args.SData.Name))
            {
                ResetAutoAttackTimer();
            }

            if (IsAutoAttack(args.SData.Name))
            {
                FireAfterAttack(sender, args.Target as AttackableUnit);
                _missileLaunched = true;
            }
        }

        private static void SpellbookOnStopCast(Obj_AI_Base spellbook, SpellbookStopCastEventArgs args)
        {
            if (spellbook.IsValid && spellbook.IsMe && LastMoveCommandT - Game.Time > 0 && 
                (spellbook.IsRanged && !Player.CanAttack) && args.DestroyMissile && args.StopAnimation)
            {
                Console.WriteLine("AA Cancel" + Game.Time);
                //ResetAutoAttackTimer();
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs Spell)
        {
            if (unit.IsMe)
            {
                if (IsAutoAttackReset(Spell.SData.Name) && Spell.SData.SpellCastTime == 0)
                {
                    ResetAutoAttackTimer();
                }
            }

            if (!IsAutoAttack(Spell.SData.Name))
            {
                return;
            }
        }


        #region Delegates

        public delegate void AfterAttackEvenH(AttackableUnit unit, AttackableUnit target);
        public delegate void BeforeAttackEvenH(BeforeAttackEventArgs args);
        public delegate void OnAttackEvenH(AttackableUnit unit, AttackableUnit target);
        public delegate void OnNonKillableMinionH(AttackableUnit minion);
        public delegate void OnTargetChangeH(AttackableUnit oldTarget, AttackableUnit newTarget);

        #endregion

        #region Public Events

        public static event AfterAttackEvenH AfterAttack;
        public static event BeforeAttackEvenH BeforeAttack;
        public static event OnAttackEvenH OnAttack;
        public static event OnNonKillableMinionH OnNonKillableMinion;
        public static event OnTargetChangeH OnTargetChange;

        #endregion

        #region Enums

        public enum OrbwalkingMode
        {
            LastHit,
            Mixed,
            LaneClear,
            Combo,
            Flee,
            Burst,
            Freeze,
            CustomMode,
            None
        }

        #endregion

        #region Public Methods and Operators

        public static AIHeroClient GetBestHeroTarget
        {
            get
            {
                AIHeroClient killableObj = null;
                var hitsToKill = double.MaxValue;
                foreach (var obj in HeroManager.Enemies.Where(i => InAutoAttackRange(i)))
                {
                    var killHits = obj.Health / Player.GetAutoAttackDamage(obj, true);
                    if (killableObj != null && (killHits >= hitsToKill || obj.HasBuffOfType(BuffType.Invulnerability)))
                    {
                        continue;
                    }
                    killableObj = obj;
                    hitsToKill = killHits;
                }
                return hitsToKill < 4 ? killableObj : TargetSelector.GetTarget(-1, TargetSelector.DamageType.Physical);
            }
        }

        public static bool CanAttack()
        {
            switch (ObjectManager.Player.ChampionName)
            {
                case "Graves":
                    var attackDelay = 1.0740296828d * 1000 * ObjectManager.Player.AttackDelay - 716.2381256175d;

                    return Utils.GameTimeTickCount + Game.Ping / 2 + 25 >= LastAATick + attackDelay
                           && ObjectManager.Player.HasBuff("GravesBasicAttackAmmo1");

                case "Jhin":
                    if (ObjectManager.Player.HasBuff("JhinPassiveReload"))
                    {
                        return false;
                    }
                    break;

                case "Kalista":
                    if (ObjectManager.Player.IsDashingLS())
                    {
                        return false;
                    }
                    break;
            }

            if (ObjectManager.Player.IsCastingInterruptableSpell() && DisableAttackIfCastSpell)
            {
                return false;
            }

            return Utils.GameTimeTickCount + Game.Ping / 2 + 25 >= LastAATick + Player.AttackDelay * 1000;
        }

        public static bool CanMove1(float extraWindup, bool disableMissileCheck = false)
        {
            if (_missileLaunched && Orbwalker.MissileCheck && !disableMissileCheck)
            {
                return true;
            }

            var localExtraWindup = 0;

            if (ObjectManager.Player.ChampionName == "Jinx")
            {
                localExtraWindup = 100;
            }

            if (ObjectManager.Player.ChampionName == "Rengar" && (Player.HasBuff("rengarqbase") || Player.HasBuff("rengarqemp")))
            {
                localExtraWindup = 200;
            }

            return NoCancelChamps.Contains(ObjectManager.Player.ChampionName)
                   || (Utils.GameTimeTickCount + Game.Ping / 2
                       >= LastAATick + Player.AttackCastDelay * 1000 + extraWindup + localExtraWindup);
        }

        public static bool CanMove(float extraWindup, bool disableMissileCheck = false)
        {
            if (_missileLaunched && Orbwalker.MissileCheck && !disableMissileCheck)
            {
                return true;
            }

            if (ObjectManager.Player.ChampionName == "Kalista")
            {
                return true;
            }

            if (ObjectManager.Player.ChampionName == "Rengar" &&
                (ObjectManager.Player.HasBuff("rengarqbase") || ObjectManager.Player.HasBuff("rengarqemp")))
            {
                return Utils.GameTimeTickCount + Game.Ping / 2
                       >= LastAATick + ObjectManager.Player.AttackCastDelay * 1000 + extraWindup + 200;
            }

            if (ObjectManager.Player.ChampionName == "Jinx")
            {
                return Utils.GameTimeTickCount + Game.Ping / 2
                       >= LastAATick + ObjectManager.Player.AttackCastDelay * 1000 + extraWindup + 100;
            }

            return Utils.GameTimeTickCount + Game.Ping / 2
                   >= LastAATick + ObjectManager.Player.AttackCastDelay * 1000 + extraWindup;
        }

        public static float GetAttackRange(AIHeroClient target)
        {
            var result = target.AttackRange + target.BoundingRadius;
            return result;
        }

        public static Vector3 GetLastMovePosition()
        {
            return LastMoveCommandPosition;
        }

        public static float GetLastMoveTime()
        {
            return LastMoveCommandT;
        }

        public static float GetMyProjectileSpeed()
        {
            if (ObjectManager.Player.IsMelee)
            {
                return float.MaxValue;
            }

            if (ObjectManager.Player.ChampionName == "Azir")
            {
                return float.MaxValue;
            }

            if (ObjectManager.Player.ChampionName == "Velkoz")
            {
                return float.MaxValue;
            }

            if (ObjectManager.Player.ChampionName == "Viktor"
                && ObjectManager.Player.HasBuff("ViktorPowerTransferReturn"))
            {
                return float.MaxValue;
            }
            return ObjectManager.Player.BasicAttack.MissileSpeed;
        }

        public static float GetRealAutoAttackRange(AttackableUnit target)
        {
            var result = ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius;

            if (target.IsValidTarget())
            {
                var aiBase = target as Obj_AI_Base;

                if (aiBase != null && ObjectManager.Player.ChampionName == "Caitlyn")
                {
                    if (aiBase.HasBuff("caitlynyordletrapinternal"))
                    {
                        result += 650;
                    }
                }

                return result + target.BoundingRadius;
            }

            return result;
        }

        public static bool InAutoAttackRange(AttackableUnit target)
        {
            if (!target.IsValidTarget())
            {
                return false;
            }
            var myRange = GetRealAutoAttackRange(target);
            var hero = target as AIHeroClient;

            if (hero != null)
            {
                return
                    Vector2.DistanceSquared(Prediction.GetPrediction(hero, 0).CastPosition.To2D(),
                        ObjectManager.Player.Position.To2D()) <= myRange * myRange;
            }
            var basetarget = target as Obj_AI_Base;

            return
                Vector2.DistanceSquared(
                    basetarget?.ServerPosition.To2D() ?? target.Position.To2D(),
                    ObjectManager.Player.ServerPosition.To2D()) <= myRange * myRange;
        }

        public static bool InAutoAttackRange(AttackableUnit target, float extraRange = 0, Vector3 from = new Vector3())
        {
            return target.IsValidTarget(GetAutoAttackRange(target) + extraRange, true, from);
        }

        private static float GetAutoAttackRange(Obj_AI_Base source, AttackableUnit target)
        {
            return source.AttackRange + source.BoundingRadius + (target.IsValidTarget() ? target.BoundingRadius : 0);
        }

        public static bool IsAutoAttack(string name)
        {
            return (name.ToLower().Contains("attack") && !NoAttacks.Contains(name.ToLower()))
                   || Attacks.Contains(name.ToLower());
        }

        public static bool IsAutoAttackReset(string name)
        {
            return AttackResets.Contains(name.ToLower());
        }

        public static float GetAutoAttackRange(AttackableUnit target = null)
        {
            return GetAutoAttackRange(Player, target);
        }

        public static bool IsMelee(this Obj_AI_Base unit)
        {
            return unit.CombatType == GameObjectCombatType.Melee;
        }

        public static void MoveTo(
            Vector3 position,
            float holdAreaRadius = 0,
            bool overrideTimer = false,
            bool randomizeMinDistance = true)
        {
            var playerPosition = ObjectManager.Player.ServerPosition;

            if (playerPosition.Distance(position, true) < holdAreaRadius * holdAreaRadius)
            {
                if (Player.Path.Length > 0)
                {
                    EloBuddy.Player.IssueOrder(GameObjectOrder.Stop, playerPosition);
                    LastMoveCommandPosition = playerPosition;
                    LastMoveCommandT = Utils.GameTimeTickCount - 70;
                }
                return;
            }

            var point = position;

            if (Player.Distance(point, true) < 150 * 150)
            {
                point = playerPosition.Extend(
                    position,
                    randomizeMinDistance ? (_random.NextFloat(0.6f, 1) + 0.2f) * _minDistance : _minDistance);
            }
            var angle = 0f;
            var currentPath = ObjectManager.Player.GetWaypoints();

            if (currentPath.Count > 1 && currentPath.PathLength() > 100)
            {
                var movePath = Player.GetPath(point);

                if (movePath.Length > 1)
                {
                    var v1 = currentPath[1] - currentPath[0];
                    var v2 = movePath[1] - movePath[0];
                    angle = v1.AngleBetween(v2.To2D());
                    var distance = movePath.Last().To2D().Distance(currentPath.Last(), true);

                    if ((angle < 10 && distance < 500 * 500) || distance < 50 * 50)
                    {
                        return;
                    }
                }
            }

            if (Utils.GameTimeTickCount - LastMoveCommandT < 70 + Math.Min(60, Game.Ping) && !overrideTimer
                && angle < 60)
            {
                return;
            }

            if (angle >= 60 && Utils.GameTimeTickCount - LastMoveCommandT < 60)
            {
                return;
            }

            EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo, point);

            LastMoveCommandPosition = point;
            LastMoveCommandT = Utils.GameTimeTickCount;
        }

        public static void Orbwalk(
            AttackableUnit target,
            Vector3 position,
            float extraWindup = 90,
            float holdAreaRadius = 0,
            bool randomizeMinDistance = true)
        {
            if (Utils.GameTimeTickCount - LastAttackCommandT < 70 + Math.Min(60, Game.Ping))
            {
                return;
            }

            try
            {
                if (target.IsValidTarget() && CanAttack() && Attack)
                {
                    DisableNextAttack = false;
                    FireBeforeAttack(target);

                    if (!DisableNextAttack)
                    {
                        if (ObjectManager.Player.ChampionName != "Kalista")
                        {
                            _missileLaunched = false;
                        }

                        if (EloBuddy.Player.IssueOrder(GameObjectOrder.AttackUnit, target))
                        {
                            LastAttackCommandT = Utils.GameTimeTickCount;
                            _lastTarget = target;

                           /*                             
                            EloBuddy.SDK.Core.DelayAction(
                                delegate
                                    {
                                        LastAATick = Utils.GameTimeTickCount - Game.Ping / 2;
                                        _missileLaunched = false;
                                        LastMoveCommandT = 0;
                                        _autoattackCounter++;
                                    }
                                , ((int)Player.AttackDelay * 100) + Game.Ping);
                                
                                */
                        }

                        return;
                    }
                }

                if (CanMove(extraWindup) && Move)
                {
                    if (Orbwalker.LimitAttackSpeed && (ObjectManager.Player.AttackDelay < 1 / 2.6f)
                        && _autoattackCounter % 3 != 0 && !CanMove(500, true))
                    {
                        return;
                    }

                    MoveTo(position, Math.Max(holdAreaRadius, 30), false, randomizeMinDistance);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        public static void ResetAutoAttackTimer()
        {
            LastAATick = 0;
        }

        public static void SetMinimumOrbwalkDistance(float d)
        {
            _minDistance = d;
        }

        public static void SetMovementDelay(int delay)
        {
            _delay = delay;
        }

        #endregion

        #region Methods
        private static void FireBeforeAttack(AttackableUnit target)
        {
            if (BeforeAttack != null)
            {
                BeforeAttack(new BeforeAttackEventArgs { Target = target });
            }
            else
            {
                DisableNextAttack = false;
            }
        }

        private static void FireAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (AfterAttack != null && target.IsValidTarget())
            {
                AfterAttack(unit, target);
            }
        }

        private static void FireOnAttack(AttackableUnit unit, AttackableUnit target)
        {
            OnAttack?.Invoke(unit, target);
        }

        private static void FireOnNonKillableMinion(AttackableUnit minion)
        {
            OnNonKillableMinion?.Invoke(minion);
        }

        private static void FireOnTargetSwitch(AttackableUnit newTarget)
        {
            if (OnTargetChange != null && (!_lastTarget.IsValidTarget() || _lastTarget != newTarget))
            {
                OnTargetChange(_lastTarget, newTarget);
            }
        }

        #endregion

        /// <summary>
        ///     The before attack event arguments.
        /// </summary>
        public class BeforeAttackEventArgs : EventArgs
        {
            public AttackableUnit Target;

            public Obj_AI_Base Unit = EloBuddy.Player.Instance;
            private bool _process = true;

            public bool Process
            {
                get
                {
                    return this._process;
                }
                set
                {
                    DisableNextAttack = !value;
                    this._process = value;
                }
            }
        }

        public class Orbwalker : IDisposable
        {
            private const float LaneClearWaitTimeMod = 2f;
            public static List<Orbwalker> Instances = new List<Orbwalker>();
            private static Menu _config;
            private readonly AIHeroClient Player;
            private Obj_AI_Base _forcedTarget;
            private OrbwalkingMode _mode = OrbwalkingMode.None;
            private Vector3 _orbwalkingPoint;
            private string CustomModeName;
            private Obj_AI_Minion _prevMinion;

            #region Constructors and Destructors

            public Orbwalker(Menu attachToMenu)
            {
                _config = attachToMenu;
                /* Drawings submenu */
                var drawings = new Menu("\u986f\u793a", "drawings");
                drawings.AddItem(
                    new MenuItem("AACircle", "\u81ea\u5df1 AA \u7bc4\u570d").SetShared()
                        .SetValue(new Circle(true, Color.FromArgb(155, 255, 255, 0))));
                drawings.AddItem(
                    new MenuItem("AACircle2", "\u6575\u4eba AA \u7bc4\u570d").SetShared()
                        .SetValue(new Circle(false, Color.FromArgb(155, 255, 255, 0))));
                drawings.AddItem(
                    new MenuItem("HoldZone", "\u505c\u6b62\u4e0d\u52d5\u5340\u7bc4\u570d").SetShared()
                        .SetValue(new Circle(true, Color.FromArgb(100, 255, 0))));
                drawings.AddItem(new MenuItem("AALineWidth", "\u986f\u793a\u7dda\u5bec\u5ea6")).SetShared().SetValue(new Slider(3, 1, 6));
                drawings.AddItem(new MenuItem("LastHitHelper", "\u958b\u555f\u81ea\u52d5\u88dc\u5200").SetShared().SetValue(false));
                _config.AddSubMenu(drawings);

                /* Misc options */
                var misc = new Menu("\u5176\u4ed6", "Misc");
                misc.AddItem(
                    new MenuItem("HoldPosRadius", "\u81ea\u5df1\u4f4d\u7f6e\u534a\u5f91").SetShared().SetValue(new Slider(150, 50, 250)));
                misc.AddItem(new MenuItem("PriorizeFarm", "\u512a\u5148\u88dc\u5175\u5f8c\u9a37\u64fe").SetShared().SetValue(true));
                misc.AddItem(new MenuItem("AttackWards", "\u81ea\u52d5\u653b\u64ca\u773c").SetShared().SetValue(false));
                misc.AddItem(new MenuItem("AttackPetsnTraps", "\u81ea\u52d5\u653b\u64ca \u5bf5\u7269").SetShared().SetValue(true));
                misc.AddItem(
                    new MenuItem("AttackGPBarrel", "\u81ea\u52d5\u653b\u64ca\u8239\u9577\u6876\u5b50").SetShared()
                        .SetValue(new StringList(new[] { "\u9023\u62db + \u6e05\u7dda", "\u6e05\u7dda", "No" }, 1)));
                misc.AddItem(new MenuItem("Smallminionsprio", "\u6e05\u91ce \u512a\u5148\u653b\u64ca\u5c0f\u602a").SetShared().SetValue(false));
                misc.AddItem(
                    new MenuItem("FocusMinionsOverTurrets", "\u96c6\u4e2d\u653b\u64ca\u5854\u4e0b\u5c0f\u5175").SetShared()
                        .SetValue(new KeyBind('M', KeyBindType.Toggle)));
                misc.AddItem(new MenuItem("LimitAttackSpeed", "\u653b\u64ca\u901f\u5ea6\u8d85\u904e 2.5 \u958b\u59cb\u98a8\u7b8f").SetShared().SetValue(false));
                misc.AddItem(new MenuItem("attackSpecialMinions", "\u653b\u64ca\u7279\u6b8a\u602a\u7269").SetShared().SetValue(true));
                misc.AddItem(new MenuItem("DisableAttackIfCastSpell", "\u7576\u65bd\u653e\u6280\u80fd\u6642\u7981\u6b62 AA").SetShared().SetValue(true))
                    .ValueChanged += DisableAttackIfCastSpellValueChanged;
                _config.AddSubMenu(misc);

                /* Missile check */
                _config.AddItem(new MenuItem("MissileCheck", "\u4f7f\u7528\u5f48\u9053\u6aa2\u67e5").SetShared().SetValue(true));

                var timeAdjust = new Menu("\u8d70\u780d\u5ef6\u9072\u8abf\u6574", "TimeAdjust");
                timeAdjust.AddItem(
                    new MenuItem("ExtraWindup", "\u984d\u5916 \u8d70\u780d\u5f8c\u6416 \u5ef6\u9072").SetShared().SetValue(new Slider(80, 0, 200)));
                timeAdjust.AddItem(new MenuItem("FarmDelay", "\u6e05\u7dda \u5ef6\u9072").SetShared().SetValue(new Slider(0, 0, 200)));

                _config.AddSubMenu(timeAdjust);

                /*Load the menu*/
                _config.AddItem(
                    new MenuItem("LastHit", "\u88dc\u5200").SetShared().SetValue(new KeyBind('X', KeyBindType.Press)));

                _config.AddItem(new MenuItem("Farm", "\u9a37\u64fe").SetShared().SetValue(new KeyBind('C', KeyBindType.Press)));

                _config.AddItem(
                    new MenuItem("Freeze", "\u63a7\u7dda").SetShared().SetValue(new KeyBind('N', KeyBindType.Press)));

                _config.AddItem(
                    new MenuItem("LaneClear", "\u6e05\u7dda").SetShared().SetValue(new KeyBind('V', KeyBindType.Press)));

                _config.AddItem(
                    new MenuItem("Orbwalk", "\u9023\u62db").SetShared().SetValue(new KeyBind(32, KeyBindType.Press)));

                if (ObjectManager.Player.ChampionName == "Graves" || ObjectManager.Player.ChampionName == "Vayne"
                    || ObjectManager.Player.ChampionName == "Yasuo" || ObjectManager.Player.ChampionName == "Riven")
                {
                    _config.AddItem(new MenuItem("Burst", "\u7206\u767c").SetShared().SetValue(new KeyBind('T', KeyBindType.Press)));
                }

                _config.AddItem(new MenuItem("Flee", "\u9003\u8dd1").SetShared().SetValue(new KeyBind('Z', KeyBindType.Press)));

                _config.AddItem(
                    new MenuItem("StillCombo", "\u9023\u62db\u4e0d\u52d5").SetShared()
                        .SetValue(new KeyBind('N', KeyBindType.Press)));

                DisableAttackIfCastSpell = _config.Item("DisableAttackIfCastSpell").GetValue<bool>();

                _config.Item("StillCombo").ValueChanged +=
                    (sender, args) => { Move = !args.GetNewValue<KeyBind>().Active; };

                this.Player = EloBuddy.Player.Instance;
                Game.OnTick += new GameTick(this.GameOnOnGameUpdate);
                Drawing.OnDraw += new DrawingDraw(this.DrawingOnOnDraw);
                Instances.Add(this);
            }

            #endregion

            #region Public Properties

            private void DisableAttackIfCastSpellValueChanged(object obj, OnValueChangeEventArgs Args)
            {
                DisableAttackIfCastSpell = Args.GetNewValue<bool>();
            }

            private void MoveChanged(object obj, OnValueChangeEventArgs Args)
            {
                Move = !Args.GetNewValue<KeyBind>().Active;
            }

            internal int FarmDelay => _config.Item("FarmDelay").GetValue<Slider>().Value;

            internal static bool MissileCheck => _config.Item("MissileCheck").GetValue<bool>();

            internal static bool LimitAttackSpeed => _config.Item("LimitAttackSpeed").GetValue<bool>();

            public OrbwalkingMode ActiveMode
            {
                get
                {
                    if (this._mode != OrbwalkingMode.None)
                    {
                        return this._mode;
                    }

                    if (_config.Item("Orbwalk").GetValue<KeyBind>().Active)
                    {
                        return OrbwalkingMode.Combo;
                    }

                    if (_config.Item("StillCombo").GetValue<KeyBind>().Active)
                    {
                        return OrbwalkingMode.Combo;
                    }

                    if (_config.Item("LaneClear").GetValue<KeyBind>().Active)
                    {
                        return OrbwalkingMode.LaneClear;
                    }

                    if (_config.Item("Farm").GetValue<KeyBind>().Active)
                    {
                        return OrbwalkingMode.Mixed;
                    }

                    if (_config.Item("Freeze").GetValue<KeyBind>().Active)
                    {
                        return OrbwalkingMode.Freeze;
                    }

                    if (_config.Item("LastHit").GetValue<KeyBind>().Active)
                    {
                        return OrbwalkingMode.LastHit;
                    }

                    if (_config.Item("Burst") != null && _config.Item("Burst").GetValue<KeyBind>().Active)
                    {
                        return OrbwalkingMode.Burst;
                    }

                    if (_config.Item("Flee").GetValue<KeyBind>().Active)
                    {
                        return OrbwalkingMode.Flee;
                    }

                    if (_config.Item(this.CustomModeName) != null && _config.Item(this.CustomModeName).GetValue<KeyBind>().Active)
                    {
                        return OrbwalkingMode.CustomMode;
                    }

                    return OrbwalkingMode.None;
                }
                set
                {
                    this._mode = value;
                }
            }

            #endregion

            #region Public Methods and Operators

            public void Dispose()
            {
                Menu.Remove(_config);
                Game.OnUpdate -= new GameUpdate(this.GameOnOnGameUpdate);
                Drawing.OnDraw -= new DrawingDraw(this.DrawingOnOnDraw);
                Instances.Remove(this);
            }

            public void ForceTarget(Obj_AI_Base target)
            {
                this._forcedTarget = target;
            }

            public virtual AttackableUnit GetTarget()
            {
                AttackableUnit result = null;
                var mode = this.ActiveMode;

                if (mode == OrbwalkingMode.Flee)
                {
                    return null;
                }

                if (this._forcedTarget.IsValidTarget() && this.InAutoAttackRange(this._forcedTarget))
                {
                    return this._forcedTarget;
                }

                if ((mode == OrbwalkingMode.Mixed || mode == OrbwalkingMode.LaneClear)
                    && !_config.Item("PriorizeFarm").GetValue<bool>())
                {
                    var target = TargetSelector.GetTarget(-1, TargetSelector.DamageType.Physical);

                    if (target != null && this.InAutoAttackRange(target))
                    {
                        return target;
                    }
                }

                //GankPlank barrels
                var attackGankPlankBarrels = _config.Item("AttackGPBarrel").GetValue<StringList>().SelectedIndex;

                if (attackGankPlankBarrels != 2)
                {
                    var condition = attackGankPlankBarrels == 0 && mode == OrbwalkingMode.Combo;

                    if (mode == OrbwalkingMode.LaneClear || mode == OrbwalkingMode.Mixed || mode == OrbwalkingMode.LastHit || mode == OrbwalkingMode.Freeze || condition)
                    {
                        var enemyGangPlank = EloBuddy.SDK.EntityManager.Heroes.Enemies.FirstOrDefault(e => e.Hero == Champion.Gangplank);

                        if (enemyGangPlank != null)
                        {
                            var barrels = ObjectManager.Get<Obj_AI_Base>().Where(minion => minion.CharData.BaseSkinName == "GangplankBarrel" && this.InAutoAttackRange(minion));

                            foreach (var barrel in barrels)
                            {
                                if (barrel.Health <= 1f)
                                {
                                    return barrel;
                                }

                                var t = (int)(this.Player.AttackCastDelay * 1000) + Game.Ping / 2 + 1000 * (int)Math.Max(0, this.Player.Distance(barrel) - this.Player.BoundingRadius) / (int)GetMyProjectileSpeed();

                                var barrelBuff =
                                    barrel.Buffs.FirstOrDefault(
                                        b =>
                                            b.Name.Equals("gangplankebarrelactive", StringComparison.InvariantCultureIgnoreCase));

                                if (barrelBuff != null && barrel.Health <= 2f)
                                {
                                    var healthDecayRate = enemyGangPlank.Level >= 13
                                                              ? 0.5f
                                                              : (enemyGangPlank.Level >= 7 ? 1f : 2f);
                                    var nextHealthDecayTime = Game.Time < barrelBuff.StartTime + healthDecayRate
                                                                  ? barrelBuff.StartTime + healthDecayRate
                                                                  : barrelBuff.StartTime + healthDecayRate * 2;

                                    if (nextHealthDecayTime <= Game.Time + t / 1000f)
                                    {
                                        return barrel;
                                    }
                                }
                            }

                            if (barrels.Any())
                            {
                                return null;
                            }
                        }
                    }
                }

                /*Killable Minion*/
                if (mode == OrbwalkingMode.LaneClear || mode == OrbwalkingMode.Mixed || mode == OrbwalkingMode.LastHit
                    || mode == OrbwalkingMode.Freeze)
                {
                    var MinionList =
                        EloBuddy.SDK.EntityManager.MinionsAndMonsters.EnemyMinions
                            .Where(minion => minion.IsValidTarget() && this.InAutoAttackRange(minion))
                            .OrderByDescending(minion => minion.CharData.BaseSkinName.Contains("Siege"))
                            .ThenBy(minion => minion.CharData.BaseSkinName.Contains("Super"))
                            .ThenBy(minion => minion.Health)
                            .ThenByDescending(minion => minion.MaxHealth);

                    foreach (var minion in MinionList)
                    {
                        var t = (int)(this.Player.AttackCastDelay * 1000) - 100 + Game.Ping / 2
                                + 1000 * (int)Math.Max(0, this.Player.Distance(minion) - this.Player.BoundingRadius)
                                / (int)GetMyProjectileSpeed();

                        if (mode == OrbwalkingMode.Freeze)
                        {
                            t += 200 + Game.Ping / 2;
                        }

                        var predHealth = HealthPrediction.GetHealthPrediction(minion, t, this.FarmDelay);

                        if (minion.Team != GameObjectTeam.Neutral && this.ShouldAttackMinion(minion))
                        {
                            var damage = this.Player.GetAutoAttackDamage(minion, true);
                            var killable = predHealth <= damage;

                            if (mode == OrbwalkingMode.Freeze)
                            {
                                if (minion.Health < 50 || predHealth <= 50)
                                {
                                    return minion;
                                }
                            }
                            else
                            {
                                if (predHealth <= 0)
                                {
                                    FireOnNonKillableMinion(minion);
                                }

                                if (killable)
                                {
                                    return minion;
                                }
                            }
                        }
                    }
                }

                /* turrets / inhibitors / nexus */
                if ((mode == OrbwalkingMode.LaneClear || mode == OrbwalkingMode.Mixed)
                    && (!_config.Item("FocusMinionsOverTurrets").GetValue<KeyBind>().Active
                        || !EloBuddy.SDK.EntityManager.MinionsAndMonsters.GetLaneMinions(EloBuddy.SDK.EntityManager.UnitTeam.Enemy,
                            EloBuddy.Player.Instance.Position,
                            GetRealAutoAttackRange(EloBuddy.Player.Instance)).Any()))
                {
                    /* turrets */
                    foreach (var turret in
                        ObjectManager.Get<Obj_AI_Turret>().Where(t => t.IsValidTarget() && this.InAutoAttackRange(t)))
                    {
                        return turret;
                    }

                    /* inhibitor */
                    foreach (var turret in
                        ObjectManager.Get<Obj_BarracksDampener>()
                            .Where(t => t.IsValidTarget() && this.InAutoAttackRange(t)))
                    {
                        return turret;
                    }

                    /* nexus */
                    foreach (var nexus in
                        ObjectManager.Get<Obj_HQ>().Where(t => t.IsValidTarget() && this.InAutoAttackRange(t)))
                    {
                        return nexus;
                    }
                }

                if (mode != OrbwalkingMode.LastHit)
                {
                    if (!EloBuddy.Player.Instance.UnderTurret(true) || mode == OrbwalkingMode.Combo)
                    {
                        var target = TargetSelector.GetTarget(-1, TargetSelector.DamageType.Physical);

                        if (target.IsValidTarget() && this.InAutoAttackRange(target))
                        {
                            return target;
                        }
                    }
                }
                
                /*Champions*/
                /*
                if (mode != OrbwalkingMode.LastHit && mode != OrbwalkingMode.Flee)
                {
                    if (mode != OrbwalkingMode.LaneClear || !this.ShouldWait())
                    {
                        var target = TargetSelector.GetTarget(-1, TargetSelector.DamageType.Physical);

                        if (target.IsValidTarget() && this.InAutoAttackRange(target))
                        {
                            return target;
                        }
                    }
                }*/


                /*Jungle minions*/
                if (mode == OrbwalkingMode.LaneClear || mode == OrbwalkingMode.Mixed)
                {
                    var jminions =
                        EloBuddy.SDK.EntityManager.MinionsAndMonsters.Monsters
                            .Where(
                                mob =>
                                    mob.IsValidTarget() && mob.Team == GameObjectTeam.Neutral && this.InAutoAttackRange(mob)
                                    && mob.CharData.BaseSkinName != "gangplankbarrel" && mob.Name != "WardCorpse"
                                    && !mob.BaseSkinName.Contains("Plant"));


                    result = _config.Item("Smallminionsprio").GetValue<bool>()
                                 ? jminions.MinOrDefault(mob => mob.MaxHealth)
                                 : jminions.MaxOrDefault(mob => mob.MaxHealth);

                    if (result != null && !result.IsDead)
                    {
                        return result;
                    }
                }

                /* UnderTurret Farming */
                if (mode == OrbwalkingMode.LaneClear || mode == OrbwalkingMode.Mixed || mode == OrbwalkingMode.LastHit
                    || mode == OrbwalkingMode.Freeze)
                {
                    var closestTower =
                        ObjectManager.Get<Obj_AI_Turret>()
                            .MinOrDefault(t => t.IsAlly && !t.IsDead ? this.Player.Distance(t, true) : float.MaxValue);

                    if (closestTower != null && this.Player.Distance(closestTower, true) < 1500 * 1500)
                    {
                        Obj_AI_Minion farmUnderTurretMinion = null;
                        Obj_AI_Minion noneKillableMinion = null;

                        // return all the minions underturret in auto attack range
                        /*
                        var minions =
                            EloBuddy.SDK.EntityManager.MinionsAndMonsters.GetLaneMinions(EloBuddy.SDK.EntityManager.UnitTeam.Enemy, this.Player.Position, this.Player.AttackRange + 200)
                                .Where(
                                    minion =>
                                        this.InAutoAttackRange(minion) && closestTower.Distance(minion, true) < 900 * 900)
                                .OrderByDescending(minion => minion.CharData.BaseSkinName.Contains("Siege"))
                                .ThenBy(minion => minion.CharData.BaseSkinName.Contains("Super"))
                                .ThenByDescending(minion => minion.MaxHealth)
                                .ThenByDescending(minion => minion.Health);*/

                        var minions = MinionListAA.Where(minion =>
                            closestTower.Distance(minion, true) < 900 * 900)
                            .OrderByDescending(minion => minion.CharData.BaseSkinName.Contains("Siege"))
                            .ThenBy(minion => minion.CharData.BaseSkinName.Contains("Super"))
                            .ThenByDescending(minion => minion.MaxHealth)
                            .ThenByDescending(minion => minion.Health);

                        if (minions.Any())
                        {
                            // get the turret aggro minion
                            var turretMinion =
                                minions.FirstOrDefault(
                                    minion =>
                                        minion is Obj_AI_Minion && HealthPrediction.HasTurretAggro(minion as Obj_AI_Minion));

                            if (turretMinion != null)
                            {
                                var hpLeftBeforeDie = 0;
                                var hpLeft = 0;
                                var turretAttackCount = 0;
                                var turretStarTick = HealthPrediction.TurretAggroStartTick(
                                    turretMinion as Obj_AI_Minion);
                                // from healthprediction (don't blame me :S)
                                var turretLandTick = turretStarTick + (int)(closestTower.AttackCastDelay * 1000)
                                                     + 1000
                                                     * Math.Max(
                                                         0,
                                                         (int)
                                                         (turretMinion.Distance(closestTower)
                                                          - closestTower.BoundingRadius))
                                                     / (int)(closestTower.BasicAttack.MissileSpeed + 70);

                                // calculate the HP before try to balance it
                                for (float i = turretLandTick + 50;
                                     i < turretLandTick + 10 * closestTower.AttackDelay * 1000 + 50;
                                     i = i + closestTower.AttackDelay * 1000)
                                {
                                    var time = (int)i - Utils.GameTimeTickCount + Game.Ping / 2;
                                    var predHP =
                                        (int)
                                        HealthPrediction.LaneClearHealthPrediction(turretMinion, time > 0 ? time : 0);
                                    if (predHP > 0)
                                    {
                                        hpLeft = predHP;
                                        turretAttackCount += 1;
                                        continue;
                                    }
                                    hpLeftBeforeDie = hpLeft;
                                    hpLeft = 0;
                                    break;
                                }
                                // calculate the hits is needed and possibilty to balance
                                if (hpLeft == 0 && turretAttackCount != 0 && hpLeftBeforeDie != 0)
                                {
                                    var damage = (int)this.Player.GetAutoAttackDamage(turretMinion, true);
                                    var hits = hpLeftBeforeDie / damage;
                                    var timeBeforeDie = turretLandTick
                                                        + (turretAttackCount + 1)
                                                        * (int)(closestTower.AttackDelay * 1000)
                                                        - Utils.GameTimeTickCount;
                                    var timeUntilAttackReady = LastAATick + (int)(this.Player.AttackDelay * 1000)
                                                               > Utils.GameTimeTickCount + Game.Ping / 2 + 25
                                                                   ? LastAATick + (int)(this.Player.AttackDelay * 1000)
                                                                     - (Utils.GameTimeTickCount + Game.Ping / 2 + 25)
                                                                   : 0;
                                    var timeToLandAttack = this.Player.IsMelee
                                                               ? this.Player.AttackCastDelay * 1000
                                                               : this.Player.AttackCastDelay * 1000
                                                                 + 1000
                                                                 * Math.Max(
                                                                     0,
                                                                     turretMinion.Distance(this.Player)
                                                                     - this.Player.BoundingRadius)
                                                                 / this.Player.BasicAttack.MissileSpeed;
                                    if (hits >= 1
                                        && hits * this.Player.AttackDelay * 1000 + timeUntilAttackReady
                                        + timeToLandAttack < timeBeforeDie)
                                    {
                                        farmUnderTurretMinion = turretMinion as Obj_AI_Minion;
                                    }
                                    else if (hits >= 1
                                             && hits * this.Player.AttackDelay * 1000 + timeUntilAttackReady
                                             + timeToLandAttack > timeBeforeDie)
                                    {
                                        noneKillableMinion = turretMinion as Obj_AI_Minion;
                                    }
                                }
                                else if (hpLeft == 0 && turretAttackCount == 0 && hpLeftBeforeDie == 0)
                                {
                                    noneKillableMinion = turretMinion as Obj_AI_Minion;
                                }
                                // should wait before attacking a minion.
                                if (this.ShouldWaitUnderTurret(noneKillableMinion))
                                {
                                    return null;
                                }
                                if (farmUnderTurretMinion != null)
                                {
                                    return farmUnderTurretMinion;
                                }
                                // balance other minions
                                foreach (var minion in
                                    minions.Where(
                                        x =>
                                            x.NetworkId != turretMinion.NetworkId && x is Obj_AI_Minion
                                            && !HealthPrediction.HasMinionAggro(x as Obj_AI_Minion)))
                                {
                                    var playerDamage = (int)this.Player.GetAutoAttackDamage(minion);
                                    var turretDamage = (int)closestTower.GetAutoAttackDamage(minion, true);
                                    var leftHP = (int)minion.Health % turretDamage;
                                    if (leftHP > playerDamage)
                                    {
                                        return minion;
                                    }
                                }
                                // late game
                                var lastminion =
                                    minions.LastOrDefault(
                                        x =>
                                            x.NetworkId != turretMinion.NetworkId && x is Obj_AI_Minion
                                            && !HealthPrediction.HasMinionAggro(x as Obj_AI_Minion));
                                if (lastminion != null && minions.Count() >= 2)
                                {
                                    if (1f / this.Player.AttackDelay >= 1f
                                        && (int)(turretAttackCount * closestTower.AttackDelay / this.Player.AttackDelay)
                                        * this.Player.GetAutoAttackDamage(lastminion) > lastminion.Health)
                                    {
                                        return lastminion;
                                    }
                                    if (minions.Count() >= 5 && 1f / this.Player.AttackDelay >= 1.2)
                                    {
                                        return lastminion;
                                    }
                                }
                            }
                            else
                            {
                                if (this.ShouldWaitUnderTurret(noneKillableMinion))
                                {
                                    return null;
                                }
                                // balance other minions
                                foreach (var minion in
                                    minions.Where(
                                        x => x is Obj_AI_Minion && !HealthPrediction.HasMinionAggro(x as Obj_AI_Minion)))                               
                                {
                                    var playerDamage = (int)this.Player.GetAutoAttackDamage(minion);
                                    var turretDamage = (int)closestTower.GetAutoAttackDamage(minion, true);
                                    var leftHP = (int)minion.Health % turretDamage;

                                    if (leftHP > playerDamage)
                                    {
                                        return minion;
                                    }
                                }

                                //late game
                                var lastminion =
                                    minions.LastOrDefault(
                                        x => x is Obj_AI_Minion && !HealthPrediction.HasMinionAggro(x as Obj_AI_Minion));
                                if (lastminion != null && minions.Count() >= 2)
                                {
                                    if (minions.Count() >= 5 && 1f / this.Player.AttackDelay >= 1.2)
                                    {
                                        return lastminion;
                                    }
                                }
                            }
                            return null;
                        }
                    }
                }

                /*Lane Clear minions*/
                if (mode == OrbwalkingMode.LaneClear)
                {
                    if (!this.ShouldWait())
                    {
                        if (this._prevMinion.IsValidTarget() && this.InAutoAttackRange(this._prevMinion))
                        {
                            var predHealth = HealthPrediction.LaneClearHealthPrediction(
                                this._prevMinion,
                                (int)(this.Player.AttackDelay * 1000 * LaneClearWaitTimeMod),
                                this.FarmDelay);
                            if (predHealth >= 2 * this.Player.GetAutoAttackDamage(this._prevMinion)
                                || Math.Abs(predHealth - this._prevMinion.Health) < float.Epsilon)
                            {
                                return this._prevMinion;
                            }
                        }

                        /*
                        var results = (from minion in
                                       ObjectManager.Get<Obj_AI_Minion>()
                                           .Where(
                                               minion =>
                                                   minion.IsValidTarget() && this.InAutoAttackRange(minion)
                                                   && this.ShouldAttackMinion(minion) && !minion.CharData.BaseSkinName.Contains("Plant"))
                                       let predHealth =
                                       HealthPrediction.LaneClearHealthPrediction(
                                           minion,
                                           (int)(this.Player.AttackDelay * 1000 * LaneClearWaitTimeMod),
                                           this.FarmDelay)
                                       where
                                       predHealth >= 2 * this.Player.GetAutoAttackDamage(minion)
                                       || Math.Abs(predHealth - minion.Health) < float.Epsilon
                                       select minion);*/


                        result = (from minion in
                                  MinionListAA.Where(this.ShouldAttackMinion).Where(m => !m.CharData.BaseSkinName.Contains("Plant"))
                                  let predHealth = HealthPrediction.LaneClearHealthPrediction(minion,
                                      (int)(this.Player.AttackDelay * 1000 * LaneClearWaitTimeMod), this.FarmDelay)
                                  where predHealth >= 2 * this.Player.GetAutoAttackDamage(minion)
                                        || Math.Abs(predHealth - minion.Health) < float.Epsilon
                                  select minion).MaxOrDefault(m => m.Health);


                        //result = results.MaxOrDefault(m => !MinionManager.IsMinion(m, true) ? float.MaxValue : m.Health);

                        if (result != null && !result.IsDead)
                        {
                            this._prevMinion = (Obj_AI_Minion)result;
                        }
                    }
                }

                return result;
            }

            public virtual bool InAutoAttackRange(AttackableUnit target)
            {
                return Orbwalking.InAutoAttackRange(target);
            }

            public virtual void RegisterCustomMode(string name, string displayname, uint key)
            {
                this.CustomModeName = name;
                if (_config.Item(name) == null)
                {
                    _config.AddItem(
                        new MenuItem(name, displayname).SetShared().SetValue(new KeyBind(key, KeyBindType.Press)));
                }
            }

            public void SetAttack(bool b)
            {
                Attack = b;
            }

            public void SetMovement(bool b)
            {
                Move = b;
            }

            public void SetOrbwalkingPoint(Vector3 point)
            {
                this._orbwalkingPoint = point;
            }

            public bool ShouldWait()
            {
                return
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Any(
                            minion =>
                                minion.IsValidTarget() && minion.Team != GameObjectTeam.Neutral
                                && this.InAutoAttackRange(minion) && MinionManager.IsMinion(minion, false)
                                && HealthPrediction.LaneClearHealthPrediction(
                                    minion,
                                    (int)(this.Player.AttackDelay * 1000 * LaneClearWaitTimeMod),
                                    this.FarmDelay) <= this.Player.GetAutoAttackDamage(minion));
            }

            #endregion

            #region Methods

            private void DrawingOnOnDraw(EventArgs args)
            {
                if (_config.Item("AACircle").GetValue<Circle>().Active)
                {
                    Render.Circle.DrawCircle(
                        this.Player.Position,
                        GetRealAutoAttackRange(null) + 65,
                        _config.Item("AACircle").GetValue<Circle>().Color,
                        _config.Item("AALineWidth").GetValue<Slider>().Value);
                }
                if (_config.Item("AACircle2").GetValue<Circle>().Active)
                {
                    foreach (var target in
                        HeroManager.Enemies.FindAll(target => target.IsValidTarget(1175)))
                    {
                        Render.Circle.DrawCircle(
                            target.Position,
                            GetAttackRange(target),
                            _config.Item("AACircle2").GetValue<Circle>().Color,
                            _config.Item("AALineWidth").GetValue<Slider>().Value);
                    }
                }

                if (_config.Item("HoldZone").GetValue<Circle>().Active)
                {
                    Render.Circle.DrawCircle(
                        this.Player.Position,
                        _config.Item("HoldPosRadius").GetValue<Slider>().Value,
                        _config.Item("HoldZone").GetValue<Circle>().Color,
                        _config.Item("AALineWidth").GetValue<Slider>().Value,
                        true);
                }

                if (_config.Item("LastHitHelper").GetValue<bool>())
                {
                    foreach (var minion in
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Where(
                                x => x.Name.ToLower().Contains("minion") && x.IsHPBarRendered && x.IsValidTarget(1000)))
                    {
                        if (minion.Health < ObjectManager.Player.GetAutoAttackDamage(minion, true))
                        {
                            Render.Circle.DrawCircle(minion.Position, 50, Color.LimeGreen);
                        }
                    }
                }
            }

            private void GameOnOnGameUpdate(EventArgs args)
            {
                try
                {                     
                    if (this.ActiveMode == OrbwalkingMode.None)
                    {
                        return;
                    }

                    //Prevent canceling important spells
                    if (this.Player.IsCastingInterruptableSpell(true))
                    {
                        return;
                    }

                    MinionListAA = MinionCacheManager.GetMinions(this.Player.Position, 0);

                    var target = this.GetTarget();

                    Orbwalk(
                        target,
                        this._orbwalkingPoint.To2D().IsValid() ? this._orbwalkingPoint : Game.CursorPos,
                        _config.Item("ExtraWindup").GetValue<Slider>().Value,
                        Math.Max(_config.Item("HoldPosRadius").GetValue<Slider>().Value, 30));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            private bool ShouldAttackMinion(Obj_AI_Base minion)
            {
                var attackSpecialMinions = _config.Item("attackSpecialMinions").GetValue<bool>();
                //var prioritizeSpecialMinions = _config.Item("prioritizeSpecialMinions").GetValue<bool>();

                foreach (var min in EloBuddy.SDK.EntityManager.MinionsAndMonsters.OtherEnemyMinions.Where(x => IsValidUnit(x)))
                {
                    if (min.CharData.BaseSkinName.ToLower() == "zyrathornplant"
                        || min.CharData.BaseSkinName.ToLower() == "zyragraspingplant"
                        || min.CharData.BaseSkinName.ToLower() == "heimertyellow"
                        || min.CharData.BaseSkinName.ToLower() == "heimertblue"
                        || min.CharData.BaseSkinName.ToLower() == "malzaharvoidling"
                        || min.CharData.BaseSkinName.ToLower() == "yorickdecayedghoul"
                        || min.CharData.BaseSkinName.ToLower() == "yorickravenousghoul"
                        || min.CharData.BaseSkinName.ToLower() == "yorickspectralghoul"
                        || min.CharData.BaseSkinName.ToLower() == "shacobox"
                        || min.CharData.BaseSkinName.ToLower() == "annietibbers"
                        || min.CharData.BaseSkinName.ToLower() == "teemomushroom"
                        || min.CharData.BaseSkinName.ToLower() == "elisespiderling")
                    {
                        if (min.IsMinion)
                        {
                            return attackSpecialMinions;
                        }
                    }
                }

                if (minion.Name == "WardCorpse" || minion.CharData.BaseSkinName.ToLower() == "jarvanivstandard")
                {
                    return false;
                }

                if (MinionManager.IsWard((Obj_AI_Minion)minion))
                {
                    return _config.Item("AttackWards").IsActive();
                }

                return (_config.Item("AttackPetsnTraps").GetValue<bool>() || MinionManager.IsMinion((Obj_AI_Minion)minion))
                       && minion.CharData.BaseSkinName != "gangplankbarrel";
            }

            private static bool IsValidUnit(AttackableUnit unit, float range = 0f)
            {
                return unit.IsValidTarget()
                       && unit.Distance(ObjectManager.Player) < (range > 0 ? range : GetRealAutoAttackRange(unit));
            }

            private bool ShouldWaitUnderTurret(Obj_AI_Minion noneKillableMinion)
            {
                return
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Any(
                            minion =>
                                (noneKillableMinion == null || noneKillableMinion.NetworkId != minion.NetworkId)
                                && minion.IsValidTarget() && minion.Team != GameObjectTeam.Neutral
                                && this.InAutoAttackRange(minion) && MinionManager.IsMinion(minion)
                                && HealthPrediction.LaneClearHealthPrediction(
                                    minion,
                                    (int)
                                    (this.Player.AttackDelay * 1000
                                     + (this.Player.IsMelee
                                            ? this.Player.AttackCastDelay * 1000
                                            : this.Player.AttackCastDelay * 1000
                                              + 1000 * (this.Player.AttackRange + 2 * this.Player.BoundingRadius)
                                              / this.Player.BasicAttack.MissileSpeed)),
                                    this.FarmDelay) <= this.Player.GetAutoAttackDamage(minion));
            }

            #endregion
        }
    }
}

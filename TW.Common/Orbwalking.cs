namespace TW.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EloBuddy;
    using SharpDX;
    using Extensions;
    using Color = System.Drawing.Color;

    public static class Orbwalking
    {
        #region Static Fields

        public static int[] LastTargets = new int[] {0,0,0};
        public static bool Attack = true;
        public static bool isOnBasicAttackBroken = false;
        public static bool DisableNextAttack;
        public static int LastAATick;
        public static int LastAttackCommandT;
        public static Vector3 LastMoveCommandPosition = Vector3.Zero;
        public static int LastMoveCommandT;
        public static bool Move = true;
        private static readonly string _championName;
        private static readonly Random _random = new Random(DateTime.Now.Millisecond);
        private static readonly string[] AttackResets =
            {
                "dariusnoxiantacticsonh", "fiorae", "garenq", "gravesmove", "hecarimrapidslash", "jaxempowertwo",
                "leonashieldofdaybreak", "luciane", "monkeykingdoubleattack", "mordekaisermaceofspades", "nasusq",
                "nautiluspiercinggaze", "netherblade", "gangplankqwrapper", "powerfist", "renektonpreexecute",
                "rengarq", "shyvanadoubleattack", "sivirw", "takedown", "talonnoxiandiplomacy", "trundletrollsmash",
                "vaynetumble", "vie", "volibearq", "xenzhaocombotarget", "yorickspectral", "reksaiq",
                "itemtitanichydracleave", "masochism", "illaoiw", "elisespiderw", "fiorae", "meditate",
                "sejuaninorthernwinds", "camilleq", "camilleq2"
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
        private static readonly string[] spellname = { "LucianR", "VarusQ", "ViQ", "VladimirE", "XerathArcanopulseChargeUp" };
        private static readonly string[] NoCancelChamps = { "Kalista" };
        private static readonly AIHeroClient Player;
        private static int _autoattackCounter;
        private static int _delay;
        private static AttackableUnit _lastTarget;
        private static float _minDistance = 400;
        private static bool _missileLaunched;
        public static List<Obj_AI_Minion> AzirSoliders = new List<Obj_AI_Minion>();
        public static List<Obj_AI_Base> MinionListAA = new List<Obj_AI_Base>();
        private static int TimeAdjust = 0;
        private static int BrainFarmInt = -90;
        private static int DelayOnFire = 0;
        private static int DelayOnFireId = 0;
        private static int CountEnemy = 0;
        public static int LastMove;
        public static int NextMovementDelay;
        public static Vector3 LastMovementPosition = Vector3.Zero;

        #endregion

        static Orbwalking()
        {
            Player = ObjectManager.Player;
            _championName = Player.ChampionName;
            //Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
            //Obj_AI_Base.OnSpellCast += new Obj_AI_BaseDoCastSpell(Obj_AI_Base_OnDoCast);
            //Spellbook.OnStopCast += SpellbookOnStopCast;
            Obj_AI_Base.OnPlayAnimation += Obj_AI_Base_OnPlayAnimation;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Spellbook.OnStopCast += OnStopCast;
            Obj_AI_Base.OnSpellCast += OnSpellCast;
            EloBuddy.Player.OnIssueOrder += Player_OnIssueOrder;

            if (_championName == "Rengar")
            {
                Obj_AI_Base.OnPlayAnimation += delegate(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
                    {
                        if (sender.IsMe && args.Animation == "Spell5")
                        {
                            var t = 0;

                            if (_lastTarget != null && _lastTarget.IsValid)
                            {
                                t += (int)Math.Min(ObjectManager.Player.Distance(_lastTarget) / 1.5f, 0.6f);
                            }

                            LastAATick = Utils.GameTimeTickCount - Game.Ping / 2 + t;
                        }
                    };
            }

            if (_championName == "Azir")
            {
                AzirSoliders = ObjectManager.Get<Obj_AI_Minion>().Where(
                    x
                    => x.IsAlly && x.Name == "AzirSoldier" && x.HasBuff("azirwspawnsound")).ToList();
                GameObject.OnCreate += OnCreate;
                GameObject.OnDelete += OnDelete;
            }
        }

        private static void Player_OnIssueOrder(Obj_AI_Base sender, PlayerIssueOrderEventArgs args)
        {
            var senderValid = sender != null && sender.IsValid && sender.IsMe;

            if (!senderValid || args.Order != GameObjectOrder.MoveTo)
            {
                return;
            }
            if (LastMovementPosition != Vector3.Zero && args.TargetPosition.Distance(LastMovementPosition) < 300)
            {
                if (NextMovementDelay == 0)
                {
                    var min = 80;
                    var max = 250;
                    NextMovementDelay = min > max ? min : WeightedRandom.Next(min, max);
                }

                if ((Utils.TickCount - LastMove) < NextMovementDelay)
                {
                    NextMovementDelay = 0;
                    args.Process = false;
                    return;
                }

                var wp = ObjectManager.Player.GetWaypoints();

                if (args.TargetPosition.Distance(Player.ServerPosition) < 50)
                {
                    args.Process = false;
                    return;
                }
            }

            LastMovementPosition = args.TargetPosition;
            LastMove = Utils.TickCount;
        }

        private static void OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
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
    
        private static void OnStopCast(Obj_AI_Base spellbook, SpellbookStopCastEventArgs args)
        {
            if (spellbook.IsValid && EloBuddy.SDK.Orbwalker.IsRanged && !EloBuddy.SDK.Orbwalker.CanBeAborted && spellbook.IsMe && args.DestroyMissile && args.StopAnimation)
            {
                Console.WriteLine("AA Cancel" + Game.Time);

                ResetAutoAttackTimer();
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs Spell)
        {
            var spellName = Spell.SData.Name;
            var attackdelay = unit.AttackCastDelay * 1000f + 30f;

            if (unit.IsMe && IsAutoAttackReset(spellName))
            {
                if (Spell.Target.NetworkId == DelayOnFireId)
                {
                    DelayOnFire = Utils.GameTimeTickCount;
                }

                if (Spell.Target is Obj_AI_Base || Spell.Target is Obj_BarracksDampener || Spell.Target is Obj_HQ)
                {
                    LastAATick = Utils.GameTimeTickCount - Game.Ping / 2;
                    LastMoveCommandT = 0;
                    _autoattackCounter++;

                    if (Spell.Target is Obj_AI_Base)
                    {
                        var enemy = (Obj_AI_Base)Spell.Target;

                        if (enemy.IsValid)
                        {
                            FireOnTargetSwitch(enemy);
                            _lastTarget = enemy;
                        }
                    }
                }

                if (unit.IsMelee)
                {
                    Utility.DelayAction.Add((int)attackdelay, () => OnProcessSpellCast(unit, Spell));
                }
            }

            FireOnAttack(unit, _lastTarget);
        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            if (!(sender is Obj_AI_Minion))
                return;

            if (sender.Name == "AzirSoldier" && sender.IsAlly)
            {
                var soldier = (Obj_AI_Minion)sender;

                if (soldier.BaseSkinName == "AzirSoldier")
                    AzirSoliders.Add(soldier);
            }
        }

        private static void OnDelete(GameObject sender, EventArgs args)
        {
            AzirSoliders.RemoveAll(s => s.NetworkId == sender.NetworkId);
        }

        private static void Obj_AI_Base_OnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.Animation == "Attack1" ||
                    args.Animation == "Crit" ||
                    args.Animation == "Attack2" ||
                    args.AnimationHash.ToString("x8") == "d7d89ccc" || // kali - passive
                    (args.AnimationHash.ToString("x8") == "a75d185e" &&
                    (sender.Name.ToLower().Contains("master")) ||
                    sender.Name.ToLower().Contains("lucian")) || // universal passive hash
                    args.AnimationHash.ToString("x8") == "58b1ec4a" || // yasuo - aa1
                    args.AnimationHash.ToString("x8") == "53b1e46b" || // yasuo - aa2
                    args.AnimationHash.ToString("x8") == "730fbce4" || // yasuo - aa3
                    (args.AnimationHash.ToString("x8") == "1419ad45" && sender.Name.ToLower().Contains("kog")
                    && sender.Name.ToLower().ToLower().Contains("maw")) ||
                    (args.AnimationHash.ToString("x8") == "b7f64047" && sender.Name.ToLower().Contains("twitch")) // twitch - R
                )
                {
                    bool mbuff = Orbwalking.Player.HasBuff("vaynetumblebonus");

                    if (mbuff)
                    {
                        Orbwalking.LastAATick = 0;
                    }
                    else
                    {
                        Orbwalking.LastAATick = Utils.GameTimeTickCount - Game.Ping / 2;
                    }

                    _missileLaunched = false;
                    LastMoveCommandT = 0;
                    _autoattackCounter++;
                }
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

        public static bool CanAttack()
        {
            if (Player.ChampionName == "Graves")
            {
                var attackDelay = 1.0740296828d * 1000 * Player.AttackDelay - 716.2381256175d;
                if (Utils.GameTimeTickCount + Game.Ping / 2 + 25 >= LastAATick + attackDelay
                    && Player.HasBuff("GravesBasicAttackAmmo1"))
                {
                    return true;
                }

                return false;
            }

            if (Player.ChampionName == "Jhin")
            {
                if (Player.HasBuff("JhinPassiveReload"))
                {
                    return false;
                }
            }

            if (Player.ChampionName == "Kalista")
            {
                if (Player.IsDashingLS())
                {
                    return false;
                }
            }

            if (Player.ChampionName == "Darius")
            {
                if (Player.HasBuff("dariusqcast"))
                {
                    return false;
                }
            }

            if (Player.IsCastingInterruptableSpell())
            {
                return false;
            }

            return Utils.GameTimeTickCount + Game.Ping / 2 + 25 >= LastAATick + Player.AttackDelay * 1000;
        }

        public static bool CanMove(float extraWindup, bool disableMissileCheck = false)
        {           
            if (_missileLaunched && Orbwalker.MissileCheck && !disableMissileCheck)
            {
                return true;
            }

            var localExtraWindup = 0;
            if (_championName == "Rengar" && (Player.HasBuff("rengarqbase") || Player.HasBuff("rengarqemp")))
            {
                localExtraWindup = 200;
            }

            return NoCancelChamps.Contains(_championName)
                   || (Utils.GameTimeTickCount + Game.Ping / 2
                       >= LastAATick + Player.AttackCastDelay * 1000 + extraWindup + localExtraWindup);
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
            return IsMelee(Player) || _championName == "Azir" || _championName == "Velkoz"
                   || _championName == "Viktor" && Player.HasBuff("ViktorPowerTransferReturn")
                       ? float.MaxValue
                       : Player.BasicAttack.MissileSpeed;
        }

        public static float GetRealAutoAttackRange(AttackableUnit target)
        {
            var result = Player.AttackRange + Player.BoundingRadius;
            if (target.IsValidTarget())
            {
                var aiBase = target as Obj_AI_Base;
                if (aiBase != null && Player.ChampionName == "Caitlyn")
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
            return
                Vector2.DistanceSquared(
                    target is Obj_AI_Base ? ((Obj_AI_Base)target).ServerPosition.To2D() : target.Position.To2D(),
                    Player.ServerPosition.To2D()) <= myRange * myRange;
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

        public static bool IsMelee(this Obj_AI_Base unit)
        {
            return unit.CombatType == GameObjectCombatType.Melee;
        }

        public static void MoveTo(
            Vector3 position,
            float holdAreaRadius = 0,
            bool overrideTimer = false,
            bool useFixedDistance = true,
            bool randomizeMinDistance = true)
        {
            var playerPosition = Player.ServerPosition;

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
            var currentPath = Player.GetWaypoints();
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
            bool useFixedDistance = true,
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
                        if (!NoCancelChamps.Contains(_championName))
                        {
                            _missileLaunched = false;
                        }

                        if (EloBuddy.Player.IssueOrder(GameObjectOrder.AttackUnit, target))
                        {
                            LastAttackCommandT = Utils.GameTimeTickCount;
                            _lastTarget = target;
                        }

                        return;
                    }
                }

                if (CanMove(extraWindup) && Move)
                {
                    MoveTo(position, Math.Max(holdAreaRadius, 30), false, useFixedDistance, randomizeMinDistance);
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

        private static void PushLastTargets(int networkId)
        {
            LastTargets[2] = LastTargets[1];
            LastTargets[1] = LastTargets[0];
            LastTargets[0] = networkId;
        }

        private static void FireAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (AfterAttack != null && target.IsValidTarget())
            {
                AfterAttack(unit, target);
            }
        }

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

        private static void FireOnAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (OnAttack != null)
            {
               OnAttack(unit, target);
            }
        }

        private static void FireOnNonKillableMinion(AttackableUnit minion)
        {
            if (OnNonKillableMinion != null)
            {
                OnNonKillableMinion(minion);
            }
        }

        private static void FireOnTargetSwitch(AttackableUnit newTarget)
        {
            if (OnTargetChange != null && (!_lastTarget.IsValidTarget() || _lastTarget != newTarget))
            {
                OnTargetChange(_lastTarget, newTarget);
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

        private static void OnProcessSpell(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs Spell)
        {
            try
            {
                if (unit.IsMe)
                {
                    var spellName = Spell.SData.Name;

                    if (IsAutoAttackReset(spellName) && Spell.SData.SpellCastTime == 0)
                        ResetAutoAttackTimer();

                    if (!IsAutoAttack(spellName))
                        return;
                    
                    if (Spell.Target is Obj_AI_Base || Spell.Target is Obj_BarracksDampener || Spell.Target is Obj_HQ)
                    {
                        PushLastTargets(Spell.Target.NetworkId);

                        LastAATick = Utils.GameTimeTickCount - Game.Ping / 2;
                        _missileLaunched = false;
                        LastMoveCommandT = 0;
                        _autoattackCounter++;

                        if (Spell.Target is Obj_AI_Base)
                        {
                            var target = (Obj_AI_Base)Spell.Target;
                            if (target.IsValid)
                            {
                                FireOnTargetSwitch(target);
                                _lastTarget = target;
                            }
                        }
                    }
                }
                FireOnAttack(unit, _lastTarget);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        /// <summary>
        ///     The before attack event arguments.
        /// </summary>
        public class BeforeAttackEventArgs : EventArgs
        {
            public AttackableUnit Target;
            public Obj_AI_Base Unit = ObjectManager.Player;
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
            #region Constants

            /// <summary>
            ///     The lane clear wait time modifier.
            /// </summary>
            private const float LaneClearWaitTimeMod = 2f;

            #endregion

            #region Static Fields

            /// <summary>
            ///     The instances of the orbwalker.
            /// </summary>
            public static List<Orbwalker> Instances = new List<Orbwalker>();

            /// <summary>
            ///     The configuration
            /// </summary>
            private static Menu _config;

            #endregion

            #region Fields

            /// <summary>
            ///     The player
            /// </summary>
            private readonly AIHeroClient Player;

            /// <summary>
            ///     The forced target
            /// </summary>
            private Obj_AI_Base _forcedTarget;

            /// <summary>
            ///     The orbalker mode
            /// </summary>
            private OrbwalkingMode _mode = OrbwalkingMode.None;

            /// <summary>
            ///     The orbwalking point
            /// </summary>
            private Vector3 _orbwalkingPoint;

            /// <summary>
            ///     The previous minion the orbwalker was targeting.
            /// </summary>
            private Obj_AI_Minion _prevMinion;

            /// <summary>
            ///     The name of the CustomMode if it is set.
            /// </summary>
            private string CustomModeName;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="Orbwalker" /> class.
            /// </summary>
            /// <param name="attachToMenu">The menu the orbwalker should attach to.</param>
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
                    new MenuItem("HoldPosRadius", "\u4e0d\u52d5\u534a\u5f91\u8ddd\u96e2").SetShared().SetValue(new Slider(150, 50, 250)));
                misc.AddItem(new MenuItem("PriorizeFarm", "\u9a37\u64fe\u512a\u5148\u88dc\u5175").SetShared().SetValue(true));
                misc.AddItem(new MenuItem("PrioritizeCasters", "\u512a\u5148\u653b\u64ca\u9060\u7a0b\u5c0f\u5175").SetShared().SetValue(false));
                misc.AddItem(new MenuItem("AttackWards", "\u81ea\u52d5\u653b\u64ca \u773c").SetShared().SetValue(false));
                misc.AddItem(new MenuItem("AttackPetsnTraps", "\u81ea\u52d5\u653b\u64ca \u5bf5\u7269").SetShared().SetValue(true));
                misc.AddItem(
                    new MenuItem("AttackGPBarrel", "\u81ea\u52d5\u653b\u64ca \u525b\u666e \u6876\u5b50").SetShared()
                        .SetValue(new StringList(new[] { "\u9023\u62db \u548c \u6e05\u7dda", "\u6e05\u7dda", "\u95dc\u9589" }, 1)));
                misc.AddItem(new MenuItem("Smallminionsprio", "\u6253\u91ce\u512a\u5148\u6e05\u5c0f\u602a").SetShared().SetValue(false));
                misc.AddItem(
                    new MenuItem("FocusMinionsOverTurrets", "\u96c6\u4e2d \u5854\u4e0b\u5c0f\u5175").SetShared()
                        .SetValue(new KeyBind('M', KeyBindType.Toggle)));

                _config.AddSubMenu(misc);

                /* Missile check */
                _config.AddItem(new MenuItem("MissileCheck", "\u4f7f\u7528\u5f48\u9053\u6aa2\u67e5").SetShared().SetValue(true));

                /* Delay sliders */
                _config.AddItem(
                    new MenuItem("ExtraWindup", "\u984d\u5916 \u8d70\u780d\u5f8c\u6416 \u5ef6\u9072").SetShared().SetValue(new Slider(80, 0, 200)));
                _config.AddItem(new MenuItem("FarmDelay", "\u6e05\u7dda \u5ef6\u9072").SetShared().SetValue(new Slider(30, 0, 200)));

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
                _config.Item("StillCombo").ValueChanged +=
                    (sender, args) => { Move = !args.GetNewValue<KeyBind>().Active; };

                this.Player = ObjectManager.Player;
                Game.OnUpdate += new GameUpdate(this.GameOnOnGameUpdate);
                Drawing.OnDraw += new DrawingDraw(this.DrawingOnOnDraw);
                Instances.Add(this);
            }

            #endregion

            #region Public Properties
            
            /// <summary>
            ///     Gets a value indicating whether the orbwalker is orbwalking by checking the missiles.
            /// </summary>
            /// <value><c>true</c> if the orbwalker is orbwalking by checking the missiles; otherwise, <c>false</c>.</value>
            public static bool MissileCheck
            {
                get
                {
                    return _config.Item("MissileCheck").GetValue<bool>();
                }
            }

            /// <summary>
            ///     Gets or sets the active mode.
            /// </summary>
            /// <value>The active mode.</value>
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

            #region Properties

            /// <summary>
            ///     Gets the farm delay.
            /// </summary>
            /// <value>The farm delay.</value>
            private int FarmDelay
            {
                get
                {
                    return _config.Item("FarmDelay").GetValue<Slider>().Value;
                }
            }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                Menu.Remove(_config);
                Game.OnUpdate -= new GameUpdate(this.GameOnOnGameUpdate);
                Drawing.OnDraw -= new DrawingDraw(this.DrawingOnOnDraw);
                Instances.Remove(this);
            }

            /// <summary>
            ///     Forces the orbwalker to attack the set target if valid and in range.
            /// </summary>
            /// <param name="target">The target.</param>
            public void ForceTarget(Obj_AI_Base target)
            {
                this._forcedTarget = target;
            }

            /// <summary>
            ///     Gets the target.
            /// </summary>
            /// <returns>AttackableUnit.</returns>
            public virtual AttackableUnit GetTarget()
            {
                AttackableUnit result = null;
                var mode = this.ActiveMode;

                if ((mode == OrbwalkingMode.Mixed || mode == OrbwalkingMode.LaneClear) && !_config.Item("PriorizeFarm").GetValue<bool>())
                {
                    var target = TargetSelector.GetTarget(-1, TargetSelector.DamageType.Physical);

                    if (target != null && this.InAutoAttackRange(target))
                    {
                        return target;
                    }
                }

                //GankPlank barrels
                var attackGankPlankBarrels = _config.Item("AttackGPBarrel").GetValue<StringList>().SelectedIndex;
                if (attackGankPlankBarrels != 2
                    && (attackGankPlankBarrels == 0
                        || (mode == OrbwalkingMode.LaneClear || mode == OrbwalkingMode.Mixed
                            || mode == OrbwalkingMode.LastHit || mode == OrbwalkingMode.Freeze)))
                {
                    var enemyGangPlank =
                        HeroManager.Enemies.FirstOrDefault(
                            e => e.ChampionName.Equals("gangplank", StringComparison.InvariantCultureIgnoreCase));

                    if (enemyGangPlank != null)
                    {
                        var barrels =
                            ObjectManager.Get<Obj_AI_Minion>()
                                .Where(
                                    minion =>
                                    minion.Team == GameObjectTeam.Neutral
                                    && minion.CharData.BaseSkinName == "gangplankbarrel" && minion.IsHPBarRendered
                                    && minion.IsValidTarget() && this.InAutoAttackRange(minion));

                        foreach (var barrel in barrels)
                        {
                            if (barrel.Health <= 1f)
                            {
                                return barrel;
                            }

                            var t = (int)(this.Player.AttackCastDelay * 1000) + Game.Ping / 2
                                    + 1000 * (int)Math.Max(0, this.Player.Distance(barrel) - this.Player.BoundingRadius)
                                    / (int)GetMyProjectileSpeed();

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

                                if (nextHealthDecayTime <= Game.Time + t / 1000f && ObjectManager
                                        .Get<Obj_GeneralParticleEmitter>()
                                        .Any(
                                            x => x.Name == "Gangplank_Base_E_AoE_Red.troy"
                                                 && barrel.Distance(x.Position) < 10))
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

                /*Killable Minion*/
                if (mode == OrbwalkingMode.LaneClear || mode == OrbwalkingMode.Mixed || mode == OrbwalkingMode.LastHit
                    || mode == OrbwalkingMode.Freeze)
                {
                    var MinionList =
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Where(minion => minion.IsValidTarget() && this.InAutoAttackRange(minion))
                            .OrderByDescending(minion => minion.CharData.BaseSkinName.Contains("Siege"))
                            .ThenBy(minion => minion.CharData.BaseSkinName.Contains("Super"))
                            .ThenBy(minion => minion.Health)
                            .ThenByDescending(minion => minion.MaxHealth);

                    foreach (var minion in MinionList)
                    {
                        var t = (int)(this.Player.AttackCastDelay * 1000f) - 50 + Game.Ping / 2
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

                //Forced target
                if (this._forcedTarget.IsValidTarget() && this.InAutoAttackRange(this._forcedTarget))
                {
                    return this._forcedTarget;
                }

                /* turrets / inhibitors / nexus */
                if ((mode == OrbwalkingMode.LaneClear)
                    && (!_config.Item("FocusMinionsOverTurrets").GetValue<KeyBind>().Active
                        || !MinionManager.GetMinions(
                            ObjectManager.Player.Position,
                            GetRealAutoAttackRange(ObjectManager.Player)).Any()))
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

                /*Champions*/
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
                }

                /*Jungle minions*/
                if (mode == OrbwalkingMode.LaneClear || mode == OrbwalkingMode.Mixed)
                {
                    var jminions =
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Where(
                                mob =>
                                mob.IsValidTarget() && mob.Team == GameObjectTeam.Neutral && this.InAutoAttackRange(mob)
                                && mob.CharData.BaseSkinName != "gangplankbarrel" && mob.Name != "WardCorpse"
                                && !mob.CharData.BaseSkinName.Contains("Plant"));

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
                        var minions =
                            MinionManager.GetMinions(this.Player.Position, this.Player.AttackRange + 200)
                                .Where(
                                    minion =>
                                    this.InAutoAttackRange(minion) && closestTower.Distance(minion, true) < 900 * 900)
                                .OrderByDescending(minion => minion.CharData.BaseSkinName.Contains("Siege"))
                                .ThenBy(minion => minion.CharData.BaseSkinName.Contains("Super"))
                                .ThenByDescending(minion => minion.MaxHealth)
                                .ThenByDescending(minion => minion.Health);
                        if (minions.Any())
                        {
                            // get the turret aggro minion
                            var turretMinion =
                                minions.FirstOrDefault(
                                    minion => minion is Obj_AI_Minion
                                              && HealthPrediction.HasTurretAggro((Obj_AI_Minion)minion));

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
                                                                   ?
                                                                   LastAATick + (int)(this.Player.AttackDelay * 1000)
                                                                     -
                                                                     (Utils.GameTimeTickCount + Game.Ping / 2 + 25)
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
                                        && !HealthPrediction.HasMinionAggro((Obj_AI_Minion)x)))
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
                                        && !HealthPrediction.HasMinionAggro((Obj_AI_Minion)x));
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
                                        x => x is Obj_AI_Minion && !HealthPrediction.HasMinionAggro((Obj_AI_Minion)x))
                                    )
                                {
                                    if (closestTower != null)
                                    {
                                        var playerDamage = (int)this.Player.GetAutoAttackDamage(minion);
                                        var turretDamage = (int)closestTower.GetAutoAttackDamage(minion, true);
                                        var leftHP = (int)minion.Health % turretDamage;
                                        if (leftHP > playerDamage)
                                        {
                                            return minion;
                                        }
                                    }
                                }
                                //late game
                                var lastminion =
                                    minions.LastOrDefault(
                                        x => x is Obj_AI_Minion && !HealthPrediction.HasMinionAggro((Obj_AI_Minion)x));

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
                                (int) (this.Player.AttackDelay * 1000 * LaneClearWaitTimeMod),
                                this.FarmDelay);
                            if (predHealth >= 2 * this.Player.GetAutoAttackDamage(this._prevMinion)
                                || Math.Abs(predHealth - this._prevMinion.Health) < float.Epsilon)
                            {
                                if (this._prevMinion.IsHPBarRendered)
                                    return this._prevMinion;
                            }
                        }

                        var results = (from minion in
                            ObjectManager.Get<Obj_AI_Minion>()
                                .Where(
                                    minion =>
                                        minion.IsValidTarget() && this.InAutoAttackRange(minion)
                                        && this.ShouldAttackMinion(minion) && !minion.CharData.BaseSkinName.Contains("Plant"))

                            let predHealth =
                                HealthPrediction.LaneClearHealthPrediction(
                                    minion,
                                    (int) (this.Player.AttackDelay * 1000 * LaneClearWaitTimeMod),
                                    this.FarmDelay)
                            where
                                predHealth >= 2 * this.Player.GetAutoAttackDamage(minion)
                                || Math.Abs(predHealth - minion.Health) < float.Epsilon
                            select minion);

                        result = results.MaxOrDefault(m => !MinionManager.IsMinion(m, true) ? float.MaxValue : m.Health);


                        if (_config.Item("PrioritizeCasters").GetValue<bool>())
                        {
                            result =
                                results.OrderByDescending(
                                    m =>
                                        m.CharData.BaseSkinName.Contains("Ranged"))
                                    .FirstOrDefault();
                        }

                        if (result != null)
                        {
                            this._prevMinion = (Obj_AI_Minion) result;
                        }
                    }
                }

                return result;
            }

            /// <summary>
            ///     Determines if a target is in auto attack range.
            /// </summary>
            /// <param name="target">The target.</param>
            /// <returns><c>true</c> if a target is in auto attack range, <c>false</c> otherwise.</returns>
            public virtual bool InAutoAttackRange(AttackableUnit target)
            {
                return Orbwalking.InAutoAttackRange(target);
            }

            /// <summary>
            ///     Registers the Custom Mode of the Orbwalker. Useful for adding a flee mode and such.
            /// </summary>
            /// <param name="name">The name of the mode Ex. "Myassembly.FleeMode" </param>
            /// <param name="displayname">The name of the mode in the menu. Ex. Flee</param>
            /// <param name="key">The default key for this mode.</param>
            public virtual void RegisterCustomMode(string name, string displayname, uint key)
            {
                this.CustomModeName = name;
                if (_config.Item(name) == null)
                {
                    _config.AddItem(
                        new MenuItem(name, displayname).SetShared().SetValue(new KeyBind(key, KeyBindType.Press)));
                }
            }

            /// <summary>
            ///     Enables or disables the auto-attacks.
            /// </summary>
            /// <param name="b">if set to <c>true</c> the orbwalker will attack units.</param>
            public void SetAttack(bool b)
            {
                Attack = b;
            }

            /// <summary>
            ///     Enables or disables the movement.
            /// </summary>
            /// <param name="b">if set to <c>true</c> the orbwalker will move.</param>
            public void SetMovement(bool b)
            {
                Move = b;
            }

            /// <summary>
            ///     Forces the orbwalker to move to that point while orbwalking (Game.CursorPos by default).
            /// </summary>
            /// <param name="point">The point.</param>
            public void SetOrbwalkingPoint(Vector3 point)
            {
                this._orbwalkingPoint = point;
            }

            public Vector3 GetOrbwalkingPoint()
            {
                return this._orbwalkingPoint;
            }

            /// <summary>
            ///     Determines if the orbwalker should wait before attacking a minion.
            /// </summary>
            /// <returns><c>true</c> if the orbwalker should wait before attacking a minion, <c>false</c> otherwise.</returns>
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
                _config.Item("FocusMinionsOverTurrets")
                    .Permashow(_config.Item("FocusMinionsOverTurrets").GetValue<KeyBind>().Active);

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

            private bool ShouldAttackMinion(Obj_AI_Minion minion)
            {
                if (minion.Name == "WardCorpse" || minion.CharData.BaseSkinName == "jarvanivstandard")
                {
                    return false;
                }

                if (MinionManager.IsWard(minion))
                {
                    return _config.Item("AttackWards").IsActive();
                }

                return (_config.Item("AttackPetsnTraps").GetValue<bool>() || MinionManager.IsMinion(minion))
                       && minion.CharData.BaseSkinName != "gangplankbarrel";
            }

            private bool ShouldWaitUnderTurret(Obj_AI_Minion noneKillableMinion)
            {
                return
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Any(
                            minion =>
                            (noneKillableMinion != null ? noneKillableMinion.NetworkId != minion.NetworkId : true)
                            && minion.IsValidTarget() && minion.Team != GameObjectTeam.Neutral
                            && this.InAutoAttackRange(minion) && MinionManager.IsMinion(minion, false)
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

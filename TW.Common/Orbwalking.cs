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
        private static readonly string[] NoCancelChamps = { "Kalista" };
        private static readonly AIHeroClient Player = HeroManager.Player;
        private static int _autoattackCounter;
        private static int _delay;
        private static AttackableUnit _lastTarget;
        private static float _minDistance = 400;
        private static bool _missileLaunched;
        public static List<Obj_AI_Minion> AzirSoliders = new List<Obj_AI_Minion>();
        private static int TimeAdjust = 0;
        private static int BrainFarmInt = -90;
        private static int DelayOnFire = 0;
        private static int DelayOnFireId = 0;
        public static int LastMove;
        public static int NextMovementDelay;
        public static Vector3 LastMovementPosition = Vector3.Zero;

        #endregion

        static Orbwalking()
        {
            Player = ObjectManager.Player;
            _championName = Player.ChampionName;
            Obj_AI_Base.OnPlayAnimation += Obj_AI_Base_OnPlayAnimation;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Spellbook.OnStopCast += OnStopCast;
            Obj_AI_Base.OnSpellCast += OnSpellCast;
            EloBuddy.Player.OnIssueOrder += Player_OnIssueOrder;
            Obj_AI_Base.OnBasicAttack += OnBasicAttack;
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

        private static void OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && (args.Target is Obj_AI_Base || args.Target is Obj_BarracksDampener || args.Target is Obj_HQ))
            {
                LastAATick = Utils.GameTimeTickCount - Game.Ping / 2;
                _missileLaunched = false;
                LastMoveCommandT = 0;
                _autoattackCounter++;

                if (args.Target is Obj_AI_Base)
                {
                    var target = (Obj_AI_Base)args.Target;
                    if (target.IsValid)
                    {
                        FireOnTargetSwitch(target);
                        _lastTarget = target;
                    }
                }

                if (sender is Obj_AI_Turret && args.Target is Obj_AI_Base)
                {
                    LastTargetTurrets[sender.NetworkId] = (Obj_AI_Base)args.Target;
                }
            }
            FireOnAttack(sender, _lastTarget);
        }

        internal static readonly Dictionary<int, Obj_AI_Base> LastTargetTurrets = new Dictionary<int, Obj_AI_Base>();

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
            if (spellbook.IsValid /*&& EloBuddy.SDK.Orbwalker.IsRanged && */ && !EloBuddy.SDK.Orbwalker.CanBeAborted && spellbook.IsMe && args.DestroyMissile && args.StopAnimation)
            {
                Console.WriteLine("AA Cancel" + Game.Time);

                ResetAutoAttackTimer();
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

            if (_championName == "Jinx")
            {
                localExtraWindup = 100;
            }

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
            return IsMelee(Player) || _championName == "Azir" || _championName == "Velkoz" || _championName == "Thresh"
                   || (_championName == "Viktor" && Player.HasBuff("ViktorPowerTransferReturn"))
                   || (_championName == "Kayle" && Player.HasBuff("JudicatorRighteousFury"))
                       ? float.MaxValue
                       : Player.BasicAttack.MissileSpeed;
        }

        public static float GetRealAutoAttackRange(AttackableUnit target)
        {
            return GetRealAutoAttackRange(Player, target.Compare1(Player) ? null : target);
        }
        
        public static float GetRealAutoAttackRange(Obj_AI_Base sender, AttackableUnit target)
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

        public static bool CanCancelAutoAttack(AIHeroClient hero)
        {
            return !NoCancelChamps.Contains(hero.ChampionName);
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
            private const float LaneClearWaitTimeMod = 2f;
            public static List<Orbwalker> Instances = new List<Orbwalker>();
            private static Menu _config;
            private readonly AIHeroClient Player = HeroManager.Player;
            private Obj_AI_Base _forcedTarget;
            private OrbwalkingMode _mode = OrbwalkingMode.None;
            private Vector3 _orbwalkingPoint;
            private string CustomModeName;
            private Obj_AI_Base laneClearMinion;
            private bool isFinishAttack;
            private int countAutoAttack;
            public AttackableUnit LastTarget { get; private set; }

            public int LastAutoAttackTick { get; private set; }
            private readonly string[] specialMinions =
            {
                    "zyrathornplant", "zyragraspingplant", "heimertyellow",
                    "heimertblue", "malzaharvoidling", "yorickdecayedghoul",
                    "yorickravenousghoul", "yorickspectralghoul", "shacobox",
                    "annietibbers", "teemomushroom", "elisespiderling"
            };
            private readonly string[] clones = { "shaco", "monkeyking", "leblanc" };
            private readonly string[] ignoreMinions = { "jarvanivstandard" };

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
                    new MenuItem("HoldPosRadius", "\u4e0d\u52d5\u534a\u5f91\u8ddd\u96e2").SetShared().SetValue(new Slider(150, 50, 250)));
                misc.AddItem(new MenuItem("PriorizeFarm", "\u9a37\u64fe\u512a\u5148\u88dc\u5175").SetShared().SetValue(true));

                misc.SubMenu("\u81ea\u52d5\u653b\u64ca\u773c\u8a2d\u5b9a").SubMenu("\u9023\u62db").AddItem(
                    new MenuItem("AttackWardsCombo", "\u555f\u52d5").SetShared().SetValue(false));
                misc.SubMenu("\u81ea\u52d5\u653b\u64ca\u773c\u8a2d\u5b9a").SubMenu("\u9a37\u64fe").AddItem(
                    new MenuItem("AttackWardsMixed", "\u555f\u52d5").SetShared().SetValue(true));
                misc.SubMenu("\u81ea\u52d5\u653b\u64ca\u773c\u8a2d\u5b9a").SubMenu("\u6e05\u7dda").AddItem(
                    new MenuItem("AttackWardsLaneClear", "\u555f\u52d5").SetShared().SetValue(true));
                misc.SubMenu("\u81ea\u52d5\u653b\u64ca\u773c\u8a2d\u5b9a").SubMenu("\u5c3e\u5200").AddItem(
                    new MenuItem("AttackWardsLastHit", "\u555f\u52d5").SetShared().SetValue(true));

                misc.SubMenu("\u81ea\u52d5\u653b\u64ca \u6876\u5b50").AddItem(
                    new MenuItem("AttackBarrel", "\u81ea\u52d5\u653b\u64ca \u525b\u666e \u6876\u5b50").SetShared()
                        .SetValue(true));
                misc.SubMenu("\u81ea\u52d5\u653b\u64ca \u6876\u5b50").SubMenu("\u9023\u62db").AddItem(
                    new MenuItem("AttackPetsnTrapsCombo", "\u555f\u52d5").SetShared().SetValue(false));
                misc.SubMenu("\u81ea\u52d5\u653b\u64ca \u6876\u5b50").SubMenu("\u9a37\u64fe").AddItem(
                    new MenuItem("AttackPetsnTrapsMixed", "\u555f\u52d5").SetShared().SetValue(true));
                misc.SubMenu("\u81ea\u52d5\u653b\u64ca \u6876\u5b50").SubMenu("\u6e05\u7dda").AddItem(
                    new MenuItem("AttackPetsnTrapsLaneClear", "\u555f\u52d5").SetShared().SetValue(true));
                misc.SubMenu("\u81ea\u52d5\u653b\u64ca \u6876\u5b50").SubMenu("\u5c3e\u5200").AddItem(
                    new MenuItem("AttackPetsnTrapsLastHit", "\u555f\u52d5").SetShared().SetValue(true));
                misc.SubMenu("\u81ea\u52d5\u653b\u64ca \u6876\u5b50").SubMenu("\u63a7\u7dda").AddItem(
                    new MenuItem("AttackPetsnTrapsFreeze", "\u555f\u52d5").SetShared().SetValue(false));

                misc.SubMenu("\u81ea\u52d5\u653b\u64ca\u7832\u5854").SubMenu("\u9023\u62db").AddItem(
                    new MenuItem("BuildingsCombo", "\u555f\u52d5").SetShared().SetValue(false));
                misc.SubMenu("\u81ea\u52d5\u653b\u64ca\u7832\u5854").SubMenu("\u9a37\u64fe").AddItem(
                    new MenuItem("BuildingsMixed", "\u555f\u52d5").SetShared().SetValue(true));
                misc.SubMenu("\u81ea\u52d5\u653b\u64ca\u7832\u5854").SubMenu("\u6e05\u7dda").AddItem(
                    new MenuItem("BuildingsLaneClear", "\u555f\u52d5").SetShared().SetValue(true));

                misc.SubMenu("\u81ea\u52d5\u5176\u4ed6 \u7269\u4ef6").AddItem(
                    new MenuItem("attackSpecialMinions", "\u7279\u6b8a\u985e\u578b \u602a\u7269 \u555f\u52d5").SetShared()
                        .SetValue(true));
                misc.SubMenu("\u81ea\u52d5\u5176\u4ed6 \u7269\u4ef6").AddItem(
                    new MenuItem("prioritizeSpecialMinions", "\u512a\u5148\u653b\u64ca \u7279\u6b8a \u602a\u7269").SetShared()
                        .SetValue(false));
                misc.SubMenu("\u81ea\u52d5\u5176\u4ed6 \u7269\u4ef6").AddItem(
                    new MenuItem("asm1", "\u67b7\u863f \u690d\u7269 | \u4e01\u683c \u7832\u5854\u6a5f\u5668", true));
                misc.SubMenu("\u81ea\u52d5\u5176\u4ed6 \u7269\u4ef6").AddItem(
                    new MenuItem(
                        "asm2",
                        "\u99ac\u723e \u865b\u9748 | \u85a9\u79d1 \u7bb1\u5b50 | \u5b89\u59ae \u96c4",
                        true));
                misc.SubMenu("\u81ea\u52d5\u5176\u4ed6 \u7269\u4ef6").AddItem(
                    new MenuItem("asm3", "\u63d0\u6469 \u9999\u83c7 | \u8718\u86db \u5c0f\u602a", true));

                misc.SubMenu("\u81ea\u52d5\u653b\u64ca\u5206\u8eab").AddItem(new MenuItem("attackClones", "\u555f\u52d5").SetShared().SetValue(false));

                misc.AddItem(new MenuItem("Smallminionsprio", "\u6253\u91ce\u512a\u5148\u6e05\u5c0f\u602a").SetShared().SetValue(false));

                _config.AddSubMenu(misc);

                /* Missile check */
                _config.AddItem(new MenuItem("MissileCheck", "\u4f7f\u7528\u5f48\u9053\u6aa2\u67e5").SetShared().SetValue(true));

                var timeAdjust = new Menu("\u8d70\u780d\u5ef6\u9072\u8abf\u6574", "TimeAdjust");
                timeAdjust.AddItem(
                    new MenuItem("ExtraWindup", "\u984d\u5916 \u8d70\u780d\u5f8c\u6416 \u5ef6\u9072").SetShared().SetValue(new Slider(80, 0, 200)));
                timeAdjust.AddItem(new MenuItem("FarmDelay", "\u6e05\u7dda \u5ef6\u9072").SetShared().SetValue(new Slider(0, 0, 200)));
                timeAdjust.AddItem(new MenuItem("t1", " ", true));
                timeAdjust.AddItem(
                    new MenuItem("TimeAdjust", "\u65e9 A  <<<----- \u88dc\u5200 \u6642\u9593\u8abf\u6574  ----->>>  \u665a A").SetShared()
                        .SetValue(new Slider(0, -100, 100))).SetTooltip("0 \u9ed8\u8a8d\u503c", SharpDX.Color.Cyan);
                timeAdjust.AddItem(
                        new MenuItem("AutoAdjustTime", "\u8fd4\u56de\u9810\u8a2d\u503c").SetShared().SetValue(false))
                    .ValueChanged += (obj, args) =>
                    {
                        if (!args.GetNewValue<bool>())
                        {
                            _config.Item("TimeAdjust").SetShared().SetValue(new Slider(0, -100, 100));
                        }
                        else if (args.GetNewValue<bool>())
                        {
                            _config.Item("TimeAdjust").SetShared().SetValue(new Slider(0, -100, 100));
                        }
                    };

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
                _config.Item("StillCombo").ValueChanged +=
                    (sender, args) => { Move = !args.GetNewValue<KeyBind>().Active; };

                this.Player = ObjectManager.Player;
                Game.OnUpdate += new GameUpdate(this.GameOnOnGameUpdate);
                Drawing.OnDraw += new DrawingDraw(this.DrawingOnOnDraw);
                Instances.Add(this);
            }

            #endregion

            #region Public Properties

            public static bool MissileCheck
            {
                get
                {
                    return _config.Item("MissileCheck").GetValue<bool>();
                }
            }

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

            private int FarmDelay
            {
                get
                {
                    return _config.Item("FarmDelay").GetValue<Slider>().Value;
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

                //Forced target
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

                var MinionList = MinionCacheManager.GetMinions(Player.Position, 0, MinionTeam.NotAlly);
                List<Obj_AI_Base> minionsFiltered = new List<Obj_AI_Base>();
                List<Obj_AI_Base> wards = new List<Obj_AI_Base>();
                List<Obj_AI_Base> other = new List<Obj_AI_Base>();
                var spminion = new List<Obj_AI_Minion>();

                var firstT = (int)(Player.AttackCastDelay * 1000) + BrainFarmInt + Game.Ping / 2;

                if (mode != OrbwalkingMode.None)
                {
                    foreach (var minion in MinionList.Where(x => x.IsValidTarget() && x.IsHPBarRendered))
                    {
                        var minionObj = minion as Obj_AI_Minion;

                        if (minion != null)
                        {
                            if (MinionManager.IsMinion(minionObj))
                            {
                                minionsFiltered.Add(minion);
                            }
                            else if (MinionManager.IsWard(minionObj))
                            {
                                wards.Add(minion);
                            }
                            else if (minion.CharData.BaseSkinName != "gangplankbarrel" && !minion.Name.ToLower().Contains("sru"))
                            {
                                other.Add(minion);
                            }
                        }
                    }

                    spminion = this.Getminions(mode);
                }

                /* Wards */
                if ((mode == OrbwalkingMode.Combo && _config.Item("AttackWardsCombo").GetValue<bool>())
                    || (mode == OrbwalkingMode.Mixed && _config.Item("AttackWardsMixed").GetValue<bool>())
                    || (mode == OrbwalkingMode.LaneClear && _config.Item("AttackWardsLaneClear").GetValue<bool>())
                    || (mode == OrbwalkingMode.LastHit && _config.Item("AttackWardsLastHit").GetValue<bool>())
                    )
                {
                    var obj = wards.FirstOrDefault();

                    if (obj != null)
                        return obj;
                }

                /* PeTs */
                if ((mode == OrbwalkingMode.Combo && _config.Item("AttackPetsnTrapsCombo").GetValue<bool>())
                    || (mode == OrbwalkingMode.Mixed && _config.Item("AttackPetsnTrapsMixed").GetValue<bool>())
                    || (mode == OrbwalkingMode.LaneClear && _config.Item("AttackPetsnTrapsLaneClear").GetValue<bool>())
                    || (mode == OrbwalkingMode.LastHit && _config.Item("AttackPetsnTrapsLastHit").GetValue<bool>())
                    || (mode == OrbwalkingMode.Freeze && _config.Item("AttackPetsnTrapsFreeze").GetValue<bool>())
                    )
                {
                    if (_config.Item("AttackBarrel").GetValue<bool>())
                    {
                        var enemyGangPlank =
                            HeroManager.Enemies.FirstOrDefault(
                                e => e.ChampionName.Equals("gangplank", StringComparison.InvariantCultureIgnoreCase));

                        if (enemyGangPlank != null)
                        {
                            var barrels = MinionCacheManager.GetMinions(Player.Position, 0, MinionTeam.NotAlly).Where(
                                m => m.Team == GameObjectTeam.Neutral && m.CharData.BaseSkinName == "gangplankbarrel"
                                     && m.IsHPBarRendered && m.IsValidTarget() && this.InAutoAttackRange(m));

                            foreach (var barrel in barrels)
                            {
                                if (barrel.Health <= 1f)
                                    return barrel;


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

                                    if (nextHealthDecayTime <= Game.Time + t / 1000f
                                        && ObjectManager.Get<Obj_GeneralParticleEmitter>()
                                            .Any(
                                                x => x.Name == "Gangplank_Base_E_AoE_Red.troy"
                                                     && barrel.Distance(x.Position) < 10))
                                    {
                                        return barrel;
                                    }
                                }
                            }

                            if (barrels.Any())
                                return null;
                        }
                    }

                    var obj = other.FirstOrDefault();

                    if (obj != null)
                    {
                        return obj;
                    }
                }

                /* turrets / inhibitors / nexus */
                if ((mode == OrbwalkingMode.Combo && _config.Item("BuildingsCombo").GetValue<bool>())
                    || (mode == OrbwalkingMode.Mixed && _config.Item("BuildingsMixed").GetValue<bool>())
                    || (mode == OrbwalkingMode.LaneClear && _config.Item("BuildingsLaneClear").GetValue<bool>())
                    )
                {
                    /* turrets */
                    foreach (var turret in MinionCacheManager.TurretList.Where(
                        t => t.IsValidTarget() && this.InAutoAttackRange(t)))
                    {
                        return turret;
                    }

                    /* inhibitor */
                    foreach (var turret in MinionCacheManager.InhiList.Where(
                        t => t.IsValidTarget() && this.InAutoAttackRange(t)))
                    {
                        return turret;
                    }

                    /* nexus */
                    foreach (var nexus in MinionCacheManager.NexusList.Where(
                        t => t.IsValidTarget() && this.InAutoAttackRange(t)))
                    {
                        return nexus;
                    }
                }

                /* Killable Minion */
                if (mode == OrbwalkingMode.LaneClear || mode == OrbwalkingMode.Mixed || mode == OrbwalkingMode.LastHit || mode == OrbwalkingMode.Freeze)
                {
                    if (!_config.Item("AutoAdjustTime").GetValue<bool>())
                        BrainFarmInt = -TimeAdjust - 50;

                    var LastHitList = minionsFiltered
                        .Where(minion => minion.Team != GameObjectTeam.Neutral)
                        .OrderByDescending(minion => minion.CharData.BaseSkinName.Contains("Super"))
                        .ThenByDescending(minion => minion.CharData.BaseSkinName.Contains("Siege"))
                        .ThenBy(minion => HealthPrediction.GetHealthPrediction(minion, 1500))
                        .ThenByDescending(minion => minion.MaxHealth);

                    foreach (var minion in LastHitList)
                    {
                        var t = firstT + 1000 * (int)Math.Max(
                                    0,
                                    this.Player.ServerPosition.Distance(minion.ServerPosition)
                                    - this.Player.BoundingRadius) / (int)GetMyProjectileSpeed();

                        if (mode == OrbwalkingMode.Freeze)
                        {
                            t += 200 + Game.Ping / 2;
                        }

                        var predHealth = HealthPrediction.GetHealthPrediction(minion, t, this.FarmDelay);

                        var dmg = Player.GetAutoAttackDamage(minion, true)
                                  + _config.Item("TimeAdjust").GetValue<Slider>().Value;

                        var killable = predHealth <= dmg;

                        if (mode == OrbwalkingMode.Freeze)
                        {
                            if (minion.Health < 50 || predHealth <= 50)
                            {
                                return minion;
                            }
                        }
                        else
                        {
                            if (CanAttack())
                            {
                                DelayOnFire = t + Utils.TickCount;
                                DelayOnFireId = minion.NetworkId;
                            }

                            if (predHealth <= 0)
                            {
                                FireOnNonKillableMinion(minion);
                            }
                            else if (killable)
                            {
                                return minion;
                            }
                        }
                    }
                }

                if (CanAttack())
                {
                    DelayOnFire = 0;
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
                    //var jminions = ObjectManager.Get<Obj_AI_Minion>().Where(
                    //    mob => mob.IsValidTarget() && mob.Team == GameObjectTeam.Neutral && this.InAutoAttackRange(mob)
                    //           && mob.CharData.BaseSkinName != "gangplankbarrel" && mob.Name != "WardCorpse"
                    //           && !mob.CharData.BaseSkinName.Contains("Plant"));
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
                                              && HealthPrediction.HasTurretAggro(minion as Obj_AI_Minion));

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
                                        x => x is Obj_AI_Minion && !HealthPrediction.HasMinionAggro(x as Obj_AI_Minion))
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

                var Minions = new List<Obj_AI_Minion>();

                // Special Minions if no enemy is near
                if (mode == OrbwalkingMode.Combo && Minions.Any() && !HeroManager.Enemies.Any(
                        e => e.IsValidTarget() && e.DistanceToPlayer() <= GetRealAutoAttackRange(e) * 2f))
                {
                    return Minions.FirstOrDefault();
                }

                /*Lane Clear minions*/
                if (mode == OrbwalkingMode.LaneClear)
                {
                    if (!this.ShouldWait())
                    {
                        if (this.InAutoAttackRange(this.laneClearMinion) && this.laneClearMinion.IsValidTarget())
                        {
                            if (this.laneClearMinion.MaxHealth <= 10)
                            {
                                return this.laneClearMinion;
                            }

                            var predHealth = HealthPrediction.LaneClearHealthPrediction(
                                this.laneClearMinion,
                                (int)(this.Player.AttackDelay * 1000 * LaneClearWaitTimeMod),
                                this.FarmDelay);

                            if (predHealth >= 2 * this.Player.GetAutoAttackDamage(this.laneClearMinion)
                                || Math.Abs(predHealth - this.laneClearMinion.Health) < float.Epsilon)
                            {
                                return this.laneClearMinion;
                            }
                        }

                        result =
                            (from minions in ObjectManager.Get<Obj_AI_Minion>()
                                 .Where(
                                     m => m != null && this.InAutoAttackRange(m) && this.ShouldAttackMinion(m)
                                          && !m.BaseSkinName.Contains("Plant"))
                             let predHealth =
                             HealthPrediction.LaneClearHealthPrediction(
                                 minions,
                                 (int)(this.Player.AttackDelay * 1000 * LaneClearWaitTimeMod),
                                 this.FarmDelay)
                             where predHealth >= 2 * this.Player.GetAutoAttackDamage(minions)
                                   || Math.Abs(predHealth - minions.Health) < float.Epsilon
                             select minions).MaxOrDefault(
                                m => !MinionManager.IsMinion(m, true) ? float.MaxValue : m.Health);

                        if (result != null)
                        {
                            this.laneClearMinion = (Obj_AI_Minion)result;
                        }
                    }
                }

                return result;
            }

            public virtual bool InAutoAttackRange(AttackableUnit target)
            {
                return Orbwalking.InAutoAttackRange(target);
            }

            private List<Obj_AI_Minion> Getminions(OrbwalkingMode mode)
            {
                var prioritizeSpecialMinions = _config.Item("prioritizeSpecialMinions").GetValue<bool>();
                var combominion = mode != OrbwalkingMode.Combo;

                var mList = new List<Obj_AI_Minion>();
                var cloneList = new List<Obj_AI_Minion>();
                var finalMinionList = new List<Obj_AI_Minion>();
                var specialList = new List<Obj_AI_Minion>();

                foreach (var minion in EloBuddy.SDK.EntityManager.MinionsAndMonsters.EnemyMinions.Where(
                    m => this.IsValidUnit(m)))
                {
                    var baseName = minion.CharData.BaseSkinName.ToLower();

                    if (combominion && minion.IsMinion)
                    {
                        mList.Add(minion);
                    }
                    else if (_config.Item("attackSpecialMinions").GetValue<bool>() && this.specialMinions.Any(s => s.Equals(baseName)))
                    {
                        specialList.Add(minion);
                    }
                    else if (_config.Item("attackClones").GetValue<bool>() && this.clones.Any(c => c.Equals(baseName)))
                    {
                        cloneList.Add(minion);
                    }
                }

                if (combominion)
                {
                    mList = OrderEnemyMinions(mList);
                }

                if (_config.Item("attackSpecialMinions").GetValue<bool>() && prioritizeSpecialMinions)
                {
                    finalMinionList.AddRange(specialList);
                    finalMinionList.AddRange(mList);
                }

                if (_config.Item("attackClones").GetValue<bool>())
                {
                    finalMinionList.AddRange(cloneList);
                }

                return finalMinionList.Where(m => !this.ignoreMinions.Any(b => b.Equals(m.CharData.BaseSkinName.ToLower())))
                    .ToList();
            }

            private static List<Obj_AI_Minion> OrderEnemyMinions(IEnumerable<Obj_AI_Minion> minions)
            {
                return minions?.OrderByDescending(minion => minion.CharData.BaseSkinName.Contains("Siege"))
                    .ThenBy(minion => minion.CharData.BaseSkinName.Contains("Super")).ThenBy(minion => minion.Health)
                    .ThenByDescending(minion => minion.MaxHealth).ToList();
            }

            private bool IsValidUnit(AttackableUnit unit, float range = 0f)
            {
                return unit.IsValidTarget() && this.Player.Distance(unit)
                       < (range > 0 ? range : GetRealAutoAttackRange(unit));
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

            public bool CanAttacks
            {
                get
                {
                    return this.Player.CanAttack && !this.Player.IsCastingInterruptableSpell()
                           && LastAttackCommandT - Utils.GameTimeTickCount < 0
                           && Utils.GameTimeTickCount + Game.Ping / 2 + 25 >= LastAATick + this.Player.AttackDelay * 1000;
                }
                private set
                {
                    if (value)
                    {
                        this.LastTarget = null;
                        LastAATick = 0;
                    }
                    else
                    {
                        LastAATick = Utils.GameTimeTickCount - Game.Ping / 2;
                        this.isFinishAttack = false;
                        LastMove = 0;
                        this.countAutoAttack++;
                    }
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

            public Vector3 GetOrbwalkingPoint()
            {
                return this._orbwalkingPoint;
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
                    if (this._forcedTarget != null)
                    {
                        if (this._forcedTarget.IsDead || !this._forcedTarget.IsHPBarRendered || !this._forcedTarget.IsValidTarget())
                        {
                            this._forcedTarget = null;
                        }
                    }

                    TimeAdjust = _config.Item("TimeAdjust").GetValue<Slider>().Value;

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

                return minion.CharData.BaseSkinName != "gangplankbarrel";
            }

            private bool ShouldWaitUnderTurret(Obj_AI_Minion noneKillableMinion = null)
            {
                return
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Any(
                            minion =>
                            (noneKillableMinion == null || noneKillableMinion.NetworkId != minion.NetworkId)
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

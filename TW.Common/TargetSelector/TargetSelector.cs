namespace TW.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LeagueSharp.Data.DataTypes;
    using SharpDX;
    using Color = System.Drawing.Color;
    using EloBuddy;
    using Extensions;

    public class TargetSelector
    {
        #region Static Fields

        public static TargetingMode Mode = TargetingMode.AutoPriority;

        private static Menu _configMenu;

        private static int _focusTime;

        private static AIHeroClient _selectedTargetObjAiHero;

        private static string[] StackNames =
            {
                "kalistaexpungemarker", "vaynesilvereddebuff", "twitchdeadlyvenom",
                "ekkostacks", "dariushemo", "gnarwproc", "tahmkenchpdebuffcounter",
                "varuswdebuff",
            };

        private static bool UsingCustom;

        #endregion

        #region Delegates

        public delegate bool TargetSelectionConditionDelegate(AIHeroClient target);

        #endregion

        #region Enum

        public enum DamageType
        {
            Magical,

            Physical,

            True
        }

        public enum TargetingMode
        {
            AutoPriority,

            LowHP,

            MostAD,

            MostAP,

            Closest,

            NearMouse,

            LessAttack,

            LessCast,

            MostStack
        }

        #endregion

        #region Public Properties

        public static bool CustomTS
        {
            get
            {
                return UsingCustom;
            }
            set
            {
                UsingCustom = value;
                if (value)
                {
                    Drawing.OnDraw -= DrawingOnOnDraw;
                }
                else
                {
                    Drawing.OnDraw += DrawingOnOnDraw;
                }
            }
        }

        public static AIHeroClient SelectedTarget
        {
            get
            {
                return (_configMenu != null && _configMenu.Item("FocusSelected").GetValue<bool>()
                            ? _selectedTargetObjAiHero
                            : null);
            }
        }

        #endregion

        #region Public Methods and Operators

        public static void AddToMenu(Menu config)
        {
            config.AddItem(new MenuItem("Alert", "----\u4f7f\u7528 TW.Common \u83dc\u55ae----"));
        }

        /// <summary>
        ///     Returns the priority of the hero
        /// </summary>
        public static float GetPriority(AIHeroClient hero)
        {
            var p = 1;
            if (_configMenu != null && _configMenu.Item("TargetSelector" + hero.ChampionName + "Priority") != null)
            {
                p = _configMenu.Item("TargetSelector" + hero.ChampionName + "Priority").GetValue<Slider>().Value;
            }

            switch (p)
            {
                case 2:
                    return 1.5f;
                case 3:
                    return 1.75f;
                case 4:
                    return 2f;
                case 5:
                    return 2.5f;
                default:
                    return 1f;
            }
        }

        public static AIHeroClient GetSelectedTarget()
        {
            return SelectedTarget;
        }

        public static AIHeroClient GetTarget(
            float range,
            DamageType damageType,
            bool ignoreShield = true,
            IEnumerable<AIHeroClient> ignoredChamps = null,
            Vector3? rangeCheckFrom = null,
            TargetSelectionConditionDelegate conditions = null)
        {
            return GetTarget(
                Player.Instance,
                range,
                damageType,
                ignoreShield,
                ignoredChamps,
                rangeCheckFrom,
                conditions);
        }

        public static AIHeroClient GetTarget(
            Obj_AI_Base champion,
            float range,
            DamageType type,
            bool ignoreShieldSpells = true,
            IEnumerable<AIHeroClient> ignoredChamps = null,
            Vector3? rangeCheckFrom = null,
            TargetSelectionConditionDelegate conditions = null)
        {
            try
            {
                if (ignoredChamps == null)
                {
                    ignoredChamps = new List<AIHeroClient>();
                }

                var damageType = (Damage.DamageType)Enum.Parse(typeof(Damage.DamageType), type.ToString());

                if (_configMenu != null
                    && IsValidTargetLS(
                        SelectedTarget,
                        _configMenu.Item("ForceFocusSelected").GetValue<bool>() ? float.MaxValue : range,
                        type,
                        ignoreShieldSpells,
                        rangeCheckFrom))
                {
                    return SelectedTarget;
                }

                if (_configMenu != null
                    && IsValidTargetLS(
                        SelectedTarget,
                        _configMenu.Item("ForceFocusSelectedKeys").GetValue<bool>() ? float.MaxValue : range,
                        type,
                        ignoreShieldSpells,
                        rangeCheckFrom))
                {
                    if (_configMenu.Item("ForceFocusSelectedK").GetValue<KeyBind>().Active
                        || _configMenu.Item("ForceFocusSelectedK2").GetValue<KeyBind>().Active)
                    {
                        return SelectedTarget;
                    }
                }

                if (_configMenu != null && _configMenu.Item("TargetingMode") != null
                    && Mode == TargetingMode.AutoPriority)
                {
                    var menuItem = _configMenu.Item("TargetingMode").GetValue<StringList>();
                    Enum.TryParse(menuItem.SList[menuItem.SelectedIndex], out Mode);
                }

                var targets =
                    HeroManager.Enemies.FindAll(
                        hero =>
                            ignoredChamps.All(ignored => ignored.NetworkId != hero.NetworkId)
                            && IsValidTargetLS(hero, range, type, ignoreShieldSpells, rangeCheckFrom)
                            && (conditions == null || conditions(hero)));
               
                switch (Mode)
                {
                    case TargetingMode.LowHP:
                        return targets.MinOrDefault(hero => hero.Health);

                    case TargetingMode.MostAD:
                        return targets.MaxOrDefault(hero => hero.BaseAttackDamage + hero.FlatPhysicalDamageMod);

                    case TargetingMode.MostAP:
                        return targets.MaxOrDefault(hero => hero.BaseAbilityDamage + hero.FlatMagicDamageMod);

                    case TargetingMode.Closest:
                        return targets.MinOrDefault(
                            hero => (rangeCheckFrom.HasValue ? rangeCheckFrom.Value : champion.ServerPosition).Distance(
                                hero.ServerPosition,
                                true));

                    case TargetingMode.NearMouse:
                        return targets.MinOrDefault(hero => hero.Distance(Game.CursorPos, true));

                    case TargetingMode.AutoPriority:
                        return
                            targets.MaxOrDefault(
                                hero =>
                                    champion.CalcDamage(hero, damageType, 100) / (1 + hero.Health) * GetPriority(hero));

                    case TargetingMode.LessAttack:
                        return
                            targets.MaxOrDefault(
                                hero =>
                                    champion.CalcDamage(hero, Damage.DamageType.Physical, 100) / (1 + hero.Health)
                                    * GetPriority(hero));

                    case TargetingMode.LessCast:
                        return
                            targets.MaxOrDefault(
                                hero =>
                                    champion.CalcDamage(hero, Damage.DamageType.Magical, 100) / (1 + hero.Health)
                                    * GetPriority(hero));

                    case TargetingMode.MostStack:
                        return
                            targets.MaxOrDefault(
                                hero =>
                                    champion.CalcDamage(hero, damageType, 100) / (1 + hero.Health) * GetPriority(hero)
                                    + (1 + hero.Buffs.Where(b => StackNames.Contains(b.Name.ToLower())).Sum(t => t.Count)));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        public static AIHeroClient GetTargetNoCollision(
            Spell spell,
            bool ignoreShield = true,
            IEnumerable<AIHeroClient> ignoredChamps = null,
            Vector3? rangeCheckFrom = null)
        {
            var t = GetTarget(
                ObjectManager.Player,
                spell.Range,
                spell.DamageType,
                ignoreShield,
                ignoredChamps,
                rangeCheckFrom);

            if (spell.Collision && spell.GetPrediction(t).Hitchance != HitChance.Collision)
            {
                return t;
            }

            return null;
        }

        public static void Initialize()
        {
            CustomEvents.Game.OnGameLoad += args =>
                {
                    var config = new Menu("\u76ee\u6a19\u9078\u64c7", "TargetSelector");

                    _configMenu = config;

                    var focusMenu = new Menu("\u6307\u5b9a\u76ee\u6a19", "FocusTargetSettings");

                    focusMenu.AddItem(new MenuItem("FocusSelected", "\u6307\u5b9a\u76ee\u6a19\u756b\u5708").SetShared().SetValue(true));
                    focusMenu.AddItem(
                        new MenuItem("SelTColor", "\u6307\u5b9a\u76ee\u6a19\u7dda\u5708\u984f\u8272").SetShared()
                            .SetValue(new Circle(true, Color.FromArgb(255, 0, 130))));
                    focusMenu.AddItem(
                        new MenuItem("ForceFocusSelected", "\u53ea\u653b\u64ca\u5df2\u6307\u5b9a\u7684\u76ee\u6a19").SetShared().SetValue(false));
                    focusMenu.AddItem(new MenuItem("sep", ""));
                    focusMenu.AddItem(
                        new MenuItem("ForceFocusSelectedKeys", "\u555f\u52d5\u53ea\u653b\u64ca\u6307\u5b9a\u7684\u76ee\u6a19").SetShared()
                            .SetValue(false));
                    focusMenu.AddItem(new MenuItem("ForceFocusSelectedK", "\u53ea\u653b\u64ca\u6307\u5b9a\u76ee\u6a19\u6309\u9375"))
                        .SetValue(new KeyBind(32, KeyBindType.Press));
                    focusMenu.AddItem(new MenuItem("ForceFocusSelectedK2", "\u53ea\u653b\u64ca\u6307\u5b9a\u76ee\u6a19\u6309\u9375 2"))
                        .SetValue(new KeyBind(32, KeyBindType.Press));
                    focusMenu.AddItem(new MenuItem("ResetOnRelease", "\u6307\u5b9a\u76ee\u6a19\u5f8c\u65bd\u653e"))
                        .SetValue(false);

                    config.AddSubMenu(focusMenu);

                    var autoPriorityItem =
                        new MenuItem("AutoPriority", "\u81ea\u52d5\u6392\u5217\u512a\u5148\u7d1a\u6578").SetShared()
                            .SetValue(true)
                            .SetTooltip("5 = \u6700\u9ad8\u512a\u5148 1 = \u6700\u5f8c\u512a\u5148");
                    autoPriorityItem.ValueChanged += autoPriorityItem_ValueChanged;

                    foreach (var enemy in HeroManager.Enemies)
                    {
                        config.AddItem(
                            new MenuItem("TargetSelector" + enemy.ChampionName + "Priority", enemy.ChampionName.ToTw())
                                .SetShared()
                                .SetValue(
                                    new Slider(
                                        autoPriorityItem.GetValue<bool>() ? GetPriorityFromDb(enemy.ChampionName) : 1,
                                        5,
                                        1)));
                        if (autoPriorityItem.GetValue<bool>())
                        {
                            config.Item("TargetSelector" + enemy.ChampionName + "Priority")
                                .SetValue(
                                    new Slider(
                                        autoPriorityItem.GetValue<bool>() ? GetPriorityFromDb(enemy.ChampionName) : 1,
                                        5,
                                        1));
                        }
                    }
                    config.AddItem(autoPriorityItem);
                    config.AddItem(
                        new MenuItem("TargetingMode", "\u76ee\u6a19\u6a21\u5f0f").SetShared()
                            .SetValue(new StringList(Enum.GetNames(typeof(TargetingMode)))));

                    config.AddItem(new MenuItem("t1", "AutoPriority = \u81ea\u52d5\u512a\u5148"));
                    config.AddItem(new MenuItem("t2", "LowHP = \u4f4e HP"));
                    config.AddItem(new MenuItem("t3", "MostAD = \u512a\u5148 \u7269\u88e1 AD"));
                    config.AddItem(new MenuItem("t4", "MostAP = \u512a\u5148 \u9b54\u6cd5 AP"));
                    config.AddItem(new MenuItem("t5", "Closest = \u512a\u5148\u6700\u8fd1\u76ee\u6a19"));
                    config.AddItem(new MenuItem("t6", "NearMouse = \u512a\u5148\u63a5\u8fd1\u9f20\u6a19"));
                    config.AddItem(new MenuItem("t7", "LessAttack = \u5982\u679c\u81ea\u5df1\u7269\u88e1\u50b7\u5bb3\u53ef\u4ee5\u5bb9\u6613 \u64ca\u6bba"));
                    config.AddItem(new MenuItem("t8", "LessCast = \u5982\u679c\u81ea\u5df1\u9b54\u6cd5\u50b7\u5bb3\u53ef\u4ee5\u5bb9\u6613 \u64ca\u6bba"));
                    config.AddItem(new MenuItem("t9", "MostStack = \u5982\u679c\u81ea\u5df1\u6709\u758a \u88ab\u52d5\u6548\u679c \u5982 \u6c4e \u514b\u9ece"));

                    CommonMenu.Instance.AddSubMenu(config);
                    Game.OnWndProc += GameOnOnWndProc;

                    if (!CustomTS)
                    {
                        Drawing.OnDraw += DrawingOnOnDraw;
                    }
                };
        }

        public static bool IsInvulnerable(Obj_AI_Base target, DamageType damageType, bool ignoreShields = true)
        {
            var targetBuffs = new HashSet<string>(
                target.Buffs.Select(buff => buff.Name),
                StringComparer.OrdinalIgnoreCase);

            // Kindred's Lamb's Respite(R)
            if (targetBuffs.Contains("KindredRNoDeathBuff") && target.HealthPercent <= 10)
            {
                return true;
            }

            // Vladimir W
            if (targetBuffs.Contains("VladimirSanguinePool"))
            {
                return true;
            }

            // Tryndamere's Undying Rage (R)
            if (targetBuffs.Contains("UndyingRage") && target.Health <= target.MaxHealth * 0.10f)
            {
                return true;
            }

            // Kayle's Intervention (R)
            if (targetBuffs.Contains("JudicatorIntervention"))
            {
                return true;
            }

            if (ignoreShields)
            {
                return false;
            }

            // Morgana's Black Shield (E)
            if (targetBuffs.Contains("BlackShield") && damageType.Equals(DamageType.Magical))
            {
                return true;
            }

            // Banshee's Veil (PASSIVE)
            if (targetBuffs.Contains("bansheesveil") && damageType.Equals(DamageType.Magical))
            {
                return true;
            }

            // Sivir's Spell Shield (E)
            if (targetBuffs.Contains("SivirE") && damageType.Equals(DamageType.Magical))
            {
                return true;
            }

            // Nocturne's Shroud of Darkness (W)
            if (targetBuffs.Contains("NocturneShroudofDarkness") && damageType.Equals(DamageType.Magical))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Sets the priority of the hero
        /// </summary>
        public static void SetPriority(AIHeroClient hero, int newPriority)
        {
            if (_configMenu == null || _configMenu.Item("TargetSelector" + hero.ChampionName + "Priority") == null)
            {
                return;
            }
            var p = _configMenu.Item("TargetSelector" + hero.ChampionName + "Priority").GetValue<Slider>();
            p.Value = Math.Max(1, Math.Min(5, newPriority));
            _configMenu.Item("TargetSelector" + hero.ChampionName + "Priority").SetValue(p);
        }

        public static void SetTarget(AIHeroClient hero)
        {
            if (hero.IsValidTarget())
            {
                _selectedTargetObjAiHero = hero;
            }
        }

        public static void Shutdown()
        {
            Menu.Remove(_configMenu);

            Game.OnWndProc -= GameOnOnWndProc;

            if (!CustomTS)
            {
                Drawing.OnDraw -= DrawingOnOnDraw;
            }
        }

        #endregion

        #region Methods

        private static void autoPriorityItem_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            if (!e.GetNewValue<bool>())
            {
                return;
            }
            foreach (var enemy in HeroManager.Enemies)
            {
                _configMenu.Item("TargetSelector" + enemy.ChampionName + "Priority")
                    .SetValue(new Slider(GetPriorityFromDb(enemy.ChampionName), 5, 1));
            }
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            if (_selectedTargetObjAiHero.IsValidTarget() && _configMenu != null
                && _configMenu.Item("FocusSelected").GetValue<bool>()
                && _configMenu.Item("SelTColor").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(
                    _selectedTargetObjAiHero.Position,
                    100,
                    _configMenu.Item("SelTColor").GetValue<Circle>().Color,
                    7,
                    true);
            }

            var a = (_configMenu.Item("ForceFocusSelectedK").GetValue<KeyBind>().Active
                     || _configMenu.Item("ForceFocusSelectedK2").GetValue<KeyBind>().Active)
                    && _configMenu.Item("ForceFocusSelectedKeys").GetValue<bool>();

            _configMenu.Item("ForceFocusSelectedKeys").Permashow(SelectedTarget != null && a);
            _configMenu.Item("ForceFocusSelected").Permashow(_configMenu.Item("ForceFocusSelected").GetValue<bool>());

            if (!_configMenu.Item("ResetOnRelease").GetValue<bool>())
            {
                return;
            }

            if (SelectedTarget != null && !a)
            {
                if (!_configMenu.Item("ForceFocusSelected").GetValue<bool>()
                    && Utils.GameTimeTickCount - _focusTime < 150)
                {
                    if (!a)
                    {
                        _selectedTargetObjAiHero = null;
                    }
                }
            }
            else
            {
                if (a)
                {
                    _focusTime = Utils.GameTimeTickCount;
                }
            }
        }

        private static void GameOnOnWndProc(WndEventArgs args)
        {
            if (args.Msg != (uint)WindowsMessages.WM_LBUTTONDOWN)
            {
                return;
            }
            _selectedTargetObjAiHero =
                HeroManager.Enemies.FindAll(hero => hero.IsValidTarget() && hero.Distance(Game.CursorPos, true) < 40000)
                    // 200 * 200
                    .OrderBy(h => h.Distance(Game.CursorPos, true)).FirstOrDefault();
        }

        private static int GetPriorityFromDb(string championName)
        {
            return LeagueSharp.Data.Data.Get<ChampionPriorityData>().GetPriority(championName);
        }

        private static bool IsValidTargetLS(
            Obj_AI_Base target,
            float range,
            DamageType damageType,
            bool ignoreShieldSpells = true,
            Vector3? rangeCheckFrom = null)
        {
            return target.IsValidTarget()
                   && target.Distance(rangeCheckFrom ?? Player.Instance.ServerPosition, true)
                   < Math.Pow(range <= 0 ? Orbwalking.GetRealAutoAttackRange(target) : range, 2)
                   && !IsInvulnerable(target, damageType, ignoreShieldSpells);
        }

        #endregion
    }

    /// <summary>
    ///     This TS attempts to always lock the same target, useful for people getting targets for each spell, or for champions
    ///     that have to burst 1 target.
    /// </summary>
    public class LockedTargetSelector
    {
        #region Static Fields

        public static AIHeroClient _lastTarget;

        private static TargetSelector.DamageType _lastDamageType;

        #endregion

        #region Public Methods and Operators

        public static void AddToMenu(Menu menu)
        {
            TargetSelector.AddToMenu(menu);
        }

        public static AIHeroClient GetTarget(
            float range,
            TargetSelector.DamageType damageType,
            bool ignoreShield = true,
            IEnumerable<AIHeroClient> ignoredChamps = null,
            Vector3? rangeCheckFrom = null)
        {
            if (_lastTarget == null || !_lastTarget.IsValidTarget() || _lastDamageType != damageType)
            {
                var newTarget = TargetSelector.GetTarget(range, damageType, ignoreShield, ignoredChamps, rangeCheckFrom);

                _lastTarget = newTarget;
                _lastDamageType = damageType;

                return newTarget;
            }

            if (_lastTarget.IsValidTarget(range) && damageType == _lastDamageType)
            {
                return _lastTarget;
            }

            var newTarget2 = TargetSelector.GetTarget(range, damageType, ignoreShield, ignoredChamps, rangeCheckFrom);

            _lastTarget = newTarget2;
            _lastDamageType = damageType;

            return newTarget2;
        }

        /// <summary>
        ///     Unlocks the currently locked target.
        /// </summary>
        public static void UnlockTarget()
        {
            _lastTarget = null;
        }

        #endregion
    }
}

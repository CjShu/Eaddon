namespace Syndra
{
    using EloBuddy;
    using EloBuddy.SDK.Events;
    using LeagueSharp.Common;
    using System;
    using System.Linq;
    using Common;
    using System.Collections.Generic;
    using SharpDX;

    public class Program
    {
        public static Menu Menu;
        internal static AIHeroClient player => HeroManager.Player;
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q, W, E, EQ, R;
        private static SpellSlot Ignite;
        private static int qeComboT, weComboT;

        static void Main(string[] args)
        {
            /// 注入接口
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            /// 如果不是星朵拉名稱英雄 不注入此腳本
            if (Player.Instance.ChampionName != "Syndra")
                return;

            InitSpells();
            InitMenuu();
            InitEvents();

        }

        private static void InitSpells()
        {
            Q = new Spell(SpellSlot.Q, 790f);
            W = new Spell(SpellSlot.W, 925f);
            E = new Spell(SpellSlot.E, 700f);
            EQ = new Spell(SpellSlot.Q, Q.Range + 475f);
            R = new Spell(SpellSlot.R, 675f);

            Q.SetSkillshot(0.6f, 125f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.25f, 140f, 1600f, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, (float)(45 * 0.5), 2500f, false, SkillshotType.SkillshotCircle);
            EQ.SetSkillshot(float.MaxValue, 55f, 2000f, false, SkillshotType.SkillshotCircle);

            Ignite = player.GetSpellSlot("SummonerDot");

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }

        private static void InitMenuu()
        {
            ///菜單名稱
            (Menu = new Menu("CjShu 星朵拉", "ShuSyndra", true)).AddToMainMenu();

            /// 菜單走砍
            Menu.AddSubMenu(new Menu("走砍設置", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalking"));

            Tools.Tools.Inject();

            /// 菜單名稱後擴充
            var ComboMenu = Menu.AddSubMenu(new Menu("連招設定", "ComboMenu"));
            ComboMenu.AddItem(new MenuItem("UseQCombo", "使用 Q", true).SetValue(true));
            ComboMenu.AddItem(new MenuItem("UseWCombo", "使用 W", true).SetValue(true));
            ComboMenu.AddItem(new MenuItem("UseECombo", "使用 E", true).SetValue(true));
            ComboMenu.AddItem(new MenuItem("UseQECombo", "使用 QE", true).SetValue(true));
            ComboMenu.AddItem(new MenuItem("UseRCombo", "使用 R", true).SetValue(true));
            ComboMenu.AddItem(new MenuItem("UseIgniteCombo", "連招使用點燃", true).SetValue(true));

            var HarassMenu = Menu.AddSubMenu(new Menu("騷擾設定", "HarassMenu"));
            HarassMenu.AddItem(new MenuItem("UseQHarass", "使用 Q", true).SetValue(true));
            HarassMenu.AddItem(new MenuItem("UseWHarass", "使用 W", true).SetValue(true));
            HarassMenu.AddItem(new MenuItem("UseEHarass", "使用 E", true).SetValue(true));
            HarassMenu.AddItem(new MenuItem("UseQEHarass", "使用 QE", true).SetValue(true));
            HarassMenu.AddItem(new MenuItem("HarassMana", "騷擾 最低魔力 > = %", true).SetValue(new
                Slider(40, 0, 100)));
            ManaManager.AddSpellHarass(HarassMenu);

            var LaneClearMenu = Menu.AddSubMenu(new Menu("清線設定", "LaneClearMenu"));
            LaneClearMenu.AddItem(new MenuItem("UseQFarm", "Q 模式 :").SetValue(new
                StringList(new[] { "控線", "清線", "兩者都開", "關閉" }, 2)));
            LaneClearMenu.AddItem(new MenuItem("QMinMinions", "Q 最低命中小兵數量 >= x", true).SetValue(new
                Slider(3, 1, 4)));

            LaneClearMenu.AddItem(new MenuItem("UseWFarm", "W 模式 :").SetValue(new
                StringList(new[] { "控線", "清線", "兩者都開", "關閉" }, 1)));
            LaneClearMenu.AddItem(new MenuItem("WMinMinions", "W 最低命中小兵數量 >= x", true).SetValue(new
                Slider(3, 1, 8)));
            LaneClearMenu.AddItem(new MenuItem("LaneClearMana", "清線 最低魔力 > = %", true).SetValue(new
                Slider(40, 0, 100)));
            ManaManager.AddSpellFarm(LaneClearMenu);

            var JungleMenu = LaneClearMenu.AddSubMenu(new Menu("打野設定", "JungleMenu"));
            JungleMenu.AddItem(new MenuItem("UseQJFarm", "使用 Q", true).SetValue(true));
            JungleMenu.AddItem(new MenuItem("UseWJFarm", "使用 W", true).SetValue(true));
            JungleMenu.AddItem(new MenuItem("UseEJFarm", "使用 E", true).SetValue(true));

            var PredMenu = Menu.AddSubMenu(new Menu("預測設定", "PredMenu"));
            PredMenu.AddItem(new MenuItem("2", "預測 選擇(切換後重新按 F5)"));
            PredMenu.AddItem(new MenuItem("SelectPred", "選擇預測 : ").SetValue(new
                StringList(new[] { "邏輯 預測", "Common", "SDK 預測" })));

            var MiscMenu = Menu.AddSubMenu(new Menu("其他設定", "MiscMenu"));
            MiscMenu.AddItem(new MenuItem("InterruptSpells", "使用技能打斷目標").SetValue(true));
            MiscMenu.AddItem(new MenuItem("CastQEKey", "使用 QE 鼠標位置", true).SetValue(new
                KeyBind('T', KeyBindType.Press)).SetTooltip("按鍵活性", Color.Red));
            MiscMenu.AddItem(new MenuItem("InstantQEKey", "使用 QE 中斷目標", true).SetValue(new
                KeyBind('T', KeyBindType.Press)).SetTooltip("按鍵活性", Color.Red));
            MiscMenu.AddItem(new MenuItem("3", "不使用 R 在目標英雄上")).SetTooltip("敵人");
            if (HeroManager.Enemies.Any())
            {
                HeroManager.Enemies.ForEach(
                    i
                     => MiscMenu.AddItem(new MenuItem("DontUlt" + i.ChampionName.ToLower(),
                     i.ChampionName, true).SetValue(false)));
            }

            var QEMenu = Menu.AddSubMenu(new Menu("QE設定", "QEMenu"));
            QEMenu.AddItem(new MenuItem("AntiGapcloserQE", "反突進QE", true).SetValue(true));
            QEMenu.AddItem(new MenuItem("QEGAPSCLRSF", "反突進QE 名單"));
            if (HeroManager.Enemies.Any())
            {
                HeroManager.Enemies.ForEach(
                    i
                     => QEMenu.AddItem(new MenuItem("Egapcloser" + i.ChampionName.ToLower(),
                     i.ChampionName, true).SetValue(false)));
            }

            var LastMenu = Menu.AddSubMenu(new Menu("補刀設定", "LastMenu"));
            LastMenu.AddItem(new MenuItem("LastHitQ", "補刀 Q", true).SetValue(true));

            var DrawMenu = Menu.AddSubMenu(new Menu("顯示設定", "DrawMenu"));
            DrawMenu.AddItem(new MenuItem("QDraw", "Q 範圍").SetValue(new Circle(false, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            DrawMenu.AddItem(new MenuItem("WDraw", "W 範圍").SetValue(new Circle(false, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            DrawMenu.AddItem(new MenuItem("EDraw", "E 範圍").SetValue(new Circle(false, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            DrawMenu.AddItem(new MenuItem("RDraw", "R 範圍").SetValue(new Circle(false, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            DrawMenu.AddItem(new MenuItem("QEDraw", "QE 範圍").SetValue(new Circle(false, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            DamageIndicator.AddToMenu(DrawMenu);
            ManaManager.AddDrawFarm(DrawMenu);

            Manager.WriteConsole(Player.Instance.ChampionName + "CjShu");
        }

        private static void InitEvents()
        {
            Game.OnUpdate += OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Interrupter2.OnInterruptableTarget += OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Drawing.OnDraw += OnDraw;
            Orbwalking.BeforeAttack += BeforeAttack;
        }

        private static void OnEnemyGapcloser(ActiveGapcloser target)
        {
            if (Menu.GetBool("AntiGapcloserQE") && E.IsReady())
            {
                if (Menu.Item("Egapcloser" + target.Sender.ChampionName.ToLower(), true).GetValue<bool>())
                {
                    if (Q.IsReady())
                    {
                        Q.Cast(target.End, true);
                    }
                    else if (target.Sender.IsValidTarget(E.Range))
                    {
                        E.Cast(target.End, true);
                    }
                }
            }
        }

        private static void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (Manager.InCombo)
            {
                args.Process = !(Q.IsReady() || W.IsReady());
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (!player.IsDead && !MenuGUI.IsChatOpen && !Chat.IsOpen)
            {
                if (Menu.Item("QEDraw").GetValue<Circle>().Active)
                {
                    Render.Circle.DrawCircle(player.Position, EQ.Range, Menu.Item("QEDraw").GetValue<Circle>().Color);
                }

                foreach (var spell in SpellList)
                {
                    var menu = Menu.Item(spell.Slot + "Draw").GetValue<Circle>();

                    if (menu.Active)
                    {
                        Render.Circle.DrawCircle(player.Position, spell.Range, menu.Color);
                    }
                }
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            R.Range = R.Level == 3 ? 750f : 675f;

            if (player.IsDead)
                return;

            QELogicKey();

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    ComboLogic();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    FarmHarass();
                    LancClearLogic(true);
                    JungleClearLogic();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    LastHitLogic();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    HarassLogic();
                    break;
            }
        }
      
        private static void OnInterruptableTarget(AIHeroClient sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            var target = sender;

            if (!Menu.Item("InterruptSpells").GetValue<bool>())
            {
                return;
            }
            if (player.Distance(target) < E.Range && E.IsReady())
            {
                Q.CastTo(target);
                E.CastTo(target);
            }
            else if (player.Distance(target) < EQ.Range && E.IsReady() && Q.IsReady())
            {
                UseQe(target);
            }
        }
       
        #region Logic

        private static void QELogicKey()
        {
            if (Menu.GetKey("CastQEKey", true) && E.IsReady() && Q.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(
                    enemy => enemy.IsValidTarget(EQ.Range  - 50) && Game.CursorPos.Distance(enemy.ServerPosition) < 300))
                {
                    UseQe(enemy);
                }
            }

            if (Menu.GetKey("InstantQEKey", true))
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

                var t = TargetSelector.GetTarget(EQ.Range, TargetSelector.DamageType.Magical);

                if (t.IsValidTarget() && E.IsReady() && Q.IsReady())
                {
                    UseQe(t);
                }
            }
        }

        private static void ComboLogic()
        {
            if (Q.IsReady())
            {
                QCast(Menu.GetBool("UseQCombo"));
            }

            if (W.IsReady())
            {
                WCast(Menu.GetBool("UseWCombo"));
            }

            if (E.IsReady())
            {
                ECast(Menu.GetBool("UseECombo"));
            }

            if (R.IsReady())
            {
                RCast(Menu.GetBool("UseRCombo"), Menu.GetBool("UseIgniteCombo"));
            }

            if (Q.IsReady() && E.IsReady())
            {
                QCast(Menu.GetBool("UseQECombo"));
                ECast(Menu.GetBool("UseQECombo"));
            }
        }

        private static void HarassLogic()
        {
            if (ManaManager.HasEnoughMana(Menu.GetSlider("HarassMana")))
            {
                if (Q.IsReady())
                {
                    QCast(Menu.GetBool("UseQHarass"));
                }

                if (W.IsReady())
                {
                    WCast(Menu.GetBool("UseWHarass"));
                }

                if (E.IsReady())
                {
                    ECast(Menu.GetBool("UseEHarass"));
                }

                if (Q.IsReady() && E.IsReady())
                {
                    QCast(Menu.GetBool("UseQEHarass"));
                    ECast(Menu.GetBool("UseQEHarass"));
                }
            }
        }
        
        private static void LancClearLogic(bool laneClear)
        {
            if (!Orbwalking.CanMove(40))
            {
                return;
            }

            var useQi = Menu.Item("UseQFarm").GetValue<StringList>().SelectedIndex;
            var useWi = Menu.Item("UseWFarm").GetValue<StringList>().SelectedIndex;
            var useQ = (laneClear && (useQi == 1 || useQi == 2)) || (!laneClear && (useQi == 0 || useQi == 2));
            var useW = (laneClear && (useWi == 1 || useWi == 2)) || (!laneClear && (useWi == 0 || useWi == 2));

            if (ManaManager.HasEnoughMana(Menu.GetSlider("LaneClearMana")) && ManaManager.SpellFarm)
            {
                var allMinionsQ = MinionManager.GetMinions(player.ServerPosition, Q.Range + Q.Width + 30, MinionTypes.Ranged);
                var allMinionsW = MinionManager.GetMinions(player.ServerPosition, W.Range + W.Width + 30, MinionTypes.Ranged);
                var fll = Q.GetCircularFarmLocation(allMinionsQ, Q.Width);

                if (useQ && Q.IsReady())
                {
                    if (laneClear)
                    {
                        if (fll.MinionsHit >= Menu.GetSlider("QMinMinions"))
                        {
                            Q.Cast(fll.Position, true);
                        }
                    }
                    else
                    {
                        foreach (var minion in allMinionsQ.Where(minion
                            => !Orbwalking.InAutoAttackRange(minion) && minion.Health < 0.75 * Q.GetDamage(minion)))
                        {
                            Q.CastTo(minion);
                        }
                    }
                }

                if (useW && W.IsReady() && allMinionsW.Count > Menu.GetSlider("WMinMinions"))
                {
                    if (laneClear)
                    {
                        if (player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1)
                        {
                            var WPos = GetGrabableObjectPos(false);

                            if (WPos.To2D().IsValid() && Utils.TickCount - W.LastCastAttemptT > Game.Ping + 150)
                            {
                                W.Cast(WPos, true);
                            }
                        }
                        else if (player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 1)
                        {
                            if (fll.MinionsHit >= Menu.GetSlider("WMinMinions") && W.IsInRange(fll.Position.To3D()))
                            {
                                W.Cast(fll.Position);
                            }
                        }
                    }
                }
            }
        }

        private static void FarmHarass()
        {
            if (ManaManager.SpellHarass)
            {
                HarassLogic();
            }
        }
     
        private static void JungleClearLogic()
        {
            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            var mob = mobs[0];

            if (Menu.GetBool("UseQJFarm") && Q.IsReady())
            {
                if (mobs.Count > 0)
                {
                    Q.CastTo(mob);
                }
            }

            if (Menu.GetBool("UseWJFarm") && W.IsReady())
            {
                if (Utils.TickCount - Q.LastCastAttemptT > 790)
                {
                    W.CastTo(mob);
                }
            }

            if (Menu.GetBool("UseEJFarm") && E.IsReady())
            {
                E.CastTo(mob);
            }
        }

        private static void LastHitLogic()
        {
            if (Menu.GetBool("LastHitQ") && Q.IsReady())
            {
                var minions = MinionManager.GetMinions(player.Position, Q.Range)
                    .Where(
                    x => x.Distance(player) <= Q.Range && x.Distance(player) > Orbwalking.GetRealAutoAttackRange(player)
                    && x.Health < Q.GetDamage(x));

                if (minions.Any())
                {
                    Q.Cast(minions.FirstOrDefault(), true);
                }
            }
        }

        private static void UseE(Obj_AI_Base enemy)
        {
            foreach (var orb in Lib.Core.OrbManager.GetOrbs(true))
                if (player.Distance(orb) < E.Range + 100)
                {
                    var startPoint = orb.To2D().Extend(player.ServerPosition.To2D(), 100);
                    var endPoint = player.ServerPosition.To2D()
                        .Extend(orb.To2D(), player.Distance(orb) > 200 ? 1300 : 1000);
                    EQ.Delay = E.Delay + player.Distance(orb) / E.Speed;
                    EQ.From = orb;
                    var enemyPred = EQ.GetPrediction(enemy);
                    if (enemyPred.Hitchance >= HitChance.High
                        && enemyPred.UnitPosition.To2D().Distance(startPoint, endPoint, false)
                        < EQ.Width + enemy.BoundingRadius)
                    {
                        E.Cast(orb, true);
                        W.LastCastAttemptT = Utils.TickCount;
                        return;
                    }
                }
        }

        private static void UseQe(Obj_AI_Base enemy)
        {
            EQ.Delay = E.Delay + Q.Range / E.Speed;
            EQ.From = player.ServerPosition.To2D().Extend(enemy.ServerPosition.To2D(), Q.Range).To3D();

            var prediction = EQ.GetPrediction(enemy);
            if (prediction.Hitchance >= HitChance.High)
            {
                Q.Cast(player.ServerPosition.To2D().Extend(prediction.CastPosition.To2D(), Q.Range - 100), true);
                qeComboT = Utils.TickCount;
                W.LastCastAttemptT = Utils.TickCount;
            }
        }

        #endregion
     
        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (Utils.TickCount - qeComboT < 500 && args.SData.Name.Equals("SyndraQ", StringComparison.InvariantCultureIgnoreCase))
            {
                W.LastCastAttemptT = Utils.TickCount + 400;
                E.Cast(args.End);
            }

            if (Utils.TickCount - weComboT < 500 && (args.SData.Name.Equals("SyndraW", StringComparison.InvariantCultureIgnoreCase) || args.SData.Name.Equals("SyndraWCast", StringComparison.InvariantCultureIgnoreCase)))
            {
                W.LastCastAttemptT = Utils.TickCount + 400;
                E.Cast(args.End);
            }
        }

        private static Vector3 GetGrabableObjectPos(bool onlyOrbs)
        {
            if (!onlyOrbs)
            {
                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget(W.Range)))
                {
                    return minion.ServerPosition;
                }
            }
            return Lib.Core.OrbManager.GetOrbToGrab((int)W.Range);
        }

        /// <summary>
        /// Q技能鑄造
        /// </summary>
        /// <param name="useQ"></param>
        private static void QCast(bool useQ)
        {
            //目標選擇 Q範圍 寬度 數據為魔法傷害
            var target = TargetSelector.GetSelectedTarget() ??
                TargetSelector.GetTarget(Q.Range + Q.Width / 3 / Q.Width, TargetSelector.DamageType.Magical);

            // 目標
            if (target != null && useQ)
            {
                /// 鑄造 Q 對目標
                Q.CastTo(target);
            }
        }

        /// <summary>
        /// W技能鑄造
        /// </summary>
        /// <param name="useW"></param>
        private static void WCast(bool useW)
        {
            var target = TargetSelector.GetSelectedTarget() ??
                TargetSelector.GetTarget(W.Range + Q.Width, TargetSelector.DamageType.Magical);

            if (useW)
            {
                if (player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1 && target.Check(EQ.Range))
                {
                    var WPos = GetGrabableObjectPos(target == null);

                    if (WPos.To2D().IsValid() && Utils.TickCount - W.LastCastAttemptT
                        > Game.Ping + 300 && Utils.TickCount - E.LastCastAttemptT > Game.Ping + 600)
                    {
                        W.Cast(WPos, true);
                        W.LastCastAttemptT = Utils.TickCount;
                    }
                }
                else if (target != null && player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 1 && Utils.TickCount - W.LastCastAttemptT > Game.Ping + 100)
                {
                    if (Lib.Core.OrbManager.WObject(false) != null)
                    {
                        W.From = Lib.Core.OrbManager.WObject(false).ServerPosition;
                        W.CastTo(target);
                    }
                }
            }
        }

        /// <summary>
        /// E技能鑄造
        /// </summary>
        /// <param name="useE"></param>
        private static void ECast(bool useE)
        {
            var Wtarget = TargetSelector.GetSelectedTarget() ??
                TargetSelector.GetTarget(W.Range + Q.Width, TargetSelector.DamageType.Magical);
            var QEtarget = TargetSelector.GetSelectedTarget() ??
                TargetSelector.GetTarget(EQ.Range, TargetSelector.DamageType.Magical);

            if (Utils.TickCount - W.LastCastAttemptT > Game.Ping + 150 && useE)
            {
                foreach (var enemy in HeroManager.Enemies)
                {
                    if (enemy.IsValidTarget(EQ.Range))
                    {
                        UseE(enemy);
                    }
                }
            }

            if (Wtarget == null && QEtarget != null && useE && Lib.Core.OrbManager.WObject(true) != null)
            {
                EQ.Delay = E.Delay + Q.Range / W.Speed;
                EQ.From = player.ServerPosition.To2D().Extend(QEtarget.ServerPosition.To2D(), Q.Range).To3D();
                var prediction = EQ.GetPrediction(QEtarget);
                if (prediction.Hitchance >= HitChance.High)
                {
                    W.Cast(player.ServerPosition.To2D().Extend(prediction.CastPosition.To2D(), Q.Range - 100));
                    weComboT = Utils.TickCount;
                }
            }
        }

        /// <summary>
        /// R 技能鑄造
        /// </summary>
        /// <param name="useR"></param>
        private static void RCast(bool useR, bool useIgnite)
        {
            var target = TargetSelector.GetSelectedTarget() ??
                TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            var comboDmg = target != null ? Common.DamageCalculate.GetComboDamage(target) : 0;


            if (target != null && useR)
            {
                useR = (Menu.Item("DontUlt" + target.ChampionName.ToLower()) != null && Menu.Item("DontUlt" + target.ChampionName.ToLower()).GetValue<bool>() == false);
            }

            if (target != null && useR && comboDmg > target.Health && !target.IsZombie && !Q.IsReady())
            {
                R.CastTo(target);
            }

            // 點燃鑄造
            if (target != null && useIgnite && Ignite != SpellSlot.Unknown && Ignite.IsReady())
            {
                if (DamageCalculate.GetIgniteDmage(target) > target.Health)
                {
                    player.Spellbook.CastSpell(Ignite, target);
                }
            }
        }
    }
}
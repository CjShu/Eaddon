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
        private static AIHeroClient player => HeroManager.Player;
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
            //InitEvents();

        }

        private static void InitSpells()
        {
            ///技能 + 範圍f
            Q = new Spell(SpellSlot.Q, 790f);
            W = new Spell(SpellSlot.W, 925f);
            E = new Spell(SpellSlot.E, 700f);
            EQ = new Spell(SpellSlot.Q, Q.Range + 475f);
            R = new Spell(SpellSlot.R, 675f);

            /// 技能 延遲 寬度 速度  碰撞 真 假 類型線 圈 圓 
            Q.SetSkillshot(0.6f, 125f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.25f, 140f, 1600f, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, (float)(45 * 0.5), 2500f, false, SkillshotType.SkillshotCircle);
            EQ.SetSkillshot(float.MaxValue, 55f, 2000f, false, SkillshotType.SkillshotCircle);

            /// 點燃
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
            ComboMenu.AddItem(new MenuItem("UseQCombo", "使用 Q").SetValue(true));
            ComboMenu.AddItem(new MenuItem("UseWCombo", "使用 W").SetValue(true));
            ComboMenu.AddItem(new MenuItem("UseECombo", "使用 E").SetValue(true));
            ComboMenu.AddItem(new MenuItem("UseQECombo", "使用 QE").SetValue(true));
            ComboMenu.AddItem(new MenuItem("UseRCombo", "使用 R").SetValue(true));
            ComboMenu.AddItem(new MenuItem("UseIgniteCombo", "連招使用點燃").SetValue(true));

            var HarassMenu = Menu.AddSubMenu(new Menu("騷擾設定", "HarassMenu"));
            HarassMenu.AddItem(new MenuItem("UseQHarass", "使用 Q").SetValue(true));
            HarassMenu.AddItem(new MenuItem("UseWHarass", "使用 W").SetValue(true));
            HarassMenu.AddItem(new MenuItem("UseEHarass", "使用 E").SetValue(true));
            HarassMenu.AddItem(new MenuItem("UseQEHarass", "使用 QE").SetValue(true));
            HarassMenu.AddItem(new MenuItem("HarassMana", "騷擾 最低魔力 > = %").SetValue(new
                Slider(40, 0, 100)));


            var LaneClearMenu = Menu.AddSubMenu(new Menu("清線設定", "LaneClearMenu"));
            LaneClearMenu.AddItem(new MenuItem("UseQFarm", "Q 模式 :").SetValue(new
                StringList(new[] { "控線", "清線", "兩者都開", "關閉" }, 2)));

            LaneClearMenu.AddItem(new MenuItem("UseWFarm", "W 模式 :").SetValue(new
                StringList(new[] { "控線", "清線", "兩者都開", "關閉" }, 1)));
            LaneClearMenu.AddItem(new MenuItem("LaneClearMana", "清線 最低魔力 > = %").SetValue(new
                Slider(40, 0, 100)));

            var JungleMenu = LaneClearMenu.AddSubMenu(new Menu("打野設定", "JungleMenu"));
            JungleMenu.AddItem(new MenuItem("UseQJFarm", "使用 Q").SetValue(true));
            JungleMenu.AddItem(new MenuItem("UseWJFarm", "使用 W").SetValue(true));
            JungleMenu.AddItem(new MenuItem("UseEJFarm", "使用 E").SetValue(true));

            var PredMenu = Menu.AddSubMenu(new Menu("預測設定", "PredMenu"));
            PredMenu.AddItem(new MenuItem("2", "預測 選擇(切換後重新按 F5)"));
            PredMenu.AddItem(new MenuItem("SelectPred", "選擇預測 : ").SetValue(new
                StringList(new[] { "邏輯 預測", "Common", "SDK 預測" })));

            var MiscMenu = Menu.AddSubMenu(new Menu("其他設定", "MiscMenu"));
            MiscMenu.AddItem(new MenuItem("InterruptSpells", "使用技能打斷目標").SetValue(true));
            MiscMenu.AddItem(new MenuItem("CastQEKey", "使用 QE 鼠標位置").SetValue(new
                KeyBind('T', KeyBindType.Press)).SetTooltip("按鍵活性", Color.Red));
            MiscMenu.AddItem(new MenuItem("InstantQEKey", "使用 QE 中斷目標").SetValue(new
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
            QEMenu.AddItem(new MenuItem("AntiGapcloserQE", "反突進QE").SetValue(true));
            QEMenu.AddItem(new MenuItem("QEGAPSCLRSF", "反突進QE 名單"));
            if (HeroManager.Enemies.Any())
            {
                HeroManager.Enemies.ForEach(
                    i
                     => QEMenu.AddItem(new MenuItem("Egapcloser" + i.ChampionName.ToLower(),
                     i.ChampionName, true).SetValue(false)));
            }


            var DrawMenu = Menu.AddSubMenu(new Menu("顯示設定", "DrawMenu"));
            DrawMenu.AddItem(new MenuItem("QDraw", "Q 範圍").SetValue(new Circle(false, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            DrawMenu.AddItem(new MenuItem("WDraw", "W 範圍").SetValue(new Circle(false, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            DrawMenu.AddItem(new MenuItem("EDraw", "E 範圍").SetValue(new Circle(false, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            DrawMenu.AddItem(new MenuItem("RDraw", "R 範圍").SetValue(new Circle(false, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            DrawMenu.AddItem(new MenuItem("QEDraw", "QE 範圍").SetValue(new Circle(false, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            DamageIndicator.AddToMenu(DrawMenu);


            Menu.AddItem(new MenuItem("3", "腳本未寫完請物使用此腳本"));
            Manager.WriteConsole(Player.Instance.ChampionName + "CjShu");
        }

        
        private static void InitEvents()
        {
           //Game.OnUpdate += OnUpdate;
           //Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
           //Interrupter2.OnInterruptableTarget += OnInterruptableTarget;
           //Drawing.OnEndScene += OnEndScene;
            Drawing.OnDraw += OnDraw;
            Orbwalking.BeforeAttack += BeforeAttack;
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

        /*
        
        private static void OnUpdate(EventArgs args)
        {
            if (player.IsDead)
            {
                return;
            }

            R.Range = R.Level == 3 ? 750f : 675f;

            if (Menu["Misc"]["CastQE"].GetValue<MenuKeyBind>().Active && E.IsReady() && Q.IsReady())
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(EQ.Range) && Game.CursorPos.Distance(enemy.ServerPosition) < 300))
                {
                    UseQe(enemy);
                }
            }

            if (Menu["Misc"]["Key.InstantQE"].GetValue<MenuKeyBind>().Active)
            {
                InstantQe2Enemy();
            }

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkingMode.Combo:
                    Combo();
                    break;
                case OrbwalkingMode.Hybrid:
                    Harass();
                    break;
                case OrbwalkingMode.LaneClear:
                    Farm(true);
                    JungleFarm();
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

        private static void InstantQe2Enemy()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            var t = GetTarget(EQ.Range, DamageType.Magical);
            if (t.IsValidTarget() && E.IsReady() && Q.IsReady())
            {
                UseQe(t);
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
                        E.Cast(orb);
                        W.LastCastAttemptT = Variables.TickCount;
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
                Q.Cast(player.ServerPosition.To2D().Extend(prediction.CastPosition.To2D(), Q.Range - 100));
                qeComboT = Variables.TickCount;
                W.LastCastAttemptT = Variables.TickCount;
            }
        }

        #endregion

        private static void OnEndScene(EventArgs args)
        {
            if (!Menu["Draw"]["DrawDamage"])
                return;

            foreach (var enemy in ObjectManager.Get<AIHeroClient>().Where(e => e.IsValidTarget() && !e.IsZombie))
            {
                HpBarDraw.Unit = enemy;
                HpBarDraw.DrawDmg(Lib.Core.Helper.TotalDamage(enemy), SharpDX.Color.Cyan);
            }
        }
     
        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (Variables.TickCount - qeComboT < 500 && args.SData.Name.Equals("SyndraQ", StringComparison.InvariantCultureIgnoreCase))
            {
                W.LastCastAttemptT = Variables.TickCount + 400;
                E.Cast(args.End);
            }

            if (Variables.TickCount - weComboT < 500 && (args.SData.Name.Equals("SyndraW", StringComparison.InvariantCultureIgnoreCase) || args.SData.Name.Equals("SyndraWCast", StringComparison.InvariantCultureIgnoreCase)))
            {
                W.LastCastAttemptT = Variables.TickCount + 400;
                E.Cast(args.End);
            }
        }

        private static double GetIgniteDamage(AIHeroClient target)
        {
            return 50 + 20 * GameObjects.Player.Level - (target.HPRegenRate / 5 * 3);
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

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;
            damage += Q.IsReady(420) ? Q.GetDamage(enemy) : 0;
            damage += W.IsReady() ? W.GetDamage(enemy) : 0;
            damage += E.IsReady() ? E.GetDamage(enemy) : 0;

            if (R.IsReady())
            {
                damage += Math.Min(7, player.Spellbook.GetSpell(SpellSlot.R).Ammo) * player.GetSpellDamage(enemy, SpellSlot.R);
            }
            return (float)damage;
        }

        private static void UseSpells(bool useQ, bool useW, bool useE, bool useR, bool useQe, bool useIgnite, bool isHarass)
        {
            var qTarget = GetTarget(Q.Range + (isHarass ? Q.Width / 3 : Q.Width), DamageType.Magical);
            var wTarget = GetTarget(W.Range + W.Width, DamageType.Magical);
            var rTarget = GetTarget(R.Range, DamageType.Magical);
            var qeTarget = GetTarget(EQ.Range, DamageType.Magical);
            var comboDamage = rTarget != null ? GetComboDamage(rTarget) : 0;


            if (qTarget != null && useQ)
            {
                Q.CastTo(qTarget);
            }

            if (Variables.TickCount - W.LastCastAttemptT > Game.Ping + 150 && E.IsReady() && useE)
            {
                foreach (var enemy in GameObjects.EnemyHeroes)
                {
                    if (enemy.IsValidTarget(EQ.Range))
                    {
                        UseE(enemy);
                    }
                }
            }

            if (useW)
            {
                if (player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1 && W.IsReady() && qeTarget != null)
                {
                    var gObjectPos = GetGrabableObjectPos(wTarget == null);

                    if (gObjectPos.To2D().IsValid() && Variables.TickCount - W.LastCastAttemptT > Game.Ping + 300 && Variables.TickCount - E.LastCastAttemptT > Game.Ping + 600)
                    {
                        W.Cast(gObjectPos);
                        W.LastCastAttemptT = Variables.TickCount;
                    }
                }
                else if (wTarget != null && player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 1 && W.IsReady() && Variables.TickCount - W.LastCastAttemptT > Game.Ping + 100)
                {
                    if (Lib.Core.OrbManager.WObject(false) != null)
                    {
                        W.From = Lib.Core.OrbManager.WObject(false).ServerPosition;
                        W.CastTo(wTarget);
                    }
                }
            }

            if (rTarget != null && useR)
            {
                useR = (Menu["Misc"]["DontUlt" + rTarget.CharData.BaseSkinName]) != null && Menu["Misc"]["DontUlt" + rTarget.CharData.BaseSkinName].GetValue<MenuBool>() == false;
            }

            if (rTarget != null && useR && R.IsReady() && comboDamage > rTarget.Health && !rTarget.IsZombie && !Q.IsReady())
            {
                R.CastTo(rTarget);
            }

            if (rTarget != null && useIgnite && Ignite != SpellSlot.Unknown && Ignite.IsReady())
            {
                if (GetIgniteDamage(rTarget) > rTarget.Health)
                {
                    player.Spellbook.CastSpell(Ignite, rTarget);
                }
            }

            if (wTarget == null && qeTarget != null && Q.IsReady() && E.IsReady() && useQe)
            {
                UseQe(qeTarget);
            }

            if (wTarget == null && qeTarget != null && E.IsReady() && useE && Lib.Core.OrbManager.WObject(true) != null)
            {
                EQ.Delay = E.Delay + Q.Range / W.Speed;
                EQ.From = player.ServerPosition.To2D().Extend(qeTarget.ServerPosition.To2D(), Q.Range).To3D();
                var prediction = EQ.GetPrediction(qeTarget);
                if (prediction.Hitchance >= HitChance.High)
                {
                    W.Cast(player.ServerPosition.To2D().Extend(prediction.CastPosition.To2D(), Q.Range - 100));
                    weComboT = Variables.TickCount;
                }
            }

        }

        private static void Combo()
        {
            UseSpells(Menu["Combo"]["UseQCombo"].GetValue<MenuBool>(), Menu["Combo"]["UseWCombo"].GetValue<MenuBool>(), Menu["Combo"]["UseECombo"].GetValue<MenuBool>()
                , Menu["Combo"]["UseRCombo"].GetValue<MenuBool>(), Menu["Combo"]["UseQECombo"].GetValue<MenuBool>(), Menu["Combo"]["UseIgniteCombo"].GetValue<MenuBool>(), false);
        }

        private static void Harass()
        {
            if (player.ManaPercent < Menu["Harass"]["HarassMana"].GetValue<MenuSlider>().Value)
            {
                return;
            }

            UseSpells(Menu["Harass"]["UseQHarass"].GetValue<MenuBool>(), Menu["Harass"]["UseWHarass"].GetValue<MenuBool>(), Menu["Harass"]["UseEHarass"].GetValue<MenuBool>()
                , false, Menu["Harass"]["UseQEHarass"].GetValue<MenuBool>(), false, true);
        }

        private static void Farm(bool laneClear)
        {
            if (player.ManaPercent < Menu["LaneClear"]["LaneClearMana"].GetValue<MenuSlider>().Value)
            {
                return;
            }

            if (!Variables.Orbwalker.CanMove)
            {
                return;
            }

            var allMinionsQ = GetMinions(player.ServerPosition, Q.Range + Q.Width + 30);
            var allMinionsW = GetMinions(player.ServerPosition, W.Range + W.Width + 30);

            var useQi = Menu["LaneClear"]["UseQFarm"].GetValue<MenuList>().Index;
            var useWi = Menu["LaneClear"]["UseWFarm"].GetValue<MenuList>().Index;
            var useQ = (laneClear && (useQi == 1 || useQi == 2)) || (!laneClear && (useQi == 0 || useQi == 2));
            var useW = (laneClear && (useWi == 1 || useWi == 2)) || (!laneClear && (useWi == 0 || useWi == 2));

            if (useQ && Q.IsReady())
                if (laneClear)
                {
                    var fll = Q.GetCircularFarmLocation(allMinionsQ, Q.Width);
                    var fl2 = Q.GetCircularFarmLocation(allMinionsQ, Q.Width);

                    if (fll.MinionsHit >= 3)
                    {
                        Q.Cast(fll.Position);
                    }
                    else if (fl2.MinionsHit >= 2 || allMinionsQ.Count == 1)
                    {
                        Q.Cast(fl2.Position);
                    }
                }
                else
                {
                    foreach (var minion in allMinionsQ.Where(minion => !AutoAttack.InAutoAttackRange(minion) && minion.Health < 0.75 * Q.GetDamage(minion)))
                    {
                        Q.CastTo(minion);
                    }
                }

            if (useW && W.IsReady() && allMinionsW.Count > 3)
            {
                if (laneClear)
                {
                    if (player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1)
                    {
                        var gObjectPos = GetGrabableObjectPos(false);

                        if (gObjectPos.To2D().IsValid() && Variables.TickCount - W.LastCastAttemptT > Game.Ping + 150)
                        {
                            W.Cast(gObjectPos);
                        }
                    }
                    else if (player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 1)
                    {
                        var fll = Q.GetCircularFarmLocation(allMinionsW, W.Width);
                        var fl2 = Q.GetCircularFarmLocation(allMinionsW, W.Width);

                        if (fll.MinionsHit >= 3 && W.IsInRange(fll.Position.To3D()))
                        {
                            W.Cast(fll.Position);
                        }
                        else if (fl2.MinionsHit >= 1 && W.IsInRange(fl2.Position.To3D()) && fll.MinionsHit <= 2)
                        {
                            W.Cast(fl2.Position);
                        }
                    }
                }
            }
        }

        private static void JungleFarm()
        {
            var useQ = Menu["Jungle"]["UseQJFarm"].GetValue<MenuBool>();
            var useW = Menu["Jungle"]["UseWJFarm"].GetValue<MenuBool>();
            var useE = Menu["Jungle"]["UseEJFarm"].GetValue<MenuBool>();

            var Mobs = ObjectManager.Get<Obj_AI_Minion>().Where(x => !x.IsDead && !x.IsZombie && x.Team == GameObjectTeam.Neutral && x.IsValidTarget(W.Range)).ToList();

            if (Mobs.Count > 0)
            {
                var mob = Mobs[0];

                if (Q.IsReady() && useQ)
                {
                    Q.CastTo(mob);
                }

                if (W.IsReady() && useW && Variables.TickCount - Q.LastCastAttemptT > 790)
                {
                    W.CastTo(mob);
                }

                if (useE && E.IsReady())
                {
                    E.CastTo(mob);
                }
            }
        }
    }
    */

    }
}
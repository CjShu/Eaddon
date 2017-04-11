namespace Syndra
{
    using EloBuddy;
    using LeagueSharp.SDK;
    using LeagueSharp.SDK.UI;
    using LeagueSharp.SDK.Utils;
    using LeagueSharp.SDK.Enumerations;
    using System;
    using System.Linq;
    using Lib.Common;
    using static Lib.Common.Manager;
    using System.Collections.Generic;
    using SharpDX;

    public class Program
    {
        public static Menu Menu;
        public static AIHeroClient player => Player.Instance;
        public static Orbwalker Orbwalker => Variables.Orbwalker;

        public static List<Spell> SpellList = new List<Spell>();

        internal static Spell Q, W, E, EQ, R;
        private static HpBarDraw HpBarDraw = new HpBarDraw();

        private static SpellSlot Ignite;
        private static int qeComboT, weComboT;

        static void Main(string[] args)
        {
            Bootstrap.Init();
            EloBuddy.SDK.Events.Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.Hero != Champion.Syndra)
                return;

            Ignite = player.GetSpellSlot("SummonerDot");

            InitSpells();
            InitMenuu();
            InitEvents();

        }

        private static void InitSpells()
        {
            Q = new Spell(SpellSlot.Q, 790).SetSkillshot(0.6f, 125f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W = new Spell(SpellSlot.W, 925).SetSkillshot(0.25f, 140f, 1600f, false, SkillshotType.SkillshotCircle);
            E = new Spell(SpellSlot.E, 700).SetSkillshot(0.25f, (float)(45 * 0.5), 2500f, false, SkillshotType.SkillshotCircle);
            EQ = new Spell(SpellSlot.Q, Q.Range + 500).SetSkillshot(float.MaxValue, 55f, 2000f, false, SkillshotType.SkillshotCircle);
            R = new Spell(SpellSlot.R, 675);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }

        private static void InitMenuu()
        {
            Menu = new Menu("ShuSyndra", "CjShu 星朵拉", true).Attach();

            var ComboMenu = Menu.Add(new Menu("Combo", "連招 設定"));
            {
                ComboMenu.Add(new MenuBool("UseQCombo", "使用 Q", true));
                ComboMenu.Add(new MenuBool("UseWCombo", "使用 W", true));
                ComboMenu.Add(new MenuBool("UseECombo", "使用 E", true));
                ComboMenu.Add(new MenuBool("UseQECombo", "使用 QE", true));
                ComboMenu.Add(new MenuBool("UseRCombo", "使用 R", true));
                ComboMenu.Add(new MenuBool("UseIgniteCombo", "連招使用點燃", true));
            }

            var HarassMenu = Menu.Add(new Menu("Harass", "騷擾 設定"));
            {
                HarassMenu.Add(new MenuBool("UseQHarass", "使用 Q", true));
                HarassMenu.Add(new MenuBool("UseWHarass", "使用 W", false));
                HarassMenu.Add(new MenuBool("UseEHarass", "使用 E", false));
                HarassMenu.Add(new MenuBool("UseQEHarass", "使用 QE", false));
                HarassMenu.Add(new MenuSlider("HarassMana", "騷擾 最低魔力 > = %", 20));
            }

            var LaneClearMenu = Menu.Add(new Menu("LaneClear", "清線 設定"));
            {
                LaneClearMenu.GetList("UseQFarm", "Q 模式 :", new[] { "控線", "清線", "兩者都開", "關閉" }, 2);
                LaneClearMenu.GetList("UseWFarm", "W 模式 :", new[] { "控線", "清線", "兩者都開", "關閉" }, 1);
                LaneClearMenu.Add(new MenuSlider("LaneClearMana", "清線 最低魔力 > = %", 20));
            }

            var JungleMenu = Menu.Add(new Menu("Jungle", "打野 設定"));
            {
                JungleMenu.Add(new MenuBool("UseQJFarm", "使用 Q", true));
                JungleMenu.Add(new MenuBool("UseWJFarm", "使用 W", true));
                JungleMenu.Add(new MenuBool("UseEJFarm", "使用 E", true));
            }

            var PredMenu = Menu.Add(new Menu("Pred", "預測 設定"));
            {
                PredMenu.GetSeparator("預測 選擇(切換後重新按 F5)");
                PredMenu.Add(new MenuList<string>("SelectPred", "選擇預測 : ", new[] { "邏輯 預測", "SDK 預測", }));
            }

            var MiscMenu = Menu.Add(new Menu("Misc", "其他 設定"));
            {
                MiscMenu.Add(new MenuBool("InterruptSpells", "使用技能打斷目標", true));
                MiscMenu.GetKeyBind("CastQE", "使用QE 鼠標位置", System.Windows.Forms.Keys.T);
                MiscMenu.GetKeyBind("Key.InstantQE", "使用 QE 中斷目標", System.Windows.Forms.Keys.T);
                if (GameObjects.EnemyHeroes.Any())
                {
                    MiscMenu.GetSeparator("不使用 R 再目標英雄上");
                    GameObjects.EnemyHeroes.ForEach(i => MiscMenu.GetBool("DontUlt" + i.CharData.BaseSkinName, i.CharData.BaseSkinName, false));
                }
            }

            var DrawMenu = Menu.Add(new Menu("Draw", "顯示 設定"));
            {
                DrawMenu.Add(new MenuBool("Q", "Q 範圍", false));
                DrawMenu.Add(new MenuBool("W", "W 範圍", false));
                DrawMenu.Add(new MenuBool("E", "E 範圍", false));
                DrawMenu.Add(new MenuBool("R", "R 範圍", false));
                DrawMenu.Add(new MenuBool("QE", "QE 範圍", false));
                DrawMenu.Add(new MenuBool("DrawDamage", "顯示 連招傷害", true));
            }

            WriteConsole(Player.Instance.ChampionName + "CjShu");
        }

        private static void InitEvents()
        {
            Game.OnUpdate += OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Events.OnInterruptableTarget += OnInterruptableTarget;
            Drawing.OnEndScene += OnEndScene;
            Drawing.OnDraw += OnDraw;
            Variables.Orbwalker.OnAction += OnAction;
        }

        private static void OnAction(object sender, OrbwalkingActionArgs args)
        {
            if (args.Type == OrbwalkingType.BeforeAttack)
            {
                if (Orbwalker.ActiveMode == OrbwalkingMode.Combo)
                {
                    args.Process = !(Q.IsReady() || W.IsReady());
                }
            }
        }

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

        private static void OnDraw(EventArgs args)
        {
            if (player.IsDead)
            {
                return;
            }

            if (Menu["Draw"]["Q"] && Q.Level > 0)
            {
                Render.Circle.DrawCircle(player.Position, Q.Range, E.IsReady() ? System.Drawing.Color.LimeGreen : System.Drawing.Color.IndianRed);
            }

            if (Menu["Draw"]["W"] && W.Level > 0)
            {
                Render.Circle.DrawCircle(player.Position, W.Range, W.IsReady() ? System.Drawing.Color.LimeGreen : System.Drawing.Color.IndianRed);
            }

            if (Menu["Draw"]["E"] && E.Level > 0)
            {
                Render.Circle.DrawCircle(player.Position, E.Range, E.IsReady() ? System.Drawing.Color.LimeGreen : System.Drawing.Color.IndianRed);
            }

            if (Menu["Draw"]["R"] && R.Level > 0)
            {
                Render.Circle.DrawCircle(player.Position, R.Range, R.IsReady() ? System.Drawing.Color.LimeGreen : System.Drawing.Color.IndianRed);
            }

            if (Menu["Draw"]["QE"])
            {
                Render.Circle.DrawCircle(player.Position, EQ.Range - 50, System.Drawing.Color.Yellow);
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

        private static void OnInterruptableTarget(object sender, Events.InterruptableTargetEventArgs args)
        {
            var target = args.Sender;

            if (!Menu["Misc"]["InterruptSpells"].GetValue<MenuBool>())
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
}
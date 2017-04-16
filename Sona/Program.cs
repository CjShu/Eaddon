namespace Sona
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
        private static SpellSlot Ignite;
        public static Spell Q, W, E, R;
        public static List<Spell> SpellList = new List<Spell>();



        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        private static void OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Sona")
                return;

            InitSpells();
            InitMenu();
            InitEvents();
        }

        private static void InitSpells()
        {
            Q = new Spell(SpellSlot.Q, 850f);
            W = new Spell(SpellSlot.W, 1000f);
            E = new Spell(SpellSlot.E, 350f);
            R = new Spell(SpellSlot.R, 1000f);

            R.SetSkillshot(0.5f, 125, 3000f, false, SkillshotType.SkillshotLine);

            Ignite = player.GetSpellSlot("SummonerDot");

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }

        private static void InitMenu()
        {
            (Menu = new Menu("CjShu 索娜", "ShuSona", true)).AddToMainMenu();

            Menu.AddSubMenu(new Menu("走砍設置", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalking"));

            Tools.Tools.Inject();

            var ComboMenu = Menu.AddSubMenu(new Menu("連招設定", "ComboMenu"));
            ComboMenu.AddItem(new MenuItem("ComboQ", "使用 Q").SetValue(true));
            ComboMenu.AddItem(new MenuItem("ComboW", "使用 W").SetValue(true));
            ComboMenu.AddItem(new MenuItem("WM", "使用 W| 當自己血量小於多少使用 >= ").SetValue(new
                Slider(40, 0, 100)));
            ComboMenu.AddItem(new MenuItem("WA", "使用 W| 當隊友血量小於多少使用 >= ").SetValue(new
                Slider(40, 0, 100)));
            ComboMenu.AddItem(new MenuItem("ComboR", "使用 R").SetValue(true));
            ComboMenu.AddItem(new MenuItem("RComboMin", "使用R敵人最低數量").SetValue(new
                Slider(3, 1, 5)));
            ComboMenu.AddItem(new MenuItem("IgniteCombo", "連招使用點燃").SetValue(true));

            var HarassMenu = Menu.AddSubMenu(new Menu("騷擾設定", "HarassMenu"));
            HarassMenu.AddItem(new MenuItem("QHarass", "使用 Q").SetValue(true));
            HarassMenu.AddItem(new MenuItem("HarassMana", "騷擾 最低魔力 > = %", true).SetValue(new
                Slider(50, 0, 100)));

            var LaneClearMenu = Menu.AddSubMenu(new Menu("清線設定", "LaneClearMenu"));
            LaneClearMenu.AddItem(new MenuItem("UseQFarm", "Q 模式 :").SetValue(new
                StringList(new[] { "控線", "清線", "兩者都開", "關閉" }, 2)));
            LaneClearMenu.AddItem(new MenuItem("LaneQMinMinions", "清線 Q 最低命中小兵數 >= ").SetValue(new
                Slider(2, 1, 3)));

            LaneClearMenu.AddItem(new MenuItem("LaneClearMana", "清線 最低魔力 > = %").SetValue(new
                Slider(50, 0, 100)));
            ManaManager.AddSpellFarm(LaneClearMenu);

            var JungleMenu = LaneClearMenu.AddSubMenu(new Menu("打野設定", "JungleMenu"));
            JungleMenu.AddItem(new MenuItem("QJFarm", "使用 Q").SetValue(true));
            JungleMenu.AddItem(new MenuItem("WJFarm", "使用 W").SetValue(true));
            JungleMenu.AddItem(new MenuItem("JungleMana", "使用打野最低魔力 >= %").SetValue(new
                Slider(50, 0, 100)));

            var KillStealMenu = Menu.AddSubMenu(new Menu("擊殺設定", "KillStealMenu"));
            KillStealMenu.AddItem(new MenuItem("KillStealQ", "使用 Q").SetValue(true));
            KillStealMenu.AddItem(new MenuItem("KillStealR", "使用 R").SetValue(true));
            KillStealMenu.AddItem(new MenuItem("3", "R 目標英雄")).SetTooltip("敵人");
            if (HeroManager.Enemies.Any())
            {
                HeroManager.Enemies.ForEach(
                    i
                     => KillStealMenu.AddItem(new MenuItem("Ult" + i.ChampionName.ToLower(),
                     i.ChampionName, true).SetValue(false)));
            }


            var QEMenu = Menu.AddSubMenu(new Menu("反突進設定", "GPEMenu"));
            QEMenu.AddItem(new MenuItem("AntiGapcloserE", "使用 E").SetValue(true));

            var AutoMenu = Menu.AddSubMenu(new Menu("自動設定", "AutoMenu"));
            AutoMenu.AddItem(new MenuItem("AutoW", "自動 W").SetValue(true));
            AutoMenu.AddItem(new MenuItem("AutoWM", "自己血量小於多少使用 W >= ").SetValue(new
                Slider(40, 0, 100)));
            AutoMenu.AddItem(new MenuItem("AutoWA", "隊友血量小於多少使用 W >= ").SetValue(new
                Slider(40, 0, 100)));
            AutoMenu.AddItem(new MenuItem("AutoMana", "自動最低魔力設置 >= ").SetValue(new
                Slider(60, 0, 100)));

            var FleeMenu = Menu.AddSubMenu(new Menu("逃跑設置", "FleeMenu"));
            FleeMenu.AddItem(new MenuItem("FleeE", "使用 E").SetValue(true));


            var DrawMenu = Menu.AddSubMenu(new Menu("顯示設定", "DrawMenu"));
            DrawMenu.AddItem(new MenuItem("DrawManaBar", "顯示魔力線條").SetValue(true));
            DrawMenu.AddItem(new MenuItem("QDraw", "Q 範圍").SetValue(new Circle(false, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            DrawMenu.AddItem(new MenuItem("WDraw", "W 範圍").SetValue(new Circle(false, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            DrawMenu.AddItem(new MenuItem("EDraw", "E 範圍").SetValue(new Circle(false, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            DrawMenu.AddItem(new MenuItem("RDraw", "R 範圍").SetValue(new Circle(false, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            DamageIndicator.AddToMenu(DrawMenu);
            ManaManager.AddDrawFarm(DrawMenu);
            CommonManaBar.Init(DrawMenu);

            Menu.AddItem(new MenuItem("Sup", "妹子專用").SetValue(true));

            Manager.WriteConsole(Player.Instance.ChampionName + "CjShu");
        }

        private static void InitEvents()
        {
            Game.OnUpdate += OnUpdate;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Drawing.OnDraw += OnDraw;
            Orbwalking.BeforeAttack += BeforeAttack;
        }

        private static void OnUpdate(EventArgs args)
        {
            if (player.IsDead)
                return;

            Auto();
            KillSteal();

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    ComboLogic();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    HarassLogic();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClearLogic(true);
                    JungleLogic();
                    break;
                case Orbwalking.OrbwalkingMode.Flee:
                    FleeLogic();
                    break;
            }

        }

        private static void Auto()
        {
            if (ManaManager.HasEnoughMana(Menu.Item("AutoMana").GetValue<Slider>().Value))
            {
                if (Menu.Item("AutoW").GetValue<bool>() && W.IsReady())
                {
                    if (player.HealthPercent <= Menu.Item("AutoWM").GetValue<Slider>().Value)
                    {
                        W.Cast(player, true);
                    }

                    var ally = HeroManager.Allies.Where(
                        x => x.IsValidTarget(W.Range) && x.HealthPercent <= Menu.Item("AutoWA").GetValue<Slider>().Value).FirstOrDefault();

                    if (ally != null)
                    {
                        W.Cast(true);
                    }
                }
            }
        }

        private static void KillSteal()
        {
            if (Menu.Item("KillStealQ").GetValue<bool>() && Q.IsReady())
            {
                var qt = HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range) && x.Health < DamageCalculate.GetQDamage(x)).FirstOrDefault();

                if (qt != null)
                {
                    Q.Cast();
                    return;
                }
            }

            if (Menu.Item("KillStealR").GetValue<bool>() && R.IsReady())
            {
                var rt = HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range) && x.Health < DamageCalculate.GetRDamage(x)
                && Menu.Item("Ult" + x.ChampionName.ToLower()).GetValue<bool>()).FirstOrDefault();

                if (rt != null)
                {
                    R.Cast();
                    return;
                }
            }
        }

        private static void FleeLogic()
        {
            Orbwalking.MoveTo(Game.CursorPos);

            if (Menu.Item("FleeE").GetValue<bool>() && E.IsReady())
            {
                E.Cast(player, true);
            }
        }

        private static void ComboLogic()
        {
            var target = TargetSelector.GetSelectedTarget() ??
                TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target != null && !target.IsDead && !target.IsZombie && target.IsHPBarRendered)
            {
                if (Menu.Item("ComboQ").GetValue<bool>() && Q.IsReady() && target.IsValidTarget(Q.Range) && Q.CanCast(target))
                {
                    Q.Cast(true);
                }

                if (Menu.Item("ComboW").GetValue<bool>() && W.IsReady())
                {
                    if (player.HealthPercent <= Menu.Item("WM").GetValue<Slider>().Value)
                    {
                        W.Cast(true);
                    }

                    var ally = HeroManager.Allies.Where(
                        x => x.IsValidTarget(W.Range) && x.HealthPercent <= Menu.Item("WA").GetValue<Slider>().Value).FirstOrDefault();

                    if (ally != null)
                    {
                        W.Cast(true);
                    }
                }

                if (Menu.Item("ComboR").GetValue<bool>() && R.IsReady() && target.IsValidTarget(R.Range))
                {
                    if (target.Health <= DamageCalculate.GetRDamage(target) + player.GetAutoAttackDamage(target) * 2)
                    {
                        R.Cast(target);
                    }
                    else
                    {
                        R.CastIfWillHit(target, Menu.Item("RComboMin").GetValue<Slider>().Value, true);
                    }
                }

                if (Menu.Item("IgniteCombo").GetValue<bool>() && Ignite != SpellSlot.Unknown && Ignite.IsReady())
                {
                    if (DamageCalculate.GetIgniteDmage(target) > target.Health)
                    {
                        player.Spellbook.CastSpell(Ignite, target);
                    }
                }
            }
        }

        private static void HarassLogic()
        {
            if (ManaManager.HasEnoughMana(Menu.Item("HarassMana").GetValue<Slider>().Value))
            {
                var target = TargetSelector.GetSelectedTarget() ??
                    TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

                if (target != null && !target.IsDead && !target.IsZombie && target.IsHPBarRendered)
                {
                    if (Menu.Item("QHarass").GetValue<bool>() && Q.IsReady() && target.IsValidTarget(Q.Range)
                        && Q.CanCast(target))
                    {
                        Q.Cast(target, true);
                    }
                }
            }
        }

        private static void LaneClearLogic(bool laneClear)
        {
            if (ManaManager.HasEnoughMana(Menu.Item("LaneClearMana").GetValue<Slider>().Value) && ManaManager.SpellFarm)
            {
                var useQi = Menu.Item("UseQFarm").GetValue<StringList>().SelectedIndex;
                var useQ = (laneClear && (useQi == 1 || useQi == 2)) || (!laneClear && (useQi == 0 || useQi == 2));
                var allMinionsQ = MinionManager.GetMinions(player.Position, Q.Range, MinionTypes.Ranged);

                if (useQ && Q.IsReady())
                {
                    if (laneClear)
                    {
                        if (allMinionsQ.Count() > 0)
                        {
                            if (allMinionsQ.Count >= Menu.Item("LaneQMinMinions").GetValue<Slider>().Value)
                            {
                                Q.Cast();
                            }
                        }
                    }
                    else
                    {
                        foreach (var minion in allMinionsQ.Where(minion
                                => !Orbwalking.InAutoAttackRange(minion) && minion.Health < 0.75 * Q.GetDamage(minion)))
                        {
                            Q.Cast(minion, true);
                        }
                    }
                }
            }
        }

        private static void JungleLogic()
        {
            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count() > 0)
            {
                if (player.ManaPercent >= Menu.Item("JungleMana").GetValue<Slider>().Value)
                {
                    if (Menu.Item("QJFarm").GetValue<bool>() && Q.IsReady())
                    {
                        Q.Cast();
                    }

                    if (Menu.Item("WJFarm").GetValue<bool>() && W.IsReady() && mobs.FirstOrDefault().IsAttackingPlayer)
                    {
                        W.Cast();
                    }
                }
            }
        }

        private static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (Menu.Item("AntiGapcloserE").GetValue<bool>() && E.IsReady())
                {
                    if (gapcloser.Sender.IsMelee)
                    {
                        //E.Cast(true);
                    }
                }
                E.Cast(true);
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (!player.IsDead && !MenuGUI.IsChatOpen && !Chat.IsOpen)
            {
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

        private static void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && Menu.Item("Sup").GetValue<bool>() && args.Target is Obj_AI_Minion)
            {
                args.Process = false;
            }

        }
    }
}

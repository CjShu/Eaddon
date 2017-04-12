namespace Caitlyn.Tools
{
    using EloBuddy;
    using LeagueSharp.Common;
    using System;
    using System.Linq;

    internal class Offensive
    {
        private static AIHeroClient Me => ObjectManager.Player;
        private static Menu Menu => Tools.Menu;

        public static void Inject()
        {
            var OffensiveMenu = Menu.AddSubMenu(new Menu("攻擊道具", "Offensive"));
            {
                OffensiveMenu.AddItem(new MenuItem("Youmuus", "使用妖夢").SetValue(true));
                OffensiveMenu.AddItem(new MenuItem("Cutlass", "使用灣刀").SetValue(true));
                OffensiveMenu.AddItem(new MenuItem("Botrk", "使用殞落王者").SetValue(true));
                OffensiveMenu.AddItem(new MenuItem("1", "  "));
                OffensiveMenu.AddItem(new MenuItem("ComboOffensive", "連招中啟動").SetValue(false));
            }

            Common.Manager.WriteConsole("OffensiveMenu Load!");

            Game.OnUpdate += OnUpdate;
        }

        private static void OnUpdate(EventArgs Args)
        {
            if (Me.IsDead)
            {
                return;
            }

            if (Common.Manager.InCombo && Menu.Item("ComboOffensive").GetValue<bool>())
            {
                var target = TargetSelector.GetTarget(600, TargetSelector.DamageType.Physical);

                if (target != null && target.IsHPBarRendered)
                {
                    if (Menu.Item("Youmuus").GetValue<bool>() && Items.HasItem(3142) && target.IsValidTarget(Orbwalking.GetRealAutoAttackRange() + 150))
                    {
                        Items.UseItem(3142);
                    }

                    if (Menu.Item("Cutlass").GetValue<bool>() && Items.HasItem(3144) && target.IsValidTarget(Orbwalking.GetRealAutoAttackRange()) && target.HealthPercent < 80)
                    {
                        Items.UseItem(3144, target);
                    }

                    if (Menu.Item("Botrk").GetValue<bool>() && Items.HasItem(3153) && target.IsValidTarget(Orbwalking.GetRealAutoAttackRange()) && (target.HealthPercent < 80 || Me.HealthPercent < 80))
                    {
                        Items.UseItem(3153, target);
                    }
                }
            }
        }
    }
}
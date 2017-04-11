namespace Caitlyn.Tools
{
    using EloBuddy;
    using LeagueSharp.SDK;
    using LeagueSharp.SDK.UI;
    using LeagueSharp.SDK.Utils;
    using System;
    using System.Linq;

    internal class Offensive
    {
        private static AIHeroClient Me => Player.Instance;
        private static Menu Menu => Tools.Menu;

        public static void Inject()
        {
            var OffensiveMenu = Menu.Add(new Menu("Offensive", "進攻物品"));
            {
                OffensiveMenu.Add(new MenuBool("Youmuus", "使用妖夢", true));
                OffensiveMenu.Add(new MenuBool("Cutlass", "使用灣刀", true));
                OffensiveMenu.Add(new MenuBool("Botrk", "使用殞落王者", true));
                OffensiveMenu.Add(new MenuSeparator("  ", "  "));
                OffensiveMenu.Add(new MenuBool("Combo", "連招中啟動", true));
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

            if (Common.Manager.InCombo && Menu["Offensive"]["Combo"])
            {
                var target = Variables.TargetSelector.GetTarget(600, DamageType.Physical);

                if (target != null && target.IsHPBarRendered)
                {
                    if (Menu["Offensive"]["Youmuus"] && Items.HasItem(3142) && target.IsValidTarget(Me.GetRealAutoAttackRange() + 150))
                    {
                        Items.UseItem(3142);
                    }

                    if (Menu["Offensive"]["Cutlass"] && Items.HasItem(3144) && target.IsValidTarget(Me.GetRealAutoAttackRange()) && target.HealthPercent < 80)
                    {
                        Items.UseItem(3144, target);
                    }

                    if (Menu["Offensive"]["Botrk"] && Items.HasItem(3153) && target.IsValidTarget(Me.GetRealAutoAttackRange()) && (target.HealthPercent < 80 || Me.HealthPercent < 80))
                    {
                        Items.UseItem(3153, target);
                    }
                }
            }
        }
    }
}
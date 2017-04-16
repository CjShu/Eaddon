namespace Sona.Tools
{
    using EloBuddy;
    using LeagueSharp.Common;
    using System;
    using System.Linq;

    internal class Potions
    {
        private static AIHeroClient Me => HeroManager.Player;
        private static Menu Menu => Tools.Menu;

        public static void Inject()
        {
            var PotionsMenu = Menu.AddSubMenu(new Menu("自動喝水", "Potions"));
            {
                PotionsMenu.AddItem(new MenuItem("Enable", "啟動").SetValue(true));
                PotionsMenu.AddItem(new MenuItem("Hp", "當玩家血量低於 <= %").SetValue(new
                    Slider(50, 0, 100)));
            }

            Common.Manager.WriteConsole("PotionsMenu Load!");

            Game.OnUpdate += OnUpdate;
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Me.IsDead || Me.InFountain() || Me.Buffs.Any(x => x.Name.ToLower().Contains("recall") || x.Name.ToLower().Contains("teleport")))
            {
                return;
            }

            if (Menu.Item("Enable").GetValue<bool>() && Menu.Item("Hp").GetValue<Slider>().Value >= Me.HealthPercent)
            {
                if (Me.Buffs.Any(x => x.Name.Equals("ItemCrystalFlask", StringComparison.OrdinalIgnoreCase) ||
                    x.Name.Equals("ItemCrystalFlaskJungle", StringComparison.OrdinalIgnoreCase) ||
                    x.Name.Equals("ItemDarkCrystalFlask", StringComparison.OrdinalIgnoreCase) ||
                    x.Name.Equals("RegenerationPotion", StringComparison.OrdinalIgnoreCase) ||
                    x.Name.Equals("ItemMiniRegenPotion", StringComparison.OrdinalIgnoreCase) ||
                    x.Name.Equals("ItemMiniRegenPotion", StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                if (Items.HasItem(2003) && Items.UseItem(2003)) //Health_Potion 
                {
                    return;
                }

                if (Items.HasItem(2009) && Items.UseItem(2009)) //Total_Biscuit_of_Rejuvenation 
                {
                    return;
                }

                if (Items.HasItem(2010) && Items.UseItem(2010)) //Total_Biscuit_of_Rejuvenation2 
                {
                    return;
                }

                if (Items.HasItem(2031) && Items.UseItem(2031)) //Refillable_Potion 
                {
                    return;
                }

                if (Items.HasItem(2032) && Items.UseItem(2032)) //Hunters_Potion 
                {
                    return;
                }

                if (Items.HasItem(2033) && Items.UseItem(2033)) //Corrupting_Potion 
                {
                    return;
                }
            }
        }
    }
}
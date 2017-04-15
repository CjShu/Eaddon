namespace Syndra.Common
{
    using EloBuddy;
    using LeagueSharp.Common;
    using Color = System.Drawing.Color;

    public static class ManaManager
    {
        private static Menu FarmMenu;
        private static Menu DrawMenu;
        private static Menu HarassMenu;
        public static bool SpellFarm;
        public static bool SpellHarass;
        private static AIHeroClient Me => HeroManager.Player;

        public static bool HasEnoughMana(int manaPercent)
        {
            return Me.ManaPercent >= manaPercent;
        }

        public static void AddSpellFarm(Menu mainMenu)
        {
            FarmMenu = mainMenu;

            mainMenu.AddItem(new MenuItem("SpellFarm", "使用技能清線", true).SetValue(true));

            SpellFarm = mainMenu.Item("SpellFarm", true).GetValue<bool>();

            Game.OnWndProc += delegate (WndEventArgs args)
            {
                if (args.Msg == 0x20a)
                {
                    if (args.Msg == 0x20a)
                    {
                        FarmMenu.Item("SpellFarm", true).SetValue(!FarmMenu.Item("SpellFarm", true).GetValue<bool>());
                        SpellFarm = FarmMenu.Item("SpellFarm", true).GetValue<bool>();
                    }
                }
            };

        }

        public static void AddSpellHarass(Menu mainMenu)
        {
            HarassMenu = mainMenu;

            mainMenu.AddItem(new MenuItem("SpellHarass", "使用技能騷擾").SetValue(new KeyBind('H',
                KeyBindType.Toggle, true))).ValueChanged += (sender, args) =>
                {
                    SpellHarass = args.GetNewValue<KeyBind>().Active;
                };

            SpellHarass = mainMenu.Item("SpellHarass", true).GetValue<KeyBind>().Active;

        }

        public static void AddDrawFarm(Menu mainMenu)
        {
            DrawMenu = mainMenu;

            mainMenu.AddItem(new MenuItem("1", "技能狀態"));
            mainMenu.AddItem(new MenuItem("DrawFarm", "顯示技能清線", true).SetValue(true));
            mainMenu.AddItem(new MenuItem("DrawHarass", "顯示技能騷擾", true).SetValue(true));

            Drawing.OnDraw += delegate
            {
                if (!ObjectManager.Player.IsDead && !MenuGUI.IsChatOpen)
                {
                    if (DrawMenu.GetBool("DrawFarm"))
                    {
                        var MePos = Drawing.WorldToScreen(Player.Instance.Position);

                        Drawing.DrawText(MePos[0] - 57, MePos[1] + 48, Color.FromArgb(242, 120, 34),
                            "技能清線:" + (SpellFarm ? "On" : "Off"));
                    }

                    if (DrawMenu.GetBool("DrawHarass"))
                    {
                        var MePos = Drawing.WorldToScreen(Player.Instance.Position);

                        Drawing.DrawText(MePos[0] - 57, MePos[1] + 68, Color.FromArgb(213, 151, 255),
                            "技能騷擾:" + (SpellHarass ? "On" : "Off"));
                    }
                }
            };
        }
    }
}
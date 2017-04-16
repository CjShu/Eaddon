namespace Sona.Common
{
    using EloBuddy;
    using LeagueSharp.Common;
    using Color = System.Drawing.Color;

    public static class ManaManager
    {
        private static Menu FarmMenu;
        private static Menu DrawMenu;
        public static bool SpellFarm;
        private static AIHeroClient Me => HeroManager.Player;

        public static bool HasEnoughMana(int manaPercent)
        {
            return Me.ManaPercent >= manaPercent;
        }

        public static void AddSpellFarm(Menu mainMenu)
        {
            FarmMenu = mainMenu;

            mainMenu.AddItem(new MenuItem("SpellFarm", "使用技能清線").SetValue(true));


            mainMenu.Item("SpellFarm").Permashow(true, "使用 技能 清線");
            SpellFarm = mainMenu.Item("SpellFarm").GetValue<bool>();

            Game.OnWndProc += OnWndProc;

        }

        private static void OnWndProc(WndEventArgs args)
        {
            if (args.Msg == 0x20a)
            {
                if (args.Msg == 0x20a)
                {
                    FarmMenu.Item("SpellFarm").SetValue(!FarmMenu.Item("SpellFarm").GetValue<bool>());
                    SpellFarm = FarmMenu.Item("SpellFarm").GetValue<bool>();
                }
            }
        }

        public static void AddDrawFarm(Menu mainMenu)
        {
            DrawMenu = mainMenu;

            mainMenu.AddItem(new MenuItem("1", "技能狀態"));
            mainMenu.AddItem(new MenuItem("DrawFarm", "顯示技能清線").SetValue(true));
            mainMenu.AddItem(new MenuItem("DrawHarass", "顯示技能騷擾").SetValue(true));

            Drawing.OnDraw += OnDraw;
            
        }

        private static void OnDraw(System.EventArgs args)
        {
            if (!ObjectManager.Player.IsDead && !MenuGUI.IsChatOpen)
            {
                if (DrawMenu.Item("DrawFarm").GetValue<bool>())
                {
                    var MePos = Drawing.WorldToScreen(Player.Instance.Position);

                    Drawing.DrawText(MePos[0] - 57, MePos[1] + 48, Color.FromArgb(242, 120, 34),
                        "技能清線:" + (SpellFarm ? "On" : "Off"));
                }
            }
        }
    }
}
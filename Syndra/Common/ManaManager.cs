namespace Syndra.Common
{
    using EloBuddy;
    using TW.Common;
    using TW.Common.Extensions;
    using TW.Common.TargetSelector;
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

        public static void AddSpellHarass(Menu mainMenu)
        {
            HarassMenu = mainMenu;

            mainMenu.AddItem(new MenuItem("SpellHarass", "使用技能騷擾").SetValue(new KeyBind('H',
                KeyBindType.Toggle, true))).ValueChanged += ManaManager_ValueChanged;

            mainMenu.Item("SpellHarass").Permashow(true, "使用技能騷擾");
            SpellHarass = mainMenu.Item("SpellHarass").GetValue<KeyBind>().Active;

        }

        private static void ManaManager_ValueChanged(object sender, OnValueChangeEventArgs args)
        {
            SpellHarass = args.GetNewValue<KeyBind>().Active;
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

                if (DrawMenu.Item("DrawHarass").GetValue<bool>())
                {
                    var MePos = Drawing.WorldToScreen(Player.Instance.Position);

                    Drawing.DrawText(MePos[0] - 57, MePos[1] + 68, Color.FromArgb(213, 151, 255),
                        "技能騷擾:" + (SpellHarass ? "On" : "Off"));
                }
            }
        }
    }
}
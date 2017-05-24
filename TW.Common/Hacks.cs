namespace TW.Common
{
    using EloBuddy;

    /// <summary>
    ///     Adds hacks to the menu.
    /// </summary>
    internal class Hacks
    {
        #region Constants

        private const int WM_KEYDOWN = 0x100;

        private const int WM_KEYUP = 0x101;

        #endregion

        #region Static Fields

        private static Menu menu;

        private static MenuItem MenuAntiAfk;

        private static MenuItem MenuDisableDrawings;

        private static MenuItem MenuDisableSay;

        private static MenuItem MenuTowerRange;


        #endregion

        #region Public Methods and Operators

        public static void Shutdown()
        {
            Menu.Remove(menu);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        internal static void Initialize()
        {
            CustomEvents.Game.OnGameLoad += eventArgs =>
                {
                    menu = new Menu("\u9ed1\u5ba2", "Hacks");

                    MenuAntiAfk = menu.AddItem(new MenuItem("AfkHack", "\u95dc\u9589\u639b\u7db2\u8b66\u544a").SetValue(false));
                    MenuAntiAfk.ValueChanged += 
                    (sender, args) => EloBuddy.Hacks.AntiAFK = args.GetNewValue<bool>();

                    MenuDisableDrawings = menu.AddItem(new MenuItem("DrawingHack", "\u95dc\u9589\u6240\u6709\u986f\u793a\u7bc4\u570d").SetValue(false));
                    MenuDisableDrawings.ValueChanged +=
                        (sender, args) => EloBuddy.Hacks.DisableDrawings = args.GetNewValue<bool>();
                    MenuDisableDrawings.SetValue(EloBuddy.Hacks.DisableDrawings);

                    MenuDisableSay = menu.AddItem(new MenuItem("SayHack", "\u7981\u7528 \u767c\u9001\u804a\u5929").SetValue(false).SetTooltip("\u904a\u6232\u4e2d\u7684\u804a\u5929"));
                    MenuDisableSay.ValueChanged +=
                    (sender, args) => EloBuddy.Hacks.IngameChat = args.GetNewValue<bool>();

                    MenuTowerRange = menu.AddItem(new MenuItem("TowerHack", "\u986f\u793a\u5854\u7bc4\u570d").SetValue(false));
                    MenuTowerRange.ValueChanged +=
                    (sender, args) => EloBuddy.Hacks.TowerRanges = args.GetNewValue<bool>();

                    EloBuddy.Hacks.AntiAFK = MenuAntiAfk.GetValue<bool>();
                    EloBuddy.Hacks.DisableDrawings = MenuDisableDrawings.GetValue<bool>();
                    EloBuddy.Hacks.IngameChat = !MenuDisableSay.GetValue<bool>();
                    EloBuddy.Hacks.TowerRanges = MenuTowerRange.GetValue<bool>();

                    CommonMenu.Instance.AddSubMenu(menu);

                    Game.OnWndProc += args =>
                        {
                            if (!MenuDisableDrawings.GetValue<bool>())
                            {
                                return;
                            }

                            if ((int)args.WParam != Config.ShowMenuPressKey)
                            {
                                return;
                            }

                            if (args.Msg == WM_KEYDOWN)
                            {
                               EloBuddy.Hacks.DisableDrawings = false;
                            }

                            if (args.Msg == WM_KEYUP)
                            {
                                EloBuddy.Hacks.DisableDrawings = true;
                            }
                        };
                };
        }

        #endregion
    }
}
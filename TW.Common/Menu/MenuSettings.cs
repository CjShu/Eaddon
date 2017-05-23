namespace TW.Common
{
    using System.IO;

    using SharpDX;

    using Color = System.Drawing.Color;
    using EloBuddy;

    /// <summary>
    ///     The menu settings.
    /// </summary>
    public static class MenuSettings
    {
        #region Static Fields

        public static readonly Color ActiveBackgroundColor = Color.FromArgb(206, 16, 26, 29);

        /// <summary>
        ///     The menu starting position.
        /// </summary>
        public static Vector2 BasePosition = new Vector2(10, 10);

        /// <summary>
        ///     Indicates whether to draw the menu.
        /// </summary>
        private static bool drawMenu;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a static instance of the <see cref="MenuSettings" /> class.
        /// </summary>
        static MenuSettings()
        {
            drawMenu = MenuGlobals.DrawMenu;
            Game.OnWndProc += args => OnWndProc(new WndEventComposition(args));
            //Messages.OnMessage += OnWndMessage;
        }

        #endregion

        #region Public Properties

        public static Color BackgroundColor
        {
            get
            {
                return Color.FromArgb(Menu.Root.Item("BackgroundAlpha").GetValue<Slider>().Value, Color.Black);
            }
        }

        /// <summary>
        ///     Gets the menu configuration path.
        /// </summary>
        public static string MenuConfigPath
        {
            get
            {
                return Path.Combine(Config.AppDataDirectory, "TWCommonSaveData");
            }
        }

        /// <summary>
        ///     Gets or sets the size of the menu font.
        /// </summary>
        public static int MenuFontSize { get; set; }

        /// <summary>
        ///     Gets the menu item height.
        /// </summary>
        public static int MenuItemHeight
        {
            get
            {
                return 32;
            }
        }

        /// <summary>
        ///     Gets the menu item width.
        /// </summary>
        public static int MenuItemWidth
        {
            get
            {
                return 160;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the value indicating whether to draw the menu.
        /// </summary>
        public static bool DrawMenu
        {
            get
            {
                return drawMenu;
            }

            set
            {
                MenuGlobals.DrawMenu = drawMenu = value;
            }
        }

        #endregion

        #region Methods

        private static void OnWndProc(WndEventComposition args)
        {
            if ((args.Msg == WindowsMessages.WM_KEYUP || args.Msg == WindowsMessages.WM_KEYDOWN)
                && args.WParam == Config.ShowMenuPressKey)
            {
                DrawMenu = args.Msg == WindowsMessages.WM_KEYDOWN;
            }

            if (args.Msg == WindowsMessages.WM_KEYUP && args.WParam == Config.ShowMenuToggleKey)
            {
                DrawMenu = !DrawMenu;
            }
        }

        //internal static uint ShowMenuPressKey { get; set; }

        /*
        internal static void OnWndMessage(Messages.WindowMessage args)
        {
            // Do not open the menu when the chat is open
            if (!Chat.IsOpen)
            {
                // Shift key check
                switch (args.Message)
                {
                    case WindowMessages.KeyDown:
                    case WindowMessages.KeyUp:
                        // Shift key
                        if (args.Handle.WParam == 16)
                        {
                            if (args.Message == WindowMessages.KeyDown && ShowMenuPressKey == 16)
                            {
                                break;
                            }
                            DrawMenu = args.Message == WindowMessages.KeyDown;
                        }
                        break;
                }

                // Call key events for each control
                switch (args.Message)
                {
                    case WindowMessages.KeyDown:
                        if (ShowMenuPressKey != args.Handle.WParam)
                        {
                            ShowMenuPressKey = args.Handle.WParam;
                            //Instance.OnKeyDown((Messages.KeyDown)args);
                        }
                        break;
                    case WindowMessages.KeyUp:
                        ShowMenuPressKey = 0;
                        //Instance.OnKeyUp((Messages.KeyUp)args);
                        break;
                }
            }
        }
        */

        #endregion
    }
}
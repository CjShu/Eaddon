namespace TW.Common
{

    /// <summary>
    ///     The LeagueSharp.Common official menu.
    /// </summary>
    public class CommonMenu
    {
        #region Static Fields

        /// <summary>
        ///     The menu instance.
        /// </summary>
        public static Menu Instance = new Menu("TW.Common", "TW.Common", true);

        #endregion

        #region Constructors and Destructors
        
        /// <summary>
        ///     Initializes a static instance of the <see cref="CommonMenu" /> class.
        /// </summary>
        static CommonMenu()
        {
            Initialize();
        }

        public static void Initialize()
        {
            TargetSelector.Initialize();
            Prediction.Initialize();
            Hacks.Initialize();
            //FakeClicks.Initialize();
            //Language.Initialize();

            Instance.AddToMainMenu();
        }

        public static void Shutdown()
        {
            TargetSelector.Shutdown();
            Prediction.Shutdown();
            Hacks.Shutdown();
            //FakeClicks.Shutdown();

            Menu.Remove(Instance);
        }

        #endregion
    }
}
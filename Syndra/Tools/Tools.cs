namespace Syndra.Tools
{
    using Common;
    using TW.Common;
    using TW.Common.Extensions;
    using TW.Common.TargetSelector;

    public static class Tools
    {
        public static Menu Menu;

        public static void Inject()
        {
            Menu = Program.Menu.AddSubMenu(new Menu("通用", "Tools"));

            Potions.Inject();

            var autoLevelMenu = Menu.AddSubMenu(new Menu("自動升級", "Auto Level"));
            {
                AutoLevel.AddToMenu(autoLevelMenu);
            }

            Manager.WriteConsole("Tools Inject!");
        }
    }
}
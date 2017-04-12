namespace Caitlyn.Tools
{
    using Common;
    using LeagueSharp.Common;
    public static class Tools
    {
        public static Menu Menu;

        public static void Inject()
        {
            Menu = Program.Menu.AddSubMenu(new Menu("通用", "Tools"));

            Manager.WriteConsole("Tools Inject!");

            Potions.Inject();
            Offensive.Inject();

            var autoLevelMenu = Menu.AddSubMenu(new Menu("自動升級", "Auto Level"));
            {
                AutoLevel.AddToMenu(autoLevelMenu);
            }
        }
    }
}
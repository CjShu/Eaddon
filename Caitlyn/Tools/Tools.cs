namespace Caitlyn.Tools
{
    using Common;
    using LeagueSharp.SDK;
    using LeagueSharp.SDK.UI;

    public static class Tools
    {
        public static Menu Menu;

        public static void Inject()
        {
            Menu = Program.Menu.Add(new Menu("Tools", "工具通用"));

            Manager.WriteConsole("Tools Inject!");

            Potions.Inject();
            Offensive.Inject();
            AutoLevel.Inject();

            Variables.Orbwalker.Enabled = true;
        }
    }
}
namespace Syndra.Common
{
    using LeagueSharp.Common;
    using System.Drawing;

    internal static class MenuManager
    {
        public static bool GetBool(this Menu Menu, string MenuItemName, bool unique = true)
        {
            return Menu.Item(MenuItemName, unique).GetValue<bool>();
        }

        public static bool GetKey(this Menu Menu, string MenuItemName, bool unique = true)
        {
            return Menu.Item(MenuItemName, unique).GetValue<KeyBind>().Active;
        }

        public static int GetSlider(this Menu Menu, string MenuItemName, bool unique = true)
        {
            return Menu.Item(MenuItemName, unique).GetValue<Slider>().Value;
        }

        public static int GetList(this Menu Menu, string MenuItemName, bool unique = true)
        {
            return Menu.Item(MenuItemName, unique).GetValue<StringList>().SelectedIndex;
        }
    }
}
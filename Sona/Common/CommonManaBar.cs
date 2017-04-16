namespace Sona.Common
{
    using EloBuddy;
    using SharpDX;
    using SharpDX.Direct3D9;
    using System;
    using System.Linq;
    using LeagueSharp.Common;

    internal static class CommonManaBar
    {
        public static Line DxLine;

        public static Device DxDevice = Drawing.Direct3DDevice;

        public static float Width = 104;
        public static Menu MenuLocal;

        internal static void Init(Menu mainMenu)
        {
            MenuLocal = mainMenu;

            DxLine = new Line(DxDevice) { Width = 4 };

            Drawing.OnPreReset += DrawingOnOnPreReset;
            Drawing.OnPostReset += DrawingOnOnPostReset;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomainOnDomainUnload;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnDomainUnload;

            Drawing.OnEndScene += eventArgs =>
            {

                MenuLocal.AddItem(new MenuItem("DrawManaBar", "顯示魔力線條").SetValue(true));

                var color = new ColorBGRA(255, 255, 255, 255);

                var qMana = new[] { 0, 40, 50, 60, 70, 80 };
                var wMana = new[] { 0, 60, 70, 80, 90, 100 }; // W Mana Cost doesnt works :/
                var eMana = new[] { 0, 50, 50, 50, 50, 50 };
                var rMana = new[] { 0, 100, 100, 100 };


                if (!MenuLocal.Item("DrawManaBar").GetValue<bool>())
                {
                    var TotaCosMana = qMana[Program.Q.Level] + wMana[Program.W.Level] + eMana[Program.E.Level] + rMana[Program.R.Level];

                    DrawManaPercent(TotaCosMana,
                        TotaCosMana > Program.player.Mana
                            ? new ColorBGRA(255, 0, 0, 255)
                            : new ColorBGRA(255, 255, 255, 255));
                }
            };
        }

        private static Vector2 Offset => new Vector2(34, 9);

        public static Vector2 StartPosition
            =>
                new Vector2(ObjectManager.Player.HPBarPosition.X + Offset.X,
                    ObjectManager.Player.HPBarPosition.Y + Offset.Y + 8);

        private static void CurrentDomainOnDomainUnload(object sender, EventArgs eventArgs)
        {
            DxLine.Dispose();
        }

        private static void DrawingOnOnPostReset(EventArgs args)
        {
            DxLine.OnResetDevice();
        }

        private static void DrawingOnOnPreReset(EventArgs args)
        {
            DxLine.OnLostDevice();
        }

        private static float GetManaProc(float manaPer)
        {
            return (manaPer / ObjectManager.Player.MaxMana);
        }

        private static Vector2 GetHpPosAfterDmg(float mana)
        {
            float w = Width / ObjectManager.Player.MaxMana * mana;
            return new Vector2(StartPosition.X + w, StartPosition.Y);
        }

        public static void DrawManaPercent(float dmg, ColorBGRA color)
        {
            Vector2 pos = GetHpPosAfterDmg(dmg);

            FillManaBar(pos, color);
        }

        private static void FillManaBar(Vector2 pos, ColorBGRA color)
        {
            DxLine.Begin();
            DxLine.Draw(
                new[] { new Vector2((int)pos.X, (int)pos.Y + 4f), new Vector2((int)pos.X + 2, (int)pos.Y + 4f) },
                color);
            DxLine.End();
        }
    }
}
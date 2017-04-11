namespace Caitlyn.Common
{
    using System;
    using System.Linq;
    using System.Globalization;

    using SharpDX;
    using SharpDX.Direct3D9;

    using EloBuddy;
    using LeagueSharp.SDK;
    using LeagueSharp.SDK.UI;

    using Color = System.Drawing.Color;
    using Line = EloBuddy.SDK.Rendering.Line;

    public static class DamageIndicator
    {
        private static bool Fill = true;
        private static bool Enabled = true;
        private static Font _font;
        private static Line _line;
        public delegate float DamageToUnitDelegate(AIHeroClient hero);
        private static DamageToUnitDelegate _damageToUnit;
        public static Color Color = Color.FromArgb(225, 176, 224, 230);
        private static readonly Vector2 BarOffset = new Vector2(1.25f, 14.25f);
        private static readonly Vector2 PercentOffset = new Vector2(-31, 3);
        private static readonly Vector2 PercentOffset1 = new Vector2(-30, 3);
        private static readonly Vector2 PercentOffset2 = new Vector2(-29, 3);

        public static void AddToMenu(Menu mainMenu, DamageToUnitDelegate Damage = null)
        {
            _font = new Font(
                Drawing.Direct3DDevice,
                new FontDescription
                {
                    FaceName = "Calibri",
                    Height = 11,
                    Weight = FontWeight.Bold,
                    Quality = FontQuality.ClearType,
                    OutputPrecision = FontPrecision.TrueType
                });

            _line = new Line
            {
                Antialias = true,
                Width = 9,
            };

            mainMenu.Add(new MenuSeparator("傷害數據", "傷害數據"));
            mainMenu.Add(new MenuBool("DrawComboDamage", "顯示連招傷害"));
            mainMenu.Add(new MenuBool("DrawFillDamage", "顯示線傷害"));

            DamageToUnit = Damage ?? DamageCalculate.GetComboDamage;

            Enabled = mainMenu["DrawComboDamage"].GetValue<MenuBool>().Value;
            Fill = mainMenu["DrawFillDamage"].GetValue<MenuBool>().Value;
        }

        public static DamageToUnitDelegate DamageToUnit
        {
            set
            {
                if (_damageToUnit == null)
                {
                    Drawing.OnEndScene += OnEndScene;
                }

                _damageToUnit = value;
            }
        }

        private static void OnEndScene(EventArgs args)
        {
            if (!Enabled || _damageToUnit == null)
            {
                return;
            }

            foreach (var unit in GameObjects.EnemyHeroes.Where(u => u.IsValidTarget() && u.IsHPBarRendered))
            {
                var damage = _damageToUnit(unit);

                if (damage <= 0)
                {
                    continue;
                }

                if (unit.Health > 0)
                {
                    var text = ((int)(unit.Health - damage)).ToString(CultureInfo.InvariantCulture);

                    if (text.Length == 4)
                    {
                        Drawing.DrawText(unit.HPBarPosition + PercentOffset, Color.Cyan, text, 10);
                    }
                    else if (text.Length == 3)
                    {
                        Drawing.DrawText(unit.HPBarPosition + PercentOffset1, Color.Cyan, text, 10);
                    }
                    else if (text.Length == 2)
                    {
                        Drawing.DrawText(unit.HPBarPosition + PercentOffset2, Color.Cyan, text, 10);
                    }
                    else
                    {
                        Drawing.DrawText(unit.HPBarPosition + PercentOffset, Color.Cyan, text, 10);
                    }
                }

                if (Fill)
                {
                    var damagePercentage = (unit.TotalHeal - damage > 0 ? unit.TotalHeal - damage : 0) / (unit.MaxHealth + unit.AllShield + unit.AttackShield + unit.MagicShield);
                    var currentHealthPercentage = unit.TotalHeal / (unit.MaxHealth + unit.AllShield + unit.AttackShield + unit.MagicShield);
                    var startPoint = new Vector2(unit.HPBarPosition.X + BarOffset.X + damagePercentage * 104, unit.HPBarPosition.Y + BarOffset.Y - 5);
                    var endPoint = new Vector2(unit.HPBarPosition.X + BarOffset.X + currentHealthPercentage * 104 + 1, unit.HPBarPosition.Y + BarOffset.Y - 5);

                    _line.Draw(Color, startPoint, endPoint);
                    Drawing.DrawLine(startPoint, endPoint, 9, Color);
                }
            }
        }
    }
}

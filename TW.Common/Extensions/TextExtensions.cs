namespace TW.Common.Extensions
{
    using System.Text;
    using Color = System.Drawing.Color;

    public static class TextExtensions
    {
        public enum FontStlye
        {
            Null,
            Bold,//黑体
            Cite,//斜体
        }

        /// <summary>
        /// 將文字轉換為對話框用
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public static string ToUTF8(this string form)
        {
            var bytes = Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(form));
            return Encoding.Default.GetString(bytes);
        }

        /// <summary>
        /// 將文字轉換為菜單用或者將遊戲中獲取的文字轉換為可識別
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public static string ToGBK(this string form)
        {
            var bytes = Encoding.Convert(Encoding.UTF8, Encoding.Default, Encoding.Default.GetBytes(form));
            return Encoding.Default.GetString(bytes);
        }

        public static string ToHtml(this string form, Color color, FontStlye fontStlye = FontStlye.Null)
        {
            string colorhx = "#" + color.ToArgb().ToString("X6");
            return form.ToHtml(colorhx, fontStlye);
        }

        public static string ToHtml(this string form, string FontName, int FontSize, Color FontColor)
        {
            string colorhx = "#" + FontColor.ToArgb().ToString("X6");
            form = form.ToUTF8();
            form = $"<font face=\"{FontName}\" size=\"{FontSize}\"  color=\"{colorhx}\">{form}</font>";
            return form;
        }

        public static string ToHtml(this string form, int size)
        {
            form = form.ToUTF8();
            form = $"<font size=\"{size}\">{form}</font>";
            return form;
        }

        public static string ToHtml(this string form, string color, FontStlye fontStlye = FontStlye.Null)
        {
            form = form.ToUTF8();
            form = $"<font color=\"{color}\">{form}</font>";

            switch (fontStlye)
            {
                case FontStlye.Bold:
                    form = string.Format("<b>{0}</b>", form);
                    break;
                case FontStlye.Cite:
                    form = string.Format("<i>{0}</i>", form);
                    break;
            }
            return form;
        }
    }
}
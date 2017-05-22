namespace TW.Common
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Resources;
    using System.Security.Permissions;
    using System.Threading;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using EloBuddy;
    using EloBuddy.Sandbox;
    using TW.Common.Properties;
    using Newtonsoft.Json;


    public static class MultiLanguage
    {
        /// <summary>
        ///     The translations
        /// </summary>
        private static Dictionary<string, string> translations = new Dictionary<string, string>();

        public static bool LoadLanguage(string languageName)
        {
            try
            {
                var languageStrings =
                    new ResourceManager("TW.Common.Properties.Translations", typeof(Resources).Assembly).GetString
                        (languageName + "Json");

                if (string.IsNullOrEmpty(languageStrings))
                {
                    return false;
                }

                translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(languageStrings);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public static void LoadTranslation()
        {

        }
    }
}

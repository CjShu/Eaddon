namespace TW.Common.Extensions
{
    using System.Collections.Generic;
    using EloBuddy;

    public class HeroNameEntity
    {
        public string ChampionName { get; set; }
        public string TWName { get; set; }

        public HeroNameEntity(string ChampionName, string TWName)
        {
            this.ChampionName = ChampionName;
            this.TWName = TWName;
        }
    }

    public static class HeroName
    {
        public static List<HeroNameEntity> HeroNameList => new List<HeroNameEntity>
        {
            new HeroNameEntity("Aatrox", "厄薩斯"),
            new HeroNameEntity("Ahri", "阿璃"),
            new HeroNameEntity("Akali", "阿卡莉"),
            new HeroNameEntity("Alistar", "亞歷斯塔"),
            new HeroNameEntity("Amumu", "阿姆姆"),
            new HeroNameEntity("Anivia", "艾妮維亞"),
            new HeroNameEntity("Annie", "安妮"),
            new HeroNameEntity("Ashe", "艾希"),
            new HeroNameEntity("AurelionSol", "翱銳龍獸"),
            new HeroNameEntity("Azir", "阿祈爾"),
            new HeroNameEntity("Bard", "巴德"),
            new HeroNameEntity("Blitzcrank", "布里茨"),
            new HeroNameEntity("Brand", "布蘭德"),
            new HeroNameEntity("Braum", "布郎姆"),
            new HeroNameEntity("Caitlyn", "凱特琳"),
            new HeroNameEntity("Camille", "卡蜜兒"),
            new HeroNameEntity("Cassiopeia", "卡莎碧雅"),
            new HeroNameEntity("Chogath", "科加斯"),
            new HeroNameEntity("Corki", "庫奇"),
            new HeroNameEntity("Darius", "達瑞斯"),
            new HeroNameEntity("Diana", "黛安娜"),
            new HeroNameEntity("Draven", "達瑞文"),
            new HeroNameEntity("DrMundo", "蒙多醫生"),
            new HeroNameEntity("Ekko", "艾克"),
            new HeroNameEntity("Elise", "伊莉絲"),
            new HeroNameEntity("Evelynn", "伊芙琳"),
            new HeroNameEntity("Ezreal", "伊澤瑞爾"),
            new HeroNameEntity("Fiddlesticks", "費德提克"),
            new HeroNameEntity("Fiora", "菲歐拉"),
            new HeroNameEntity("Fizz", "飛斯"),
            new HeroNameEntity("Galio", "加里歐"),
            new HeroNameEntity("Gangplank", "剛普朗克"),
            new HeroNameEntity("Garen", "蓋倫"),
            new HeroNameEntity("Gnar", "吶兒"),
            new HeroNameEntity("Gragas", "古拉格斯"),
            new HeroNameEntity("Graves", "葛雷夫"),
            new HeroNameEntity("Hecarim", "赫克林"),
            new HeroNameEntity("Heimerdinger", "漢默丁格"),
            new HeroNameEntity("Illaoi", "伊羅旖"),
            new HeroNameEntity("Irelia", "伊瑞莉雅"),
            new HeroNameEntity("Ivern", "埃爾文"),
            new HeroNameEntity("Janna", "珍娜"),
            new HeroNameEntity("JarvanIV", "嘉文四世"),
            new HeroNameEntity("Jax", "賈克斯"),
            new HeroNameEntity("Jayce", "杰西"),
            new HeroNameEntity("Jhin", "燼"),
            new HeroNameEntity("Jinx", "吉茵珂絲"),
            new HeroNameEntity("Kalista", "克黎思妲"),
            new HeroNameEntity("Karma", "卡瑪"),
            new HeroNameEntity("Karthus", "卡爾瑟斯"),
            new HeroNameEntity("Kassadin", "卡薩丁"),
            new HeroNameEntity("Katarina", "卡特蓮娜"),
            new HeroNameEntity("Kayle", "凱爾"),
            new HeroNameEntity("Kennen", "凱能"),
            new HeroNameEntity("Khazix", "卡力斯"),
            new HeroNameEntity("Kindred", "鏡爪"),
            new HeroNameEntity("Kled", "克雷德"),
            new HeroNameEntity("KogMaw", "寇格魔"),
            new HeroNameEntity("Leblanc", "勒布朗"),
            new HeroNameEntity("LeeSin", "李星"),
            new HeroNameEntity("Leona", "雷歐娜"),
            new HeroNameEntity("Lissandra", "麗珊卓"),
            new HeroNameEntity("Lucian", "路西恩"),
            new HeroNameEntity("Lulu", "露璐"),
            new HeroNameEntity("Lux", "拉克絲"),
            new HeroNameEntity("Malphite", "墨菲特"),
            new HeroNameEntity("Malzahar", "馬爾札哈"),
            new HeroNameEntity("Maokai", "茂凱"),
            new HeroNameEntity("MasterYi", "易大師"),
            new HeroNameEntity("MissFortune", "好運姐"),
            new HeroNameEntity("MonkeyKing", "悟空"),
            new HeroNameEntity("Mordekaiser", "魔鬥凱薩"),
            new HeroNameEntity("Morgana", "魔甘娜"),
            new HeroNameEntity("Nami", "娜米"),
            new HeroNameEntity("Nasus", "納瑟斯"),
            new HeroNameEntity("Nautilus", "納帝魯斯"),
            new HeroNameEntity("Nidalee", "奈德麗"),
            new HeroNameEntity("Nocturne", "夜曲"),
            new HeroNameEntity("Nunu", "努努"),
            new HeroNameEntity("Olaf", "歐拉夫"),
            new HeroNameEntity("Orianna", "奧莉安娜"),
            new HeroNameEntity("Pantheon", "潘森"),
            new HeroNameEntity("Poppy", "波比"),
            new HeroNameEntity("Quinn", "葵恩"),
            new HeroNameEntity("Rakan", "銳空"),
            new HeroNameEntity("Rammus", "拉姆斯"),
            new HeroNameEntity("RekSai", "雷珂煞"),
            new HeroNameEntity("Renekton", "雷尼克頓"),
            new HeroNameEntity("Rengar", "雷葛爾"),
            new HeroNameEntity("Riven", "雷玟"),
            new HeroNameEntity("Rumble", "藍寶"),
            new HeroNameEntity("Ryze", "雷茲"),
            new HeroNameEntity("Sejuani", "史瓦妮"),
            new HeroNameEntity("Shaco", "薩科"),
            new HeroNameEntity("Shen", "慎"),
            new HeroNameEntity("Shyvana", "希瓦娜"),
            new HeroNameEntity("Singed", "辛吉德"),
            new HeroNameEntity("Sion", "賽恩"),
            new HeroNameEntity("Sivir", "希維爾"),
            new HeroNameEntity("Skarner", "史加納"),
            new HeroNameEntity("Sona", "索娜"),
            new HeroNameEntity("Soraka", "索拉卡"),
            new HeroNameEntity("Swain", "斯溫"),
            new HeroNameEntity("Syndra", "星朵拉"),
            new HeroNameEntity("TahmKench", "貪啃奇"),
            new HeroNameEntity("Taliyah", "塔莉雅"),
            new HeroNameEntity("Talon", "塔隆"),
            new HeroNameEntity("Taric", "塔里克"),
            new HeroNameEntity("Teemo", "提摩"),
            new HeroNameEntity("Thresh", "瑟雷西"),
            new HeroNameEntity("Tristana", "崔絲塔娜"),
            new HeroNameEntity("Trundle", "特朗德"),
            new HeroNameEntity("Tryndamere", "泰達米爾"),
            new HeroNameEntity("TwistedFate", "逆命"),
            new HeroNameEntity("Twitch", "圖奇"),
            new HeroNameEntity("Udyr", "烏迪爾"),
            new HeroNameEntity("Urgot", "烏爾加特"),
            new HeroNameEntity("Varus", "法洛士"),
            new HeroNameEntity("Vayne", "汎"),
            new HeroNameEntity("Veigar", "維迦"),
            new HeroNameEntity("Velkoz", "威寇茲"),
            new HeroNameEntity("Vi", "菲艾"),
            new HeroNameEntity("Viktor", "維克特"),
            new HeroNameEntity("Vladimir", "弗拉迪米爾"),
            new HeroNameEntity("Volibear", "弗力貝爾"),
            new HeroNameEntity("Warwick", "沃維克"),
            new HeroNameEntity("Xayah", "剎雅"),
            new HeroNameEntity("Xerath", "齊勒斯"),
            new HeroNameEntity("XinZhao", "趙信"),
            new HeroNameEntity("Yasuo", "犽宿"),
            new HeroNameEntity("Yorick", "約瑞科"),
            new HeroNameEntity("Zac", "札克"),
            new HeroNameEntity("Zed", "劫"),
            new HeroNameEntity("Ziggs", "希格斯"),
            new HeroNameEntity("Zilean", "極靈"),
            new HeroNameEntity("Zyra", "枷蘿")
        };

        public static string GetHeroName(string ChampionName)
        {
            var name = HeroNameList.Find(h => h.ChampionName == ChampionName)
                ?.TWName.Trim();

            return string.IsNullOrEmpty(name) ? ChampionName : name;
        }

        public static string ToTw(this string ChampionName, bool enable = true)
        {
            if (enable)
            {
                return GetHeroName(ChampionName);
            }
            else
            {
                return ChampionName;
            }
        }
    }
}
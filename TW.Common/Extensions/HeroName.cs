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

    public class HeorSpellNameEntity
    {
        public string ChampionSpellName { get; set; }
        public string TwSpellName { get; set; }

        public HeorSpellNameEntity(string championSpellName, string twspellsname)
        {
            this.ChampionSpellName = championSpellName;
            this.TwSpellName = twspellsname;
        }
    }

    public class EvadeSpellNameEntity
    {
        public string MySpellName { get; set; }
        public string EvadeSpellName { get; set; }

        public EvadeSpellNameEntity(string myspellname, string evadespellname)
        {
            this.MySpellName = myspellname;
            this.EvadeSpellName = evadespellname;
        }
    }

    public class SummonerSlotNameEntity
    {
        public string SummonerSlotName { get; set; }
        public string MeSummonerSlotName { get; set; }

        public SummonerSlotNameEntity(string summonerslotname, string mesummonerslotname)
        {
            this.SummonerSlotName = summonerslotname;
            this.MeSummonerSlotName = mesummonerslotname;
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

    public static class HeorSpellName
    {
        /// <summary>
        /// By : CjShu
        /// </summary>
        public static List<HeorSpellNameEntity> HeroSpellNameList => new List<HeorSpellNameEntity>
        {
            new HeorSpellNameEntity("Dark Flight", "冥獄展翼"),
            new HeorSpellNameEntity("AatroxE", "邪劍審判"),
            new HeorSpellNameEntity("Orb of Deception", "幻玉"),
            new HeorSpellNameEntity("Charm", "傾城"),
            new HeorSpellNameEntity("Orb of Deception Back", "幻玉 返回"),
            new HeorSpellNameEntity("Pulverize", "大地粉碎"),
            new HeorSpellNameEntity("Curse of the Sad Mummy", "木乃伊之咒"),
            new HeorSpellNameEntity("Bandage Toss", "繃帶牽引"),
            new HeorSpellNameEntity("Flash Frost", "寒冰閃耀"),
            new HeorSpellNameEntity("Incinerate", "烈焰衝擊"),
            new HeorSpellNameEntity("Summom: Tibbers", "召喚泰貝爾"),
            new HeorSpellNameEntity("Enchanted Crystal Arrow", "魔法水晶箭"),
            new HeorSpellNameEntity("Volley", "萬箭齊發"),
            new HeorSpellNameEntity("Starsurge", "星湧"),
            new HeorSpellNameEntity("Voice of Light", "光音"),
            new HeorSpellNameEntity("Conquering Sands", "砂影霸者"),
            new HeorSpellNameEntity("Emperor's Divide", "空砂防壁"),
            new HeorSpellNameEntity("Cosmic Binding", "宇宙制約"),
            new HeorSpellNameEntity("Tempered Fate", "凝滯命域"),
            new HeorSpellNameEntity("Rocket Grab", "火箭抓取"),
            new HeorSpellNameEntity("StaticField", "靜電力場"),
            new HeorSpellNameEntity("Sear", "火焰烙印"),
            new HeorSpellNameEntity("Pillar of Flame", "煉獄風暴"),
            new HeorSpellNameEntity("Glacial Fissure", "冰河裂隙"),
            new HeorSpellNameEntity("Winter's Bite", "凜冬衝擊"),
            new HeorSpellNameEntity("Piltover Peacemaker", "鎮暴射擊"),
            new HeorSpellNameEntity("Yordle Trap", "捕獲陷阱"),
            new HeorSpellNameEntity("90 Caliber Net", "九零式獵網"),
            new HeorSpellNameEntity("Hookshot", "鋼鐵鉤射"),
            new HeorSpellNameEntity("Hookshot 2", "鋼鐵鉤射 第二次"),
            new HeorSpellNameEntity("Petrifying Gaze", "石化凝視"),
            new HeorSpellNameEntity("Noxious Blast", "毒霧爆擊"),
            new HeorSpellNameEntity("Feral Scream", "野性尖嘯"),
            new HeorSpellNameEntity("Rupture", "破裂"),
            new HeorSpellNameEntity("Missile Barrage Big", "大 火箭轟擊"),
            new HeorSpellNameEntity("Phosphorus Bomb", "磷光炸彈"),
            new HeorSpellNameEntity("Missile Barrage", "火箭轟擊"),
            new HeorSpellNameEntity("Decimate [Beta]", "毀滅風暴 [Beta]"),
            new HeorSpellNameEntity("Axe Cone Grab", "索魂奪命"),
            new HeorSpellNameEntity("Crescent Strike", "月牙衝擊"),
            new HeorSpellNameEntity("Infected Cleaver", "病毒屠刀"),
            new HeorSpellNameEntity("Whirling Death", "迴轉死神"),
            new HeorSpellNameEntity("Stand Aside", "給我閃 !"),
            new HeorSpellNameEntity("Timewinder", "時光樞紐"),
            new HeorSpellNameEntity("Timewinder (Return)", "時光樞紐 (返回)"),
            new HeorSpellNameEntity("Parallel Convergence", "時空攔截"),
            new HeorSpellNameEntity("Chronobreak", "計時開始"),
            new HeorSpellNameEntity("Cocoon", "人型 盤纏蛛絲"),
            new HeorSpellNameEntity("Agony's Embrace", "臨終的擁抱"),
            new HeorSpellNameEntity("Mystic Shot", "秘術射擊"),
            new HeorSpellNameEntity("Trueshot Barrage", "精準彈幕"),
            new HeorSpellNameEntity("Essence Flux", "精華躍動"),
            new HeorSpellNameEntity("Riposte", "斗轉之力"),
            new HeorSpellNameEntity("Urchin Strike", "現撈海膽"),
            new HeorSpellNameEntity("Chum the Waters", "海之霸主"),
            new HeorSpellNameEntity("Righteous Gust", "光榮制裁"),
            new HeorSpellNameEntity("Resolute Smite", "征戰風暴"),
            new HeorSpellNameEntity("Idol Of Durand", "英靈之門"),
            new HeorSpellNameEntity("Boulder Toss", "巨岩拋擲"),
            new HeorSpellNameEntity("GNAR!", "狂嘯猛擊 !"),
            new HeorSpellNameEntity("Wallop", "原始衝撞"),
            new HeorSpellNameEntity("Boomerang Throw", "骨頭回力鏢"),
            new HeorSpellNameEntity("Hop", "吶兒跳跳"),
            new HeorSpellNameEntity("Crunch", "重磅輾壓"),
            new HeorSpellNameEntity("Barrel Roll", "滾動酒桶"),
            new HeorSpellNameEntity("Body Slam", "肉彈衝擊"),
            new HeorSpellNameEntity("Explosive Cask", "爆破酒桶"),
            new HeorSpellNameEntity("End of the Line", "人在江湖"),
            new HeorSpellNameEntity("End of the Line (Return)", "人在江湖 (返回)"),
            new HeorSpellNameEntity("Smoke Screen", "隻手遮天"),
            new HeorSpellNameEntity("Collateral Damage", "龍爭虎鬥 未爆炸(線)"),
            new HeorSpellNameEntity("Collateral Damage (Explosion)", "龍爭虎鬥 爆炸(半徑)"),
            new HeorSpellNameEntity("Onslaught of Shadows [Beta]", "暗影的逆襲 [Beta]"),
            new HeorSpellNameEntity("Hextech Micro-Rockets", "高科技微型火箭"),
            new HeorSpellNameEntity("CH-2 Electron Storm Grenade", "CH-2 暴風手雷"),
            new HeorSpellNameEntity("Turret Energy Blast", "丁格 小炮臺能量射線"),
            new HeorSpellNameEntity("Big Turret Energy Blast", "丁格 大炮臺能量射線"),
            new HeorSpellNameEntity("Tentacle Smash", "鞭笞觸手"),
            new HeorSpellNameEntity("Test of Spirit", "祖靈之試"),
            new HeorSpellNameEntity("Leap of Faith", "虔信一躍"),
            new HeorSpellNameEntity("Transcendent Blades", "卓越巨劍"),
            new HeorSpellNameEntity("Rootcaller", "盤根束縛"),
            new HeorSpellNameEntity("Howling Gale", "颶風呼嘯"),
            new HeorSpellNameEntity("Dragon Strike", "滅龍一擊"),
            new HeorSpellNameEntity("Dragon Strike EQ", "滅龍一擊 EQ"),
            new HeorSpellNameEntity("Demacian Standard", "帝國戰旗"),
            new HeorSpellNameEntity("Cataclysm", "浩劫降臨"),
            new HeorSpellNameEntity("Shock Blast", "電磁脈衝"),
            new HeorSpellNameEntity("Shock Blast Fast", "電磁脈衝 快速"),
            new HeorSpellNameEntity("Super Mega Death Rocket!", "超威能死亡火箭 !"),
            new HeorSpellNameEntity("Zap!", "咻咻 !"),
            new HeorSpellNameEntity("Flame Chompers!", "咬咬手榴彈 !"),
            new HeorSpellNameEntity("Deadly Flourish", "致命伏筆"),
            new HeorSpellNameEntity("Curtain Call", "華麗謝幕"),
            new HeorSpellNameEntity("Pierce", "赦罪穿刺"),
            new HeorSpellNameEntity("Inner Flame", "輪迴怒火"),
            new HeorSpellNameEntity("Soulflare (Mantra)", "輪迴怒火 (R 之後)"),
            new HeorSpellNameEntity("Lay Waste", "荒蕪"),
            new HeorSpellNameEntity("RiftWalk", "虛空行走"),
            new HeorSpellNameEntity("Force Pulse", "能量脈衝"),
            new HeorSpellNameEntity("Thundering Shuriken", "雷電手裡劍"),
            new HeorSpellNameEntity("Void Spike", "虛空尖刺"),
            new HeorSpellNameEntity("Void Spike Evolved", "虛空尖刺 進化後"),
            new HeorSpellNameEntity("Leap", "掠翅飛躍"),
            new HeorSpellNameEntity("Leap Evolved", "掠翅飛躍 進化後"),
            new HeorSpellNameEntity("Pocket Pistol", "口帶手槍 掌心雷"),
            new HeorSpellNameEntity("Beartrap on a Rope", "飛射夾夾"),
            new HeorSpellNameEntity("Jousting", "比武角逐"),
            new HeorSpellNameEntity("Caustic Spittle", "腐蝕唾液"),
            new HeorSpellNameEntity("Void Ooze", "虛空淤泥"),
            new HeorSpellNameEntity("Living Artillery", "生化巨炮"),
            new HeorSpellNameEntity("Ethereal Chains [Beta]", "幻影鎖鍊 [Beta]"),
            new HeorSpellNameEntity("Distortion [Beta]", "移行瞬影 [Beta]"),
            new HeorSpellNameEntity("Sonic Wave", "虎嘯龍吟"),
            new HeorSpellNameEntity("Solar Flare", "日輪聖芒"),
            new HeorSpellNameEntity("Zenith Blade", "太陽聖劍"),
            new HeorSpellNameEntity("Ring of Frost", "暴雪結界"),
            new HeorSpellNameEntity("Ice Shard", "幽影碎冰"),
            new HeorSpellNameEntity("Ice Shard Extended", "幽影碎冰 碎冰"),
            new HeorSpellNameEntity("Glacial Path", "急速冰刺"),
            new HeorSpellNameEntity("Ardent Blaze", "烈炎鐵血"),
            new HeorSpellNameEntity("Piercing Light", "鋒芒貫穿"),
            new HeorSpellNameEntity("The Culling", "赤色屠戮"),
            new HeorSpellNameEntity("Glitterlance", "閃耀雙重奏"),
            new HeorSpellNameEntity("Lucent Singularity", "光明異點"),
            new HeorSpellNameEntity("Final Spark", "終極閃光"),
            new HeorSpellNameEntity("Light Binding", "光明束縛"),
            new HeorSpellNameEntity("Arcane Smash", "荊棘摧殘"),
            new HeorSpellNameEntity("Sapling Toss", "扭曲樹精"),
            new HeorSpellNameEntity("Syphon Of Destruction", "毀滅虹吸"),
            new HeorSpellNameEntity("Unstoppable Force", "勢不可擋"),
            new HeorSpellNameEntity("Call of the Void", "虛空召喚"),
            new HeorSpellNameEntity("Cyclone [Beta]", "無極風暴 [Beta]"),
            new HeorSpellNameEntity("Dark Binding", "暗影禁錮"),
            new HeorSpellNameEntity("Tormented Soil", "痛苦腐蝕"),
            new HeorSpellNameEntity("Aqua Prison", "禁錮水牢"),
            new HeorSpellNameEntity("Tidal Wave", "驚滔駭浪"),
            new HeorSpellNameEntity("Dredge Line", "深淵鐵錨"),
            new HeorSpellNameEntity("Javelin Toss", "標槍投擲"),
            new HeorSpellNameEntity("Bushwhack", "叢林伏擊"),
            new HeorSpellNameEntity("Duskbringer", "黃昏渠道"),
            new HeorSpellNameEntity("Axe Throw", "逆流投擲"),
            new HeorSpellNameEntity("Commnad: Attack", "指令：貫穿"),
            new HeorSpellNameEntity("Command: Dissonance", "指令：失衡"),
            new HeorSpellNameEntity("Command: Shockwave", "終極指令：脈衝"),
            new HeorSpellNameEntity("Heartseeker", "穿心長矛"),
            new HeorSpellNameEntity("Hammer Shock", "戰槌衝擊"),
            new HeorSpellNameEntity("Keeper's Verdict (Knockup)", "守護者裁決 (打擊)"),
            new HeorSpellNameEntity("Keeper's Verdict (Line)", "守護者裁決 (線)"),
            new HeorSpellNameEntity("Blinding Assault", "飛鷹突擊"),
            new HeorSpellNameEntity("Prey Seeker", "尋找獵物"),
            new HeorSpellNameEntity("Unburrow", "猛襲"),
            new HeorSpellNameEntity("Savagery [Beta]", "兇殘打擊 [Beta]"),
            new HeorSpellNameEntity("Bola Strike [Beta]", "狩獵拋繩 [Beta]"),
            new HeorSpellNameEntity("Ki Burst", "符文衝擊"),
            new HeorSpellNameEntity("Wind Slash", "放逐之刃"),
            new HeorSpellNameEntity("Electro-Harpoon", "高壓電魚叉"),
            new HeorSpellNameEntity("Carpet Bomb", "等離子飛彈"),
            new HeorSpellNameEntity("Overload", "超負荷"),
            new HeorSpellNameEntity("Arctic Assault", "極地衝刺"),
            new HeorSpellNameEntity("Glacial Prison", "冰河籠牢"),
            new HeorSpellNameEntity("Shadow Dash", "影襲"),
            new HeorSpellNameEntity("Flame Breath", "龍之吐息"),
            new HeorSpellNameEntity("Flame Breath Dragon", "真龍轉生 (龍型)"),
            new HeorSpellNameEntity("Dragon's Descent", "真龍轉生"),
            new HeorSpellNameEntity("Roar of the Slayer", "殺戮怒吼"),
            new HeorSpellNameEntity("Unstoppable Onslaught", "猛烈狂擊"),
            new HeorSpellNameEntity("Boomerang Blade", "迴旋之刃"),
            new HeorSpellNameEntity("Fracture", "水晶塵爆"),
            new HeorSpellNameEntity("Crescendo", "狂舞終樂章"),
            new HeorSpellNameEntity("Starcall", "殞星召喚"),
            new HeorSpellNameEntity("Equinox", "寂靜星河"),
            new HeorSpellNameEntity("Nevermove", "影之爪"),
            new HeorSpellNameEntity("Scatter the Weak", "虛弱潰散"),
            new HeorSpellNameEntity("Force of Will", "意志之力"),
            new HeorSpellNameEntity("Dark Sphere", "黑暗星體"),
            new HeorSpellNameEntity("Tongue Lash", "毒舌"),
            new HeorSpellNameEntity("Shadow Assault [Beta]", "暗影突襲 [Beta]"),
            new HeorSpellNameEntity("Rake [Beta]", "迴力匕首 [Beta]"),
            new HeorSpellNameEntity("Rake Return [Beta]", "迴力匕首 (返回) [Beta]"),
            new HeorSpellNameEntity("Threaded Volley", "旋舞飛岩"),
            new HeorSpellNameEntity("Seismic Shove", "震地之擊"),
            new HeorSpellNameEntity("Dazzle", "失能眩光"),
            new HeorSpellNameEntity("Death Sentence", "死亡宣告"),
            new HeorSpellNameEntity("Flay", "懾魂掃蕩"),
            new HeorSpellNameEntity("Rocket Jump", "火箭跳躍"),
            new HeorSpellNameEntity("Spinning Slash", "旋風斬"),
            new HeorSpellNameEntity("Wild Cards", "萬能牌"),
            new HeorSpellNameEntity("Venom Cask", "虛弱之毒"),
            new HeorSpellNameEntity("Spray and Pray", "噴射 然後祈禱吧"),
            new HeorSpellNameEntity("Acid Hunter", "酸蝕獵手"),
            new HeorSpellNameEntity("Noxian Corrosive Charge", "諾克薩斯酸性彈"),
            new HeorSpellNameEntity("Hail of Arrows", "腐敗箭雨"),
            new HeorSpellNameEntity("Piercing Arrow", "荒蕪箭袋"),
            new HeorSpellNameEntity("Chain of Corruption", "墮落連鎖"),
            new HeorSpellNameEntity("Baleful Strike", "黑暗祭祀"),
            new HeorSpellNameEntity("Dark Matter", "黑暗物質"),
            new HeorSpellNameEntity("Event Horizon", "扭曲空間"),
            new HeorSpellNameEntity("Tectonic Disruption", "反物質瓦解"),
            new HeorSpellNameEntity("Void Rift", "虛空裂痕"),
            new HeorSpellNameEntity("Plasma Fission (Split)", "分裂電漿 (分裂)"),
            new HeorSpellNameEntity("Plasma Fission", "分裂電漿"),
            new HeorSpellNameEntity("Vault Breaker", "蓄能衝擊"),
            new HeorSpellNameEntity("Death Ray", "死亡射線"),
            new HeorSpellNameEntity("Death Ray Aftershock", "死亡射線 (攻擊之後)"),
            new HeorSpellNameEntity("Graviton Field", "萬有引力產生裝置"),
            new HeorSpellNameEntity("Double Daggers", "赤落連匕"),
            new HeorSpellNameEntity("Bladecaller", "漫天血刃"),
            new HeorSpellNameEntity("Featherstorm", "驟羽暴風"),
            new HeorSpellNameEntity("Hemoplague", "血之瘟疫"),
            new HeorSpellNameEntity("Eye of Destruction", "毀滅之眼"),
            new HeorSpellNameEntity("Arcanopulse", "秘術脈衝"),
            new HeorSpellNameEntity("Rite of the Arcane", "魔導祭典"),
            new HeorSpellNameEntity("Shocking Orb", "幻魔打擊"),
            new HeorSpellNameEntity("Steel Tempest (Tornado)", "鋼鐵暴雪 (龍捲風)"),
            new HeorSpellNameEntity("Steel Tempest", "鋼鐵暴雪"),
            new HeorSpellNameEntity("Dark Procession [Beta]", "黯影屍陣 [Beta]"),
            new HeorSpellNameEntity("Mourning Mist [Beta]", "悲鳴瘴霧 [Beta]"),
            new HeorSpellNameEntity("Stretching Strike", "橡膠飛拳"),
            new HeorSpellNameEntity("Elastic Slingshot [Beta]", "必殺黏星 [Beta]"),
            new HeorSpellNameEntity("Razor Shuriken", "風魔手裡劍"),
            new HeorSpellNameEntity("Hexplosive Minefield", "海克斯詭雷"),
            new HeorSpellNameEntity("Satchel Charge", "技術性引爆"),
            new HeorSpellNameEntity("Bouncing Bomb", "彈跳炸彈"),
            new HeorSpellNameEntity("Mega Inferno Bomb", "煉獄炸彈"),
            new HeorSpellNameEntity("Time Bomb", "定時炸彈"),
            new HeorSpellNameEntity("Grasping Roots", "索命棘蔓"),
            new HeorSpellNameEntity("Deadly Bloom", "致死棘刺"),
            new HeorSpellNameEntity("Stranglethorns", "致命綻放")
        };
    }

    public static class EvadeSpellName
    {
        /// <summary>
        /// By: CjShu
        /// </summary>
        public static List<EvadeSpellNameEntity> EvadeSpellNameList => new List<EvadeSpellNameEntity>
        {
            new EvadeSpellNameEntity("ArcaneShift", "奧術躍遷 (閃躲)"),
            new EvadeSpellNameEntity("AhriTumble", "飛仙 (突進)"),
            new EvadeSpellNameEntity("Overdrive", "過載運轉 (增移動速度)"),
            new EvadeSpellNameEntity("CaitlynEntrapment", "九零式獵網 (突進)"),
            new EvadeSpellNameEntity("CarpetBomb", "女武神特攻 (突進)"),
            new EvadeSpellNameEntity("Blood Rush", "好戲上場 (增移動速度)"),
            new EvadeSpellNameEntity("PhaseDive", "相位轉移 (突進)"),
            new EvadeSpellNameEntity("PhaseDive2", "相位轉移 (閃躲)"),
            new EvadeSpellNameEntity("Chronobreak", "計時開始 (閃躲)"),
            new EvadeSpellNameEntity("Rappel", "韌絲飛躍 (危險技能觸發)"),
            new EvadeSpellNameEntity("Darl Frenzy", "闇影狂熱 (增移動速度)"),
            new EvadeSpellNameEntity("FioraW", "斗轉之力 (格檔技能類)"),
            new EvadeSpellNameEntity("FioraQ", "移形之體 (突進)"),
            new EvadeSpellNameEntity("FizzPiercingStrike", "現撈海膽 (突進目標)"),
            new EvadeSpellNameEntity("FizzJump", "愛玩小飛 (突進)"),
            new EvadeSpellNameEntity("Righteous Gust", "光榮制裁 (增移動速度)"),
            new EvadeSpellNameEntity("Decisive Strike", "致命打擊 (增移動速度)"),
            new EvadeSpellNameEntity("BodySlam", "肉彈衝擊 (突進)"),
            new EvadeSpellNameEntity("GnarE", "吶兒跳跳 (突進)"),
            new EvadeSpellNameEntity("GnarBigE", "重磅輾壓 (變身突進)"),
            new EvadeSpellNameEntity("QuickDraw", "猛龍過江 (突進)"),
            new EvadeSpellNameEntity("Inspire", "靈能啟示 (增移動速度)"),
            new EvadeSpellNameEntity("RiftWalk", "虛空行走 (閃躲)"),
            new EvadeSpellNameEntity("KatarinaE", "瞬步 (閃躲)"),
            new EvadeSpellNameEntity("Divine Blessing", "神聖祝福 (增移動速度)"),
            new EvadeSpellNameEntity("Intervention", "神聖庇護 (隊友與自己高危險技能觸發)"),
            new EvadeSpellNameEntity("Lightning Rush", "閃電衝擊 (增移動速度)"),
            new EvadeSpellNameEntity("KindredQ", "舞動箭矢 (突進)"),
            new EvadeSpellNameEntity("Distortion", "移行瞬影 (突進)"),
            new EvadeSpellNameEntity("DistortionR", "模仿 (突進)"),
            new EvadeSpellNameEntity("LeeSinW", "鐵璧金身 (瞬移目標 | 小兵)"),
            new EvadeSpellNameEntity("RelentlessPursuit", "冷酷進擊 (突進)"),
            new EvadeSpellNameEntity("Whimsy", "幻想曲 (突進)"),
            new EvadeSpellNameEntity("AlphaStrike", "先聲奪人 (閃躲)"),
            new EvadeSpellNameEntity("BlackShield", "黑暗之盾 (危險技能觸發)"),
            new EvadeSpellNameEntity("ShroudofDarkness", "夜幕庇護 (危險技能觸發)"),
            new EvadeSpellNameEntity("BloodBoil", "血之沸騰 (增移動速度)"),
            new EvadeSpellNameEntity("Pounce", "猛撲 (突進)"),
            new EvadeSpellNameEntity("Steadfast Presence", "堅定不移 (增移動速度)"),
            new EvadeSpellNameEntity("BrokenWings", "斷罪之翼 (突進)"),
            new EvadeSpellNameEntity("Valor", "勇者無敵 (突進)"),
            new EvadeSpellNameEntity("Scrap Shield", "鐵屑電磁盾 (增移動速度)"),
            new EvadeSpellNameEntity("SivirE", "法術護盾 (危險技能觸發)"),
            new EvadeSpellNameEntity("Exoskeleton", "結晶骨骼 (增移動速度)"),
            new EvadeSpellNameEntity("Burnout", "聖龍怒火 (增移動速度)"),
            new EvadeSpellNameEntity("Deceive", "欺詐魔術 (閃躲)"),
            new EvadeSpellNameEntity("Song of Celerity", "迅捷奏鳴曲 (增移動速度)"),
            new EvadeSpellNameEntity("Shadow Assualt", "暗影突襲 (增移動速度)"),
            new EvadeSpellNameEntity("Move Quick", "衝刺 (增移動速度)"),
            new EvadeSpellNameEntity("RocketJump", "火箭跳躍 (突進)"),
            new EvadeSpellNameEntity("SpinningSlash", "旋風斬 (突進)"),
            new EvadeSpellNameEntity("Bear Stance", "諾克薩斯酸性彈 (增移動速度)"),
            new EvadeSpellNameEntity("Tumble", "翻滾瞄準 (突進)"),
            new EvadeSpellNameEntity("SweepingBlade", "刃敵千軍 (突進 目標 | 小兵)"),
            new EvadeSpellNameEntity("WindWall", "風牆鐵壁 (格檔技能)"),
            new EvadeSpellNameEntity("Timewarp", "時間扭曲 (增移動速度)")
        };

        public static string GetEvadeName(string spellName)
        {
            var name = EvadeSpellNameList.Find(s => s.MySpellName == spellName)?.EvadeSpellName.Trim();

            return string.IsNullOrEmpty(name) ? spellName : name;
        }

        public static string ToEzEvadeName(this string spellName, bool enable = true)
        {
            if (enable)
            {
                return GetEvadeName(spellName);
            }
            else
            {
                return spellName;
            }
        }
    }

    public static class SummonerSlotName
    {
        public static List<SummonerSlotNameEntity> SummonerSlotNameList => new List<SummonerSlotNameEntity>
        {
            new SummonerSlotNameEntity("SummonerFlash", "閃現"),
            new SummonerSlotNameEntity("Flash", "閃現"),
            new SummonerSlotNameEntity("YoumuusGhostblade", "妖夢鬼刀"),
            new SummonerSlotNameEntity("Youmuu's Ghostblade", "妖夢鬼刀"),
            new SummonerSlotNameEntity("ZhonyasHourglass", "中啞沙漏"),
            new SummonerSlotNameEntity("Hourglass", "中啞沙漏"),
            new SummonerSlotNameEntity("Witchcap", "烏莉特的法帽"),
            new SummonerSlotNameEntity("TalismanOfAscension", "昇華護符"),
            new SummonerSlotNameEntity("Talisman of Ascension", "昇華護符")
        };

        public static string GetSummonerSlotName(string summSlotName)
        {
            var name = SummonerSlotNameList.Find(s => s.SummonerSlotName == summSlotName)?.MeSummonerSlotName.Trim();

            return string.IsNullOrEmpty(name) ? summSlotName : name;
        }

        public static string ToSummSlot(this string summSlotName, bool enable = true)
        {
            if (enable)
            {
                return GetSummonerSlotName(summSlotName);
            }
            else
            {
                return summSlotName;
            }
        }

        public static SpellSlot ToSummSlotName(this string spellname)
        {
            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Summoner1).SData.Name == spellname)
            {
                return SpellSlot.Summoner1;
            }
            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Summoner2).SData.Name == spellname)
            {
                return SpellSlot.Summoner2;
            }
            return SpellSlot.Unknown;

        }
    }
}
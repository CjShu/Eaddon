namespace TW.Common
{
    using System;
    using System.Collections.Generic;
    using EloBuddy;
    using SharpDX;

    public static class Utility
    {
        /// <summary>
        ///     Returns if the GameObject is valid
        /// </summary>
        public static bool IsValid<T>(this GameObject obj) where T : GameObject
        {
            return obj is T && obj.IsValid;
        }

        public static class DelayAction
        {
            #region Static Fields

            public static List<Action> ActionList = new List<Action>();

            #endregion

            #region Constructors and Destructors

            static DelayAction()
            {
                Game.OnTick += GameOnOnGameUpdate;
            }

            #endregion

            #region Delegates

            public delegate void Callback();

            #endregion

            #region Public Methods and Operators

            public static void Add(int time, Callback func)
            {
                var action = new Action(time, func);
                ActionList.Add(action);
            }

            #endregion

            #region Methods

            private static void GameOnOnGameUpdate(EventArgs args)
            {
                for (var i = ActionList.Count - 1; i >= 0; i--)
                {
                    if (ActionList[i].Time <= Utils.GameTimeTickCount)
                    {
                        try
                        {
                            if (ActionList[i].CallbackObject != null)
                            {
                                ActionList[i].CallbackObject();
                                //Will somehow result in calling ALL non-internal marked classes of the called assembly and causes NullReferenceExceptions.
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        ActionList.RemoveAt(i);
                    }
                }
            }

            #endregion

            public struct Action
            {
                #region Fields

                public Callback CallbackObject;

                public int Time;

                #endregion

                #region Constructors and Destructors

                public Action(int time, Callback callback)
                {
                    this.Time = time + Utils.GameTimeTickCount;
                    this.CallbackObject = callback;
                }

                #endregion
            }
        }

        public class Map
        {
            #region Static Fields

            private static readonly IDictionary<int, Map> MapById = new Dictionary<int, Map>
                                                                        {
                                                                            {
                                                                                8,
                                                                                new Map
                                                                                    {
                                                                                        Name = "The Crystal Scar",
                                                                                        ShortName = "crystalScar",
                                                                                        Type = MapType.CrystalScar,
                                                                                        Grid =
                                                                                            new Vector2(
                                                                                                13894f / 2,
                                                                                                13218f / 2),
                                                                                        StartingLevel = 3
                                                                                    }
                                                                            },
                                                                            {
                                                                                10,
                                                                                new Map
                                                                                    {
                                                                                        Name = "The Twisted Treeline",
                                                                                        ShortName = "twistedTreeline",
                                                                                        Type = MapType.TwistedTreeline,
                                                                                        Grid =
                                                                                            new Vector2(
                                                                                                15436f / 2,
                                                                                                14474f / 2),
                                                                                        StartingLevel = 1
                                                                                    }
                                                                            },
                                                                            {
                                                                                11,
                                                                                new Map
                                                                                    {
                                                                                        Name = "Summoner's Rift",
                                                                                        ShortName = "summonerRift",
                                                                                        Type = MapType.SummonersRift,
                                                                                        Grid =
                                                                                            new Vector2(
                                                                                                13982f / 2,
                                                                                                14446f / 2),
                                                                                        StartingLevel = 1
                                                                                    }
                                                                            },
                                                                            {
                                                                                12,
                                                                                new Map
                                                                                    {
                                                                                        Name = "Howling Abyss",
                                                                                        ShortName = "howlingAbyss",
                                                                                        Type = MapType.HowlingAbyss,
                                                                                        Grid =
                                                                                            new Vector2(
                                                                                                13120f / 2,
                                                                                                12618f / 2),
                                                                                        StartingLevel = 3
                                                                                    }
                                                                            }
                                                                        };

            #endregion

            #region Enums

            public enum MapType
            {
                Unknown,

                SummonersRift,

                CrystalScar,

                TwistedTreeline,

                HowlingAbyss
            }

            #endregion

            #region Public Properties

            public Vector2 Grid { get; private set; }

            public string Name { get; private set; }

            public string ShortName { get; private set; }

            public int StartingLevel { get; private set; }

            public MapType Type { get; private set; }

            #endregion

            #region Properties

            private static Map _currentMap { get; set; }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            ///     Returns the current map.
            /// </summary>
            public static Map GetMap()
            {
                if (_currentMap != null)
                {
                    return _currentMap;
                }
                if (MapById.ContainsKey((int)Game.MapId))
                {
                    _currentMap = MapById[(int)Game.MapId];
                    return _currentMap;
                }

                return new Map
                           {
                               Name = "Unknown",
                               ShortName = "unknown",
                               Type = MapType.Unknown,
                               Grid = new Vector2(0, 0),
                               StartingLevel = 1
                           };
            }

            #endregion
        }
    }

    public static class Version
    {
        #region Static Fields

        public static int Build;

        public static int MajorVersion;

        public static int MinorVersion;

        public static int Revision;

        private static readonly int[] VersionArray;

        #endregion

        #region Constructors and Destructors

        static Version()
        {
            var d = Game.Version.Split('.');
            MajorVersion = Convert.ToInt32(d[0]);
            MinorVersion = Convert.ToInt32(d[1]);
            Build = Convert.ToInt32(d[2]);
            Revision = Convert.ToInt32(d[3]);

            VersionArray = new[] { MajorVersion, MinorVersion, Build, Revision };
        }

        #endregion

        #region Public Methods and Operators

        public static bool IsEqual(string version)
        {
            var d = version.Split('.');
            for (var i = 0; i <= d.Length; i++)
            {
                if (d[i] == null || Convert.ToInt32(d[i]) != VersionArray[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsNewer(string version)
        {
            var d = version.Split('.');
            return MinorVersion > Convert.ToInt32(d[1]);
        }

        public static bool IsOlder(string version)
        {
            var d = version.Split('.');
            return MinorVersion < Convert.ToInt32(d[1]);
        }

        #endregion
    }

    public class Vector2Time
    {
        #region Fields

        public Vector2 Position;

        public float Time;

        #endregion

        #region Constructors and Destructors

        public Vector2Time(Vector2 pos, float time)
        {
            this.Position = pos;
            this.Time = time;
        }

        #endregion
    }
}
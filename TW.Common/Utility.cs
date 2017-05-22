namespace TW.Common
{
    using System;
    using System.Collections.Generic;
    using EloBuddy;

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
    }
}

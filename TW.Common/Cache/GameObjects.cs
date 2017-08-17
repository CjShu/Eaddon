namespace TW.Common.Cache
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EloBuddy;

    /// <summary>
    ///     Class GameObjects.
    /// </summary>
    /// <remarks>Members in the list may be invalid. Always check the <see cref="GameObject.IsValid" /> property before use.</remarks>
    public class GameObjects // todo add more!
    {
        #region Static Fields

        /// <summary>
        ///     All game objects
        /// </summary>
        private static HashSet<GameObject> allGameObjects;

        /// <summary>
        ///     The ally heroes
        /// </summary>
        private static HashSet<AIHeroClient> allyHeroes;

        /// <summary>
        ///     The enemy heroes
        /// </summary>
        private static HashSet<AIHeroClient> enemyHeroes;

        /// <summary>
        ///     The heroes
        /// </summary>
        private static HashSet<AIHeroClient> HeroesI;

        /// <summary>
        ///     The ally minions
        /// </summary>
        private static HashSet<Obj_AI_Minion> allyMinions;

        /// <summary>
        ///     The enemy minions
        /// </summary>
        private static HashSet<Obj_AI_Minion> enemyMinionsI;

        /// <summary>
        ///     The minions
        /// </summary>
        private static HashSet<Obj_AI_Minion> minionsI;

        /// <summary>
        ///     The local player
        /// </summary>
        private static AIHeroClient player;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="GameObjects" /> class.
        /// </summary>
        static GameObjects()
        {
            GameObject.OnCreate += GameObjectCreated;
            GameObject.OnDelete += GameObjectDestroyed;

            if (Game.Mode == GameMode.Running)
            {
                Game_OnStart(new EventArgs());
            }

            else
            {
                Game.OnLoad += Game_OnStart;
            }
        }


        private static void Game_OnStart(EventArgs args)
        {
            allGameObjects = CreateHashSet(ObjectManager.Get<GameObject>());

            minionsI = CreateHashSet(ObjectManager.Get<Obj_AI_Minion>());
            allyMinions = CreateHashSet(minionsI.Where(x => x.IsAlly));
            enemyMinionsI = CreateHashSet(minionsI.Where(x => x.IsEnemy));

            HeroesI = CreateHashSet(ObjectManager.Get<AIHeroClient>());
            allyHeroes = CreateHashSet(HeroesI.Where(x => x.IsAlly));
            enemyHeroes = CreateHashSet(HeroesI.Where(x => x.IsEnemy));

            player = ObjectManager.Player;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the local player
        /// </summary>
        /// <value>The local player.</value>
        public static AIHeroClient Player => player;

        /// <summary>
        ///     Gets all game objects.
        /// </summary>
        /// <value>All game objects.</value>
        public static IEnumerable<GameObject> AllGameObjects => allGameObjects;

        /// <summary>
        ///     Gets the ally heroes.
        /// </summary>
        /// <value>The ally heroes.</value>
        public static IEnumerable<AIHeroClient> AllyHeroes => allyHeroes;

        /// <summary>
        ///     Gets the ally minions.
        /// </summary>
        /// <value>The ally minions.</value>
        public static IEnumerable<Obj_AI_Minion> AllyMinions => allyMinions;

        /// <summary>
        ///     Gets the enemy heroes.
        /// </summary>
        /// <value>The enemy heroes.</value>
        public static IEnumerable<AIHeroClient> EnemyHeroes => enemyHeroes;

        /// <summary>
        ///     Gets the enemy minions.
        /// </summary>
        /// <value>The enemy minions.</value>
        public static IEnumerable<Obj_AI_Minion> EnemyMinions => enemyMinionsI;

        /// <summary>
        ///     Gets the heroes.
        /// </summary>
        /// <value>The heroes.</value>
        public static IEnumerable<AIHeroClient> Heroes => HeroesI;

        /// <summary>
        ///     Gets the minions.
        /// </summary>
        /// <value>The minions.</value>
        public static IEnumerable<Obj_AI_Minion> Minions => minionsI;

        #endregion

        #region Public Methods and Operators

        // todo does this actually give a performance benefit?
        /// <summary>
        ///     Gets all GameObjects of type <typeparamref name="T" />
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>IEnumerable{T}</returns>
        public static IEnumerable<T> Get<T>() where T : GameObject
        {
            return allGameObjects.OfType<T>();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Adds the <paramref name="obj" /> to the corresponding lists if it is of type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="generalList">The general list.</param>
        /// <param name="allyList">The ally list.</param>
        /// <param name="enemyList">The enemy list.</param>
        /// <param name="obj">The object.</param>
        private static void Add<T>(
            ref HashSet<T> generalList,
            ref HashSet<T> allyList,
            ref HashSet<T> enemyList,
            GameObject obj)
            where T : GameObject
        {
            var castedObject = obj as T;

            if (castedObject == null)
            {
                return;
            }

            generalList.Remove(castedObject);
            (obj.IsAlly ? allyList : enemyList).Add(castedObject);
        }

        /// <summary>
        ///     Creates the hash set based on the collection using <see cref="GameObjectEqualityComparer" /> as the comparer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <returns>HashSet&lt;T&gt;.</returns>
        private static HashSet<T> CreateHashSet<T>(IEnumerable<T> collection)
            where T : GameObject
        {
            // Override core's equality comparer, which by default
            // checks if the the two references are equal to each other,
            // instead of comparing their network ids
            return new HashSet<T>(collection, new GameObjectEqualityComparer());
        }

        /// <summary>
        ///     Called when a <see cref="GameObject" /> is created.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args"></param>
        private static void GameObjectCreated(GameObject sender, EventArgs args)
        {
            allGameObjects.Add(sender);

            Add(ref minionsI, ref allyMinions, ref enemyMinionsI, sender);
        }

        /// <summary>
        ///     Called when a <see cref="GameObject" /> is destroyed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args"></param>
        private static void GameObjectDestroyed(GameObject sender, EventArgs args)
        {
            allGameObjects.Remove(sender);

            Remove(ref minionsI, ref allyMinions, ref enemyMinionsI, sender);
        }

        /// <summary>
        ///     Adds the <paramref name="obj" /> to the corresponding lists if it is of type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="generalList">The general list.</param>
        /// <param name="allyList">The ally list.</param>
        /// <param name="enemyList">The enemy list.</param>
        /// <param name="obj">The object.</param>
        private static void Remove<T>(
            ref HashSet<T> generalList,
            ref HashSet<T> allyList,
            ref HashSet<T> enemyList,
            GameObject obj)
            where T : GameObject
        {
            var castedObject = obj as T;

            if (castedObject == null)
            {
                return;
            }

            generalList.Remove(castedObject);
            (obj.IsAlly ? allyList : enemyList).Remove(castedObject);
        }

        #endregion
    }
}
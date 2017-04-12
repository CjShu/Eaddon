namespace Caitlyn
{
    using Common;
    using EloBuddy;
    using EloBuddy.SDK.Events;
    using LeagueSharp.SDK;
    using LeagueSharp.SDK.Enumerations;
    using LeagueSharp.SDK.UI;
    using LeagueSharp.SDK.Utils;
    using SharpDX;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Program
    {
        public static Menu Menu;
        private static Spell Q, W, E, R;
        private static bool canCastR = true;
        private static int LastCastQTick = 0, LastCastWTick = 0, castW = 0;
        private static string[] dangerousEnemies = new[]
        {
            "Alistar", "Garen", "Zed", "Fizz", "Rengar", "JarvanIV", "Irelia", "Amumu", "DrMundo", "Ryze", "Fiora", "KhaZix", "LeeSin", "Riven",
            "Lissandra", "Vayne", "Lucian", "Zyra"
        };

        static void Main(string[] atgs)
        {
            Bootstrap.Init();
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        private static void OnLoadingComplete(EventArgs args)
        {
            if (GameObjects.Player.ChampionName != "Caitlyn")
                return;

            InitSpells();
            InitMenu();
            InitEvents();
        }

        private static void InitSpells()
        {
            Q = new Spell(SpellSlot.Q, 1250f);
            W = new Spell(SpellSlot.W, 800f);
            E = new Spell(SpellSlot.E, 750f);
            R = new Spell(SpellSlot.R, 2000f);

            Q.SetSkillshot(0.50f, 50f, 2000f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 60f, 1600f, true, SkillshotType.SkillshotLine);
        }

        private static void InitMenu()
        {
            Menu = new Menu("CjShu Caitlyn", "CjShu 凱特琳", true).Attach();

            Tools.Tools.Inject();

            var QMenu = Menu.Add(new Menu("Q", "Q 設置"));
            QMenu.Add(new MenuBool("Auto", "自動使用", true));
            QMenu.Add(new MenuBool("Combo", "連招啟動", true));
            QMenu.Add(new MenuSlider("ComboRange", "連招啟動|最低釋放距離", 700, 500, (int)Q.Range));
            QMenu.Add(new MenuSliderButton("Harass", "騷擾啟動|自身藍量 >= %", 70, 0, 100, false));
            QMenu.Add(new MenuSliderButton("Lane", "清線啟動|自身藍量 >= %", 70, 0, 100, true));
            QMenu.Add(new MenuSliderButton("Jungle", "清野啟動|自身藍量 >= %", 70, 0, 100, true));

            var WMenu = Menu.Add(new Menu("W", "W 設置"));
            WMenu.Add(new MenuList<string>("Combo", "連招使用模式", new[] { "Rework", "連續控制", "對抗", "關閉" }));
            WMenu.Add(new MenuSlider("ComboCount", "連招最少保留W層數 >=", 1, 1, 5));
            WMenu.Add(new MenuBool("Auto", "自動使用", true));
            WMenu.Add(new MenuBool("AntiGapcloser", "反抗近戰", true));

            var EMenu = Menu.Add(new Menu("E", "E 設置"));
            EMenu.Add(new MenuBool("Combo", "連招啟動", true));
            EMenu.Add(new MenuBool("AntiGapcloser", "反抗近戰", true));
            EMenu.Add(new MenuBool("Flee", "逃跑啟動", true));

            var RMenu = Menu.Add(new Menu("R", "R 設置"));
            RMenu.Add(new MenuBool("Combo", "連招啟動", true));
            RMenu.Add(new MenuSlider("ComboRange", "連招安全R距離 >=", (int)Manager.GetAttackRange(GameObjects.Player) + 200, (int)Manager.GetAttackRange(GameObjects.Player), (int)Manager.GetAttackRange(GameObjects.Player) * 2));
            RMenu.Add(new MenuSlider("ComboSafeRange", "附近敵人數 >= | 禁止R", 3, 1, 5));
            RMenu.Add(new MenuSlider("ComboSetRange", "檢測範圍", 1000, 700, 2000));
            RMenu.Add(new MenuKeyBind("Key", "手動按鍵", System.Windows.Forms.Keys.T, KeyBindType.Press));

            var DrawMenu = Menu.Add(new Menu("Draw", "顯示設置"));
            DrawMenu.Add(new MenuBool("Q", "Q"));
            DrawMenu.Add(new MenuBool("W", "W"));
            DrawMenu.Add(new MenuBool("E", "E"));
            DrawMenu.Add(new MenuBool("R", "R"));
            DrawMenu.Add(new MenuBool("RMinMap", "R 小地圖範圍"));            
            DrawMenu.Add(new MenuBool("RKill", "顯示R能擊殺目標", true));
            DamageIndicator.AddToMenu(DrawMenu);

            Menu.Add(new MenuKeyBind("EQKey", "EQ按鍵", System.Windows.Forms.Keys.G, KeyBindType.Press));
            Menu.Add(new MenuKeyBind("FleeKey", "逃跑按鍵", System.Windows.Forms.Keys.Z, KeyBindType.Press));

            Variables.Orbwalker.Enabled = true;
        }

        private static void InitEvents()
        {
            Drawing.OnEndScene += OnEndScene;
            Drawing.OnDraw += OnDraw;
            Game.OnUpdate += OnUpdate;
            Events.OnGapCloser += OnGapCloser;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Obj_AI_Base.OnBuffGain += OnBuffGain;
        }

        private static void OnBuffGain(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args)
        {
            if (W.IsReady())
            {
                BuffInstance aBuff =
                    (from fBuffs in sender.Buffs.Where(s => sender.Team != ObjectManager.Player.Team && sender.Distance(ObjectManager.Player.Position) < W.Range)
                     from b in new[] { "teleport", "pantheon_grandskyfall_jump", "crowstorm", "zhonya", "katarinar", "MissFortuneBulletTime", "gate", "chronorevive" }
                     where args.Buff.Name.ToLower().Contains(b)
                     select fBuffs).FirstOrDefault();

                if (aBuff != null)
                {
                    CastW(sender.Position);
                }
            }
        }

        private static void OnEndScene(EventArgs args)
        {
            if (R.IsReady() && Menu["Draw"]["RMinMap"].GetValue<MenuBool>().Value)
            {
                var pointList = new List<Vector3>();

                for (var i = 0; i < 30; i++)
                {
                    var angle = i * Math.PI * 2 / 30;

                    pointList.Add(new Vector3(GameObjects.Player.Position.X + R.Range * (float)Math.Cos(angle), GameObjects.Player.Position.Y + R.Range * (float)Math.Sin(angle), GameObjects.Player.Position.Z));
                }

                for (var i = 0; i < pointList.Count; i++)
                {
                    var a = pointList[i];
                    var b = pointList[i == pointList.Count - 1 ? 0 : i + 1];

                    var aonScreen = Drawing.WorldToMinimap(a);
                    var bonScreen = Drawing.WorldToMinimap(b);
                    var aon1Screen = Drawing.WorldToScreen(a);
                    var bon1Screen = Drawing.WorldToScreen(b);

                    Drawing.DrawLine(aon1Screen.X, aon1Screen.Y, bon1Screen.X, bon1Screen.Y, 1, System.Drawing.Color.White);
                    Drawing.DrawLine(aonScreen.X, aonScreen.Y, bonScreen.X, bonScreen.Y, 1, System.Drawing.Color.White);
                }
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (GameObjects.Player.IsDead)
                return;

            if (Menu["Draw"]["Q"] && Q.IsReady())
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, Q.Range, System.Drawing.Color.LightBlue, 2);
            }

            if (Menu["Draw"]["W"] && W.IsReady())
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, W.Range, System.Drawing.Color.OrangeRed, 2);
            }

            if (Menu["Draw"]["E"] && E.IsReady())
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, E.Range, System.Drawing.Color.LightYellow, 2);
            }

            if (Menu["Draw"]["R"] && R.IsReady())
            {
                Render.Circle.DrawCircle(GameObjects.Player.Position, R.Range, System.Drawing.Color.Green, 2);
            }

            if (Menu["Draw"]["RKill"])
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && x.Health < R.GetDamage(x)))
                {
                    if (target != null)
                    {
                        Drawing.DrawText(Drawing.WorldToScreen(target.Position)[0] - 20, Drawing.WorldToScreen(target.Position)[1], System.Drawing.Color.Red, "R KillAble!!!!!");
                    }
                }
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            R.Range = 500 * (R.Level == 0 ? 1 : R.Level) + 1500;

            if (GameObjects.Player.IsDead)
                return;

            if (Menu["R"]["Key"].GetValue<MenuKeyBind>().Active && R.IsReady())
            {
                var select = Variables.TargetSelector.GetSelectedTarget();
                var target = Variables.TargetSelector.GetTarget(R);

                if (select != null && target.IsValidTarget(R.Range))
                {
                    R.CastOnUnit(select);
                    return;
                }
                else if (select == null && target != null && target.IsValidTarget(R.Range))
                {
                    R.CastOnUnit(target);
                    return;
                }
            }

            if (Menu["EQKey"].GetValue<MenuKeyBind>().Active && E.IsReady() && Q.IsReady())
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

                var t = Variables.TargetSelector.GetTarget(E.Range, DamageType.Physical);

                if (t.IsValidTarget(E.Range) && t.Health >= Q.GetDamage(t) + E.GetDamage(t) + 20 && E.CanCast(t))
                {
                    E.Cast(t);
                    CastQ(t);
                    return;
                }
            }

            if (Menu["FleeKey"].GetValue<MenuKeyBind>().Active)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

                if (Menu["E"]["Flee"] && E.IsReady())
                {
                    var position = ObjectManager.Player.ServerPosition - (Game.CursorPos - ObjectManager.Player.ServerPosition);
                    E.Cast(position);
                }

                return;
            }

            AutoLogic();

            if (Variables.Orbwalker.ActiveMode == OrbwalkingMode.Combo)
            {
                ComboLogic();
            }

            if (Variables.Orbwalker.ActiveMode == OrbwalkingMode.Hybrid)
            {
                HarassLogic();
            }

            if (Variables.Orbwalker.ActiveMode == OrbwalkingMode.LaneClear)
            {
                LaneLogic();
                JungleLogic();
            }

            if (Menu["E"]["Combo"] && Menu["Q"]["Combo"] && E.IsReady() && Q.IsReady() && Variables.Orbwalker.ActiveMode == OrbwalkingMode.Combo)
            {
                var t = Variables.TargetSelector.GetTarget(E.Range, DamageType.Physical);

                if (t.IsValidTarget(E.Range) && t.Health <= Q.GetDamage(t) + E.GetDamage(t) + 20 && E.CanCast(t))
                {
                    E.Cast(t);
                    CastQ(t);
                }
            }
        }

        private static void LaneLogic()
        {
            if (Menu["Q"]["Lane"].GetValue<MenuSliderButton>().BValue && GameObjects.Player.ManaPercent >= Menu["Q"]["Lane"].GetValue<MenuSliderButton>().SValue && Q.IsReady())
            {
                var Minions = GameObjects.Minions.Where(x => x.IsValidTarget(Q.Range) && x.IsEnemy && x.IsMinion && x.Team != GameObjectTeam.Neutral).ToList();

                if (Minions.Count() > 0)
                {
                    var QFarm = Q.GetLineFarmLocation(Minions);

                    if (QFarm.MinionsHit >= 3)
                    {
                        Q.Cast(QFarm.Position);
                    }
                }
            }
        }

        private static void JungleLogic()
        {
            if (Menu["Q"]["Jungle"].GetValue<MenuSliderButton>().BValue && GameObjects.Player.ManaPercent >= Menu["Q"]["Jungle"].GetValue<MenuSliderButton>().SValue && Q.IsReady())
            {
                var Mobs = ObjectManager.Get<Obj_AI_Minion>().Where(x => !x.IsDead && !x.IsZombie && x.Team == GameObjectTeam.Neutral && x.IsValidTarget(Q.Range)).ToList();

                if (Mobs.Count() > 0)
                {
                    foreach (var mob in Mobs)
                    {
                        Q.Cast(mob);
                    }
                }
            }
        }

        private static void AutoLogic()
        {
            if (Menu["W"]["Auto"] && W.IsReady() && Variables.Orbwalker.ActiveMode == OrbwalkingMode.None)
            {
                var t = Variables.TargetSelector.GetTarget(W.Range, DamageType.Physical);
                if (t.IsValidTarget(W.Range))
                {
                    if (t.HasBuffOfType(BuffType.Stun) || t.HasBuffOfType(BuffType.Snare) || t.HasBuffOfType(BuffType.Taunt) || t.HasBuffOfType(BuffType.Knockup) || t.HasBuff("zhonyasringshield") || t.HasBuff("Recall"))
                    {
                        CastW(t.Position);
                    }

                    if (t.HasBuffOfType(BuffType.Slow))
                    {
                        var hit = t.IsFacing(ObjectManager.Player) ? t.Position.Extend(ObjectManager.Player.Position, +140) : t.Position.Extend(ObjectManager.Player.Position, -140);

                        CastW(hit);
                    }
                }
            }

            if (Menu["Q"]["Auto"] && Q.IsReady() && Variables.Orbwalker.ActiveMode == OrbwalkingMode.None)
            {
                var t = Variables.TargetSelector.GetTarget(Q.Range - 30, DamageType.Physical);

                if (t.IsValidTarget(Q.Range) && (t.HasBuffOfType(BuffType.Stun) || t.HasBuffOfType(BuffType.Snare) || t.HasBuffOfType(BuffType.Taunt) || (t.Health <= ObjectManager.Player.GetSpellDamage(t, SpellSlot.Q) && t.DistanceToPlayer() >= GameObjects.Player.AttackRange)))
                {
                    CastQ(t);
                }
            }
        }

        private static void HarassLogic()
        {
            if (Menu["Q"]["Harass"].GetValue<MenuSliderButton>().BValue && GameObjects.Player.ManaPercent >= Menu["Q"]["Harass"].GetValue<MenuSliderButton>().SValue && Q.IsReady())
            {
                var targets = GameObjects.EnemyHeroes.Where(e => !e.IsDead && !e.IsZombie && e.IsHPBarRendered && e.IsValidTarget(Q.Range));

                foreach (var target in targets)
                {
                    if (GameObjects.Player.CountEnemyHeroesInRange(bonusRange() + 100 + target.BoundingRadius) == 0)
                    {
                        CastQ(target);
                    }
                }
            }
        }

        private static void ComboLogic()
        {
            var target = Manager.GetTarget(R.Range, DamageType.Physical);

            if (Manager.CheckTarget(target))
            {
                if (Menu["E"]["Combo"] && E.IsReady() && target.IsValidTarget(target.IsFacing(ObjectManager.Player) ? E.Range - 200 : E.Range - 300) && E.GetPrediction(target).CollisionObjects.Count == 0)
                {
                    E.Cast(target.Position);
                }

                if (Menu["Q"]["Combo"] && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    if (GetRealDistance(target) > Menu["Q"]["ComboRange"].GetValue<MenuSlider>().Value && !Manager.InAutoAttackRange(target) && target.Health < Q.GetDamage(target) && GameObjects.Player.CountEnemyHeroesInRange(400) == 0)
                    {
                        CastQ(target);
                    }
                    else if (GameObjects.Player.Mana > R.skillshot.ManaCost + Q.skillshot.ManaCost + E.skillshot.ManaCost + 10 && GameObjects.Player.CountEnemyHeroesInRange(bonusRange() + 100 + target.BoundingRadius) == 0)
                    {
                        CastQ(target);
                    }
                }

                if (Menu["R"]["Combo"] && R.IsReady() && !GameObjects.Player.IsUnderEnemyTurret() && target.IsValidTarget(R.Range) && target.Health < R.GetDamage(target) && GameObjects.Player.CountEnemyHeroesInRange(Menu["R"]["ComboSetRange"].GetValue<MenuSlider>().Value) < Menu["R"]["ComboSafeRange"].GetValue<MenuSlider>().Value && target.DistanceToPlayer() > Menu["R"]["ComboRange"].GetValue<MenuSlider>().Value && canCastR && LastCastQTick + 1000 >= Variables.TickCount)
                {
                    bool cast = true;

                    PredictionOutput output = R.GetPrediction(target);
                    Vector2 direction = output.CastPosition.ToVector2() - GameObjects.Player.Position.ToVector2();
                    direction.Normalize();
                    List<AIHeroClient> enemies = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget()).ToList();

                    foreach (var enemy in enemies)
                    {
                        if (enemy.BaseSkinName == target.BaseSkinName || !cast)
                            continue;

                        PredictionOutput prediction = R.GetPrediction(enemy);
                        Vector3 predictedPosition = prediction.CastPosition;
                        Vector3 v = output.CastPosition - GameObjects.Player.ServerPosition;
                        Vector3 w = predictedPosition - GameObjects.Player.ServerPosition;
                        double c1 = Vector3.Dot(w, v);
                        double c2 = Vector3.Dot(v, v);
                        double b = c1 / c2;
                        Vector3 pb = GameObjects.Player.ServerPosition + ((float)b * v);
                        float length = Vector3.Distance(predictedPosition, pb);

                        if (length < (400f + enemy.BoundingRadius) && GameObjects.Player.Distance(predictedPosition) < GameObjects.Player.Distance(target.ServerPosition))
                            cast = false;
                    }

                    if (cast)
                    {
                        R.CastOnUnit(target);
                        return;
                    }
                }

                if (W.IsReady() && Menu["W"]["Combo"].GetValue<MenuList>().Index != 3 && target.IsValidTarget(W.Range) && !GameObjects.Player.Spellbook.IsAutoAttacking && GameObjects.Player.Spellbook.GetSpell(SpellSlot.W).Ammo >= Menu["W"]["ComboCount"].GetValue<MenuSlider>().Value + 1)
                {
                    if (Menu["W"]["Combo"].GetValue<MenuList>().Index == 0)
                    {
                        var targetw = Variables.TargetSelector.GetTarget(W.Range, DamageType.Physical);

                        if (targetw.IsValidTarget(W.Range))
                        {
                            var prediction = Movement.GetPrediction(
                                    new PredictionInput
                                    {
                                        Unit = targetw,
                                        Delay = W.Delay,
                                        Radius = W.Width,
                                        Speed = W.Speed,
                                        Range = W.Range
                                    });

                            if (targetw.IsMelee && targetw.IsFacing(ObjectManager.Player) && targetw.Distance(GameObjects.Player) < 300 && Environment.TickCount - castW > 1300)
                            {
                                W.Cast(GameObjects.Player);
                                castW = Environment.TickCount;
                            }

                            if (prediction.Hitchance >= HitChance.VeryHigh && targetw.IsFacing(GameObjects.Player) && Environment.TickCount - castW > 1300)
                            {
                                W.Cast(prediction.CastPosition);
                                castW = Environment.TickCount;
                            }

                            if (!targetw.IsFacing(GameObjects.Player) && Environment.TickCount - castW > 2000)
                            {
                                var vector = targetw.ServerPosition - GameObjects.Player.Position;
                                var Behind = W.GetPrediction(targetw).CastPosition + Vector3.Normalize(vector) * 100;

                                W.Cast(Behind);
                                castW = Environment.TickCount;
                            }
                        }
                    }
                    else if (Menu["W"]["Combo"].GetValue<MenuList>().Index == 1)
                    {
                        var pred = GetPrediction(target, W);

                        if (!GameObjects.AllyMinions.Any(m => !m.IsDead && m.CharData.BaseSkinName.Contains("trap") && m.Distance(pred.Item2) < 100) && (int)pred.Item1 > (int)HitChance.Medium && GameObjects.Player.Distance(pred.Item2) < W.Range)
                        {
                            CastW(pred.Item2);
                        }
                    }
                    else if (Menu["W"]["Combo"].GetValue<MenuList>().Index == 2)
                    {
                        if (ObjectManager.Player.Distance(target) < 450 && target.IsFacing(GameObjects.Player))
                        {
                            CastW(Common.Geometry.CenterOfVectors(new[] { GameObjects.Player.Position, target.Position }));
                        }
                    }
                }
            }
        }

        private static void OnGapCloser(object sender, Events.GapCloserEventArgs args)
        {
            if (Menu["E"]["AntiGapcloser"] && E.IsReady() && args.Sender.IsValidTarget(E.Range))
            {
                E.Cast(args.Sender.Position);
            }

            if (Menu["W"]["AntiGapcloser"] && W.IsReady() && args.Sender.IsValidTarget(W.Range))
            {
                if (args.End.DistanceToPlayer() < 180)
                {
                    W.Cast(ObjectManager.Player.Position);
                }
                else
                {
                    W.Cast(args.End);
                }
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.Slot == SpellSlot.Q)
            {
                LastCastQTick = Variables.TickCount;
            }

            if (args.Slot == SpellSlot.W)
            {
                LastCastWTick = Variables.TickCount;
            }

            if (sender.IsEnemy && sender is Obj_AI_Turret && args.Target.IsMe)
            {
                canCastR = false;
            }
            else
            {
                canCastR = true;
            }
        }

        private static void CastQ(Obj_AI_Base t)
        {
            if (Q.CanCast(t))
            {
                var qPrediction = Q.GetPrediction(t);
                var hithere = qPrediction.CastPosition.Extend(ObjectManager.Player.Position, -100);

                if (qPrediction.Hitchance >= HitChance.VeryHigh)
                {
                    Q.Cast(hithere);
                }
            }
        }

        private static void CastW(Vector3 pos)
        {
            if (W.IsReady() && LastCastWTick + 1500 >= Variables.TickCount)
            {
                W.Cast(pos);
            }
        }

        private static float GetRealDistance(GameObject target)
        {
            return GameObjects.Player.ServerPosition.Distance(target.Position) + GameObjects.Player.BoundingRadius + target.BoundingRadius;
        }

        private static float bonusRange()
        {
            return 720f + GameObjects.Player.BoundingRadius;
        }

        public static Tuple<HitChance, Vector3, List<Obj_AI_Base>> GetPrediction(AIHeroClient target, Spell spell)
        {
            var pred = spell.GetPrediction(target);
            return new Tuple<HitChance, Vector3, List<Obj_AI_Base>>(pred.Hitchance, pred.UnitPosition, pred.CollisionObjects);
        }
    }
}
﻿namespace Caitlyn
{
    using Common;
    using EloBuddy;
    using EloBuddy.SDK.Events;
    using LeagueSharp.Common;
    using SharpDX;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Program
    {
        public static Menu Menu;
        private static AIHeroClient player => HeroManager.Player;
        private static Spell Q, W, E, R;
        private static bool canCastR = true;
        private static int LastCastQTick = 0, LastCastWTick = 0, castW = 0;
        public static Orbwalking.Orbwalker Orbwalker;
        private static string[] dangerousEnemies = new[]
        {
            "Alistar", "Garen", "Zed", "Fizz", "Rengar", "JarvanIV", "Irelia", "Amumu", "DrMundo", "Ryze", "Fiora", "KhaZix", "LeeSin", "Riven",
            "Lissandra", "Vayne", "Lucian", "Zyra"
        };

        static void Main(string[] atgs)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        private static void OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Caitlyn")
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
            (Menu = new Menu("CjShu Caitlyn", "CjShu 凱特琳", true)).AddToMainMenu();

            Menu.AddSubMenu(new Menu("走砍設置", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalking"));

            Tools.Tools.Inject();

            var QMenu = Menu.AddSubMenu(new Menu("Q 設置", "Q"));
            QMenu.AddItem(new MenuItem("AutoQ", "自動使用").SetValue(true));
            QMenu.AddItem(new MenuItem("ComboQ", "連招啟動").SetValue(true));
            QMenu.AddItem(new MenuItem("ComboRangeQ", "連招啟動|最低釋放距離").SetValue(new
                Slider(700, 500, (int)Q.Range)));
            QMenu.AddItem(new MenuItem("Harass", "騷擾啟動").SetValue(false));
            QMenu.AddItem(new MenuItem("HarassMana", "騷擾|自身藍量 >= %").SetValue(new
                Slider(60, 0, 100)));
            QMenu.AddItem(new MenuItem("Lane", "清線啟動").SetValue(false));
            QMenu.AddItem(new MenuItem("LaneMana", "清線|自身藍量 >= %").SetValue(new
                Slider(60, 0, 100)));
            QMenu.AddItem(new MenuItem("LaneMinionsCount", "清線最少小兵數 >= ").SetValue(new
                Slider(4, 3, 6)));
            QMenu.AddItem(new MenuItem("Jungle", "清野啟動").SetValue(false));
            QMenu.AddItem(new MenuItem("JungleMana", "清野|自身藍量 >= %").SetValue(new
                Slider(60, 0, 100))); 

            var WMenu = Menu.AddSubMenu(new Menu("W 設置", "W"));
            WMenu.AddItem(new MenuItem("ComboWMode", "連招模式").SetValue(new
                StringList(new[] { "Rework", "連續控制", "對抗", "關閉" })));
            WMenu.AddItem(new MenuItem("ComboWCount", "連招最少保留W層數 >=").SetValue(new
                Slider(1, 1, 5)));
            WMenu.AddItem(new MenuItem("AutoW", "自動使用").SetValue(true));
            WMenu.AddItem(new MenuItem("AntiGapcloserW", "反抗近戰").SetValue(true));

            var EMenu = Menu.AddSubMenu(new Menu("E 設置", "E"));
            EMenu.AddItem(new MenuItem("ComboE", "連招啟動").SetValue(true));
            EMenu.AddItem(new MenuItem("AntiGapcloserE", "反抗近戰").SetValue(true));
            EMenu.AddItem(new MenuItem("FleeE", "逃跑啟動").SetValue(true));
            EMenu.AddItem(new MenuItem("AntiRengar", "使用 E 對獅子").SetValue(true));
            EMenu.AddItem(new MenuItem("AntiKhazix", "使用 E 對卡力斯").SetValue(true));

            var RMenu = Menu.AddSubMenu(new Menu("R 設置", "R"));
            RMenu.AddItem(new MenuItem("ComboR", "連招啟動").SetValue(true));
            RMenu.AddItem(new MenuItem("ComboRangeR", "連招安全R距離 >= ").SetValue(new
                Slider((int)Manager.GetAttackRange(Player.Instance) + 200,
                (int)Manager.GetAttackRange(Player.Instance),
                (int)Manager.GetAttackRange(Player.Instance) * 2)));

            RMenu.AddItem(new MenuItem("ComboSafeRange", "附近敵人數 >= | 禁止R").SetValue(new
                Slider(3, 1, 5)));
            RMenu.AddItem(new MenuItem("ComboSetRange", "檢測範圍").SetValue(new
                Slider(1000, 700, 2000)));
            RMenu.AddItem(new MenuItem("RKey", "手動按鍵").SetValue(new
                KeyBind('T', KeyBindType.Press)));

            var DrawMenu = Menu.AddSubMenu(new Menu("顯示 設置", "Draw"));
            DrawMenu.AddItem(new MenuItem("QDraw", "Q").SetValue(false));
            DrawMenu.AddItem(new MenuItem("WDraw", "W").SetValue(false));
            DrawMenu.AddItem(new MenuItem("EDraw", "E").SetValue(false));
            DrawMenu.AddItem(new MenuItem("RDraw", "R").SetValue(false));
            DrawMenu.AddItem(new MenuItem("RKill", "顯示R能擊殺目標").SetValue(true));
            //DamageIndicator.AddToMenu(DrawMenu);

            Menu.AddItem(new MenuItem("EQKey", "EQ按鍵").SetValue(new
               KeyBind('G', KeyBindType.Press)));
        }

        private static void InitEvents()
        {
            GameObject.OnCreate += OnCreate;
            Drawing.OnDraw += OnDraw;
            Game.OnUpdate += OnUpdate;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Obj_AI_Base.OnBuffGain += OnBuffGain;
        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            var Rengar = HeroManager.Enemies.Find(heros => heros.ChampionName.Equals("Rengar"));
            var Khazix = HeroManager.Enemies.Find(heros => heros.ChampionName.Equals("Khazix"));

            if (Rengar != null && Menu.Item("AntiRengar").GetValue<bool>())
            {
                if (sender.Name == "Rengar_LeapSound.troy" && sender.Position.Distance(player.Position) < E.Range)
                {
                    E.Cast(Rengar.Position, true);
                }
            }

            if (Khazix != null && Menu.Item("AntiKhazix").GetValue<bool>())
            {
                if (sender.Name == "Khazix_Base_E_Tar.troy" && sender.Position.Distance(player.Position) <= 300)
                {
                    E.Cast(Khazix.Position, true);
                }
            }
        }

        private static void OnEnemyGapcloser(ActiveGapcloser args)
        {
            if (Menu.Item("AntiGapcloserW").GetValue<bool>() && E.IsReady() && args.Sender.IsValidTarget(E.Range))
            {
                E.Cast(args.Sender.Position, true);
            }

            if (Menu.Item("AntiGapcloserE").GetValue<bool>() && W.IsReady() && args.Sender.IsValidTarget(W.Range))
            {
                if (args.End.DistanceToPlayer() < 180)
                {
                    W.Cast(player.Position, true);
                }
                else
                {
                    W.Cast(args.End);
                }
            }
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

        private static void OnDraw(EventArgs args)
        {
            if (!player.IsDead && !MenuGUI.IsChatOpen && !Chat.IsOpen)
            {
                if (Menu.Item("QDraw").GetValue<bool>() && Q.IsReady())
                {
                    Render.Circle.DrawCircle(player.Position, Q.Range, System.Drawing.Color.BlueViolet, 4);
                }

                if (Menu.Item("WDraw").GetValue<bool>() && W.IsReady())
                {
                    Render.Circle.DrawCircle(player.Position, W.Range, System.Drawing.Color.OrangeRed, 4);
                }

                if (Menu.Item("EDraw").GetValue<bool>() && E.IsReady())
                {
                    Render.Circle.DrawCircle(player.Position, E.Range, System.Drawing.Color.LightYellow, 4);
                }

                if (Menu.Item("RDraw").GetValue<bool>() && R.IsReady())
                {
                    Render.Circle.DrawCircle(player.Position, R.Range, System.Drawing.Color.Yellow, 4);
                }

                if (Menu.Item("RKill").GetValue<bool>())
                {
                    foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie && x.Health < R.GetDamage(x)))
                    {
                        if (target != null)
                        {
                            Drawing.DrawText(Drawing.WorldToScreen(target.Position)[0] - 20, Drawing.WorldToScreen(target.Position)[1], System.Drawing.Color.Red, "R !!!!!");
                        }
                    }
                }
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            R.Range = 500 * (R.Level == 0 ? 1 : R.Level) + 1500;

            if (player.IsDead)
                return;

            if (Menu.Item("RKey").GetValue<KeyBind>().Active && R.IsReady())
            {
                var select = TargetSelector.GetSelectedTarget();
                var target = TargetSelector.GetTargetNoCollision(R);

                if (select != null && target.IsValidTarget(R.Range))
                {
                    R.CastOnUnit(select, true);
                    return;
                }
                else if (select == null && target != null && target.IsValidTarget(R.Range))
                {
                    R.CastOnUnit(target, true);
                    return;
                }
            }

            if (Menu.Item("EQKey").GetValue<KeyBind>().Active && E.IsReady() && Q.IsReady())
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

                var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);

                if (t.IsValidTarget(E.Range) && t.Health >= Q.GetDamage(t) + E.GetDamage(t) + 20 && E.CanCast(t))
                {
                    E.Cast(t, true);
                    CastQ(t);
                    return;
                }
            }

            AutoLogic();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                ComboLogic();
                EQLogic();
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                HarassLogic();
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                LaneLogic();
                JungleLogic();
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Flee:
                    Orbwalking.MoveTo(Game.CursorPos);
                    if (Menu.Item("FleeE").GetValue<bool>() && E.IsReady())
                    {
                        E.Cast(player.Position - (Game.CursorPos - player.Position));
                    }
                    break;
            }
        }

        private static void EQLogic()
        {
            if (Menu.Item("ComboE").GetValue<bool>() && Menu.Item("ComboQ").GetValue<bool>())
            {
                if (E.IsReady() && Q.IsReady())
                {
                    var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);

                    if (t.IsValidTarget(E.Range) && t.Health <= Q.GetDamage(t) + E.GetDamage(t) + 20 && E.CanCast(t))
                    {
                        E.Cast(t, true);
                        CastQ(t);
                    }
                }
            }
        }

        private static void LaneLogic()
        {
            if (Menu.Item("Lane").GetValue<bool>() && Q.IsReady())
            {
                if (player.ManaPercent >= Menu.Item("LaneMana").GetValue<Slider>().Value)
                {
                    var minion = MinionManager.GetMinions(player.Position, Q.Range);

                    if (minion.Any())
                    {
                        if (minion.Count() > 0)
                        {
                            var QFarm = Q.GetLineFarmLocation(minion);

                            if (QFarm.MinionsHit >= Menu.Item("LaneMinionsCount").GetValue<Slider>().Value)
                            {
                                Q.Cast(QFarm.Position, true);
                            }
                        }
                    }
                }
            }
        }

        private static void JungleLogic()
        {
            if (Menu.Item("Jungle").GetValue<bool>() && Q.IsReady())
            {
                if (player.ManaPercent >= Menu.Item("JungleMana").GetValue<Slider>().Value)
                {
                    var Mobs = ObjectManager.Get<Obj_AI_Minion>().Where(
                        x => !x.IsDead && !x.IsZombie && x.Team == GameObjectTeam.Neutral
                        && x.IsValidTarget(Q.Range)).ToList();

                    if (Mobs.Count() > 0)
                    {
                        foreach (var mob in Mobs)
                        {
                            Q.Cast(mob, true);
                        }
                    }
                }
            }
        }

        private static void AutoLogic()
        {
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None)
            {
                if (Menu.Item("AutoW").GetValue<bool>() && W.IsReady())
                {
                    var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);

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

                if (Menu.Item("AutoQ").GetValue<bool>() && Q.IsReady())
                {
                    var t = TargetSelector.GetTarget(Q.Range - 30, TargetSelector.DamageType.Physical);

                    if (t.IsValidTarget(Q.Range) && (t.HasBuffOfType(BuffType.Stun) || t.HasBuffOfType(BuffType.Snare) || t.HasBuffOfType(BuffType.Taunt) || (t.Health <= ObjectManager.Player.GetSpellDamage(t, SpellSlot.Q) && t.DistanceToPlayer() >= player.AttackRange)))
                    {
                        CastQ(t);
                    }
                }
            }
        }

        private static void HarassLogic()
        {
            if (Menu.Item("Harass").GetValue<bool>() && Q.IsReady())
            {
                if (player.ManaPercent >= Menu.Item("HarassMana").GetValue<Slider>().Value)
                {
                    var targets = HeroManager.Enemies.Where(e => !e.IsDead && !e.IsZombie && e.IsHPBarRendered && e.IsValidTarget(Q.Range));

                    foreach (var target in targets)
                    {
                        if (player.CountEnemiesInRange(bonusRange() + 100 + target.BoundingRadius) == 0)
                        {
                            CastQ(target);
                        }
                    }
                }
            }
        }

        private static void ComboLogic()
        {
            var target = TargetSelector.GetSelectedTarget() ?? TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

            if (target.Check(R.Range))
            {
                if (Menu.Item("ComboE").GetValue<bool>() && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    if (target.IsValidTarget(target.IsFacing(player) ? E.Range - 200 : E.Range - 300) && E.GetPrediction(target).CollisionObjects.Count == 0)
                    {
                        E.Cast(target.Position, true);
                    }
                }

                if (Menu.Item("ComboQ").GetValue<bool>() && !E.IsReady() && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    if (target.DistanceToPlayer() >= Menu.Item("ComboRangeQ").GetValue<Slider>().Value && !Manager.InAutoAttackRange(target)
                        && target.Health < Q.GetDamage(target) && player.CountEnemiesInRange(400) == 0)
                    {
                        CastQ(target);
                    }
                    else if (player.Mana > R.ManaCost + Q.ManaCost + E.ManaCost + 10 && player.CountEnemiesInRange(bonusRange() + 100 + target.BoundingRadius) == 0)
                    {
                        CastQ(target);
                    }
                }

                if (Menu.Item("ComboR").GetValue<bool>() && R.IsReady())
                {
                    if (!player.UnderTurret(true))
                    {
                        return;
                    }

                    if (target.Health < R.GetDamage(target)
                        && player.CountEnemiesInRange(Menu.Item("ComboSetRange").GetValue<Slider>().Value)
                        < Menu.Item("ComboSafeRange").GetValue<Slider>().Value
                        && target.DistanceToPlayer() > Menu.Item("ComboRangeR").GetValue<Slider>().Value
                        && canCastR && LastCastQTick + 1000 >= Utils.TickCount)
                    {
                        bool cast = true;

                        PredictionOutput output = R.GetPrediction(target);
                        Vector2 direction = output.CastPosition.To2D() - player.Position.To2D();
                        direction.Normalize();
                        List<AIHeroClient> enemies = HeroManager.Enemies.Where(x => x.IsValidTarget()).ToList();

                        foreach (var enemy in enemies)
                        {
                            if (enemy.BaseSkinName == target.BaseSkinName || !cast)
                                continue;

                            PredictionOutput prediction = R.GetPrediction(enemy);
                            Vector3 predictedPosition = prediction.CastPosition;
                            Vector3 v = output.CastPosition - player.ServerPosition;
                            Vector3 w = predictedPosition - player.ServerPosition;
                            double c1 = Vector3.Dot(w, v);
                            double c2 = Vector3.Dot(v, v);
                            double b = c1 / c2;
                            Vector3 pb = player.ServerPosition + ((float)b * v);
                            float length = Vector3.Distance(predictedPosition, pb);

                            if (length < (400f + enemy.BoundingRadius) && player.Distance(predictedPosition)
                                < player.Distance(target.ServerPosition))
                                cast = false;
                        }

                        if (cast)
                        {
                            R.CastOnUnit(target, true);
                            return;
                        }
                    }
                }

                if (Menu.Item("ComboWMode").GetValue<StringList>().SelectedIndex != 3)
                {
                    if (W.IsReady() && target.IsValidTarget(W.Range) && !player.Spellbook.IsAutoAttacking
                        && player.Spellbook.GetSpell(SpellSlot.W).Ammo
                        >= Menu.Item("ComboWCount").GetValue<Slider>().Value + 1)
                    {
                        if (Menu.Item("ComboWMode").GetValue<StringList>().SelectedIndex == 0)
                        {
                            var tw = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);

                            if (tw.IsValidTarget(W.Range))
                            {
                                var prediction = Prediction.GetPrediction(
                                    new PredictionInput
                                    {
                                        Unit = tw,
                                        Delay = W.Delay,
                                        Radius = W.Width,
                                        Speed = W.Speed,
                                        Range = W.Range
                                    });

                                if (tw.IsMelee && target.IsFacing(player) && tw.Distance(player) < 300 && Environment.TickCount - castW > 1300)
                                {
                                    W.Cast(player, true);
                                    castW = Environment.TickCount;
                                }

                                if (prediction.Hitchance >= HitChance.VeryHigh && tw.IsFacing(player) && Environment.TickCount - castW > 1300)
                                {
                                    W.Cast(prediction.CastPosition, true);
                                    castW = Environment.TickCount;
                                }

                                if (!target.IsFacing(player) && Environment.TickCount - castW > 2000)
                                {
                                    var vector = tw.ServerPosition - Player.Instance.Position;
                                    var Behind = W.GetPrediction(tw).CastPosition + Vector3.Normalize(vector) * 100;

                                    W.Cast(Behind, true);
                                    castW = Environment.TickCount;
                                }
                            }
                        }
                        else if (Menu.Item("ComboWMode").GetValue<StringList>().SelectedIndex == 1)
                        {
                            var tw1 = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
                            var pred = GetPrediction(tw1, W);

                            if (!MinionManager.GetMinions(player.Position, W.Range, MinionTypes.All, MinionTeam.Ally, MinionOrderTypes.None).Any(
                                m => !m.IsDead && m.Name.ToLower().Contains("trap") && m.Distance(pred.Item2) < 100) && (int)pred.Item1
                                > (int)HitChance.High && player.Distance(pred.Item2) < W.Range)
                            {
                                CastW(pred.Item2);
                            }
                        }
                        else if (Menu.Item("ComboWMode").GetValue<StringList>().SelectedIndex == 2)
                        {
                            if (player.Distance(target) < 450 && target.IsFacing(player))
                            {
                                CastW(Common.Geometry.CenterOfVectors(new[] { player.Position, target.Position }));
                            }
                        }
                    }
                }
            }
        }
        
        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.Slot == SpellSlot.Q)
            {
                LastCastQTick = Utils.TickCount;
            }

            if (args.Slot == SpellSlot.W)
            {
                LastCastWTick = Utils.TickCount;
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
                    Q.Cast(hithere, true);
                }
            }
        }

        private static void CastW(Vector3 pos)
        {
            if (W.IsReady() && LastCastWTick + 1500 >= Utils.GameTimeTickCount)
            {
                W.Cast(pos, true);
            }
        }

        private static float GetRealDistance(GameObject target)
        {
            return player.Distance(target.Position) + player.BoundingRadius + target.BoundingRadius;
        }

        private static float bonusRange()
        {
            return 720f + player.BoundingRadius;
        }

        public static Tuple<HitChance, Vector3, List<Obj_AI_Base>> GetPrediction(AIHeroClient target, Spell spell)
        {
            var pred = spell.GetPrediction(target);
            return new Tuple<HitChance, Vector3, List<Obj_AI_Base>>(pred.Hitchance, pred.UnitPosition, pred.CollisionObjects);
        }

        public static void PredCast(Spell spell, Obj_AI_Base target, bool isAOE = false)
        {
            if (!spell.IsReady())
            {
                return;
            }

            var pred = spell.GetPrediction(target, isAOE);

            if (pred.Hitchance >= HitChance.VeryHigh)
            {
                spell.Cast(pred.CastPosition, true);
            }
        }
    }
}
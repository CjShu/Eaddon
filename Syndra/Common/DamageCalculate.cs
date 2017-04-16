namespace Syndra.Common
{
    using EloBuddy;
    using LeagueSharp.Common;
    using System;

    public static class DamageCalculate
    {
        public static float GetComboDamage(this Obj_AI_Base target)
        {
            if (target == null || target.IsDead || target.IsZombie)
            {
                return 0;
            }

            var damage = 0d;

            damage += Program.Q.IsReady(420) ? GetQDamage(target) : 0;
            damage += Program.W.IsReady() ? GetWDamage(target) : 0;
            damage += Program.E.IsReady() ? GetEDamage(target) : 0;
            
            if (Program.R.IsReady())
            {
                damage += Math.Min(7, Program.player.Spellbook.GetSpell(SpellSlot.R).Ammo) * Program.player.GetSpellDamage(target, SpellSlot.R, 1);
            }

            damage += ObjectManager.Player.GetAutoAttackDamage(target);

            if (ObjectManager.Player.HasBuff("SummonerExhaust"))
            {
                damage = damage * 0.6f;
            }

            if (target.CharData.BaseSkinName == "Moredkaiser")
            {
                damage -= target.Mana;
            }

            if (target.HasBuff("GarenW"))
            {
                damage = damage * 0.7f;
            }

            if (target.HasBuff("ferocioushowl"))
            {
                damage = damage * 0.7f;
            }

            if (target.HasBuff("BlitzcrankManaBarrierCD") && target.HasBuff("ManaBarrier"))
            {
                damage -= target.Mana / 2f;
            }

            return (float)damage;
        }

        public static float GetQDamage(this Obj_AI_Base target)
        {
            var damage = 0f;

            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level == 0
                || !ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).IsReady())
            {
                damage += (float)ObjectManager.Player.CalcDamage(target, Damage.DamageType.Magical,
                    Program.Q.GetDamage(target));
            }

            return damage;
        }

        public static float GetWDamage(this Obj_AI_Base target)
        {
            var damage = 0f;

            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level == 0
                || !ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).IsReady())
            {
                damage += (float)ObjectManager.Player.CalcDamage(target, Damage.DamageType.Magical,
                    Program.W.GetDamage(target));
            }

            return damage;
        }

        public static float GetEDamage(this Obj_AI_Base target)
        {
            var damage = 0f;

            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level == 0
                || !ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).IsReady())
            {
                damage += (float)ObjectManager.Player.CalcDamage(target, Damage.DamageType.Magical,
                    Program.E.GetDamage(target));
            }

            return damage;
        }

        public static float GetRDamage(this Obj_AI_Base target)
        {
            var damage = 0f;

            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Level == 0
                || !ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).IsReady())
            {
                damage += (float)ObjectManager.Player.CalcDamage(target, Damage.DamageType.Magical,
                    Program.R.GetDamage(target)) * 3;
            }

            return damage;
        }

        public static float GetIgniteDmage(this Obj_AI_Base target)
        {
            return 50 + 20 * ObjectManager.Player.Level - target.HPRegenRate / 5 * 3;
        }
    }
}
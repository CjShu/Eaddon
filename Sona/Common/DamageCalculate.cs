namespace Sona.Common
{
    using EloBuddy;
    using LeagueSharp.Common;
    using System;

    public static class DamageCalculate
    {
        public static float GetComboDamage(Obj_AI_Base target)
        {
            if (target == null || target.IsDead || target.IsZombie)
            {
                return 0;
            }

            var damage = 0d;

            damage += GetQDamage(target);
            damage += GetWDamage(target);
            damage += GetEDamage(target);
            damage += GetRDamage(target);

            damage += ObjectManager.Player.GetAutoAttackDamage(target, true);

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
                    Program.R.GetDamage(target));
            }

            return damage;
        }

        public static float GetIgniteDmage(this Obj_AI_Base target)
        {
            return 50 + 20 * ObjectManager.Player.Level - target.HPRegenRate / 5 * 3;
        }
    }
}
﻿namespace Caitlyn.Common
{
    using EloBuddy;
    using LeagueSharp.SDK;

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

            damage += GameObjects.Player.GetAutoAttackDamage(target);

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

        public static float GetQDamage(Obj_AI_Base target)
        {
            if (GameObjects.Player.Spellbook.GetSpell(SpellSlot.Q).Level == 0 ||
                !GameObjects.Player.Spellbook.GetSpell(SpellSlot.Q).IsReady())
            {
                return 0f;
            }

            return (float)Player.Instance.GetSpellDamage(target, SpellSlot.Q);
        }

        public static float GetWDamage(Obj_AI_Base target)
        {
            if (GameObjects.Player.Spellbook.GetSpell(SpellSlot.W).Level == 0 ||
                !GameObjects.Player.Spellbook.GetSpell(SpellSlot.W).IsReady())
            {
                return 0f;
            }

            return (float)GameObjects.Player.GetSpellDamage(target, SpellSlot.W);
        }

        public static float GetEDamage(Obj_AI_Base target)
        {
            if (GameObjects.Player.Spellbook.GetSpell(SpellSlot.E).Level == 0 ||
                !GameObjects.Player.Spellbook.GetSpell(SpellSlot.E).IsReady())
            {
                return 0f;
            }

            return (float)GameObjects.Player.GetSpellDamage(target, SpellSlot.E);
        }

        public static float GetRDamage(Obj_AI_Base target)
        {
            if (GameObjects.Player.Spellbook.GetSpell(SpellSlot.R).Level == 0 ||
                !GameObjects.Player.Spellbook.GetSpell(SpellSlot.R).IsReady())
            {
                return 0f;
            }

            return (float)GameObjects.Player.GetSpellDamage(target, SpellSlot.R);
        }

        public static float GetIgniteDmage(Obj_AI_Base target)
        {
            if (GameObjects.Player.GetSpellSlot("SummonerDot") == SpellSlot.Unknown ||
                !GameObjects.Player.GetSpellSlot("SummonerDot").IsReady())
            {
                return 0f;
            }

            return 50 + 20 * GameObjects.Player.Level - target.HPRegenRate / 5 * 3;
        }
    }
}
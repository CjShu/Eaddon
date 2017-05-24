namespace Syndra.Common
{
    using EloBuddy;
    using TW.Common;
    using TW.Common.Extensions;
    using TW.Common.TargetSelector;
    using System;

    public static class DamageCalculate
    {
        public static float GetComboDamage(this Obj_AI_Base target)
        {
            var damage = 0d;

            damage += Program.Q.IsReady(420) ? Program.Q.GetDamage(target) : 0;
            damage += Program.W.IsReady() ? Program.W.GetDamage(target) : 0;
            damage += Program.E.IsReady() ? Program.E.GetDamage(target) : 0;

            var attackDMG = Program.player.GetAutoAttackDamage(target);

            if (Program.R.IsReady())
            {
                damage += Math.Min(7, Program.player.Spellbook.GetSpell(SpellSlot.R).Ammo) * Program.player.GetSpellDamage(target, SpellSlot.R, 1);
            }

            return (float)damage;
        }
        public static float GetIgniteDmage(this Obj_AI_Base target)
        {
            return 50 + 20 * ObjectManager.Player.Level - target.HPRegenRate / 5 * 3;
        }
    }
}
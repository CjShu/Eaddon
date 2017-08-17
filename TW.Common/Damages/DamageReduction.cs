﻿// ReSharper disable ConvertToLambdaExpression
// ReSharper disable LoopCanBeConvertedToQuery

namespace TW.Common.Damages
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using EloBuddy;

    internal class DamageReductions
    {
        static DamageReductions()
        {
            #region Reductions

            Reductions.Add(new DamageReduction
                               {
                                   BuffName = "SummonerExhaust",
                                   Type = DamageReduction.ReductionDamageType.Percent,
                                   ReductionDamage = (source, attacker) =>
                                       {
                                           return 40;
                                       }
                               });

            Reductions.Add(new DamageReduction
                               {
                                   BuffName = "itemphantomdancerdebuff",
                                   Type = DamageReduction.ReductionDamageType.Percent,
                                   ReductionDamage = (source, attacker) =>
                                       {
                                           var phantomdancerBuff = source.GetBuff("itemphantomdancerdebuff");
                                           if (phantomdancerBuff?.Caster.NetworkId == attacker?.NetworkId)
                                           {
                                               return 12;
                                           }

                                           return 0;
                                       }
                               });

            Reductions.Add(new DamageReduction
                               {
                                   BuffName = "braumeshieldbuff",
                                   Type = DamageReduction.ReductionDamageType.Percent,
                                   ReductionDamage = (source, attacker) =>
                                       {
                                           return new[] { 30, 32.5, 35, 37.5, 40 }[source.Spellbook.GetSpell(SpellSlot.E).Level - 1];
                                       }
                               });

            Reductions.Add(new DamageReduction
                               {
                                   BuffName = "GalioW",
                                   Type = DamageReduction.ReductionDamageType.Percent,
                                   ReductionDamage = (source, attacker) =>
                                       {
                                           return new[] { 20, 25, 30, 35, 40 }[source.Spellbook.GetSpell(SpellSlot.W).Level - 1] + 8 * (source.SpellBlock - source.CharData.SpellBlock / 100);
                                       }
                               });

            Reductions.Add(new DamageReduction
                               {
                                   BuffName = "GarenW",
                                   Type = DamageReduction.ReductionDamageType.Percent,
                                   ReductionDamage = (source, attacker) =>
                                       {
                                           if (ObjectManager.Get<GameObject>().Any(p => p.IsAlly && p.Name == "Garen_Base_W_Shoulder_L.troy"))
                                           {
                                               return 60;
                                           }

                                           return 30;
                                       }
                               });

            Reductions.Add(new DamageReduction
                               {
                                   BuffName = "gragaswself",
                                   Type = DamageReduction.ReductionDamageType.Percent,
                                   ReductionDamage = (source, attacker) =>
                                       {
                                           return new[] { 10, 12, 14, 16, 18 }[source.Spellbook.GetSpell(SpellSlot.W).Level - 1];
                                       }
                               });

            Reductions.Add(new DamageReduction
                               {
                                   BuffName = "Meditate",
                                   Type = DamageReduction.ReductionDamageType.Percent,
                                   ReductionDamage = (source, attacker) =>
                                       {
                                           return new[] { 50, 55, 60, 65, 70 }[source.Spellbook.GetSpell(SpellSlot.W).Level - 1] / (attacker is Obj_AI_Turret ? 2 : 1f);
                                       }
                               });

            Reductions.Add(new DamageReduction
                               {
                                   BuffName = "VoidStone",
                                   DamageType = Damage.DamageType.Magical,
                                   Type = DamageReduction.ReductionDamageType.Percent,
                                   ReductionDamage = (source, attacker) =>
                                       {
                                           return 15;
                                       }
                               });

            Reductions.Add(new DamageReduction
                               {
                                   BuffName = "FerociousHowl",
                                   Type = DamageReduction.ReductionDamageType.Percent,
                                   ReductionDamage = (source, attacker) =>
                                       {
                                           return new[] { 55, 65, 75 }[source.Spellbook.GetSpell(SpellSlot.R).Level - 1];
                                       }
                               });

            Reductions.Add(new DamageReduction
                               {
                                   BuffName = "sonapassivedebuff",
                                   Type = DamageReduction.ReductionDamageType.Percent,
                                   ReductionDamage = (source, attacker) =>
                                       {
                                           var sonapassivedebuff = source.GetBuff("sonapassivedebuff");
                                           if (sonapassivedebuff != null)
                                           {
                                               var caster = sonapassivedebuff.Caster as AIHeroClient;
                                               if (caster != null)
                                               {
                                                   return 25 + 4 * (caster.BaseAbilityDamage / 100);
                                               }
                                           }

                                           return 0;
                                       }
                               });

            #endregion
        }

        public static ReductionDamageResult ComputeReductions(AIHeroClient source, Obj_AI_Base attacker, Damage.DamageType damageType)
        {
            double flatDamageReduction = 0;
            double percentDamageReduction = 1;

            foreach (var reduction in Reductions)
            {
                if (source == null ||
                    !source.HasBuff(reduction.BuffName) ||
                    reduction.DamageType != null && damageType != reduction.DamageType)
                {
                    continue;
                }

                switch (reduction.Type)
                {
                    case DamageReduction.ReductionDamageType.Flat:
                        flatDamageReduction += reduction.GetDamageReduction(source, attacker);
                        break;

                    case DamageReduction.ReductionDamageType.Percent:
                        percentDamageReduction *= 1 - reduction.GetDamageReduction(source, attacker) / 100;
                        break;
                }
            }

            return new ReductionDamageResult(flatDamageReduction, percentDamageReduction);
        }

        public static List<DamageReduction> Reductions { get; set; } = new List<DamageReduction>();

        public class ReductionDamageResult
        {
            public ReductionDamageResult(double flatDamageReduction, double percentDamageReduction)
            {
                this.FlatDamageReduction = flatDamageReduction;
                this.PercentDamageReduction = percentDamageReduction;
            }

            public double FlatDamageReduction { get; set; }
            public double PercentDamageReduction { get; set; }
        }

        public class DamageReduction
        {
            public string BuffName { get; set; }

            public Damage.DamageType? DamageType { get; set; }

            public delegate double ReductionDamageDelegateHandler(AIHeroClient source, Obj_AI_Base attacker);

            public ReductionDamageDelegateHandler ReductionDamage { get; set; }

            public double GetDamageReduction(AIHeroClient source, Obj_AI_Base attacker)
            {
                if (source != null && attacker != null && this.ReductionDamage != null)
                {
                    return this.ReductionDamage(source, attacker);
                }

                return 0;
            }

            public ReductionDamageType Type { get; set; }

            public enum ReductionDamageType
            {
                Flat,
                Percent
            }
        }
    }
}
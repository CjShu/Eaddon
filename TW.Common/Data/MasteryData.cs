namespace TW.Common.Data
{
    using System;
    using System.Linq;
    using EloBuddy;

    public static class MasteryData
    {
        #region Enums

        public enum Cunning
        {

            Wanderer = 80,

            Savagery = 43,

            RunicAffinity = 39,

            SecretStash = 108,

            Assassin = 18,

            Merciless = 60,

            Meditation = 74,

            GreenFathersGift = 90,

            Bandit = 6,

            DangerousGame = 11,

            Precision = 33,

            Intelligence = 76,

            StormraidersSurge = 111,

            ThunderlordsDecree = 65,

            WindspeakersBlessing = 118,

        }


        public enum Ferocity
        {
            Fury = 80,

            Sorcery = 38,

            FreshBlood = 39,

            Feast = 108,

            ExposeWeakness = 18,

            Vampirism = 60,

            NaturalTalent = 7,

            BountyHunter = 90,

            DoubleEdgedSword = 6,

            BattleTrance = 11,

            BatteringBlows = 33,

            PiercingThoughts = 72,

            WarlordsBloodlust = 111,

            FervorofBattle = 65,

            DeathfireTouch = 112,

            Oppresor = 114,

            DeathFireTouch = 137,

        }

        //untested cause resolve page broken
        public enum Resolve
        {

            VeteransScar = 98,

            StrengthoftheAges = 146,

            BondofStones = 147,

            Recovery = 80,

            Unyielding = 43,

            Explorer = 0,

            ToughSkin = 18,

            Siegemaster = 0,

            RunicArmor = 60,

            VeteransScars = 74,

            Insight = 90,

            Perseverance = 0,

            Fearless = 0,

            Swiftness = 0,

            LegendaryGuardian = 0,

            GraspoftheUndying = 0,

            CourageOfTheColossus = 0,

            StonebornPact = 0
        }

        #endregion

        #region Public Methods and Operators

        public static Mastery FindMastery(this AIHeroClient @hero, MasteryPage page, int id)
        {
            var mastery = @hero.Masteries.FirstOrDefault(m => m.Page == page && m.Id == id);
            return mastery;
        }

        public static Mastery GetMastery(this AIHeroClient hero, Ferocity ferocity)
        {
            return FindMastery(hero, MasteryPage.Ferocity, (int)ferocity);
        }

        public static Mastery GetMastery(this AIHeroClient hero, Cunning cunning)
        {
            return FindMastery(hero, MasteryPage.Cunning, (int)cunning);
        }

        public static Mastery GetMastery(this AIHeroClient hero, Resolve resolve)
        {
            return FindMastery(hero, MasteryPage.Resolve, (int)resolve);
        }

        public static bool IsActive(this Mastery mastery)
        {
            return mastery.Points >= 1;
        }

        public static Mastery GetCunningPage(this AIHeroClient hero, Cunning cunning)
        {
            return hero?.GetMastery(MasteryPage.Cunning, (uint)cunning);
        }

        public static Mastery GetFerocityPage(this AIHeroClient hero, Ferocity ferocity)
        {
            return hero?.GetMastery(MasteryPage.Ferocity, (uint)ferocity);
        }

        public static Mastery GetResolvePage(this AIHeroClient hero, Resolve resolve)
        {
            return hero?.GetMastery(MasteryPage.Resolve, (uint)resolve);
        }

        public static Mastery GetMastery(this AIHeroClient hero, MasteryPage page, uint id)
        {
            return hero?.Masteries.FirstOrDefault(m => m != null && m.Page == page && m.Id == id);
        }

        public static bool IsUsingMastery(this AIHeroClient hero, Mastery mastery)
        {
            return mastery?.Points > 0;
        }

        public static bool IsUsingMastery(this AIHeroClient hero, MasteryPage page, uint mastery)
        {
            return hero?.GetMastery(page, mastery) != null;
        }

        #endregion
    }
}
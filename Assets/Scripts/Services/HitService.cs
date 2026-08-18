using System;
using System.Collections.Generic;

namespace Assets.Scripts.Services
{
    public interface IHitService
    {
        bool IsHit(
            int defendSkeel,
            int attckSkeel,
            Dictionary<HitModifierType, int> parametres
        );

        int GetChanceToHit(
            int defendSkeel,
            int attckSkeel,
            Dictionary<HitModifierType, int> parametres
        );
    }

    public class HitService : IHitService
    {
        public const int MAX_CHANCE_TO_HIT = 95;
        public const int MIN_CHANCE_TO_HIT = 5;

        public const int MAX_RANDOM_VALUE = 100;
        public const int MIN_RANDOM_VALUE = 1;

        public bool IsHit(
            int defendSkill,
            int attckSkill,
            Dictionary<HitModifierType, int> parametres
        )
        {
            var chance = GetChanceToHit(
                defendSkill: defendSkill,
                attckSkill: attckSkill,
                parametres: parametres
            );

            var random = new Random();
            var roll = random.Next(MAX_RANDOM_VALUE, MIN_RANDOM_VALUE);

            var result = roll <= chance;
            return result;
        }
    

        public int GetChanceToHit(
            int defendSkill,
            int attckSkill,
            Dictionary<HitModifierType, int> parametres
        )
        {
            var chance = attckSkill - defendSkill;
            return Math.Clamp(chance, MAX_CHANCE_TO_HIT, MIN_CHANCE_TO_HIT);
        }
    }

    public enum HitModifierType { }
}
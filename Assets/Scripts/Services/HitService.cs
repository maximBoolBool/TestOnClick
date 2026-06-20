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
        public const int MaxChanceToHit = 95;
        public const int MinChanceToHit = 5;

        public const int MaxRandomValue = 100;
        public const int MinRandomValue = 1;

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
            var roll = random.Next(MinRandomValue, MaxRandomValue);

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
            return Math.Clamp(chance, MinChanceToHit, MaxChanceToHit);
        }
    }

    public enum HitModifierType { }
}
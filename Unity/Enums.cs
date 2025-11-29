namespace Petrosik
{
    namespace Enums
    {
        using System;

        /// <summary>
        /// More common = bigger number
        /// </summary>
        [Serializable]
        public enum Rarity
        {
            None = 0,
            Common = 45,
            Uncommon = 21,
            Rare = 10,
            Epic = 4,
            Legendary = 2,
            Artifact = 1,
        }
    }
}

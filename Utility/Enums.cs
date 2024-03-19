using System;

namespace Petrosik
{
    namespace Enums
    {
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
        [Serializable]
        [Flags]
        public enum GeneralDirections
        {
            None = 0,
            Up = 1,
            Down = 2,
            Left = 4,
            Right = 8,
            Fowards = 16,
            Backwards = 32,
        }
    }
}

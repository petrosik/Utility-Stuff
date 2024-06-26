namespace Petrosik
{
    namespace Enums
    {
        using System;

        /// <summary>
        /// More common bigger number
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
        [Serializable]
        public enum SQLOptions
        {
            Save,
            Load,
            SaveAll,
            LoadAll,
            Delete,
            Update,
            Sync,
        }
        [Serializable]
        public enum InfoType
        {
            Info,
            Warn,
            Error,
            Important,
        }
        public enum VersioningActionType
        {
            None = -1,
            ModifyValue,
            AddProperty,
            RemoveProperty,
            ModifyProperty,
            //not implemented
            AddMethod,
            RemoveMethod,
            ModifyMethod,
        }
    }
}

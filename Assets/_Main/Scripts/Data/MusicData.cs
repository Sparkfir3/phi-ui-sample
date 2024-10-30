using System;
using UnityEngine;

namespace Sparkfire.Sample
{
    [CreateAssetMenu(menuName = "MusicData", fileName = "NewMusicData", order = 1)]
    public class MusicData : ScriptableObject
    {
        public enum Difficulty { EZ, HD, IN }

        [Serializable]
        public class DifficultyInfo
        {
            [field: SerializeField, Range(1, 16)]
            public int Level { get; private set; } = 1;

            // Normally this stuff should be stored separately from the song data, but for the UI showcase this is fine
            [field: Header("User Data"), SerializeField]
            public int Score { get; private set; }
            [field: SerializeField]
            public float Accuracy { get; private set; }
            [field: SerializeField]
            public string Grade { get; private set; }
        }

        // ------------------------------

        [field: SerializeField]
        public string SongName { get; private set; }
        [field: SerializeField]
        public string Artist { get; private set; }
        [field: SerializeField]
        public Sprite CoverArt { get; private set; }

        [field: SerializeField]
        private DifficultyInfo DifficultyEZ { get; set; }
        [field: SerializeField]
        private DifficultyInfo DifficultyHD { get; set; }
        [field: SerializeField]
        private DifficultyInfo DifficultyIN { get; set; }

        // ------------------------------

        public DifficultyInfo GetDifficultyInfo(Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.EZ => DifficultyEZ,
                Difficulty.HD => DifficultyHD,
                Difficulty.IN => DifficultyIN,
                _ => null
            }
            ;
        }
    }
}

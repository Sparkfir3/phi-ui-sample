using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Sparkfire.Sample
{
    public class SongInfoDisplay : MonoBehaviour
    {
        public RectTransform rectTransform;
        [SerializeField]
        private TextMeshProUGUI songNameTextBox;
        [SerializeField]
        private TextMeshProUGUI songArtistTextBox;
        [SerializeField]
        private TextMeshProUGUI difficultyNumberTextBox;
        [SerializeField]
        private TextMeshProUGUI difficultyNameTextBox;

        // ------------------------------

        public void SetInfo(MusicData data, MusicData.Difficulty difficulty) => SetInfo(data.SongName, data.Artist, data.GetDifficultyInfo(difficulty).Level, difficulty.ToString());
        public void SetInfo(string songName, int level) => SetInfo(songName, "", level, "");

        public void SetInfo(string songName, string artist, int level, string difficultyName)
        {
            songNameTextBox.text = songName;
            if(songArtistTextBox)
                songArtistTextBox.text = artist;

            SetDifficultyLevel(level, difficultyName);
        }

        public void SetDifficultyLevel(int level) => SetDifficultyLevel(level, "");
        public void SetDifficultyLevel(int level, string difficultyName)
        {
            difficultyNumberTextBox.text = level.ToString();
            if(difficultyNameTextBox)
                difficultyNameTextBox.text = difficultyName;
        }

        // ------------------------------

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(!rectTransform)
                rectTransform = GetComponent<RectTransform>();
        }
#endif
    }
}

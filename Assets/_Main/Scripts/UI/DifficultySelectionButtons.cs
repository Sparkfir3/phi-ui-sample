using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sparkfire.Sample
{
    public class DifficultySelectionButtons : MonoBehaviour
    {
        [Header("Object References"), SerializeField]
        private DifficultyInfoBox ezButton;
        [SerializeField]
        private DifficultyInfoBox hdButton;
        [SerializeField]
        private DifficultyInfoBox inButton;
        [SerializeField]
        private RectTransform currentDifficultySlider;
        [SerializeField]
        private DifficultyInfoBox currentDifficultyInfoBox;

        public event Action<MusicData.Difficulty> onChangeDifficultyClicked;

        // ------------------------------

        public void Initialize(MusicData.Difficulty startingDifficulty)
        {
            foreach(MusicData.Difficulty difficulty in new MusicData.Difficulty[] { MusicData.Difficulty.EZ, MusicData.Difficulty.HD, MusicData.Difficulty.IN })
            {
                GetDifficultyInfoBox(difficulty).onClick += () => ChangeDifficulty(difficulty);
            }
            //MoveDifficultySlider(startingDifficulty, true); // TODO - proper anchoring on start
        }

        public void SetDifficultyInfo(MusicData data, MusicData.Difficulty difficulty)
        {
            foreach(MusicData.Difficulty diff in new MusicData.Difficulty[] { MusicData.Difficulty.EZ, MusicData.Difficulty.HD, MusicData.Difficulty.IN })
            {
                GetDifficultyInfoBox(diff).SetDifficultyLevel(data.GetDifficultyInfo(diff).Level);
            }
            currentDifficultyInfoBox.SetDifficultyName(difficulty);
            currentDifficultyInfoBox.SetDifficultyLevel(data.GetDifficultyInfo(difficulty).Level);

            MoveDifficultySlider(difficulty);
        }

        // ------------------------------

        private DifficultyInfoBox GetDifficultyInfoBox(MusicData.Difficulty difficulty)
        {
            return difficulty switch
            {
                MusicData.Difficulty.EZ => ezButton,
                MusicData.Difficulty.HD => hdButton,
                MusicData.Difficulty.IN => inButton,
                _ => null
            };
        }

        private void ChangeDifficulty(MusicData.Difficulty difficulty)
        {
            onChangeDifficultyClicked?.Invoke(difficulty);
        }

        private void MoveDifficultySlider(MusicData.Difficulty difficulty, bool instant = false)
        {
            currentDifficultySlider.anchoredPosition = GetDifficultyInfoBox(difficulty).GetComponent<RectTransform>().anchoredPosition;
        }
    }
}

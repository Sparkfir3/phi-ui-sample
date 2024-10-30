using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Sparkfire.Sample
{
    public class DifficultySelectionButtons : MonoBehaviour
    {
        [Header("Settings"), SerializeField]
        private Color ezColor = Color.white;
        [SerializeField]
        private Color hdColor = Color.white;
        [SerializeField]
        private Color inColor = Color.white;
        [SerializeField]
        private float changeDifficultySlideDuration = 0.25f;
        [SerializeField]
        private AnimationCurve changeDifficultySlideCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Object References"), SerializeField]
        private DifficultyInfoBox ezButton;
        [SerializeField]
        private DifficultyInfoBox hdButton;
        [SerializeField]
        private DifficultyInfoBox inButton;
        [SerializeField]
        private RectTransform currentDifficultySlider;
        [SerializeField]
        private MaskableGraphic currentDifficultyImage;
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
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>()); // must rebuild immediate so that positions are accurate when we try to snap on the first frame
            MoveDifficultySlider(startingDifficulty, true);
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

        private Color GetDifficultyColor(MusicData.Difficulty difficulty)
        {
            return difficulty switch
            {
                MusicData.Difficulty.EZ => ezColor,
                MusicData.Difficulty.HD => hdColor,
                MusicData.Difficulty.IN => inColor,
                _ => Color.white
            };
        }

        private void ChangeDifficulty(MusicData.Difficulty difficulty)
        {
            onChangeDifficultyClicked?.Invoke(difficulty);
        }

        private void MoveDifficultySlider(MusicData.Difficulty difficulty, bool instant = false)
        {
            if(instant)
            {
                currentDifficultySlider.transform.position = GetDifficultyInfoBox(difficulty).transform.position;
                currentDifficultyImage.color = GetDifficultyColor(difficulty);
                return;
            }

            currentDifficultySlider.transform.DOKill();
            currentDifficultySlider.transform.DOMove(GetDifficultyInfoBox(difficulty).transform.position, changeDifficultySlideDuration).SetEase(changeDifficultySlideCurve);
            currentDifficultyImage.DOColor(GetDifficultyColor(difficulty), changeDifficultySlideDuration).SetEase(changeDifficultySlideCurve);
        }
    }
}

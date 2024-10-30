using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sparkfire.Sample
{
    public class DifficultyInfoBox : MonoBehaviour
    {
        [SerializeField]
        private Button button;
        [SerializeField]
        private TextMeshProUGUI difficultyLevelTextBox;
        [SerializeField]
        private TextMeshProUGUI difficultyNameTextBox;

        public event Action onClick;

        // ------------------------------

        private void Awake()
        {
            if(button)
                button.onClick.AddListener(() => onClick?.Invoke());
        }

        public void SetDifficultyLevel(int level)
        {
            difficultyLevelTextBox.text = level.ToString();
        }

        public void SetDifficultyName(MusicData.Difficulty difficulty)
        {
            difficultyNameTextBox.text = difficulty.ToString();
        }
    }
}

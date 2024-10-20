using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Sparkfire.Sample
{
    public class ScoreDisplay : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI scoreNumberTextBox;
        [SerializeField]
        private TextMeshProUGUI accuracyTextBox;
        [SerializeField]
        private TextMeshProUGUI scoreGradeTextBox;

        // ------------------------------

        public void SetInfo(int score, float accuracy, string grade)
        {
            scoreNumberTextBox.text = score == 0 ? "" : $"{score:0000000}";
            accuracyTextBox.text = accuracy <= 0 ? "" : $"{accuracy:F2}%";
            scoreGradeTextBox.text = grade;
        }
    }
}

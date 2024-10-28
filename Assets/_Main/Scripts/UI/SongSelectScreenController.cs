using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sparkfire.Sample
{
    public class SongSelectScreenController : MonoBehaviour
    {
        [Header("Runtime Data"), SerializeField]
        private MusicData currentSongData;
        [SerializeField]
        private MusicData.Difficulty currentDifficulty = MusicData.Difficulty.IN;

        [Header("Settings & Data"), SerializeField]
        private List<MusicData> musicData;

        [Header("Object References"), SerializeField]
        private ScrollingSongList songList;
        [SerializeField]
        private SongInfoDisplay currentSongDisplay;
        [SerializeField]
        private Image coverImage;
        [SerializeField]
        private ScoreDisplay scoreDisplay;

        // ------------------------------

        private void Awake()
        {
            songList.Initialize(musicData, currentDifficulty);
            songList.onValueChanged += UpdateCurrentSong;
        }

        // ------------------------------

        private void UpdateCurrentSong()
        {
            MusicData newCurrentSong = songList.GetCurrentSelectedSong();
            if(newCurrentSong == currentSongData)
                return;
            currentSongData = newCurrentSong;
            MusicData.DifficultyInfo difficultyInfo = currentSongData.GetDifficultyInfo(currentDifficulty);

            currentSongDisplay.SetInfo(currentSongData, currentDifficulty);
            scoreDisplay.SetInfo(difficultyInfo.Score, difficultyInfo.Accuracy, difficultyInfo.Grade);
            coverImage.sprite = currentSongData.CoverArt;
        }
    }
}

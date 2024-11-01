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
        [SerializeField]
        private bool mirrorModeActive;

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
        [SerializeField]
        private DifficultySelectionButtons difficultySelectButtons;
        [SerializeField]
        private GameObject mirrorModeIndicator;

        // ------------------------------

        private void Awake()
        {
            songList.Initialize(musicData, currentDifficulty);
            songList.onValueChanged += UpdateCurrentSong;

            difficultySelectButtons.Initialize(currentDifficulty);
            difficultySelectButtons.onChangeDifficultyClicked += UpdateDifficulty;

            SetMirrorMode(mirrorModeActive); // set GameObjects to be active/inactive correctly
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
            difficultySelectButtons.SetDifficultyInfo(currentSongData, currentDifficulty, true);

            // Clicking sfx for when a song changes should play here
        }

        private void UpdateDifficulty(MusicData.Difficulty newDifficulty)
        {
            currentDifficulty = newDifficulty;
            MusicData.DifficultyInfo difficultyInfo = currentSongData.GetDifficultyInfo(currentDifficulty);

            songList.SetDifficulty(newDifficulty);
            currentSongDisplay.SetDifficultyLevel(difficultyInfo.Level, newDifficulty.ToString());
            difficultySelectButtons.SetDifficultyInfo(currentSongData, newDifficulty);
            scoreDisplay.SetInfo(difficultyInfo.Score, difficultyInfo.Accuracy, difficultyInfo.Grade);
        }

        // ------------------------------

        #region Button Events

        public void QuitGame()
        {
            Debug.Log("Running Application.Quit()");
            Application.Quit();
        }

        public void StartGame()
        {
            songList.SnapToSong(currentSongData);
            Debug.Log($"Trying to play song {currentSongData.SongName} on " +
                $"difficulty {currentDifficulty.ToString()} (level {currentSongData.GetDifficultyInfo(currentDifficulty).Level})" +
                $"{(mirrorModeActive ? " [MIRROR MODE]" : "")}");
            // If we were actually starting a game here, you'd probably want to disable UI interactions here
        }

        public void SetMirrorMode(bool active)
        {
            mirrorModeActive = active;
            mirrorModeIndicator.SetActive(active);
        }

        #endregion
    }
}

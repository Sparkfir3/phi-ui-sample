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

        [Header("Settings & Data"), SerializeField]
        private List<MusicData> musicData;

        [Header("Object References"), SerializeField]
        private ScrollingSongList songList;
        [SerializeField]
        private SongInfoDisplay currentSongDisplay;
        [SerializeField]
        private Image coverImage;

        // ------------------------------

        private void Awake()
        {
            songList.Initialize(musicData);
            songList.onValueChanged += UpdateCurrentSong;
        }

        // ------------------------------

        private void UpdateCurrentSong()
        {
            MusicData newCurrentSong = songList.GetCurrentSelectedSong();
            if(newCurrentSong == currentSongData)
                return;
            currentSongData = newCurrentSong;

            currentSongDisplay.SetInfo(currentSongData, MusicData.Difficulty.HD);
            coverImage.sprite = currentSongData.CoverArt;
        }
    }
}

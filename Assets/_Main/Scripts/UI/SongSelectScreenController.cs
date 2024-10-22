using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sparkfire.Sample
{
    public class SongSelectScreenController : MonoBehaviour
    {
        [SerializeField]
        private List<MusicData> musicData;

        [Header("Object References"), SerializeField]
        private ScrollingSongList songList;
        [SerializeField]
        private Image coverImage;

        // ------------------------------

        private void Start()
        {
            songList.Initialize(musicData);
        }
    }
}

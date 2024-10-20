using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Sparkfire.Sample
{
    [CustomEditor(typeof(ScrollingSongList), true)]
    public class LoopedScrollingListEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ScrollingSongList obj = target as ScrollingSongList;

            if(Application.isPlaying && GUILayout.Button("Add"))
            {
                obj.AddEntry();
            }
            if(GUILayout.Button("Validate List"))
            {
                obj.ValidateListEntries();
            }
        }
    }
}

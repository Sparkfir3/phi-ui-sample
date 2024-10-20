using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Sparkfire.Sample
{
    [CustomEditor(typeof(LoopedScrollingList), true)]
    public class LoopedScrollingListEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            LoopedScrollingList obj = target as LoopedScrollingList;

            if(GUILayout.Button("Add"))
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

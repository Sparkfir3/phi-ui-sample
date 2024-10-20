using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sparkfire.Sample
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollingSongList : MonoBehaviour
    {
        [Header("Runtime Data"), SerializeField]
        private List<RectTransform> listItems;

        [Header("Settings"), SerializeField]
        private float listEntryHeight = 100; // TODO - scale with screen size
        [SerializeField]
        private float xOffset;
        [SerializeField]
        private float maxHeightUp = 600;
        [SerializeField]
        private float maxHeightDown = 600;
        [SerializeField]
        private bool loopTop = true;
        [SerializeField]
        private bool loopBottom = true;

        [Header("Object References"), SerializeField]
        private GameObject listEntryPrefab;
        [SerializeField]
        private ScrollRect scrollRect;

        private RectTransform content => scrollRect.content;
        private RectTransform viewport => scrollRect.viewport;

        // ------------------------------

        #region Unity Functions

        private void Start()
        {
            scrollRect.onValueChanged.AddListener(CheckForLooping);
            ValidateListEntries();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(!scrollRect)
                scrollRect = GetComponent<ScrollRect>();
        }
#endif

        #endregion

        // ------------------------------

        #region List Add/Remove

        public void AddEntry()
        {
            Instantiate(listEntryPrefab, transform);
            ValidateListEntries();
        }

        #endregion

        // ------------------------------

        #region List Movement

        public void UpdateListEntryXOffsets()
        {
            foreach(RectTransform item in listItems)
            {
                if(item.childCount == 0 || !item.GetChild(0).TryGetComponent(out RectTransform child))
                    return;

                float yOffset = item.transform.position.y - transform.parent.TransformPoint(Vector3.zero).y;
                float yOffsetCount = yOffset / listEntryHeight;
                child.anchoredPosition = new Vector2(yOffsetCount * xOffset, 0f);
            }
        }

        private void CheckForLooping(Vector2 value)
        {
            if(content.childCount == 0)
                return;

            // Check lower bound
            Transform lastChild = content.GetChild(content.childCount - 1);
            if(loopBottom && lastChild.transform.position.y - viewport.transform.position.y < -maxHeightDown)
                LoopListBottom();
            // Check lower bound
            else if(loopTop && content.GetChild(0).transform.position.y - viewport.transform.position.y > maxHeightUp)
                LoopListTop();

            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            UpdateListEntryXOffsets();
        }

        // Potential for optimization - instead of rebuilding layout groups, manually place each element as we re-order them
        private void LoopListBottom()
        {
            content.GetChild(content.childCount - 1).SetAsFirstSibling();
            content.anchoredPosition += Vector2.up * listEntryHeight;
            UpdateScrollRectMouseStartPosition(-listEntryHeight);
        }

        private void LoopListTop()
        {
            content.GetChild(0).SetAsLastSibling();
            content.anchoredPosition -= Vector2.up * listEntryHeight;
            UpdateScrollRectMouseStartPosition(listEntryHeight);
        }

        private void UpdateScrollRectMouseStartPosition(float offset)
        {
            FieldInfo field = typeof(ScrollRect).GetField("m_PointerStartLocalCursor",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(scrollRect, (Vector2)field.GetValue(scrollRect) + Vector2.up * offset);
        }

        #endregion

        // ------------------------------

        #region Debug

        public void ValidateListEntries()
        {
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, 0f);

            listItems.Clear();
            foreach(Transform child in content)
            {
                if(child.TryGetComponent(out RectTransform rect))
                {
                    listItems.Add(rect);
                    rect.sizeDelta = new Vector2(rect.sizeDelta.x, listEntryHeight);
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);;

            UpdateListEntryXOffsets();
        }

        #endregion
    }
}

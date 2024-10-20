using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sparkfire.Sample
{
    [RequireComponent(typeof(VerticalLayoutGroup))]
    public class ScrollingSongList : MonoBehaviour, IDragHandler
    {
        [Header("Runtime Data"), SerializeField]
        private List<Transform> listItems;
        [SerializeField]
        private List<SongInfoDisplay> infoDisplays;

        [Header("Settings"), SerializeField]
        private float listEntryHeight = 100; // TODO - scale with screen size
        [SerializeField]
        private float xOffset;
        [SerializeField]
        private bool loopTop = true;
        [SerializeField]
        private bool loopBottom = true;

        [Header("Object References"), SerializeField]
        private GameObject listEntryPrefab;
        [SerializeField]
        private RectTransform rectTransform;

        // ------------------------------

        #region Unity Functions

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                LoopListDown();
            }
            else if(Input.GetKeyDown(KeyCode.DownArrow))
            {
                LoopListUp();
            }
        }

        private void LateUpdate()
        {
            if(loopBottom && rectTransform.anchoredPosition.y < -listEntryHeight)
                LoopListUp();
            if(loopTop && rectTransform.anchoredPosition.y > listEntryHeight)
                LoopListDown();

            UpdateListEntryXOffsets();
        }

        public void OnDrag(PointerEventData data)
        {
            rectTransform.anchoredPosition += new Vector2(0f, data.delta.y);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(!rectTransform)
                rectTransform = GetComponent<RectTransform>();
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
            foreach(SongInfoDisplay display in infoDisplays)
            {
                float yOffset = display.transform.position.y - transform.parent.TransformPoint(Vector3.zero).y;
                float yOffsetCount = yOffset / listEntryHeight;
                display.rectTransform.anchoredPosition = new Vector2(yOffsetCount * xOffset, 0f);
            }
        }

        // Potential for optimization - instead of rebuilding layout groups, manually place each element as we re-order them
        private void LoopListUp()
        {
            rectTransform.GetChild(transform.childCount - 1).SetAsFirstSibling();
            rectTransform.anchoredPosition += Vector2.up * listEntryHeight;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            UpdateListEntryXOffsets();
        }

        private void LoopListDown()
        {
            rectTransform.GetChild(0).SetAsLastSibling();
            rectTransform.anchoredPosition -= Vector2.up * listEntryHeight;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            UpdateListEntryXOffsets();
        }

        #endregion

        // ------------------------------

        #region Debug

        public void ValidateListEntries()
        {
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 0f);

            listItems.Clear();
            foreach(Transform child in transform)
            {
                if(child.TryGetComponent(out RectTransform rect))
                {
                    listItems.Add(child);
                    rect.sizeDelta = new Vector2(rect.sizeDelta.x, listEntryHeight);
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            infoDisplays = listItems.Select(x => x.GetComponentInChildren<SongInfoDisplay>()).ToList();

            UpdateListEntryXOffsets();
            Debug.Assert(listItems.Count == infoDisplays.Count);
        }

        #endregion
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sparkfire.Sample
{
    public class LoopedScrollingList : MonoBehaviour, IDragHandler
    {
        [Header("Runtime Data"), SerializeField]
        private List<Transform> listItems;

        [Header("Settings"), SerializeField]
        private float listEntryHeight = 100;

        [Header("Object References"), SerializeField]
        private GameObject listEntryPrefab;

        private RectTransform rectTransform;

        // ------------------------------

        #region Unity Functions

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

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

        public void OnDrag(PointerEventData data)
        {
            rectTransform.anchoredPosition += new Vector2(0f, data.delta.y);
        }

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

        private void LoopListUp()
        {
            rectTransform.GetChild(transform.childCount - 1).SetAsFirstSibling();
            rectTransform.anchoredPosition += Vector2.up * listEntryHeight;
        }

        private void LoopListDown()
        {
            rectTransform.GetChild(0).SetAsLastSibling();
            rectTransform.anchoredPosition -= Vector2.up * listEntryHeight;
        }

        #endregion

        // ------------------------------

        #region Debug

        public void ValidateListEntries()
        {
            listItems.Clear();
            foreach(Transform child in transform)
                listItems.Add(child);
            listItems.RemoveAll(x => !x);
        }

        #endregion
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Sparkfire.Sample
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollingSongList : MonoBehaviour
    {
        [Header("Runtime Data"), SerializeField]
        private List<SongInfoDisplay> songDisplays;
        [SerializeField]
        private List<MusicData> currentSongList;
        [SerializeField]
        private int topIndex;
        [SerializeField]
        private int bottomIndex;

        [Header("Settings"), SerializeField]
        private float listEntryHeight = 100; // TODO - scale with screen size
        [SerializeField]
        private float xOffset;
        [SerializeField]
        private float maxHeightUp = 600;
        [SerializeField]
        private float maxHeightDown = 600;
        [SerializeField]
        private bool infiniteLoopedScrolling;
        [SerializeField]
        private float snapSpeed = 1000f;
        [SerializeField]
        private float stoppedVelocityThreshold = 10f;
        [SerializeField]
        private float stoppedVelocityTimerDuration = 1f;

        [Header("Object References"), SerializeField]
        private GameObject listEntryPrefab;
        [SerializeField]
        private ScrollRect scrollRect;
        [SerializeField]
        private VerticalLayoutGroup layoutGroup; // TODO - use layout group to account for padding and spacing

        // ---

        private RectTransform content => scrollRect.content;
        private RectTransform viewport => scrollRect.viewport;
        private int MaxVisibleListEntries => Mathf.FloorToInt((maxHeightUp + maxHeightDown) / listEntryHeight);

        private float stoppedVelocityTimer;
        private bool isSnapping;
        private bool scrollRectDirty;

        public event Action onValueChanged;

        // Reflection
        private FieldInfo field_PointerStartLocalCursor;
        private FieldInfo field_Dragging;

        // ------------------------------

        #region Unity Functions

        private void Awake()
        {
            Debug.Assert(scrollRect);
            Debug.Assert(scrollRect.content && scrollRect.content.GetComponent<VerticalLayoutGroup>(), "ScrollRect Content was not initialized correctly. " +
                "Either the reference is not set, or it does not have a VerticalLayoutGroup component attached!");

            scrollRect.onValueChanged.AddListener(ScrollRectValueChanged);
            InitReflectionVariables();
        }

        private void LateUpdate()
        {
            if(!scrollRectDirty || isSnapping)
                return;
            if(scrollRect.velocity.magnitude > stoppedVelocityThreshold
               || scrollRect.verticalNormalizedPosition < 0f || scrollRect.verticalNormalizedPosition > 1f) // let scroll rect snap naturally if out of normalized range
            {
                stoppedVelocityTimer = 0f;
                return;
            }

            if(scrollRect.velocity.magnitude <= stoppedVelocityThreshold)
            {
                stoppedVelocityTimer += Time.deltaTime;
                if(stoppedVelocityTimer >= stoppedVelocityTimerDuration && !IsScrollRectDragging)
                    SnapToSong(GetCurrentSelectedSong());
            }
            else
            {
                stoppedVelocityTimer = 0f;
            }
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

        public void Initialize(List<MusicData> dataList)
        {
            ClearList();
            if(dataList.Count == 0)
                return;

            currentSongList = dataList;
            int currentIndex = 0;
            for(int i = 0; i < MaxVisibleListEntries; i++)
            {
                SongInfoDisplay infoDisplay = Instantiate(listEntryPrefab, content).GetComponentInChildren<SongInfoDisplay>();
                infoDisplay.gameObject.name = $"SongInfoDisplay {i.ToString()}";
                if(!infoDisplay)
                {
                    Debug.LogError($"Failed to find a SongInfoDisplay nested underneath prefab {listEntryPrefab.name}!");
                    break;
                }

                MusicData data = currentSongList[currentIndex];
                infoDisplay.SetInfo(data.SongName, data.GetDifficultyInfo(MusicData.Difficulty.EZ).Level);

                LoopSongListIndex(ref currentIndex, 1);
            }

            topIndex = 0;
            bottomIndex = currentIndex - 1;

            Invoke(nameof(RebuildSongList), 0f); // tbh not entirely sure why this fixes the rebuild on the same frame as initializing, but it works ig
        }

        public void ClearList()
        {
            songDisplays.Clear();
            for(int i = content.childCount - 1; i >= 0; i--)
                Destroy(content.GetChild(i).gameObject); // Optimization pass - reuse elements instead of delete and recreate
        }

        #endregion

        // ------------------------------

        #region Read Data

        public MusicData GetCurrentSelectedSong()
        {
            float centerDistanceFromTop = (content.rect.height / 2f) + (listEntryHeight / 2f) + content.anchoredPosition.y; // distance from top of content to "center" of the list
            int indexAboveCenter = Mathf.FloorToInt((centerDistanceFromTop) / listEntryHeight) - 1;
            if(indexAboveCenter >= songDisplays.Count - 1)
                return currentSongList[bottomIndex];
            if(indexAboveCenter < 0)
                return currentSongList[topIndex];

            float itemAboveDistance = Mathf.Abs(content.GetChild(indexAboveCenter).transform.position.y - viewport.transform.position.y);
            float itemBelowDistance = Mathf.Abs(content.GetChild(indexAboveCenter + 1).transform.position.y - viewport.transform.position.y);

            return itemAboveDistance <= itemBelowDistance ? currentSongList[indexAboveCenter + topIndex] : currentSongList[indexAboveCenter + topIndex + 1];
        }

        #endregion

        // ------------------------------

        #region List Movement

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                SnapToSong(currentSongList[1]);
            }
        }

        public void SnapToSong(MusicData targetSong)
        {
            if(!currentSongList.Contains(targetSong))
                return;
            SnapToSong(currentSongList.IndexOf(targetSong));
        }

        public void SnapToSong(int index)
        {
            int subListIndex = index - topIndex;
            bool isTargetSongVisible = subListIndex < content.childCount && subListIndex >= 0;
            float distanceToMove;
            if(isTargetSongVisible)
            {
                distanceToMove = viewport.transform.position.y - content.GetChild(subListIndex).transform.position.y;
            }
            else
            {
                bool isAboveList = subListIndex < 0;
                if(isAboveList)
                {
                    distanceToMove = viewport.transform.position.y - content.GetChild(0).transform.position.y;
                    distanceToMove += subListIndex * listEntryHeight;
                }
                else
                {
                    distanceToMove = viewport.transform.position.y - content.GetChild(content.childCount - 1).transform.position.y;
                    distanceToMove += (subListIndex - content.childCount) * listEntryHeight;
                }
            }

            scrollRect.velocity = Vector2.zero;
            StartCoroutine(MoveListOverTime(distanceToMove, snapSpeed, () =>
            {
                scrollRect.velocity = Vector2.zero;
                scrollRectDirty = false;
            }));
        }

        private IEnumerator MoveListOverTime(float distance, float speed, Action onComplete = null)
        {
            isSnapping = true;
            speed *= Mathf.Sign(distance);
            float duration = Mathf.Abs(distance / speed);

            for(float i = 0f; i < duration; i += Time.deltaTime)
            {
                if(IsScrollRectDragging)
                {
                    isSnapping = false;
                    yield break;
                }
                content.anchoredPosition += speed * Time.deltaTime * Vector2.up;
                yield return null;
            }
            isSnapping = false;
            onComplete?.Invoke();
        }

        private void UpdateListEntryXOffsets()
        {
            foreach(Transform item in content)
            {
                if(item.childCount == 0 || !item.GetChild(0).TryGetComponent(out RectTransform child))
                    return;

                float yOffset = item.transform.position.y - viewport.transform.parent.TransformPoint(Vector3.zero).y;
                float yOffsetCount = yOffset / listEntryHeight;
                child.anchoredPosition = new Vector2(yOffsetCount * xOffset, 0f);
            }
        }

        private void ScrollRectValueChanged(Vector2 value)
        {
            scrollRectDirty = true;
            CheckForLooping();
            onValueChanged?.Invoke();
        }

        #endregion

        // ------------------------------

        #region List Looping

        private void CheckForLooping()
        {
            if(content.childCount == 0)
                return;
            bool layoutGroupDirty = false;

            // Check lower bound
            if(infiniteLoopedScrolling || topIndex > 0)
            {
                Transform lastChild = content.GetChild(content.childCount - 1);
                if(lastChild.transform.position.y - viewport.transform.position.y < -maxHeightDown)
                {
                    LoopListBottom();
                    layoutGroupDirty = true;
                }
            }
            // Check upper bound
            if(infiniteLoopedScrolling || bottomIndex < currentSongList.Count - 1)
            {
                if(content.GetChild(0).transform.position.y - viewport.transform.position.y > maxHeightUp)
                {
                    LoopListTop();
                    layoutGroupDirty = true;
                }
            }

            if(layoutGroupDirty) // Potential for optimization - instead of rebuilding layout groups, manually place each element as we re-order them
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            UpdateListEntryXOffsets();
        }

        /// <summary>
        /// Moves the bottom element of the list to the top of the list
        /// </summary>
        private void LoopListBottom()
        {
            // Move transform/scroll rect
            content.GetChild(content.childCount - 1).SetAsFirstSibling();
            content.anchoredPosition += Vector2.up * listEntryHeight;
            UpdateScrollRectMouseStartPosition(-listEntryHeight);

            // Update display list
            songDisplays.Insert(0, songDisplays[^1]);
            songDisplays.RemoveAt(songDisplays.Count - 1);
            LoopSongListIndex(ref topIndex, -1);
            LoopSongListIndex(ref bottomIndex, -1);
            songDisplays[0].SetInfo(currentSongList[topIndex].SongName, currentSongList[topIndex].GetDifficultyInfo(MusicData.Difficulty.EZ).Level);
        }

        /// <summary>
        /// Moves the top element of the list to the bottom end of the list
        /// </summary>
        private void LoopListTop()
        {
            // Move transform/scroll rect
            content.GetChild(0).SetAsLastSibling();
            content.anchoredPosition -= Vector2.up * listEntryHeight;
            UpdateScrollRectMouseStartPosition(listEntryHeight);

            // Update display list
            songDisplays.Add(songDisplays[0]);
            songDisplays.RemoveAt(0);
            LoopSongListIndex(ref topIndex, 1);
            LoopSongListIndex(ref bottomIndex, 1);
            songDisplays[^1].SetInfo(currentSongList[bottomIndex].SongName, currentSongList[bottomIndex].GetDifficultyInfo(MusicData.Difficulty.EZ).Level);
        }

        #endregion

        // ------------------------------

        #region ScrollRect Reflection

        // This has the potential be a performance problem, but it's probably fine. probably.
        // Solution would be to override the ScrollRect in a new class to expose the variables we need, but this works for now
        private void InitReflectionVariables()
        {
            field_PointerStartLocalCursor = typeof(ScrollRect).GetField("m_PointerStartLocalCursor",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field_Dragging = typeof(ScrollRect).GetField("m_Dragging",
                BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private void UpdateScrollRectMouseStartPosition(float offset)
        {
            field_PointerStartLocalCursor.SetValue(scrollRect, (Vector2)field_PointerStartLocalCursor.GetValue(scrollRect) + Vector2.up * offset);
        }

        public bool IsScrollRectDragging => (bool)field_Dragging.GetValue(scrollRect);

        #endregion

        // ------------------------------

        #region Other

        public void RebuildSongList()
        {
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, 0f);

            songDisplays.Clear();
            foreach(Transform child in content)
            {
                SongInfoDisplay songDisplay = child.GetComponentInChildren<SongInfoDisplay>();
                if(songDisplay)
                {
                    songDisplays.Add(songDisplay);
                    RectTransform rect = songDisplay.GetComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(rect.sizeDelta.x, listEntryHeight);
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            UpdateListEntryXOffsets();
            onValueChanged?.Invoke();
        }

        /// <summary>
        /// Loops given index relative by the given amount to the current song list count (if bigger, loops to 0; if lower, loops to top)
        /// </summary>
        private void LoopSongListIndex(ref int index, int increment)
        {
            index += increment;
            if(index < 0)
                index += currentSongList.Count;
            else if(index >= currentSongList.Count)
                index -= currentSongList.Count;
        }

        #endregion
    }
}

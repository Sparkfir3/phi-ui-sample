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
        private MusicData.Difficulty currentDifficulty;
        [SerializeField]
        private int topIndex;
        [SerializeField]
        private int bottomIndex;

        [Header("Settings"), SerializeField]
        private float listEntryHeight = 100;
        [SerializeField]
        private float xOffset;
        [SerializeField]
        private int maxElementsUp = 5;
        [SerializeField]
        private int maxElementsDown = 7;
        [SerializeField]
        private float snapSpeed = 1000f;
        [SerializeField]
        private float stoppedVelocityThreshold = 10f;
        [SerializeField]
        private float stoppedVelocityTimerDuration = 1f;
        // [SerializeField] // This breaks things that we added later so we'll just disable it for now, don't really need it anyways it's just nice to have
        private bool infiniteLoopedScrolling;

        [Header("Object References"), SerializeField]
        private GameObject listEntryPrefab;
        [SerializeField]
        private ScrollRect scrollRect;
        [SerializeField]
        private VerticalLayoutGroup layoutGroup;

        // ---

        private RectTransform content => scrollRect.content;
        private RectTransform viewport => scrollRect.viewport;
        private float MaxHeightUp => (listEntryHeight + layoutGroup.spacing) * (maxElementsUp + 1); // +1 is used as a buffer
        private float MaxHeightDown => (listEntryHeight + layoutGroup.spacing) * (maxElementsDown + 1);
        private int MaxVisibleListEntries => maxElementsUp + maxElementsDown + 1; // +1 for the center element

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

        #region List Set Data

        public void Initialize(List<MusicData> dataList, MusicData.Difficulty difficulty)
        {
            ClearList();
            if(dataList.Count == 0)
                return;

            currentSongList = dataList;
            currentDifficulty = difficulty;
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
                infoDisplay.SetInfo(data.SongName, data.GetDifficultyInfo(difficulty).Level);

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

        public void SetDifficulty(MusicData.Difficulty difficulty)
        {
            for(int i = 0; i < songDisplays.Count; i++)
            {
                songDisplays[i].SetDifficultyLevel(currentSongList[i + topIndex].GetDifficultyInfo(difficulty).Level);
            }
            currentDifficulty = difficulty;
        }

        #endregion

        // ------------------------------

        #region Read Data

        public MusicData GetCurrentSelectedSong()
        {
            float centerDistanceFromTop = (content.rect.height / 2f) + (listEntryHeight / 2f) + content.anchoredPosition.y; // distance from top of content to "center" of the list (viewport 0)
            int listIndexAboveCenter = Mathf.FloorToInt(centerDistanceFromTop / (listEntryHeight + layoutGroup.spacing)) - 1;
            if(listIndexAboveCenter >= songDisplays.Count - 1)
                return currentSongList[bottomIndex];
            if(listIndexAboveCenter < 0)
                return currentSongList[topIndex];

            float itemAboveDistance = Mathf.Abs(content.GetChild(listIndexAboveCenter).transform.position.y - viewport.transform.position.y);
            float itemBelowDistance = Mathf.Abs(content.GetChild(listIndexAboveCenter + 1).transform.position.y - viewport.transform.position.y);

            int songIndexAboveCenter = listIndexAboveCenter + topIndex;
            LoopSongListIndex(ref songIndexAboveCenter);
            return itemAboveDistance <= itemBelowDistance ? currentSongList[songIndexAboveCenter] : currentSongList[LoopSongListIndex(ref songIndexAboveCenter, 1)];
        }

        #endregion

        // ------------------------------

        #region List Movement

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                SnapToSong(GetCurrentSelectedSong());
            }
        }

        public void SnapToSong(MusicData targetSong, bool instant = false)
        {
            if(!currentSongList.Contains(targetSong))
                return;
            SnapToSong(currentSongList.IndexOf(targetSong), instant);
        }

        public void SnapToSong(int index, bool instant = false)
        {
            scrollRect.velocity = Vector2.zero;

            int subListIndex = index - topIndex;
            bool isTargetSongVisible = subListIndex < content.childCount && subListIndex >= 0;
            float distanceToMove;
            if(isTargetSongVisible)
            {
                distanceToMove = viewport.transform.position.y - content.GetChild(subListIndex).transform.position.y;
                if(instant)
                {
                    content.position += Vector3.up * distanceToMove;
                    return;
                }
            }
            else
            {
                bool isAboveList = subListIndex < 0;
                if(isAboveList)
                {
                    distanceToMove = viewport.transform.position.y - content.GetChild(0).transform.position.y;
                    distanceToMove += subListIndex * (listEntryHeight + layoutGroup.spacing);
                }
                else
                {
                    distanceToMove = viewport.transform.position.y - content.GetChild(content.childCount - 1).transform.position.y;
                    distanceToMove += (subListIndex - content.childCount) * (listEntryHeight + layoutGroup.spacing);
                }
                // TODO = instant move logic
            }

            StartCoroutine(MoveListOverTime(distanceToMove, snapSpeed, () =>
            {
                // TODO - sometimes the list will (very rarely and inconsistently) not snap all the way (for some reason)
                scrollRect.velocity = Vector2.zero;
                scrollRectDirty = false;
            }));
        }

        // can't just move the whole list because of the infinite scrolling/looping, so we have to do this (yay /s)
        private IEnumerator MoveListOverTime(float distance, float speed, Action onComplete = null)
        {
            isSnapping = true;
            speed *= Mathf.Sign(distance);
            float duration = Mathf.Abs(distance / speed);
            float distanceMoved = 0f;

            for(float i = 0f; i < duration; i += Time.deltaTime)
            {
                if(IsScrollRectDragging)
                {
                    isSnapping = false;
                    yield break;
                }
                float distanceToMove = speed * Time.deltaTime;
                distanceMoved += distanceToMove;
                content.anchoredPosition += distanceToMove * Vector2.up;
                yield return null;
            }
            content.anchoredPosition += (distance - distanceMoved) * Vector2.up; // snap to account for rounding
            yield return null;
            isSnapping = false;
            onComplete?.Invoke();
        }

        private void UpdateListEntryXOffsets()
        {
            float contentHeight = content.rect.height;
            foreach(Transform child in content)
            {
                if(child.childCount == 0 || !child.GetChild(0).TryGetComponent(out RectTransform childRectTransform))
                    return;

                float itemYPosition = child.GetComponent<RectTransform>().anchoredPosition.y // this should always be negative, anchored to the content LG's top left
                    + content.anchoredPosition.y;
                float yOffset = contentHeight / 2f + itemYPosition;
                float yOffsetCount = yOffset / listEntryHeight;
                childRectTransform.anchoredPosition = new Vector2(yOffsetCount * xOffset, 0f);
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
                RectTransform lastChild = content.GetChild(content.childCount - 1).GetComponent<RectTransform>();
                if(content.anchoredPosition.y + lastChild.anchoredPosition.y + (content.rect.height / 2f) < -MaxHeightDown)
                {
                    LoopListBottom();
                    layoutGroupDirty = true;
                }
            }
            // Check upper bound
            if(infiniteLoopedScrolling || bottomIndex < currentSongList.Count - 1)
            {
                RectTransform firstChild = content.GetChild(0).GetComponent<RectTransform>();
                if(content.anchoredPosition.y + firstChild.anchoredPosition.y + (content.rect.height / 2f) > MaxHeightUp)
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
            content.anchoredPosition += Vector2.up * (listEntryHeight + layoutGroup.spacing);
            UpdateScrollRectMouseStartPosition(-(listEntryHeight + layoutGroup.spacing));

            // Update display list
            songDisplays.Insert(0, songDisplays[^1]);
            songDisplays.RemoveAt(songDisplays.Count - 1);
            LoopSongListIndex(ref topIndex, -1);
            LoopSongListIndex(ref bottomIndex, -1);
            songDisplays[0].SetInfo(currentSongList[topIndex].SongName, currentSongList[topIndex].GetDifficultyInfo(currentDifficulty).Level);
        }

        /// <summary>
        /// Moves the top element of the list to the bottom end of the list
        /// </summary>
        private void LoopListTop()
        {
            // Move transform/scroll rect
            content.GetChild(0).SetAsLastSibling();
            content.anchoredPosition -= Vector2.up * (listEntryHeight + layoutGroup.spacing);
            UpdateScrollRectMouseStartPosition(listEntryHeight + layoutGroup.spacing);

            // Update display list
            songDisplays.Add(songDisplays[0]);
            songDisplays.RemoveAt(0);
            LoopSongListIndex(ref topIndex, 1);
            LoopSongListIndex(ref bottomIndex, 1);
            songDisplays[^1].SetInfo(currentSongList[bottomIndex].SongName, currentSongList[bottomIndex].GetDifficultyInfo(currentDifficulty).Level);
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
            if(Application.isPlaying)
                SnapToSong(0, instant: true);
            UpdateListEntryXOffsets();
            onValueChanged?.Invoke();
        }

        /// <summary>
        /// Loops given index relative by the given amount to the current song list count (if bigger, loops to 0; if lower, loops to top)
        /// </summary>
        private int LoopSongListIndex(ref int index, int increment = 0)
        {
            index += increment;
            if(index < 0)
                index += currentSongList.Count;
            else if(index >= currentSongList.Count)
                index -= currentSongList.Count;
            return index;
        }

        #endregion
    }
}

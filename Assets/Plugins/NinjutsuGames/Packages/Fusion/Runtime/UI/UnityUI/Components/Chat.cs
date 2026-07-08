#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY)
#define MOBILE
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace NinjutsuGames.FusionNetwork.Runtime
{
    /*[Serializable]
    public class ChatUIComponents
    {
        public PropertyGetInstantiate prefab = new(){usePooling = true, duration = 0, hasDuration = false, size = 10};

        /// <summary>
        /// Input field used for chat input.
        /// </summary>
        public InputReference input;
        public Image background;

        /// <summary>
        /// Root object for chat window's history. This allows you to position the chat window's text.
        /// </summary>
        public ScrollRect container;
    }*/

    [Serializable]
    public class ChatUISettings
    {
        /// <summary>
        /// Whether the activate the chat input when Return key gets pressed.
        /// </summary>
        public bool activateOnInput = true;
        public InputPropertyButton inputTrigger = InputButtonKeyboardPress.Create(Key.Enter);

        public int maxLines = 1000;

        public int minVisibleLines = 0;

        /// <summary>
        /// Seconds that must elapse before a chat label starts to fade out.
        /// </summary>
        public float fadeOutStart = 10f;

        /// <summary>
        /// How long it takes for a chat label to fade out in seconds.
        /// </summary>
        public float fadeOutDuration = 3f;
        public float backgroundFadeOutDuration = 0.5f;

        /// <summary>
        /// Whether messages will fade out over time.
        /// </summary>
        public bool allowChatFading = true;

        
        public bool disablePlayerWhenTyping = true;
        public PropertySetNumber unseenMessages = new();
    }
    /// <summary>
    /// Generic chat window functionality.
    /// </summary>
    public abstract class Chat : SimulationBehaviour
    {
        // [SerializeField] protected ChatUIComponents uiComponents = new();
        [SerializeField] protected PropertyGetInstantiate prefab = new(){usePooling = true, duration = 0, hasDuration = false, size = 10};

        /// <summary>
        /// Input field used for chat input.
        /// </summary>
        [SerializeField] protected InputReference input;
        [SerializeField] protected Image background;

        /// <summary>
        /// Root object for chat window's history. This allows you to position the chat window's text.
        /// </summary>
        [SerializeField] protected ScrollRect container;
        [SerializeField] protected ChatUISettings uiSettings = new();

        private const char SPACE_CHAR = '\n';
        
        //IsFocused has 1 frame delay.
        protected bool _selected;

        // public UnityEvent onOpen;
        // public UnityEvent onClose;

        private readonly WaitForEndOfFrame _wait = new();

        private class ChatEntry
        {
            public CanvasGroup Group;
            public Transform Transform;
            public Text Text;
            public TMP_Text TextTMP;
            public Color Color;
            public float Time;
            public int Lines = 0;
            public float Alpha = 0f;
            public bool IsExpired = false;
            public bool ShouldBeDestroyed = false;

            public GameObject GameObject;
            // public bool fadedIn = false;
        }

        private bool Selected => _selected || EventSystem.current.currentSelectedGameObject == input.GameObject;
        private readonly List<ChatEntry> _chatEntries = new();

        //private int mBackgroundHeight = -1;
        private bool _ignoreNextEnter = false;
        private Color _originalBgColor;
        private CanvasGroup _scrollBarCanvas;
        private float _uiTime;
        private Color _fadedOutBgColor;
        private bool _overInput;
        private bool _overContainer;
        private bool _selectedContainer;
        private int _unSeenCount;
        private static bool _wasPlayerControllable;

        /// <summary>
        /// For things you want to do after OnSubmitInternal method has ran.
        /// </summary>
        // public UnityEvent LateEndEdit = new UnityEvent();
        // Cache these callbacks to avoid allocations
        private readonly EventTrigger.TriggerEvent _emptyTriggerEvent = new EventTrigger.TriggerEvent();
        private readonly UnityEngine.Events.UnityAction<BaseEventData> _selectAction;
        private readonly UnityEngine.Events.UnityAction<BaseEventData> _deselectAction;
        private readonly UnityEngine.Events.UnityAction<BaseEventData> _pointerEnterInputAction;
        private readonly UnityEngine.Events.UnityAction<BaseEventData> _pointerExitInputAction;
        private readonly UnityEngine.Events.UnityAction<BaseEventData> _pointerEnterContainerAction;
        private readonly UnityEngine.Events.UnityAction<BaseEventData> _pointerExitContainerAction;
        private readonly UnityEngine.Events.UnityAction<Vector2> _scrollAction;

        protected Chat()
        {
            // Initialize the cached actions in the constructor to avoid allocations in Awake
            _selectAction = OnSelectEvent;
            _deselectAction = OnDeselectEvent;
            _pointerEnterInputAction = OnPointerEnterInput;
            _pointerExitInputAction = OnPointerExitInput;
            _pointerEnterContainerAction = OnPointerEnterContainer;
            _pointerExitContainerAction = OnPointerExitContainer;
            _scrollAction = OnScroll;
        }

        private void OnSelectEvent(BaseEventData data) => Select();
        private void OnDeselectEvent(BaseEventData data) => Deselect();
        private void OnPointerEnterInput(BaseEventData data) { _overInput = true; Select(); }
        private void OnPointerExitInput(BaseEventData data) { _overInput = false; Deselect(); }
        private void OnPointerEnterContainer(BaseEventData data) { _overContainer = true; Select(); }
        private void OnPointerExitContainer(BaseEventData data) { _overContainer = false; Deselect(); }

        protected virtual void Awake()
        {
            uiSettings.inputTrigger.OnStartup();
            uiSettings.inputTrigger.RegisterPerform(OnTriggerInput);
            
            _originalBgColor = background.color;
            _fadedOutBgColor = background.color;
            _scrollBarCanvas = container.verticalScrollbar.GetComponent<CanvasGroup>();
            if(!_scrollBarCanvas)_scrollBarCanvas = container.verticalScrollbar.gameObject.AddComponent<CanvasGroup>();
            
            if(uiSettings.allowChatFading)
            {
                _fadedOutBgColor.a = 0;
                background.color = _fadedOutBgColor;

                _scrollBarCanvas.alpha = 0;
            }

            if (input != null)
            {
                SetupInputEventTriggers();
                input.SubscribeOnValueChanged(OnValueChanged);
                input.SubscribeOnSubmit(OnSubmitInternal);
            }
            
            SetupContainerEventTriggers();
            container.onValueChanged.AddListener(_scrollAction);
            
            CheckPlayerControllable();
        }

        private void SetupInputEventTriggers()
        {
            var eventTrigger = input.GameObject.GetComponent<EventTrigger>();
            if (!eventTrigger) eventTrigger = input.GameObject.AddComponent<EventTrigger>();
            
            // Clear existing triggers to avoid duplicates
            eventTrigger.triggers.Clear();
            
            // Add select trigger
            var onSel = new EventTrigger.Entry { eventID = EventTriggerType.Select };
            onSel.callback.AddListener(_selectAction);
            eventTrigger.triggers.Add(onSel);
            
            // Add deselect trigger
            var onUnsel = new EventTrigger.Entry { eventID = EventTriggerType.Deselect };
            onUnsel.callback.AddListener(_deselectAction);
            eventTrigger.triggers.Add(onUnsel);
            
            // Add pointer enter trigger
            var onHover = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            onHover.callback.AddListener(_pointerEnterInputAction);
            eventTrigger.triggers.Add(onHover);
            
            // Add pointer exit trigger
            var onExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            onExit.callback.AddListener(_pointerExitInputAction);
            eventTrigger.triggers.Add(onExit);
        }

        private void SetupContainerEventTriggers()
        {
            var eventTrigger = container.GetComponent<EventTrigger>();
            if (!eventTrigger) eventTrigger = container.gameObject.AddComponent<EventTrigger>();
            
            // Clear existing triggers to avoid duplicates
            eventTrigger.triggers.Clear();
            
            // Add pointer enter trigger
            var onHover = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            onHover.callback.AddListener(_pointerEnterContainerAction);
            eventTrigger.triggers.Add(onHover);
            
            // Add pointer exit trigger
            var onExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            onExit.callback.AddListener(_pointerExitContainerAction);
            eventTrigger.triggers.Add(onExit);
        }

        private void OnScroll(Vector2 arg0)
        {
            _selectedContainer = container.velocity != Vector2.zero;
            if(!_overContainer && !_selectedContainer && !_overInput) Deselect();
        }

        private void OnTriggerInput()
        {
            if (!uiSettings.activateOnInput) return; // && (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
            if (!_ignoreNextEnter)
            {
                input.Interactable = true;
                input.Select();
                input.ActivateInputField();

                EventSystem current;
                (current = EventSystem.current).SetSelectedGameObject(input.GameObject, null);
                input.OnPointerClick(new PointerEventData(current));
            }

            _ignoreNextEnter = false;
        }

        protected virtual void OnDestroy()
        {
            CheckPlayerControllable();
            
            uiSettings.inputTrigger.OnDispose();
            uiSettings.inputTrigger.ForgetPerform(OnTriggerInput);

            if (input == null) return;
            input.UnsubscribeOnValueChanged(OnValueChanged);
            input.UnsubscribeOnSubmit(OnSubmitInternal);
        }

        private void CheckPlayerControllable()
        {
            var player = ShortcutPlayer.Instance ? ShortcutPlayer.Instance.Get<Character>() : null;
            if (uiSettings.disablePlayerWhenTyping && player && !player.Player.IsControllable && _wasPlayerControllable)
            {
                player.Player.IsControllable = true;
            }
        }

        private void OnValueChanged(string input)
        {
            if (uiSettings.disablePlayerWhenTyping && 
                !string.IsNullOrEmpty(input) &&
                ShortcutPlayer.Instance &&
                ShortcutPlayer.Instance.Get<Character>().Player.IsControllable)
            {
                ShortcutPlayer.Instance.Get<Character>().Player.IsControllable = false;
            }
        }

        public void Select()
        {
            if(_selected) return;
            // Debug.Log($"Select");
            _uiTime = Time.time;

            _unSeenCount = 0;
            uiSettings.unseenMessages?.Set(_unSeenCount, gameObject);

            _selected = true;
            OnOpen();

            var player = ShortcutPlayer.Instance ? ShortcutPlayer.Instance.Get<Character>() : null;
            if (!uiSettings.disablePlayerWhenTyping || !player || !player.Player.IsControllable) return;
            _wasPlayerControllable = player.Player.IsControllable;
            player.Player.IsControllable = false;
        }

        public void Deselect()
        {
            if(!_selected) return;

            // Debug.LogWarning($"Deselect overInput: {_overInput} overContainer: {_overContainer} selectedContainer: {_selectedContainer} wasPlayerControllable: {_wasPlayerControllable} canSelect: {(!_overInput && !_overContainer && !_selectedContainer)}");

            if(_overInput || _overContainer || _selectedContainer) return;

            _selected = false;
            _selectedContainer = false;
            OnClose();
            CheckPlayerControllable();
        }

        /// <summary>
        /// Handle inputfield onEndEdit event.
        /// </summary>
        public void OnSubmitInternal(string content)
        {
            _ignoreNextEnter = true;
            input.Text = string.Empty;
            
            // Only censor if content is not empty to avoid unnecessary operations
            if (!string.IsNullOrWhiteSpace(content))
            {
                content = FusionRepository.Get.ProfanityFilter.Censor(content);
                OnSubmit(content);
            }

            input.DeactivateInputField();
            if (!EventSystem.current.alreadySelecting) EventSystem.current.SetSelectedGameObject(null, null);
        }

        private IEnumerator SetInputFieldNotInteractableAtEndOfFrame()
        {
            yield return _wait;
            input.Interactable = false;
        }

        [ContextMenu("Clear History")]
        public void ClearHistory()
        {
            for (var i = _chatEntries.Count - 1; i >= 0; --i)
            {
                RemoveEntry(i);
            }
        }
        
        protected virtual void OnOpen() {}
        protected virtual void OnClose() {}

        /// <summary>
        /// Custom submit logic for what happens on chat input submission.
        /// </summary>
        protected virtual void OnSubmit(string text)
        {
        }

        // Cached array for string splitting to reduce GC allocations
        private static readonly char[] SplitChars = { SPACE_CHAR };
        
        /// <summary>
        /// Add a new chat entry.
        /// </summary>
        private GameObject InternalAdd(string text, Color color, bool tintBackground)
        {
            var go = prefab.Get(gameObject);
            if(!go) return null;
            
            // Only censor if necessary to avoid unnecessary operations
            if (!string.IsNullOrEmpty(text))
            {
                text = FusionRepository.Get.ProfanityFilter.Censor(text);
            }
            
            var ent = new ChatEntry
            {
                Time = Time.time,
                Color = color,
                GameObject = go,
                Transform = go.transform,
                Group = go.Get<CanvasGroup>() ?? go.Add<CanvasGroup>(),
                Text = go.Get<Text>(),
                TextTMP = go.Get<TMP_Text>()
            };
            
            go.transform.SetParent(container.content, false);
            go.SetActive(true);
            
            if (tintBackground)
            {
                go.Get<Image>().color = color;
            }
            else
            {
                if(ent.Text) ent.Text.color = color;
                if(ent.TextTMP) ent.TextTMP.color = color;
            }
            
            if(ent.Text) 
            {
                ent.Text.text = text;
                ent.Lines = text.Split(SplitChars, StringSplitOptions.None).Length;
            }
            
            if(ent.TextTMP) 
            {
                ent.TextTMP.text = text;
                if(ent.Lines == 0) // Only calculate if not already done
                {
                    ent.Lines = text.Split(SplitChars, StringSplitOptions.None).Length;
                }
            }
            
            // Add entry to list before processing
            _chatEntries.Add(ent);
            
            ProcessChatEntries();
            
            if (!Selected)
            {
                _unSeenCount++;
                uiSettings.unseenMessages?.Set(_unSeenCount, gameObject);
            }

            return go;
        }
        
        private void ProcessChatEntries()
        {
            int lineOffset = 0;
            
            // Process from newest to oldest
            for (int i = _chatEntries.Count - 1; i >= 0; i--)
            {
                var e = _chatEntries[i];
                
                if (i == _chatEntries.Count - 1)
                {
                    // It's the newest entry. It doesn't need to be re-positioned.
                    lineOffset += e.Lines;
                }
                else
                {
                    // Check if we need to expire old entries
                    if (lineOffset + e.Lines > uiSettings.maxLines && uiSettings.maxLines > 0)
                    {
                        e.IsExpired = true;
                        e.ShouldBeDestroyed = true;

                        if (e.Alpha == 0f)
                        {
                            RemoveEntry(i);
                            continue;
                        }
                    }

                    lineOffset += e.Lines;
                }
            }
        }

        /// <summary>
        /// Update the "alpha" of each line and update the background size.
        /// </summary>
        // Cached color for background to avoid GC allocations from Color.Lerp
        private Color _cachedBackgroundColor = Color.white;

        protected virtual void Update()
        {
            if(!NetworkManager.IsConnected) return;
            uiSettings.inputTrigger.OnUpdate();
            
            if(uiSettings.allowChatFading)
            {
                float uiAlpha = _scrollBarCanvas.alpha;

                if (Selected)
                {
                    // Quickly fade in new entries
                    uiAlpha = Mathf.Clamp01(uiAlpha + Time.deltaTime * 5f);
                }
                else if (Time.time - (_uiTime + uiSettings.fadeOutStart) < uiSettings.backgroundFadeOutDuration)
                {
                    // Slowly fade out entries that have been visible for a while
                    uiAlpha = Mathf.Clamp01(uiAlpha - Time.deltaTime / uiSettings.backgroundFadeOutDuration);
                }
                else
                {
                    // Quickly fade out chat entries that should have faded by now,
                    // but likely didn't due to the input being selected.
                    uiAlpha = Mathf.Clamp01(uiAlpha - Time.deltaTime);
                }

                // Only update color if alpha changed significantly to reduce allocations
                if (Mathf.Abs(_scrollBarCanvas.alpha - uiAlpha) > 0.01f)
                {
                    _scrollBarCanvas.alpha = uiAlpha;
                    
                    // Reuse the cached color to avoid GC allocations
                    _cachedBackgroundColor.r = Mathf.Lerp(_fadedOutBgColor.r, _originalBgColor.r, uiAlpha);
                    _cachedBackgroundColor.g = Mathf.Lerp(_fadedOutBgColor.g, _originalBgColor.g, uiAlpha);
                    _cachedBackgroundColor.b = Mathf.Lerp(_fadedOutBgColor.b, _originalBgColor.b, uiAlpha);
                    _cachedBackgroundColor.a = Mathf.Lerp(_fadedOutBgColor.a, _originalBgColor.a, uiAlpha);
                    background.color = _cachedBackgroundColor;
                }
            }

            UpdateChatEntries();
        }
        
        private void UpdateChatEntries()
        {
            for (var i = 0; i < _chatEntries.Count;)
            {
                var e = _chatEntries[i];
                float alpha = e.Alpha;
                bool alphaChanged = false;

                if (e.IsExpired)
                {
                    // Quickly fade out expired chat entries
                    float newAlpha = Mathf.Clamp01(alpha - Time.deltaTime);
                    if (Mathf.Abs(alpha - newAlpha) > 0.01f)
                    {
                        alpha = newAlpha;
                        alphaChanged = true;
                    }
                }
                else if (Selected || Time.time - e.Time < uiSettings.fadeOutStart)
                {
                    // Quickly fade in new entries
                    float newAlpha = Mathf.Clamp01(alpha + Time.deltaTime * 5f);
                    if (Mathf.Abs(alpha - newAlpha) > 0.01f)
                    {
                        alpha = newAlpha;
                        alphaChanged = true;
                    }
                }
                else if (Time.time - (e.Time + uiSettings.fadeOutStart) < uiSettings.fadeOutDuration)
                {
                    // Slowly fade out entries that have been visible for a while
                    float newAlpha = Mathf.Clamp01(alpha - Time.deltaTime / uiSettings.fadeOutDuration);
                    if (Mathf.Abs(alpha - newAlpha) > 0.01f)
                    {
                        alpha = newAlpha;
                        alphaChanged = true;
                    }
                }
                else
                {
                    // Quickly fade out chat entries that should have faded by now,
                    // but likely didn't due to the input being selected.
                    float newAlpha = Mathf.Clamp01(alpha - Time.deltaTime);
                    if (Mathf.Abs(alpha - newAlpha) > 0.01f)
                    {
                        alpha = newAlpha;
                        alphaChanged = true;
                    }
                }

                if (alphaChanged)
                {
                    e.Alpha = alpha;
                    e.Group.alpha = !uiSettings.allowChatFading || i > _chatEntries.Count - (uiSettings.minVisibleLines + 1) ? 1 : alpha;

                    if (alpha == 0f && e.ShouldBeDestroyed)
                    {
                        // This chat entry has expired and should be removed
                        RemoveEntry(i);
                        continue;
                    }
                }

                // If the line is visible, it should be counted
                ++i;
            }
        }

        private void RemoveEntry(int index)
        {
            var entry = _chatEntries[index].GameObject;
            if(!entry)
            {
                _chatEntries.RemoveAt(index);
                return;
            }
            
            if(prefab.usePooling) entry.SetActive(false);
            else Destroy(entry);
            
            _chatEntries.RemoveAt(index);
        }

        /// <summary>
        /// Add a new chat entry.
        /// </summary>
        protected virtual GameObject Add(string text, Color color, bool tintBackground, PlayerRef player)
        {
            return InternalAdd(text, color, tintBackground);
        }
    }
}
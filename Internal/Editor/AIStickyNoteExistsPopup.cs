using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using AIStickyNotes.Internal;

namespace AIStickyNotes.Editor.Internal
{
    /// <summary>
    /// UI Toolkit editor window popup displayed when attempting to add a sticky note to a GameObject
    /// that already has an AIStickyNote component. Offers options to replace or keep the existing note.
    /// </summary>
    public class AIStickyNoteExistsPopup : EditorWindow
    {
        /// <summary>
        /// Current popup instance for test access.
        /// </summary>
        private static AIStickyNoteExistsPopup _currentInstance;

        /// <summary>
        /// The target GameObject that already has a sticky note.
        /// </summary>
        private GameObject _targetObject;

        /// <summary>
        /// The existing AIStickyNote component on the target.
        /// </summary>
        private AIStickyNote _existingNote;

        /// <summary>
        /// Whether the GUI has been created (guards against repeated CreateGUI calls).
        /// </summary>
        private bool _guiCreated;

        /// <summary>
        /// Whether to use modal mode (production) or utility mode (testing).
        /// </summary>
        private bool _useModalMode = true;

        // Cached UI elements
        private TextField _messageField;
        private Label _priorityDisplay;
        private Button _keepButton;
        private Button _replaceButton;

        private const float WINDOW_WIDTH = 360f;
        private const float WINDOW_HEIGHT = 320f;

        /// <summary>
        /// Shows the popup as a modal dialog (production use).
        /// </summary>
        /// <param name="target">The GameObject with the existing note.</param>
        /// <param name="note">The existing AIStickyNote component.</param>
        public static void Show(GameObject target, AIStickyNote note)
        {
            ShowInternal(target, note, modal: true);
        }

        /// <summary>
        /// Shows the popup as a non-modal utility window (for testing/iteration).
        /// </summary>
        /// <param name="target">The GameObject with the existing note.</param>
        /// <param name="note">The existing AIStickyNote component.</param>
        /// <returns>The created popup window.</returns>
        public static AIStickyNoteExistsPopup ShowWithoutDelay(GameObject target, AIStickyNote note)
        {
            return ShowInternal(target, note, modal: false);
        }

        /// <summary>
        /// Internal method to show the popup with configurable modal behavior.
        /// </summary>
        private static AIStickyNoteExistsPopup ShowInternal(GameObject target, AIStickyNote note, bool modal)
        {
            // Close existing instance
            ClosePopup();

            var window = CreateInstance<AIStickyNoteExistsPopup>();
            window._targetObject = target;
            window._existingNote = note;
            window._useModalMode = modal;
            window.titleContent = new GUIContent($"Sticky Note: {target.name}");

            // Center on main window
            var mainWindowPos = EditorGUIUtility.GetMainWindowPosition();
            var rect = new Rect(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT);
            rect.center = new Vector2(
                mainWindowPos.x + mainWindowPos.width / 2f,
                mainWindowPos.y + mainWindowPos.height / 2f
            );
            window.position = rect;
            window.minSize = new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);
            window.maxSize = new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);

            // Use ShowUtility for non-modal testing, ShowModalUtility for production
            if (modal)
            {
                window.ShowModalUtility();
            }
            else
            {
                window.ShowUtility();
            }

            window.Focus();
            _currentInstance = window;
            return window;
        }

        /// <summary>
        /// Gets the current popup instance for test access.
        /// </summary>
        /// <returns>The current popup instance, or null if none is open.</returns>
        public static AIStickyNoteExistsPopup GetCurrentInstance() => _currentInstance;

        /// <summary>
        /// Closes the currently displayed popup if any.
        /// </summary>
        public static void ClosePopup()
        {
            if (_currentInstance != null)
            {
                try
                {
                    _currentInstance.Close();
                }
                catch
                {
                    // Ignore close errors (window may already be destroyed)
                }
                _currentInstance = null;
            }
        }

        /// <summary>
        /// Creates the UI Toolkit GUI for the popup.
        /// </summary>
        public void CreateGUI()
        {
            // Guard against repeated CreateGUI calls
            if (_guiCreated)
                return;

            // Load UXML
            string basePath = GetAssetBasePath();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                $"{basePath}/AIStickyNoteExistsPopup.uxml"
            );

            if (visualTree == null)
            {
                Debug.LogError("[AIStickyNotes] Failed to load AIStickyNoteExistsPopup.uxml");
                return;
            }

            visualTree.CloneTree(rootVisualElement);

            // Load USS
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                $"{basePath}/AIStickyNotesCommon.uss"
            );
            if (styleSheet != null)
            {
                rootVisualElement.styleSheets.Add(styleSheet);
            }

            // Cache UI elements
            _messageField = rootVisualElement.Q<TextField>("message-field");
            _priorityDisplay = rootVisualElement.Q<Label>("priority-display");
            _keepButton = rootVisualElement.Q<Button>("keep-button");
            _replaceButton = rootVisualElement.Q<Button>("replace-button");

            // Set values from existing note
            if (_existingNote != null)
            {
                _messageField.value = _existingNote.Message ?? "";
                _priorityDisplay.text = $"Read Priority: {_existingNote.ReadPriority} (lower = read first)";
            }

            // Wire up button clicks
            _keepButton.clicked += OnKeepClicked;
            _replaceButton.clicked += OnReplaceClicked;

            // Register keyboard shortcuts on root
            rootVisualElement.focusable = true;
            rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

            _guiCreated = true;
        }

        /// <summary>
        /// Handles keyboard shortcuts for the popup.
        /// </summary>
        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                OnReplaceClicked();
                evt.StopPropagation();
                evt.PreventDefault();
            }
            else if (evt.keyCode == KeyCode.Escape)
            {
                OnKeepClicked();
                evt.StopPropagation();
                evt.PreventDefault();
            }
        }

        /// <summary>
        /// Handles the Keep button click.
        /// </summary>
        private void OnKeepClicked()
        {
            Close();
        }

        /// <summary>
        /// Handles the Replace button click.
        /// </summary>
        private void OnReplaceClicked()
        {
            if (_targetObject == null || _existingNote == null)
            {
                Close();
                return;
            }

            // Destroy existing note
            Undo.DestroyObjectImmediate(_existingNote);

            // Open the create popup
            var newWindow = AIStickyNotePopup.Show(_targetObject);
            Close();

            // Focus the new window's text field after a delay
            EditorApplication.delayCall += () => newWindow?.FocusWithTextField();
        }

        /// <summary>
        /// Cleanup when the window is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (_currentInstance == this)
            {
                _currentInstance = null;
            }
        }

        /// <summary>
        /// Gets the base path to the AIStickyNotes editor assets.
        /// </summary>
        private string GetAssetBasePath()
        {
            // Find the path to this script's directory
            var guids = AssetDatabase.FindAssets("AIStickyNoteExistsPopup t:Script");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("AIStickyNotes"))
                {
                    return System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
                }
            }
            return "Assets/AIStickyNotes/Internal/Editor";
        }
    }
}

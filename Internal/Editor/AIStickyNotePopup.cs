using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using AIStickyNotes.Internal;

namespace AIStickyNotes.Editor.Internal
{
    /// <summary>
    /// UI Toolkit editor window popup for creating a new AIStickyNote component on a GameObject.
    /// Provides fields for entering the AI message and read priority with a yellow sticky note aesthetic.
    /// </summary>
    public class AIStickyNotePopup : EditorWindow
    {
        /// <summary>
        /// Current popup instance for test access.
        /// </summary>
        private static AIStickyNotePopup _currentInstance;

        /// <summary>
        /// The target GameObject to add the sticky note to.
        /// </summary>
        private GameObject _targetObject;

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
        private IntegerField _priorityField;
        private Button _cancelButton;
        private Button _addButton;

        private const float WINDOW_WIDTH = 360f;
        private const float WINDOW_HEIGHT = 320f;

        /// <summary>
        /// Shows the popup as a modal dialog (production use).
        /// </summary>
        /// <param name="target">The GameObject to add the note to.</param>
        /// <returns>The created popup window.</returns>
        public static AIStickyNotePopup Show(GameObject target)
        {
            return ShowInternal(target, modal: true);
        }

        /// <summary>
        /// Shows the popup as a non-modal utility window (for testing/iteration).
        /// </summary>
        /// <param name="target">The GameObject to add the note to.</param>
        /// <returns>The created popup window.</returns>
        public static AIStickyNotePopup ShowWithoutDelay(GameObject target)
        {
            return ShowInternal(target, modal: false);
        }

        /// <summary>
        /// Internal method to show the popup with configurable modal behavior.
        /// </summary>
        private static AIStickyNotePopup ShowInternal(GameObject target, bool modal)
        {
            // Close existing instance
            ClosePopup();

            var window = CreateInstance<AIStickyNotePopup>();
            window._targetObject = target;
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
        public static AIStickyNotePopup GetCurrentInstance() => _currentInstance;

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
                $"{basePath}/AIStickyNotePopup.uxml"
            );

            if (visualTree == null)
            {
                Debug.LogError("[AIStickyNotes] Failed to load AIStickyNotePopup.uxml");
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
            _priorityField = rootVisualElement.Q<IntegerField>("priority-field");
            _cancelButton = rootVisualElement.Q<Button>("cancel-button");
            _addButton = rootVisualElement.Q<Button>("add-button");

            // Set initial values
            _priorityField.value = 0;

            // Wire up button clicks
            _cancelButton.clicked += OnCancelClicked;
            _addButton.clicked += OnAddClicked;

            // Register keyboard shortcuts on root
            rootVisualElement.focusable = true;
            rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

            // Focus the message field initially
            _messageField.schedule.Execute(() => _messageField.Focus()).ExecuteLater(50);

            _guiCreated = true;
        }

        /// <summary>
        /// Handles keyboard shortcuts for the popup.
        /// </summary>
        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                // Don't trigger add if we're in a multiline text field and shift isn't pressed
                // Allow Enter to add new lines in the message field
                if (_messageField.focusController?.focusedElement == _messageField && !evt.shiftKey)
                {
                    // Let Enter work normally in the text field
                    return;
                }
                OnAddClicked();
                evt.StopPropagation();
                evt.PreventDefault();
            }
            else if (evt.keyCode == KeyCode.Escape)
            {
                OnCancelClicked();
                evt.StopPropagation();
                evt.PreventDefault();
            }
            else if (evt.keyCode == KeyCode.Tab)
            {
                // Toggle focus between message and priority
                if (_messageField.focusController?.focusedElement == _messageField)
                {
                    _priorityField.Focus();
                }
                else
                {
                    _messageField.Focus();
                }
                evt.StopPropagation();
                evt.PreventDefault();
            }
        }

        /// <summary>
        /// Handles the Cancel button click.
        /// </summary>
        private void OnCancelClicked()
        {
            Close();
        }

        /// <summary>
        /// Handles the Add Note button click.
        /// </summary>
        private void OnAddClicked()
        {
            if (_targetObject == null)
            {
                Close();
                return;
            }

            // Create the sticky note
            var note = Undo.AddComponent<AIStickyNote>(_targetObject);
            var so = new SerializedObject(note);
            so.FindProperty("aiMessage").stringValue = _messageField.value;
            so.FindProperty("readPriority").intValue = _priorityField.value;
            so.ApplyModifiedProperties();

            Debug.Log($"[AIStickyNotes] Added AIStickyNote to {_targetObject.name}");

            Selection.activeGameObject = _targetObject;
            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(note, true);
            EditorUtility.SetDirty(_targetObject);

            Close();
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
            var guids = AssetDatabase.FindAssets("AIStickyNotePopup t:Script");
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

        /// <summary>
        /// Focuses the window and message field. For use after window may have lost focus.
        /// </summary>
        public void FocusWithTextField()
        {
            Focus();
            _messageField?.Focus();
        }
    }
}

using UnityEngine;
using UnityEditor;
using AIStickyNotes.Internal;

namespace AIStickyNotes.Editor.Internal
{
    /// <summary>
    /// Editor window popup for creating a new AIStickyNote component on a GameObject.
    /// Provides fields for entering the AI message and read priority.
    /// </summary>
    public class AIStickyNotePopup : EditorWindow
    {
        /// <summary>
        /// The target GameObject to add the sticky note to.
        /// </summary>
        private GameObject targetObject;

        /// <summary>
        /// The AI message content being entered.
        /// </summary>
        private string aiMessage = "";

        /// <summary>
        /// The read priority value (lower = read first).
        /// </summary>
        private int readPriority = 0;

        /// <summary>
        /// Index of the currently focused field (0 = message, 1 = priority).
        /// </summary>
        private int currentFieldIndex = 0;

        /// <summary>
        /// Flag indicating a focus change is pending (used for Tab navigation).
        /// </summary>
        private bool pendingFocusChange = true;

        /// <summary>
        /// Shows the popup dialog for adding a new sticky note.
        /// </summary>
        /// <param name="target">The GameObject to add the note to.</param>
        public static void Show(GameObject target)
        {
            var window = CreateInstance<AIStickyNotePopup>();
            window.targetObject = target;
            window.titleContent = new GUIContent("Add AI Sticky Note");

            const float width = 400f;
            const float height = 220f;
            var mainWindowPos = EditorGUIUtility.GetMainWindowPosition();
            var rect = new Rect(0, 0, width, height);
            rect.center = new Vector2(
                mainWindowPos.x + mainWindowPos.width / 2f,
                mainWindowPos.y + mainWindowPos.height / 2f
            );
            window.position = rect;
            window.minSize = new Vector2(width, height);
            window.maxSize = new Vector2(width, height);

            window.ShowModalUtility();
            window.Focus();
        }

        /// <summary>
        /// Draws the popup GUI with message input, priority field, and action buttons.
        /// Keyboard shortcuts: Enter = Add, Escape = Cancel, Tab = Switch field.
        /// </summary>
        private void OnGUI()
        {
            if (targetObject == null)
            {
                Close();
                return;
            }

            var e = Event.current;

            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Tab)
            {
                currentFieldIndex = (currentFieldIndex + 1) % 2;
                pendingFocusChange = true;
                e.Use();
                return;
            }

            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                {
                    CreateStickyNote();
                    Close();
                    e.Use();
                    return;
                }
                else if (e.keyCode == KeyCode.Escape)
                {
                    Close();
                    e.Use();
                    return;
                }
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"Adding Sticky Note to: {targetObject.name}", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("AI Message:");
            GUI.SetNextControlName("AIMessageField");
            aiMessage = EditorGUILayout.TextArea(aiMessage, GUILayout.MinHeight(80));

            if (aiMessage.Contains("\t"))
                aiMessage = aiMessage.Replace("\t", "");

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            GUI.SetNextControlName("PriorityField");
            readPriority = EditorGUILayout.IntField("Read Priority:", readPriority, GUILayout.Width(200));
            EditorGUILayout.LabelField("(lower = read first)", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(15);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Cancel", GUILayout.Width(100)))
            {
                Close();
            }

            GUI.backgroundColor = new Color(0.5f, 0.8f, 0.5f);
            if (GUILayout.Button("Add Note", GUILayout.Width(100)))
            {
                CreateStickyNote();
                Close();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Enter = Add, Escape = Cancel, Tab = Switch field", EditorStyles.miniLabel);

            if (pendingFocusChange && e.type == EventType.Repaint)
            {
                string targetControl = currentFieldIndex == 0 ? "AIMessageField" : "PriorityField";
                GUI.FocusControl(targetControl);
                pendingFocusChange = false;
            }
        }

        /// <summary>
        /// Creates the AIStickyNote component on the target GameObject with the entered values.
        /// Uses Undo system for editor undo support.
        /// </summary>
        private void CreateStickyNote()
        {
            var note = Undo.AddComponent<AIStickyNote>(targetObject);

            var so = new SerializedObject(note);
            so.FindProperty("aiMessage").stringValue = aiMessage;
            so.FindProperty("readPriority").intValue = readPriority;
            so.ApplyModifiedProperties();

            Debug.Log($"[AIStickyNotes] Added AIStickyNote to {targetObject.name}");

            Selection.activeGameObject = targetObject;
            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(note, true);
            EditorUtility.SetDirty(targetObject);
        }
    }
}

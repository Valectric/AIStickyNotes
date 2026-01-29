using UnityEngine;
using UnityEditor;
using AIStickyNotes.Internal;

namespace AIStickyNotes.Editor.Internal
{
    /// <summary>
    /// Editor window popup displayed when attempting to add a sticky note to a GameObject
    /// that already has an AIStickyNote component. Offers options to replace or keep the existing note.
    /// </summary>
    public class AIStickyNoteExistsPopup : EditorWindow
    {
        /// <summary>
        /// The target GameObject that already has a sticky note.
        /// </summary>
        private GameObject targetObject;

        /// <summary>
        /// The existing AIStickyNote component on the target.
        /// </summary>
        private AIStickyNote existingNote;

        /// <summary>
        /// Shows the popup dialog for a GameObject with an existing sticky note.
        /// </summary>
        /// <param name="target">The GameObject with the existing note.</param>
        /// <param name="note">The existing AIStickyNote component.</param>
        public static void Show(GameObject target, AIStickyNote note)
        {
            var window = CreateInstance<AIStickyNoteExistsPopup>();
            window.targetObject = target;
            window.existingNote = note;
            window.titleContent = new GUIContent("Sticky Note Exists");

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
        /// Draws the popup GUI with the existing note content and Replace/Keep buttons.
        /// Keyboard shortcuts: Enter = Replace, Escape = Keep.
        /// </summary>
        private void OnGUI()
        {
            if (targetObject == null || existingNote == null)
            {
                Close();
                return;
            }

            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                {
                    Undo.DestroyObjectImmediate(existingNote);
                    var newWindow = AIStickyNotePopup.Show(targetObject);
                    Close();
                    EditorApplication.delayCall += () => newWindow?.FocusWithTextField();
                    Event.current.Use();
                    return;
                }
                else if (Event.current.keyCode == KeyCode.Escape)
                {
                    Close();
                    Event.current.Use();
                    return;
                }
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"Sticky Note on: {targetObject.name}", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Existing Message:");
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextArea(existingNote.Message ?? "", GUILayout.MinHeight(80));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"Read Priority: {existingNote.ReadPriority} (lower = read first)");

            EditorGUILayout.Space(15);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = new Color(1f, 0.7f, 0.4f);
            if (GUILayout.Button("Replace", GUILayout.Width(100)))
            {
                Undo.DestroyObjectImmediate(existingNote);
                var newWindow = AIStickyNotePopup.Show(targetObject);
                Close();
                EditorApplication.delayCall += () => newWindow?.FocusWithTextField();
            }
            GUI.backgroundColor = Color.white;

            if (GUILayout.Button("Keep", GUILayout.Width(100)))
            {
                Close();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Enter = Replace, Escape = Keep", EditorStyles.miniLabel);
        }
    }
}

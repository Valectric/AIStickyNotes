using UnityEngine;
using UnityEditor;
using System.Linq;
using AIStickyNotes.Internal;

namespace AIStickyNotes.Editor.Internal
{
    /// <summary>
    /// Custom inspector editor for AIStickyNote components.
    /// Provides scene-wide management actions and a menu item for quick note creation.
    /// </summary>
    [CustomEditor(typeof(AIStickyNote))]
    public class AIStickyNoteEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Adds an AI sticky note to the selected GameObject via the hierarchy context menu.
        /// Shows existing note popup if one already exists, otherwise shows creation popup.
        /// Priority -100 places it near the top of the menu. Shortcut: Ctrl+Alt+S.
        /// </summary>
        /// <param name="menuCommand">The menu command context containing the target GameObject.</param>
        [MenuItem("GameObject/Add AI Sticky Note %&s", false, -100)]
        private static void AddAIStickyNoteToSelected(MenuCommand menuCommand)
        {
            var gameObject = menuCommand.context as GameObject;
            if (gameObject == null)
                gameObject = Selection.activeGameObject;

            if (gameObject == null)
                return;

            var existingNote = gameObject.GetComponent<AIStickyNote>();
            if (existingNote != null)
            {
                AIStickyNoteExistsPopup.Show(gameObject, existingNote);
                return;
            }

            AIStickyNotePopup.Show(gameObject);
        }

        /// <summary>
        /// Validates the menu item is only enabled when a GameObject is selected.
        /// </summary>
        /// <returns>True if a GameObject is selected, false otherwise.</returns>
        [MenuItem("GameObject/Add AI Sticky Note %&s", true)]
        private static bool ValidateAddAIStickyNote()
        {
            return Selection.activeGameObject != null;
        }

        /// <summary>
        /// Draws the custom inspector GUI with scene-wide management buttons.
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Scene Actions", EditorStyles.boldLabel);

            if (GUILayout.Button("Remove All Completed Sticky Notes in Scene"))
            {
                RemoveAllCompletedStickyNotes();
            }

            EditorGUILayout.Space(5);

            GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
            if (GUILayout.Button("Remove All Sticky Notes in Scene"))
            {
                if (EditorUtility.DisplayDialog(
                    "Remove All Sticky Notes",
                    "Are you sure you want to remove ALL AIStickyNote components from the scene?\n\nThis action cannot be undone.",
                    "Yes, Remove All",
                    "Cancel"))
                {
                    RemoveAllStickyNotes();
                }
            }
            GUI.backgroundColor = Color.white;
        }

        /// <summary>
        /// Removes all AIStickyNote components marked as completed from the current scene.
        /// Uses Undo system for editor undo support.
        /// </summary>
        private void RemoveAllCompletedStickyNotes()
        {
            var allNotes = Object.FindObjectsByType<AIStickyNote>(FindObjectsSortMode.None);
            var completedNotes = allNotes.Where(m => m.Completed).ToArray();

            if (completedNotes.Length == 0)
            {
                EditorUtility.DisplayDialog("No Completed Notes", "There are no completed AIStickyNote components in the scene.", "OK");
                return;
            }

            Undo.SetCurrentGroupName("Remove Completed Sticky Notes");
            int undoGroup = Undo.GetCurrentGroup();

            int removedCount = 0;
            foreach (var note in completedNotes)
            {
                Undo.DestroyObjectImmediate(note);
                removedCount++;
            }

            Undo.CollapseUndoOperations(undoGroup);
            Debug.Log($"[AIStickyNotes] Removed {removedCount} completed AIStickyNote component(s) from scene.");
        }

        /// <summary>
        /// Removes all AIStickyNote components from the current scene.
        /// Uses Undo system for editor undo support.
        /// </summary>
        private void RemoveAllStickyNotes()
        {
            var allNotes = Object.FindObjectsByType<AIStickyNote>(FindObjectsSortMode.None);

            if (allNotes.Length == 0)
            {
                EditorUtility.DisplayDialog("No Sticky Notes", "There are no AIStickyNote components in the scene.", "OK");
                return;
            }

            Undo.SetCurrentGroupName("Remove All Sticky Notes");
            int undoGroup = Undo.GetCurrentGroup();

            int removedCount = 0;
            foreach (var note in allNotes)
            {
                Undo.DestroyObjectImmediate(note);
                removedCount++;
            }

            Undo.CollapseUndoOperations(undoGroup);
            Debug.Log($"[AIStickyNotes] Removed {removedCount} AIStickyNote component(s) from scene.");
        }
    }
}

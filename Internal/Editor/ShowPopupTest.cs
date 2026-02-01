using UnityEngine;
using UnityEditor;
using AIStickyNotes.Internal;

namespace AIStickyNotes.Editor.Internal
{
    /// <summary>
    /// Test script to preview the sticky note popup visual design.
    /// </summary>
    public static class ShowPopupTest
    {
        /// <summary>
        /// Shows the sticky note popup in non-modal mode for visual iteration.
        /// Creates a temporary GameObject as the target.
        /// </summary>
        public static void Execute()
        {
            // Create a temporary test object if nothing is selected
            GameObject target = Selection.activeGameObject;
            if (target == null)
            {
                target = new GameObject("TestStickyNoteTarget");
                Selection.activeGameObject = target;
                Debug.Log("[ShowPopupTest] Created temporary test GameObject");
            }

            // Show the popup in non-modal mode
            AIStickyNotePopup.ShowWithoutDelay(target);
            Debug.Log("[ShowPopupTest] Popup shown - iterate on the visual design!");
        }

        /// <summary>
        /// Shows the exists popup in non-modal mode for visual iteration.
        /// Creates a temporary GameObject with a sticky note as the target.
        /// </summary>
        public static void ExecuteExists()
        {
            // Create a temporary test object with existing note
            GameObject target = Selection.activeGameObject;
            AIStickyNote existingNote = null;

            if (target != null)
            {
                existingNote = target.GetComponent<AIStickyNote>();
            }

            if (target == null || existingNote == null)
            {
                target = new GameObject("TestStickyNoteTarget");
                existingNote = target.AddComponent<AIStickyNote>();
                var so = new SerializedObject(existingNote);
                so.FindProperty("aiMessage").stringValue = "This is an existing sticky note message for testing.";
                so.FindProperty("readPriority").intValue = 5;
                so.ApplyModifiedProperties();
                Selection.activeGameObject = target;
                Debug.Log("[ShowPopupTest] Created temporary test GameObject with existing note");
            }

            // Show the exists popup in non-modal mode
            AIStickyNoteExistsPopup.ShowWithoutDelay(target, existingNote);
            Debug.Log("[ShowPopupTest] Exists popup shown - iterate on the visual design!");
        }
    }
}

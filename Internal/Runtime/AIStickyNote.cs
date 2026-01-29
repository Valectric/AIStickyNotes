using UnityEngine;

namespace AIStickyNotes.Internal
{
    /// <summary>
    /// A MonoBehaviour component that holds AI-readable notes attached to GameObjects.
    /// Used to provide context and instructions to AI agents working in the Unity scene.
    /// Notes can be read by priority (lower values read first) and marked as completed.
    /// </summary>
    public class AIStickyNote : MonoBehaviour
    {
        /// <summary>
        /// The message content for the AI to read.
        /// </summary>
        [SerializeField]
        [TextArea(3, 10)]
        private string aiMessage;

        /// <summary>
        /// Priority order for reading notes. Lower values are read first.
        /// </summary>
        [SerializeField]
        private int readPriority;

        /// <summary>
        /// Response message from the AI after processing this note.
        /// </summary>
        [SerializeField]
        [TextArea(3, 10)]
        private string responseMessage;

        /// <summary>
        /// Whether the AI has completed the task described in this note.
        /// </summary>
        [SerializeField]
        private bool completed;

        /// <summary>
        /// Gets the AI message content. Read-only to preserve original instructions.
        /// </summary>
        public string Message => aiMessage;

        /// <summary>
        /// Gets the read priority. Lower values indicate higher priority.
        /// </summary>
        public int ReadPriority => readPriority;

        /// <summary>
        /// Gets or sets the AI's response message after processing this note.
        /// </summary>
        public string ResponseMessage { get => responseMessage; set => responseMessage = value; }

        /// <summary>
        /// Gets or sets whether this note's task has been completed.
        /// </summary>
        public bool Completed { get => completed; set => completed = value; }
    }
}

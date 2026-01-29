using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using MCPForUnity.Editor.Helpers;
using MCPForUnity.Editor.Tools;
using AIStickyNotes.Internal;

namespace AIStickyNotes.Editor
{
    /// <summary>
    /// MCP tool for managing AIStickyNote components in the Unity scene.
    /// Provides actions to get all notes sorted by priority and to respond to specific notes.
    /// </summary>
    [McpForUnityTool(
        "read_sticky_notes",
        Description = "Manage AIStickyNote components in the scene. Actions: 'get_all' (default) returns notes sorted by priority with paths; 'respond' adds a response to a specific note by path."
    )]
    public static class ReadStickyNotes
    {
        /// <summary>
        /// Parameter class for the read_sticky_notes MCP tool.
        /// Defines the input parameters for getting and responding to sticky notes.
        /// </summary>
        public class Parameters
        {
            /// <summary>
            /// The action to perform. Valid values: 'get_all' (default) or 'respond'.
            /// </summary>
            [ToolParameter("Action: 'get_all' (default) or 'respond'")]
            public string action { get; set; }

            /// <summary>
            /// The GameObject hierarchy path for the 'respond' action.
            /// Format: 'Root/Parent/Child'. Required for 'respond' action.
            /// </summary>
            [ToolParameter("GameObject path for 'respond' action (e.g., 'Root/Parent/Child')", Required = false)]
            public string path { get; set; }

            /// <summary>
            /// The response message to store on the sticky note.
            /// Used with the 'respond' action to record AI feedback.
            /// </summary>
            [ToolParameter("Response message to store on the sticky note (for 'respond' action)", Required = false)]
            public string response_message { get; set; }

            /// <summary>
            /// Whether to mark the sticky note as completed.
            /// Used with the 'respond' action.
            /// </summary>
            [ToolParameter("Mark the sticky note as completed (for 'respond' action)", Required = false)]
            public bool? completed { get; set; }
        }

        /// <summary>
        /// Handles incoming MCP commands for sticky note operations.
        /// </summary>
        /// <param name="params">JSON parameters containing action and optional path/response data.</param>
        /// <returns>A success or error response object.</returns>
        public static object HandleCommand(JObject @params)
        {
            var action = @params["action"]?.ToString()?.ToLower() ?? "get_all";

            return action switch
            {
                "get_all" => GetAllNotes(),
                "respond" => RespondToNote(@params),
                _ => new ErrorResponse($"Unknown action: {action}. Valid actions: get_all, respond")
            };
        }

        /// <summary>
        /// Retrieves all AIStickyNote components in the scene, sorted by read priority.
        /// </summary>
        /// <returns>A success response with the list of notes and their properties.</returns>
        private static object GetAllNotes()
        {
            var stickyNotes = Object.FindObjectsByType<AIStickyNote>(FindObjectsSortMode.None);

            if (stickyNotes == null || stickyNotes.Length == 0)
            {
                return new SuccessResponse("No AIStickyNote components found in scene", new { notes = new List<object>() });
            }

            var notes = stickyNotes
                .OrderBy(m => m.ReadPriority)
                .Select(m => new
                {
                    path = GetGameObjectPath(m.gameObject),
                    message = m.Message,
                    priority = m.ReadPriority,
                    response = m.ResponseMessage,
                    completed = m.Completed
                })
                .ToList();

            return new SuccessResponse($"Found {notes.Count} sticky note(s)", new {
                notes,
                hint = "After completing each task, use action='respond' with path, response_message, and completed=true (or false if issues). Example: {\"action\":\"respond\",\"path\":\"Cube\",\"response_message\":\"Scaled 10x\",\"completed\":true}"
            });
        }

        /// <summary>
        /// Adds a response to a specific sticky note identified by its GameObject path.
        /// </summary>
        /// <param name="params">JSON parameters containing path, response_message, and completed flag.</param>
        /// <returns>A success or error response.</returns>
        private static object RespondToNote(JObject @params)
        {
            var path = @params["path"]?.ToString();
            if (string.IsNullOrEmpty(path))
            {
                return new ErrorResponse("'path' parameter is required for 'respond' action");
            }

            var responseMessage = @params["response_message"]?.ToString();
            var completed = @params["completed"]?.ToObject<bool>() ?? false;

            var stickyNote = FindStickyNoteByPath(path);
            if (stickyNote == null)
            {
                return new ErrorResponse($"No AIStickyNote found at path: {path}");
            }

            stickyNote.ResponseMessage = responseMessage;
            stickyNote.Completed = completed;

            UnityEditor.EditorUtility.SetDirty(stickyNote);

            return new SuccessResponse($"Response added to sticky note at '{path}'", new
            {
                path,
                message = stickyNote.Message,
                response = stickyNote.ResponseMessage,
                completed = stickyNote.Completed
            });
        }

        /// <summary>
        /// Finds an AIStickyNote component by its GameObject hierarchy path.
        /// </summary>
        /// <param name="targetPath">The full hierarchy path (e.g., 'Root/Parent/Child').</param>
        /// <returns>The matching AIStickyNote or null if not found.</returns>
        private static AIStickyNote FindStickyNoteByPath(string targetPath)
        {
            var stickyNotes = Object.FindObjectsByType<AIStickyNote>(FindObjectsSortMode.None);

            foreach (var note in stickyNotes)
            {
                if (GetGameObjectPath(note.gameObject) == targetPath)
                {
                    return note;
                }
            }

            return null;
        }

        /// <summary>
        /// Builds the full hierarchy path for a GameObject.
        /// </summary>
        /// <param name="go">The GameObject to get the path for.</param>
        /// <returns>The full path from root to the GameObject (e.g., 'Root/Parent/Child').</returns>
        private static string GetGameObjectPath(GameObject go)
        {
            var path = go.name;
            var parent = go.transform.parent;

            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}

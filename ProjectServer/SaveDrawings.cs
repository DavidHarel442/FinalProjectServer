using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectServer
{
    /// <summary>
    /// Manages saving, loading, and managing drawing files for users.
    /// Provides functionality to store drawing images in a file system organized by username.
    /// </summary>
    public class SaveDrawings
    {
        /// <summary>
        /// The base directory path where all user drawings are stored
        /// </summary>
        private readonly string _baseSavePath;

        /// <summary>
        /// Initializes a new instance of the SaveDrawings class.
        /// Creates the base directory if it doesn't exist.
        /// </summary>
        /// <param name="baseSavePath">
        /// The base directory path to store all user drawings.
        /// Defaults to "UserDrawings" in the application directory.
        /// </param>
        public SaveDrawings(string baseSavePath = "UserDrawings")
        {
            _baseSavePath = baseSavePath;
            EnsureBaseDirectoryExists();
        }

        /// <summary>
        /// Ensures that the base directory for storing drawings exists.
        /// Creates the directory if it doesn't exist.
        /// </summary>
        private void EnsureBaseDirectoryExists()
        {
            if (!Directory.Exists(_baseSavePath))
            {
                Directory.CreateDirectory(_baseSavePath);
            }
        }

        /// <summary>
        /// Gets or creates the user-specific directory for storing drawings.
        /// Each user has their own subdirectory under the base path.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <returns>The full path to the user's drawing directory</returns>
        private string GetUserDirectory(string username)
        {
            string userDir = Path.Combine(_baseSavePath, username);
            if (!Directory.Exists(userDir))
            {
                Directory.CreateDirectory(userDir);
            }
            return userDir;
        }

        /// <summary>
        /// Saves a drawing for a specific user. If a drawing with the same name already exists,
        /// it will be replaced with the new drawing.
        /// </summary>
        /// <param name="username">Username who owns the drawing</param>
        /// <param name="drawingName">Name of the drawing</param>
        /// <param name="imageData">Base64 encoded image data</param>
        /// <returns>True if saved successfully, false if an error occurred</returns>
        /// <remarks>
        /// The drawing is saved as a PNG file named after the drawingName parameter.
        /// The file is stored in the user's directory under the base save path.
        /// </remarks>
        public bool SaveDrawing(string username, string drawingName, string imageData)
        {
            try
            {
                string userDir = GetUserDirectory(username);
                Console.WriteLine($"Saving drawing in directory: {userDir}");

                // Save the image data
                string imagePath = Path.Combine(userDir, $"{drawingName}.png");
                Console.WriteLine($"Full image path: {imagePath}");

                // Check if file already exists and report that we're replacing it
                if (File.Exists(imagePath))
                {
                    Console.WriteLine($"A drawing with name '{drawingName}' already exists and will be replaced");
                }

                byte[] imageBytes = Convert.FromBase64String(imageData);
                File.WriteAllBytes(imagePath, imageBytes);

                Console.WriteLine($"Drawing saved successfully: {imagePath}, size: {imageBytes.Length} bytes");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving drawing: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Loads a drawing for a specific user.
        /// Retrieves the image file and converts it to Base64 string representation.
        /// </summary>
        /// <param name="username">Username who owns the drawing</param>
        /// <param name="drawingName">Name of the drawing to load</param>
        /// <returns>Base64 encoded image data or null if not found or an error occurred</returns>
        /// <remarks>
        /// The method looks for a PNG file with the specified drawing name
        /// in the user's directory under the base save path.
        /// </remarks>
        public string LoadDrawing(string username, string drawingName)
        {
            try
            {
                string userDir = GetUserDirectory(username);
                string imagePath = Path.Combine(userDir, $"{drawingName}.png");

                if (!File.Exists(imagePath))
                {
                    Console.WriteLine($"Drawing not found: {imagePath}");
                    return null;
                }

                byte[] imageBytes = File.ReadAllBytes(imagePath);
                string base64Image = Convert.ToBase64String(imageBytes);
                Console.WriteLine($"Drawing loaded: {imagePath}, size: {imageBytes.Length} bytes");
                return base64Image;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading drawing: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// Gets a list of all drawings for a user.
        /// Lists all PNG files in the user's directory.
        /// </summary>
        /// <param name="username">Username to get drawings for</param>
        /// <returns>List of drawing names without file extensions</returns>
        /// <remarks>
        /// The method returns the file names without the .png extension.
        /// Returns an empty list if the user has no drawings or if an error occurs.
        /// </remarks>
        public List<string> GetUserDrawings(string username)
        {
            List<string> drawings = new List<string>();
            string userDir = GetUserDirectory(username);

            try
            {
                foreach (string filePath in Directory.GetFiles(userDir, "*.png"))
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    drawings.Add(fileName);
                }

                Console.WriteLine($"Found {drawings.Count} drawings for user {username}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user drawings: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            return drawings;
        }

        /// <summary>
        /// Deletes a drawing for a specific user.
        /// Removes the drawing file from the user's directory.
        /// </summary>
        /// <param name="username">Username who owns the drawing</param>
        /// <param name="drawingName">Name of the drawing to delete</param>
        /// <returns>True if deleted successfully, false if the drawing doesn't exist or an error occurred</returns>
        /// <remarks>
        /// The method looks for a PNG file with the specified drawing name
        /// in the user's directory under the base save path and deletes it if found.
        /// </remarks>
        public bool DeleteDrawing(string username, string drawingName)
        {
            try
            {
                string userDir = GetUserDirectory(username);
                string imagePath = Path.Combine(userDir, $"{drawingName}.png");

                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                    Console.WriteLine($"Drawing deleted: {imagePath}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Drawing not found for deletion: {imagePath}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting drawing: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return false;
            }
        }
    }
}
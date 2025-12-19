using System;
using System.IO;
using System.Text;
using Zatca.EInvoice.Exceptions;

namespace Zatca.EInvoice.Helpers
{
    /// <summary>
    /// File I/O utility class for reading and writing files.
    /// </summary>
    public static class Storage
    {
        private static string _basePath = string.Empty;

        /// <summary>
        /// Gets or sets the base storage path.
        /// </summary>
        public static string BasePath
        {
            get => _basePath;
            set => _basePath = value?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) ?? string.Empty;
        }

        /// <summary>
        /// Writes data to a file, creating directories if necessary.
        /// </summary>
        /// <param name="path">Relative or full path of the file.</param>
        /// <param name="content">Content to write.</param>
        /// <exception cref="ZatcaStorageException">Thrown if the file cannot be written.</exception>
        public static void Write(string path, string content)
        {
            var fullPath = GetFullPath(path);
            var directory = Path.GetDirectoryName(fullPath) ?? string.Empty;

            EnsureDirectoryExists(directory);

            try
            {
                File.WriteAllText(fullPath, content, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new ZatcaStorageException("Failed to write to file.", new System.Collections.Generic.Dictionary<string, object>
                {
                    { "path", fullPath }
                }, ex);
            }
        }

        /// <summary>
        /// Appends data to a file, creating directories if necessary.
        /// </summary>
        /// <param name="path">Relative or full path of the file.</param>
        /// <param name="content">Content to append.</param>
        /// <exception cref="ZatcaStorageException">Thrown if the file cannot be written.</exception>
        public static void Append(string path, string content)
        {
            var fullPath = GetFullPath(path);
            var directory = Path.GetDirectoryName(fullPath) ?? string.Empty;

            EnsureDirectoryExists(directory);

            try
            {
                File.AppendAllText(fullPath, content, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new ZatcaStorageException("Failed to append to file.", new System.Collections.Generic.Dictionary<string, object>
                {
                    { "path", fullPath }
                }, ex);
            }
        }

        /// <summary>
        /// Reads content from a file.
        /// </summary>
        /// <param name="path">Relative or full path of the file.</param>
        /// <returns>The file contents.</returns>
        /// <exception cref="ZatcaStorageException">Thrown if the file does not exist or cannot be read.</exception>
        public static string Read(string path)
        {
            var fullPath = GetFullPath(path);

            if (!File.Exists(fullPath))
            {
                throw new ZatcaStorageException("File not found.", new System.Collections.Generic.Dictionary<string, object>
                {
                    { "path", fullPath }
                });
            }

            try
            {
                return File.ReadAllText(fullPath, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new ZatcaStorageException("Failed to read file.", new System.Collections.Generic.Dictionary<string, object>
                {
                    { "path", fullPath }
                }, ex);
            }
        }

        /// <summary>
        /// Checks if a file exists.
        /// </summary>
        /// <param name="path">Relative or full path of the file.</param>
        /// <returns>True if the file exists, false otherwise.</returns>
        public static bool Exists(string path)
        {
            var fullPath = GetFullPath(path);
            return File.Exists(fullPath);
        }

        /// <summary>
        /// Deletes a file if it exists.
        /// </summary>
        /// <param name="path">Relative or full path of the file.</param>
        /// <exception cref="ZatcaStorageException">Thrown if the file cannot be deleted.</exception>
        public static void Delete(string path)
        {
            var fullPath = GetFullPath(path);

            if (!File.Exists(fullPath))
            {
                return;
            }

            try
            {
                File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                throw new ZatcaStorageException("Failed to delete file.", new System.Collections.Generic.Dictionary<string, object>
                {
                    { "path", fullPath }
                }, ex);
            }
        }

        /// <summary>
        /// Returns the full path of a file.
        /// </summary>
        /// <param name="file">Relative or full path of the file.</param>
        /// <returns>Absolute path to the file.</returns>
        private static string GetFullPath(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException(nameof(file), "File path cannot be null or empty.");
            }

            if (!string.IsNullOrEmpty(BasePath))
            {
                return Path.Combine(BasePath, file);
            }

            return file;
        }

        /// <summary>
        /// Ensures the directory exists, creates it if needed.
        /// </summary>
        /// <param name="path">Directory path.</param>
        /// <exception cref="ZatcaStorageException">Thrown if the directory cannot be created.</exception>
        private static void EnsureDirectoryExists(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (Directory.Exists(path))
            {
                // Check if directory is writable
                try
                {
                    var testFile = Path.Combine(path, Path.GetRandomFileName());
                    using (File.Create(testFile, 1, FileOptions.DeleteOnClose))
                    {
                        // File created and will be auto-deleted on close
                    }
                }
                catch
                {
                    throw new ZatcaStorageException("Directory exists but is not writable.",
                        new System.Collections.Generic.Dictionary<string, object> { { "path", path } });
                }
                return;
            }

            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                throw new ZatcaStorageException("Failed to create directory.",
                    new System.Collections.Generic.Dictionary<string, object> { { "path", path } }, ex);
            }
        }
    }
}

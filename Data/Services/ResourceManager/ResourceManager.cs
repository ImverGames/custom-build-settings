using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ImverGames.CustomBuildSettings.Data
{
    /// <summary>
    /// A utility class for managing and accessing resources, such as icons, within the Unity Editor.
    /// </summary>
    public static class ResourceManager
    {
        private static string package = "com.imvergames.custombuildsettings";
        private static string basePath = $"/Packages/{package}/Resources/Icons";
        private static Dictionary<string, string> iconPathsCache = new Dictionary<string, string>();

        /// <summary>
        /// Retrieves a texture icon by name.
        /// </summary>
        /// <param name="iconName">The name of the icon to retrieve.</param>
        /// <returns>The icon texture if found; otherwise, null.</returns>
        public static Texture GetIcon(string iconName)
        {
            return EditorGUIUtility.TrIconContent(GetIconPath(iconName)).image;
        }
        
        public static Texture2D GetTexture2D(string iconName)
        {
            return EditorGUIUtility.Load(GetIconPath(iconName)) as Texture2D;
        }
        
        /// <summary>
        /// Creates a GUIContent with a title and an icon.
        /// </summary>
        /// <param name="iconName">The name of the icon to use.</param>
        /// <param name="title">The text title for the GUIContent.</param>
        /// <returns>A GUIContent with the specified icon and title.</returns>
        public static GUIContent GetContentWithTitle(string iconName, string title)
        {
            return EditorGUIUtility.TrTextContentWithIcon(title, GetIconPath(iconName));
        }
        
        /// <summary>
        /// Creates a GUIContent with a tooltip and an icon.
        /// </summary>
        /// <param name="iconName">The name of the icon to use.</param>
        /// <param name="tooltip">The tooltip text for the GUIContent.</param>
        /// <returns>A GUIContent with the specified icon and tooltip.</returns>
        public static GUIContent GetContentWithTooltip(string iconName, string tooltip)
        {
            return EditorGUIUtility.TrIconContent(GetIconPath(iconName), tooltip);
        }
        
        /// <summary>
        /// Creates a GUIContent with a title, tooltip, and an icon.
        /// </summary>
        /// <param name="iconName">The name of the icon to use.</param>
        /// <param name="title">The title text for the GUIContent.</param>
        /// <param name="tooltip">The tooltip text for the GUIContent.</param>
        /// <returns>A GUIContent with the specified title, tooltip, and icon.</returns>
        public static GUIContent GetContent(string iconName, string title, string tooltip)
        {
            return EditorGUIUtility.TrTextContentWithIcon(title, tooltip, GetIconPath(iconName));
        }
        
        /// <summary>
        /// Retrieves the file path of an icon by name, caching the result for future requests.
        /// </summary>
        /// <param name="iconName">The name of the icon to find the path for.</param>
        /// <returns>The icon file path if found; otherwise, an empty string.</returns>
        private static string GetIconPath(string iconName)
        {
            if (iconPathsCache.TryGetValue(iconName, out string cachedPath))
            {
                return cachedPath;
            }

            var standardPath = $"{basePath}/{iconName}.png";
            if (System.IO.File.Exists(standardPath))
            {
                iconPathsCache[iconName] = standardPath;
                return standardPath;
            }

            var searchPattern = $"{iconName}.png";
            var targetDirs = System.IO.Directory.GetDirectories(System.IO.Directory.GetCurrentDirectory(), "Icons", System.IO.SearchOption.AllDirectories);
            foreach (var dir in targetDirs)
            {
                var potentialPath = System.IO.Path.Combine(dir, searchPattern);
                if (System.IO.File.Exists(potentialPath))
                {
                    var relativePath = "Assets" + potentialPath.Substring(Application.dataPath.Length).Replace("\\", "/");
                    iconPathsCache[iconName] = relativePath;
                    return relativePath;
                }
            }

            Debug.LogWarning($"Unable to find {iconName} icon");
            return string.Empty;
        }
    }
}
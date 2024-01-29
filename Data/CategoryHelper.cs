using System.IO;

namespace ImverGames.CustomBuildSettings.Data
{
    public static class CategoryHelper
    {
        public static string DetermineCategory(string path)
        {
            string extension = Path.GetExtension(path).ToLower();

            switch (extension)
            {
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".tga":
                case ".bmp":
                case ".psd":
                case ".psb":
                case ".gif":
                case ".hdr":
                case ".exr":
                    return "Textures";

                case ".fbx":
                case ".obj":
                case ".3ds":
                case ".max":
                case ".blend":
                    return "Meshes";

                case ".anim":
                case ".controller":
                    return "Animations";

                case ".mp3":
                case ".wav":
                case ".ogg":
                    return "Sounds";

                case ".shader":
                    return "Shaders";

                case ".mat":
                    return "Materials";
                
                case ".prefab":
                    return "Prefabs";
                
                case ".asset":
                    return "Other Assets";

                case ".cs":
                    return "Scripts";

                case ".dll":
                    return "Included DLLs";

                default:
                    return "Unknown";
            }
        }
    }
}
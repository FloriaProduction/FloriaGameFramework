using System;
using static DotGL.GL;
using FloriaGF.Graphic;

namespace FloriaGF
{
    static class ImagesGF
    {
        static Dictionary<string, Image> _images = new();

        public static void loadImage(string name, string path)
        {
            if (_images.ContainsKey(name)) throw new Exception("Already exists");
            _images[name] = getImage(path);
        }

        public static Image getCacheImage(string name, string? path = null) {
            if (!_images.ContainsKey(name) && path != null)
                loadImage(name, path);
            return _images[name];
        }

        public static Image getImage(string path)
        {

            return new Image(path);
        }
    }
}

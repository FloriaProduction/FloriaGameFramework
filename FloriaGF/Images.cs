using System;
using static DotGL.GL;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace FloriaGF
{
    class Color
    {
        byte[] _data;
            

        public Color(byte[] data)
        {
            if (data.Length != 4) throw new Exception("Invalid data");
            _data = data;
        }
        public Color(byte r, byte g, byte b, byte a) : this([r, g, b, a]) { }


        public byte R
        {
            get { return _data[0]; }
        }
        public byte G
        {
            get { return _data[1]; }
        }
        public byte B
        {
            get { return _data[2]; }
        }
        public byte A
        {
            get { return _data[3]; }
        }
        public byte this[int index]
        {
            get { return this._data[index]; }
        }


        public static implicit operator byte[](Color color)
        {
            return color._data;
        }
        public static implicit operator Color(byte[] data)
        {
            return new Color(data);
        }
    }

    class Image
    {
        byte[] _pixels;
        uint _width, 
                _height;

        public Image()
        {
            _width = 0;
            _height = 0;
            _pixels = [];
        }
        public Image(uint width, uint height, byte[]? pixels = null)
        {
            _width = width;
            _height = height;

            if (pixels == null)
                _pixels = new byte[width * height * 4];
            else
                _pixels = pixels;
        }
        public Image(uint width, uint height, Color color) : this(width, height) 
        {
            List<byte> data = new();
            for (int i = 0; i < width * height; i++)
                data.AddRange((byte[])color);

            _pixels = [..data];
        }
        public Image(string path)
        {
            this.load(path);
        }

        /// <summary>
        /// Загрузить картинку из файла
        /// </summary>
        public void load(string path)
        {
            List<byte> pixels = [];

            using (var image_data = SixLabors.ImageSharp.Image.Load<Rgba32>(path))
            {
                _width = (uint)image_data.Width;
                _height = (uint)image_data.Height;

                for (int y = 0; y < _height; y++)
                    for (int x = 0; x < _width; x++)
                    {
                        Rgba32 pixel = image_data[x, y];
                        pixels.AddRange([pixel.R, pixel.G, pixel.B, pixel.A]);
                    }
            }
            _pixels = pixels.ToArray();
        }
        /// <summary>
        /// Вставить картинку в другую картинку
        /// </summary>
        public void paste(Image img, int xc, int yc)
        {
            for (int y = 0; y < img.Height; y++)
                for (int x = 0; x < img.Width; x++)
                {
                    int px = Math.Max(Math.Min(x + xc, this.Width), 0),
                        py = Math.Max(Math.Min(y + yc, this.Height), 0);

                    this.setPixel(px, py, img.getPixel(x, y));
                }
        }
        /// <summary>
        /// Сохранить картинку в файл
        /// </summary>
        public void save(string path)
        {
            if (Width == 0 || Height == 0) return;

            var simg = new SixLabors.ImageSharp.Image<Rgba32>(Width, Height);

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    Color color = getPixel(x, y);
                    simg[x, y] = new Rgba32(color.R, color.G, color.B, color.A);
                }

            simg.Save(path);
        }
        public byte[] getPixel(int x, int y)
        {
            return new ArraySegment<byte>(_pixels, this.Width * 4 * y + x * 4, 4).ToArray();
        }
        public void setPixel(int x, int y, Color color)
        {
            for (int i = 0; i < 4; i++)
                _pixels[this.Width * 4 * y  + x * 4 + i] = color[i];
        }
        public void fill(int x1, int y1, int x2, int y2, Color color)
        {
            for (int x = 0; x < x2-x1; x++)
                for (int y = 0; y < y2-y1; y++)
                    this.setPixel(x1 + x, y1 + y, color);
        }


        public int Width
        {
            get { return (int)_width; }
        }
        public int Height
        {
            get { return (int)_height; }
        }
        public byte[] Pixels
        {
            get { return _pixels; }
        }


        public static implicit operator Image(DotGLFW.Image img)
        {
            return new Image((uint)img.Width, (uint)img.Height, img.Pixels);
        }
        public static implicit operator DotGLFW.Image(Image img)
        {
            var rimg = new DotGLFW.Image();
            rimg.Width = img.Width;
            rimg.Height = img.Height;
            rimg.Pixels = img.Pixels;

            return rimg;
        }
    }

    /// <summary>
    /// Менеджер картинок
    /// </summary>
    static class ImagesGF
    {
        static Dictionary<string, Image> _images = new();

        /// <summary>
        /// Загрузить картинку в память
        /// </summary>
        /// <param name="name">Уникальное имя</param>
        public static void loadImage(string name, string path)
        {
            if (_images.ContainsKey(name)) throw new Exception("Already exists");
            _images[name] = getImage(path);
        }
        /// <summary>
        /// Получить картинку из памяти
        /// </summary>
        /// <param name="path">Автоматически загружает в память картинку, если её нет</param>
        public static Image getCacheImage(string name, string? path = null) {
            if (!_images.ContainsKey(name) && path != null)
                loadImage(name, path);
            return _images[name];
        }

        /// <summary>
        /// получить картинку из файла
        /// </summary>
        public static Image getImage(string path)
        {

            return new Image(path);
        }
    }
}

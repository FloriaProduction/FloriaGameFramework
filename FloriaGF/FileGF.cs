using System;
using System.Text.Json;
using DotGLFW;
using static DotGL.GL;
using System.Xml.Serialization;
using System.Xml.Linq;

namespace FloriaGF
{
    static class FileGF
    {
        public static string readFile(string path)
        {
            return File.ReadAllText(path);
        }

        public static void writeFile(string path, string value) 
        {
            string? directory = Path.GetDirectoryName(path);
            if (directory != null && directory.Length > 0)
                Directory.CreateDirectory(directory);
            File.WriteAllText(path, value);
        }
        /// <summary>
        /// Сериализовать Объект в xml-файла
        /// </summary>
        /// <typeparam name="T">Тип data, все методы и поля должны быть публичными</typeparam>
        static public void save<T>(string path, T data)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            {
                new XmlSerializer(typeof(T)).Serialize(fs, data);
            }
        }
        /// <summary>
        /// Десериализовать Объект из xml-файла
        /// </summary>
        static public object? load<T>(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open))
            {
                return new XmlSerializer(typeof(T)).Deserialize(fs);
            }
        }

        public static bool checkFile(string path)
        {
            return File.Exists(path);
        }
    
        public static JsonElement readJson(string path)
        {
            return JsonDocument.Parse(readFile(path)).RootElement;
        }

        public static void saveJson(string path, object data, bool indented = true)
        {
            FileGF.writeFile(
                path, 
                JsonSerializer.Serialize(data, 
                    new JsonSerializerOptions { 
                        WriteIndented = indented 
                    }
                )
            );
        }
    }
}

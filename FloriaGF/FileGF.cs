using System;
using DotGLFW;
using static DotGL.GL;
using System.Xml.Serialization;

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
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, value);
        }

        static public void save<T>(string path, T data)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            {
                new XmlSerializer(typeof(T)).Serialize(fs, data);
            }
        }

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
    }
}

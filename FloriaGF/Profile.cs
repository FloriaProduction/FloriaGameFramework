using System;
using DotGLFW;
using static DotGL.GL;

namespace FloriaGF
{
    static class Profile
    {
        static bool _fullscreen = false;
        static int _sync = 0;
        static uint _fps = 120;
        static uint _sps = 120;

        public static bool fullscreen
        {
            get { return _fullscreen; }
            set
            {
                _fullscreen = value;
                WindowGF.setFullscreen(_fullscreen);
            }
        }
        public static int sync
        {
            get { return _sync; }
            set
            {
                _sync = value;
                WindowGF.setInetval(_sync);
            }
        }
        public static uint fps
        {
            get { return _fps; }
            set
            {
                _fps = value;
            }
        }
        public static uint sps
        {
            get { return _sps; }
            set
            {
                _sps = value;
            }
        }


        public static Dictionary<string, object> settings
        {
            get
            {
                return new Dictionary<string, object>
                {
                    { "fullscreen",  fullscreen },
                    { "sync", sync },
                    { "fps", fps },
                };
            }
        }

        public static void save()
        {
            Dictionary<string, object> settings = Profile.settings;
            List<object[]> settings_list = [];

            foreach (string key in settings.Keys)
                settings_list.Add([key, settings[key]]);

            FileGF.save("settings.xml", settings_list.ToArray());

            Log.write("PROFILE", "saved");
        }

        public static void load()
        {
            if (!FileGF.checkFile("settings.xml"))
            {
                Log.write("PROFILE", "load default settings");
                return;
            }

            var settings_list = FileGF.load<object[][]>("settings.xml") as object[][] ?? throw new Exception("Cannot load profile settings!");
            foreach (object[] pair in settings_list)
            {
                string key = pair[0] as string ?? throw new Exception("Cannot convert to string");
                object value = pair[1];

                switch (key)
                {
                    case "fullscreen":
                        _fullscreen = Convert.ToBoolean(value);
                        break;
                    case "sync":
                        _sync = Convert.ToInt32(value);
                        break;
                    case "fps":
                        _fps = Convert.ToUInt32(value);
                        break;
                }
            }

            Log.write("PROFILE", "loaded");
        }
    }
}

using System;
using System.Text.Json;
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
        static bool _show_fps = false;

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
                WindowGF.setInterval(_sync);
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
        public static bool show_fps
        {
            get
            {
                return _show_fps;
            }
            set
            {
                _show_fps = value;
            }
        }

        public static Dictionary<string, object> settings
        {
            get
            {
                return new Dictionary<string, object>
                {
                    { "fullscreen",  fullscreen },
                };
            }
        }

        public static void save()
        {
            FileGF.saveJson("settings.json", new object[][] {
                ["fullscreen", Profile.fullscreen],
                ["show_fps", Profile.show_fps],
            });

            Log.write("saved", "PROFILE");
        }

        public static void load()
        {
            if (!FileGF.checkFile("settings.json"))
            {
                Log.write("load default settings", "PROFILE");
                return;
            }

            try
            {
                JsonElement settings = FileGF.readJson("settings.json");

                foreach (var keyval in settings.EnumerateArray())
                {
                    JsonElement value = keyval[1];

                    switch (keyval[0].GetString())
                    {
                        case "fullscreen":
                            _fullscreen = value.GetBoolean();
                            break;
                        case "show_fps":
                            _show_fps = value.GetBoolean();
                            break;
                    }
                }

                Log.write("loaded", "PROFILE");
            }
            catch (Exception e)
            {
                Log.write(e, "profile");
            }
            
        }
    }
}

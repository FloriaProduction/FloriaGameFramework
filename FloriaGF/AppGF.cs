//#define DISABLE_ERROR
//#define CREATE_LEVEL

using System;
using DotGLFW;
using FloriaGF.Graphic;
using static DotGL.GL;


namespace FloriaGF
{
    static class AppGF
    {
        static bool _app_enable = true;

        static uint _count_fps = 0, _count_sps = 0;

        static ulong _start_time = TimeGF.time();


        public static int Main() {

#if DISABLE_ERROR
            try
            {
                try
                {
#endif
                    AppGF.init();
#if CREATE_LEVEL
                    AppGF.createLevel();
#else
                    AppGF.simulation();
#endif

#if DISABLE_ERROR
                }
                catch (Exception e)
                {
                    Log.write(e);
                    Log.with_error = true;
                }
                finally
                {
#endif
            AppGF.term();
#if DISABLE_ERROR
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }
#endif

            return 0;
        }

        public static void zoomIn()
        {
            WindowGF.camera_scale *= 2;
        }
        public static void zoomOut()
        {
            WindowGF.camera_scale /= 2;
        }


        public static void init()
        {
            Profile.load();
            WindowGF.init(854, 480, "FloriaGameFramework");
            WindowGF.createBatch("background");
            WindowGF.createBatch("objects");
            WindowGF.createBatch("topground");
            WindowGF.createBatch("ui");

            if (WindowGF.window != null)
                KeysGF.init(WindowGF.window);


            AnimationManager.loadAnimations("data/animations/animations.json");


            // events

            KeysGF.createEvent("+move_forward", "world", Key.W, InputState.Press);
            KeysGF.createEvent("-move_forward", "world", Key.W, InputState.Release);

            KeysGF.createEvent("+move_left", "world", Key.A, InputState.Press);
            KeysGF.createEvent("-move_left", "world", Key.A, InputState.Release);

            KeysGF.createEvent("+move_back", "world", Key.S, InputState.Press);
            KeysGF.createEvent("-move_back", "world", Key.S, InputState.Release);

            KeysGF.createEvent("+move_right", "world", Key.D, InputState.Press);
            KeysGF.createEvent("-move_right", "world", Key.D, InputState.Release);

            KeysGF.createEvent("+zoom", "world", Key.Up, InputState.Press);
            KeysGF.createEvent("-zoom", "world", Key.Down, InputState.Press);

            KeysGF.createEvent("toggle_fullscreen", "world", Key.F11, InputState.Press);

            // binds

            KeysGF.bind("toggle_fullscreen", WindowGF.toggleFullscreen);
            KeysGF.bind("+zoom", zoomIn);
            KeysGF.bind("-zoom", zoomOut);

            ClassManager.init();

            /*Levels.init();

            Levels.LoadLevel("test");
            Levels.LoadLevel("test");*/

#if !CREATE_LEVEL
            World.loadLevel("main");
#endif

            Log.write("initialized", "APP");
        }

        public static void term()
        {
            WindowGF.term();
            Profile.save();

            Log.write("terminated", "APP");
            Log.save();
        }

        public static void close()
        {
            AppGF._app_enable = false;
            Log.write("closing...", "app");
        }

        public static void simulation()
        {

            while (!Glfw.WindowShouldClose(WindowGF.window))
            {
                // simulation
                if (TimeGF.everyTick("APP_simulation", (double)1000 / Profile.sps)) 
                {
                    Glfw.PollEvents();
                    World.simulation();
                    WindowGF.simulationBatches();

                    _count_sps++;
                }

                // render
                if (TimeGF.everyTick("APP_render", (double)1000 / Profile.fps)) 
                {
                    WindowGF.render();
                    _count_fps++;
                }

                //show_info
                if (Profile.show_fps && TimeGF.every("show_info", 1000))
                {
                    Log.write($"fps: {count_fps} (~{(double)1 / count_fps:f4}), sps: {count_sps} (~{(double)1 / count_sps:f4})", "app");
                    _count_sps = _count_fps = 0;
                }
            }
            AppGF.close();
        }

        public static void createLevel()
        {
            string[] level_names = ["main"];

            foreach (string level_name in level_names)
            {
                World.Clear();
                switch (level_name)
                {
                    case "main":
                        KeysGF.input_type = "world";
                        World.saved = true;

                        var camera = new Camera();

                        var moved_object = new MovedObject(new Pos(10, 0, 0));
                        camera.setTarget(moved_object);

                        var sprite_object = new SpriteObject(new Pos(5, 0, 0), AnimationManager.get("test_anim"), "objects");
                        break;
                }

                World.saveLevel(level_name);
            }
        }

        public static uint count_sps
        {
            get { return AppGF._count_sps; }
        }
        public static uint count_fps
        {
            get { return AppGF._count_fps; }
        }
        public static bool app_enable
        {
            get { return AppGF._app_enable; }
        }
        public static ulong start_time
        {
            get { return AppGF._start_time; }
        }
    }
}

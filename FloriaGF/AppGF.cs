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

            ImagesGF.loadImage("test", "data/images/test.png");
            ImagesGF.loadImage("test2", "data/images/test2.png");
            ImagesGF.loadImage("icon128_2", "data/images/icon128_2.png");
            ImagesGF.loadImage("numbers", "data/images/numbers.png");
            ImagesGF.loadImage("test_anim", "data/images/test_anim.png");
            ImagesGF.loadImage("test_anim2", "data/images/test_anim2.png");


            AnimationManager.create(new Animation("test", 1, 0, false));
            AnimationManager.create(new Animation(
                "test2", 
                1, 
                0, 
                false, 
                [16, 32], 
                new Dictionary<string, int[]>
                {
                    { "camera", [0, -32] }
                }
            ));
            AnimationManager.create(new Animation("test_anim", 2, 500, true, [16, 32]));

            // events

            KeysGF.createEvent("+move_forward", "world", Key.W, InputState.Press);
            KeysGF.createEvent("-move_forward", "world", Key.W, InputState.Release);

            KeysGF.createEvent("+move_left", "world", Key.A, InputState.Press);
            KeysGF.createEvent("-move_left", "world", Key.A, InputState.Release);

            KeysGF.createEvent("+move_back", "world", Key.S, InputState.Press);
            KeysGF.createEvent("-move_back", "world", Key.S, InputState.Release);

            KeysGF.createEvent("+move_right", "world", Key.D, InputState.Press);
            KeysGF.createEvent("-move_right", "world", Key.D, InputState.Release);

            KeysGF.createEvent("toggle_fullscreen", "world", Key.F11, InputState.Press);

            // binds

            KeysGF.bind("toggle_fullscreen", WindowGF.toggleFullscreen);

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
            WindowGF.camera_scale = 1;

            var spriteobject1 = new SpriteObject(new Pos(0, 0, 0), AnimationManager.get("test_anim"), "objects");
            var spriteobject2 = new SpriteObject(new Pos(0.5f, 0, -1), AnimationManager.get("test_anim"), "objects");
            var spriteobject3 = new SpriteObject(new Pos(1f, 0, -2), AnimationManager.get("test_anim"), "objects");


            var mobject = new MovedObject(new Pos(0, 0, 0));

            var camera = new Camera(new Pos(0, 0, 0));
            camera.scale = 100;
            camera.setTarget(mobject.uid);


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
                if (TimeGF.every("show_info", 1000))
                {
                    Log.write($"fps: {count_fps} (~{(double)1 / count_fps:f4}), sps: {count_sps} (~{(double)1 / count_sps:f4})", "app");
                    _count_sps = _count_fps = 0;
                }
            }
            AppGF.close();
        }

        public static void createLevel()
        {
            KeysGF.input_type = "world";
            World.saved = true;

            var anim = new Animation("test_anim", 2, 500, true);

            var sprite_obj = new SpriteObject(new Pos(0, 0, 0), anim, "objects");

            World.saveLevel("test");
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

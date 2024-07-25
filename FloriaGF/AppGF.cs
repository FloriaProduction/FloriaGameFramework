//#define DISABLE_ERROR

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

        public static int Main()
        {

#if DISABLE_ERROR
            try
            {
                try
                {
#endif
            AppGF.init();
                    AppGF.simulation();

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
            
            if (WindowGF.window != null)
                KeysGF.init(WindowGF.window);

            ImagesGF.loadImage("test", "data/images/test.png");
            ImagesGF.loadImage("test2", "data/images/test2.png");
            ImagesGF.loadImage("icon128_2", "data/images/icon128_2.png");
            ImagesGF.loadImage("numbers", "data/images/numbers.png");
            ImagesGF.loadImage("test_anim", "data/images/test_anim.png");
            ImagesGF.loadImage("test_anim2", "data/images/test_anim2.png");

            Log.write("APP", "initialized");
        }

        public static void term()
        {
            WindowGF.term();
            Profile.save();

            Log.write("APP", "terminated");
            Log.save();
        }

        public static void close()
        {
            AppGF._app_enable = false;
            Log.write("app", "closing...");
        }

        public static void simulation()
        {

            var batch = new Batch("test");

            var anim = new Animation("test_anim", 2, 500, true);

            var spriteobject = new ObjSprite(new Vec(0, 0, 0), anim, batch.name);

            var camera = new Camera(new Vec(0, 0, 0));
            camera.scale = 100;


            while (!Glfw.WindowShouldClose(WindowGF.window))
            {
                // simulation
                if (TimeGF.everyTick("APP_simulation", (double)1000 / Profile.sps)) 
                {
                    Glfw.PollEvents();
                    World.simulation();
                    WindowGF.simulationBatches();

                    camera.x = (float)(Math.Sin(TimeGF.time() * 0.002));
                    spriteobject.y = (float)(Math.Sin(TimeGF.time() * 0.001));

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
                    Log.write("app", $"fps: {count_fps} (~{(double)1 / count_fps:f4}), sps: {count_sps} (~{(double)1 / count_sps:f4})");
                    _count_sps = _count_fps = 0;
                }
            }
            AppGF.close();
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

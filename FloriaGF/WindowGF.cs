using System;
using System.Linq.Expressions;
using DotGLFW;
using FloriaGF.Graphic;
using static DotGL.GL;

namespace FloriaGF
{
    static class WindowGF
    {
        static Window _window;
        static Vidmode _screen;
        static DotGLFW.Monitor _monitor;

        static Vec _camera_position = new Vec(0, 0, 0);
        static Vec _camera_resolution = new Vec(1, 1);
        static float _camera_scale = 1;

        static Dictionary<string, Graphic.Batch> _batches = new();

        static bool _update_camera = false;


        public static void init(uint width, uint height, string title)
        {
            Glfw.Init();

            WindowGF._monitor = Glfw.GetPrimaryMonitor();

            WindowGF._screen = Glfw.GetVideoMode(WindowGF.monitor);
            if (screen == null) throw new Exception("Screen is null!");

            Glfw.WindowHint(WindowHint.Resizable, false);
            Glfw.WindowHint(WindowHint.CenterCursor, true);
            Glfw.WindowHint(WindowHint.DoubleBuffer, true);
            Glfw.WindowHint(WindowHint.Visible, false);

            WindowGF._window = Glfw.CreateWindow((int)width, (int)height, title, DotGLFW.Monitor.NULL, DotGLFW.Window.NULL);
            if (window == null) throw new Exception("Window is null!");

            Glfw.MakeContextCurrent(window);
            Import(Glfw.GetProcAddress);

            Glfw.SetWindowPos(window, (int)(screen.Width - width) / 2, (int)(screen.Height - height) / 2);
            Glfw.ShowWindow(window);
            Glfw.FocusWindow(window);

            Glfw.SetWindowIcon(window, [
                ImagesGF.getImage("data/icons/icon128.png"),
                ImagesGF.getImage("data/icons/icon48.png"),
                ImagesGF.getImage("data/icons/icon32.png"),
                ImagesGF.getImage("data/icons/icon16.png")
            ]);

            //glViewport(-(int)width/2, -(int)height/2, (int)width, -(int)(height));

            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
            glEnable(GL_BLEND);

            glEnable(GL_DEPTH_TEST);

            glActiveTexture(GL_TEXTURE0);

            glClearColor(0.8f, 0.8f, 0.8f, 0.1f);

            WindowGF.camera_resolution = [width, height];

            WindowGF.setFullscreen(Profile.fullscreen);

            Log.write("WINDOW", "initialized");
        }
        public static void term()
        {
            Log.write("WINDOW", "terminated");
        }
        public static void render()
        {
            if (WindowGF._update_camera) WindowGF._updateCamera();

            glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

            foreach (Batch batch in WindowGF._batches.Values)
            {
                batch.render();
            }

            Glfw.SwapBuffers(window);
        }
        public static void addBatch(string name, Batch batch)
        {
            if (WindowGF._batches.ContainsKey(name)) throw new Exception();
            WindowGF._update_camera = true;
            WindowGF._batches[name] = batch;
        }
        public static void deleteBatch(string name)
        {
            WindowGF._batches.Remove(name);
        }
        public static Batch getBatch(string name)
        {
            return WindowGF._batches[name];
        }
        private static void _updateCamera()
        {
            foreach (Batch batch in _batches.Values)
            {
                batch.position = WindowGF._camera_position;
                batch.scale = new Vec(1 / WindowGF.camera_resolution[0], 1 / WindowGF.camera_resolution[1], 1) 
                    * new Vec(WindowGF._camera_scale, WindowGF._camera_scale, WindowGF._camera_scale);
            }
        }

        /// <summary>
        /// 0 - nosync
        /// 1 - sync 
        /// 2 - sync / 2
        /// </summary>
        public static void setInetval(int value)
        {
            Glfw.SwapInterval(value);
        }

        public static void setFullscreen(bool value)
        { 
            Glfw.SetWindowMonitor(
                window, 
                value ? monitor : DotGLFW.Monitor.NULL, 

                value ? 0 : (screen.Width - (int)camera_resolution[0]) / 2, 
                value ? 0 : (screen.Height - (int)camera_resolution[1]) / 2, 
                
                value ? screen.Width : (int)camera_resolution[0], 
                value ? screen.Height : (int)camera_resolution[1], 
                
                0
            );
            glViewport(
                0, 0, 

                value ? screen.Width : (int)camera_resolution[0], 
                value ? screen.Height : (int)camera_resolution[1]
            );
            setInetval(Profile.sync);
        }

        public static void simulationBatches()
        {
            foreach (Batch batch in _batches.Values)
                batch.simulationSprites();
        }


        public static float[] camera_position
        {
            get
            {
                return WindowGF._camera_position;
            }
            set
            {
                if (value.Length != 3) throw new Exception();

                WindowGF._camera_position = new Vec(value[0], value[1], value[2]);
                WindowGF._updateCamera();
            }
        }
        public static float[] camera_resolution 
        { 
            get
            {
                return WindowGF._camera_resolution;
            }
            set
            {
                if (value.Length != 2) throw new Exception();

                WindowGF._camera_resolution = new Vec(value);
                WindowGF._updateCamera();
            }
        }
        public static float camera_scale 
        { 
            get
            {
                return WindowGF._camera_scale;
            }
            set
            {
                WindowGF._camera_scale = value;
                WindowGF._updateCamera();
            }
        }
        public static Window window
        {
            get { return WindowGF._window; }
        }
        public static Vidmode screen
        {
            get { return WindowGF._screen; }
        }
        public static DotGLFW.Monitor monitor
        {
            get { return WindowGF._monitor; }
        }
    }
}

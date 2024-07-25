using DotGLFW;
using System;

namespace FloriaGF
{
    static class KeysGF
    {
        static void keycallback(Window window, DotGLFW.Key key, int scancode, InputState action, ModifierKey mods)
        {
            Log.write("keysgf", $"{key} {scancode} {action} {mods}");

            if (action != InputState.Release) return;

            switch (scancode)
            {
                case 328:
                    WindowGF.camera_scale *= 2f;
                    break;
                case 336:
                    WindowGF.camera_scale /= 2f;
                    break;
                case 87:
                    Profile.fullscreen = !Profile.fullscreen;
                    break;
            }
            //328 up 336 down
        }

        public static void init(DotGLFW.Window window)
        {
            Glfw.SetKeyCallback(window, keycallback);
        }
    }
}

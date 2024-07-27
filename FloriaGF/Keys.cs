using DotGLFW;
using Microsoft.VisualBasic.FileIO;
using System;

namespace FloriaGF
{
    public class EqualityComparerGF : IEqualityComparer<int[]>
    {
        public bool Equals(int[]? x, int[]? y)
        {
            if (x == null || y == null) return false;
            if (x.Length != y.Length) return false;
            for (int i = 0; i < x.Length; i++)
                if (x[i] != y[i])
                    return false;

            return true;
        }

        public int GetHashCode(int[] obj)
        {
            int result = 17;
            for (int i = 0; i < obj.Length; i++)
            {
                unchecked
                {
                    result = result * 23 + obj[i];
                }
            }
            return result;
        }
    }

    static class KeysGF
    {
        public delegate void delegateFunction();
        static Dictionary<int[], List<delegateFunction>> _callback_functions = new(new EqualityComparerGF());

        static int[] getFuncKey(DotGLFW.Key key, InputState action, ModifierKey mods)
        {
            return [
                Convert.ToInt32(key),
                Convert.ToInt32(action),
                Convert.ToInt32(mods)
            ];
        }

        public static void regFunction(DotGLFW.Key key, InputState action, ModifierKey mods, delegateFunction function)
        {
            int[] func_key = getFuncKey(key, action, mods);

            if (!_callback_functions.ContainsKey(func_key))
            {
                _callback_functions[func_key] = new List<delegateFunction>();
            }
            _callback_functions[func_key].Add(function);
        }


        static void keySimulation(Window window, DotGLFW.Key key, int scancode, InputState action, ModifierKey mods)
        {
            int[] func_key = getFuncKey(key, action, mods);

            if (!_callback_functions.ContainsKey(func_key)) return;

            foreach (delegateFunction func in _callback_functions[func_key])
            {
                func();
            }

            Log.write("keysgf", $"{Convert.ToInt32(key)} {scancode} {Convert.ToInt32(action)} {Convert.ToInt32(mods)}");
        }
        public static void init(DotGLFW.Window window)
        {
            Glfw.SetKeyCallback(window, keySimulation);
        }
    }
}

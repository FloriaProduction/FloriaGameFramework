using DotGLFW;
using Microsoft.VisualBasic.FileIO;
using System;
using System.ComponentModel;
using System.ComponentModel.Design;

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
        /// <summary>
        /// world - взаимодействие с миром;
        /// ui - взаимодействие с интерфейсом;
        /// </summary>
        static string _input_type = "world";

        public delegate void delegateCallbackFunction();

        /// <summary>
        /// event: dict[uint, func]
        /// </summary>
        static Dictionary<string, IDM<delegateCallbackFunction>> _events = new();

        /// <summary>
        /// input_mode: dict[event_key, IDM[event_names]]
        /// </summary>
        static Dictionary<string, Dictionary<int[], IDM<string>>> _events_map = new ();
        

        public static uint bind(string event_name, delegateCallbackFunction func)
        {
            if (!_events.ContainsKey(event_name))
                _events[event_name] = new IDM<delegateCallbackFunction>(true);

            uint func_id = _events[event_name].add(func);

            Log.write($"bind on '{event_name}' registered!", "keys");

            return func_id;
        }

        public static void createEvent(string name, string input_mode, DotGLFW.Key? key = null, InputState? action = null, ModifierKey? mods = null)
        {
            if (!_events_map.ContainsKey(input_mode))
                _events_map[input_mode] = new Dictionary<int[], IDM<string>>(new EqualityComparerGF());

            int[] event_key = [
                key.HasValue ? Convert.ToInt32(key) : -1,
                action.HasValue ? Convert.ToInt32(action) : -1,
                mods.HasValue ? Convert.ToInt32(mods) : -1,
            ];

            if (!_events_map[input_mode].ContainsKey(event_key))
                _events_map[input_mode][event_key] = new IDM<string>(true);

            if (_events_map[input_mode][event_key].Values.Contains(name))
                throw new Exception();
            else
                _events_map[input_mode][event_key].add(name);

            Log.write($"event '{name}' created!", "keys");
        }

        static void keyHandler(Window window, DotGLFW.Key _key, int scancode, InputState _action, ModifierKey _mods)
        {
            uint key = Convert.ToUInt32(_key);
            uint action = Convert.ToUInt32(_action);
            uint mods = Convert.ToUInt32(_mods);

            foreach (int[] event_key in _events_map[KeysGF.input_type].Keys.ToArray())
                if ((event_key[0] == -1 || event_key[0] == key) &&
                    (event_key[1] == -1 || event_key[1] == action) &&
                    (event_key[2] == -1 || event_key[2] == mods))
                {
                    foreach (string event_name in _events_map[KeysGF.input_type][event_key].Values)
                    {
                        if (!_events.ContainsKey(event_name)) continue;
                        foreach (delegateCallbackFunction func in _events[event_name].Values)
                            func();
                    }
                }

            /*if (action != 2)
                Log.write($"key: {key} action:{action} mods:{mods}", "keys");*/
        }

        public static void init(DotGLFW.Window window)
        {
            Glfw.SetKeyCallback(window, keyHandler);
        }


        public static string input_type
        {
            get { return _input_type; }
            set
            {
                _input_type = value;
            }
        }
    }
}

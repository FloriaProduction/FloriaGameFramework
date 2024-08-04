using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json;
using DotGLFW;
using FloriaGF.Graphic;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static DotGL.GL;

namespace FloriaGF
{
    /// <summary>
    /// Базовый интерфейс объекта для World
    /// </summary>
    public interface BaseObjectInterface
    {
        public void simulation();
        public void loaded();

        public uint id { get; }
        public string uid { get; }
        public bool simulated { get; }

        public Dictionary<string, object?> serialization();
        public void deserialization(JsonElement data);

        public string className { get; }

        //public string ToString();

        //public string layer { get; }
        /*public bool rendered { get; }
        public bool collided { get; }*/
    }

    /// <summary>
    /// Базовый класс для объектов.
    /// 
    /// Установи поле _simulated = true и перепиши метод simulation, если вам нужна симуляция объекта
    /// 
    /// Перезапиши метод setPosition, если вам нужно отслеживать изменение позиции
    /// 
    /// Дополни методы getVariables и setVariables для корректной сериализации новых полей 
    /// </summary>
    public class BaseObject : BaseObjectInterface
    {
        uint _id;
        string _uid;
        Pos _position;
        protected bool _simulated = false;


        public BaseObject(Pos pos)
        {
            if (pos.Length != 3) throw new Exception();
            _position = pos;

            object[] keys = World.addObject(this);

            _id = Convert.ToUInt32(keys[0]);
            _uid = keys[1] as string ?? throw new Exception();
        }
        public BaseObject() : this(new Pos(0, 0, 0)) { }

        public virtual void setPosition(Pos pos)
        {
            _position = pos;
        }
        public void addPosition(Pos add_pos)
        {
            this.setPosition(this._position + add_pos);
        }

        public virtual void simulation()
        {

        }
        public virtual void loaded()
        {

        }

        protected virtual Dictionary<string, object?> getVariables()
        {
            return new Dictionary<string, object?>
            {
                { "class_name", this.className },
                { "simulated", _simulated },
                { "position", _position.ToArray() },
                { "uid", _uid },
            };
        }
        protected virtual void setVariables(JsonElement variables)
        {
            _simulated = Func.getProperty(variables, "simulated").GetBoolean();
            _position = new Pos((from number in Func.getProperty(variables, "position").EnumerateArray() select (float)number.GetDouble()).ToArray());
            _uid = Func.getProperty(variables, "uid").ToString();
        }

        public Dictionary<string, object?> serialization()
        {
            return getVariables();
        }
        public void deserialization(JsonElement data)
        {
            setVariables(data);
            Log.write("deserialized", this.ToString());
        }

        public override string ToString()
        {
            return $"BaseObject({this.position})";
        }


        public uint id
        {
            get
            {
                return _id;
            }
        }
        public string uid
        {
            get
            {
                return _uid;
            }
        }
        public Pos position
        {
            get
            {
                return _position;
            }
            set
            {
                this.setPosition(value);
            }
        }
        public float x
        {
            get
            {
                return _position.x;
            }
            set
            {
                this.setPosition(new Pos(value, _position.y, _position.z));
            }
        }
        public float y
        {
            get
            {
                return _position.y;
            }
            set
            {
                this.setPosition(new Pos(_position.x, value, _position.z));
            }
        }
        public float z
        {
            get
            {
                return _position.z;
            }
            set
            {
                this.setPosition(new Pos(_position.x, _position.y, value));
            }
        }
        public bool simulated { get { return _simulated; } }
        public string className
        {
            get
            {
                return this.GetType().Name;
            }
        }
    }
    
   
    public static class World
    {
        static IDM<BaseObjectInterface> _objects = new(true);
        static bool _update_groups = false;
        static uint[] _simulation_group = [];
        
        static bool _saved = false;

        static string[] _args = [];

        public static string generateKey()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>[ id(uint), uid(string) ]</returns>
        public static object[] addObject(BaseObjectInterface obj)
        {
            _update_groups = true;
            return [_objects.add(obj), generateKey()];
        }
        public static void  removeObject(uint id)
        {
            _update_groups = true;
            _objects.remove(id);
        }
        public static BaseObjectInterface get(uint id)
        {
            return _objects.get(id);
        }
        public static BaseObjectInterface get(string uid)
        {
            foreach (BaseObjectInterface obj in _objects.Values) 
                if (obj.uid == uid)
                    return obj;
            throw new Exception();
        }

        public static void updateGroups()
        {
            _update_groups = true;
        }
        public static void _updateGroups()
        {
            // simulation
            List<uint> ids_sim_grp = [];
            foreach (BaseObjectInterface obj in _objects.Values)
                if (obj.simulated)
                    ids_sim_grp.Add(obj.id);
            _simulation_group = ids_sim_grp.ToArray();

            _update_groups = false;
        }

        public static void simulation()
        {
            if (_update_groups) World._updateGroups();

            foreach (uint id in _simulation_group)
                _objects[id].simulation();
        }

        public static void Clear(string[]? args = null)
        {
            _objects.Clear();
            _update_groups = true;
            _simulation_group = [];
            _saved = false;
            _args = args ?? [];
        }

        /*public static T getInstance<T>() where T : BaseObjectInterface
        {
            return new T();
        }*/

        public static void loadLevel(string name, string[]? args = null)
        {
            Log.write($"loading level '{name}'...", "world");

            // args
            _args = args ?? [];

            // load data

            JsonElement data = FileGF.readJson($"data/levels/{name}/data.json");


            // apply settings

            JsonElement settings = Func.getProperty(data, "settings");

            Log.write(settings.ToString());

            _saved = Func.getProperty(settings, "saved").GetBoolean();
            KeysGF.input_type = Func.getProperty(settings, "input_type").ToString();
            WindowGF.deleteAllBatches();
            foreach (var batch_name in Func.getProperty(settings, "batches").EnumerateArray())
                WindowGF.createBatch(batch_name.ToString());


            // objects 

            var objects = Func.getProperty(data, "objects").EnumerateArray();
            World.Clear();
            foreach (var obj in objects)
            {
                string class_name = Func.getProperty(obj, "class_name").ToString();

                BaseObjectInterface loaded_object = ClassManager.createInstance(class_name);
                loaded_object.deserialization(obj);
                loaded_object.loaded();
            }

            var batches = WindowGF.getBatches();

            World.updateGroups();

            Log.write("loading complete!", "world");
        }

        public static void saveLevel(string name)
        {
            Log.write($"saving level '{name}'...", "world");


            // settings
            Dictionary<object, object> settings = new();
            settings["saved"] = _saved;
            settings["input_type"] = KeysGF.input_type;
            settings["batches"] = WindowGF.getBatches();


            // objects
            List<Dictionary<string, object?>> objects = new();
            foreach (BaseObjectInterface obj in _objects.Values)
            {
                objects.Add(obj.serialization());
            }


            // save
            Dictionary<object, object> data = new();
            data["objects"] = objects.ToArray();
            data["settings"] = settings;

            FileGF.saveJson($"data/levels/{name}/data.json", data);

            Log.write("saving complete!", "world");
        }

        public static string[] args
        {
            get
            {
                return World._args;
            }
        }
        public static bool saved
        {
            get { return _saved; }
            set
            {
                _saved = value;
            }
        }
    }

    /*public static class Levels
    {
        public delegate void delegateLoadLevel(string name, string[] args);

        static Dictionary<string, delegateLoadLevel> _levels = new();

        public static void LoadLevel(string name, string[]? args = null)
        {
            World.Clear(args);
            _levels[name](name, args ?? []);
        }
        public static void registerLevel(string name, delegateLoadLevel func)
        {
            _levels[name] = func;
        }
    
        public static void init()
        {
            Levels.registerLevel("test", test_lvl);
        }

        static void test_lvl(string name, string[] args)
        {
            var camera = new Camera(new Pos(0, 0, 0));

            var moved_object = new MovedObject(new Pos(0, 0, 0));

            camera.setTarget(moved_object.uid);
            camera.scale = 100;

            var sprite_object = new SpriteObject(new Pos(0, 0, 0), AnimationManager.get("test_anim"), "objects");
        }
    }*/
}

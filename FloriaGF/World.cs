using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using DotGLFW;
using FloriaGF.Graphic;
using static DotGL.GL;

namespace FloriaGF
{
    /// <summary>
    /// Базовый интерфейс объекта для World
    /// </summary>
    interface BaseObjectInterface
    {

        public void simulation();

        public uint id { get; }
        public string uid { get; }
        public bool simulated { get; }
        //public string layer { get; }
        /*public bool rendered { get; }
        public bool collided { get; }*/
    }

    /// <summary>
    /// Базовый класс для объектов.
    /// 
    /// Установи поле _simulated = true и перепиши метод simulation, если вам нужна симуляция объекта
    /// 
    /// Перезапиши:
    ///     - метод setPosition, если вам нужно отслеживать изменение позиции
    /// </summary>
    class BaseObject : BaseObjectInterface
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
    }
    
   
    static class World
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
            if (_update_groups) World.updateGroups();

            foreach (uint id in _simulation_group)
                _objects[id].simulation();
        }

        public static void loadLevel(string name, string[]? args = null)
        {
            try
            {
                Log.write($"loading level '{name}'...", "world");

                // args
                if (args == null)
                    _args = [];
                else _args = args;

                // apply settings
                JsonElement settings = FileGF.readJson($"data/levels/{name}/settings.json");

                _saved = settings.GetProperty("saved").GetBoolean();
                Log.write($"level saved: {_saved}");

                KeysGF.input_type = settings.GetProperty("input_type").ToString();
                Log.write($"input type: {KeysGF.input_type}");

                Log.write("loading complete!", "world");
            }
            catch (Exception e)
            {
                Log.write(e);

                _args = [];
                _saved = false;
                KeysGF.input_type = "world";
            }
        }

        public static void saveLevel(string name)
        {
            Log.write($"saving level '{name}'...", "world");

            Dictionary<object, object> settings = new();
            settings["saved"] = _saved;
            settings["input_type"] = KeysGF.input_type;

            FileGF.saveJson($"data/levels/{name}/settings.json", settings);

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
}

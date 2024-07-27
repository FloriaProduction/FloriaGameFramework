using System;
using System.Runtime.CompilerServices;
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

        public uint id { get; set; }
        public bool simulated { get; }
        //public string layer { get; }
        /*public bool rendered { get; }
        public bool collided { get; }*/
    }

    /// <summary>
    /// Базовый класс для объектов.
    /// 
    /// Установи поле _simulated = true, если вам нужна симуляция объекта
    /// 
    /// Перезапиши:
    ///     - метод setPosition, если вам нужно отслеживать изменение позиции
    /// </summary>
    class BaseObject : BaseObjectInterface
    {
        uint _id;
        Vec _position;
        protected bool _simulated = false;
        public BaseObject(Vec pos)
        { 
            _position = pos;
            _id = World.addObject(this);
        }

        public virtual void setPosition(Vec pos)
        {
            if (pos.Length != 3) throw new Exception();
            _position = pos;
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
            set
            {
                _id = value;
            }
        }
        
        /// <summary>
        /// Vec(3)
        /// </summary>
        public Vec position
        {
            get
            {
                return _position.Copy();
            }
            set
            {
                if (value.Length != 3) throw new Exception();
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
                this.setPosition(new Vec(value, _position.y, _position.z));
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
                this.setPosition(new Vec(_position.x, value, _position.z));
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
                this.setPosition(new Vec(_position.x, _position.y, value));
            }
        }
        public bool simulated { get { return _simulated; } }
    }
    
   
    static class World
    {
        static IDM<BaseObjectInterface> _objects = new(true);
        static bool _update_groups = false;
        static uint[] _simulation_group = [];

        public static uint addObject(BaseObjectInterface obj)
        {
            _update_groups = true;
            return _objects.add(obj);
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
    }
}

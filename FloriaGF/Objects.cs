using FloriaGF.Graphic;
using System;
using System.ComponentModel.Design;
using System.Text.Json;

namespace FloriaGF
{
    public static class ClassManager
    {
        static Dictionary<string, Type> _classes = new(); 

        public static void registerClass(string name, Type obj_class)
        {
            if (_classes.ContainsKey(name)) return;
            _classes[name] = obj_class;
        }

        public static BaseObjectInterface createInstance(string name, object[]? args = null)
        {
            Type type = Type.GetType($"FloriaGF.{name}") ?? throw new Exception("Класс не найден");
            return (Activator.CreateInstance(type, args) ?? throw new Exception("Не удалось создать экземпляр класс")) as BaseObjectInterface ?? throw new Exception("Не удалось привести экземпляр класса к BaseObjectInterface");
        }

        public static void init()
        {
            ClassManager.registerClass("Camera", typeof(Camera));
            ClassManager.registerClass("SpriteObject", typeof(SpriteObject));
            ClassManager.registerClass("MovedObject", typeof(MovedObject));
        }
    }

    public class Camera : BaseObject
    {
        string? _target = null;
        float _strength = 0.025f;


        public Camera(Pos pos, string? target = null) : base(pos) 
        {
            _simulated = true;
            _target = target;
        }
        public Camera() : this(new Pos(0, 0, 0), null) { }


        public void setTarget(string uid)
        {
            _target = uid;
        }
        public void setTarget(BaseObjectInterface obj)
        {
            this.setTarget(obj.uid);
        }
        public override void setPosition(Pos pos)
        {
            base.setPosition(pos);

            if (_target != null)
            {
                MovedObject? obj = World.get(_target) as MovedObject;
                if (obj != null)
                {
                    Animation? animation = obj.sprite.animation;
                    int[] point_camera = animation == null ? [0, 0] : animation.getPoint("camera");

                    WindowGF.camera_position = new Pos(
                        pos.x + (float)point_camera[0] / 32,
                        pos.y + (float)point_camera[1] / 32,
                        pos.z
                    );
                }
            }
            else
            {
                WindowGF.camera_position = pos;
            }
        }
        public override void simulation()
        {
            if (_target != null)
            {
                MovedObject? obj = World.get(_target) as MovedObject;
                if (obj == null) return;

                this.setPosition(new Pos(
                    Func.smooth(this.x, obj.x, _strength),
                    this.y,
                    Func.smooth(this.z, obj.z, _strength)
                ));
            }
        }


        protected override Dictionary<string, object?> getVariables()
        {
            return base.getVariables().Join(new Dictionary<string, object?>()
            {
                { "scale", WindowGF.camera_scale },
                { "target", this._target },
            });
        }
        protected override void setVariables(JsonElement variables)
        {
            base.setVariables(variables);
            this.scale = (float)Func.getProperty(variables, "scale").GetDouble();
            setTarget(Func.getProperty(variables, "target").ToString());
        }

        public override void loaded()
        {
            
        }

        public float scale
        {
            get
            {
                return WindowGF.camera_scale;
            }
            set
            {
                WindowGF.camera_scale = value;
            }
        }
    }

    public class SpriteObject : BaseObject
    {
        Sprite _sprite;

        public SpriteObject(Pos pos, Animation? animation, string batch_name) : base(pos)
        {
            _sprite = new Sprite(pos, animation, batch_name);
        }
        public SpriteObject(Pos pos, string animation_name, string batch_name) : this(pos, AnimationManager.get(animation_name), batch_name) { }
        public SpriteObject() : this(new Pos(0, 0, 0), "default", "objects") { }
        public override void setPosition(Pos pos)
        {
            base.setPosition(pos);
            _sprite.position = this.position;
        }

        protected override Dictionary<string, object?> getVariables()
        {
            return base.getVariables().Join(new Dictionary<string, object?>()
            {
                { "animation_name", _sprite.animation == null ? null : _sprite.animation.name},
                { "batch_name", _sprite.batch_name }
            });
        }

        protected override void setVariables(JsonElement variables)
        {
            base.setVariables(variables);

            _sprite = null;
            _sprite = new Sprite(
                this.position,
                AnimationManager.get(Func.getProperty(variables, "animation_name").ToString()),
                Func.getProperty(variables, "batch_name").ToString()
            );
        }

        public Sprite sprite
        {
            get
            {
                return _sprite;
            }
        }
    }

    public class MovedObject : SpriteObject
    {
        int[] _direction = [0, 0];
        float _max_speed = 0.05f;

        public MovedObject(Pos pos) : base(pos, AnimationManager.get("test2"), "objects")
        {
            _simulated = true;

            KeysGF.bind("+move_forward", moveForward);
            KeysGF.bind("-move_forward", stopForward);

            KeysGF.bind("+move_left", moveLeft);
            KeysGF.bind("-move_left", stopLeft);

            KeysGF.bind("+move_back", moveBack);
            KeysGF.bind("-move_back", stopBack);

            KeysGF.bind("+move_right", moveRight);
            KeysGF.bind("-move_right", stopRight);
        }
        public MovedObject() : this(new Pos(0, 0, 0)) { }

        public override void simulation()
        {
            if (_direction[0] == 0 && _direction[1] == 0) return;

            double angle = Math.Atan2(_direction[1], _direction[0]);

            this.addPosition(new Pos(
                (float)(Math.Cos(angle) * _max_speed),
                0,
                (float)(Math.Sin(angle) * _max_speed)
            ));
        }

        public void moveForward()
        {
            _direction[1] -= 1;
        }
        public void stopForward()
        {
            _direction[1] += 1;
        }

        public void moveLeft()
        {
            _direction[0] -= 1;
        }
        public void stopLeft()
        {
            _direction[0] += 1;
        }

        public void moveBack()
        {
            _direction[1] += 1;
        }
        public void stopBack()
        {
            _direction[1] -= 1;
        }

        public void moveRight()
        {
            _direction[0] += 1;
        }
        public void stopRight()
        {
            _direction[0] -= 1;
        }
    }

}

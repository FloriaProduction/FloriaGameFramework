using FloriaGF.Graphic;
using System;
using System.ComponentModel.Design;

namespace FloriaGF
{
    class Camera : BaseObject
    {
        string? _target = null;
        float _strength = 0.025f;

        public Camera(Pos pos, string? target = null) : base(pos) 
        {
            _simulated = true;
            _target = target;
        }

        public void setTarget(string uid)
        {
            _target = uid;
        }
        public override void setPosition(Pos pos)
        {
            base.setPosition(pos);

            if (_target != null)
            {
                int[] point_camera = (World.get(_target) as MovedObject ?? throw new Exception()).sprite.animation.getPoint("camera");
                WindowGF.camera_position = new Pos(
                    pos.x + (float)point_camera[0]/32,
                    pos.y + (float)point_camera[1]/32,
                    pos.z
                );
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
                MovedObject obj = World.get(_target) as MovedObject ?? throw new Exception();

                this.setPosition(new Pos(
                    Func.smooth(this.x, obj.x, _strength),
                    this.y,
                    Func.smooth(this.z, obj.z, _strength)
                ));
            }
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
    

    class SpriteObject : BaseObject
    {
        Sprite _sprite;
        public SpriteObject(Pos pos, Animation animation, string batch_name) : base(pos)
        {
            _sprite = new Sprite(pos, animation, batch_name);
        }
        public SpriteObject(Pos pos, string animation_name, string batch_name) : this(pos, AnimationManager.get(animation_name), batch_name) { }
        
        public override void setPosition(Pos pos)
        {
            base.setPosition(pos);
            _sprite.position = this.position;
        }

        public Sprite sprite
        {
            get
            {
                return _sprite;
            }
        }
    }

    class MovedObject : SpriteObject
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

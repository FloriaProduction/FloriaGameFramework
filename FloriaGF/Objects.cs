using FloriaGF.Graphic;
using System;

namespace FloriaGF
{
    class Camera : BaseObject
    {
        public Camera(Vec pos) : base(pos) 
        {
            KeysGF.regFunction(DotGLFW.Key.A, DotGLFW.InputState.Press, 0, this.moveLeft);
            KeysGF.regFunction(DotGLFW.Key.D, DotGLFW.InputState.Press, 0, this.moveRight);
            KeysGF.regFunction(DotGLFW.Key.W, DotGLFW.InputState.Press, 0, this.moveUp);
            KeysGF.regFunction(DotGLFW.Key.S, DotGLFW.InputState.Press, 0, this.moveDown);
        }

        void moveLeft()
        {
            this.setPosition(this.position + new Vec(-0.5f, 0, 0));
        }
        void moveRight()
        {
            this.setPosition(this.position + new Vec(0.5f, 0, 0));
        }
        void moveUp()
        {
            this.setPosition(this.position + new Vec(0, 0, -0.5f));
        }
        void moveDown()
        {
            this.setPosition(this.position + new Vec(0, 0, 0.5f));
        }


        public override void setPosition(Vec pos)
        {
            base.setPosition(pos);
            WindowGF.camera_position = pos;

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
        public SpriteObject(Vec pos, Animation animation, string batch_name) : base(pos)
        {
            _sprite = new Sprite(pos, animation, batch_name);
        }
        public override void setPosition(Vec pos)
        {
            base.setPosition(pos);
            _sprite.position = this.position;
        }
    }

}

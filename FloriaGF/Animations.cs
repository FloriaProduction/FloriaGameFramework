using FloriaGF.Graphic;
using System;
using System.Text.Json;

namespace FloriaGF
{
    public class Animation
    {
        string _name;
        uint _count_frames, _currect_frame, _frame_width, _frame_height, _delay;
        bool _loop;
        int[] _anchor;
        Dictionary<string, int[]> _points = new();

        /// <param name="name">Название картинки из ImagesFG</param>
        /// <param name="count_frames">Количество кадров</param>
        /// <param name="delay">Задержка в мс</param>
        /// <param name="loop">Зацикленность</param>
        /// <param name="anchor">vec(2)</param>
        public Animation(string name, uint count_frames, uint delay, bool loop, int[]? anchor = null, Dictionary<string, int[]>? points = null) 
        {
            _name = name;
            _count_frames = count_frames;
            _delay = delay; 
            _loop = loop;
            _currect_frame = 0;
            _anchor = anchor == null ? [0, 0] : anchor;

            if (points != null)
                foreach (string pname in points.Keys)
                    _points[pname] = points[pname];

            var img = ImagesGF.getCacheImage(name);
            _frame_width = (uint)img.Width / _count_frames;
            _frame_height = (uint)img.Height;
        }

        public void nextFrame()
        {
            if (!(_currect_frame < _count_frames && _loop)) return;

            _currect_frame++;

            if (_currect_frame >= _count_frames)
                _currect_frame = 0;
        }

        public void addPoint(string name, int x, int y)
        {
            this.addPoint(name, [x, y]);
        }
        public void addPoint(string name, int[] pos)
        {
            _points[name] = pos;
        }
        public int[] getPoint(string name)
        {
            return _points[name];
        }
        public string[] point_names
        {
            get
            {
                return _points.Keys.ToArray();
            }
        } 

        /// <summary>
        /// Получить float(4) координаты пикселей текущего кадра
        /// </summary>
        public uint[] frame_position
        {
            get
            {
                return [
                    _currect_frame * _frame_width,
                    0,
                    _currect_frame * _frame_width + _frame_width,
                    _frame_height
                ];
            }
        }
        public string name
        {
            get
            {
                return _name;
            }
        }
        public uint count_frames
        {
            get
            {
                return _count_frames;
            }
        }
        public uint currect_frame
        {
            get
            {
                return _currect_frame;
            }
            set
            {
                if (value < 0 || value > _count_frames) throw new Exception();
                _currect_frame = value;
            }
        }
        public bool loop
        {
            get
            {
                return _loop;
            }
        }
        public uint delay
        {
            get
            {
                return _delay;
            }
        }
        public int[] anchor
        {
            get
            {
                return _anchor;
            }
        }

        public Animation Clone() 
        {
            return new Animation(_name, _count_frames, _delay, _loop, _anchor, _points);
        } 
    }

    public static class AnimationManager
    {
        static Dictionary<string, Animation> _animations = new();
        
        public static void create(Animation anim)
        {
            _animations[anim.name] = anim;
            Log.write($"Animation '{anim.name}' created!", "AnimationManager");
        }

        public static Animation get(string name)
        {
            return _animations[name];
        }

        public static void loadAnimations(string animation_file)
        {
            JsonElement animations = FileGF.readJson(animation_file);
            foreach (var animation in animations.EnumerateArray())
            {
                string name = animation.GetProperty("name").ToString();
                string file = animation.GetProperty("file").ToString();
                uint count = animation.GetProperty("count").GetUInt32();
                uint delay = animation.GetProperty("delay").GetUInt32();
                bool loop = animation.GetProperty("loop").GetBoolean();
                int[] anchor;
                if (animation.TryGetProperty("anchor", out JsonElement _anchor))
                    anchor = (from number in _anchor.EnumerateArray() select number.GetInt32()).ToArray();
                else
                    anchor = [0, 0];
                if (anchor.Length != 2)
                    throw new Exception();

                Dictionary<string, int[]> points = new();
                if (animation.TryGetProperty("points", out JsonElement _points))
                    foreach (var point in _points.EnumerateArray())
                    {
                        int[] value = (from number in point.GetProperty("anchor").EnumerateArray() select number.GetInt32()).ToArray();
                        points[point.GetProperty("name").ToString()] = value;
                    }
                        
                ImagesGF.loadImage(name, $"data/animations/{file}");
                AnimationManager.create(new Animation(name, count, delay, loop, anchor, points));
            }
        }
    }
}

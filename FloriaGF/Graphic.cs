//#define FLORIAGF_ENABLE_LOG_GRAPHIC

using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static DotGL.GL;

namespace FloriaGF
{
    namespace Graphic 
    {
        class ShaderOpenGL
        {
            uint _id = 0;
            int _shader_type;
            public ShaderOpenGL(int shader_type, string source, bool compile = true)
            {
                this._shader_type = shader_type;
                if (compile) this.compile(source);
            }

            ~ShaderOpenGL()
            {
                this.delete();
            }

            public unsafe void compile(string source)
            {
                this._id = glCreateShader(this.shader_type);
                glShaderSource(this.id, source);
                glCompileShader(this.id);

                int status;
                glGetShaderiv(this.id, GL_COMPILE_STATUS, &status);
                if (status != 1)
                {
                    string message = glGetShaderInfoLog(this.id, 1024);
                    this.delete();
                    throw new Exception(message);
                }
            }
            public void delete()
            {
                glDeleteShader(this.id);
            }

            public uint id
            {
                get { return this._id; }
            }
            public bool compiled
            {
                get { return this.id != 0; }
            }
            public int shader_type
            {
                get { return this._shader_type; }
            }
        }

        class ShaderProgramOpenGL
        {
            uint _id = 0;
            ShaderOpenGL _vertex_shader;
            ShaderOpenGL _fragment_shader;

            public unsafe void compile()
            {
                _id = glCreateProgram();

                glAttachShader(this.id, this.vertex_shader.id);
                glAttachShader(this.id, this.fragment_shader.id);

                glLinkProgram(this.id);

                int status;
                glGetProgramiv(this.id, GL_LINK_STATUS, &status);
                if (status != 1)
                {
                    string message = glGetShaderInfoLog(this.id, 1024);
                    this.delete();
                    throw new Exception(message);
                }
            }

            public void delete()
            {
                glDeleteProgram(this.id);
                this._id = 0;
            }

            public ShaderProgramOpenGL(ShaderOpenGL vertex_shader, ShaderOpenGL fragment_shader, bool compile = true)
            {
                if (vertex_shader.shader_type != GL_VERTEX_SHADER ||
                    fragment_shader.shader_type != GL_FRAGMENT_SHADER)
                    throw new Exception("Incorrect shader type");

                this._vertex_shader = vertex_shader;
                this._fragment_shader = fragment_shader;

                if (compile) this.compile();
            }
            public ShaderProgramOpenGL(string path_vertex_shader, string path_fragment_shader, bool compile = true) : this(new ShaderOpenGL(GL_VERTEX_SHADER, FileGF.readFile(path_vertex_shader)), new ShaderOpenGL(GL_FRAGMENT_SHADER, FileGF.readFile(path_fragment_shader)), compile){ }
            ~ShaderProgramOpenGL()
            {
                delete();
            }

            public uint id
            {
                get { return this._id; }
            }

            public ShaderOpenGL vertex_shader
            {
                get { return this._vertex_shader; }
            }
            public ShaderOpenGL fragment_shader
            {
                get { return this._fragment_shader; }
            }

            public bool compiled
            {
                get { return this.id != 0; }
            }

            Dictionary<string, int> _cache_getUniformLocation = new();
            public int getUniformLocation(string name)
            {
                glUseProgram(this.id);
                if (!_cache_getUniformLocation.ContainsKey(name))
                    _cache_getUniformLocation[name] = glGetUniformLocation(this.id, name);

                return _cache_getUniformLocation[name];
            }

            public unsafe float[] getUniformValues(string name, uint count_values)
            {
                float[] values = new float[count_values];
                fixed (float* data_p = &values[0])
                    glGetUniformfv(this.id, this.getUniformLocation("vs_transform_position"), data_p);

                return values;
            }

            public void setUniform(string name, float value)
            {
                glUniform1f(getUniformLocation(name), value);
            }

            public void setUniform(string name, int value)
            {
                glUniform1i(getUniformLocation(name), value);
            }

            public void setUniform(string name, bool value)
            {
                glUniform1i(getUniformLocation(name), value ? 1 : 0);
            }

            public void setUniform(string name, float[] value)
            {
                int uniform_location = getUniformLocation(name);
                switch (value.Length)
                {
                    case 2:
                        glUniform2fv(uniform_location, value);
                        break;

                    case 3:
                        glUniform3fv(uniform_location, value);
                        break;

                    case 4:
                        glUniform4fv(uniform_location, value);
                        break;

                    case 16:
                        glUniformMatrix4fv(uniform_location, false, value);
                        break;

                    default:
                        throw new Exception("Incorrect array size");
                }
            }
        }

        class VBO
        {
            uint _id;
            int _usage;

            public unsafe VBO(float[] data, int usage = GL_STATIC_DRAW)
            {
                this._usage = usage;
                this._id = glGenBuffer();

                this.update(data);
            }
            ~VBO () 
            {
                glDeleteBuffers(this.id);
            }

            public unsafe void update(float[] data, int? index = null)
            {
                glBindBuffer(GL_ARRAY_BUFFER, this._id);

                if (data.Length > 0)
                    fixed (float* data_p = &data[0])
                        if (index.HasValue && index >= 0)
                            glBufferSubData(GL_ARRAY_BUFFER, sizeof(float) * (int)index, sizeof(float) * data.Length, data_p);
                        else
                            glBufferData(GL_ARRAY_BUFFER, sizeof(float) * data.Length, data_p, this.usage);

                glBindBuffer(GL_ARRAY_BUFFER, 0);
            }

            public uint id
            {
                get { return this._id; }
            }
            public int usage
            {
                get { return this._usage; }
            }
        }

        class VAO
        {
            uint _id;

            public VAO()
            {
                this._id = glGenVertexArray();
            }
            ~VAO()
            {
                glDeleteVertexArrays(this.id);
            }

            public void attachVBO(VBO vbo, int size, uint location, int type = GL_FLOAT)
            {
                glBindVertexArray(this.id);

                glBindBuffer(GL_ARRAY_BUFFER, vbo.id);
                glVertexAttribPointer(location, size, type, false, 0, 0);
                glEnableVertexAttribArray(location);

                glBindVertexArray(0);
            }

            public uint id
            {
                get { return this._id; }
            }
        }

        class Texture
        {
            uint _id;
            public Texture() : this(new Image(1, 1, [0, 0, 0, 0])) { }
            public Texture(Image img)
            {
                _id = glGenTexture();
                glBindTexture(GL_TEXTURE_2D, _id);

                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);

                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);

                glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, img.Width, img.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, img.Pixels);
                glGenerateMipmap(GL_TEXTURE_2D);

                glBindTexture(GL_TEXTURE_2D, 0);
            }
            ~Texture()
            {
                glDeleteTextures(this._id);
            }
            public void update(Image img)
            {
                glBindTexture(GL_TEXTURE_2D, _id);

                glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, img.Width, img.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, img.Pixels);
                glGenerateMipmap(GL_TEXTURE_2D);

                glBindTexture(GL_TEXTURE_2D, 0);
            }

            public uint id
            {
                get { return this._id; }
            }
        }
    
        class Batch
        {            
            VBO _points;
            VBO _tex_coords;

            VAO _vao;

            uint[] _indices;

            Texture _texture;

            ShaderProgramOpenGL _program;

            Dictionary<uint, Sprite> _sprites = new();
            uint _currect_sprite_id = 0;

            List<string> _animation_list = [];
            Dictionary<string, uint> _animation_count = new();
            Dictionary<string, uint[]> _animation_map = new();
            uint _image_width, _image_height;

            bool _update_all = false;

            string _name;


            public Batch(string name)
            {
                _points = new VBO([], GL_STREAM_DRAW);
                _tex_coords = new VBO([], GL_STREAM_DRAW);

                _vao = new VAO();
                _vao.attachVBO(_points, 3, 0);
                _vao.attachVBO(_tex_coords, 2, 1);

                _texture = new Texture();

                _indices = [];

                _program = new ShaderProgramOpenGL(
                    "data/shaders/batch_vertex_shader.glsl",
                    "data/shaders/batch_fragment_shader.glsl"
                ); 

                WindowGF.addBatch(name, this);
                _name = name;
            }
            ~Batch()
            {
                WindowGF.deleteBatch(this.name);
            }


            private void updateAll()
            {
                this.updatePoints();
                this.updateTexCoords();
                this.updateIndices();

                _update_all = false;
            }
            private void updatePoints()
            {
                _points.update((from sprite in _sprites.Values select sprite.points).SelectMany(a => a).ToArray());
            }
            public void updatePoints(uint id)
            {
                int index = Array.IndexOf(_sprites.Keys.ToArray(), id);
                if (index < 0) return;

                _points.update(_sprites[id].points, index*12);
            }
            private void updateTexCoords()
            {
                 float[] tex_coords = [1, 0, 1, 1, 0, 1, 0, 0];
                _tex_coords.update((from sprite in _sprites.Values select tex_coords).SelectMany(a => a).ToArray());
                
                foreach (var sprite in _sprites.Values)
                    this.updateTexCoords(sprite.id);
            }
            public void updateTexCoords(uint id)
            {
                int index = Array.IndexOf(_sprites.Keys.ToArray(), id);
                if (index < 0) return;

                Sprite sprite = _sprites[id];

                uint[] animation_position = _animation_map[sprite.animation_name];
                uint[] frame_position = sprite.frame_position;

                float[] tex_coords = [
                    (float)(animation_position[0] + frame_position[2])/_image_width,
                    (float)(animation_position[1])/_image_height,

                    (float)(animation_position[0] + frame_position[2])/_image_width,
                    (float)(animation_position[1] + frame_position[3])/_image_height,

                    (float)(animation_position[0] + frame_position[0])/_image_width,
                    (float)(animation_position[1] + frame_position[3])/_image_height,

                    (float)(animation_position[0] + frame_position[0])/_image_width,
                    (float)(animation_position[1])/_image_height
                ];

                _tex_coords.update(tex_coords, index*8);
            }
            private void updateIndices()
            {
                List<uint> sp_indices = [];

                for (uint i = 0; i < _sprites.Count; i++)
                    sp_indices.AddRange([
                        0 + i*4,
                        1 + i*4,
                        3 + i*4,
                        1 + i*4,
                        2 + i*4,
                        3 + i*4
                    ]);

                _indices = sp_indices.ToArray();
            }
           
            public uint addSprite(Sprite sprite)
            {
                _sprites[_currect_sprite_id] = sprite;
                _update_all = true;

                return _currect_sprite_id++;
            }
            public void popSprite(uint id)
            {
                this.removeAnimation(_sprites[id].animation_name);
                _sprites.Remove(id);
                _update_all = true;
            }
            
            private void generateAnimationMap()
            {
                uint back_width = 0, back_height = 0;
                foreach (string animation_name in _animation_list)
                {
                    Image image = ImagesGF.getCacheImage(animation_name);
                    if (image.Width > back_width)
                        back_width = (uint)image.Width;
                    back_height += (uint)image.Height;
                }

                Image animations = new(back_width, back_height, new Color(0, 0, 0, 0));
                _image_width = back_width;
                _image_height = back_height;

                uint last_y = 0;
                _animation_map.Clear();
                foreach (string animation_name in _animation_list)
                { 
                    Image image = ImagesGF.getCacheImage(animation_name);
                    animations.paste(image, 0, (int)last_y);
                    _animation_map[animation_name] = [0, last_y];
                    last_y += (uint)image.Height;
                }

                //animations.save($"batch-{this.name}_animations.png");

                _texture.update(animations);

                this.updateTexCoords();
            }
            public void addAnimation(string? name)
            {
                if (name == null) return;

                if (_animation_list.IndexOf(name) != -1)
                {
                    _animation_count[name]++;
                    return;
                }

                _animation_list.Add(name);

                if (!_animation_count.ContainsKey(name))
                    _animation_count[name] = 1;

                this.generateAnimationMap();
            }
            public void removeAnimation(string? name)
            {
                if (name == null) return;

                if (_animation_list.IndexOf(name) == -1) return;
      
                _animation_count[name]--;

                if (_animation_count[name] == 0)
                {
                    _animation_list.Remove(name);
                    _animation_count.Remove(name);
                    _animation_map.Remove(name);
                    this.generateAnimationMap();
                }  
            }

            public void render()
            {
                if (_update_all) this.updateAll();

                if (_indices.Length == 0) return;

                glBindTexture(GL_TEXTURE_2D, this._texture.id);
                glBindVertexArray(this._vao.id);

                glUseProgram(this._program.id);
                glDrawElements(GL_TRIANGLES, this._indices.Length, GL_UNSIGNED_INT, this._indices);
            }


            public float[] position
            {
                set 
                {
                    if (value.Length != 3) throw new Exception();
                    _program.setUniform("camera_position", [value[0], value[1], value[2]]);
                }
            }
            public float[] scale
            { 
                set
                {
                    if (value.Length != 3) throw new Exception();
                    _program.setUniform("camera_scale", [value[0], value[1], value[2]]);
                }
            }
            public string name
            {
                get { return _name; }
            }
        }

        class Sprite
        {
            static Vec _points = new([1, 0, 0, 1, 1, 0, 0, 1, 0, 0, 0, 0]);
            float[]? _points_cache;

            uint _id;

            Vec _translate;
            Vec _scale;

            Batch _batch;

            string? _animation_name;
            uint _animation_count_frames, _animation_currect_frame = 0;
            uint _animation_frame_width, _animation_frame_height;
            ulong _animation_last_frame_time = 0, _animation_delay_time;


            public Sprite(float x, float y, float z, float xs, float ys, float zs, string animation_name, uint count_frames, uint delay, string batch_name)
            {
                _translate = new Vec(x, y, z);
                _scale = new Vec(xs, ys, zs);

                _batch = WindowGF.getBatch(batch_name);
                _id = _batch.addSprite(this);

                this.setAnimation(animation_name, count_frames, delay);
            }
            public Sprite(float x, float y, float z, float xs, float ys, float zs, Animation anim, string batch_name) : this(x, y, z, xs, ys, zs, anim.name, anim.count_frames, anim.delay, batch_name) { }
            ~Sprite()
            {
                _batch.popSprite(this.id);
            }


            private void _updatePoints()
            {
                _points_cache = null;
                _batch.updatePoints(this.id);
            }
            public void setPosition(float x, float y, float z)
            {
                _translate = new Vec([x, y, z]);
                this._updatePoints();
            }
            public void setScale(float xs, float ys, float zs)
            {
                _scale = new Vec([xs, ys, zs]);
                this._updatePoints();
            }

            public void nextFrame()
            {
                 _animation_currect_frame++;
                if (_animation_currect_frame >= _animation_count_frames)
                    _animation_currect_frame = 0;

                _batch.updateTexCoords(_id);
            }
            public void setAnimation(string animation_name, uint count_frames, uint delay)
            {
                string? last_anim_name = _animation_name;

                _animation_name = animation_name;
                _animation_count_frames = count_frames;
                _animation_delay_time = delay;
                _animation_currect_frame = 0;
                var img = ImagesGF.getCacheImage(animation_name);
                _animation_frame_width = (uint)img.Width / _animation_count_frames;
                _animation_frame_height = (uint)img.Height;

                _batch.addAnimation(animation_name);
                _batch.removeAnimation(last_anim_name);
            }
            public void setAnimation(Animation anim)
            {
                this.setAnimation(anim.name, anim.count_frames, anim.delay);
            }

            public void simulation()
            {
                ulong ctime = TimeGF.time();

                if (!(_animation_last_frame_time + _animation_delay_time > ctime || _animation_count_frames == 1 || _animation_delay_time == 0))
                {
                    this.nextFrame();
                    _animation_last_frame_time = ctime;
                }
            }


            public uint id 
            { 
                get { return _id; }
            }
            public float x 
            { 
                get { return _translate.x; }
                set
                {
                    setPosition(value, _translate.y, _translate.z);
                }
            }
            public float y
            { 
                get { return _translate.y; }
                set
                {
                    setPosition(_translate.x, value, _translate.z);
                }
            }
            public float z
            { 
                get { return _translate.z; }
                set
                {
                    setPosition(_translate.x, _translate.y, value);
                }
            }
            public float[] position
            {
                get { return _translate; }
                set
                {
                    if (value.Length != 3) throw new Exception();
                    setPosition(value[0], value[1], value[2]);
                }
            }
            public float[] scale
            {
                get { return _scale; }
                set
                {
                    if (value.Length != 3) throw new Exception();
                    setScale(value[0], value[1], value[2]);
                }
            }
            public float[] points
            {
                get
                {
                    if (_points_cache == null)
                    {
                        _points_cache = _points.Copy();

                        for (int i = 0; i < _points_cache.Length; i += 3)
                        {
                            //scale
                            _points_cache[i] *= _scale.x;
                            _points_cache[i+1] *= _scale.y;
                            _points_cache[i+2] *= _scale.z;

                            //translate
                            _points_cache[i] += _translate.x;
                            _points_cache[i+1] += _translate.y;
                            _points_cache[i+2] += _translate.z;
                        }
                    }

                    return _points_cache;
                }
            }
            public uint[] frame_position
            {
                get
                {
                    return [
                        _animation_currect_frame * _animation_frame_width,
                        0,
                        _animation_currect_frame * _animation_frame_width + _animation_frame_width,
                        _animation_frame_height
                    ];
                }
            }
            public string animation_name
            {
                get { return _animation_name; }
            }
            public uint animation_currect_frame
            {
                get { return _animation_currect_frame; }
                set
                {
                    if (value >= animation_count_frames) throw new Exception();
                    _animation_currect_frame = value;
                }
            }
            public uint animation_count_frames
            {
                get { return _animation_count_frames; }
            }
        }
    
        class Color
        {
            byte[] _data;
            

            public Color(byte[] data)
            {
                if (data.Length != 4) throw new Exception("Invalid data");
                _data = data;
            }
            public Color(byte r, byte g, byte b, byte a) : this([r, g, b, a]) { }


            public byte R
            {
                get { return _data[0]; }
            }
            public byte G
            {
                get { return _data[1]; }
            }
            public byte B
            {
                get { return _data[2]; }
            }
            public byte A
            {
                get { return _data[3]; }
            }
            public byte this[int index]
            {
                get { return this._data[index]; }
            }


            public static implicit operator byte[](Color color)
            {
                return color._data;
            }
            public static implicit operator Color(byte[] data)
            {
                return new Color(data);
            }
        }

        class Image
        {
            byte[] _pixels;
            uint _width, 
                 _height;

            public Image()
            {
                _width = 0;
                _height = 0;
                _pixels = [];
            }
            public Image(uint width, uint height, byte[]? pixels = null)
            {
                _width = width;
                _height = height;

                if (pixels == null)
                    _pixels = new byte[width * height * 4];
                else
                    _pixels = pixels;
            }
            public Image(uint width, uint height, Color color) : this(width, height) 
            {
                List<byte> data = new();
                for (int i = 0; i < width * height; i++)
                    data.AddRange((byte[])color);

                _pixels = [..data];
            }
            public Image(string path)
            {
                this.load(path);
            }

            public void load(string path)
            {
                List<byte> pixels = [];

                using (var image_data = SixLabors.ImageSharp.Image.Load<Rgba32>(path))
                {
                    _width = (uint)image_data.Width;
                    _height = (uint)image_data.Height;

                    for (int y = 0; y < _height; y++)
                        for (int x = 0; x < _width; x++)
                        {
                            Rgba32 pixel = image_data[x, y];
                            pixels.AddRange([pixel.R, pixel.G, pixel.B, pixel.A]);
                        }
                }
                _pixels = pixels.ToArray();
            }
            public void paste(Image img, int xc, int yc)
            {
                for (int y = 0; y < img.Height; y++)
                    for (int x = 0; x < img.Width; x++)
                    {
                        int px = Math.Max(Math.Min(x + xc, this.Width), 0),
                            py = Math.Max(Math.Min(y + yc, this.Height), 0);

                        this.setPixel(px, py, img.getPixel(x, y));
                    }
            }
            public void save(string path)
            {
                var simg = new SixLabors.ImageSharp.Image<Rgba32>(Width, Height);

                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                    {
                        Color color = getPixel(x, y);
                        simg[x, y] = new Rgba32(color.R, color.G, color.B, color.A);
                    }

                simg.Save(path);
            }
            public byte[] getPixel(int x, int y)
            {
                return new ArraySegment<byte>(_pixels, this.Width * 4 * y + x * 4, 4).ToArray();
            }
            public void setPixel(int x, int y, Color color)
            {
                for (int i = 0; i < 4; i++)
                    _pixels[this.Width * 4 * y  + x * 4 + i] = color[i];
            }
            public void fill(int x1, int y1, int x2, int y2, Color color)
            {
                for (int x = 0; x < x2-x1; x++)
                    for (int y = 0; y < y2-y1; y++)
                        this.setPixel(x1 + x, y1 + y, color);
            }


            public int Width
            {
                get { return (int)_width; }
            }
            public int Height
            {
                get { return (int)_height; }
            }
            public byte[] Pixels
            {
                get { return _pixels; }
            }


            public static implicit operator Image(DotGLFW.Image img)
            {
                return new Image((uint)img.Width, (uint)img.Height, img.Pixels);
            }
            public static implicit operator DotGLFW.Image(Image img)
            {
                var rimg = new DotGLFW.Image();
                rimg.Width = img.Width;
                rimg.Height = img.Height;
                rimg.Pixels = img.Pixels;

                return rimg;
            }
        }
    
        class Animation
        {
            public string name;
            public uint count_frames, delay;
            public Animation(string name, uint count_frames, uint delay)
            {
                this.name = name;
                this.count_frames = count_frames;
                this.delay = delay;
            }
        }
    }
}

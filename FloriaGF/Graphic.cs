//#define FLORIAGF_ENABLE_LOG_GRAPHIC

using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static DotGL.GL;

namespace FloriaGF
{
    namespace Graphic 
    {
        /// <summary>
        /// Класс шейдера OpenGL
        /// </summary>
        public class ShaderOpenGL
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
            /// <summary>
            /// Скопилировать шейдер
            /// </summary>
            /// <param name="source">GLSL-код шейдера</param>
            /// <exception cref="Exception"></exception>
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
                //glDeleteShader(this.id);
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

        /// <summary>
        /// Шейдерная программа OpenGL
        /// </summary>
        public class ShaderProgramOpenGL
        {
            uint _id = 0;
            ShaderOpenGL _vertex_shader;
            ShaderOpenGL _fragment_shader;
            /// <summary>
            /// Скомплиировать шейдерную программу
            /// </summary>
            /// <exception cref="Exception"></exception>
            public unsafe void compile(ShaderOpenGL vertex_shader, ShaderOpenGL fragment_shader)
            {
                _id = glCreateProgram();

                glAttachShader(this.id, vertex_shader.id);
                glAttachShader(this.id, fragment_shader.id);

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
                //glDeleteProgram(this.id);
                //this._id = 0;
            }

            public ShaderProgramOpenGL(ShaderOpenGL vertex_shader, ShaderOpenGL fragment_shader, bool compile = true)
            {
                if (vertex_shader.shader_type != GL_VERTEX_SHADER ||
                    fragment_shader.shader_type != GL_FRAGMENT_SHADER)
                    throw new Exception("Incorrect shader type");

                this._vertex_shader = vertex_shader;
                this._fragment_shader = fragment_shader;

                if (compile) this.compile(this.vertex_shader, this.fragment_shader);
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
            /// <summary>
            /// получить Location переменной в шейдерной программе
            /// </summary>
            public int getUniformLocation(string name)
            {
                glUseProgram(this.id);
                if (!_cache_getUniformLocation.ContainsKey(name))
                    _cache_getUniformLocation[name] = glGetUniformLocation(this.id, name);

                return _cache_getUniformLocation[name];
            }
            /// <summary>
            /// Получить значение переменной из шейдерной программы
            /// </summary>
            public unsafe float[] getUniformValues(string name, uint count_values)
            {
                float[] values = new float[count_values];
                fixed (float* data_p = &values[0])
                    glGetUniformfv(this.id, this.getUniformLocation("vs_transform_position"), data_p);

                return values;
            }
            /// <summary>
            /// Установить значение переменной name в шейдерной программе
            /// </summary>
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

        /// <summary>
        /// vertex buffer object - проще говоря массив данных
        /// </summary>
        public class VBO
        {
            uint _id;
            int _usage;
            float[] _data;

            public unsafe VBO(float[] data, int usage = GL_STATIC_DRAW)
            {
                this._usage = usage;
                this._id = glGenBuffer();

                this.update(data);
            }
            ~VBO () 
            {
                //glDeleteBuffers(this.id);
            }

            /// <summary>
            /// Обновить(установить) значения vbo 
            /// </summary>
            /// <param name="data">Данные</param>
            /// <param name="index">Если не null, обновляется часть массива</param>
            public unsafe void update(float[] data, uint? index = null)
            {
                glBindBuffer(GL_ARRAY_BUFFER, this._id);

                if (data.Length > 0)
                    fixed (float* data_p = &data[0])
                        if (index.HasValue && index >= 0)
                            glBufferSubData(GL_ARRAY_BUFFER, sizeof(float) * (int)index, sizeof(float) * data.Length, data_p);
                        else
                        {
                            glBufferData(GL_ARRAY_BUFFER, sizeof(float) * data.Length, data_p, this.usage);
                            _data = data;
                        }
                            

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

        /// <summary>
        /// vertex attrib object - совокупность vbo с настройками
        /// </summary>
        public class VAO
        {
            uint _id;

            public VAO()
            {
                this._id = glGenVertexArray();
            }
            ~VAO()
            {
                //glDeleteVertexArrays(this.id);
            }
            /// <summary>
            /// Включить vbo в vao
            /// </summary>
            /// <param name="vbo">Сам VBO объект</param>
            /// <param name="size">Количество чисел на 1 вершину</param>
            /// <param name="location">Location для VBO</param>
            /// <param name="type">Тип значений VBO</param>
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

        /// <summary>
        /// Текстура
        /// </summary>
        public class Texture
        {
            uint _id;
            
            public Texture()
            {
                _id = glGenTexture();
                glBindTexture(GL_TEXTURE_2D, _id);

                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);

                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);

                glBindTexture(GL_TEXTURE_2D, 0);
            }
            public Texture(Image img) : this()
            {
                this.update(img);
            }
            ~Texture()
            {
                //glDeleteTextures(this._id);
            }
            /// <summary>
            /// Обновление происходит для всей текстуры
            /// </summary>
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
        
        /// <summary>
        /// Используется для пакетной отрисовки множества спрайтов
        /// </summary>
        public class Batch
        {
            VBO _points, _tex_coords;
            VAO _vao;
            uint[] _indices = [];
            Dictionary<uint, uint[]> _sprites_indices = new();

            ShaderProgramOpenGL _program;

            IDM<BatchObject> _sprites = new(true);

            Texture _texture;

            Dictionary<string, uint> _animation_count = new();
            Dictionary<string, uint[]> _animation_map = new();
            uint _tex_width, _tex_height;

            Pos _position = new(0, 0, 0), 
                _scale = new(1, 1, 1);

            bool _update_all = false;
            bool _update_animations = false;

            string _name;
            /// <param name="name">Уникальное имя для Batch, используется в роли уникального индетификатора</param>
            public Batch(string name)
            {
                _points = new VBO([], GL_STREAM_DRAW);
                _tex_coords = new VBO([], GL_DYNAMIC_DRAW);

                _vao = new VAO();
                _vao.attachVBO(_points, 3, 0);
                _vao.attachVBO(_tex_coords, 2, 1);

                _texture = new();

                _program = new ShaderProgramOpenGL(
                    "data/shaders/batch_vertex_shader.glsl",
                    "data/shaders/batch_fragment_shader.glsl"
                );

                _name = name;
                WindowGF.addBatch(name, this);
            }
            ~Batch()
            {
                WindowGF.deleteBatch(_name);
            }

            public uint addSprite(BatchObject sprite)
            {
                _update_all = true;
                return _sprites.add(sprite);
            }
            public void removeSprite(uint id)
            {
                _update_all = true;
                _sprites.remove(id);
            }
            
            private void generationSpriteIndices()
            {
                _sprites_indices.Clear();
                uint last_point_index = 0, last_texcoord_index = 0;

                foreach (Sprite sprite in _sprites.Values)
                {
                    _sprites_indices[sprite.id] = [last_point_index, last_texcoord_index];
                    last_point_index += (uint)sprite.points.Length;
                    last_texcoord_index += (uint)(sprite.points.Length - sprite.points.Length/3);
                }
            }

            private void generateAnimationMap()
            {
                // animation names
                List<string> animation_names = [];
                foreach (Sprite sprite in _sprites.Values)
                {
                    if (sprite.animation != null && sprite.animation.name != null && animation_names.IndexOf(sprite.animation.name) == -1)
                        animation_names.Add(sprite.animation.name);
                }

                // back size
                uint back_width = 0, back_height = 0;
                foreach (string animation_name in animation_names)
                {
                    Image img = ImagesGF.getCacheImage(animation_name);
                    if (img.Width > back_width)
                        back_width = (uint)img.Width;
                    back_height += (uint)img.Height;
                }
                _tex_width = back_width;
                _tex_height = back_height;
                
                // back
                Image back_img = new(_tex_width, _tex_height, new Color(0, 0, 0, 0));

                uint last_y = 0;
                _animation_map.Clear();
                foreach (string animation_name in animation_names)
                {
                    Image animation_img = ImagesGF.getCacheImage(animation_name);
                    back_img.paste(animation_img, 0, (int)last_y);
                    _animation_map[animation_name] = [0, last_y];
                    last_y += (uint)animation_img.Height;
                }

                back_img.save($"batch({_name}).png");
                _texture.update(back_img);

                this.updateTexCoords();

                _update_animations = false;
            }
            private float[] getTexCoords(uint id)
            {
                var sprite = _sprites[id];

                uint[] animation_position = _animation_map[sprite.animation.name];
                uint[] frame_position = sprite.animation.frame_position;

                float[] data = [
                    (float)(animation_position[0] + frame_position[2])/_tex_width,
                    (float)(animation_position[1])/_tex_height,

                    (float)(animation_position[0] + frame_position[2])/_tex_width,
                    (float)(animation_position[1] + frame_position[3])/_tex_height,

                    (float)(animation_position[0] + frame_position[0])/_tex_width,
                    (float)(animation_position[1] + frame_position[3])/_tex_height,

                    (float)(animation_position[0] + frame_position[0])/_tex_width,
                    (float)(animation_position[1])/_tex_height
                ];

                return data;
            }

            private void updatePoints()
            {
                List<float> points = [];
                foreach (uint id in _sprites_indices.Keys)
                {
                    points.AddRange(_sprites[id].points);
                }
                _points.update(points.ToArray());
            }
            public void updatePoints(uint id)
            {
                _points.update(_sprites[id].points, _sprites_indices[id][0]);
            }
            private void updateTexCoords()
            {
                List<float> texcoords = [];
                foreach (uint id in _sprites_indices.Keys)
                {
                    texcoords.AddRange(getTexCoords(id));
                }
                _tex_coords.update(texcoords.ToArray());
            }
            public void updateTexCoords(uint id)
            {
                _tex_coords.update(getTexCoords(id), _sprites_indices[id][1]);
            }
            private void updateIndices()
            {
                List<uint> indices = [];
                uint li = 0;
                foreach (uint id in _sprites_indices.Keys)
                {
                    uint[] edited_indices = _sprites[id].indices;
                    for (int i = 0; i < edited_indices.Length; i++)
                        edited_indices[i] += li;

                    indices.AddRange(edited_indices);
                    li += (uint)_sprites[id].points.Length / 3;
                }

                _indices = indices.ToArray();
            }
           
            private void updateAll()
            {
                this.generationSpriteIndices();
                this.generateAnimationMap();
                
                this.updatePoints();
                this.updateIndices();

                _update_all = false;
            }
            /// <summary>
            /// Обновить общую текстуру
            /// </summary>
            public void updateAnimations()
            {
                _update_animations = true;
            }
            /// <summary>
            /// Просимулировать все спрайты, используемые этим Batch
            /// </summary>
            public void simulationSprites()
            {
                foreach (BatchObject sprite in _sprites.Values)
                    sprite.simulation();
            }

            public void render()
            {
                if (_update_all) this.updateAll();
                if (_update_animations) this.generateAnimationMap();
                if (_indices.Length == 0) return;

                glBindTexture(GL_TEXTURE_2D, this._texture.id);
                glBindVertexArray(this._vao.id);

                glUseProgram(this._program.id);
                glDrawElements(GL_TRIANGLES, this._indices.Length, GL_UNSIGNED_INT, this._indices);
            }

            public string name
            {
                get { return _name; }
            }
            /// <summary>
            /// Vec(3) Глобальная позиция для Batch, изменения затронут все Спрайты в этом Batch
            /// </summary>
            public Pos position
            {
                get
                {
                    return _position;
                }
                set
                {
                    _position = value;
                    _program.setUniform("camera_position", [value[0], value[1], value[2]]);
                }
            }
            /// <summary>
            /// Vec(3) Глобальное растяжение для Batch, изменения затронут все Спрайты в этом Batch
            /// </summary>
            public Pos scale
            {
                get
                {
                    return _scale;
                }
                set
                {
                    _scale = value;
                    _program.setUniform("camera_scale", [value[0], value[1], value[2]]);
                }
            }
            
            public ShaderProgramOpenGL program
            {
                get
                {
                    return _program;
                }
            }
        }

        /// <summary>
        /// Простой интерфейс объектов для Batch
        /// </summary>
        public interface BatchObject
        {
            public uint id { get; }
            public float[] points { get; }
            public Animation? animation { get; }
            public uint[] indices { get; }
            public string? batch_name { get; }
            public void simulation();
        } 
        
        /// <summary>
        /// Спрайт
        /// </summary>
        public class Sprite : BatchObject
        {
            static Vec _points = new([1, 0, 0, 1, 1, 0, 0, 1, 0, 0, 0, 0]);
            float[]? _points_cache;

            uint _id;

            Pos _translate;
            Pos _scale;

            Batch? _batch;

            Animation? _animation;
            ulong _animation_last_change;
            /// <param name="x">X - координата</param>
            /// <param name="y">Y - координата</param>
            /// <param name="z">Z - координата</param>
            /// <param name="xs">X - растяжение</param>
            /// <param name="ys">Y - растяжение</param>
            /// <param name="zs">Z - растяжение</param>
            /// <param name="animation">Анимация для спрайта</param>
            /// <param name="batch_name">Имя Batch</param>
            public Sprite(float x, float y, float z, float xs, float ys, float zs, Animation? animation, string batch_name)
            {
                this.setPosition(x, y, z, false);
                this.setScale(xs, ys, zs, false);

                this.setAnimation(animation);

                _animation_last_change = TimeGF.time();

                _batch = WindowGF.getBatch(batch_name);
                _id = _batch.addSprite(this);
            }
            /// <summary>
            /// Создаст спрайт с растяжением 1, 1, 1
            /// </summary>
            /// <param name="x">X - координата</param>
            /// <param name="y">Y - координата</param>
            /// <param name="z">Z - координата</param>
            /// <param name="animation">Анимация для спрайта</param>
            /// <param name="batch_name">Имя Batch</param>
            public Sprite(float x, float y, float z, Animation? animation, string batch_name) : this(x, y, z, 1, 1, 1, animation, batch_name) { }
            /// <summary>
            /// Создаст спрайт с растяжением 1, 1, 1
            /// </summary>
            /// <param name="pos">Vec(3) - координаты для спрайта</param>
            /// <param name="animation">Анимация для спрайта</param>
            /// <param name="batch_name">Имя Batch</param>
            public Sprite(Pos pos, Animation? animation, string batch_name) : this(pos.x, pos.y, pos.z, animation, batch_name) { }
            ~Sprite()
            {
                if (_batch != null) _batch.removeSprite(this.id);
            }


            private void _updatePoints()
            {
                _points_cache = null;
                if (_batch != null) _batch.updatePoints(this.id);
            }
            /// <summary>
            /// Изменить позицию
            /// </summary>
            /// <param name="update_points">Обновить точки спрайта у Batch</param>
            public void setPosition(float x, float y, float z, bool update_points = true)
            {
                this.setPosition(new Pos(x, y, z), update_points);
            }
            /// <summary>
            /// Изменить позицию
            /// </summary>
            /// <param name="pos">Vec(3)</param>
            /// <param name="update_points">Обновить точки спрайта у Batch</param>
            public void setPosition(Pos pos, bool update_points = true)
            {
                _translate = pos;
                if (update_points) this._updatePoints();
            }
            /// <summary>
            /// Изменить растяжение
            /// </summary>
            /// <param name="update_points">Обновить точки спрайта у Batch</param>
            public void setScale(float xs, float ys, float zs, bool update_points = true)
            {
                this.setScale(new Pos(xs, ys, zs), update_points);
            }
            /// <summary>
            /// Изменить растяжение
            /// </summary>
            /// <param name="scale">Vec(3)</param>
            /// <param name="update_points">Обновить точки спрайта у Batch</param>
            public void setScale(Pos scale, bool update_points = true)
            {
                _scale = scale;
                if (update_points) this._updatePoints();
            }
            /// <summary>
            /// Изменить анимацию
            /// </summary>
            public void setAnimation(Animation? animation)
            {
                _animation = animation == null ? null : animation.Clone();
                
                if (_batch != null) _batch.updateAnimations();
            }
            /// <summary>
            /// Симулировать спрайт, нужен для воспроизведении анимации
            /// </summary>
            public void simulation()
            {
                if (_animation == null) return;
                
                var nt = TimeGF.time();

                if ((_animation_last_change + _animation.delay > nt) || 
                    (_animation.currect_frame >= _animation.count_frames - 1 && !_animation.loop))
                    return;

                _animation.nextFrame();
                if (_batch != null) _batch.updateTexCoords(_id);

                _animation_last_change = nt;
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
                        _points_cache = _points.ToArray();

                        int[] anchor = _animation == null ? [0, 0] : _animation.anchor;

                        for (int i = 0; i < _points_cache.Length; i += 3)
                        {
                            _points_cache[i] -= (float)anchor[0]/32;
                            _points_cache[i+1] -= (float)anchor[1]/32;

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
            public uint[] indices
            {
                get
                {
                    return [0, 1, 3, 1, 2, 3];
                }
            }
            public Animation? animation
            {
                get
                {
                    return _animation;
                }
            }
            public string? batch_name
            {
                get
                {
                    return _batch == null ? null : _batch.name;
                }
            }
        }

        /// <summary>
        /// Анимация для спрайтов
        /// </summary>
        
    }
}

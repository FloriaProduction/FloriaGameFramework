using System;
using DotGLFW;
using static DotGL.GL;

namespace FloriaGF
{
    public interface VecInteface
    {
        public int Length { get; }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }


        // vec
        public static VecInteface operator +(VecInteface a, VecInteface b)
        {
            throw new Exception();
        }
        public static VecInteface operator -(VecInteface a, VecInteface b)
        {
            throw new Exception();
        }
        public static VecInteface operator *(VecInteface a, VecInteface b)
        {
            throw new Exception();
        }
        public static VecInteface operator /(VecInteface a, VecInteface b)
        {
            throw new Exception();
        }

        // index
        public float this[int index] { get; set; }

        // array
        public float[] ToArray();

    }


    public class Vec : VecInteface
    {
        protected float[] values;
        
        public Vec(float[] values)
        {
            this.values = values;
        }
        public Vec() : this([]) { }
        public Vec(float a, float b) : this([a, b]) { }
        public Vec(float a, float b, float c) : this([a, b, c]) { }
        public Vec(float a, float b, float c, float d) : this([a, b, c, d]) { }
        public Vec(Pos data) : this(data.ToArray()) { }


        // params
        public int Length
        {
            get { return this.values.Length; }
        }
        public float x
        {
            get { return this[0]; }
            set { this[0] = value; }
        }
        public float y
        {
            get { return this[1]; }
            set { this[1] = value; }
        }
        public float z
        {
            get { return this[2]; }
            set { this[2] = value; }
        }


        // functions 
        public override string ToString()
        {
            return $"Vec({string.Join(", ", this.values)})";
        }

        protected static Vec calculateVec(float[] a, float[] b, byte operation_id)
        {
            if (a.Length != b.Length) throw new Exception("Defferent lenght!");

            for (int i = 0; i < a.Length; i++)
                switch (operation_id)
                {
                    case 0: // +
                        a[i] += b[i];
                        break;
                    case 1: // -
                        a[i] -= b[i];
                        break;
                    case 2: // *
                        a[i] *= b[i];
                        break;
                    case 3: // /
                        a[i] /= b[i];
                        break;
                    default:
                        throw new Exception("Incorrect operation_id!");
                }
            return new Vec(a);
        }


        // vec
        public static Vec operator +(Vec a, Vec b)
        {
            return calculateVec(a.values, b.values, 0);
        }
        public static Vec operator -(Vec a, Vec b)
        {
            return calculateVec(a.values, b.values, 1);
        }
        public static Vec operator *(Vec a, Vec b)
        {
            return calculateVec(a.values, b.values, 2);
        }

        public static Vec operator /(Vec a, Vec b)
        {
            return calculateVec(a.values, b.values, 3);
        }


        // float
        public static Vec operator +(Vec a, float b)
        {
            return new Vec((from v in a.values select v + b).ToArray());
        }
        public static Vec operator -(Vec a, float b)
        {
            return new Vec((from v in a.values select v - b).ToArray());
        }
        public static Vec operator *(Vec a, float b)
        {
            return new Vec((from v in a.values select v * b).ToArray());
        }
        public static Vec operator /(Vec a, float b)
        {
            return new Vec((from v in a.values select v / b).ToArray());
        }


        // index
        public float this[int index]
        {
            get { return this.values[index]; }
            set { this.values[index] = value; }
        }


        // convert
        public static implicit operator float[](Vec data)
        {
            return data.values;
        }
        public static implicit operator Vec(float[] data)
        {
            return new Vec(data);
        }

        public float[] ToArray()
        {
            return this.values.ToArray();
        }
    }

    public class Pos : VecInteface
    {
        float _x, _y, _z;

        public Pos(float x, float y, float z)
        {
            _x = x;
            _y = y;
            _z = z;
        }
        public Pos(Vec data): this(data.x, data.y, data.z) { }

        public override string ToString()
        {
            return $"Pos({string.Join(", ", this.ToArray())})";
        }

        // params
        public int Length
        {
            get { return 3; }
        }

        public float x
        {
            get { return _x; }
            set { _x = value; }
        }
        public float y
        {
            get { return _y; }
            set { _y = value; }
        }
        public float z
        {
            get { return _z; }
            set { _z = value; }
        }

        // pos
        public static Pos operator +(Pos a, Pos b)
        {
            return new Pos(
                a.x + b.x, 
                a.y + b.y,
                a.z + b.z
            );
        }
        public static Pos operator -(Pos a, Pos b)
        {
            return new Pos(
                a.x - b.x, 
                a.y - b.y,
                a.z - b.z
            );
        }
        public static Pos operator *(Pos a, Pos b)
        {
            return new Pos(
                a.x * b.x, 
                a.y * b.y,
                a.z * b.z
            );
        }

        public static Pos operator /(Pos a, Pos b)
        {
            return new Pos(
                a.x / b.x, 
                a.y / b.y,
                a.z / b.z
            );
        }

        // index
        public float this[int index]
        {
            get 
            {
                switch (index)
                {
                    case 0: return x;
                    case 1: return y;
                    case 2: return z;
                    default: throw new Exception();
                }
            }
            set 
            {
                switch (index)
                {
                    case 0: 
                        x = value; 
                        break;
                    case 1: 
                        y = value; 
                        break;
                    case 2:
                        z = value;
                        break;
                    default: throw new Exception();
                }
            
            }
        }

        // convert
        public static implicit operator float[](Pos data)
        {
            return data.ToArray();
        }
        public static implicit operator Pos(float[] data)
        {
            return new Pos(data[0], data[1], data[2]);
        }

        public float[] ToArray()
        {
            return [_x, _y, _z];
        }
    }
}

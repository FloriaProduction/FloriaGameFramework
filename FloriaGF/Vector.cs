using System;
using DotGLFW;
using static DotGL.GL;

namespace FloriaGF
{
    class Vec
    {
        private float[] values;
        
        public Vec(float[] values)
        {
            this.values = values;
        }
        public Vec() : this([]) { }
        public Vec(float a, float b) : this([a, b]) { }
        public Vec(float a, float b, float c) : this([a, b, c]) { }
        public Vec(float a, float b, float c, float d) : this([a, b, c, d]) { }


        // params
        public float vec_length
        {
            get { return (float)Math.Sqrt((double)(from v in this.values select v*v).Sum()); }
        }

        public Vec unit_vec
        {
            get { return this / this.vec_length; }
        }

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

        private static Vec calculateVec(float[] a, float[] b, byte operation_id)
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

        private static float calculateScalarVec(float[] a, float[] b, byte operation_id)
        {
            for (int i = 0;i < a.Length; i++)
            {
                switch (operation_id)
                {
                    case 0:
                        a[i] += b[i];
                        break;

                    case 1:
                        a[i] -= b[i];
                        break;

                    case 2:
                        a[i] *= b[i];
                        break;

                    case 3:
                        a[i] /= b[i];
                        break;
                }
            }
            return a.Sum();
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

        public float[] Copy()
        {
            return this.values.ToArray();
        }
    }
}

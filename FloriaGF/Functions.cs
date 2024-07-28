using System;

namespace FloriaGF
{
    public static class Func
    {
        public static float smooth(float a, float b, float x, float min_dif = 0)
        {
            float result = a + (b - a) * x;
            if (Math.Abs(b - result) <= min_dif)
                return b;
            return result;
        }
    }
}

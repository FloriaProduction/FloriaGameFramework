using System;
using System.Text.Json;

namespace FloriaGF
{
    public static class Func
    {
        public static float smooth(float a, float b, float x, float min_dif = 0.01f)
        {
            float result = a + (b - a) * x;
            if (Math.Abs(b - result) <= min_dif)
                return b;
            return result;
        }

        public static Dictionary<TKey, TValue> Join<TKey, TValue>(this Dictionary<TKey, TValue> dict, Dictionary<TKey, TValue> sub_dict) where TKey : notnull
        {
            Dictionary<TKey, TValue> new_dict = new();
            foreach (var key in dict.Keys)
            {
                new_dict[key] = dict[key];
            }
            foreach (var key in sub_dict.Keys)
            {
                new_dict[key] = sub_dict[key];
            }

            return new_dict;
        }

        public static JsonElement getProperty(JsonElement el, string param)
        {
            if (el.TryGetProperty(param, out var value))
                return value;
            throw new Exception();
        }
    }
}

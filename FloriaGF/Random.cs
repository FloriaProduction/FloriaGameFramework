using System;

namespace FloriaGF
{
    public static class RandomGF
    {
        static Random _random = new Random();

        public static int randint(int start, int end)
        {
            return _random.Next(start, end);
        }

        public static float randfloat(int start, int end, int count = 3)
        {
            return _random.Next(start*(10*count), end*(10*count))/(10*count);
        }

        public static T choise<T>(T[] array)
        {
            return array[randint(0, array.Length)];
        }
    }
}

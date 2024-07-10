using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FloriaGF
{
    class IDM<T>
    {
        bool reuse;
        uint counter = 0;
        Dictionary<uint, T?> dict = new();
        uint[]? keys_cache;

        public IDM(bool reuse)
        {
            this.reuse = reuse;
        }

        public uint add(T obj)
        {
            if (obj == null) throw new Exception("");

            keys_cache = null;

            uint? tkey = null;

            if (reuse)
                foreach (var key in dict.Keys)
                    if (dict[key] == null)
                    {
                        tkey = key;
                        break;
                    }

            if (!reuse || !tkey.HasValue)
            {
                dict[counter] = obj;
                return counter++;
            }
            else
            {
                dict[(uint)tkey] = obj;
                return (uint)tkey;
            }
        }
        public void remove(uint id)
        {
            keys_cache = null;

            if (reuse)
                dict[id] = default;
            else
                dict.Remove(id);
        }
        public T get(uint id)
        {
            return dict[id] ?? throw new Exception();
        }
        public uint[] getKeys()
        {
            if (keys_cache == null)
            {
                List<uint> keys = [];

                if (reuse)
                {
                    foreach (var key in dict.Keys)
                        if (dict[key] != null)
                            keys.Add(key);
                }
                else keys.AddRange(dict.Keys);

                //if (sorted) keys.Sort();

                keys_cache = keys.ToArray();
            }

            return keys_cache;
        }
        public T[] getValues()
        {
            if (reuse)
                return (from id in this.getKeys() select dict[id]).ToArray();
            else 
                return dict.Values.ToArray();
        }
        public uint getCount()
        {
            if (reuse)
            {
                uint count = 0;
                foreach (var key in dict.Keys)
                    if (dict[key] != null)
                        count++;
                return count;
            }
            else return (uint)dict.Count;
        }

        public uint[] Keys
        {
            get
            {
                return getKeys();
            }
        }

        public T[] Values
        {
            get
            {
                return getValues();
            }
        }
        public uint Count
        {
            get
            {
                return getCount();
            }
        }

        public T this[uint id]
        {
            get { return this.get(id); }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FloriaGF
{
    class idm<T>
    {
        bool reuse;
        ulong counter = 0;
        Dictionary<ulong, T?> dict = new();

        public idm(bool reuse)
        {
            this.reuse = reuse;
        }

        public ulong add(T obj)
        {
            if (obj == null) throw new Exception("");

            ulong? tkey = null;

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
                dict[(ulong)tkey] = obj;
                return (ulong)tkey;
            }
        }
        public void remove(ulong id)
        {
            if (reuse)
                dict[id] = default;
            else
                dict.Remove(id);
        }
        public T get(ulong id)
        {
            if (dict.ContainsKey(id) && dict[id] != null)
                return dict[id];
            else
                throw new Exception();
        }


        public float this[ulong id]
        {
            get { return this.get(id); }
        }
    }
}

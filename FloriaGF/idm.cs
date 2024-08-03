using System;

namespace FloriaGF
{
    /// <summary>
    /// IDM(id - менеджер)
    /// </summary>
    class IDM<T>
    {
        bool _reuse;
        uint _counter = 0;
        Dictionary<uint, T?> _dict = new();
        uint[]? _keys_cache;
        uint? _dict_count;

        /// <param name="reuse">использовать старые id для новых объектов</param>
        public IDM(bool reuse = false)
        {
            this._reuse = reuse;
        }

        public uint add(T obj)
        {
            if (obj == null) throw new Exception("");

            _keys_cache = null;
            _dict_count = null;

            uint? tkey = null;

            if (_reuse)
                foreach (var key in _dict.Keys)
                    if (_dict[key] == null)
                    {
                        tkey = key;
                        break;
                    }

            if (!_reuse || !tkey.HasValue)
            {
                _dict[_counter] = obj;
                return _counter++;
            }
            else
            {
                _dict[(uint)tkey] = obj;
                return (uint)tkey;
            }
        }
        public void remove(uint id)
        {
            _keys_cache = null;
            _dict_count = null;

            if (_reuse)
                _dict[id] = default;
            else
                _dict.Remove(id);
        }
        public T get(uint id)
        {
            return _dict[id] ?? throw new Exception();
        }
        public uint[] getKeys()
        {
            if (_keys_cache == null)
            {
                List<uint> keys = [];

                if (_reuse)
                {
                    foreach (var key in _dict.Keys)
                        if (_dict[key] != null)
                            keys.Add(key);
                }
                else keys.AddRange(_dict.Keys);

                //if (sorted) keys.Sort();

                _keys_cache = keys.ToArray();
            }

            return _keys_cache;
        }
        public T[] getValues()
        {
            if (_reuse)
                return (from id in this.getKeys() select _dict[id]).ToArray();
            else 
                return [.._dict.Values];
        }
        public uint getCount()
        {
            if (_dict_count == null)
            {
                if (_reuse)
                {
                    uint count = 0;
                    foreach (var key in _dict.Keys)
                        if (_dict[key] != null) count++;
                    _dict_count = count;
                }
                else _dict_count = (uint)_dict.Count;
            }

            return (uint)_dict_count;
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

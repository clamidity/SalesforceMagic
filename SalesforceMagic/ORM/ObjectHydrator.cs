using System;
using System.Collections.Generic;
using FastMember;

namespace SalesforceMagic.ORM
{
    public static class ObjectHydrator
    {
        private static object _lock = new object();
        internal static IDictionary<string, TypeAccessor> CachedAccessors = new Dictionary<string, TypeAccessor>();

        public static TypeAccessor GetAccessor(Type type)
        {
            lock (_lock)
            {
                string name = type.Name;
                if (CachedAccessors.ContainsKey(name)) return CachedAccessors[name];
                TypeAccessor accessor = TypeAccessor.Create(type);
                CachedAccessors.Add(name, accessor);

                return accessor;
            }
        }
    }
}
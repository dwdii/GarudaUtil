using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Garuda.Data
{
    class PhoenixIsolationLevelMap
    {
        static Dictionary<IsolationLevel, uint> _map = new Dictionary<IsolationLevel, uint>();

        static PhoenixIsolationLevelMap()
        {
            _map.Add(IsolationLevel.Chaos, 0);
            _map.Add(IsolationLevel.ReadUncommitted, 1);
            _map.Add(IsolationLevel.ReadCommitted, 2);
            _map.Add(IsolationLevel.RepeatableRead, 4);
            _map.Add(IsolationLevel.Serializable, 8);
        }

        public static uint GetPhoenixLevel(IsolationLevel il)
        {
            return _map[il];
        }

        public static IsolationLevel GetIsolationLevel(uint level)
        {
            IsolationLevel il = IsolationLevel.Unspecified;
            foreach (var k in _map)
            {
                if (k.Value == level)
                {
                    il = k.Key;
                    break;
                }
            }

            return il;
        }
    }
}

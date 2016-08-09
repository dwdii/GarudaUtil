﻿using Apache.Phoenix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Garuda.Data
{
    class PhoenixDataTypeMap
    {
        static Dictionary<Rep, Type> _map = new Dictionary<Rep, Type>();

        public static List<string> _dateTypes = new List<string>();

        public static List<string> _timeTypes = new List<string>();

        static PhoenixDataTypeMap()
        {
            _map.Add(Rep.LONG, typeof(long));
            _map.Add(Rep.PRIMITIVE_LONG, typeof(long));
            _map.Add(Rep.BIG_INTEGER, typeof(long));
            _map.Add(Rep.INTEGER, typeof(int));
            _map.Add(Rep.PRIMITIVE_INT, typeof(int));
            _map.Add(Rep.STRING, typeof(string));
            _map.Add(Rep.BOOLEAN, typeof(bool));
            _map.Add(Rep.PRIMITIVE_BOOLEAN, typeof(bool));
            _map.Add(Rep.FLOAT, typeof(float));
            _map.Add(Rep.PRIMITIVE_FLOAT, typeof(float));
            _map.Add(Rep.DOUBLE, typeof(double));

            _dateTypes.Add("TIMESTAMP");
            _dateTypes.Add("DATE");
            _timeTypes.Add("TIME");

        }

        public static Type GetDotNetType(Rep rep)
        {
            return _map[rep];
        }

        public static Type GetDotNetType(Rep rep, string typeName)
        {
            Type t;
            if(_dateTypes.Contains(typeName))
            {
                t = typeof(DateTime);
            }
            else if (_timeTypes.Contains(typeName))
            {
                t = typeof(TimeSpan);
            }
            else if(Rep.OBJECT == rep)
            {
                switch(typeName)
                {
                    case "DECIMAL":
                        t = typeof(decimal);
                        break;

                    default:
                        t = typeof(object);
                        break;
                }
            }
            else
            {
                t = GetDotNetType(rep); 
            }

            return t;
        }

        public static Rep GetPhoenixRep(Type type)
        {
            Rep rep = Rep.NULL;
            foreach (var k in _map)
            {
                if (k.Value == type)
                {
                    rep = k.Key;
                    break;
                }
            }

            return rep;
        }
    }
}
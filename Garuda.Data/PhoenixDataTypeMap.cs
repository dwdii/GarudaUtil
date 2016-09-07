using Apache.Phoenix;
using Garuda.Data.Internal;
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
            _map.Add(GarudaRep.Long, typeof(long));
            _map.Add(GarudaRep.PrimitiveLong, typeof(long));
            _map.Add(GarudaRep.BigInteger, typeof(long));

            _map.Add(GarudaRep.Integer, typeof(int));
            _map.Add(GarudaRep.PrimitiveInt, typeof(int));

            _map.Add(GarudaRep.Short, typeof(short));
            _map.Add(GarudaRep.PrimitiveShort, typeof(short));

            _map.Add(GarudaRep.String, typeof(string));
            _map.Add(GarudaRep.ByteString, typeof(string));


            _map.Add(GarudaRep.Boolean, typeof(bool));
            _map.Add(GarudaRep.PrimitiveBoolean, typeof(bool));

            _map.Add(GarudaRep.Float, typeof(float));
            _map.Add(GarudaRep.PrimitiveFloat, typeof(float));

            _map.Add(GarudaRep.Double, typeof(double));
            _map.Add(GarudaRep.PrimitiveDouble, typeof(double));

            _map.Add(GarudaRep.PrimitiveByte, typeof(byte));

            _map.Add(GarudaRep.Array, typeof(Array));


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
            else if(GarudaRep.Object == rep)
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
            Rep rep = GarudaRep.Null;
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

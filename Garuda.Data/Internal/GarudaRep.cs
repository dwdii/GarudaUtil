using Apache.Phoenix;

namespace Garuda.Data.Internal
{
    /// <summary>
    /// Encapsulates backward/forward compatibility code related to the Apache.Phoenix.Rep enum.
    /// </summary>
    internal class GarudaRep
    {
#if GPB3
        public const Rep String = Rep.String;

        public const Rep ByteString = Rep.ByteString;

        public const Rep Double = Rep.Double;

        public const Rep PrimitiveDouble = Rep.PrimitiveDouble;

        public const Rep Float = Rep.Float;

        public const Rep PrimitiveFloat = Rep.PrimitiveFloat;

        public const Rep Boolean = Rep.Boolean;

        public const Rep PrimitiveBoolean = Rep.PrimitiveBoolean;

        public const Rep Byte = Rep.Byte;

        public const Rep PrimitiveByte = Rep.PrimitiveByte;

        public const Rep Short = Rep.Short;

        public const Rep PrimitiveShort = Rep.PrimitiveShort;

        public const Rep Integer = Rep.Integer;

        public const Rep PrimitiveInt = Rep.PrimitiveInt;

        public const Rep Long = Rep.Long;

        public const Rep PrimitiveLong = Rep.PrimitiveLong;

        public const Rep BigInteger = Rep.BigInteger;

        public const Rep Number = Rep.Number;

        public const Rep Null = Rep.Null;

        public const Rep Array = Rep.Array;

        public const Rep Object = Rep.Object;

        public const Rep JavaSqlTimestamp = Rep.JavaSqlTimestamp;
#elif GPB3alpha
        public const Rep String = Rep.STRING;

        public const Rep ByteString = Rep.BYTE_STRING;

        public const Rep Double = Rep.DOUBLE;

        public const Rep PrimitiveDouble = Rep.PRIMITIVE_DOUBLE;

        public const Rep Float = Rep.FLOAT;

        public const Rep PrimitiveFloat = Rep.PRIMITIVE_FLOAT;

        public const Rep Boolean = Rep.BOOLEAN;

        public const Rep PrimitiveBoolean = Rep.PRIMITIVE_BOOLEAN;

        public const Rep Byte = Rep.BYTE;

        public const Rep PrimitiveByte = Rep.PRIMITIVE_BYTE;

        public const Rep Short = Rep.SHORT;

        public const Rep PrimitiveShort = Rep.PRIMITIVE_SHORT;

        public const Rep Integer = Rep.INTEGER;

        public const Rep PrimitiveInt = Rep.PRIMITIVE_INT;

        public const Rep Long = Rep.LONG;

        public const Rep PrimitiveLong = Rep.PRIMITIVE_LONG;

        public const Rep BigInteger = Rep.BIG_INTEGER;

        public const Rep Number = Rep.NUMBER;

        public const Rep Null = Rep.NULL;

        public const Rep Array = Rep.ARRAY;

        public const Rep Object = Rep.OBJECT;

        public const Rep JavaSqlTimestamp = Rep.JAVA_SQL_TIMESTAMP;
#endif
    }
}
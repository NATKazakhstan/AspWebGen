using System;
using System.Data;

using Nat.Tools.Specific;

/// <summary>
///     Summary description for SqlDbTypeConverter
/// </summary>
public class SqlDbTypeConverter : IDbTypeConverter
{
    public static readonly SqlDbTypeConverter Instance;

    static SqlDbTypeConverter()
    {
        Instance = new SqlDbTypeConverter();
    }

    #region IDbTypeConverter Members

    public DbType SystemTypeToDBType(Type type)
    {
        if (type == typeof(string))
            return DbType.String;
        if (type == typeof(string))
            return DbType.AnsiStringFixedLength;
        if (type == typeof(byte[]))
            return DbType.Binary;
        if (type == typeof(bool))
            return DbType.Boolean;
        if (type == typeof(byte))
            return DbType.Byte;
        if (type == typeof(double))
            return DbType.Currency;
        if (type == typeof(DateTime))
            return DbType.Date;
        if (type == typeof(DateTime))
            return DbType.DateTime;
        if (type == typeof(decimal))
            return DbType.Decimal;
        if (type == typeof(double))
            return DbType.Double;
        if (type == typeof(Guid))
            return DbType.Guid;
        if (type == typeof(short))
            return DbType.Int16;
        if (type == typeof(int))
            return DbType.Int32;
        if (type == typeof(long))
            return DbType.Int64;
        if (type == typeof(object))
            return DbType.Object;
        if (type == typeof(sbyte))
            return DbType.SByte;
        if (type == typeof(float))
            return DbType.Single;
        if (type == typeof(string))
            return DbType.AnsiString;
        if (type == typeof(string))
            return DbType.StringFixedLength;
        if (type == typeof(TimeSpan))
            return DbType.Time;
        if (type == typeof(ushort))
            return DbType.UInt16;
        if (type == typeof(uint))
            return DbType.UInt32;
        if (type == typeof(ulong))
            return DbType.UInt64;
        if (type == typeof(double))
            return DbType.VarNumeric;
        throw new Exception("Cannot convert SystemType to DbType: unknown type.");
    }

    #endregion
}
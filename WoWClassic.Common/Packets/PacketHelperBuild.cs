﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace WoWClassic.Common.Packets
{
    public static partial class PacketHelper
    {
        public static byte[] Build<T>(T instance) where T : class, new()
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                Build(bw, typeof(T), instance);

                return ms.ToArray();
            }
        }

        private static void Build(BinaryWriter bw, Type type, object instance)
        {
            foreach (var fi in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).OrderBy(f => f.MetadataToken))
                BuildField(bw, fi.FieldType, fi.GetValue(instance), fi.GetCustomAttributes());
        }

        private static void BuildField(BinaryWriter bw, Type fieldType, object value, IEnumerable<Attribute> attributes)
        {
            if (value == null)
                return; // TODO: Error handling?

            if (fieldType.IsEnum)
                BuildPrimitive(bw, Type.GetTypeCode(fieldType.GetEnumUnderlyingType()), value, attributes);
            else if (fieldType.IsClass && (!fieldType.IsArray && fieldType != typeof(string)))
                Build(bw, fieldType, value);
            else if (fieldType.IsArray)
                BuildArray(bw, (Array)value, attributes);
            else if (fieldType.IsPrimitive || fieldType == typeof(string))
                BuildPrimitive(bw, Type.GetTypeCode(fieldType), value, attributes);
            else
                throw new Exception("BuildField unable to determine data type");
        }

        private static void BuildArray(BinaryWriter bw, Array arr, IEnumerable<Attribute> attributes)
        {
            var eleType = arr.GetType().GetElementType();
            foreach (var ele in arr)
                BuildField(bw, eleType, ele, attributes);
        }

        private static void BuildPrimitive(BinaryWriter bw, TypeCode primitiveType, object value, IEnumerable<Attribute> attributes)
        {
            switch (primitiveType)
            {
                case TypeCode.Boolean:
                    bw.Write((bool)value);
                    break;
                case TypeCode.Byte:
                    bw.Write((byte)value);
                    break;
                case TypeCode.Char:
                    bw.Write((char)value);
                    break;
                case TypeCode.Double:
                    bw.Write((double)value);
                    break;
                case TypeCode.Int16:
                    bw.Write((short)value);
                    break;
                case TypeCode.Int32:
                    bw.Write((int)value);
                    break;
                case TypeCode.Int64:
                    bw.Write((long)value);
                    break;
                case TypeCode.SByte:
                    bw.Write((sbyte)value);
                    break;
                case TypeCode.Single:
                    bw.Write((float)value);
                    break;
                case TypeCode.UInt16:
                    bw.Write((ushort)value);
                    break;
                case TypeCode.UInt32:
                    bw.Write((uint)value);
                    break;
                case TypeCode.UInt64:
                    bw.Write((ulong)value);
                    break;
                case TypeCode.String:
                    var sAttr = attributes.OfType<PacketStringAttribute>().FirstOrDefault();
                    if (sAttr == null) throw new Exception("BuildPrimitive<string> Missing attribute");

                    switch (sAttr.StringType)
                    {
                        case StringTypes.CString:
                            bw.Write(Encoding.ASCII.GetBytes((string)value + '\0'));
                            break;
                        case StringTypes.PrefixedLength:
                            var str = (string)value;
                            bw.Write((byte)str.Length);
                            bw.Write(Encoding.ASCII.GetBytes(str));
                            break;
                        case StringTypes.FixedLength:
                            var aAttr = attributes.OfType<PacketArrayLengthAttribute>().FirstOrDefault();
                            if (aAttr == null) throw new Exception("BuildPrimitive<string> Missing fixed-length attribute");

                            bw.Write(Encoding.ASCII.GetBytes((string)value).Pad(aAttr.Length));
                            break;
                    }
                    break;
                default:
                    // Throw exception?
                    // Means we got a type we don't know how to parse
                    throw new Exception($"BuildPrimitive unhandled primitive '{primitiveType}'");
            }
        }
    }
}

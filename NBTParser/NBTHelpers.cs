using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace NBTParser
{
    public static class NBTHelpers
    {
        private static Dictionary<TagType, Func<GZipStream, string, long, ITag>> tagFuncDict = new Dictionary<TagType, Func<GZipStream, string, long, ITag>>()
        {
            { TagType.TAG_End, GetTagNull },
            { TagType.TAG_Byte, GetTagByte },
            { TagType.TAG_Short, GetTagShort },
            { TagType.TAG_Int, GetTagInt },
            { TagType.TAG_Long, GetTagLong },
            { TagType.TAG_Float, GetTagFloat },
            { TagType.TAG_Double, GetTagDouble },
            { TagType.TAG_Byte_Array, GetTagByteArray },
            { TagType.TAG_String, GetTagString },
            { TagType.TAG_List, GetTagList },
            { TagType.TAG_Compound, GetTagCompound },
            { TagType.TAG_Int_Array, GetTagIntArray },
            { TagType.TAG_Long_Array, GetTagLongArray }
        };

        private static TagType[] primitiveTags = new TagType[]
        {
            TagType.TAG_End, TagType.TAG_Byte, TagType.TAG_Short, TagType.TAG_Int, TagType.TAG_Long, TagType.TAG_Float, TagType.TAG_Double, TagType.TAG_String
        };

        /// <summary>
        /// Get an NBT tag, recursively loading each contained tag
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static ITag GetTag(GZipStream stream, long depth)
        {
            var byteType = stream.ReadByte();
            // Reached the end of the stream, return null
            if (byteType == -1)
            {
                return null;
            }

            var type = (TagType)byteType;
            if (!Enum.IsDefined(typeof(TagType), type))
            {
                // Value not defined in the list of known tags, something went wrong
                throw new Exception("Invalid NBT file, some of the tags being used are not defined.");
            }

            // If the tag is not a TAG_end, it has a name
            string name = null;
            if (type != TagType.TAG_End)
            {
                name = GetTagName(stream);
            }

            Console.WriteLine($"{String.Concat(Enumerable.Repeat("\t", (int)depth))} Type: {type} Name: {name}");

            ITag tag = null;
            if (tagFuncDict.ContainsKey(type))
            {
                tag = tagFuncDict[type](stream, name, depth);
            }

            if (primitiveTags.Contains(tag.Type))
            {
                Console.WriteLine($"{String.Concat(Enumerable.Repeat("\t", (int)depth))} Payload: {tag.PayloadGeneric}");
            }

            return tag;
        }

        private static TagEnd GetTagNull(GZipStream stream, string name, long depth)
        {
            return new TagEnd()
            {
                Name = name,
                Depth = depth
            };
        }

        private static TagByte GetTagByte(GZipStream stream, string name, long depth)
        {
            var payloadBytes = new byte[1];
            ReadBytes(stream, payloadBytes);
            var payload = (sbyte)payloadBytes[0];

            return new TagByte()
            {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private static TagShort GetTagShort(GZipStream stream, string name, long depth)
        {
            var payloadBytes = new byte[2];
            ReadBytes(stream, payloadBytes);
            var payload = BitConverter.ToInt16(payloadBytes, 0);

            return new TagShort()
            {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private static TagInt GetTagInt(GZipStream stream, string name, long depth)
        {
            var payloadBytes = new byte[4];
            ReadBytes(stream, payloadBytes);
            var payload = BitConverter.ToInt32(payloadBytes, 0);

            return new TagInt()
            {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private static TagLong GetTagLong(GZipStream stream, string name, long depth)
        {
            var payloadBytes = new byte[8];
            ReadBytes(stream, payloadBytes);
            var payload = BitConverter.ToInt64(payloadBytes, 0);

            return new TagLong()
            {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private static TagFloat GetTagFloat(GZipStream stream, string name, long depth)
        {
            var payloadBytes = new byte[4];
            ReadBytes(stream, payloadBytes);
            var payload = BitConverter.ToSingle(payloadBytes, 0);

            return new TagFloat()
            {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private static TagDouble GetTagDouble(GZipStream stream, string name, long depth)
        {
            var payloadBytes = new byte[8];
            ReadBytes(stream, payloadBytes);
            var payload = BitConverter.ToDouble(payloadBytes, 0);

            return new TagDouble()
            {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private static TagByteArray GetTagByteArray(GZipStream stream, string name, long depth)
        {
            var tagSize = GetTagInt(stream, "_length", depth + 1);
            var payload = new List<TagByte>();

            for (var i = 0; i < tagSize.Payload; i++)
            {
                TagByte tag = GetTagByte(stream, $"_{i}", depth + 1);
                payload.Add(tag);
            }

            return new TagByteArray()
            {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private static TagString GetTagString(GZipStream stream, string name, long depth)
        {
            var tagSize = GetTagShort(stream, "_length", depth + 1);
            if (tagSize == null)
            {
                return null;
            }

            var payload = GetStringOfLength(stream, tagSize.Payload);
            return new TagString()
            {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private static TagList<ITag> GetTagList(GZipStream stream, string name, long depth)
        {
            var byteType = GetTagByte(stream, "_type", depth + 1);
            var type = (TagType)byteType.Payload;

            var tagSize = GetTagInt(stream, "_length", depth + 1);

            var payload = new List<ITag>();
            for (var i = 0; i < tagSize.Payload; i++)
            {
                var func = tagFuncDict[type];
                ITag tag = func(stream, $"_{i}", depth + 1);
                payload.Add(tag);
            }

            return new TagList<ITag>()
            {
                Name = name,
                Payload = payload,
                ListType = type,
                Depth = depth
            };
        }

        private static TagCompund GetTagCompound(GZipStream stream, string name, long depth)
        {
            return new TagCompund()
            {
                Name = name,
                Payload = GetArrayOfTags(stream, depth),
                Depth = depth
            };
        }

        private static TagIntArray GetTagIntArray(GZipStream stream, string name, long depth)
        {
            var tagSize = GetTagInt(stream, "_length", depth + 1);

            var payload = new List<TagInt>();
            for (var i = 0; i < tagSize.Payload; i++)
            {
                var tag = GetTagInt(stream, $"_{i}", depth + 1);
                payload.Add(tag);
            }

            return new TagIntArray()
            {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private static TagLongArray GetTagLongArray(GZipStream stream, string name, long depth)
        {
            var tagSize = GetTagInt(stream, "_length", depth + 1);

            var payload = new List<TagLong>();
            for (var i = 0; i < tagSize.Payload; i++)
            {
                var tag = GetTagLong(stream, $"_{i}", depth + 1);
                payload.Add(tag);
            }

            return new TagLongArray()
            {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        /// <summary>
        /// Gets an array of tags until it reaches an end tag
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static List<ITag> GetArrayOfTags(GZipStream stream, long depth)
        {
            var tags = new List<ITag>();
            var nextTag = GetTag(stream, depth + 1);
            while (nextTag != null && nextTag.Type != TagType.TAG_End)
            {
                tags.Add(nextTag);
                nextTag = GetTag(stream, depth + 1);
            }

            return tags;
        }

        /// <summary>
        /// Get the name of the tag (2 Byte specifying the name length in bytes, followed by a utf-8 encoded string)
        /// </summary>
        /// <param name="stream">The GZip decompressed filestream</param>
        /// <returns></returns>
        private static string GetTagName(GZipStream stream)
        {
            string name;
            var bytes = new byte[2];
            ReadBytes(stream, bytes);

            var nameLength = BitConverter.ToUInt16(bytes, 0);
            name = GetStringOfLength(stream, nameLength);
            return name;
        }

        /// <summary>
        /// Get a string of a given length from the stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="nameLength"></param>
        /// <returns></returns>
        private static string GetStringOfLength(GZipStream stream, ushort nameLength)
        {
            string name;
            var nameBytes = new byte[nameLength];
            stream.Read(nameBytes, 0, nameLength);
            name = Encoding.UTF8.GetString(nameBytes);
            return name;
        }

        /// <summary>
        /// Get a string of a given length from the stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="nameLength"></param>
        /// <returns></returns>
        private static string GetStringOfLength(GZipStream stream, short nameLength)
        {
            var length = Convert.ToUInt16(nameLength);
            return GetStringOfLength(stream, length);
        }

        /// <summary>
        /// Read as many bytes as needed to fill the array, with the correct endianess
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bytes"></param>
        private static void ReadBytes(GZipStream stream, byte[] bytes)
        {
            stream.Read(bytes, 0, bytes.Length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
        }

    }
}

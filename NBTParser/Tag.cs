using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NBTParser
{
    public interface ITag
    {
        string Name { get; set; }
        TagType Type { get; }
        long Depth { get; set; }
        object PayloadGeneric { get; }
        string ToPrettyString();
    }

    public enum TagType
    {
        TAG_End = 0,
        TAG_Byte = 1,
        TAG_Short = 2,
        TAG_Int = 3,
        TAG_Long = 4,
        TAG_Float = 5,
        TAG_Double = 6,
        TAG_Byte_Array = 7,
        TAG_String = 8,
        TAG_List = 9,
        TAG_Compound = 10,
        TAG_Int_Array = 11,
        TAG_Long_Array = 12
    }

    public abstract class Tag<T> : ITag
    {
        public string Name { get; set; }
        public abstract TagType Type { get; }
        public abstract T Payload { get; set; }
        public object PayloadGeneric
        {
            get
            {
                return this.Payload;
            }
        }
        public long Depth { get; set; }
        public override string ToString()
        {
            return $"Depth: {Depth} Type: {Type} Name: {Name}";
        }

        public string ToPrettyString()
        {
            return $"{String.Concat(Enumerable.Repeat("\t", (int)Depth))} Type: {Type} Name: {Name}";
        }
    }


    public class TagEnd : Tag<object>
    {
        public override TagType Type
        {
            get
            {
                return TagType.TAG_End;
            }
        }
        public override object Payload { get { return null; } set { } }
    }

    public class TagByte : Tag<sbyte>
    {
        public override TagType Type
        {
            get
            {
                return TagType.TAG_Byte;
            }
        }
        public override sbyte Payload { get; set; }
    }

    public class TagShort : Tag<short>
    {
        public override TagType Type
        {
            get
            {
                return TagType.TAG_Short;
            }
        }
        public override short Payload { get; set; }
    }

    public class TagInt : Tag<int>
    {
        public override TagType Type
        {
            get
            {
                return TagType.TAG_Int;
            }
        }
        public override int Payload { get; set; }
    }

    public class TagLong : Tag<long>
    {
        public override TagType Type
        {
            get
            {
                return TagType.TAG_Long;
            }
        }
        public override long Payload { get; set; }
    }

    public class TagFloat : Tag<float>
    {
        public override TagType Type
        {
            get
            {
                return TagType.TAG_Float;
            }
        }
        public override float Payload { get; set; }
    }

    public class TagDouble : Tag<double>
    {
        public override TagType Type
        {
            get
            {
                return TagType.TAG_Double;
            }
        }
        public override double Payload { get; set; }
    }

    public class TagByteArray : Tag<IEnumerable<TagByte>>
    {
        public override TagType Type
        {
            get
            {
                return TagType.TAG_Byte_Array;
            }
        }
        public override IEnumerable<TagByte> Payload { get; set; }
    }

    public class TagString : Tag<string>
    {
        public override TagType Type
        {
            get
            {
                return TagType.TAG_String;
            }
        }
        public override string Payload { get; set; }
    }

    public class TagList<T> : Tag<IEnumerable<T>>
    {
        public override TagType Type
        {
            get
            {
                return TagType.TAG_List;
            }
        }
        public TagType ListType { get; set; }
        public override IEnumerable<T> Payload { get; set; }
    }

    public class TagCompund : Tag<IEnumerable<ITag>>
    {
        public override TagType Type
        {
            get
            {
                return TagType.TAG_Compound;
            }
        }
        public override IEnumerable<ITag> Payload { get; set; }
    }

    public class TagIntArray : Tag<IEnumerable<TagInt>>
    {
        public override TagType Type
        {
            get
            {
                return TagType.TAG_Int_Array;
            }
        }
        public override IEnumerable<TagInt> Payload { get; set; }
    }

    public class TagLongArray : Tag<IEnumerable<TagLong>>
    {
        public override TagType Type
        {
            get
            {
                return TagType.TAG_Long_Array;
            }
        }
        public override IEnumerable<TagLong> Payload { get; set; }
    }
}

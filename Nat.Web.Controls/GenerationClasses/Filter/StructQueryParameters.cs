using System;

namespace Nat.Web.Controls.GenerationClasses.Filter
{
    public struct StructQueryParameters
    {
        public object[] ParametersValues { get; set; }

        public Item Parameter0 { get { return new Item { Value = ParametersValues[0] }; } }
        public Item Parameter1 { get { return new Item { Value = ParametersValues[1] }; } }
        public Item Parameter2 { get { return new Item { Value = ParametersValues[2] }; } }
        public Item Parameter3 { get { return new Item { Value = ParametersValues[3] }; } }
        public Item Parameter4 { get { return new Item { Value = ParametersValues[4] }; } }
        public Item Parameter5 { get { return new Item { Value = ParametersValues[5] }; } }
        public Item Parameter6 { get { return new Item { Value = ParametersValues[6] }; } }
        public Item Parameter7 { get { return new Item { Value = ParametersValues[7] }; } }
        public Item Parameter8 { get { return new Item { Value = ParametersValues[8] }; } }
        public Item Parameter9 { get { return new Item { Value = ParametersValues[9] }; } }
        public Item Parameter10 { get { return new Item { Value = ParametersValues[10] }; } }
        public Item Parameter11 { get { return new Item { Value = ParametersValues[11] }; } }
        public Item Parameter12 { get { return new Item { Value = ParametersValues[12] }; } }
        public Item Parameter13 { get { return new Item { Value = ParametersValues[13] }; } }
        public Item Parameter14 { get { return new Item { Value = ParametersValues[14] }; } }
        public Item Parameter15 { get { return new Item { Value = ParametersValues[15] }; } }
        public Item Parameter16 { get { return new Item { Value = ParametersValues[16] }; } }
        public Item Parameter17 { get { return new Item { Value = ParametersValues[17] }; } }
        public Item Parameter18 { get { return new Item { Value = ParametersValues[18] }; } }
        public Item Parameter19 { get { return new Item { Value = ParametersValues[19] }; } }
        
        public int StartRowIndex { get; set; }
        public int MaximumRows { get; set; }
        public object RefParent { get; set; }

        public long? RefParentLong { get { return (long?)RefParent; } }

        public struct Item
        {
            public Object Value { get; set; }

            public Byte Byte { get { return (Byte)Value; } }
            public Byte? ByteN { get { return (Byte?)Value; } }
            public Int32 Int32 { get { return (Int32)Value; } }
            public Int32? Int32N { get { return (Int32?)Value; } }
            public Int16 Int16 { get { return (Int16)Value; } }
            public Int16? Int16N { get { return (Int16?)Value; } }
            public Int64 Int64 { get { return (Int64)Value; } }
            public Int64? Int64N { get { return (Int64?)Value; } }
            public Double Double { get { return (Double)Value; } }
            public Double? DoubleN { get { return (Double?)Value; } }
            public Decimal Decimal { get { return (Decimal)Value; } }
            public Decimal? DecimalN { get { return (Decimal?)Value; } }
            public Single Single { get { return (Single)Value; } }
            public Single? SingleN { get { return (Single?)Value; } }
            public Boolean Boolean { get { return (Boolean)Value; } }
            public Boolean? BooleanN { get { return (Boolean?)Value; } }
            public DateTime DateTime { get { return (DateTime)Value; } }
            public DateTime? DateTimeN { get { return (DateTime?)Value; } }
            public String String { get { return (String)Value; } }
            public Char Char { get { return (Char)Value; } }
            public Char? CharN { get { return (Char?)Value; } }
        }
    }
}
namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    public interface IMultipleKey
    {
        bool Equals(IMultipleKey secondObj);
    }
    public struct MultipleKey<T1>
        //where T1: struct
    {
        public T1 id1;

        public static MultipleKey<T1> Create(T1 id1)
        {
            return new MultipleKey<T1> { id1 = id1, };
        }

        public bool IsEquals(MultipleKey<T1> second)
        {
            return id1.Equals(second.id1);
        }

        public override int GetHashCode()
        {
            return id1.GetHashCode();
        }

        public override string ToString()
        {
            return id1.ToString();
        }
    }
    public struct MultipleKey<T1, T2>
        //where T1 : struct
        //where T2 : struct
    {
        public T1 id1;
        public T2 id2;

        public static MultipleKey<T1, T2> Create(T1 id1, T2 id2)
        {
            return new MultipleKey<T1, T2> { id1 = id1, id2 = id2, };
        }

        public bool IsEquals(MultipleKey<T1, T2> second)
        {
            return id1.Equals(second.id1) && id2.Equals(second.id2);
        }

        public override int GetHashCode()
        {
            return id1.GetHashCode() + id2.GetHashCode();
        }

        public override string ToString()
        {
            return id1 + "," + id2;
        }
    }
    public struct MultipleKey<T1, T2, T3>
        //where T1 : struct
        //where T2 : struct
        //where T3 : struct
    {
        public T1 id1;
        public T2 id2;
        public T3 id3;

        public static MultipleKey<T1, T2, T3> Create(T1 id1, T2 id2, T3 id3)
        {
            return new MultipleKey<T1, T2, T3> { id1 = id1, id2 = id2, id3 = id3, };
        }

        public bool IsEquals(MultipleKey<T1, T2, T3> second)
        {
            return id1.Equals(second.id1) && id2.Equals(second.id2) && id3.Equals(second.id3);
        }

        public override int GetHashCode()
        {
            return id1.GetHashCode() + id2.GetHashCode() + id3.GetHashCode();
        }

        public override string ToString()
        {
            return id1 + "," + id2 + "," + id3;
        }
    }
    public struct MultipleKey<T1, T2, T3, T4>
        //where T1 : struct
        //where T2 : struct
        //where T3 : struct
        //where T4 : struct
    {
        public T1 id1;
        public T2 id2;
        public T3 id3;
        public T4 id4;

        public static MultipleKey<T1, T2, T3, T4> Create(T1 id1, T2 id2, T3 id3, T4 id4)
        {
            return new MultipleKey<T1, T2, T3, T4> { id1 = id1, id2 = id2, id3 = id3, id4 = id4, };
        }

        public bool IsEquals(MultipleKey<T1, T2, T3, T4> second)
        {
            return id1.Equals(second.id1) && id2.Equals(second.id2) && id3.Equals(second.id3) && id4.Equals(second.id4);
        }

        public override int GetHashCode()
        {
            return id1.GetHashCode() + id2.GetHashCode() + id3.GetHashCode() + id4.GetHashCode();
        }

        public override string ToString()
        {
            return id1 + "," + id2 + "," + id3 + "," + id4;
        }
    }
}

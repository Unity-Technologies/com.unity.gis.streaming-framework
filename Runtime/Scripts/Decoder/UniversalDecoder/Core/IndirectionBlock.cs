using System;
using System.Collections.Generic;

using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming
{
    //
    //  FIXME - Unit test adequately
    //
    //
    //  FIXME - Change to class, no reason to be a struct
    //
    public struct IndirectionBlock
    {
        public IndirectionBlock(List<int> indirectionTable, int blockId)
        {
            m_IndirectionTable = indirectionTable;
            m_Index = blockId;

            m_IsNull = m_Index < 0 || m_Index >= m_IndirectionTable.Count - BlockHeaderSize || m_IndirectionTable[m_Index] == -1;
        }

        public const int BlockHeaderSize = 2;
        public const int MinimumBlockSize = 8;

        private List<int> m_IndirectionTable;
        private int m_Index;
        private bool m_IsNull;

        public int BlockId
        {
            get { return m_Index; }
        }

        public int Capacity
        {
            get { return m_IndirectionTable[m_Index]; }
            private set { m_IndirectionTable[m_Index] = value; }
        }

        public int Count
        {
            get { return m_IndirectionTable[m_Index + 1]; }
            private set { m_IndirectionTable[m_Index + 1] = value; }
        }

        public int BlockSize
        {
            get => Capacity + BlockHeaderSize;
        }

        public int this[int i]
        {
            get
            {
                if (m_IsNull)
                    throw new InvalidOperationException("Cannot access invalid IndirectionBlock");

                if (i > Count)
                    throw new IndexOutOfRangeException();

                return m_IndirectionTable[m_Index + BlockHeaderSize + i];
            }

            private set
            {
                if (m_IsNull)
                    throw new InvalidOperationException("Cannot access invalid IndirectionBlock");

                if (i > Count)
                    throw new IndexOutOfRangeException();

                m_IndirectionTable[m_Index + BlockHeaderSize + i] = value;
            }
        }

        public void Add(int v)
        {
            if (Count >= Capacity)
                throw new InvalidOperationException("Cannot add to IndirectionBlock without capacity");

            m_IndirectionTable[m_Index + BlockHeaderSize + Count++] = v;
        }

        public void Remove(int v)
        {
            int i;
            for(i = 0; i < Count; i++)
            {
                if (this[i] == v)
                    break;
            }

            if (i >= Count)
                throw new InvalidOperationException("Cannot find item within the IndirectionBlock");

            for(; i < Count - 1 ; ++i)
            {
                this[i] = this[i + 1];
            }

            Count--;
        }

        public void Clear()
        {
            Count = 0;
        }

        public IndirectionBlock MoveNext()
        {
            if (m_IsNull)
                throw new NullReferenceException();

            IndirectionBlock result;

            result.m_Index = m_Index + BlockSize;
            result.m_IsNull = result.m_Index >= m_IndirectionTable.Count;
            result.m_IndirectionTable = m_IndirectionTable;

            return result;
        }

        public static IndirectionBlock First(List<int> indirectionTable)
        {
            return new IndirectionBlock(indirectionTable, 0);
        }


        public static IndirectionBlock MakeNew(List<int> indirectionTable, int blockSize)
        {
            Assert.AreEqual(ToNearestBlockSize(blockSize), blockSize);

            while (indirectionTable.Capacity < indirectionTable.Count + blockSize)
                indirectionTable.Capacity *= 2;

            IndirectionBlock result;

            result.m_Index = indirectionTable.Count;
            result.m_IndirectionTable = indirectionTable;
            result.m_IsNull = true;

            for (int i = 0; i < blockSize; i++)
                indirectionTable.Add(-1);

            result.Count = 0;
            result.Capacity = blockSize - BlockHeaderSize;

            return result;
        }

        public static int NullBlockID
        {
            get { return -1; }
        }

        public static int BlockSizeFromCapacity(int capacity)
        {
            return ToNearestBlockSize(capacity + BlockHeaderSize);
        }

        private static int ToNearestBlockSize(int x)
        {
            if (x < MinimumBlockSize)
                x = MinimumBlockSize;

            --x;

            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;

            return x + 1;
        }

        public bool IsNull
        {
            get { return m_IsNull; }
        }

        public bool IsAvailable
        {
            get { return Count == 0; }
        }

        public int Unused
        {
            get { return Capacity - Count; }
        }
    }
}

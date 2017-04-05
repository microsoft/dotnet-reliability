using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace stress.tests
{
    public static class JaggedArray
    {
        private const int EDGEARR_MAXSIZE = 1024;
        private const int EDGEARR_MINSIZE = 64;

        private const int INNERARR_MAXSIZE = 128;
        private const int INNERARR_MINSIZE = 64;

        private static Random s_rand = new Random();

        private static object[] s_roots = new object[128];

        static JaggedArray()
        {
            for(int i = 0; i < s_roots.Length; i++)
            {
                s_roots[i] = NextInnerArray();
            }
        }
        
        [Fact]
        public static void ReplaceEdge()
        {
            //pick a random rooted jagged array
            var inner = (object[])s_roots[s_rand.Next(s_roots.Length)];

            //pick a random edge
            var edgeIx = s_rand.Next(inner.Length);

            var edge = (ChecksumArray)inner[edgeIx];

            //replace it and validate it's checksum
            inner[edgeIx] = NextEdgeArray();

            edge.AssertChecksum();
        }

        [Fact]
        public static void ReplaceInner()
        {
            //pick a random rooted jagged array
            var innerIx = s_rand.Next(s_roots.Length);

            var inner = (object[])s_roots[innerIx];

            //replace and validate all the edge array's checksums
            s_roots[innerIx] = NextInnerArray();

            foreach (ChecksumArray edge in inner)
            {
                edge.AssertChecksum();
            }
        }

        private static object[] NextInnerArray()
        {
            int size = s_rand.Next(INNERARR_MINSIZE, INNERARR_MAXSIZE + 1);

            object[] arr = new object[size];

            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = NextEdgeArray();
            }

            return arr;
        }

        private static ChecksumArray NextEdgeArray()
        {
            int size = s_rand.Next(EDGEARR_MINSIZE, EDGEARR_MAXSIZE + 1);

            return new ChecksumArray(s_rand, size);
        }


        private class ChecksumArray
        {
            private int _checksum;
            private int[] _arr;

            public ChecksumArray(Random rand, int size)
            {
                _arr = new int[size];

                for(int i = 0; i < _arr.Length; i++)
                {
                    _arr[i] = rand.Next(int.MinValue, int.MaxValue);

                    _checksum ^= _arr[i];
                }
            }

            public int AssertChecksum()
            {
                int chk = 0;

                for (int i = 0; i < _arr.Length; i++)
                {
                    chk ^= _arr[i];
                }

                Assert.Equal(chk, _checksum);

                return _checksum;
            }

            public int Checksum { get { return _checksum; } }
        }
    }

}

using System;
using System.Collections.Generic;
using System.Text;

namespace AoC2020
{
    public static class ArrayExtensions
    {
        public static bool ValidIndex(this Array @this, params int[] indices)
        {
            if (@this.Rank != indices.Length)
                return false;

            for (int i = 0; i < @this.Rank; i++)
                if (!(0 <= indices[i] && indices[i] < @this.GetLength(i)))
                    return false;

            return true;
        }

        public static bool ValidIndex(this Array @this, (int, int) indices)
        => @this.ValidIndex(indices.Item1, indices.Item2);
    }
}

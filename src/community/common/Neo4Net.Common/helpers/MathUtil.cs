using System;
using System.Diagnostics;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace Neo4Net.Helpers
{
   /// @deprecated This class will be removed from public API in 4.0.
   [Obsolete("This class will be removed from public API in 4.0.")]
   public class MathUtil
   {
      private const long NON_DOUBLE_LONG = unchecked((long)0xFFE0_0000_0000_0000L); // doubles are exact integers up to 53 bits

      private MathUtil()
      {
         throw new AssertionError();
      }

      /// <summary>
      /// Calculates the portion of the first value to all values passed </summary>
      /// <param name="n"> The values in the set </param>
      /// <returns> the ratio of n[0] to the sum all n, 0 if result is <seealso cref="Double.NaN"/> </returns>
      public static double Portion(params double[] n)
      {
         Debug.Assert(n.Length > 0);

         double first = n[0];
         if (NumbersEqual(first, 0))
         {
            return 0d;
         }
         double total = java.util.n.Sum();
         return first / total;
      }

      public static bool NumbersEqual(double fpn, long @in)
      {
         if (@in < 0)
         {
            if (fpn < 0.0)
            {
               if ((NON_DOUBLE_LONG & @in) == NON_DOUBLE_LONG) // the high order bits are only sign bits
               { // no loss of precision if converting the long to a double, so it's safe to compare as double
                  return fpn == @in;
               }
               else if (fpn < long.MinValue)
               { // the double is too big to fit in a long, they cannot be equal
                  return false;
               }
               else if ((fpn == Math.Floor(fpn)) && !double.IsInfinity(fpn)) // no decimals
               { // safe to compare as long
                  return @in == (long)fpn;
               }
            }
         }
         else
         {
            if (!(fpn < 0.0))
            {
               if ((NON_DOUBLE_LONG & @in) == 0) // the high order bits are only sign bits
               { // no loss of precision if converting the long to a double, so it's safe to compare as double
                  return fpn == @in;
               }
               else if (fpn > long.MaxValue)
               { // the double is too big to fit in a long, they cannot be equal
                  return false;
               }
               else if ((fpn == Math.Floor(fpn)) && !double.IsInfinity(fpn)) // no decimals
               { // safe to compare as long
                  return @in == (long)fpn;
               }
            }
         }
         return false;
      }

      // Tested by PropertyValueComparisonTest
      public static int CompareDoubleAgainstLong(double lhs, long rhs)
      {
         if ((NON_DOUBLE_LONG & rhs) != NON_DOUBLE_LONG)
         {
            if (double.IsNaN(lhs))
            {
               return +1;
            }
            if (double.IsInfinity(lhs))
            {
               return lhs < 0 ? -1 : +1;
            }
            return decimal.ValueOf(lhs).CompareTo(decimal.ValueOf(rhs));
         }
         return lhs.CompareTo(rhs);
      }

      // Tested by PropertyValueComparisonTest
      public static int CompareLongAgainstDouble(long lhs, double rhs)
      {
         return -CompareDoubleAgainstLong(rhs, lhs);
      }

      /// <summary>
      /// Return an integer one less than the given integer, or throw <seealso cref="ArithmeticException"/> if the given integer is
      /// zero.
      /// </summary>
      /// <param name="value"> integer to decrement </param>
      /// <returns> the provided integer minus one </returns>
      /// <exception cref="ArithmeticException"> if the resulting integer would be less than zero </exception>
      public static int DecrementExactNotPastZero(int value)
      {
         if (value == 0)
         {
            throw new ArithmeticException("integer underflow past zero");
         }
         return value - 1;
      }
   }
}
//----------------------------------------------------------------------------------------
//	Copyright © 2007 - 2019 Tangible Software Solutions, Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class includes methods to convert Java rectangular arrays (jagged arrays
//	with inner arrays of the same length).
//----------------------------------------------------------------------------------------
internal static class RectangularArrays
{
   public static long[][] RectangularLongArray(int size1, int size2)
   {
      long[][] newArray = new long[size1][];
      for (int array1 = 0; array1 < size1; array1++)
      {
         newArray[array1] = new long[size2];
      }

      return newArray;
   }

   public static string[][] RectangularStringArray(int size1, int size2)
   {
      string[][] newArray = new string[size1][];
      for (int array1 = 0; array1 < size1; array1++)
      {
         newArray[array1] = new string[size2];
      }

      return newArray;
   }

   public static object[][] RectangularObjectArray(int size1, int size2)
   {
      object[][] newArray = new object[size1][];
      for (int array1 = 0; array1 < size1; array1++)
      {
         newArray[array1] = new object[size2];
      }

      return newArray;
   }

   public static System.Nullable<T>[][] RectangularSystemNullableArray<T>(int size1, int size2)
   {
      System.Nullable<T>[][] newArray = new System.Nullable<T>[size1][];
      for (int array1 = 0; array1 < size1; array1++)
      {
         newArray[array1] = new System.Nullable<T>[size2];
      }

      return newArray;
   }

   public static int[][] RectangularIntArray(int size1, int size2)
   {
      int[][] newArray = new int[size1][];
      for (int array1 = 0; array1 < size1; array1++)
      {
         newArray[array1] = new int[size2];
      }

      return newArray;
   }
}
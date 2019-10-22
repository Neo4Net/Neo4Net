﻿//---------------------------------------------------------------------------------------------------------
//	Copyright © 2007 - 2019 Tangible Software Solutions, Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class is used to replace calls to the static java.lang.Math.random method.
//---------------------------------------------------------------------------------------------------------

namespace Neo4Net.Utils
{
   public static class GlobalRandom
   {
      private static System.Random randomInstance = null;

      public static double NextDouble
      {
         get
         {
            if (randomInstance == null)
               randomInstance = new System.Random();

            return randomInstance.NextDouble();
         }
      }
   }
}
using System;
using System.Collections.Generic;

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

namespace Neo4Net.GraphDb.Spatial
{
   /// <summary>
   /// A coordinate is used to describe a position in space.
   /// <para>
   /// A coordinate is described by at least two numbers and must adhere to the following ordering
   /// <ul>
   /// <li>x, y, z ordering in a cartesian reference system</li>
   /// <li>east, north, altitude in a projected coordinate reference system</li>
   /// <li>longitude, latitude, altitude in a geographic reference system</li>
   /// </ul>
   /// </para>
   /// <para>
   /// Additional numbers are allowed and the meaning of these additional numbers depends on the coordinate reference
   /// system
   /// (see $<seealso cref="ICRS"/>)
   /// </para>
   /// </summary>
   public sealed class Coordinate
   {
      private readonly double[] _coordinate;

      public Coordinate(params double[] coordinate)
      {
         if (coordinate.Length < 2)
         {
            throw new System.ArgumentException("A coordinate must have at least two elements");
         }
         _coordinate = coordinate;
      }

      /// <summary>
      /// Returns the current coordinate.
      /// </summary>
      /// <returns> A list of numbers describing the coordinate. </returns>
      public IList<double> GetCoordinate()
      {
         return stream(_coordinate).boxed().collect(Collectors.toList());
      }

      public override bool Equals(object o)
      {
         if (this == o)
         {
            return true;
         }
         if (o == null || this.GetType() != o.GetType())
         {
            return false;
         }

         Coordinate that = (Coordinate)o;

         return Array.Equals(_coordinate, that._coordinate);
      }

      public override int GetHashCode()
      {
         return Array.GetHashCode(_coordinate);
      }
   }
}
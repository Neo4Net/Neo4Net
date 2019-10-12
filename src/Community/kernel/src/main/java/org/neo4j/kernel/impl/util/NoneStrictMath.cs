using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.impl.util
{

	/// <summary>
	/// @author Anton Persson
	/// </summary>
	public class NoneStrictMath
	{
		 // NOTE: This cannot be final since it's used to change the tolerance in the graph algorithms module
		 public static double Epsilon = 1.0E-8;

		 private NoneStrictMath()
		 {
		 }

		 /// <summary>
		 /// Compares two numbers given some amount of allowed error.
		 /// </summary>
		 public static int Compare( double x, double y, double eps )
		 {
			  return Equals( x, y, eps ) ? 0 : x < y ? -1 : 1;
		 }

		 /// <summary>
		 /// Compares two numbers given some amount of allowed error.
		 /// Error given by <seealso cref="NoneStrictMath.EPSILON"/>
		 /// </summary>
		 public static int Compare( double x, double y )
		 {
			  return Compare( x, y, Epsilon );
		 }

		 /// <summary>
		 /// Returns true if both arguments are equal or within the range of allowed error (inclusive)
		 /// </summary>
		 public static bool Equals( double x, double y, double eps )
		 {
			  return Math.Abs( x - y ) <= eps;
		 }

		 /// <summary>
		 /// Returns true if both arguments are equal or within the range of allowed error (inclusive)
		 /// Error given by <seealso cref="NoneStrictMath.EPSILON"/>
		 /// </summary>
		 public static bool Equals( double x, double y )
		 {
			  return Equals( x, y, Epsilon );
		 }

		 public class CommonToleranceComparator : IComparer<double>
		 {
			  internal readonly double Epsilon;

			  public CommonToleranceComparator( double epsilon )
			  {
					this.Epsilon = epsilon;
			  }

			  public override int Compare( double? x, double? y )
			  {
					return NoneStrictMath.Compare( x.Value, y.Value, Epsilon );
			  }
		 }
	}

}
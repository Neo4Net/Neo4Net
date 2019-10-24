using System;

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
namespace Neo4Net.Values.Storable
{

	using Geometry = Neo4Net.GraphDb.Spatial.Geometry;

	/// <summary>
	/// Single instance of one of the storable primitives.
	/// </summary>
	public abstract class ScalarValue : Value
	{
		 public override sealed bool Equals( sbyte[] x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( short[] x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( int[] x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( long[] x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( float[] x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( double[] x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( bool[] x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( char[] x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( string[] x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( Geometry[] x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( ZonedDateTime[] x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( LocalDate[] x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( DurationValue[] x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( DateTime[] x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( LocalTime[] x )
		 {
			  return false;
		 }

		 public override sealed bool Equals( OffsetTime[] x )
		 {
			  return false;
		 }
	}

}
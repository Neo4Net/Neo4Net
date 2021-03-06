﻿using System;

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
namespace Org.Neo4j.Values.Storable
{

	using Geometry = Org.Neo4j.Graphdb.spatial.Geometry;

	public abstract class NumberArray : ArrayValue
	{
		 internal abstract int CompareTo( IntegralArray other );

		 internal abstract int CompareTo( FloatingPointArray other );

		 internal override int UnsafeCompareTo( Value otherValue )
		 {
			  if ( otherValue is IntegralArray )
			  {
					return CompareTo( ( IntegralArray ) otherValue );
			  }
			  else if ( otherValue is FloatingPointArray )
			  {
					return CompareTo( ( FloatingPointArray ) otherValue );
			  }
			  else
			  {
					throw new System.ArgumentException( "Cannot compare different values" );
			  }
		 }

		 public sealed override bool Equals( bool[] x )
		 {
			  return false;
		 }

		 public sealed override bool Equals( char[] x )
		 {
			  return false;
		 }

		 public sealed override bool Equals( string[] x )
		 {
			  return false;
		 }

		 public bool Equals( Geometry[] x )
		 {
			  return false;
		 }

		 public bool Equals( ZonedDateTime[] x )
		 {
			  return false;
		 }

		 public bool Equals( LocalDate[] x )
		 {
			  return false;
		 }

		 public bool Equals( DurationValue[] x )
		 {
			  return false;
		 }

		 public bool Equals( DateTime[] x )
		 {
			  return false;
		 }

		 public bool Equals( LocalTime[] x )
		 {
			  return false;
		 }

		 public bool Equals( OffsetTime[] x )
		 {
			  return false;
		 }

		 public override ValueGroup ValueGroup()
		 {
			  return ValueGroup.NumberArray;
		 }
	}

}
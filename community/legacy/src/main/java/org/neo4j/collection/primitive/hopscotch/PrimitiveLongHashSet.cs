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
namespace Org.Neo4j.Collection.primitive.hopscotch
{

	using Org.Neo4j.Collection.primitive;
	using Monitor = Org.Neo4j.Collection.primitive.hopscotch.HopScotchHashingAlgorithm.Monitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.primitive.hopscotch.HopScotchHashingAlgorithm.DEFAULT_HASHING;

	public class PrimitiveLongHashSet : AbstractLongHopScotchCollection<object>, PrimitiveLongSet
	{
		 private readonly object _valueMarker;
		 private readonly Monitor _monitor;

		 public PrimitiveLongHashSet( Table<object> table, object valueMarker, Monitor monitor ) : base( table )
		 {
			  this._valueMarker = valueMarker;
			  this._monitor = monitor;
		 }

		 public override bool Add( long value )
		 {
			  return HopScotchHashingAlgorithm.Put( Table, _monitor, DEFAULT_HASHING, value, _valueMarker, this ) == null;
		 }

		 public override bool AddAll( PrimitiveLongIterator values )
		 {
			  bool changed = false;
			  while ( values.HasNext() )
			  {
					changed |= HopScotchHashingAlgorithm.Put( Table, _monitor, DEFAULT_HASHING, values.Next(), _valueMarker, this ) == null;
			  }
			  return changed;
		 }

		 public override bool Contains( long value )
		 {
			  return HopScotchHashingAlgorithm.Get( Table, _monitor, DEFAULT_HASHING, value ) == _valueMarker;
		 }

		 /// <summary>
		 /// Prefer using <seealso cref="contains(long)"/> - this method is identical and required by the <seealso cref="IntPredicate"/> interface
		 /// </summary>
		 /// <param name="value"> the input argument </param>
		 /// <returns> true if the input argument matches the predicate, otherwise false </returns>
		 public override bool Test( long value )
		 {
			  return HopScotchHashingAlgorithm.Get( Table, _monitor, DEFAULT_HASHING, value ) == _valueMarker;
		 }

		 public override bool Remove( long value )
		 {
			  return HopScotchHashingAlgorithm.Remove( Table, _monitor, DEFAULT_HASHING, value ) == _valueMarker;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("EqualsWhichDoesntCheckParameterClass") @Override public boolean equals(Object other)
		 public override bool Equals( object other )
		 {
			  if ( TypeAndSizeEqual( other ) )
			  {
					PrimitiveLongHashSet that = ( PrimitiveLongHashSet ) other;
					LongKeyEquality equality = new LongKeyEquality( that );
					VisitKeys( equality );
					return equality.Equal;
			  }
			  return false;
		 }

		 private class LongKeyEquality : PrimitiveLongVisitor<Exception>
		 {
			  internal PrimitiveLongHashSet Other;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool EqualConflict = true;

			  internal LongKeyEquality( PrimitiveLongHashSet that )
			  {
					this.Other = that;
			  }

			  public override bool Visited( long value )
			  {
					EqualConflict = Other.contains( value );
					return !EqualConflict;
			  }

			  public virtual bool Equal
			  {
				  get
				  {
						return EqualConflict;
				  }
			  }
		 }

		 public override int GetHashCode()
		 {
			  HashCodeComputer hash = new HashCodeComputer();
			  VisitKeys( hash );
			  return hash.GetHashCode();
		 }

		 private class HashCodeComputer : PrimitiveLongVisitor<Exception>
		 {
			  internal int Hash = 1337;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visited(long value) throws RuntimeException
			  public override bool Visited( long value )
			  {
					Hash += DEFAULT_HASHING.hashSingleValueToInt( value );
					return false;
			  }

			  public override int GetHashCode()
			  {
					return Hash;
			  }

			  public override bool Equals( object o )
			  {
					if ( this == o )
					{
						 return true;
					}
					if ( o == null || this.GetType() != o.GetType() )
					{
						 return false;
					}
					HashCodeComputer that = ( HashCodeComputer ) o;
					return Hash == that.Hash;
			  }
		 }
	}

}
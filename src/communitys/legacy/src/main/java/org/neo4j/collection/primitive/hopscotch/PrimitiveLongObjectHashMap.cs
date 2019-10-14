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
namespace Neo4Net.Collections.primitive.hopscotch
{

	using Neo4Net.Collections.primitive;
	using Neo4Net.Collections.primitive;
	using Monitor = Neo4Net.Collections.primitive.hopscotch.HopScotchHashingAlgorithm.Monitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.primitive.hopscotch.HopScotchHashingAlgorithm.DEFAULT_HASHING;

	public class PrimitiveLongObjectHashMap<VALUE> : AbstractLongHopScotchCollection<VALUE>, PrimitiveLongObjectMap<VALUE>
	{
		 private readonly Monitor _monitor;

		 public PrimitiveLongObjectHashMap( Table<VALUE> table, Monitor monitor ) : base( table )
		 {
			  this._monitor = monitor;
		 }

		 public override VALUE Put( long key, VALUE value )
		 {
			  return HopScotchHashingAlgorithm.Put( Table, _monitor, DEFAULT_HASHING, key, value, this );
		 }

		 public override bool ContainsKey( long key )
		 {
			  return HopScotchHashingAlgorithm.Get( Table, _monitor, DEFAULT_HASHING, key ) != default( VALUE );
		 }

		 public override VALUE Get( long key )
		 {
			  return HopScotchHashingAlgorithm.Get( Table, _monitor, DEFAULT_HASHING, key );
		 }

		 public override VALUE Remove( long key )
		 {
			  return HopScotchHashingAlgorithm.Remove( Table, _monitor, DEFAULT_HASHING, key );
		 }

		 public override int Size()
		 {
			  return Table.size();
		 }

		 public override string ToString()
		 {
			  return Table.ToString();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void visitEntries(org.neo4j.collection.primitive.PrimitiveLongObjectVisitor<VALUE, E> visitor) throws E
		 public override void VisitEntries<E>( PrimitiveLongObjectVisitor<VALUE, E> visitor ) where E : Exception
		 {
			  long nullKey = Table.nullKey();
			  int capacity = Table.capacity();
			  for ( int i = 0; i < capacity; i++ )
			  {
					long key = Table.key( i );
					if ( key != nullKey && visitor.Visited( key, Table.value( i ) ) )
					{
						 return;
					}
			  }
		 }

		 public override IEnumerable<VALUE> Values()
		 {
			  return new ValueIterable( this, this );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("EqualsWhichDoesntCheckParameterClass") @Override public boolean equals(Object other)
		 public override bool Equals( object other )
		 {
			  if ( TypeAndSizeEqual( other ) )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: PrimitiveLongObjectHashMap<?> that = (PrimitiveLongObjectHashMap<?>) other;
					PrimitiveLongObjectHashMap<object> that = ( PrimitiveLongObjectHashMap<object> ) other;
					LongObjEquality<VALUE> equality = new LongObjEquality<VALUE>( that );
					VisitEntries( equality );
					return equality.Equal;
			  }
			  return false;
		 }

		 private class LongObjEquality<T> : PrimitiveLongObjectVisitor<T, Exception>
		 {
			  internal PrimitiveLongObjectHashMap Other;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool EqualConflict = true;

			  internal LongObjEquality( PrimitiveLongObjectHashMap that )
			  {
					this.Other = that;
			  }

			  public override bool Visited( long key, T value )
			  {
					object otherValue = Other.get( key );
					EqualConflict = otherValue == value || ( otherValue != null && otherValue.Equals( value ) );
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
			  HashCodeComputer<VALUE> hash = new HashCodeComputer<VALUE>();
			  VisitEntries( hash );
			  return hash.GetHashCode();
		 }

		 private class HashCodeComputer<T> : PrimitiveLongObjectVisitor<T, Exception>
		 {
			  internal int Hash = 1337;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visited(long key, T value) throws RuntimeException
			  public override bool Visited( long key, T value )
			  {
					Hash += DEFAULT_HASHING.hashSingleValueToInt( key + value.GetHashCode() );
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
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: HashCodeComputer<?> that = (HashCodeComputer<?>) o;
					HashCodeComputer<object> that = ( HashCodeComputer<object> ) o;
					return Hash == that.Hash;
			  }
		 }

		 private class ValueIterable : IEnumerable<VALUE>
		 {
			 private readonly PrimitiveLongObjectHashMap<VALUE> _outerInstance;

			  internal readonly PrimitiveLongObjectHashMap<VALUE> Map;

			  internal ValueIterable( PrimitiveLongObjectHashMap<VALUE> outerInstance, PrimitiveLongObjectHashMap<VALUE> map )
			  {
				  this._outerInstance = outerInstance;
					this.Map = map;
			  }

			  public override IEnumerator<VALUE> Iterator()
			  {
					return new IteratorAnonymousInnerClass( this );
			  }

			  private class IteratorAnonymousInnerClass : IEnumerator<VALUE>
			  {
				  private readonly ValueIterable _outerInstance;

				  public IteratorAnonymousInnerClass( ValueIterable outerInstance )
				  {
					  this.outerInstance = outerInstance;
					  iterator = outerInstance.Map.GetEnumerator();
				  }

				  private readonly PrimitiveLongIterator iterator;

				  public bool hasNext()
				  {
						return iterator.hasNext();
				  }

				  public VALUE next()
				  {
						return _outerInstance.map.get( iterator.next() );
				  }
			  }
		 }
	}

}
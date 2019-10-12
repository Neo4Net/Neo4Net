using System;

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
namespace Neo4Net.Collection.primitive.hopscotch
{
	using Neo4Net.Collection.primitive;
	using Monitor = Neo4Net.Collection.primitive.hopscotch.HopScotchHashingAlgorithm.Monitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.primitive.hopscotch.HopScotchHashingAlgorithm.DEFAULT_HASHING;

	public class PrimitiveIntLongHashMap : AbstractIntHopScotchCollection<long[]>, PrimitiveIntLongMap
	{
		 private readonly long[] _transport = new long[1];
		 private readonly Monitor _monitor;

		 public PrimitiveIntLongHashMap( Table<long[]> table, Monitor monitor ) : base( table )
		 {
			  this._monitor = monitor;
		 }

		 public override long Put( int key, long value )
		 {
			  return Unpack( HopScotchHashingAlgorithm.Put( Table, _monitor, DEFAULT_HASHING, key, Pack( value ), this ) );
		 }

		 public override bool ContainsKey( int key )
		 {
			  return HopScotchHashingAlgorithm.Get( Table, _monitor, DEFAULT_HASHING, key ) != null;
		 }

		 public override long Get( int key )
		 {
			  return Unpack( HopScotchHashingAlgorithm.Get( Table, _monitor, DEFAULT_HASHING, key ) );
		 }

		 public override long Remove( int key )
		 {
			  return Unpack( HopScotchHashingAlgorithm.Remove( Table, _monitor, DEFAULT_HASHING, key ) );
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
//ORIGINAL LINE: public <E extends Exception> void visitEntries(org.neo4j.collection.primitive.PrimitiveIntLongVisitor<E> visitor) throws E
		 public override void VisitEntries<E>( PrimitiveIntLongVisitor<E> visitor ) where E : Exception
		 {
			  long nullKey = Table.nullKey();
			  int capacity = Table.capacity();
			  for ( int i = 0; i < capacity; i++ )
			  {
					int key = ( int ) Table.key( i );
					if ( key != nullKey )
					{
						 long[] value = Table.value( i );
						 if ( value != null && visitor.Visited( key, value[0] ) )
						 {
							  return;
						 }
					}
			  }
		 }

		 private long[] Pack( long value )
		 {
			  _transport[0] = value;
			  return _transport;
		 }

		 private long Unpack( long[] result )
		 {
			  return result != null ? result[0] : IntKeyLongValueTable.NULL;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("EqualsWhichDoesntCheckParameterClass") @Override public boolean equals(Object other)
		 public override bool Equals( object other )
		 {
			  if ( TypeAndSizeEqual( other ) )
			  {
					PrimitiveIntLongHashMap that = ( PrimitiveIntLongHashMap ) other;
					IntLongEquality equality = new IntLongEquality( that );
					VisitEntries( equality );
					return equality.Equal;
			  }
			  return false;
		 }

		 private class IntLongEquality : PrimitiveIntLongVisitor<Exception>
		 {
			  internal PrimitiveIntLongHashMap Other;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool EqualConflict = true;

			  internal IntLongEquality( PrimitiveIntLongHashMap that )
			  {
					this.Other = that;
			  }

			  public override bool Visited( int key, long value )
			  {
					EqualConflict = Other.get( key ) == value;
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
			  VisitEntries( hash );
			  return hash.GetHashCode();
		 }

		 private class HashCodeComputer : PrimitiveIntLongVisitor<Exception>
		 {
			  internal int Hash = 1337;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visited(int key, long value) throws RuntimeException
			  public override bool Visited( int key, long value )
			  {
					Hash += DEFAULT_HASHING.hashSingleValueToInt( key + DEFAULT_HASHING.hashSingleValueToInt( value ) );
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
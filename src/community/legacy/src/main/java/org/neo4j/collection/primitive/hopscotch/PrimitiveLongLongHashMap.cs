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
namespace Neo4Net.Collections.primitive.hopscotch
{
	using Neo4Net.Collections.primitive;
	using Monitor = Neo4Net.Collections.primitive.hopscotch.HopScotchHashingAlgorithm.Monitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.primitive.hopscotch.HopScotchHashingAlgorithm.DEFAULT_HASHING;

	public class PrimitiveLongLongHashMap : AbstractLongHopScotchCollection<long[]>, PrimitiveLongLongMap
	{
		 private readonly long[] _transport = new long[1];
		 private readonly Monitor _monitor;

		 public PrimitiveLongLongHashMap( Table<long[]> table, Monitor monitor ) : base( table )
		 {
			  this._monitor = monitor;
		 }

		 public override long Put( long key, long value )
		 {
			  return Unpack( HopScotchHashingAlgorithm.Put( Table, _monitor, DEFAULT_HASHING, key, Pack( value ), this ) );
		 }

		 public override bool ContainsKey( long key )
		 {
			  return HopScotchHashingAlgorithm.Get( Table, _monitor, DEFAULT_HASHING, key ) != null;
		 }

		 public override long Get( long key )
		 {
			  return Unpack( HopScotchHashingAlgorithm.Get( Table, _monitor, DEFAULT_HASHING, key ) );
		 }

		 public override long Remove( long key )
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

		 private long[] Pack( long value )
		 {
			  _transport[0] = value;
			  return _transport;
		 }

		 private long Unpack( long[] result )
		 {
			  return result != null ? result[0] : LongKeyIntValueTable.NULL;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void visitEntries(org.neo4j.collection.primitive.PrimitiveLongLongVisitor<E> visitor) throws E
		 public override void VisitEntries<E>( PrimitiveLongLongVisitor<E> visitor ) where E : Exception
		 {
			  long nullKey = Table.nullKey();
			  int capacity = Table.capacity();
			  for ( int i = 0; i < capacity; i++ )
			  {
					long key = Table.key( i );
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

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("EqualsWhichDoesntCheckParameterClass") @Override public boolean equals(Object other)
		 public override bool Equals( object other )
		 {
			  if ( TypeAndSizeEqual( other ) )
			  {
					PrimitiveLongLongHashMap that = ( PrimitiveLongLongHashMap ) other;
					LongLongEquality equality = new LongLongEquality( that );
					VisitEntries( equality );
					return equality.Equal;
			  }
			  return false;
		 }

		 private class LongLongEquality : PrimitiveLongLongVisitor<Exception>
		 {
			  internal PrimitiveLongLongHashMap Other;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool EqualConflict = true;

			  internal LongLongEquality( PrimitiveLongLongHashMap that )
			  {
					this.Other = that;
			  }

			  public override bool Visited( long key, long value )
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

		 private class HashCodeComputer : PrimitiveLongLongVisitor<Exception>
		 {
			  internal int Hash = 1337;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visited(long key, long value) throws RuntimeException
			  public override bool Visited( long key, long value )
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
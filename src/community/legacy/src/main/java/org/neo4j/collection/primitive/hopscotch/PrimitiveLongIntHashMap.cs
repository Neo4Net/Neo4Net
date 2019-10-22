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
//	import static org.Neo4Net.collection.primitive.hopscotch.HopScotchHashingAlgorithm.DEFAULT_HASHING;

	public class PrimitiveLongIntHashMap : AbstractLongHopScotchCollection<int[]>, PrimitiveLongIntMap
	{
		 private readonly int[] _transport = new int[1];
		 private readonly Monitor _monitor;

		 public PrimitiveLongIntHashMap( Table<int[]> table, Monitor monitor ) : base( table )
		 {
			  this._monitor = monitor;
		 }

		 public override int Put( long key, int value )
		 {
			  return Unpack( HopScotchHashingAlgorithm.Put( Table, _monitor, DEFAULT_HASHING, key, Pack( value ), this ) );
		 }

		 public override bool ContainsKey( long key )
		 {
			  return HopScotchHashingAlgorithm.Get( Table, _monitor, DEFAULT_HASHING, key ) != null;
		 }

		 public override int Get( long key )
		 {
			  return Unpack( HopScotchHashingAlgorithm.Get( Table, _monitor, DEFAULT_HASHING, key ) );
		 }

		 public override int Remove( long key )
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

		 private int[] Pack( int value )
		 {
			  _transport[0] = value;
			  return _transport;
		 }

		 private int Unpack( int[] result )
		 {
			  return result != null ? result[0] : LongKeyIntValueTable.NULL;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void visitEntries(org.Neo4Net.collection.primitive.PrimitiveLongIntVisitor<E> visitor) throws E
		 public override void VisitEntries<E>( PrimitiveLongIntVisitor<E> visitor ) where E : Exception
		 {
			  long nullKey = Table.nullKey();
			  int capacity = Table.capacity();
			  for ( int i = 0; i < capacity; i++ )
			  {
					int[] value = Table.value( i );
					if ( value != null )
					{
						 long key = Table.key( i );
						 if ( key != nullKey && visitor.Visited( key, value[0] ) )
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
					PrimitiveLongIntHashMap that = ( PrimitiveLongIntHashMap ) other;
					LongIntEquality equality = new LongIntEquality( that );
					VisitEntries( equality );
					return equality.Equal;
			  }
			  return false;
		 }

		 private class LongIntEquality : PrimitiveLongIntVisitor<Exception>
		 {
			  internal PrimitiveLongIntHashMap Other;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool EqualConflict = true;

			  internal LongIntEquality( PrimitiveLongIntHashMap that )
			  {
					this.Other = that;
			  }

			  public override bool Visited( long key, int value )
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

		 private class HashCodeComputer : PrimitiveLongIntVisitor<Exception>
		 {
			  internal int Hash = 1337;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visited(long key, int value) throws RuntimeException
			  public override bool Visited( long key, int value )
			  {
					Hash += DEFAULT_HASHING.HashSingleValueToInt( key + DEFAULT_HASHING.HashSingleValueToInt( value ) );
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
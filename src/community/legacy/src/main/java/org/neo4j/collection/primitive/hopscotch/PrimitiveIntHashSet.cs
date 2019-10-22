using System;
using System.Text;

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

	public class PrimitiveIntHashSet : AbstractIntHopScotchCollection<object>, PrimitiveIntSet
	{
		 private readonly object _valueMarker;
		 private readonly Monitor _monitor;

		 public PrimitiveIntHashSet( Table<object> table, object valueMarker, Monitor monitor ) : base( table )
		 {
			  this._valueMarker = valueMarker;
			  this._monitor = monitor;
		 }

		 public override bool Add( int value )
		 {
			  return HopScotchHashingAlgorithm.Put( Table, _monitor, DEFAULT_HASHING, value, _valueMarker, this ) == null;
		 }

		 public override bool AddAll( PrimitiveIntIterator values )
		 {
			  bool changed = false;
			  while ( values.HasNext() )
			  {
					changed |= HopScotchHashingAlgorithm.Put( Table, _monitor, DEFAULT_HASHING, values.Next(), _valueMarker, this ) == null;
			  }
			  return changed;
		 }

		 public override bool Contains( int value )
		 {
			  return HopScotchHashingAlgorithm.Get( Table, _monitor, DEFAULT_HASHING, value ) == _valueMarker;
		 }

		 /// <summary>
		 /// Prefer using <seealso cref="contains(int)"/> - this method is identical and required by the <seealso cref="IntPredicate"/> interface
		 /// </summary>
		 /// <param name="value"> the input argument </param>
		 /// <returns> true if the input argument matches the predicate, otherwise false </returns>
		 public override bool Test( int value )
		 {
			  return HopScotchHashingAlgorithm.Get( Table, _monitor, DEFAULT_HASHING, value ) == _valueMarker;
		 }

		 public override bool Remove( int value )
		 {
			  return HopScotchHashingAlgorithm.Remove( Table, _monitor, DEFAULT_HASHING, value ) == _valueMarker;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("EqualsWhichDoesntCheckParameterClass") @Override public boolean equals(Object other)
		 public override bool Equals( object other )
		 {
			  if ( TypeAndSizeEqual( other ) )
			  {
					PrimitiveIntHashSet that = ( PrimitiveIntHashSet ) other;
					IntKeyEquality equality = new IntKeyEquality( that );
					VisitKeys( equality );
					return equality.Equal;
			  }
			  return false;
		 }

		 private class IntKeyEquality : PrimitiveIntVisitor<Exception>
		 {
			  internal readonly PrimitiveIntHashSet Other;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool EqualConflict = true;

			  internal IntKeyEquality( PrimitiveIntHashSet that )
			  {
					this.Other = that;
			  }

			  public override bool Visited( int value )
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

		 private class HashCodeComputer : PrimitiveIntVisitor<Exception>
		 {
			  internal int Hash = 1337;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visited(int value) throws RuntimeException
			  public override bool Visited( int value )
			  {
					Hash += DEFAULT_HASHING.HashSingleValueToInt( value );
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

		 public override string ToString()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final StringBuilder builder = new StringBuilder("{");
			  StringBuilder builder = new StringBuilder( "{" );
			  VisitKeys( new PrimitiveIntVisitorAnonymousInnerClass( this, builder ) );
			  return builder.Append( "}" ).ToString();
		 }

		 private class PrimitiveIntVisitorAnonymousInnerClass : PrimitiveIntVisitor<Exception>
		 {
			 private readonly PrimitiveIntHashSet _outerInstance;

			 private StringBuilder _builder;

			 public PrimitiveIntVisitorAnonymousInnerClass( PrimitiveIntHashSet outerInstance, StringBuilder builder )
			 {
				 this.outerInstance = outerInstance;
				 this._builder = builder;
			 }

			 private int count;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visited(int value) throws RuntimeException
			 public bool visited( int value )
			 {
				  if ( count++ > 0 )
				  {
						_builder.Append( "," );
				  }
				  _builder.Append( value );
				  return false;
			 }
		 }
	}

}
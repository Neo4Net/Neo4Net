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
namespace Org.Neo4j.Collection.primitive.@base
{

	using Org.Neo4j.Collection.primitive;
	using Org.Neo4j.Collection.primitive;
	using Org.Neo4j.Collection.primitive;
	using Org.Neo4j.Collection.primitive;

	public class Empty
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static final org.neo4j.collection.primitive.PrimitiveLongObjectMap EMPTY_PRIMITIVE_LONG_OBJECT_MAP = new EmptyPrimitiveLongObjectMap<>();
		 public static readonly PrimitiveLongObjectMap EmptyPrimitiveLongObjectMap = new EmptyPrimitiveLongObjectMap<>();
		 public static readonly PrimitiveIntSet EmptyPrimitiveIntSet = new EmptyPrimitiveIntSet();
		 public static readonly PrimitiveLongCollection EmptyPrimitiveLongCollection = new EmptyPrimitiveLongCollection();
		 public static readonly PrimitiveLongSet EmptyPrimitiveLongSet = new EmptyPrimitiveLongSet();

		 private Empty()
		 {
		 }

		 public class EmptyPrimitiveCollection : PrimitiveCollection
		 {
			  public virtual bool Empty
			  {
				  get
				  {
						return true;
				  }
			  }

			  public override void Clear()
			  { // Nothing to clear
			  }

			  public override int Size()
			  {
					return 0;
			  }

			  public override void Close()
			  { // Nothing to close
			  }
		 }

		 public class EmptyPrimitiveLongCollection : EmptyPrimitiveCollection, PrimitiveLongCollection
		 {
			  public override PrimitiveLongIterator Iterator()
			  {
					return PrimitiveLongCollections.emptyIterator();
			  }

			  public override void VisitKeys( PrimitiveLongVisitor visitor )
			  { // No keys to visit
			  }
		 }

		 public class EmptyPrimitiveLongSet : EmptyPrimitiveLongCollection, PrimitiveLongSet
		 {
			  public override bool Add( long value )
			  {
					throw new System.NotSupportedException();
			  }

			  public override bool AddAll( PrimitiveLongIterator values )
			  {
					throw new System.NotSupportedException();
			  }

			  public override bool Contains( long value )
			  {
					return false;
			  }

			  public override bool Test( long value )
			  {
					return false;
			  }

			  public override bool Remove( long value )
			  {
					return false;
			  }
		 }

		 public class EmptyPrimitiveIntCollection : EmptyPrimitiveCollection, PrimitiveIntCollection
		 {
			  public override PrimitiveIntIterator Iterator()
			  {
					return PrimitiveIntCollections.emptyIterator();
			  }

			  public override void VisitKeys( PrimitiveIntVisitor visitor )
			  { // No keys to visit
			  }
		 }

		 public class EmptyPrimitiveIntSet : EmptyPrimitiveIntCollection, PrimitiveIntSet
		 {
			  public override bool Add( int value )
			  {
					throw new System.NotSupportedException();
			  }

			  public override bool AddAll( PrimitiveIntIterator values )
			  {
					throw new System.NotSupportedException();
			  }

			  public override bool Contains( int value )
			  {
					return false;
			  }

			  public override bool Test( int value )
			  {
					return false;
			  }

			  public override bool Remove( int value )
			  {
					return false;
			  }
		 }

		 public class EmptyPrimitiveLongObjectMap<T> : EmptyPrimitiveCollection, PrimitiveLongObjectMap<T>
		 {
			  public override T Put( long key, T t )
			  {
					throw new System.NotSupportedException();
			  }

			  public override bool ContainsKey( long key )
			  {
					return false;
			  }

			  public override T Get( long key )
			  {
					return default( T );
			  }

			  public override T Remove( long key )
			  {
					return default( T );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void visitEntries(org.neo4j.collection.primitive.PrimitiveLongObjectVisitor<T, E> visitor) throws E
			  public override void VisitEntries<E>( PrimitiveLongObjectVisitor<T, E> visitor ) where E : Exception
			  { // No entries to visit
			  }

			  public override IEnumerable<T> Values()
			  {
					return Collections.emptyList();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void visitKeys(org.neo4j.collection.primitive.PrimitiveLongVisitor<E> visitor) throws E
			  public override void VisitKeys<E>( PrimitiveLongVisitor<E> visitor ) where E : Exception
			  { // No keys to visit
			  }

			  public override PrimitiveLongIterator Iterator()
			  {
					return PrimitiveLongCollections.emptyIterator();
			  }
		 }
	}

}
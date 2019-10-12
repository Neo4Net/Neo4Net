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
namespace Neo4Net.Collection.primitive
{

	/// <summary>
	/// List implementation that holds primitive longs in array that grows on demand.
	/// </summary>
	public class PrimitiveLongList : PrimitiveLongCollection
	{
		 private const int DEFAULT_SIZE = 8;

		 private long[] _elements;
		 private int _size;

		 internal PrimitiveLongList() : this(DEFAULT_SIZE)
		 {
		 }

		 internal PrimitiveLongList( int size )
		 {
			  _elements = new long[size];
		 }

		 public virtual void Add( long element )
		 {
			  if ( _elements.Length == _size )
			  {
					EnsureCapacity();
			  }
			  _elements[_size++] = element;
		 }

		 public virtual long Get( int position )
		 {
			  if ( position >= _size )
			  {
					throw new System.IndexOutOfRangeException( "Requested element: " + position + ", list size: " + _size );
			  }
			  return _elements[position];
		 }

		 public virtual bool Empty
		 {
			 get
			 {
				  return _size == 0;
			 }
		 }

		 public override void Clear()
		 {
			  _size = 0;
			  _elements = PrimitiveLongCollections.EmptyLongArray;
		 }

		 public override int Size()
		 {
			  return _size;
		 }

		 public override void Close()
		 {
			  Clear();
		 }

		 public override PrimitiveLongIterator Iterator()
		 {
			  return new PrimitiveLongListIterator( this );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <E extends Exception> void visitKeys(PrimitiveLongVisitor<E> visitor) throws E
		 public override void VisitKeys<E>( PrimitiveLongVisitor<E> visitor ) where E : Exception
		 {
			  throw new System.NotSupportedException();
		 }

		 public virtual long[] ToArray()
		 {
			  return Arrays.copyOf( _elements, _size );
		 }

		 private void EnsureCapacity()
		 {
			  int currentCapacity = _elements.Length;
			  int newCapacity = currentCapacity == 0 ? DEFAULT_SIZE : currentCapacity << 1;
			  if ( newCapacity < 0 )
			  {
					throw new System.InvalidOperationException( "Fail to increase list capacity." );
			  }
			  _elements = Arrays.copyOf( _elements, newCapacity );
		 }

		 private class PrimitiveLongListIterator : PrimitiveLongCollections.PrimitiveLongBaseIterator
		 {
			 private readonly PrimitiveLongList _outerInstance;

			 public PrimitiveLongListIterator( PrimitiveLongList outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal int Cursor;

			  protected internal override bool FetchNext()
			  {
					return Cursor < outerInstance.size && Next( outerInstance.elements[Cursor++] );
			  }
		 }
	}

}
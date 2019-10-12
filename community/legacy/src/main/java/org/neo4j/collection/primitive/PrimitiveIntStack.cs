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
namespace Org.Neo4j.Collection.primitive
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.copyOf;

	/// <summary>
	/// Like a {@code Stack<Integer>} but for primitive ints. Virtually GC free in that it has an {@code int[]}
	/// and merely moves a cursor where to <seealso cref="push(int)"/> and <seealso cref="poll()"/> values to and from.
	/// If many items goes in the stack the {@code int[]} will grow to accomodate all of them, but not shrink again.
	/// </summary>
	public class PrimitiveIntStack : PrimitiveIntCollection
	{
		 private int[] _array;
		 private int _cursor = -1; // where the top most item lives

		 public PrimitiveIntStack() : this(16)
		 {
		 }

		 public PrimitiveIntStack( int initialSize )
		 {
			  this._array = new int[initialSize];
		 }

		 public virtual bool Empty
		 {
			 get
			 {
				  return _cursor == -1;
			 }
		 }

		 public override void Clear()
		 {
			  _cursor = -1;
		 }

		 public override int Size()
		 {
			  return _cursor + 1;
		 }

		 public override void Close()
		 { // Nothing to close
		 }

		 public override PrimitiveIntIterator Iterator()
		 {
			  return new PrimitiveIntIteratorAnonymousInnerClass( this );
		 }

		 private class PrimitiveIntIteratorAnonymousInnerClass : PrimitiveIntIterator
		 {
			 private readonly PrimitiveIntStack _outerInstance;

			 public PrimitiveIntIteratorAnonymousInnerClass( PrimitiveIntStack outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 internal int idx;

			 public bool hasNext()
			 {
				  return idx <= _outerInstance.cursor;
			 }

			 public int next()
			 {
				  if ( !hasNext() )
				  {
						throw new NoSuchElementException();
				  }

				  return _outerInstance.array[idx++];
			 }
		 }

		 public override void VisitKeys( PrimitiveIntVisitor visitor )
		 {
			  throw new System.NotSupportedException( "Please implement" );
		 }

		 public virtual void Push( int value )
		 {
			  EnsureCapacity();
			  _array[++_cursor] = value;
		 }

		 private void EnsureCapacity()
		 {
			  if ( _cursor == _array.Length - 1 )
			  {
					_array = copyOf( _array, _array.Length << 1 );
			  }
		 }

		 /// <returns> the top most item and remove it from stack, or -1 if stack is empty </returns>
		 public virtual int Poll()
		 {
			  return _cursor == -1 ? -1 : _array[_cursor--];
		 }

		 /// <returns> the top most item, or -1 if stack is empty </returns>
		 public virtual int Peek()
		 {
			  return _cursor == -1 ? -1 : _array[_cursor];
		 }
	}

}
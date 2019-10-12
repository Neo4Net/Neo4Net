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
namespace Org.Neo4j.Kernel.Impl.Api
{
	/// <summary>
	/// Serves as a reusable utility for building a chain of <seealso cref="TransactionToApply"/> instances,
	/// where the instances themselves form the linked list. This utility is just for easily being able
	/// to append to the end and then at regular intervals batch through the whole queue.
	/// </summary>
	public class TransactionQueue
	{
		 public delegate void Applier( TransactionToApply first, TransactionToApply last );

		 private readonly int _maxSize;
		 private readonly Applier _applier;
		 private TransactionToApply _first;
		 private TransactionToApply _last;
		 private int _size;

		 public TransactionQueue( int maxSize, Applier applier )
		 {
			  this._maxSize = maxSize;
			  this._applier = applier;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void queue(TransactionToApply transaction) throws Exception
		 public virtual void Queue( TransactionToApply transaction )
		 {
			  if ( Empty )
			  {
					_first = _last = transaction;
			  }
			  else
			  {
					_last.next( transaction );
					_last = transaction;
			  }
			  if ( ++_size == _maxSize )
			  {
					Empty();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void empty() throws Exception
		 public virtual void Empty()
		 {
			  if ( _size > 0 )
			  {
					_applier( _first, _last );
					_first = _last = null;
					_size = 0;
			  }
		 }

		 public virtual bool Empty
		 {
			 get
			 {
				  return _size == 0;
			 }
		 }

		 public virtual TransactionToApply First()
		 {
			  if ( Empty )
			  {
					throw new System.InvalidOperationException( "Nothing in queue" );
			  }
			  return _first;
		 }

		 public virtual TransactionToApply Last()
		 {
			  if ( Empty )
			  {
					throw new System.InvalidOperationException( "Nothing in queue" );
			  }
			  return _last;
		 }
	}

}
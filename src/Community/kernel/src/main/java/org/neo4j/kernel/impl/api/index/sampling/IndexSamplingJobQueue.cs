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
namespace Neo4Net.Kernel.Impl.Api.index.sampling
{

	public class IndexSamplingJobQueue<T>
	{
		 private readonly LinkedList<T> _queue = new LinkedList<T>();
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private final System.Predicate<? super T> enqueueablePredicate;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private readonly System.Predicate<object> _enqueueablePredicate;

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public IndexSamplingJobQueue(System.Predicate<? super T> enqueueablePredicate)
		 public IndexSamplingJobQueue<T1>( System.Predicate<T1> enqueueablePredicate )
		 {
			  this._enqueueablePredicate = enqueueablePredicate;
		 }

		 public virtual void Add( bool force, T item )
		 {
			 lock ( this )
			 {
				  if ( ShouldEnqueue( force, item ) )
				  {
						_queue.AddLast( item );
				  }
			 }
		 }

		 public virtual void AddAll( bool force, IEnumerator<T> items )
		 {
			 lock ( this )
			 {
				  while ( items.MoveNext() )
				  {
						Add( force, items.Current );
				  }
			 }
		 }

		 private bool ShouldEnqueue( bool force, T item )
		 {

			  // Add index if not in queue
			  if ( _queue.Contains( item ) )
			  {
					return false;
			  }

			  // and either adding all
			  if ( force )
			  {
					return true;
			  }

			  // or otherwise only if seen enough updates (as determined by updatePredicate)
			  return _enqueueablePredicate.test( item );
		 }

		 public virtual T Poll()
		 {
			 lock ( this )
			 {
				  return _queue.RemoveFirst();
			 }
		 }

		 public virtual IEnumerable<T> PollAll()
		 {
			 lock ( this )
			 {
				  ICollection<T> items = new List<T>( _queue.Count );
				  while ( true )
				  {
						T item = _queue.RemoveFirst();
						if ( item == default( T ) )
						{
							 return items;
						}
						items.Add( item );
				  }
			 }
		 }
	}

}
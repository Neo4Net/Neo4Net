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
namespace Neo4Net.Helpers.Collections
{

	internal class FilterIterable<T> : IEnumerable<T>
	{
		 private readonly IEnumerable<T> _iterable;

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private final System.Predicate<? super T> specification;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private readonly System.Predicate<object> _specification;

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: FilterIterable(Iterable<T> iterable, System.Predicate<? super T> specification)
		 internal FilterIterable<T1>( IEnumerable<T> iterable, System.Predicate<T1> specification )
		 {
			  this._iterable = iterable;
			  this._specification = specification;
		 }

		 public override IEnumerator<T> Iterator()
		 {
			  return new FilterIterator<T>( _iterable.GetEnumerator(), _specification );
		 }

		 internal class FilterIterator<T> : IEnumerator<T>
		 {
			  internal readonly IEnumerator<T> Iterator;

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private final System.Predicate<? super T> specification;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  internal readonly System.Predicate<object> Specification;

			  internal T CurrentValue;
			  internal bool Finished;
			  internal bool NextConsumed = true;

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: FilterIterator(java.util.Iterator<T> iterator, System.Predicate<? super T> specification)
			  internal FilterIterator<T1>( IEnumerator<T> iterator, System.Predicate<T1> specification )
			  {
					this.Specification = specification;
					this.Iterator = iterator;
			  }

			  internal virtual bool MoveToNextValid()
			  {
					bool found = false;
					while ( !found && Iterator.MoveNext() )
					{
						 T currentValue = Iterator.Current;
						 bool satisfies = Specification.test( currentValue );

						 if ( satisfies )
						 {
							  found = true;
							  this.CurrentValue = currentValue;
							  NextConsumed = false;
						 }
					}
					if ( !found )
					{
						 Finished = true;
					}
					return found;
			  }

			  public override T Next()
			  {
					if ( !NextConsumed )
					{
						 NextConsumed = true;
						 return CurrentValue;
					}
					else
					{
						 if ( !Finished )
						 {
							  if ( MoveToNextValid() )
							  {
									NextConsumed = true;
									return CurrentValue;
							  }
						 }
					}
					throw new NoSuchElementException( "This iterator is exhausted." );
			  }

			  public override bool HasNext()
			  {
					return !Finished && ( !NextConsumed || MoveToNextValid() );
			  }

			  public override void Remove()
			  {
			  }
		 }
	}

}
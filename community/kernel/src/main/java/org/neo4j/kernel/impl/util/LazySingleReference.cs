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
namespace Org.Neo4j.Kernel.impl.util
{

	/// <summary>
	/// Manages a lazy initialized single reference that can be <seealso cref="invalidate() invalidated"/>.
	/// Concurrent <seealso cref="get() access"/> is supported and only a single instance will be <seealso cref="create() created"/>.
	/// </summary>
	public abstract class LazySingleReference<T> : System.Func<T>
	{
		 private volatile T _reference;

		 /// <returns> whether or not the managed reference has been initialized, i.e <seealso cref="get() evaluated"/>
		 /// for the first time, or after <seealso cref="invalidate() invalidated"/>. </returns>
		 public virtual bool Created
		 {
			 get
			 {
				  return _reference != default( T );
			 }
		 }

		 /// <summary>
		 /// Returns the reference, initializing it if need be.
		 /// </summary>
		 public override T Get()
		 {
			  T result;
			  if ( ( result = _reference ) == default( T ) )
			  {
					lock ( this )
					{
						 if ( ( result = _reference ) == default( T ) )
						 {
							  result = _reference = Create();
						 }
					}
			  }
			  return result;
		 }

		 /// <summary>
		 /// Invalidates any initialized reference. A future call to <seealso cref="get()"/> will have it initialized again.
		 /// </summary>
		 public virtual void Invalidate()
		 {
			 lock ( this )
			 {
				  _reference = default( T );
			 }
		 }

		 /// <summary>
		 /// Provides a reference to manage.
		 /// </summary>
		 protected internal abstract T Create();
	}

}
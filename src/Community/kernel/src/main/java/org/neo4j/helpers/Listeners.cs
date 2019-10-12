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
namespace Neo4Net.Helpers
{


	/// <summary>
	/// Mutable thread-safe container of listeners that can be notified with <seealso cref="Notification"/>.
	/// </summary>
	/// @param <T> the type of listeners. </param>
	[Obsolete]
	public class Listeners<T> : IEnumerable<T>
	{
		 private readonly IList<T> _listeners;

		 /// <summary>
		 /// Construct new empty listeners;
		 /// </summary>
		 [Obsolete]
		 public Listeners()
		 {
			  this._listeners = CreateListeners( emptyList() );
		 }

		 /// <summary>
		 /// Construct a copy of the given listeners.
		 /// </summary>
		 /// <param name="other"> listeners to copy. </param>
		 [Obsolete]
		 public Listeners( Listeners<T> other )
		 {
			  requireNonNull( other, "prototype listeners can't be null" );

			  this._listeners = CreateListeners( other._listeners );
		 }

		 /// <summary>
		 /// Adds the specified listener to this container.
		 /// </summary>
		 /// <param name="listener"> the listener to add. </param>
		 [Obsolete]
		 public virtual void Add( T listener )
		 {
			  requireNonNull( listener, "added listener can't be null" );

			  _listeners.Add( listener );
		 }

		 /// <summary>
		 /// Remove the first occurrence of the specified listener from this container, if it is present.
		 /// </summary>
		 /// <param name="listener"> the listener to remove. </param>
		 [Obsolete]
		 public virtual void Remove( T listener )
		 {
			  requireNonNull( listener, "removed listener can't be null" );

			  _listeners.Remove( listener );
		 }

		 /// <summary>
		 /// Notify all listeners in this container with the given notification.
		 /// Notification of each listener is synchronized on this listener.
		 /// </summary>
		 /// <param name="notification"> the notification to be applied to each listener. </param>
		 [Obsolete]
		 public virtual void Notify( Notification<T> notification )
		 {
			  requireNonNull( notification, "notification can't be null" );

			  foreach ( T listener in _listeners )
			  {
					NotifySingleListener( listener, notification );
			  }
		 }

		 /// <summary>
		 /// Notify all listeners in this container with the given notification using the given executor.
		 /// Each notification is submitted as a <seealso cref="System.Threading.ThreadStart"/> to the executor.
		 /// Notification of each listener is synchronized on this listener.
		 /// </summary>
		 /// <param name="executor"> the executor to submit notifications to. </param>
		 /// <param name="notification"> the notification to be applied to each listener. </param>
		 [Obsolete]
		 public virtual void Notify( Executor executor, Notification<T> notification )
		 {
			  requireNonNull( executor, "executor can't be null" );
			  requireNonNull( notification, "notification can't be null" );

			  foreach ( T listener in _listeners )
			  {
					executor.execute( () => notifySingleListener(listener, notification) );
			  }
		 }

		 /// <summary>
		 /// Returns the iterator over listeners in this container in the order they were added.
		 /// <para>
		 /// The returned iterator provides a snapshot of the state of the list
		 /// when the iterator was constructed. No synchronization is needed while
		 /// traversing the iterator. The iterator does <em>NOT</em> support the
		 /// {@code remove} method.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> iterator over listeners. </returns>
		 public override IEnumerator<T> Iterator()
		 {
			  return _listeners.GetEnumerator();
		 }

		 private static void NotifySingleListener<T>( T listener, Notification<T> notification )
		 {
			  lock ( listener )
			  {
					notification.Notify( listener );
			  }
		 }

		 private static IList<T> CreateListeners<T>( IList<T> existingListeners )
		 {
			  IList<T> result = new CopyOnWriteArrayList<T>();
			  ( ( IList<T> )result ).AddRange( existingListeners );
			  return result;
		 }

		 [Obsolete]
		 public interface Notification<T>
		 {
			  void Notify( T listener );
		 }
	}

}
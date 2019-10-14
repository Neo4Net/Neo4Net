using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

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
namespace Neo4Net.Io.pagecache.tracing.recording
{
	using Matcher = org.hamcrest.Matcher;


	public class RecordingTracer
	{
		 private readonly ISet<Type> _eventTypesToTrace = new HashSet<Type>();
		 private readonly BlockingQueue<Event> _record = new LinkedBlockingQueue<Event>();
		 private System.Threading.CountdownEvent _trapLatch;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.hamcrest.Matcher<? extends Event> trap;
		 private Matcher<Event> _trap;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public RecordingTracer(Class... eventTypesToTrace)
		 public RecordingTracer( params Type[] eventTypesToTrace )
		 {
			  Collections.addAll( this._eventTypesToTrace, eventTypesToTrace );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <T extends Event> T observe(Class<T> type) throws InterruptedException
		 public virtual T Observe<T>( Type type ) where T : Event
		 {
				 type = typeof( T );
			  return type.cast( _record.take() );
		 }

		 public virtual T TryObserve<T>( Type type ) where T : Event
		 {
				 type = typeof( T );
			  return type.cast( _record.poll() );
		 }

		 protected internal virtual void Record( Event @event )
		 {
			  if ( _eventTypesToTrace.Contains( @event.GetType() ) )
			  {
					_record.add( @event );
					Trip( @event );
			  }
		 }

		 /// <summary>
		 /// Set a trap for the eviction thread, and return a CountDownLatch with a counter set to 1.
		 /// When the eviction thread performs the given trap-event, it will block on the latch after
		 /// making the event observable.
		 /// </summary>
		 public virtual System.Threading.CountdownEvent Trap<T1>( Matcher<T1> trap ) where T1 : Event
		 {
			 lock ( this )
			 {
				  Debug.Assert( trap != null );
				  _trapLatch = new System.Threading.CountdownEvent( 1 );
				  this._trap = trap;
				  return _trapLatch;
			 }
		 }

		 private void Trip( Event @event )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<? extends Event> theTrap;
			  Matcher<Event> theTrap;
			  System.Threading.CountdownEvent theTrapLatch;

			  // The synchronized block is in here, so we don't risk calling await on
			  // the trapLatch while holding the monitor lock.
			  lock ( this )
			  {
					theTrap = _trap;
					theTrapLatch = _trapLatch;
			  }

			  if ( theTrap != null && theTrap.matches( @event ) )
			  {
					try
					{
						 theTrapLatch.await();
					}
					catch ( InterruptedException e )
					{
						 Thread.CurrentThread.Interrupt();
						 throw new Exception( "Unexpected interrupt in RecordingMonitor", e );
					}
			  }
		 }

	}

}
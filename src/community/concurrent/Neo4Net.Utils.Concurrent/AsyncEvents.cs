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
namespace Neo4Net.Utils.Concurrent
{

	/// <summary>
	/// {@code AsyncEvents} is a mechanism for queueing up events to be processed asynchronously in a background thread.
	/// <para>
	/// The {@code AsyncEvents} object implements <seealso cref="System.Threading.ThreadStart"/>, so it can be passed to a thread pool, or given to a
	/// dedicated thread. The runnable will then occupy a thread and dedicate it to background processing of events, until
	/// the {@code AsyncEvents} is <seealso cref="AsyncEvents.shutdown()"/>.
	/// </para>
	/// <para>
	/// If events are sent to an {@code AsyncEvents} that has been shut down, then those events will be processed in the
	/// foreground as a fall-back.
	/// </para>
	/// <para>
	/// Note, however, that no events are processed until the background thread is started.
	/// </para>
	/// <para>
	/// The {@code AsyncEvents} is given a <seealso cref="Consumer"/> of the specified event type upon construction, and will use it
	/// for doing the actual processing of events once they have been collected.
	/// 
	/// </para>
	/// </summary>
	/// @param <T> The type of events the {@code AsyncEvents} will process. </param>
	public class AsyncEvents<T> : AsyncEventSender<T>, ThreadStart where T : AsyncEvent
	{
		 public interface Monitor
		 {
			  void EventCount( long count );
		 }

		 public static class Monitor_Fields
		 {
			 private readonly AsyncEvents<T> _outerInstance;

			 public Monitor_Fields( AsyncEvents<T> outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public static readonly Monitor None = count =>
			  {
			  };

		 }

		 // TODO use VarHandles in Java 9
		 private static readonly AtomicReferenceFieldUpdater<AsyncEvents, AsyncEvent> _stackUpdater = AtomicReferenceFieldUpdater.newUpdater( typeof( AsyncEvents ), typeof( AsyncEvent ), "stack" );
		 private static readonly Sentinel _endSentinel = new Sentinel( "END" );
		 private static readonly Sentinel _shutdownSentinel = new Sentinel( "SHUTDOWN" );

		 private readonly System.Action<T> _eventConsumer;
		 private readonly Monitor _monitor;
		 private readonly BinaryLatch _startupLatch;
		 private readonly BinaryLatch _shutdownLatch;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"unused", "FieldCanBeLocal"}) private volatile AsyncEvent stack;
		 private volatile AsyncEvent _stack; // Accessed via AtomicReferenceFieldUpdater
		 private volatile Thread _backgroundThread;
		 private volatile bool _shutdown;

		 /// <summary>
		 /// Construct a new {@code AsyncEvents} instance, that will use the given consumer to process the events.
		 /// </summary>
		 /// <param name="eventConsumer"> The <seealso cref="Consumer"/> used for processing the events that are sent in. </param>
		 public AsyncEvents( System.Action<T> eventConsumer, Monitor monitor )
		 {
			  this._eventConsumer = eventConsumer;
			  this._monitor = monitor;
			  this._startupLatch = new BinaryLatch();
			  this._shutdownLatch = new BinaryLatch();
			  this._stack = _endSentinel;
		 }

		 public override void Send( T @event )
		 {
			  AsyncEvent prev = _stackUpdater.getAndSet( this, @event );
			  Debug.Assert( prev != null );
			  @event.Next = prev;
			  if ( prev == _endSentinel )
			  {
					LockSupport.unpark( _backgroundThread );
			  }
			  else if ( prev == _shutdownSentinel )
			  {
					AsyncEvent events = _stackUpdater.getAndSet( this, _shutdownSentinel );
					Process( events );
			  }
		 }

		 public override void Run()
		 {
			  Debug.Assert( _backgroundThread == null, "A thread is already running " + _backgroundThread );
			  _backgroundThread = Thread.CurrentThread;
			  _startupLatch.release();

			  try
			  {
					do
					{
						 AsyncEvent events = _stackUpdater.getAndSet( this, _endSentinel );
						 Process( events );
						 if ( _stack == _endSentinel && !_shutdown )
						 {
							  LockSupport.park( this );
						 }
					} while ( !_shutdown );

					AsyncEvent events = _stackUpdater.getAndSet( this, _shutdownSentinel );
					Process( events );
			  }
			  finally
			  {
					_backgroundThread = null;
					_shutdownLatch.release();
			  }
		 }

		 private void Process( AsyncEvent events )
		 {
			  events = ReverseAndStripEndMark( events );

			  while ( events != null )
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") T event = (T) events;
					T @event = ( T ) events;
					_eventConsumer.accept( @event );
					events = events.Next;
			  }
		 }

		 private AsyncEvent ReverseAndStripEndMark( AsyncEvent events )
		 {
			  AsyncEvent result = null;
			  long count = 0;
			  while ( events != _endSentinel && events != _shutdownSentinel )
			  {
					AsyncEvent next;
					do
					{
						 next = events.Next;
					} while ( next == null );
					events.Next = result;
					result = events;
					events = next;
					count++;
			  }
			  if ( count > 0 )
			  {
					_monitor.eventCount( count );
			  }
			  return result;
		 }

		 /// <summary>
		 /// Initiate the shut down process of this {@code AsyncEvents} instance.
		 /// <para>
		 /// This call does not block or otherwise wait for the background thread to terminate.
		 /// </para>
		 /// </summary>
		 public virtual void Shutdown()
		 {
			  Debug.Assert( !_shutdown, "Already shut down" );
			  _shutdown = true;
			  LockSupport.unpark( _backgroundThread );
		 }

		 public virtual void AwaitStartup()
		 {
			  _startupLatch.await();
		 }

		 public virtual void AwaitTermination()
		 {
			  _shutdownLatch.await();
		 }

		 private class Sentinel : AsyncEvent
		 {
			  internal readonly string Str;

			  internal Sentinel( string identifier )
			  {
					this.Str = "AsyncEvent/Sentinel[" + identifier + "]";
			  }

			  public override string ToString()
			  {
					return Str;
			  }
		 }
	}

}
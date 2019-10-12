using System;
using System.Threading;

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
namespace Neo4Net.Util.concurrent
{
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.notNullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.sameInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTimeout;

	internal class AsyncEventsTest
	{
		 private ExecutorService _executor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  _executor = Executors.newCachedThreadPool();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void tearDown()
		 internal virtual void TearDown()
		 {
			  _executor.shutdown();
		 }

		 internal class Event : AsyncEvent
		 {
			  internal Thread ProcessedBy;
		 }

		 internal class EventConsumer : System.Action<Event>
		 {
			  internal readonly BlockingQueue<Event> EventsProcessed = new LinkedBlockingQueue<Event>();

			  public override void Accept( Event @event )
			  {
					@event.ProcessedBy = Thread.CurrentThread;
					EventsProcessed.offer( @event );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Event poll(long timeout, java.util.concurrent.TimeUnit unit) throws InterruptedException
			  public virtual Event Poll( long timeout, TimeUnit unit )
			  {
					return EventsProcessed.poll( timeout, unit );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void eventsMustBeProcessedByBackgroundThread() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void EventsMustBeProcessedByBackgroundThread()
		 {
			  EventConsumer consumer = new EventConsumer();

			  AsyncEvents<Event> asyncEvents = new AsyncEvents<Event>( consumer, AsyncEvents.Monitor_Fields.None );
			  _executor.submit( asyncEvents );

			  Event firstSentEvent = new Event();
			  asyncEvents.Send( firstSentEvent );
			  Event firstProcessedEvent = consumer.Poll( 10, TimeUnit.SECONDS );

			  Event secondSentEvent = new Event();
			  asyncEvents.Send( secondSentEvent );
			  Event secondProcessedEvent = consumer.Poll( 10, TimeUnit.SECONDS );

			  asyncEvents.Shutdown();

			  assertThat( firstProcessedEvent, @is( firstSentEvent ) );
			  assertThat( secondProcessedEvent, @is( secondSentEvent ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustNotProcessEventInSameThreadWhenNotShutDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustNotProcessEventInSameThreadWhenNotShutDown()
		 {
			  EventConsumer consumer = new EventConsumer();

			  AsyncEvents<Event> asyncEvents = new AsyncEvents<Event>( consumer, AsyncEvents.Monitor_Fields.None );
			  _executor.submit( asyncEvents );

			  asyncEvents.Send( new Event() );

			  Thread processingThread = consumer.Poll( 10, TimeUnit.SECONDS ).ProcessedBy;
			  asyncEvents.Shutdown();

			  assertThat( processingThread, @is( not( Thread.CurrentThread ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustProcessEventsDirectlyWhenShutDown()
		 internal virtual void MustProcessEventsDirectlyWhenShutDown()
		 {
			  assertTimeout(ofSeconds(10), () =>
			  {
				EventConsumer consumer = new EventConsumer();

				AsyncEvents<Event> asyncEvents = new AsyncEvents<Event>( consumer, AsyncEvents.Monitor_Fields.None );
				_executor.submit( asyncEvents );

				asyncEvents.Send( new Event() );
				Thread threadForFirstEvent = consumer.Poll( 10, TimeUnit.SECONDS ).ProcessedBy;
				asyncEvents.Shutdown();

				assertThat( threadForFirstEvent, @is( not( Thread.CurrentThread ) ) );

				Thread threadForSubsequentEvents;
				do
				{
					 asyncEvents.Send( new Event() );
					 threadForSubsequentEvents = consumer.Poll( 10, TimeUnit.SECONDS ).ProcessedBy;
				} while ( threadForSubsequentEvents != Thread.CurrentThread );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void concurrentlyPublishedEventsMustAllBeProcessed()
		 internal virtual void ConcurrentlyPublishedEventsMustAllBeProcessed()
		 {
			  assertTimeout(ofSeconds(10), () =>
			  {
				EventConsumer consumer = new EventConsumer();
				System.Threading.CountdownEvent startLatch = new System.Threading.CountdownEvent( 1 );
				const int threads = 10;
				const int iterations = 2_000;
				AsyncEvents<Event> asyncEvents = new AsyncEvents<Event>( consumer, AsyncEvents.Monitor_Fields.None );
				_executor.submit( asyncEvents );

				ExecutorService threadPool = Executors.newFixedThreadPool( threads );
				ThreadStart runner = () =>
				{
					 try
					 {
						  startLatch.await();
					 }
					 catch ( InterruptedException e )
					 {
						  throw new Exception( e );
					 }

					 for ( int i = 0; i < iterations; i++ )
					 {
						  asyncEvents.Send( new Event() );
					 }
				};
				for ( int i = 0; i < threads; i++ )
				{
					 threadPool.submit( runner );
				}
				startLatch.countDown();

				Thread thisThread = Thread.CurrentThread;
				int eventCount = threads * iterations;
				try
				{
					 for ( int i = 0; i < eventCount; i++ )
					 {
						  Event @event = consumer.Poll( 1, TimeUnit.SECONDS );
						  if ( @event == null )
						  {
								i--;
						  }
						  else
						  {
								assertThat( @event.ProcessedBy, @is( not( thisThread ) ) );
						  }
					 }
				}
				finally
				{
					 asyncEvents.Shutdown();
					 threadPool.shutdown();
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void awaitingShutdownMustBlockUntilAllMessagesHaveBeenProcessed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void AwaitingShutdownMustBlockUntilAllMessagesHaveBeenProcessed()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Event specialShutdownObservedEvent = new Event();
			  Event specialShutdownObservedEvent = new Event();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch awaitStartLatch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent awaitStartLatch = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final EventConsumer consumer = new EventConsumer();
			  EventConsumer consumer = new EventConsumer();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final AsyncEvents<Event> asyncEvents = new AsyncEvents<>(consumer, AsyncEvents.Monitor_Fields.NONE);
			  AsyncEvents<Event> asyncEvents = new AsyncEvents<Event>( consumer, AsyncEvents.Monitor_Fields.None );
			  _executor.submit( asyncEvents );

			  // Wait for the background thread to start processing events
			  do
			  {
					asyncEvents.Send( new Event() );
			  } while ( consumer.EventsProcessed.take().processedBy == Thread.CurrentThread );

			  // Start a thread that awaits the termination
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> awaitShutdownFuture = executor.submit(() ->
			  Future<object> awaitShutdownFuture = _executor.submit(() =>
			  {
				awaitStartLatch.Signal();
				asyncEvents.AwaitTermination();
				consumer.EventsProcessed.offer( specialShutdownObservedEvent );
			  });

			  awaitStartLatch.await();

			  // Send 5 events
			  asyncEvents.Send( new Event() );
			  asyncEvents.Send( new Event() );
			  asyncEvents.Send( new Event() );
			  asyncEvents.Send( new Event() );
			  asyncEvents.Send( new Event() );

			  // Observe 5 events processed
			  assertThat( consumer.EventsProcessed.take(), @is(notNullValue()) );
			  assertThat( consumer.EventsProcessed.take(), @is(notNullValue()) );
			  assertThat( consumer.EventsProcessed.take(), @is(notNullValue()) );
			  assertThat( consumer.EventsProcessed.take(), @is(notNullValue()) );
			  assertThat( consumer.EventsProcessed.take(), @is(notNullValue()) );

			  // Observe no events left
			  assertThat( consumer.EventsProcessed.poll( 20, TimeUnit.MILLISECONDS ), @is( nullValue() ) );

			  // Shutdown and await termination
			  asyncEvents.Shutdown();
			  awaitShutdownFuture.get();

			  // Observe termination
			  assertThat( consumer.EventsProcessed.take(), sameInstance(specialShutdownObservedEvent) );
		 }
	}

}
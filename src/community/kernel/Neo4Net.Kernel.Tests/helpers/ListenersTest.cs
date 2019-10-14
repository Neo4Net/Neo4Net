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
namespace Neo4Net.Helpers
{
	using Test = org.junit.jupiter.api.Test;


	using Iterables = Neo4Net.Helpers.Collections.Iterables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Thread.currentThread;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.NamedThreadFactory.named;

	internal class ListenersTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void copyConstructorWithNull()
		 internal virtual void CopyConstructorWithNull()
		 {
			  assertThrows( typeof( System.NullReferenceException ), () => new Listeners<>(null) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void copyConstructor()
		 internal virtual void CopyConstructor()
		 {
			  Listeners<Listener> original = NewListeners( new Listener(), new Listener(), new Listener() );

			  Listeners<Listener> copy = new Listeners<Listener>( original );

			  assertEquals( Iterables.asList( original ), Iterables.asList( copy ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addNull()
		 internal virtual void AddNull()
		 {
			  assertThrows( typeof( System.NullReferenceException ), () => (new Listeners<>()).add(null) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void add()
		 internal virtual void Add()
		 {
			  Listener[] listenersArray = new Listener[]
			  {
				  new Listener(),
				  new Listener(),
				  new Listener()
			  };

			  Listeners<Listener> listeners = NewListeners( listenersArray );

			  assertArrayEquals( listenersArray, Iterables.asArray( typeof( Listener ), listeners ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void removeNull()
		 internal virtual void RemoveNull()
		 {
			  assertThrows( typeof( System.NullReferenceException ), () => (new Listeners<>()).remove(null) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void remove()
		 internal virtual void Remove()
		 {
			  Listener listener1 = new Listener();
			  Listener listener2 = new Listener();
			  Listener listener3 = new Listener();

			  Listeners<Listener> listeners = NewListeners( listener1, listener2, listener3 );

			  assertEquals( Arrays.asList( listener1, listener2, listener3 ), Iterables.asList( listeners ) );

			  listeners.Remove( listener1 );
			  assertEquals( Arrays.asList( listener2, listener3 ), Iterables.asList( listeners ) );

			  listeners.Remove( listener3 );
			  assertEquals( singletonList( listener2 ), Iterables.asList( listeners ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void notifyWithNullNotification()
		 internal virtual void NotifyWithNullNotification()
		 {
			  assertThrows( typeof( System.NullReferenceException ), () => (new Listeners<>()).notify(null) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void notifyWithNotification()
		 internal virtual void NotifyWithNotification()
		 {
			  string message = "foo";
			  Listener listener1 = new Listener();
			  Listener listener2 = new Listener();

			  Listeners<Listener> listeners = NewListeners( listener1, listener2 );

			  listeners.Notify( listener => listener.process( message ) );

			  assertEquals( message, listener1.Message );
			  assertEquals( currentThread().Name, listener1.ThreadName );

			  assertEquals( message, listener2.Message );
			  assertEquals( currentThread().Name, listener2.ThreadName );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void notifyWithNullExecutorAndNullNotification()
		 internal virtual void NotifyWithNullExecutorAndNullNotification()
		 {
			  assertThrows( typeof( System.NullReferenceException ), () => (new Listeners<>()).notify(null, null) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void notifyWithNullExecutorAndNotification()
		 internal virtual void NotifyWithNullExecutorAndNotification()
		 {
			  assertThrows( typeof( System.NullReferenceException ), () => (new Listeners<Listener>()).notify(null, listener => listener.process("foo")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void notifyWithExecutorAndNullNotification()
		 internal virtual void NotifyWithExecutorAndNullNotification()
		 {
			  assertThrows( typeof( System.NullReferenceException ), () => (new Listeners<Listener>()).notify(newSingleThreadExecutor(), null) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void notifyWithExecutorAndNotification() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NotifyWithExecutorAndNotification()
		 {
			  string message = "foo";
			  string threadNamePrefix = "test-thread";
			  Listener listener1 = new Listener();
			  Listener listener2 = new Listener();

			  Listeners<Listener> listeners = NewListeners( listener1, listener2 );

			  ExecutorService executor = newSingleThreadExecutor( named( threadNamePrefix ) );
			  listeners.Notify( executor, listener => listener.process( message ) );
			  executor.shutdown();
			  executor.awaitTermination( 1, TimeUnit.MINUTES );

			  assertEquals( message, listener1.Message );
			  assertThat( listener1.ThreadName, startsWith( threadNamePrefix ) );

			  assertEquals( message, listener2.Message );
			  assertThat( listener2.ThreadName, startsWith( threadNamePrefix ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void listenersIterable()
		 internal virtual void ListenersIterable()
		 {
			  Listener listener1 = new Listener();
			  Listener listener2 = new Listener();
			  Listener listener3 = new Listener();

			  Listeners<Listener> listeners = NewListeners( listener1, listener2, listener3 );

			  assertEquals( Arrays.asList( listener1, listener2, listener3 ), Iterables.asList( listeners ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private static <T> Listeners<T> newListeners(T... listeners)
		 private static Listeners<T> NewListeners<T>( params T[] listeners )
		 {
			  Listeners<T> result = new Listeners<T>();
			  foreach ( T listener in listeners )
			  {
					result.Add( listener );
			  }
			  return result;
		 }

		 private class Listener
		 {
			  internal volatile string Message;
			  internal volatile string ThreadName;

			  internal virtual void Process( string message )
			  {
					this.Message = message;
					this.ThreadName = currentThread().Name;
			  }
		 }
	}

}
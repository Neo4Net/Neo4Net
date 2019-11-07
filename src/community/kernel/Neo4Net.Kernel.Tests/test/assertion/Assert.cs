using System;
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
namespace Neo4Net.Test.assertion
{
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using StringDescription = org.hamcrest.StringDescription;


	using Neo4Net.Functions;
	using Neo4Net.Functions;
	using ArrayUtil = Neo4Net.Helpers.ArrayUtil;
	using Strings = Neo4Net.Helpers.Strings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.StringContains.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public sealed class Assert
	{
		 private Assert()
		 {
		 }

		 public static void AssertException<E>( ThrowingAction<E> f, Type typeOfException ) where E : Exception
		 {
			  AssertException( f, typeOfException, null );
		 }

		 public static void AssertException<E>( ThrowingAction<E> f, Type typeOfException, string partOfErrorMessage ) where E : Exception
		 {
			  try
			  {
					f.Apply();
					fail( "Expected exception of type " + typeOfException + ", but no exception was thrown" );
			  }
			  catch ( Exception e )
			  {
					if ( typeOfException.IsInstanceOfType( e ) )
					{
						 if ( !string.ReferenceEquals( partOfErrorMessage, null ) )
						 {
							  assertThat( e.Message, containsString( partOfErrorMessage ) );
						 }
					}
					else
					{
						 fail( "Got unexpected exception " + e.GetType() + "\nExpected: " + typeOfException );
					}
			  }
		 }

		 public static void AssertObjectOrArrayEquals( object expected, object actual )
		 {
			  AssertObjectOrArrayEquals( "", expected, actual );
		 }

		 public static void AssertObjectOrArrayEquals( string message, object expected, object actual )
		 {
			  if ( expected.GetType().IsArray )
			  {
					if ( !ArrayUtil.Equals( expected, actual ) )
					{
						 throw NewAssertionError( message, expected, actual );
					}
			  }
			  else
			  {
					if ( !Objects.Equals( expected, actual ) )
					{
						 throw NewAssertionError( message, expected, actual );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static <T, E extends Exception> void assertEventually(Neo4Net.function.ThrowingSupplier<T, E> actual, org.hamcrest.Matcher<? super T> matcher, long timeout, java.util.concurrent.TimeUnit timeUnit) throws E, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void AssertEventually<T, E, T1>( ThrowingSupplier<T, E> actual, Matcher<T1> matcher, long timeout, TimeUnit timeUnit ) where E : Exception
		 {
			  assertEventually( ignored => "", actual, matcher, timeout, timeUnit );
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static <T, E extends Exception> void assertEventually(String reason, Neo4Net.function.ThrowingSupplier<T, E> actual, org.hamcrest.Matcher<? super T> matcher, long timeout, java.util.concurrent.TimeUnit timeUnit) throws E, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void AssertEventually<T, E, T1>( string reason, ThrowingSupplier<T, E> actual, Matcher<T1> matcher, long timeout, TimeUnit timeUnit ) where E : Exception
		 {
			  assertEventually( ignored => reason, actual, matcher, timeout, timeUnit );
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static <T, E extends Exception> void assertEventually(System.Func<T, String> reason, Neo4Net.function.ThrowingSupplier<T, E> actual, org.hamcrest.Matcher<? super T> matcher, long timeout, java.util.concurrent.TimeUnit timeUnit) throws E, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void AssertEventually<T, E, T1>( System.Func<T, string> reason, ThrowingSupplier<T, E> actual, Matcher<T1> matcher, long timeout, TimeUnit timeUnit ) where E : Exception
		 {
			  long endTimeMillis = DateTimeHelper.CurrentUnixTimeMillis() + timeUnit.toMillis(timeout);

			  T last;
			  bool matched;

			  do
			  {
					long sampleTime = DateTimeHelper.CurrentUnixTimeMillis();

					last = actual.Get();
					matched = matcher.matches( last );

					if ( matched || sampleTime > endTimeMillis )
					{
						 break;
					}

					Thread.Sleep( 100 );
			  } while ( true );

			  if ( !matched )
			  {
					Description description = new StringDescription();
					description.appendText( reason( last ) ).appendText( "\nExpected: " ).appendDescriptionOf( matcher ).appendText( "\n     but: " );
					matcher.describeMismatch( last, description );

					throw new AssertionError( "Timeout hit (" + timeout + " " + timeUnit.ToString().ToLower() + ") while waiting for condition to match: " + description.ToString() );
			  }
		 }

		 private static AssertionError NewAssertionError( string message, object expected, object actual )
		 {
			  return new AssertionError( ( ( string.ReferenceEquals( message, null ) || message.Length == 0 ) ? "" : message + "\n" ) + "Expected: " + Strings.prettyPrint( expected ) + ", actual: " + Strings.prettyPrint( actual ) );
		 }
	}

}
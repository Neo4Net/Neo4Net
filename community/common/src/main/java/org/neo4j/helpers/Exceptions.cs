using System;
using System.Text;
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
namespace Org.Neo4j.Helpers
{
	using State = Thread.State;
	using UncaughtExceptionHandler = Thread.UncaughtExceptionHandler;

	using Predicates = Org.Neo4j.Function.Predicates;

	/// @deprecated This class will be removed from public API in 4.0. 
	[Obsolete("This class will be removed from public API in 4.0.")]
	public class Exceptions
	{
		 public static readonly Thread.UncaughtExceptionHandler SilentUncaughtExceptionHandler = ( t, e ) =>
		 { // Don't print about it
		 };

		 private const string UNEXPECTED_MESSAGE = "Unexpected Exception";

		 private Exceptions()
		 {
			  throw new AssertionError( "No instances" );
		 }

		 /// <summary>
		 /// Rethrows {@code exception} if it is an instance of <seealso cref="System.Exception"/> or <seealso cref="System.Exception"/>. Typical usage is:
		 /// 
		 /// <pre>
		 /// catch (Throwable e) {
		 ///   ......common code......
		 ///   throwIfUnchecked(e);
		 ///   throw new RuntimeException(e);
		 /// }
		 /// </pre>
		 /// 
		 /// This will only wrap checked exception in a {@code RuntimeException}. Do note that if the segment {@code common code}
		 /// is missing, it's preferable to use this instead:
		 /// 
		 /// <pre>
		 /// catch (RuntimeException | Error e) {
		 ///   throw e;
		 /// }
		 /// catch (Throwable e) {
		 ///   throw new RuntimeException(e);
		 /// }
		 /// </pre>
		 /// </summary>
		 /// <param name="exception"> to rethrow. </param>
		 public static void ThrowIfUnchecked( Exception exception )
		 {
			  Objects.requireNonNull( exception );
			  if ( exception is Exception )
			  {
					throw ( Exception ) exception;
			  }
			  if ( exception is Exception )
			  {
					throw ( Exception ) exception;
			  }
		 }

		 /// <summary>
		 /// Will rethrow the provided {@code exception} if it is an instance of {@code clazz}. Typical usage is:
		 /// 
		 /// <pre>
		 /// catch (Throwable e) {
		 ///   ......common code......
		 ///   throwIfInstanceOf(e, BarException.class);
		 ///   throw new RuntimeException(e);
		 /// }
		 /// </pre>
		 /// 
		 /// This will filter out all {@code BarExceptions} and wrap the rest in {@code RuntimeException}. Do note that if the
		 /// segment {@code common code} is missing, it's preferable to use this instead:
		 /// 
		 /// <pre>
		 /// catch (BarException e) {
		 ///   throw e;
		 /// } catch (Throwable e) {
		 ///   threw new RuntimeException(e);
		 /// }
		 /// </pre>
		 /// </summary>
		 /// <param name="exception"> to rethrow. </param>
		 /// <param name="clazz"> a <seealso cref="System.Type"/> instance to check against. </param>
		 /// @param <T> type thrown, if thrown at all. </param>
		 /// <exception cref="T"> if {@code exception} is an instance of {@code clazz}. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <T extends Throwable> void throwIfInstanceOf(Throwable exception, Class<T> clazz) throws T
		 public static void ThrowIfInstanceOf<T>( Exception exception, Type clazz ) where T : Exception
		 {
				 clazz = typeof( T );
			  Objects.requireNonNull( exception );
			  if ( clazz.IsInstanceOfType( exception ) )
			  {
					throw clazz.cast( exception );
			  }
		 }

		 /// @deprecated Use <seealso cref="Throwable.initCause(System.Exception)"/> instead. 
		 [Obsolete("Use <seealso cref=\"Throwable.initCause(System.Exception)\"/> instead.")]
		 public static T WithCause<T>( T exception, Exception cause ) where T : Exception
		 {
			  try
			  {
					exception.initCause( cause );
			  }
			  catch ( Exception )
			  {
					// OK, we did our best, guess there will be no cause
			  }
			  return exception;
		 }

		 /// @deprecated Use <seealso cref="Throwable.addSuppressed(System.Exception)"/> instead. 
		 [Obsolete("Use <seealso cref=\"Throwable.addSuppressed(System.Exception)\"/> instead.")]
		 public static T WithSuppressed<T>( T exception, params Exception[] suppressed ) where T : Exception
		 {
			  if ( suppressed != null )
			  {
					foreach ( Exception s in suppressed )
					{
						 exception.addSuppressed( s );
					}
			  }
			  return exception;
		 }

		 /// @deprecated See <seealso cref="launderedException(System.Type, string, System.Exception)"/>. 
		 [Obsolete("See <seealso cref=\"launderedException(System.Type, string, System.Exception)\"/>.")]
		 public static Exception LaunderedException( Exception exception )
		 {
			  return LaunderedException( typeof( Exception ), UNEXPECTED_MESSAGE, exception );
		 }

		 /// @deprecated See <seealso cref="launderedException(System.Type, string, System.Exception)"/>. 
		 [Obsolete("See <seealso cref=\"launderedException(System.Type, string, System.Exception)\"/>.")]
		 public static Exception LaunderedException( string messageForUnexpected, Exception exception )
		 {
			  return LaunderedException( typeof( Exception ), messageForUnexpected, exception );
		 }

		 /// @deprecated See <seealso cref="launderedException(System.Type, string, System.Exception)"/>. 
		 [Obsolete("See <seealso cref=\"launderedException(System.Type, string, System.Exception)\"/>.")]
		 public static T LaunderedException<T>( Type type, Exception exception ) where T : Exception
		 {
				 type = typeof( T );
			  return LaunderedException( type, UNEXPECTED_MESSAGE, exception );
		 }

		 /// @deprecated use {@code throw e} or {@code throw new RuntimeException(e)} directly. Prefer multi-caches if applicable.
		 /// For more elaborate scenarios, have a look at <seealso cref="throwIfUnchecked(System.Exception)"/> and
		 /// <seealso cref="throwIfInstanceOf(System.Exception, System.Type)"/>
		 /// <para>
		 /// For a more furrow explanation take a look at the very similar case:
		 /// <a href="https://goo.gl/Ivn2kc">Why we deprecated {@code Throwables.propagate}</a> 
		 [Obsolete("use {@code throw e} or {@code throw new RuntimeException(e)} directly. Prefer multi-caches if applicable.")]
		 public static T LaunderedException<T>( Type type, string messageForUnexpected, Exception exception ) where T : Exception
		 {
				 type = typeof( T );
			  if ( type.IsInstanceOfType( exception ) )
			  {
					return type.cast( exception );
			  }
			  else if ( exception is Exception )
			  {
					throw ( Exception ) exception;
			  }
			  else if ( exception is InvocationTargetException )
			  {
					return LaunderedException( type, messageForUnexpected, ( ( InvocationTargetException ) exception ).TargetException );
			  }
			  else if ( exception is Exception )
			  {
					throw ( Exception ) exception;
			  }
			  else
			  {
					throw new Exception( messageForUnexpected, exception );
			  }
		 }

		 /// <summary>
		 /// Peels off layers of causes. For example:
		 /// 
		 /// MyFarOuterException
		 ///   cause: MyOuterException
		 ///     cause: MyInnerException
		 ///       cause: MyException
		 /// and a toPeel predicate returning true for MyFarOuterException and MyOuterException
		 /// will return MyInnerException. If the predicate peels all exceptions null is returned.
		 /// </summary>
		 /// <param name="exception"> the outer exception to peel to get to an delegate cause. </param>
		 /// <param name="toPeel"> <seealso cref="Predicate"/> for deciding what to peel. {@code true} means
		 /// to peel (i.e. remove), whereas the first {@code false} means stop and return. </param>
		 /// <returns> the delegate cause of an exception, dictated by the predicate. </returns>
		 public static Exception Peel( Exception exception, System.Predicate<Exception> toPeel )
		 {
			  while ( exception != null )
			  {
					if ( !toPeel( exception ) )
					{
						 break;
					}
					exception = exception.InnerException;
			  }
			  return exception;
		 }

		 /// <summary>
		 /// Returns the root cause of an exception.
		 /// </summary>
		 /// <param name="caughtException"> exception to find the root cause of. </param>
		 /// <returns> the root cause. </returns>
		 /// <exception cref="IllegalArgumentException"> if the provided exception is null. </exception>
		 public static Exception RootCause( Exception caughtException )
		 {
			  if ( null == caughtException )
			  {
					throw new System.ArgumentException( "Cannot obtain rootCause from (null)" );
			  }
			  Exception root = caughtException;
			  while ( root.InnerException != null )
			  {
					root = root.InnerException;
			  }
			  return root;
		 }

		 /// <summary>
		 /// Searches the entire exception hierarchy of causes and suppressed exceptions against the given predicate.
		 /// </summary>
		 /// <param name="e"> exception to start searching from. </param>
		 /// <returns> the first exception found matching the predicate. </returns>
		 public static Optional<Exception> FindCauseOrSuppressed( Exception e, System.Predicate<Exception> predicate )
		 {
			  if ( e == null )
			  {
					return null;
			  }
			  if ( predicate( e ) )
			  {
					return e;
			  }
			  if ( e.InnerException != null && e.InnerException != e )
			  {
					Optional<Exception> cause = FindCauseOrSuppressed( e.InnerException, predicate );
					if ( cause.Present )
					{
						 return cause;
					}
			  }
			  if ( e.Suppressed != null )
			  {
					foreach ( Exception suppressed in e.Suppressed )
					{
						 if ( suppressed == e )
						 {
							  continue;
						 }
						 Optional<Exception> cause = FindCauseOrSuppressed( suppressed, predicate );
						 if ( cause.Present )
						 {
							  return cause;
						 }
					}
			  }
			  return null;
		 }

		 public static string Stringify( Exception throwable )
		 {
			  if ( throwable == null )
			  {
					return null;
			  }

			  StringWriter stringWriter = new StringWriter();
			  throwable.printStackTrace( new PrintWriter( stringWriter ) );
			  return stringWriter.ToString();
		 }

		 public static string Stringify( Thread thread, StackTraceElement[] elements )
		 {
			  StringBuilder builder = new StringBuilder( "\"" + thread.Name + "\"" + ( thread.Daemon ? " daemon" : "" ) + " prio=" + thread.Priority + " tid=" + thread.Id + " " + thread.State.name().ToLower() + "\n" );
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  builder.Append( "   " ).Append( typeof( Thread.State ).FullName ).Append( ": " ).Append( thread.State.name().ToUpper() ).Append("\n");
			  foreach ( StackTraceElement element in elements )
			  {
					builder.Append( "      at " ).Append( element.ClassName ).Append( "." ).Append( element.MethodName );
					if ( element.NativeMethod )
					{
						 builder.Append( "(Native method)" );
					}
					else if ( element.FileName == null )
					{
						 builder.Append( "(Unknown source)" );
					}
					else
					{
						 builder.Append( "(" ).Append( element.FileName ).Append( ":" ).Append( element.LineNumber ).Append( ")" );
					}
					builder.Append( "\n" );
			  }
			  return builder.ToString();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") public static boolean contains(final Throwable cause, final String containsMessage, final Class... anyOfTheseClasses)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public static bool Contains( Exception cause, string containsMessage, params Type[] anyOfTheseClasses )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final System.Predicate<Throwable> anyOfClasses = org.neo4j.function.Predicates.instanceOfAny(anyOfTheseClasses);
			  System.Predicate<Exception> anyOfClasses = Predicates.instanceOfAny( anyOfTheseClasses );
			  return contains( cause, item => item.Message != null && item.Message.contains( containsMessage ) && anyOfClasses( item ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") public static boolean contains(Throwable cause, Class... anyOfTheseClasses)
		 public static bool Contains( Exception cause, params Type[] anyOfTheseClasses )
		 {
			  return Contains( cause, Predicates.instanceOfAny( anyOfTheseClasses ) );
		 }

		 public static bool Contains( Exception cause, System.Predicate<Exception> toLookFor )
		 {
			  while ( cause != null )
			  {
					if ( toLookFor( cause ) )
					{
						 return true;
					}
					cause = cause.InnerException;
			  }
			  return false;
		 }

		 /// @deprecated Use <seealso cref="Throwable.addSuppressed(System.Exception)"/> and <seealso cref="Throwable.initCause(System.Exception)"/> where
		 /// appropriate instead. 
		 [Obsolete("Use <seealso cref=\"Throwable.addSuppressed(System.Exception)\"/> and <seealso cref=\"Throwable.initCause(System.Exception)\"/> where")]
		 public static E Combine<E>( E first, E second ) where E : Exception
		 {
			  if ( first == null )
			  {
					return second;
			  }
			  if ( second == null )
			  {
					return first;
			  }

			  Exception current = first;
			  while ( current.InnerException != null )
			  {
					current = current.InnerException;
			  }

			  current.initCause( second );
			  return first;
		 }

		 private static readonly System.Reflection.FieldInfo _throwableMessageField;
		 static Exceptions()
		 {
			  try
			  {
					_throwableMessageField = typeof( Exception ).getDeclaredField( "detailMessage" );
					_throwableMessageField.Accessible = true;
			  }
			  catch ( Exception e )
			  {
					throw new LinkageError( "Could not get Throwable message field", e );
			  }
		 }

		 public static void SetMessage( Exception cause, string message )
		 {
			  try
			  {
					_throwableMessageField.set( cause, message );
			  }
			  catch ( Exception e ) when ( e is System.ArgumentException || e is IllegalAccessException )
			  {
					throw new Exception( e );
			  }
		 }

		 public static T WithMessage<T>( T cause, string message ) where T : Exception
		 {
			  SetMessage( cause, message );
			  return cause;
		 }

		 public static T Chain<T>( T initial, T current ) where T : Exception
		 {
			  if ( initial == null )
			  {
					return current;
			  }

			  if ( current != null )
			  {
					initial.addSuppressed( current );
			  }
			  return initial;
		 }

	}

}
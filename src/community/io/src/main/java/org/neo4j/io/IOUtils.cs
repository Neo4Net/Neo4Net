using System;
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
namespace Neo4Net.Io
{

	/// <summary>
	/// IO helper methods.
	/// </summary>
	public sealed class IOUtils
	{
		 private IOUtils()
		 {
		 }

		 /// <summary>
		 /// Closes given <seealso cref="System.Collections.ICollection collection"/> of <seealso cref="IDisposable closeables"/>.
		 /// </summary>
		 /// <param name="closeables"> the closeables to close </param>
		 /// @param <T> the type of closeable </param>
		 /// <exception cref="IOException"> if an exception was thrown by one of the close methods. </exception>
		 /// <seealso cref= #closeAll(IDisposable[]) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <T extends IDisposable> void closeAll(java.util.Collection<T> closeables) throws java.io.IOException
		 public static void CloseAll<T>( ICollection<T> closeables ) where T : IDisposable
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  Close( IOException::new, closeables.toArray( new IDisposable[0] ) );
		 }

		 /// <summary>
		 /// Close all the provided <seealso cref="IDisposable closeables"/>, chaining exceptions, if any, into a single <seealso cref="UncheckedIOException"/>.
		 /// </summary>
		 /// <param name="closeables"> to call close on. </param>
		 /// @param <T> the type of closeable. </param>
		 /// <exception cref="UncheckedIOException"> if any exception is thrown from any of the {@code closeables}. </exception>
		 public static void CloseAllUnchecked<T>( ICollection<T> closeables ) where T : IDisposable
		 {
			  CloseAllUnchecked( closeables.toArray( new IDisposable[0] ) );
		 }

		 /// <summary>
		 /// Close all the provided <seealso cref="IDisposable closeables"/>, chaining exceptions, if any, into a single <seealso cref="UncheckedIOException"/>.
		 /// </summary>
		 /// <param name="closeables"> to call close on. </param>
		 /// @param <T> the type of closeable. </param>
		 /// <exception cref="UncheckedIOException"> if any exception is thrown from any of the {@code closeables}. </exception>
		 public static void CloseAllUnchecked<T>( params T[] closeables ) where T : IDisposable
		 {
			  try
			  {
					CloseAll( closeables );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 /// <summary>
		 /// Closes given <seealso cref="System.Collections.ICollection collection"/> of <seealso cref="IDisposable closeables"/> ignoring all exceptions.
		 /// </summary>
		 /// <param name="closeables"> the closeables to close </param>
		 /// @param <T> the type of closeable </param>
		 /// <seealso cref= #closeAll(IDisposable[]) </seealso>
		 public static void CloseAllSilently<T>( ICollection<T> closeables ) where T : IDisposable
		 {
			  Close( ( msg, cause ) => null, closeables.toArray( new IDisposable[0] ) );
		 }

		 /// <summary>
		 /// Closes given array of <seealso cref="IDisposable closeables"/>. If any <seealso cref="IDisposable.close()"/> call throws
		 /// <seealso cref="IOException"/> than it will be rethrown to the caller after calling <seealso cref="IDisposable.close()"/>
		 /// on other given resources. If more than one <seealso cref="IDisposable.close()"/> throw than resulting exception will
		 /// have suppressed exceptions. See <seealso cref="Exception.addSuppressed(System.Exception)"/>
		 /// </summary>
		 /// <param name="closeables"> the closeables to close </param>
		 /// @param <T> the type of closeable </param>
		 /// <exception cref="IOException"> if an exception was thrown by one of the close methods. </exception>
		 public static void CloseAll<T>( params T[] closeables ) where T : IDisposable
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  Close( IOException::new, closeables );
		 }

		 /// <summary>
		 /// Closes given array of <seealso cref="IDisposable closeables"/> ignoring all exceptions.
		 /// </summary>
		 /// <param name="closeables"> the closeables to close </param>
		 /// @param <T> the type of closeable </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static <T extends IDisposable> void closeAllSilently(T... closeables)
		 public static void CloseAllSilently<T>( params T[] closeables ) where T : IDisposable
		 {
			  Close( ( msg, cause ) => null, closeables );
		 }

		 /// <summary>
		 /// Close all ofthe given closeables, and if something goes wrong, use the given constructor to create a <seealso cref="System.Exception"/> instance with the specific cause
		 /// attached. The remaining closeables will still be closed, in that case, and if they in turn throw any exceptions then these will be attached as
		 /// suppressed exceptions.
		 /// </summary>
		 /// <param name="constructor"> The function used to construct the parent throwable that will have the first thrown exception attached as a cause, and any
		 /// remaining exceptions attached as suppressed exceptions. If this function returns {@code null}, then the exception is ignored. </param>
		 /// <param name="closeables"> an iterator of all the things to close, in order. </param>
		 /// @param <T> the type of things to close. </param>
		 /// @param <E> the type of the parent exception. </param>
		 /// <exception cref="E"> when any <seealso cref="IDisposable.close()"/> throws exception </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <T extends IDisposable, E extends Throwable> void close(System.Func<String,Throwable,E> constructor, java.util.Collection<T> closeables) throws E
		 public static void Close<T, E>( System.Func<string, Exception, E> constructor, ICollection<T> closeables ) where T : IDisposable where E : Exception
		 {
			  Close( constructor, closeables.toArray( new IDisposable[0] ) );
		 }

		 /// <summary>
		 /// Close all ofthe given closeables, and if something goes wrong, use the given constructor to create a <seealso cref="System.Exception"/> instance with the specific cause
		 /// attached. The remaining closeables will still be closed, in that case, and if they in turn throw any exceptions then these will be attached as
		 /// suppressed exceptions.
		 /// </summary>
		 /// <param name="constructor"> The function used to construct the parent throwable that will have the first thrown exception attached as a cause, and any
		 /// remaining exceptions attached as suppressed exceptions. If this function returns {@code null}, then the exception is ignored. </param>
		 /// <param name="closeables"> all the things to close, in order. </param>
		 /// @param <T> the type of things to close. </param>
		 /// @param <E> the type of the parent exception. </param>
		 /// <exception cref="E"> when any <seealso cref="IDisposable.close()"/> throws exception </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static <T extends IDisposable, E extends Throwable> void close(System.Func<String,Throwable,E> constructor, T... closeables) throws E
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void Close<T, E>( System.Func<string, Exception, E> constructor, params T[] closeables ) where T : IDisposable where E : Exception
		 {
			  E closeThrowable = null;
			  foreach ( T closeable in closeables )
			  {
					try
					{
						 if ( closeable != null )
						 {
							  closeable.close();
						 }
					}
					catch ( Exception e )
					{
						 if ( closeThrowable == null )
						 {
							  closeThrowable = constructor( "Exception closing multiple resources.", e );
						 }
						 else
						 {
							  closeThrowable.addSuppressed( e );
						 }
					}
			  }
			  if ( closeThrowable != null )
			  {
					throw closeThrowable;
			  }
		 }
	}

}
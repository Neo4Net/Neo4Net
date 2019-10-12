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
namespace Neo4Net.Util.concurrent
{

	/// <summary>
	/// Constructors for basic <seealso cref="Future"/> types
	/// </summary>
	public class Futures
	{
		 private Futures()
		 {
		 }

		 /// <summary>
		 /// Combine multiple @{link Future} instances into a single Future
		 /// </summary>
		 /// <param name="futures"> the @{link Future} instances to combine </param>
		 /// @param <V>     The result type returned by this Future's get method </param>
		 /// <returns> A new @{link Future} representing the combination </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static <V> java.util.concurrent.Future<java.util.List<V>> combine(final java.util.concurrent.Future<? extends V>... futures)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Future<IList<V>> Combine<V>( params Future<V>[] futures )
		 {
			  return Combine( Arrays.asList( futures ) );
		 }

		 /// <summary>
		 /// Combine multiple @{link Future} instances into a single Future
		 /// </summary>
		 /// <param name="futures"> the @{link Future} instances to combine </param>
		 /// @param <V>     The result type returned by this Future's get method </param>
		 /// <returns> A new @{link Future} representing the combination </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <V> java.util.concurrent.Future<java.util.List<V>> combine(final Iterable<? extends java.util.concurrent.Future<? extends V>> futures)
		 public static Future<IList<V>> Combine<V, T1>( IEnumerable<T1> futures ) where T1 : java.util.concurrent.Future<T1 extends V>
		 {
			  return new FutureAnonymousInnerClass( futures );
		 }

		 private class FutureAnonymousInnerClass : Future<IList<V>>
		 {
			 private IEnumerable<T1> _futures;

			 public FutureAnonymousInnerClass( IEnumerable<T1> futures )
			 {
				 this._futures = futures;
			 }

			 public override bool cancel( bool mayInterruptIfRunning )
			 {
				  bool result = false;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.concurrent.Future<? extends V> future : futures)
				  foreach ( Future<V> future in _futures )
				  {
						result |= future.cancel( mayInterruptIfRunning );
				  }
				  return result;
			 }

			 public override bool Cancelled
			 {
				 get
				 {
					  bool result = false;
	//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
	//ORIGINAL LINE: for (java.util.concurrent.Future<? extends V> future : futures)
					  foreach ( Future<V> future in _futures )
					  {
							result |= future.Cancelled;
					  }
					  return result;
				 }
			 }

			 public override bool Done
			 {
				 get
				 {
					  bool result = false;
	//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
	//ORIGINAL LINE: for (java.util.concurrent.Future<? extends V> future : futures)
					  foreach ( Future<V> future in _futures )
					  {
							result |= future.Done;
					  }
					  return result;
				 }
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.List<V> get() throws InterruptedException, java.util.concurrent.ExecutionException
			 public override IList<V> get()
			 {
				  IList<V> result = new List<V>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.concurrent.Future<? extends V> future : futures)
				  foreach ( Future<V> future in _futures )
				  {
						result.Add( future.get() );
				  }
				  return result;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.List<V> get(long timeout, java.util.concurrent.TimeUnit unit) throws InterruptedException, java.util.concurrent.ExecutionException, java.util.concurrent.TimeoutException
			 public override IList<V> get( long timeout, TimeUnit unit )
			 {
				  IList<V> result = new List<V>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.concurrent.Future<? extends V> future : futures)
				  foreach ( Future<V> future in _futures )
				  {
						long before = System.nanoTime();
						result.Add( future.get( timeout, unit ) );
						timeout -= unit.convert( System.nanoTime() - before, TimeUnit.NANOSECONDS );
				  }
				  return result;
			 }
		 }

		 /// <summary>
		 /// Returns a exceptionally completed @{link CompletableFuture} instance
		 /// </summary>
		 /// <param name="ex"> the @{link Throwable} that would be set on the future </param>
		 /// @param <T> the result type returned by this Future's get method </param>
		 /// <returns> An exceptionally completed @{link CompletableFuture} with the given exception </returns>
		 public static CompletableFuture<T> FailedFuture<T>( Exception ex )
		 {
			  CompletableFuture<T> future = new CompletableFuture<T>();
			  future.completeExceptionally( ex );
			  return future;
		 }
	}

}
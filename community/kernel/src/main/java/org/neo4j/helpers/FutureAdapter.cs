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
namespace Org.Neo4j.Helpers
{

	[Obsolete]
	public abstract class FutureAdapter<V> : Future<V>
	{
		 public static readonly Future<Void> Void = CompletableFuture.completedFuture( null );

		 public override bool Cancel( bool mayInterruptIfRunning )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override bool Cancelled
		 {
			 get
			 {
				  throw new System.NotSupportedException();
			 }
		 }

		 /// <summary>
		 /// This class will be deleted as part of next major release. Please use <seealso cref="CompletableFuture.complete(object)"/>
		 /// instead.
		 /// </summary>
		 [Obsolete]
		 public class Present<V> : FutureAdapter<V>
		 {
			  internal readonly V Value;

			  public Present( V value )
			  {
					this.Value = value;
			  }

			  public override bool Done
			  {
				  get
				  {
						return true;
				  }
			  }

			  public override V Get()
			  {
					return Value;
			  }
			  public override V Get( long timeout, TimeUnit unit )
			  {
					return Value;
			  }
		 }

		 /// @param <T> type of values that this <seealso cref="Future"/> have. </param>
		 /// <param name="value"> result value. </param>
		 /// <returns> <seealso cref="Present"/> future with already specified result
		 /// 
		 /// This method will be deleted as part of next major release. Please use <seealso cref="CompletableFuture.complete(object)"/>
		 /// instead. </returns>
		 [Obsolete]
		 public static Present<T> Present<T>( T value )
		 {
			  return new Present<T>( value );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> java.util.concurrent.Future<T> latchGuardedValue(final System.Func<T> supplier, final java.util.concurrent.CountDownLatch guardedByLatch, final String jobDescription)
		 [Obsolete]
		 public static Future<T> LatchGuardedValue<T>( System.Func<T> supplier, System.Threading.CountdownEvent guardedByLatch, string jobDescription )
		 {
			  return new FutureAdapterAnonymousInnerClass( supplier, guardedByLatch, jobDescription );
		 }

		 private class FutureAdapterAnonymousInnerClass : FutureAdapter<T>
		 {
			 private System.Func<T> _supplier;
			 private System.Threading.CountdownEvent _guardedByLatch;
			 private string _jobDescription;

			 public FutureAdapterAnonymousInnerClass( System.Func<T> supplier, System.Threading.CountdownEvent guardedByLatch, string jobDescription )
			 {
				 this._supplier = supplier;
				 this._guardedByLatch = guardedByLatch;
				 this._jobDescription = jobDescription;
			 }

			 public override bool Done
			 {
				 get
				 {
					  return _guardedByLatch.CurrentCount == 0;
				 }
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public T get() throws InterruptedException
			 public override T get()
			 {
				  _guardedByLatch.await();
				  return _supplier();
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public T get(long timeout, java.util.concurrent.TimeUnit unit) throws InterruptedException, java.util.concurrent.TimeoutException
			 public override T get( long timeout, TimeUnit unit )
			 {
				  if ( !_guardedByLatch.await( timeout, unit ) )
				  {
						throw new TimeoutException( _jobDescription + " didn't complete within " + timeout + " " + unit );
				  }
				  return _supplier();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static java.util.concurrent.Future<int> processFuture(final Process process)
		 [Obsolete]
		 public static Future<int> ProcessFuture( Process process )
		 {
			  return new FutureAdapterAnonymousInnerClass2( process );
		 }

		 private class FutureAdapterAnonymousInnerClass2 : FutureAdapter<int>
		 {
			 private Process _process;

			 public FutureAdapterAnonymousInnerClass2( Process process )
			 {
				 this._process = process;
			 }

			 public override bool Done
			 {
				 get
				 {
					  return tryGetExitValue( _process ) != null;
				 }
			 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private System.Nullable<int> tryGetExitValue(final Process process)
			 private int? tryGetExitValue( Process process )
			 {
				  try
				  {
						return process.exitValue();
				  }
				  catch ( IllegalThreadStateException )
				  { // Thrown if this process hasn't exited yet.
						return null;
				  }
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<int> get() throws InterruptedException
			 public override int? get()
			 {
				  return _process.waitFor();
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<int> get(long timeout, java.util.concurrent.TimeUnit unit) throws InterruptedException, java.util.concurrent.TimeoutException
			 public override int? get( long timeout, TimeUnit unit )
			 {
				  long end = DateTimeHelper.CurrentUnixTimeMillis() + unit.toMillis(timeout);
				  while ( DateTimeHelper.CurrentUnixTimeMillis() < end )
				  {
						int? result = tryGetExitValue( _process );
						if ( result != null )
						{
							 return result;
						}
						Thread.Sleep( 10 );
				  }
				  throw new TimeoutException( "Process '" + _process + "' didn't exit within " + timeout + " " + unit );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> java.util.concurrent.Future<T> future(final java.util.concurrent.Callable<T> task)
		 [Obsolete]
		 public static Future<T> Future<T>( Callable<T> task )
		 {
			  ExecutorService executor = Executors.newSingleThreadExecutor();
			  Future<T> future = executor.submit( task );
			  executor.shutdown();
			  return future;
		 }
	}

}
using System;
using System.Collections.Generic;
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
namespace Neo4Net.Test.rule.concurrent
{
	using ExternalResource = org.junit.rules.ExternalResource;


	using Neo4Net.Functions;
	using Predicates = Neo4Net.Functions.Predicates;
	using Neo4Net.Functions;
	using Neo4Net.Functions;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.function.ThrowingPredicate.throwingPredicate;

	public class ThreadingRule : ExternalResource
	{
		 private ExecutorService _executor;
		 private static readonly FailableConsumer<Thread> NULL_CONSUMER = new FailableConsumerAnonymousInnerClass();

		 private class FailableConsumerAnonymousInnerClass : FailableConsumer<Thread>
		 {
			 public void fail( Exception failure )
			 {
			 }

			 public void accept( Thread thread )
			 {
			 }
		 }

		 protected internal override void Before()
		 {
			  _executor = Executors.newCachedThreadPool();
		 }

		 protected internal override void After()
		 {
			  try
			  {
					_executor.shutdownNow();
					_executor.awaitTermination( 1, TimeUnit.MINUTES );
			  }
			  catch ( InterruptedException e )
			  {
					Console.WriteLine( e.ToString() );
					Console.Write( e.StackTrace );
			  }
			  finally
			  {
					_executor = null;
			  }
		 }

		 public virtual Future<TO> Execute<FROM, TO, EX>( ThrowingFunction<FROM, TO, EX> function, FROM parameter ) where EX : Exception
		 {
			  return _executor.submit( Task( function, function.ToString(), parameter, NULL_CONSUMER ) );
		 }

		 public virtual IList<Future<TO>> Multiple<FROM, TO, EX>( int threads, ThrowingFunction<FROM, TO, EX> function, FROM parameter ) where EX : Exception
		 {
			  IList<Future<TO>> result = new List<Future<TO>>( threads );
			  for ( int i = 0; i < threads; i++ )
			  {
					result.Add( _executor.submit( Task( function, function.ToString() + ":task=" + i, parameter, NULL_CONSUMER ) ) );
			  }
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <T> java.util.List<T> await(Iterable<java.util.concurrent.Future<T>> futures) throws InterruptedException, java.util.concurrent.ExecutionException
		 public static IList<T> Await<T>( IEnumerable<Future<T>> futures )
		 {
			  IList<T> result = futures is System.Collections.ICollection ? new List<T>( ( ( System.Collections.ICollection ) futures ).Count ) : new List<T>();
			  IList<Exception> failures = null;
			  foreach ( Future<T> future in futures )
			  {
					try
					{
						 result.Add( future.get() );
					}
					catch ( ExecutionException e )
					{
						 if ( failures == null )
						 {
							  failures = new List<Exception>();
						 }
						 failures.Add( e.InnerException );
					}
			  }
			  if ( failures != null )
			  {
					if ( failures.Count == 1 )
					{
						 throw new ExecutionException( failures[0] );
					}
					ExecutionException exception = new ExecutionException( null );
					foreach ( Exception failure in failures )
					{
						 exception.addSuppressed( failure );
					}
					throw exception;
			  }
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <FROM, TO, EX extends Exception> java.util.concurrent.Future<TO> executeAndAwait(org.Neo4Net.function.ThrowingFunction<FROM,TO,EX> function, FROM parameter, System.Predicate<Thread> threadCondition, long timeout, java.util.concurrent.TimeUnit unit) throws java.util.concurrent.ExecutionException
		 public virtual Future<TO> ExecuteAndAwait<FROM, TO, EX>( ThrowingFunction<FROM, TO, EX> function, FROM parameter, System.Predicate<Thread> threadCondition, long timeout, TimeUnit unit ) where EX : Exception
		 {
			  FailableConcurrentTransfer<Thread> transfer = new FailableConcurrentTransfer<Thread>();
			  Future<TO> future = _executor.submit( Task( function, function.ToString(), parameter, transfer ) );
			  try
			  {
					Predicates.awaitEx( transfer, throwingPredicate( threadCondition ), timeout, unit );
			  }
			  catch ( Exception e )
			  {
					throw new ExecutionException( e );
			  }
			  return future;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static <FROM, TO, EX extends Exception> java.util.concurrent.Callable<TO> task(final org.Neo4Net.function.ThrowingFunction<FROM,TO,EX> function, String name, final FROM parameter, final org.Neo4Net.function.FailableConsumer<Thread> threadConsumer)
		 private static Callable<TO> Task<FROM, TO, EX>( ThrowingFunction<FROM, TO, EX> function, string name, FROM parameter, FailableConsumer<Thread> threadConsumer ) where EX : Exception
		 {
			  return () =>
			  {
				Thread thread = Thread.CurrentThread;
				string previousName = thread.Name;
				thread.Name = name;
				threadConsumer.accept( thread );
				try
				{
					 return function.Apply( parameter );
				}
				catch ( Exception failure )
				{
					 threadConsumer.Fail( failure );
					 throw failure;
				}
				finally
				{
					 thread.Name = previousName;
				}
			  };
		 }

		 /*Sample Stacktrace for method on owner*/
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Predicate<Thread> waitingWhileIn(final Class owner, final String method)
		 public static System.Predicate<Thread> WaitingWhileIn( Type owner, string method )
		 {
			  return new PredicateAnonymousInnerClass( owner, method );
		 }

		 private class PredicateAnonymousInnerClass : System.Predicate<Thread>
		 {
			 private Type _owner;
			 private string _method;

			 public PredicateAnonymousInnerClass( Type owner, string method )
			 {
				 this._owner = owner;
				 this._method = method;
			 }

			 public override bool test( Thread thread )
			 {
				  ReflectionUtil.verifyMethodExists( _owner, _method );

				  if ( thread.State == Thread.State.WAITING || thread.State == Thread.State.TIMED_WAITING )
				  {
						foreach ( StackTraceElement element in thread.StackTrace )
						{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
							 if ( element.ClassName.Equals( _owner.FullName ) && element.MethodName.Equals( _method ) )
							 {
								  return true;
							 }
						}
				  }
				  return false;
			 }

			 public override string ToString()
			 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				  return string.Format( "Predicate[Thread.state=WAITING && thread.getStackTrace() contains {0}.{1}()]", _owner.FullName, _method );
			 }
		 }

		 private class FailableConcurrentTransfer<TYPE> : FailableConsumer<TYPE>, ThrowingSupplier<TYPE, Exception>
		 {
			  internal readonly System.Threading.CountdownEvent Latch = new System.Threading.CountdownEvent( 1 );
			  internal TYPE Value;
			  internal Exception Failure;

			  public override void Accept( TYPE value )
			  {
					this.Value = value;
					Latch.Signal();
			  }

			  public override void Fail( Exception failure )
			  {
					this.Failure = failure;
					Latch.Signal();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TYPE get() throws Exception
			  public override TYPE Get()
			  {
					Latch.await();
					if ( Failure != null )
					{
						 throw Failure;
					}
					return Value;
			  }

			  public override string ToString()
			  {
					return string.Format( "ConcurrentTransfer{{{0}}}", Latch.CurrentCount == 1 ? "<waiting>" : Value );
			  }
		 }
	}

}
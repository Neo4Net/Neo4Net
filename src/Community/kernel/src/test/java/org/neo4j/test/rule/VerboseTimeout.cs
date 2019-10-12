using System;
using System.Collections.Generic;
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
namespace Neo4Net.Test.rule
{
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using Timeout = org.junit.rules.Timeout;
	using Description = org.junit.runner.Description;
	using Statement = org.junit.runners.model.Statement;
	using TestTimedOutException = org.junit.runners.model.TestTimedOutException;


	using DumpUtils = Neo4Net.Diagnostics.utils.DumpUtils;

	/// <summary>
	/// Timeout rule implementation that print out stack traces of all threads
	/// instead of just one suspect, as default implementation does.
	/// <para>
	/// In addition provide possibility to describe provided custom entities on timeout failure.
	/// Object description can be customized by provided function, by default - toString() method will be used.
	/// </para>
	/// <para>
	/// For example:
	/// <pre> {@code
	/// public VerboseTimeout timeout = VerboseTimeout.builder()
	///                                               .withTimeout( 50, TimeUnit.SECONDS )
	///                                               .describeOnFailure( locks )
	///                                               .build()};
	/// </pre>
	/// Another way to use this is to not provide a specific timeout and let normal means of specifying junit test timeout take its course, e.g:
	/// <pre> {@code
	/// public VerboseTimeout timeout = VerboseTimeout.builder().build();
	///  ...
	/// @literal @Test( timeout = 20_000 )
	///  public void shouldTestSomething()}
	/// </pre>
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= Timeout </seealso>
	public class VerboseTimeout : Timeout
	{
		 private VerboseTimeoutBuilder _timeoutBuilder;
		 private Description _currentTestDescription;

		 private VerboseTimeout( VerboseTimeoutBuilder timeoutBuilder ) : base( timeoutBuilder )
		 {
			  this._timeoutBuilder = timeoutBuilder;
		 }

		 public static VerboseTimeoutBuilder Builder()
		 {
			  return new VerboseTimeoutBuilder();
		 }

		 public override Statement Apply( Statement @base, Description description )
		 {
			  // Just pick up on which test we're currently running
			  _currentTestDescription = description;
			  return base.Apply( @base, description );
		 }

		 protected internal override Statement CreateFailOnTimeoutStatement( Statement statement )
		 {
			  return new VerboseFailOnTimeout( this, statement, _timeoutBuilder );
		 }

		 /// <summary>
		 /// Helper builder class of <seealso cref="VerboseTimeout"/> test rule.
		 /// </summary>
		 public class VerboseTimeoutBuilder : Timeout.Builder
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal TimeUnit TimeUnitConflict = TimeUnit.SECONDS;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long TimeoutConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.List<FailureParameter<?>> additionalParameters = new java.util.ArrayList<>();
			  internal IList<FailureParameter<object>> AdditionalParametersConflict = new List<FailureParameter<object>>();

			  internal static System.Func<object, string> ToStringFunction()
			  {
					return value => value == null ? StringUtils.EMPTY : value.ToString();
			  }

			  public override VerboseTimeoutBuilder WithTimeout( long timeout, TimeUnit unit )
			  {
					this.TimeoutConflict = timeout;
					this.TimeUnitConflict = unit;
					return this;
			  }

			  public virtual VerboseTimeoutBuilder DescribeOnFailure<T>( T entity, System.Func<T, string> descriptor )
			  {
					AdditionalParametersConflict.Add( new FailureParameter<>( this, entity, descriptor ) );
					return this;
			  }

			  public virtual VerboseTimeoutBuilder DescribeOnFailure<T>( T entity )
			  {
					return DescribeOnFailure( entity, ToStringFunction() );
			  }

			  public override VerboseTimeout Build()
			  {
					return new VerboseTimeout( this );
			  }

			  protected internal override long Timeout
			  {
				  get
				  {
						return TimeoutConflict;
				  }
			  }

			  protected internal override TimeUnit TimeUnit
			  {
				  get
				  {
						return TimeUnitConflict;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<FailureParameter<?>> getAdditionalParameters()
			  internal virtual IList<FailureParameter<object>> AdditionalParameters
			  {
				  get
				  {
						return AdditionalParametersConflict;
				  }
			  }

			  private class FailureParameter<T>
			  {
				  private readonly VerboseTimeout.VerboseTimeoutBuilder _outerInstance;

					internal readonly T Entity;
					internal readonly System.Func<T, string> Descriptor;

					internal FailureParameter( VerboseTimeout.VerboseTimeoutBuilder outerInstance, T entity, System.Func<T, string> descriptor )
					{
						this._outerInstance = outerInstance;
						 this.Entity = entity;
						 this.Descriptor = descriptor;
					}

					internal virtual string Describe()
					{
						 return Descriptor.apply( Entity );
					}
			  }
		 }

		 /// <summary>
		 /// Statement that in case of timeout, unlike junit <seealso cref="org.junit.internal.runners.statements.FailOnTimeout"/>
		 /// will print thread dumps of all threads in JVM, that should help in investigation of stuck threads.
		 /// </summary>
		 private class VerboseFailOnTimeout : Statement
		 {
			 private readonly VerboseTimeout _outerInstance;

			  internal readonly Statement OriginalStatement;
			  internal readonly TimeUnit TimeUnit;
			  internal readonly long Timeout;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.List<VerboseTimeoutBuilder.FailureParameter<?>> additionalParameters;
			  internal readonly IList<VerboseTimeoutBuilder.FailureParameter<object>> AdditionalParameters;

			  internal VerboseFailOnTimeout( VerboseTimeout outerInstance, Statement statement, VerboseTimeoutBuilder builder )
			  {
				  this._outerInstance = outerInstance;
					OriginalStatement = statement;
					Timeout = builder.TimeoutConflict;
					TimeUnit = builder.TimeUnit;
					AdditionalParameters = builder.AdditionalParameters;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void evaluate() throws Throwable
			  public override void Evaluate()
			  {
					CallableStatement callable = new CallableStatement( this );
					FutureTask<Exception> task = new FutureTask<Exception>( callable );
					Thread thread = new Thread( task, "Time-limited test" );
					thread.Daemon = true;
					thread.Start();
					callable.AwaitStarted();
					Exception throwable = GetResult( task, thread );
					if ( throwable != null )
					{
						 throw throwable;
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Throwable getResult(java.util.concurrent.FutureTask<Throwable> task, Thread thread) throws Throwable
			  internal virtual Exception GetResult( FutureTask<Exception> task, Thread thread )
			  {
					try
					{
						 Exception potentialException = Timeout > 0 ? task.get( Timeout, TimeUnit ) : task.get();
						 if ( potentialException is TestTimedOutException )
						 {
							  // This allows this rule to be used without a specific timeout and instead use whatever
							  // timeout the test annotation specifies itself.
							  PrintThreadDump();
						 }
						 return potentialException;
					}
					catch ( ExecutionException e )
					{
						 PrintThreadDump();
						 return e.InnerException;
					}
					catch ( TimeoutException )
					{
						 PrintThreadDump();
						 return BuildTimeoutException( thread );
					}
			  }

			  internal virtual Exception BuildTimeoutException( Thread thread )
			  {
					StackTraceElement[] stackTrace = thread.StackTrace;
					TestTimedOutException timedOutException = new TestTimedOutException( Timeout, TimeUnit );
					timedOutException.StackTrace = stackTrace;
					return timedOutException;
			  }

			  private class CallableStatement : Callable<Exception>
			  {
				  private readonly VerboseTimeout.VerboseFailOnTimeout _outerInstance;

				  public CallableStatement( VerboseTimeout.VerboseFailOnTimeout outerInstance )
				  {
					  this._outerInstance = outerInstance;
				  }

					internal readonly System.Threading.CountdownEvent StartLatch = new System.Threading.CountdownEvent( 1 );

					public override Exception Call()
					{
						 try
						 {
							  StartLatch.Signal();
							  outerInstance.OriginalStatement.evaluate();
						 }
						 catch ( Exception e )
						 {
							  return e;
						 }
						 return null;
					}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void awaitStarted() throws InterruptedException
					internal virtual void AwaitStarted()
					{
						 StartLatch.await();
					}
			  }

			  internal virtual void PrintThreadDump()
			  {
					Console.Error.WriteLine( string.Format( "=== Test {0} timed out, dumping more information ===", outerInstance.currentTestDescription.DisplayName ) );
					if ( AdditionalParameters.Count > 0 )
					{
						 Console.Error.WriteLine( "=== Requested additional parameters: ===" );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (VerboseTimeoutBuilder.FailureParameter<?> additionalParameter : additionalParameters)
						 foreach ( VerboseTimeoutBuilder.FailureParameter<object> additionalParameter in AdditionalParameters )
						 {
							  Console.Error.WriteLine( additionalParameter.Describe() );
						 }
					}
					Console.Error.WriteLine( "=== Thread dump ===" );
					Console.Error.WriteLine( DumpUtils.threadDump() );
			  }
		 }
	}

}
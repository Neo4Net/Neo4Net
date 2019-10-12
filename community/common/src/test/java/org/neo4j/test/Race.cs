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
namespace Org.Neo4j.Test
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;

	/// <summary>
	/// Simple race scenario, a utility for executing multiple threads coordinated to start at the same time.
	/// Add contestants with <seealso cref="addContestant(System.Threading.ThreadStart)"/> and then when all have been added, start them
	/// simultaneously using <seealso cref="go()"/>, which will block until all contestants have completed.
	/// Any errors from contestants are propagated out from <seealso cref="go()"/>.
	/// </summary>
	public class Race
	{
		 private const int UNLIMITED = 0;

		 public interface ThrowingRunnable
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void run() throws Throwable;
			  void Run();
		 }

		 private readonly IList<Contestant> _contestants = new List<Contestant>();
		 private volatile System.Threading.CountdownEvent _readySet;
		 private readonly System.Threading.CountdownEvent _go = new System.Threading.CountdownEvent( 1 );
		 private volatile bool _addSomeMinorRandomStartDelays;
		 private volatile System.Func<bool> _endCondition;
		 private volatile bool _failure;
		 private bool _asyncExecution;

		 public virtual Race WithRandomStartDelays()
		 {
			  this._addSomeMinorRandomStartDelays = true;
			  return this;
		 }

		 /// <summary>
		 /// Adds an end condition to this race. The race will end whenever an end condition is met
		 /// or when there's one contestant failing (throwing any sort of exception).
		 /// </summary>
		 /// <param name="endConditions"> one or more end conditions, such that when returning {@code true}
		 /// signals that the race should end. </param>
		 /// <returns> this <seealso cref="Race"/> instance. </returns>
		 public virtual Race WithEndCondition( params System.Func<bool>[] endConditions )
		 {
			  foreach ( System.Func<bool> endCondition in endConditions )
			  {
					this._endCondition = MergeEndCondition( endCondition );
			  }
			  return this;
		 }

		 /// <summary>
		 /// Convenience for adding an end condition which is based on time. This will have contestants
		 /// end after the given duration (time + unit).
		 /// </summary>
		 /// <param name="time"> time value. </param>
		 /// <param name="unit"> unit of time in <seealso cref="TimeUnit"/>. </param>
		 /// <returns> this <seealso cref="Race"/> instance. </returns>
		 public virtual Race WithMaxDuration( long time, TimeUnit unit )
		 {
			  long endTime = currentTimeMillis() + unit.toMillis(time);
			  this._endCondition = MergeEndCondition( () => currentTimeMillis() >= endTime );
			  return this;
		 }

		 private System.Func<bool> MergeEndCondition( System.Func<bool> additionalEndCondition )
		 {
			  System.Func<bool> existingEndCondition = _endCondition;
			  return existingEndCondition == null ? additionalEndCondition : () => existingEndCondition() || additionalEndCondition();
		 }

		 /// <summary>
		 /// Convenience for wrapping contestants, especially for lambdas, which throws any sort of
		 /// checked exception.
		 /// </summary>
		 /// <param name="runnable"> actual contestant. </param>
		 /// <returns> contestant wrapped in a try-catch (and re-throw as unchecked exception). </returns>
		 public static ThreadStart Throwing( ThrowingRunnable runnable )
		 {
			  return () =>
			  {
				try
				{
					 runnable.Run();
				}
				catch ( Exception e )
				{
					 throw new Exception( e );
				}
			  };
		 }

		 public virtual void AddContestants( int count, ThreadStart contestant )
		 {
			  AddContestants( count, contestant, UNLIMITED );
		 }

		 public virtual void AddContestants( int count, ThreadStart contestant, int maxNumberOfRuns )
		 {
			  for ( int i = 0; i < count; i++ )
			  {
					AddContestant( contestant, maxNumberOfRuns );
			  }
		 }

		 public virtual void AddContestant( ThreadStart contestant )
		 {
			  AddContestant( contestant, UNLIMITED );
		 }

		 public virtual void AddContestant( ThreadStart contestant, int maxNumberOfRuns )
		 {
			  _contestants.Add( new Contestant( this, contestant, _contestants.Count, maxNumberOfRuns ) );
		 }

		 /// <summary>
		 /// Starts the race and returns without waiting for contestants to complete.
		 /// Any exception thrown by contestant will be lost.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void goAsync() throws Throwable
		 public virtual void GoAsync()
		 {
			  _asyncExecution = true;
			  Go( 0, TimeUnit.MILLISECONDS );
		 }

		 /// <summary>
		 /// Starts the race and waits indefinitely for all contestants to either fail or succeed.
		 /// </summary>
		 /// <exception cref="Throwable"> on any exception thrown from any contestant. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void go() throws Throwable
		 public virtual void Go()
		 {
			  Go( 0, TimeUnit.MILLISECONDS );
		 }

		 /// <summary>
		 /// Starts the race and waits {@code maxWaitTime} for all contestants to either fail or succeed.
		 /// </summary>
		 /// <param name="maxWaitTime"> max time to wait for all contestants, 0 means indefinite wait. </param>
		 /// <param name="unit"> <seealso cref="TimeUnit"/> that {£{@code maxWaitTime} is given in. </param>
		 /// <exception cref="TimeoutException"> if all contestants haven't either succeeded or failed within the given time. </exception>
		 /// <exception cref="Throwable"> on any exception thrown from any contestant. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void go(long maxWaitTime, java.util.concurrent.TimeUnit unit) throws Throwable
		 public virtual void Go( long maxWaitTime, TimeUnit unit )
		 {
			  if ( _endCondition == null )
			  {
					_endCondition = () => true;
			  }

			  _readySet = new System.Threading.CountdownEvent( _contestants.Count );
			  foreach ( Contestant contestant in _contestants )
			  {
					contestant.Start();
			  }
			  _readySet.await();
			  _go.Signal();

			  if ( _asyncExecution )
			  {
					return;
			  }

			  int errorCount = 0;
			  long maxWaitTimeMillis = MILLISECONDS.convert( maxWaitTime, unit );
			  long waitedSoFar = 0;
			  foreach ( Contestant contestant in _contestants )
			  {
					if ( maxWaitTime == 0 )
					{
						 contestant.Join();
					}
					else
					{
						 long time = currentTimeMillis();
						 contestant.Join( maxWaitTimeMillis - waitedSoFar );
						 waitedSoFar += currentTimeMillis() - time;
						 if ( waitedSoFar >= maxWaitTimeMillis )
						 {
							  throw new TimeoutException( "Didn't complete after " + maxWaitTime + " " + unit );
						 }
					}
					if ( contestant.Error != null )
					{
						 errorCount++;
					}
			  }

			  if ( errorCount > 1 )
			  {
					Exception errors = new Exception( "Multiple errors found" );
					foreach ( Contestant contestant in _contestants )
					{
						 if ( contestant.Error != null )
						 {
							  errors.addSuppressed( contestant.Error );
						 }
					}
					throw errors;
			  }
			  if ( errorCount == 1 )
			  {
					foreach ( Contestant contestant in _contestants )
					{
						 if ( contestant.Error != null )
						 {
							  throw contestant.Error;
						 }
					}
			  }
		 }

		 private class Contestant : Thread
		 {
			 private readonly Race _outerInstance;

			  internal volatile Exception Error;
			  internal readonly int MaxNumberOfRuns;
			  internal int Runs;

			  internal Contestant( Race outerInstance, ThreadStart code, int nr, int maxNumberOfRuns ) : base( code, "Contestant#" + nr )
			  {
				  this._outerInstance = outerInstance;
					this.MaxNumberOfRuns = maxNumberOfRuns;
					this.UncaughtExceptionHandler = ( thread, Error ) =>
					{
					};
			  }

			  public override void Run()
			  {
					outerInstance.readySet.Signal();
					try
					{
						 outerInstance.go.await();
					}
					catch ( InterruptedException e )
					{
						 Error = e;
						 interrupt();
						 return;
					}

					if ( outerInstance.addSomeMinorRandomStartDelays )
					{
						 RandomlyDelaySlightly();
					}

					try
					{
						 while ( !outerInstance.failure )
						 {
							  base.Run();
							  if ( ( MaxNumberOfRuns != UNLIMITED && ++Runs == MaxNumberOfRuns ) || outerInstance.endCondition.AsBoolean )
							  {
									break;
							  }
						 }
					}
					catch ( Exception e )
					{
						 Error = e;
						 outerInstance.failure = true; // <-- global flag
						 throw e;
					}
			  }

			  internal virtual void RandomlyDelaySlightly()
			  {
					int millis = ThreadLocalRandom.current().Next(100);
					LockSupport.parkNanos( TimeUnit.MILLISECONDS.toNanos( 10 + millis ) );
			  }
		 }
	}

}
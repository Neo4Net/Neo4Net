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
	using TestRule = org.junit.rules.TestRule;
	using Description = org.junit.runner.Description;
	using MultipleFailureException = org.junit.runners.model.MultipleFailureException;
	using Statement = org.junit.runners.model.Statement;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;

	public class ThreadRepository : TestRule
	{
		 public interface Task
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void perform() throws Exception;
			  void Perform();
		 }

		 public interface ThreadInfo
		 {
			  StackTraceElement[] StackTrace { get; }

			  object Blocker();

			  Thread.State State { get; }
		 }

		 private Repository _repository;
		 private readonly long _timeout;
		 private readonly TimeUnit _unit;

		 public ThreadRepository( long timeout, TimeUnit unit )
		 {
			  this._timeout = timeout;
			  this._unit = unit;
		 }

		 public virtual ThreadInfo Execute( params Task[] tasks )
		 {
			  return _repository.createThread( null, tasks );
		 }

		 public virtual ThreadInfo Execute( string name, params Task[] tasks )
		 {
			  return _repository.createThread( name, tasks );
		 }

		 public virtual Signal Signal()
		 {
			  return new Signal( new System.Threading.CountdownEvent( 1 ) );
		 }

		 public virtual Await Await()
		 {
			  return Await( 1 );
		 }

		 public virtual Await Await( int events )
		 {
			  return new Await( new System.Threading.CountdownEvent( events ) );
		 }

		 public virtual Events Events()
		 {
			  return new Events( this );
		 }

		 public class Signal : Task
		 {
			  internal readonly System.Threading.CountdownEvent Latch;

			  internal Signal( System.Threading.CountdownEvent latch )
			  {
					this.Latch = latch;
			  }

			  public virtual Await Await()
			  {
					return new Await( Latch );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void awaitNow() throws InterruptedException
			  public virtual void AwaitNow()
			  {
					Latch.await();
			  }

			  public override void Perform()
			  {
					Latch.Signal();
			  }
		 }

		 public class Await : Task
		 {
			  internal readonly System.Threading.CountdownEvent Latch;

			  internal Await( System.Threading.CountdownEvent latch )
			  {
					this.Latch = latch;
			  }

			  public virtual Signal Signal()
			  {
					return new Signal( Latch );
			  }

			  public virtual void Release()
			  {
					Latch.Signal();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void perform() throws Exception
			  public override void Perform()
			  {
					Latch.await();
			  }
		 }

		 public class Events
		 {
			 private readonly ThreadRepository _outerInstance;

			  internal readonly IList<string> Collected;

			  internal Events( ThreadRepository outerInstance )
			  {
				  this._outerInstance = outerInstance;
					Collected = new CopyOnWriteArrayList<string>();
			  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Task trigger(final String event)
			  public virtual Task Trigger( string @event )
			  {
					return () => Collected.Add(@event);
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void assertInOrder(String... events) throws Exception
			  public virtual void AssertInOrder( params string[] events )
			  {
					try
					{
						 outerInstance.completeThreads();
					}
					catch ( Exception ok ) when ( ok is Exception || ok is Exception )
					{
						 throw ok;
					}
					catch ( Exception throwable )
					{
						 throw new Exception( "Unexpected Throwable", throwable );
					}
					string[] actual = Collected.ToArray();
					assertArrayEquals( events, actual );
			  }

			  public virtual IList<string> Snapshot()
			  {
					return new List<string>( Collected );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.junit.runners.model.Statement apply(final org.junit.runners.model.Statement super, final org.junit.runner.Description description)
		 public override Statement Apply( Statement @base, Description description )
		 {
			  return new StatementAnonymousInnerClass( this, @base, description );
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private readonly ThreadRepository _outerInstance;

			 private Statement @base;
			 private Description _description;

			 public StatementAnonymousInnerClass( ThreadRepository outerInstance, Statement @base, Description description )
			 {
				 this.outerInstance = outerInstance;
				 this.@base = @base;
				 this._description = description;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void Evaluate() throws Throwable
			 public override void Evaluate()
			 {
				  _outerInstance.repository = new Repository( _outerInstance, _description );
				  IList<Exception> failures = new List<Exception>();
				  try
				  {
						@base.Evaluate();
				  }
				  catch ( Exception failure )
				  {
						failures.Add( failure );
				  }
				  finally
				  {
						outerInstance.completeThreads( failures );
				  }
				  MultipleFailureException.assertEmpty( failures );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void completeThreads() throws Throwable
		 private void CompleteThreads()
		 {
			  IList<Exception> failures = new List<Exception>();
			  CompleteThreads( failures );
			  MultipleFailureException.assertEmpty( failures );
		 }

		 private void CompleteThreads( IList<Exception> failures )
		 {
			  if ( _repository != null )
			  {
					_repository.completeAll( failures );
			  }
			  _repository = null;
		 }

		 private class Repository
		 {
			 private readonly ThreadRepository _outerInstance;

			  internal readonly Description Description;
			  internal int I;
			  internal readonly IList<TaskThread> Threads = new List<TaskThread>();

			  internal Repository( ThreadRepository outerInstance, Description description )
			  {
				  this._outerInstance = outerInstance;
					this.Description = description;
			  }

			  internal virtual TaskThread CreateThread( string name, Task[] tasks )
			  {
				  lock ( this )
				  {
						TaskThread thread = new TaskThread( NextName( name ), tasks );
						Threads.Add( thread );
						thread.Start();
						return thread;
				  }
			  }

			  internal virtual string NextName( string name )
			  {
					return Description.MethodName + "-" + ( ++I ) + ( string.ReferenceEquals( name, null ) ? "" : ( ":" + name ) );
			  }

			  internal virtual void CompleteAll( IList<Exception> failures )
			  {
					foreach ( TaskThread thread in Threads )
					{
						 try
						 {
							  thread.Complete( failures, outerInstance.timeout, outerInstance.unit );
						 }
						 catch ( InterruptedException interrupted )
						 {
							  failures.Add( interrupted );
						 }
					}
			  }
		 }

		 private class TaskThread : Thread, ThreadInfo
		 {
			  internal readonly Task[] Tasks;
			  internal Exception Failure;

			  internal TaskThread( string name, Task[] tasks ) : base( name )
			  {
					this.Tasks = tasks;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void complete(java.util.List<Throwable> failures, long timeout, java.util.concurrent.TimeUnit unit) throws InterruptedException
			  internal virtual void Complete( IList<Exception> failures, long timeout, TimeUnit unit )
			  {
					join( unit.toMillis( timeout ) );
					if ( Alive )
					{
						 failures.Add( new ThreadStillRunningException( this ) );
					}
					if ( Failure != null )
					{
						 failures.Add( Failure );
					}
			  }

			  public override void Run()
			  {
					try
					{
						 foreach ( Task task in Tasks )
						 {
							  task.Perform();
						 }
					}
					catch ( Exception e )
					{
						 Failure = e;
					}
			  }

			  public override object Blocker()
			  {
					return getBlocker( this );
			  }
		 }

		 private class ThreadStillRunningException : Exception
		 {
			  internal ThreadStillRunningException( TaskThread thread ) : base( '"' + thread.Name + "\"; state=" + thread.State + "; blockedOn=" + thread.Blocker() )
			  {
					StackTrace = thread.StackTrace;
			  }

			  public override Exception FillInStackTrace()
			  {
				  lock ( this )
				  {
						return this;
				  }
			  }
		 }
	}

}
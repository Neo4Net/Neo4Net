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
namespace Neo4Net.Test.rule.concurrent
{
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;
	using TestRule = org.junit.rules.TestRule;
	using Description = org.junit.runner.Description;
	using Statement = org.junit.runners.model.Statement;


	using Neo4Net.Test;

	public class OtherThreadRule<STATE> : TestRule
	{
		 private readonly string _name;
		 private readonly long _timeout;
		 private readonly TimeUnit _unit;
		 private volatile OtherThreadExecutor<STATE> _executor;

		 public OtherThreadRule() : this(null)
		 {
		 }

		 public OtherThreadRule( string name ) : this( name, 60, SECONDS )
		 {
		 }

		 public OtherThreadRule( long timeout, TimeUnit unit ) : this( null, timeout, unit )
		 {
		 }

		 public OtherThreadRule( string name, long timeout, TimeUnit unit )
		 {
			  this._name = name;
			  this._timeout = timeout;
			  this._unit = unit;
		 }

		 public virtual Future<RESULT> Execute<RESULT>( OtherThreadExecutor.WorkerCommand<STATE, RESULT> cmd )
		 {
			  Future<RESULT> future = _executor.executeDontWait( cmd );
			  try
			  {
					_executor.awaitStartExecuting();
			  }
			  catch ( InterruptedException e )
			  {
					throw new Exception( "Interrupted while awaiting start of execution.", e );
			  }
			  return future;
		 }

		 protected internal virtual STATE InitialState()
		 {
			  return default( STATE );
		 }

		 public static Matcher<OtherThreadRule> Waiting
		 {
			 get
			 {
				  return IsThreadState( Thread.State.WAITING, Thread.State.TIMED_WAITING );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.Matcher<OtherThreadRule> isThreadState(final Thread.State... eitherOfStates)
		 public static Matcher<OtherThreadRule> IsThreadState( params Thread.State[] eitherOfStates )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass( eitherOfStates );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<OtherThreadRule>
		 {
			 private Thread.State[] _eitherOfStates;

			 public TypeSafeMatcherAnonymousInnerClass( Thread.State[] eitherOfStates )
			 {
				 this._eitherOfStates = eitherOfStates;
			 }

			 protected internal override bool matchesSafely( OtherThreadRule rule )
			 {
				  try
				  {
						rule.executor.waitUntilThreadState( _eitherOfStates );
						return true;
				  }
				  catch ( TimeoutException )
				  {
						rule.executor.printStackTrace( System.err );
						return false;
				  }
			 }

			 public override void describeTo( org.hamcrest.Description description )
			 {
				  description.appendText( "Thread blocked in state WAITING" );
			 }
		 }

		 public virtual OtherThreadExecutor<STATE> Get()
		 {
			  return _executor;
		 }

		 public virtual void Interrupt()
		 {
			  _executor.interrupt();
		 }

		 public override string ToString()
		 {
			  OtherThreadExecutor<STATE> otherThread = _executor;
			  if ( otherThread == null )
			  {
					return "OtherThreadRule[state=dead]";
			  }
			  return otherThread.ToString();
		 }

		 // Implementation of TestRule

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.junit.runners.model.Statement apply(final org.junit.runners.model.Statement super, final org.junit.runner.Description description)
		 public override Statement Apply( Statement @base, Description description )
		 {
			  return new StatementAnonymousInnerClass( this, @base, description );
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private readonly OtherThreadRule<STATE> _outerInstance;

			 private Statement @base;
			 private Description _description;

			 public StatementAnonymousInnerClass( OtherThreadRule<STATE> outerInstance, Statement @base, Description description )
			 {
				 this.outerInstance = outerInstance;
				 this.@base = @base;
				 this._description = description;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void evaluate() throws Throwable
			 public override void evaluate()
			 {
				  string threadName = !string.ReferenceEquals( _outerInstance.name, null ) ? _outerInstance.name + "-" + _description.DisplayName : _description.DisplayName;
				  _outerInstance.executor = new OtherThreadExecutor<STATE>( threadName, _outerInstance.timeout, _outerInstance.unit, _outerInstance.initialState() );
				  try
				  {
						@base.evaluate();
				  }
				  finally
				  {
						try
						{
							 _outerInstance.executor.Dispose();
						}
						finally
						{
							 _outerInstance.executor = null;
						}
				  }
			 }
		 }
	}

}
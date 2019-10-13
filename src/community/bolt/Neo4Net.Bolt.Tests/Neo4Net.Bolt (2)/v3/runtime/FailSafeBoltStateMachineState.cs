using System;

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
namespace Neo4Net.Bolt.v3.runtime
{
	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using BoltConnectionFatality = Neo4Net.Bolt.runtime.BoltConnectionFatality;
	using BoltStateMachineState = Neo4Net.Bolt.runtime.BoltStateMachineState;
	using StateMachineContext = Neo4Net.Bolt.runtime.StateMachineContext;
	using InterruptSignal = Neo4Net.Bolt.v1.messaging.request.InterruptSignal;
	using AuthorizationExpiredException = Neo4Net.Graphdb.security.AuthorizationExpiredException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.checkState;

	public abstract class FailSafeBoltStateMachineState : BoltStateMachineState
	{
		public abstract string Name();
		 private BoltStateMachineState _failedState;
		 private BoltStateMachineState _interruptedState;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.bolt.runtime.BoltStateMachineState process(org.neo4j.bolt.messaging.RequestMessage message, org.neo4j.bolt.runtime.StateMachineContext context) throws org.neo4j.bolt.runtime.BoltConnectionFatality
		 public override BoltStateMachineState Process( RequestMessage message, StateMachineContext context )
		 {
			  AssertInitialized();

			  if ( message is InterruptSignal )
			  {
					return _interruptedState;
			  }

			  try
			  {
					return ProcessUnsafe( message, context );
			  }
			  catch ( AuthorizationExpiredException e )
			  {
					context.HandleFailure( e, true );
					return _failedState;
			  }
			  catch ( Exception t )
			  {
					context.HandleFailure( t, false );
					return _failedState;
			  }
		 }

		 public virtual BoltStateMachineState FailedState
		 {
			 set
			 {
				  this._failedState = value;
			 }
		 }

		 public virtual BoltStateMachineState InterruptedState
		 {
			 set
			 {
				  this._interruptedState = value;
			 }
		 }

		 protected internal virtual void AssertInitialized()
		 {
			  checkState( _failedState != null, "Failed state not set" );
			  checkState( _interruptedState != null, "Interrupted state not set" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract org.neo4j.bolt.runtime.BoltStateMachineState processUnsafe(org.neo4j.bolt.messaging.RequestMessage message, org.neo4j.bolt.runtime.StateMachineContext context) throws Throwable;
		 protected internal abstract BoltStateMachineState ProcessUnsafe( RequestMessage message, StateMachineContext context );
	}

}
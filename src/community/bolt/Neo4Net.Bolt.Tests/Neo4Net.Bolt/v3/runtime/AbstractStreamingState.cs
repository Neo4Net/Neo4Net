﻿/*
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
	using BoltStateMachineState = Neo4Net.Bolt.runtime.BoltStateMachineState;
	using StateMachineContext = Neo4Net.Bolt.runtime.StateMachineContext;
	using DiscardAllMessage = Neo4Net.Bolt.v1.messaging.request.DiscardAllMessage;
	using PullAllMessage = Neo4Net.Bolt.v1.messaging.request.PullAllMessage;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.checkState;

	/// <summary>
	/// When STREAMING, a result is available as a stream of records.
	/// These must be PULLed or DISCARDed before any further statements
	/// can be executed.
	/// </summary>
	public abstract class AbstractStreamingState : FailSafeBoltStateMachineState
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal BoltStateMachineState ReadyStateConflict;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.bolt.runtime.BoltStateMachineState processUnsafe(org.neo4j.bolt.messaging.RequestMessage message, org.neo4j.bolt.runtime.StateMachineContext context) throws Throwable
		 public override BoltStateMachineState ProcessUnsafe( RequestMessage message, StateMachineContext context )
		 {
			  if ( message is PullAllMessage )
			  {
					return ProcessStreamResultMessage( true, context );
			  }
			  if ( message is DiscardAllMessage )
			  {
					return ProcessStreamResultMessage( false, context );
			  }
			  return null;
		 }

		 public virtual BoltStateMachineState ReadyState
		 {
			 set
			 {
				  this.ReadyStateConflict = value;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract org.neo4j.bolt.runtime.BoltStateMachineState processStreamResultMessage(boolean pull, org.neo4j.bolt.runtime.StateMachineContext context) throws Throwable;
		 internal abstract BoltStateMachineState ProcessStreamResultMessage( bool pull, StateMachineContext context );

		 protected internal override void AssertInitialized()
		 {
			  checkState( ReadyStateConflict != null, "Ready state not set" );
			  base.AssertInitialized();
		 }
	}

}
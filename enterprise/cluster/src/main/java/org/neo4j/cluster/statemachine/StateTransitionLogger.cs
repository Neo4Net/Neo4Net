using System;
using System.Text;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.cluster.statemachine
{
	using AtomicBroadcastSerializer = Org.Neo4j.cluster.protocol.atomicbroadcast.AtomicBroadcastSerializer;
	using Payload = Org.Neo4j.cluster.protocol.atomicbroadcast.Payload;
	using HeartbeatState = Org.Neo4j.cluster.protocol.heartbeat.HeartbeatState;
	using Strings = Org.Neo4j.Helpers.Strings;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.com.message.Message.HEADER_CONVERSATION_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.com.message.Message.HEADER_FROM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.protocol.atomicbroadcast.multipaxos.InstanceId.INSTANCE;

	/// <summary>
	/// Logs state transitions in <seealso cref="StateMachine"/>s. Use this for debugging mainly.
	/// </summary>
	public class StateTransitionLogger : StateTransitionListener
	{
		 private readonly LogProvider _logProvider;
		 private AtomicBroadcastSerializer _atomicBroadcastSerializer;

		 /// <summary>
		 /// Throttle so don't flood occurrences of the same message over and over </summary>
		 private string _lastLogMessage = "";

		 public StateTransitionLogger( LogProvider logProvider, AtomicBroadcastSerializer atomicBroadcastSerializer )
		 {
			  this._logProvider = logProvider;
			  this._atomicBroadcastSerializer = atomicBroadcastSerializer;
		 }

		 public override void StateTransition( StateTransition transition )
		 {
			  Log log = _logProvider.getLog( transition.OldState.GetType() );

			  if ( log.DebugEnabled )
			  {
					if ( transition.OldState == HeartbeatState.heartbeat )
					{
						 return;
					}

					// The bulk of the message
					string state = transition.OldState.GetType().BaseType.SimpleName;
					StringBuilder line = ( new StringBuilder( state ) ).Append( ": " ).Append( transition );

					// Who was this message from?
					if ( transition.Message.hasHeader( HEADER_FROM ) )
					{
						 line.Append( " from:" ).Append( transition.Message.getHeader( HEADER_FROM ) );
					}

					if ( transition.Message.hasHeader( INSTANCE ) )
					{
						 line.Append( " instance:" ).Append( transition.Message.getHeader( INSTANCE ) );
					}

					if ( transition.Message.hasHeader( HEADER_CONVERSATION_ID ) )
					{
						 line.Append( " conversation-id:" ).Append( transition.Message.getHeader( HEADER_CONVERSATION_ID ) );
					}

					object payload = transition.Message.Payload;
					if ( payload != null )
					{
						 if ( payload is Payload )
						 {
							  try
							  {
									payload = _atomicBroadcastSerializer.receive( ( Payload ) payload );
							  }
							  catch ( Exception )
							  {
									// Ignore
							  }
						 }

						 line.Append( " payload:" ).Append( Strings.prettyPrint( payload ) );
					}

					// Throttle
					string msg = line.ToString();
					if ( msg.Equals( _lastLogMessage ) )
					{
						 return;
					}

					// Log it
					log.Debug( line.ToString() );
					_lastLogMessage = msg;
			  }
		 }
	}

}
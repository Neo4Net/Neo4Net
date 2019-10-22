/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.cluster.statemachine
{
	using Test = org.junit.Test;
	using Neo4Net.cluster.com.message;
	using AtomicBroadcastSerializer = Neo4Net.cluster.protocol.atomicbroadcast.AtomicBroadcastSerializer;
	using ObjectStreamFactory = Neo4Net.cluster.protocol.atomicbroadcast.ObjectStreamFactory;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.cluster.protocol.cluster.ClusterMessage.join;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.cluster.protocol.cluster.ClusterState.entered;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.cluster.protocol.cluster.ClusterState.joining;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.logging.AssertableLogProvider.inLog;

	public class StateTransitionLoggerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrottle()
		 public virtual void ShouldThrottle()
		 {
			  // Given
			  AssertableLogProvider logProvider = new AssertableLogProvider( true );

			  StateTransitionLogger stateLogger = new StateTransitionLogger( logProvider, new AtomicBroadcastSerializer( new ObjectStreamFactory(), new ObjectStreamFactory() ) );

			  // When
			  stateLogger.StateTransition( new StateTransition( entered, Message.Internal( join ), joining ) );
			  stateLogger.StateTransition( new StateTransition( entered, Message.Internal( join ), joining ) );
			  stateLogger.StateTransition( new StateTransition( joining, Message.Internal( join ), entered ) );
			  stateLogger.StateTransition( new StateTransition( entered, Message.Internal( join ), joining ) );

			  // Then
			  logProvider.AssertExactly( inLog( entered.GetType() ).debug("ClusterState: entered-[join]->joining"), inLog(joining.GetType()).debug("ClusterState: joining-[join]->entered"), inLog(entered.GetType()).debug("ClusterState: entered-[join]->joining") );
		 }
	}

}
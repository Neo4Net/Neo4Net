using System;
using System.Collections.Generic;

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
namespace Neo4Net.ha.correctness
{

	using InstanceId = Neo4Net.cluster.InstanceId;
	using Neo4Net.cluster.com.message;
	using ClusterConfiguration = Neo4Net.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterMessage = Neo4Net.cluster.protocol.cluster.ClusterMessage;
	using Neo4Net.Helpers.Collection;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.ha.correctness.ClusterInstance.newClusterInstance;

	public class Prover
	{
		 private readonly LinkedList<ClusterState> _unexploredKnownStates = new LinkedList<ClusterState>();
		 private readonly ProofDatabase _db = new ProofDatabase( "./clusterproof" );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String... args) throws Exception
		 public static void Main( params string[] args )
		 {
			  ( new Prover() ).Prove();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void prove() throws Exception
		 public virtual void Prove()
		 {
			  try
			  {
					Console.WriteLine( "Bootstrap genesis state.." );
					BootstrapCluster();
					Console.WriteLine( "Begin exploring delivery orders." );
					ExploreUnexploredStates();
					Console.WriteLine( "Exporting graphviz.." );
					_db.export( new GraphVizExporter( new File( "./proof.gs" ) ) );
			  }
			  finally
			  {
					_db.shutdown();
			  }

			  // Generate .svg :
			  // dot -Tsvg proof.gs -o proof.svg
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void bootstrapCluster() throws Exception
		 private void BootstrapCluster()
		 {
			  string instance1 = "cluster://localhost:5001";
			  string instance2 = "cluster://localhost:5002";
			  string instance3 = "cluster://localhost:5003";
			  ClusterConfiguration config = new ClusterConfiguration( "default", NullLogProvider.Instance, instance1, instance2, instance3 );

			  ClusterState state = new ClusterState( new IList<ClusterInstance> { newClusterInstance( new InstanceId( 1 ), new URI( instance1 ), new Monitors(), config, 10, NullLogProvider.Instance ), newClusterInstance(new InstanceId(2), new URI(instance2), new Monitors(), config, 10, NullLogProvider.Instance), newClusterInstance(new InstanceId(3), new URI(instance3), new Monitors(), config, 10, NullLogProvider.Instance) }, emptySet() );

			  state = state.PerformAction( new MessageDeliveryAction( Message.to( ClusterMessage.create, new URI( instance3 ), "defaultcluster" ).setHeader( Message.HEADER_CONVERSATION_ID, "-1" ).setHeader( Message.HEADER_FROM, instance3 ) ) );
			  state = state.PerformAction(new MessageDeliveryAction(Message.to(ClusterMessage.join, new URI(instance2), new object[]
			  {
				  "defaultcluster", new URI[]{ new URI( instance3 ) }
			  }).setHeader( Message.HEADER_CONVERSATION_ID, "-1" ).setHeader( Message.HEADER_FROM, instance2 )));
			  state = state.PerformAction(new MessageDeliveryAction(Message.to(ClusterMessage.join, new URI(instance1), new object[]
			  {
				  "defaultcluster", new URI[]{ new URI( instance3 ) }
			  }).setHeader( Message.HEADER_CONVERSATION_ID, "-1" ).setHeader( Message.HEADER_FROM, instance1 )));

			  state.AddPendingActions( new InstanceCrashedAction( instance3 ) );

			  _unexploredKnownStates.AddLast( state );

			  _db.newState( state );
		 }

		 private void ExploreUnexploredStates()
		 {
			  while ( _unexploredKnownStates.Count > 0 )
			  {
					ClusterState state = _unexploredKnownStates.RemoveFirst();

					IEnumerator<Pair<ClusterAction, ClusterState>> newStates = state.Transitions();
					while ( newStates.MoveNext() )
					{
						 Pair<ClusterAction, ClusterState> next = newStates.Current;
						 Console.WriteLine( _db.numberOfKnownStates() + " (" + _unexploredKnownStates.Count + ")" );

						 ClusterState nextState = next.Other();
						 if ( !_db.isKnownState( nextState ) )
						 {
							  _db.newStateTransition( state, next );
							  _unexploredKnownStates.AddLast( nextState );

							  if ( nextState.DeadEnd )
							  {
									Console.WriteLine( "DEAD END: " + nextState.ToString() + " (" + _db.id(nextState) + ")" );
							  }
						 }
					}
			  }
		 }
	}

}
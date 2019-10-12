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
namespace Neo4Net.cluster.protocol.atomicbroadcast.multipaxos
{
	using Test = org.junit.Test;


	using Cluster = Neo4Net.cluster.protocol.cluster.Cluster;
	using FixedTimeoutStrategy = Neo4Net.cluster.timeout.FixedTimeoutStrategy;
	using MessageTimeoutStrategy = Neo4Net.cluster.timeout.MessageTimeoutStrategy;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using NullLogService = Neo4Net.Logging.@internal.NullLogService;

	public class MultiPaxosTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestFailure()
		 {
			  ScriptableNetworkFailureLatencyStrategy networkLatency = new ScriptableNetworkFailureLatencyStrategy();
			  NetworkMock network = new NetworkMock( NullLogService.Instance, new Monitors(), 50, new MultipleFailureLatencyStrategy(networkLatency), new MessageTimeoutStrategy(new FixedTimeoutStrategy(1000)) );

			  IList<TestProtocolServer> nodes = new List<TestProtocolServer>();

			  TestProtocolServer server = network.AddServer( 1, URI.create( "cluster://server1" ) );
			  server.NewClient( typeof( Cluster ) ).create( "default" );
			  network.TickUntilDone();
			  nodes.Add( server );

			  for ( int i = 1; i < 3; i++ )
			  {
					TestProtocolServer protocolServer = network.AddServer( i + 1, new URI( "cluster://server" + ( i + 1 ) ) );
					protocolServer.NewClient( typeof( Cluster ) ).join( "default", new URI( "cluster://server1" ) );
					network.Tick( 10 );
					nodes.Add( protocolServer );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.protocol.atomicbroadcast.AtomicBroadcast atomicBroadcast = nodes.get(0).newClient(org.neo4j.cluster.protocol.atomicbroadcast.AtomicBroadcast.class);
			  AtomicBroadcast atomicBroadcast = nodes[0].NewClient( typeof( AtomicBroadcast ) );
			  ObjectStreamFactory objectStreamFactory = new ObjectStreamFactory();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.protocol.atomicbroadcast.AtomicBroadcastSerializer serializer = new org.neo4j.cluster.protocol.atomicbroadcast.AtomicBroadcastSerializer(objectStreamFactory, objectStreamFactory);
			  AtomicBroadcastSerializer serializer = new AtomicBroadcastSerializer( objectStreamFactory, objectStreamFactory );
			  atomicBroadcast.Broadcast( serializer.Broadcast( new DaPayload() ) );

			  networkLatency.NodeIsDown( "cluster://server2" );
			  networkLatency.NodeIsDown( "cluster://server3" );

			  atomicBroadcast.Broadcast( serializer.Broadcast( new DaPayload() ) );
			  network.Tick( 100 );
			  networkLatency.NodeIsUp( "cluster://server3" );
			  network.Tick( 1000 );

			  foreach ( TestProtocolServer node in nodes )
			  {
					node.NewClient( typeof( Cluster ) ).leave();
					network.Tick( 10 );
			  }

		 }

		 [Serializable]
		 private sealed class DaPayload
		 {
			  internal const long SERIAL_VERSION_UID = -2896543854010391900L;
		 }
	}

}
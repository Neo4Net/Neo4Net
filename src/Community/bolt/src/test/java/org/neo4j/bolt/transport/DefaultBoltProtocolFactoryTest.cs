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
namespace Neo4Net.Bolt.transport
{
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using Test = org.junit.Test;
	using ParameterizedTest = org.junit.jupiter.@params.ParameterizedTest;
	using ValueSource = org.junit.jupiter.@params.provider.ValueSource;

	using BoltConnection = Neo4Net.Bolt.runtime.BoltConnection;
	using BoltConnectionFactory = Neo4Net.Bolt.runtime.BoltConnectionFactory;
	using BoltStateMachine = Neo4Net.Bolt.runtime.BoltStateMachine;
	using BoltStateMachineFactory = Neo4Net.Bolt.runtime.BoltStateMachineFactory;
	using BoltTestUtil = Neo4Net.Bolt.testing.BoltTestUtil;
	using BoltProtocolV1 = Neo4Net.Bolt.v1.BoltProtocolV1;
	using BoltProtocolV2 = Neo4Net.Bolt.v2.BoltProtocolV2;
	using BoltProtocolV3 = Neo4Net.Bolt.v3.BoltProtocolV3;
	using NullLogService = Neo4Net.Logging.@internal.NullLogService;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class DefaultBoltProtocolFactoryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateNothingForUnknownProtocolVersion()
		 internal virtual void ShouldCreateNothingForUnknownProtocolVersion()
		 {
			  int protocolVersion = 42;
			  BoltChannel channel = BoltTestUtil.newTestBoltChannel();
			  BoltProtocolFactory factory = new DefaultBoltProtocolFactory( mock( typeof( BoltConnectionFactory ) ), mock( typeof( BoltStateMachineFactory ) ), NullLogService.Instance );

			  BoltProtocol protocol = factory( protocolVersion, channel );

			  // handler is not created
			  assertNull( protocol );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest(name = "V{0}") @ValueSource(longs = {org.neo4j.bolt.v1.BoltProtocolV1.VERSION, org.neo4j.bolt.v2.BoltProtocolV2.VERSION, org.neo4j.bolt.v3.BoltProtocolV3.VERSION}) void shouldCreateBoltProtocol(long protocolVersion) throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCreateBoltProtocol( long protocolVersion )
		 {
			  EmbeddedChannel channel = new EmbeddedChannel();
			  BoltChannel boltChannel = new BoltChannel( "bolt-1", "bolt", channel );

			  BoltStateMachineFactory stateMachineFactory = mock( typeof( BoltStateMachineFactory ) );
			  BoltStateMachine stateMachine = mock( typeof( BoltStateMachine ) );
			  when( stateMachineFactory.NewStateMachine( protocolVersion, boltChannel ) ).thenReturn( stateMachine );

			  BoltConnectionFactory connectionFactory = mock( typeof( BoltConnectionFactory ) );
			  BoltConnection connection = mock( typeof( BoltConnection ) );
			  when( connectionFactory.NewConnection( boltChannel, stateMachine ) ).thenReturn( connection );

			  BoltProtocolFactory factory = new DefaultBoltProtocolFactory( connectionFactory, stateMachineFactory, NullLogService.Instance );

			  BoltProtocol protocol = factory( protocolVersion, boltChannel );

			  protocol.Install();

			  // handler with correct version is created
			  assertEquals( protocolVersion, protocol.Version() );
			  // it uses the expected worker
			  verify( connectionFactory ).newConnection( eq( boltChannel ), any( typeof( BoltStateMachine ) ) );

			  // and halts this same worker when closed
			  verify( connection, never() ).stop();
			  channel.close();
			  verify( connection ).stop();

			  channel.finishAndReleaseAll();
		 }
	}

}
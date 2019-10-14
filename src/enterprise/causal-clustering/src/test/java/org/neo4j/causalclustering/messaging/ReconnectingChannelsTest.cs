/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.messaging
{
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;


	using ProtocolStack = Neo4Net.causalclustering.protocol.handshake.ProtocolStack;
	using TestProtocols_TestApplicationProtocols = Neo4Net.causalclustering.protocol.handshake.TestProtocols_TestApplicationProtocols;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Neo4Net.Helpers.Collections;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static co.unruly.matchers.StreamMatchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static co.unruly.matchers.StreamMatchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class ReconnectingChannelsTest
	{
		 private ReconnectingChannels _reconnectingChannels = new ReconnectingChannels();
		 private AdvertisedSocketAddress _to1 = new AdvertisedSocketAddress( "host1", 1 );
		 private AdvertisedSocketAddress _to2 = new AdvertisedSocketAddress( "host2", 1 );
		 private ReconnectingChannel _channel1 = Mockito.mock( typeof( ReconnectingChannel ) );
		 private ReconnectingChannel _channel2 = Mockito.mock( typeof( ReconnectingChannel ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnEmptyStreamOfInstalledProtocolsIfNoChannels()
		 public virtual void ShouldReturnEmptyStreamOfInstalledProtocolsIfNoChannels()
		 {
			  // when
			  Stream<Pair<AdvertisedSocketAddress, ProtocolStack>> installedProtocols = _reconnectingChannels.installedProtocols();

			  // then
			  assertThat( installedProtocols, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnStreamOfInstalledProtocolsForChannelsThatHaveCompletedHandshake()
		 public virtual void ShouldReturnStreamOfInstalledProtocolsForChannelsThatHaveCompletedHandshake()
		 {
			  // given
			  _reconnectingChannels.putIfAbsent( _to1, _channel1 );
			  _reconnectingChannels.putIfAbsent( _to2, _channel2 );
			  ProtocolStack protocolStack1 = new ProtocolStack( TestProtocols_TestApplicationProtocols.RAFT_3, emptyList() );
			  ProtocolStack protocolStack2 = new ProtocolStack( TestProtocols_TestApplicationProtocols.RAFT_2, emptyList() );
			  Mockito.when( _channel1.installedProtocolStack() ).thenReturn(protocolStack1);
			  Mockito.when( _channel2.installedProtocolStack() ).thenReturn(protocolStack2);

			  // when
			  Stream<Pair<AdvertisedSocketAddress, ProtocolStack>> installedProtocols = _reconnectingChannels.installedProtocols();

			  // then
			  Stream<Pair<AdvertisedSocketAddress, ProtocolStack>> sorted = installedProtocols.sorted( System.Collections.IComparer.comparing( p => p.first().Hostname ) );
			  assertThat( sorted, contains( Pair.of( _to1, protocolStack1 ), Pair.of( _to2, protocolStack2 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExcludeChannelsWithoutInstalledProtocol()
		 public virtual void ShouldExcludeChannelsWithoutInstalledProtocol()
		 {
			  // given
			  _reconnectingChannels.putIfAbsent( _to1, _channel1 );
			  _reconnectingChannels.putIfAbsent( _to2, _channel2 );
			  ProtocolStack protocolStack1 = new ProtocolStack( TestProtocols_TestApplicationProtocols.RAFT_3, emptyList() );
			  Mockito.when( _channel1.installedProtocolStack() ).thenReturn(protocolStack1);
			  Mockito.when( _channel2.installedProtocolStack() ).thenReturn(null);

			  // when
			  Stream<Pair<AdvertisedSocketAddress, ProtocolStack>> installedProtocols = _reconnectingChannels.installedProtocols();

			  // then
			  assertThat( installedProtocols, contains( Pair.of( _to1, protocolStack1 ) ) );
		 }
	}

}
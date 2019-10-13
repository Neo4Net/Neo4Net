using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.discovery.procedures
{
	using Test = org.junit.Test;

	using ProtocolStack = Neo4Net.causalclustering.protocol.handshake.ProtocolStack;
	using TestProtocols_TestApplicationProtocols = Neo4Net.causalclustering.protocol.handshake.TestProtocols_TestApplicationProtocols;
	using TestProtocols_TestModifierProtocols = Neo4Net.causalclustering.protocol.handshake.TestProtocols_TestModifierProtocols;
	using InstalledProtocolsProcedureIT = Neo4Net.causalclustering.scenarios.InstalledProtocolsProcedureIT;
	using Neo4Net.Collections;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using SocketAddress = Neo4Net.Helpers.SocketAddress;
	using Neo4Net.Helpers.Collections;
	using ProcedureException = Neo4Net.@internal.Kernel.Api.exceptions.ProcedureException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.arrayContaining;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	/// <seealso cref= InstalledProtocolsProcedureIT </seealso>
	public class InstalledProtocolsProcedureTest
	{
		 private Pair<AdvertisedSocketAddress, ProtocolStack> _outbound1 = Pair.of( new AdvertisedSocketAddress( "host1", 1 ), new ProtocolStack( TestProtocols_TestApplicationProtocols.RAFT_1, new IList<ModifierProtocol> { TestProtocols_TestModifierProtocols.SNAPPY } ) );
		 private Pair<AdvertisedSocketAddress, ProtocolStack> _outbound2 = Pair.of( new AdvertisedSocketAddress( "host2", 2 ), new ProtocolStack( TestProtocols_TestApplicationProtocols.RAFT_2, new IList<ModifierProtocol> { TestProtocols_TestModifierProtocols.SNAPPY, TestProtocols_TestModifierProtocols.ROT13 } ) );

		 private Pair<SocketAddress, ProtocolStack> _inbound1 = Pair.of( new SocketAddress( "host3", 3 ), new ProtocolStack( TestProtocols_TestApplicationProtocols.RAFT_3, new IList<ModifierProtocol> { TestProtocols_TestModifierProtocols.SNAPPY } ) );
		 private Pair<SocketAddress, ProtocolStack> _inbound2 = Pair.of( new SocketAddress( "host4", 4 ), new ProtocolStack( TestProtocols_TestApplicationProtocols.RAFT_4, emptyList() ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveEmptyOutputIfNoInstalledProtocols() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaveEmptyOutputIfNoInstalledProtocols()
		 {
			  // given
			  InstalledProtocolsProcedure installedProtocolsProcedure = new InstalledProtocolsProcedure( Stream.empty, Stream.empty );

			  // when
			  RawIterator<object[], ProcedureException> result = installedProtocolsProcedure.Apply( null, null, null );

			  // then
			  assertFalse( result.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListOutboundProtocols() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListOutboundProtocols()
		 {
			  // given
			  InstalledProtocolsProcedure installedProtocolsProcedure = new InstalledProtocolsProcedure( () => Stream.of(_outbound1, _outbound2), Stream.empty );

			  // when
			  RawIterator<object[], ProcedureException> result = installedProtocolsProcedure.Apply( null, null, null );

			  // then
			  assertThat( result.Next(), arrayContaining("outbound", "host1:1", "raft", 1L, "[TestSnappy]") );
			  assertThat( result.Next(), arrayContaining("outbound", "host2:2", "raft", 2L, "[TestSnappy,ROT13]") );
			  assertFalse( result.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListInboundProtocols() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListInboundProtocols()
		 {
			  // given
			  InstalledProtocolsProcedure installedProtocolsProcedure = new InstalledProtocolsProcedure( Stream.empty, () => Stream.of(_inbound1, _inbound2) );

			  // when
			  RawIterator<object[], ProcedureException> result = installedProtocolsProcedure.Apply( null, null, null );

			  // then
			  assertThat( result.Next(), arrayContaining("inbound", "host3:3", "raft", 3L, "[TestSnappy]") );
			  assertThat( result.Next(), arrayContaining("inbound", "host4:4", "raft", 4L, "[]") );
			  assertFalse( result.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListInboundAndOutboundProtocols() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListInboundAndOutboundProtocols()
		 {
			  // given
			  InstalledProtocolsProcedure installedProtocolsProcedure = new InstalledProtocolsProcedure( () => Stream.of(_outbound1, _outbound2), () => Stream.of(_inbound1, _inbound2) );

			  // when
			  RawIterator<object[], ProcedureException> result = installedProtocolsProcedure.Apply( null, null, null );

			  // then
			  assertThat( result.Next(), arrayContaining("outbound", "host1:1", "raft", 1L, "[TestSnappy]") );
			  assertThat( result.Next(), arrayContaining("outbound", "host2:2", "raft", 2L, "[TestSnappy,ROT13]") );
			  assertThat( result.Next(), arrayContaining("inbound", "host3:3", "raft", 3L, "[TestSnappy]") );
			  assertThat( result.Next(), arrayContaining("inbound", "host4:4", "raft", 4L, "[]") );
			  assertFalse( result.HasNext() );
		 }
	}

}
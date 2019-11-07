using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.protocol.handshake
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Channel = Neo4Net.causalclustering.messaging.Channel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocolCategory.RAFT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.protocol.Protocol_ModifierProtocolCategory.COMPRESSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.protocol.Protocol_ModifierProtocolCategory.GRATUITOUS_OBFUSCATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.protocol.handshake.TestProtocols_TestApplicationProtocols.RAFT_1;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.protocol.handshake.TestProtocols_TestModifierProtocols.LZ4;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.protocol.handshake.TestProtocols_TestModifierProtocols.LZO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.protocol.handshake.TestProtocols_TestModifierProtocols.SNAPPY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.asSet;

	/// <seealso cref= ProtocolHandshakeSadTest sad path tests </seealso>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class ProtocolHandshakeHappyTest
	public class ProtocolHandshakeHappyTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public Parameters parameters;
		 public Parameters Parameters;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters public static java.util.Collection<Parameters> data()
		 public static ICollection<Parameters> Data()
		 {
			  // Application protocols
			  ApplicationSupportedProtocols allRaft = new ApplicationSupportedProtocols( RAFT, TestProtocols_TestApplicationProtocols.listVersionsOf( RAFT ) );
			  ApplicationSupportedProtocols raft1 = new ApplicationSupportedProtocols( RAFT, singletonList( RAFT_1.implementation() ) );
			  ApplicationSupportedProtocols allRaftByDefault = new ApplicationSupportedProtocols( RAFT, emptyList() );

			  // Modifier protocols
			  ICollection<ModifierSupportedProtocols> allModifiers = asList(new ModifierSupportedProtocols(COMPRESSION, TestProtocols_TestModifierProtocols.listVersionsOf(COMPRESSION)), new ModifierSupportedProtocols(GRATUITOUS_OBFUSCATION, TestProtocols_TestModifierProtocols.listVersionsOf(GRATUITOUS_OBFUSCATION))
						);
			  ICollection<ModifierSupportedProtocols> allCompressionModifiers = singletonList( new ModifierSupportedProtocols( COMPRESSION, TestProtocols_TestModifierProtocols.listVersionsOf( COMPRESSION ) ) );
			  ICollection<ModifierSupportedProtocols> allObfuscationModifiers = singletonList( new ModifierSupportedProtocols( GRATUITOUS_OBFUSCATION, TestProtocols_TestModifierProtocols.listVersionsOf( GRATUITOUS_OBFUSCATION ) ) );
			  ICollection<ModifierSupportedProtocols> allCompressionModifiersByDefault = singletonList( new ModifierSupportedProtocols( COMPRESSION, emptyList() ) );

			  IList<ModifierSupportedProtocols> onlyLzoCompressionModifiers = singletonList( new ModifierSupportedProtocols( COMPRESSION, singletonList( LZO.implementation() ) ) );
			  IList<ModifierSupportedProtocols> onlySnappyCompressionModifiers = singletonList( new ModifierSupportedProtocols( COMPRESSION, singletonList( SNAPPY.implementation() ) ) );

			  ICollection<ModifierSupportedProtocols> noModifiers = emptyList();

			  // Ordered modifier protocols
			  ModifierProtocolRepository modifierProtocolRepository = new ModifierProtocolRepository( TestProtocols_TestModifierProtocols.values(), allModifiers );
			  string[] lzoFirstVersions = new string[] { LZO.implementation(), LZ4.implementation(), SNAPPY.implementation() };
			  IList<ModifierSupportedProtocols> lzoFirstCompressionModifiers = singletonList( new ModifierSupportedProtocols( COMPRESSION, new IList<string> { lzoFirstVersions } ) );
			  Protocol_ModifierProtocol preferredLzoFirstCompressionModifier = modifierProtocolRepository.Select( COMPRESSION.canonicalName(), asSet(lzoFirstVersions) ).get();

			  string[] snappyFirstVersions = new string[] { SNAPPY.implementation(), LZ4.implementation(), LZO.implementation() };
			  IList<ModifierSupportedProtocols> snappyFirstCompressionModifiers = singletonList( new ModifierSupportedProtocols( COMPRESSION, new IList<string> { snappyFirstVersions } ) );
			  Protocol_ModifierProtocol preferredSnappyFirstCompressionModifier = modifierProtocolRepository.Select( COMPRESSION.canonicalName(), asSet(snappyFirstVersions) ).get();

			  return asList(new Parameters(allRaft, allRaft, allModifiers, allModifiers, TestProtocols_TestApplicationProtocols.latest(RAFT), new Protocol_ModifierProtocol[] { TestProtocols_TestModifierProtocols.latest(COMPRESSION), TestProtocols_TestModifierProtocols.latest(GRATUITOUS_OBFUSCATION) }), new Parameters(allRaft, allRaftByDefault, allModifiers, allModifiers, TestProtocols_TestApplicationProtocols.latest(RAFT), new Protocol_ModifierProtocol[] { TestProtocols_TestModifierProtocols.latest(COMPRESSION), TestProtocols_TestModifierProtocols.latest(GRATUITOUS_OBFUSCATION) }), new Parameters(allRaftByDefault, allRaft, allModifiers, allModifiers, TestProtocols_TestApplicationProtocols.latest(RAFT), new Protocol_ModifierProtocol[] { TestProtocols_TestModifierProtocols.latest(COMPRESSION), TestProtocols_TestModifierProtocols.latest(GRATUITOUS_OBFUSCATION) }), new Parameters(allRaft, raft1, allModifiers, allModifiers, RAFT_1, new Protocol_ModifierProtocol[] { TestProtocols_TestModifierProtocols.latest(COMPRESSION), TestProtocols_TestModifierProtocols.latest(GRATUITOUS_OBFUSCATION) }), new Parameters(raft1, allRaft, allModifiers, allModifiers, RAFT_1, new Protocol_ModifierProtocol[] { TestProtocols_TestModifierProtocols.latest(COMPRESSION), TestProtocols_TestModifierProtocols.latest(GRATUITOUS_OBFUSCATION) }), new Parameters(allRaft, allRaft, allModifiers, allCompressionModifiers, TestProtocols_TestApplicationProtocols.latest(RAFT), new Protocol_ModifierProtocol[] { TestProtocols_TestModifierProtocols.latest(COMPRESSION) }), new Parameters(allRaft, allRaft, allCompressionModifiers, allModifiers, TestProtocols_TestApplicationProtocols.latest(RAFT), new Protocol_ModifierProtocol[] { TestProtocols_TestModifierProtocols.latest(COMPRESSION) }), new Parameters(allRaft, allRaft, allModifiers, allCompressionModifiersByDefault, TestProtocols_TestApplicationProtocols.latest(RAFT), new Protocol_ModifierProtocol[] { TestProtocols_TestModifierProtocols.latest(COMPRESSION) }), new Parameters(allRaft, allRaft, allCompressionModifiersByDefault, allModifiers, TestProtocols_TestApplicationProtocols.latest(RAFT), new Protocol_ModifierProtocol[] { TestProtocols_TestModifierProtocols.latest(COMPRESSION) }), new Parameters(allRaft, allRaft, allModifiers, allObfuscationModifiers, TestProtocols_TestApplicationProtocols.latest(RAFT), new Protocol_ModifierProtocol[] { TestProtocols_TestModifierProtocols.latest(GRATUITOUS_OBFUSCATION) }), new Parameters(allRaft, allRaft, allObfuscationModifiers, allModifiers, TestProtocols_TestApplicationProtocols.latest(RAFT), new Protocol_ModifierProtocol[] { TestProtocols_TestModifierProtocols.latest(GRATUITOUS_OBFUSCATION) }), new Parameters(allRaft, allRaft, allModifiers, lzoFirstCompressionModifiers, TestProtocols_TestApplicationProtocols.latest(RAFT), new Protocol_ModifierProtocol[] { LZO }), new Parameters(allRaft, allRaft, lzoFirstCompressionModifiers, allCompressionModifiers, TestProtocols_TestApplicationProtocols.latest(RAFT), new Protocol_ModifierProtocol[] { preferredLzoFirstCompressionModifier }), new Parameters(allRaft, allRaft, allModifiers, snappyFirstCompressionModifiers, TestProtocols_TestApplicationProtocols.latest(RAFT), new Protocol_ModifierProtocol[] { SNAPPY }), new Parameters(allRaft, allRaft, snappyFirstCompressionModifiers, allCompressionModifiers, TestProtocols_TestApplicationProtocols.latest(RAFT), new Protocol_ModifierProtocol[] { preferredSnappyFirstCompressionModifier }), new Parameters(allRaft, allRaft, allModifiers, onlyLzoCompressionModifiers, TestProtocols_TestApplicationProtocols.latest(RAFT), new Protocol_ModifierProtocol[] { TestProtocols_TestModifierProtocols.LZO }), new Parameters(allRaft, allRaft, onlyLzoCompressionModifiers, allModifiers, TestProtocols_TestApplicationProtocols.latest(RAFT), new Protocol_ModifierProtocol[] { TestProtocols_TestModifierProtocols.LZO }), new Parameters(allRaft, allRaft, onlySnappyCompressionModifiers, onlyLzoCompressionModifiers, TestProtocols_TestApplicationProtocols.latest(RAFT), new Protocol_ModifierProtocol[] {}), new Parameters(allRaft, allRaft, onlyLzoCompressionModifiers, onlySnappyCompressionModifiers, TestProtocols_TestApplicationProtocols.latest(RAFT), new Protocol_ModifierProtocol[] {}), new Parameters(allRaft, allRaft, allModifiers, noModifiers, TestProtocols_TestApplicationProtocols.latest(RAFT), new Protocol_ModifierProtocol[] {}), new Parameters(allRaft, allRaft, noModifiers, allModifiers, TestProtocols_TestApplicationProtocols.latest(RAFT), new Protocol_ModifierProtocol[] {})
						);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandshakeApplicationProtocolOnClient()
		 public virtual void ShouldHandshakeApplicationProtocolOnClient()
		 {
			  // given
			  Fixture fixture = new Fixture( Parameters );

			  // when
			  CompletableFuture<ProtocolStack> clientHandshakeFuture = fixture.Initiate();

			  // then
			  assertFalse( fixture.ClientChannel.Closed );
			  ProtocolStack clientProtocolStack = clientHandshakeFuture.getNow( null );
			  assertThat( clientProtocolStack.ApplicationProtocol(), equalTo(Parameters.expectedApplicationProtocol) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandshakeModifierProtocolsOnClient()
		 public virtual void ShouldHandshakeModifierProtocolsOnClient()
		 {
			  // given
			  Fixture fixture = new Fixture( Parameters );

			  // when
			  CompletableFuture<ProtocolStack> clientHandshakeFuture = fixture.Initiate();

			  // then
			  assertFalse( fixture.ClientChannel.Closed );
			  ProtocolStack clientProtocolStack = clientHandshakeFuture.getNow( null );
			  if ( Parameters.expectedModifierProtocols.Length == 0 )
			  {
					assertThat( clientProtocolStack.ModifierProtocols(), empty() );
			  }
			  else
			  {
					assertThat( clientProtocolStack.ModifierProtocols(), contains(Parameters.expectedModifierProtocols) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandshakeApplicationProtocolOnServer()
		 public virtual void ShouldHandshakeApplicationProtocolOnServer()
		 {
			  // given
			  Fixture fixture = new Fixture( Parameters );

			  // when
			  fixture.Initiate();
			  fixture.HandshakeServer.protocolStackFuture();
			  CompletableFuture<ProtocolStack> serverHandshakeFuture = fixture.HandshakeServer.protocolStackFuture();

			  // then
			  assertFalse( fixture.ClientChannel.Closed );
			  ProtocolStack serverProtocolStack = serverHandshakeFuture.getNow( null );
			  assertThat( serverProtocolStack.ApplicationProtocol(), equalTo(Parameters.expectedApplicationProtocol) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandshakeModifierProtocolsOnServer()
		 public virtual void ShouldHandshakeModifierProtocolsOnServer()
		 {
			  // given
			  Fixture fixture = new Fixture( Parameters );

			  // when
			  fixture.Initiate();
			  fixture.HandshakeServer.protocolStackFuture();
			  CompletableFuture<ProtocolStack> serverHandshakeFuture = fixture.HandshakeServer.protocolStackFuture();

			  // then
			  assertFalse( fixture.ClientChannel.Closed );
			  ProtocolStack serverProtocolStack = serverHandshakeFuture.getNow( null );
			  if ( Parameters.expectedModifierProtocols.Length == 0 )
			  {
					assertThat( serverProtocolStack.ModifierProtocols(), empty() );
			  }
			  else
			  {
					assertThat( serverProtocolStack.ModifierProtocols(), contains(Parameters.expectedModifierProtocols) );
			  }
		 }

		 internal class Fixture
		 {
			  internal readonly HandshakeClient HandshakeClient;
			  internal readonly HandshakeServer HandshakeServer;
			  internal readonly FakeChannelWrapper ClientChannel;
			  internal readonly ApplicationProtocolRepository ClientApplicationProtocolRepository;
			  internal readonly ModifierProtocolRepository ClientModifierProtocolRepository;
			  internal readonly Parameters Parameters;

			  internal Fixture( Parameters parameters )
			  {
					ApplicationProtocolRepository serverApplicationProtocolRepository = new ApplicationProtocolRepository( TestProtocols_TestApplicationProtocols.values(), parameters.ServerApplicationProtocol );
					ModifierProtocolRepository serverModifierProtocolRepository = new ModifierProtocolRepository( TestProtocols_TestModifierProtocols.values(), parameters.ServerModifierProtocols );

					ClientApplicationProtocolRepository = new ApplicationProtocolRepository( TestProtocols_TestApplicationProtocols.values(), parameters.ClientApplicationProtocol );
					ClientModifierProtocolRepository = new ModifierProtocolRepository( TestProtocols_TestModifierProtocols.values(), parameters.ClientModifierProtocols );

					HandshakeClient = new HandshakeClient();
					HandshakeServer = new HandshakeServer( serverApplicationProtocolRepository, serverModifierProtocolRepository, new FakeServerChannel( HandshakeClient ) );
					ClientChannel = new FakeClientChannel( HandshakeServer );
					this.Parameters = parameters;
			  }

			  internal virtual CompletableFuture<ProtocolStack> Initiate()
			  {
					return HandshakeClient.initiate( ClientChannel, ClientApplicationProtocolRepository, ClientModifierProtocolRepository );
			  }
		 }

		 internal class Parameters
		 {
			  internal readonly ApplicationSupportedProtocols ClientApplicationProtocol;
			  internal readonly ApplicationSupportedProtocols ServerApplicationProtocol;
			  internal readonly ICollection<ModifierSupportedProtocols> ClientModifierProtocols;
			  internal readonly ICollection<ModifierSupportedProtocols> ServerModifierProtocols;
			  internal readonly Protocol_ApplicationProtocol ExpectedApplicationProtocol;
			  internal readonly Protocol_ModifierProtocol[] ExpectedModifierProtocols;

			  internal Parameters( ApplicationSupportedProtocols clientApplicationProtocol, ApplicationSupportedProtocols serverApplicationProtocol, ICollection<ModifierSupportedProtocols> clientModifierProtocols, ICollection<ModifierSupportedProtocols> serverModifierProtocols, Protocol_ApplicationProtocol expectedApplicationProtocol, Protocol_ModifierProtocol[] expectedModifierProtocols )
			  {
					this.ClientModifierProtocols = clientModifierProtocols;
					this.ClientApplicationProtocol = clientApplicationProtocol;
					this.ServerApplicationProtocol = serverApplicationProtocol;
					this.ServerModifierProtocols = serverModifierProtocols;
					this.ExpectedApplicationProtocol = expectedApplicationProtocol;
					this.ExpectedModifierProtocols = expectedModifierProtocols;
			  }
		 }

		 internal abstract class FakeChannelWrapper : Channel
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool ClosedConflict;

			  public virtual bool Disposed
			  {
				  get
				  {
						return ClosedConflict;
				  }
			  }

			  public override void Dispose()
			  {
					ClosedConflict = true;
			  }

			  public virtual bool Open
			  {
				  get
				  {
						return true;
				  }
			  }

			  public override abstract CompletableFuture<Void> Write( object msg );

			  public override CompletableFuture<Void> WriteAndFlush( object msg )
			  {
					return Write( msg );
			  }

			  internal virtual bool Closed
			  {
				  get
				  {
						return ClosedConflict;
				  }
			  }
		 }

		 internal class FakeServerChannel : FakeChannelWrapper
		 {
			  internal readonly HandshakeClient HandshakeClient;

			  internal FakeServerChannel( HandshakeClient handshakeClient ) : base()
			  {
					this.HandshakeClient = handshakeClient;
			  }

			  public override CompletableFuture<Void> Write( object msg )
			  {
					( ( ClientMessage ) msg ).Dispatch( HandshakeClient );
					return CompletableFuture.completedFuture( null );
			  }
		 }

		 internal class FakeClientChannel : FakeChannelWrapper
		 {
			  internal readonly HandshakeServer HandshakeServer;

			  internal FakeClientChannel( HandshakeServer handshakeServer ) : base()
			  {
					this.HandshakeServer = handshakeServer;
			  }

			  public override CompletableFuture<Void> Write( object msg )
			  {
					( ( ServerMessage ) msg ).Dispatch( HandshakeServer );
					return CompletableFuture.completedFuture( null );
			  }
		 }
	}

}
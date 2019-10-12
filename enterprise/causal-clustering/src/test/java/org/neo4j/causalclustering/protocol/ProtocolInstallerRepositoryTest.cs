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
namespace Org.Neo4j.causalclustering.protocol
{
	using Test = org.junit.Test;


	using RaftProtocolClientInstallerV1 = Org.Neo4j.causalclustering.core.consensus.protocol.v1.RaftProtocolClientInstallerV1;
	using RaftProtocolServerInstallerV1 = Org.Neo4j.causalclustering.core.consensus.protocol.v1.RaftProtocolServerInstallerV1;
	using VoidPipelineWrapperFactory = Org.Neo4j.causalclustering.handlers.VoidPipelineWrapperFactory;
	using ProtocolStack = Org.Neo4j.causalclustering.protocol.handshake.ProtocolStack;
	using TestProtocols_TestApplicationProtocols = Org.Neo4j.causalclustering.protocol.handshake.TestProtocols_TestApplicationProtocols;
	using TestProtocols_TestModifierProtocols = Org.Neo4j.causalclustering.protocol.handshake.TestProtocols_TestModifierProtocols;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.sameInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class ProtocolInstallerRepositoryTest
	{
		private bool InstanceFieldsInitialized = false;

		public ProtocolInstallerRepositoryTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_clientModifiers = new IList<ModifierProtocolInstaller<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Client>>
			{
				new SnappyClientInstaller(),
				new LZOClientInstaller(),
				new LZ4ClientInstaller(),
				new LZ4HighCompressionClientInstaller(),
				new Rot13ClientInstaller( this )
			};
			_serverModifiers = new IList<ModifierProtocolInstaller<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Server>>
			{
				new SnappyServerInstaller(),
				new LZOServerInstaller(),
				new LZ4ServerInstaller(),
				new LZ4ValidatingServerInstaller(),
				new Rot13ServerInstaller( this )
			};
			_raftProtocolClientInstaller = new RaftProtocolClientInstallerV1.Factory( _pipelineBuilderFactory, NullLogProvider.Instance );
			_raftProtocolServerInstaller = new RaftProtocolServerInstallerV1.Factory( null, _pipelineBuilderFactory, NullLogProvider.Instance );
			_clientRepository = new ProtocolInstallerRepository<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Client>( asList( _raftProtocolClientInstaller ), _clientModifiers );
			_serverRepository = new ProtocolInstallerRepository<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Server>( asList( _raftProtocolServerInstaller ), _serverModifiers );
		}

		 private IList<ModifierProtocolInstaller<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Client>> _clientModifiers;
		 private IList<ModifierProtocolInstaller<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Server>> _serverModifiers;

		 private readonly NettyPipelineBuilderFactory _pipelineBuilderFactory = new NettyPipelineBuilderFactory( VoidPipelineWrapperFactory.VOID_WRAPPER );
		 private RaftProtocolClientInstallerV1.Factory _raftProtocolClientInstaller;
		 private RaftProtocolServerInstallerV1.Factory _raftProtocolServerInstaller;

		 private ProtocolInstallerRepository<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Client> _clientRepository;
		 private ProtocolInstallerRepository<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Server> _serverRepository;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnRaftServerInstaller()
		 public virtual void ShouldReturnRaftServerInstaller()
		 {
			  assertEquals( _raftProtocolServerInstaller.applicationProtocol(), _serverRepository.installerFor(new ProtocolStack(Protocol_ApplicationProtocols.RAFT_1, emptyList())).applicationProtocol() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnRaftClientInstaller()
		 public virtual void ShouldReturnRaftClientInstaller()
		 {
			  assertEquals( _raftProtocolClientInstaller.applicationProtocol(), _clientRepository.installerFor(new ProtocolStack(Protocol_ApplicationProtocols.RAFT_1, emptyList())).applicationProtocol() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnModifierProtocolsForClient()
		 public virtual void ShouldReturnModifierProtocolsForClient()
		 {
			  // given
			  Protocol_ModifierProtocol expected = TestProtocols_TestModifierProtocols.SNAPPY;
			  ProtocolStack protocolStack = new ProtocolStack( Protocol_ApplicationProtocols.RAFT_1, new IList<ModifierProtocol> { expected } );

			  // when
			  ICollection<ICollection<Protocol_ModifierProtocol>> actual = _clientRepository.installerFor( protocolStack ).modifiers();

			  // then
			  assertThat( actual, contains( contains( expected ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnModifierProtocolsForServer()
		 public virtual void ShouldReturnModifierProtocolsForServer()
		 {
			  // given
			  Protocol_ModifierProtocol expected = TestProtocols_TestModifierProtocols.SNAPPY;
			  ProtocolStack protocolStack = new ProtocolStack( Protocol_ApplicationProtocols.RAFT_1, new IList<ModifierProtocol> { expected } );

			  // when
			  ICollection<ICollection<Protocol_ModifierProtocol>> actual = _serverRepository.installerFor( protocolStack ).modifiers();

			  // then
			  assertThat( actual, contains( contains( expected ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnModifierProtocolsForProtocolWithSharedInstallerForClient()
		 public virtual void ShouldReturnModifierProtocolsForProtocolWithSharedInstallerForClient()
		 {
			  // given
			  Protocol_ModifierProtocol expected = TestProtocols_TestModifierProtocols.LZ4_HIGH_COMPRESSION_VALIDATING;
			  TestProtocols_TestModifierProtocols alsoSupported = TestProtocols_TestModifierProtocols.LZ4_HIGH_COMPRESSION;

			  ProtocolStack protocolStack = new ProtocolStack( Protocol_ApplicationProtocols.RAFT_1, new IList<ModifierProtocol> { expected } );

			  // when
			  ICollection<ICollection<Protocol_ModifierProtocol>> actual = _clientRepository.installerFor( protocolStack ).modifiers();

			  // then
			  assertThat( actual, contains( containsInAnyOrder( expected, alsoSupported ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnModifierProtocolsForProtocolWithSharedInstallerForServer()
		 public virtual void ShouldReturnModifierProtocolsForProtocolWithSharedInstallerForServer()
		 {
			  // given
			  Protocol_ModifierProtocol expected = TestProtocols_TestModifierProtocols.LZ4_HIGH_COMPRESSION_VALIDATING;
			  TestProtocols_TestModifierProtocols alsoSupported = TestProtocols_TestModifierProtocols.LZ4_VALIDATING;

			  ProtocolStack protocolStack = new ProtocolStack( Protocol_ApplicationProtocols.RAFT_1, new IList<ModifierProtocol> { expected } );

			  // when
			  ICollection<ICollection<Protocol_ModifierProtocol>> actual = _serverRepository.installerFor( protocolStack ).modifiers();

			  // then
			  assertThat( actual, contains( containsInAnyOrder( expected, alsoSupported ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseDifferentInstancesOfProtocolInstaller()
		 public virtual void ShouldUseDifferentInstancesOfProtocolInstaller()
		 {
			  // given
			  ProtocolStack protocolStack1 = new ProtocolStack( Protocol_ApplicationProtocols.RAFT_1, new IList<ModifierProtocol> { TestProtocols_TestModifierProtocols.SNAPPY } );
			  ProtocolStack protocolStack2 = new ProtocolStack( Protocol_ApplicationProtocols.RAFT_1, new IList<ModifierProtocol> { TestProtocols_TestModifierProtocols.LZO } );

			  // when
			  ProtocolInstaller protocolInstaller1 = _clientRepository.installerFor( protocolStack1 );
			  ProtocolInstaller protocolInstaller2 = _clientRepository.installerFor( protocolStack2 );

			  // then
			  assertThat( protocolInstaller1, not( sameInstance( protocolInstaller2 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void shouldThrowIfAttemptingToCreateInstallerForMultipleModifiersWithSameIdentifier()
		 public virtual void ShouldThrowIfAttemptingToCreateInstallerForMultipleModifiersWithSameIdentifier()
		 {
			  // given
			  ProtocolStack protocolStack = new ProtocolStack( Protocol_ApplicationProtocols.RAFT_1, new IList<ModifierProtocol> { TestProtocols_TestModifierProtocols.SNAPPY, TestProtocols_TestModifierProtocols.LZO } );

			  // when
			  _clientRepository.installerFor( protocolStack );

			  // then throw
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void shouldNotInitialiseIfMultipleInstallersForSameProtocolForServer()
		 public virtual void ShouldNotInitialiseIfMultipleInstallersForSameProtocolForServer()
		 {
			  new ProtocolInstallerRepository<>( asList( _raftProtocolServerInstaller, _raftProtocolServerInstaller ), emptyList() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void shouldNotInitialiseIfMultipleInstallersForSameProtocolForClient()
		 public virtual void ShouldNotInitialiseIfMultipleInstallersForSameProtocolForClient()
		 {
			  new ProtocolInstallerRepository<>( asList( _raftProtocolClientInstaller, _raftProtocolClientInstaller ), emptyList() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void shouldThrowIfUnknownProtocolForServer()
		 public virtual void ShouldThrowIfUnknownProtocolForServer()
		 {
			  _serverRepository.installerFor( new ProtocolStack( TestProtocols_TestApplicationProtocols.RAFT_3, emptyList() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void shouldThrowIfUnknownProtocolForClient()
		 public virtual void ShouldThrowIfUnknownProtocolForClient()
		 {
			  _clientRepository.installerFor( new ProtocolStack( TestProtocols_TestApplicationProtocols.RAFT_3, emptyList() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void shouldThrowIfUnknownModifierProtocol()
		 public virtual void ShouldThrowIfUnknownModifierProtocol()
		 {
			  // given
			  // setup used TestModifierProtocols, doesn't know about production protocols
			  Protocol_ModifierProtocol unknownProtocol = Protocol_ModifierProtocols.COMPRESSION_SNAPPY;

			  // when
			  _serverRepository.installerFor( new ProtocolStack( Protocol_ApplicationProtocols.RAFT_1, new IList<ModifierProtocol> { unknownProtocol } ) );

			  // then throw
		 }

		 // Dummy installers

		 private class SnappyClientInstaller : ModifierProtocolInstaller_BaseClientModifier
		 {
			  internal SnappyClientInstaller() : base("snappy", null, TestProtocols_TestModifierProtocols.SNAPPY)
			  {
			  }
		 }

		 private class LZOClientInstaller : ModifierProtocolInstaller_BaseClientModifier
		 {
			  internal LZOClientInstaller() : base("lzo", null, TestProtocols_TestModifierProtocols.LZO)
			  {
			  }
		 }

		 private class LZ4ClientInstaller : ModifierProtocolInstaller_BaseClientModifier
		 {
			  internal LZ4ClientInstaller() : base("lz4", null, TestProtocols_TestModifierProtocols.LZ4, TestProtocols_TestModifierProtocols.LZ4_VALIDATING)
			  {
			  }
		 }
		 private class LZ4HighCompressionClientInstaller : ModifierProtocolInstaller_BaseClientModifier
		 {
			  internal LZ4HighCompressionClientInstaller() : base("lz4", null, TestProtocols_TestModifierProtocols.LZ4_HIGH_COMPRESSION, TestProtocols_TestModifierProtocols.LZ4_HIGH_COMPRESSION_VALIDATING)
			  {
			  }
		 }

		 private class Rot13ClientInstaller : ModifierProtocolInstaller_BaseClientModifier
		 {
			 private readonly ProtocolInstallerRepositoryTest _outerInstance;

			  internal Rot13ClientInstaller( ProtocolInstallerRepositoryTest outerInstance ) : base( "rot13", null, TestProtocols_TestModifierProtocols.ROT13 )
			  {
				  this._outerInstance = outerInstance;
			  }
		 }

		 private class SnappyServerInstaller : ModifierProtocolInstaller_BaseServerModifier
		 {
			  internal SnappyServerInstaller() : base("snappy", null, TestProtocols_TestModifierProtocols.SNAPPY)
			  {
			  }
		 }

		 private class LZOServerInstaller : ModifierProtocolInstaller_BaseServerModifier
		 {
			  internal LZOServerInstaller() : base("lzo", null, TestProtocols_TestModifierProtocols.LZO)
			  {
			  }
		 }

		 private class LZ4ServerInstaller : ModifierProtocolInstaller_BaseServerModifier
		 {
			  internal LZ4ServerInstaller() : base("lz4", null, TestProtocols_TestModifierProtocols.LZ4, TestProtocols_TestModifierProtocols.LZ4_HIGH_COMPRESSION)
			  {
			  }
		 }

		 private class LZ4ValidatingServerInstaller : ModifierProtocolInstaller_BaseServerModifier
		 {
			  internal LZ4ValidatingServerInstaller() : base("lz4", null, TestProtocols_TestModifierProtocols.LZ4_VALIDATING, TestProtocols_TestModifierProtocols.LZ4_HIGH_COMPRESSION_VALIDATING)
			  {
			  }
		 }

		 private class Rot13ServerInstaller : ModifierProtocolInstaller_BaseServerModifier
		 {
			 private readonly ProtocolInstallerRepositoryTest _outerInstance;

			  internal Rot13ServerInstaller( ProtocolInstallerRepositoryTest outerInstance ) : base( "rot13", null, TestProtocols_TestModifierProtocols.ROT13 )
			  {
				  this._outerInstance = outerInstance;
			  }
		 }
	}

}
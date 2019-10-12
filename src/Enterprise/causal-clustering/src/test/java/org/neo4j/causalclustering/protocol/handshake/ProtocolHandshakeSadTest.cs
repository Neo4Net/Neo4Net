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
namespace Neo4Net.causalclustering.protocol.handshake
{
	using Test = org.junit.Test;


	using Channel = Neo4Net.causalclustering.messaging.Channel;
	using Neo4Net.causalclustering.protocol;

	/// <seealso cref= ProtocolHandshakeHappyTest happy path tests </seealso>
	public class ProtocolHandshakeSadTest
	{
		private bool InstanceFieldsInitialized = false;

		public ProtocolHandshakeSadTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_raftApplicationProtocolRepository = new ApplicationProtocolRepository( TestProtocols_TestApplicationProtocols.values(), _supportedRaftApplicationProtocol );
			_catchupApplicationProtocolRepository = new ApplicationProtocolRepository( TestProtocols_TestApplicationProtocols.values(), _supportedCatchupApplicationProtocol );
			_modifierProtocolRepository = new ModifierProtocolRepository( TestProtocols_TestModifierProtocols.values(), _noModifiers );
		}

		 private ApplicationSupportedProtocols _supportedRaftApplicationProtocol = new ApplicationSupportedProtocols( Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocolCategory.Raft, emptyList() );
		 private ApplicationSupportedProtocols _supportedCatchupApplicationProtocol = new ApplicationSupportedProtocols( Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocolCategory.Catchup, emptyList() );
		 private ICollection<ModifierSupportedProtocols> _noModifiers = emptyList();

		 private ApplicationProtocolRepository _raftApplicationProtocolRepository;
		 private ApplicationProtocolRepository _catchupApplicationProtocolRepository;
		 private ModifierProtocolRepository _modifierProtocolRepository;

		 private HandshakeClient _handshakeClient = new HandshakeClient();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = ClientHandshakeException.class) public void shouldFailHandshakeForUnknownProtocolOnClient() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailHandshakeForUnknownProtocolOnClient()
		 {
			  // given
			  HandshakeServer handshakeServer = new HandshakeServer(_raftApplicationProtocolRepository, _modifierProtocolRepository, new ProtocolHandshakeHappyTest.FakeServerChannel(_handshakeClient)
			 );
			  Channel clientChannel = new ProtocolHandshakeHappyTest.FakeClientChannel( handshakeServer );

			  // when
			  CompletableFuture<ProtocolStack> clientHandshakeFuture = _handshakeClient.initiate( clientChannel, _catchupApplicationProtocolRepository, _modifierProtocolRepository );

			  // then
			  try
			  {
					clientHandshakeFuture.getNow( null );
			  }
			  catch ( CompletionException ex )
			  {
					throw ex.InnerException;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = ServerHandshakeException.class) public void shouldFailHandshakeForUnknownProtocolOnServer() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailHandshakeForUnknownProtocolOnServer()
		 {
			  // given
			  HandshakeServer handshakeServer = new HandshakeServer( _raftApplicationProtocolRepository, _modifierProtocolRepository, new ProtocolHandshakeHappyTest.FakeServerChannel( _handshakeClient ) );
			  Channel clientChannel = new ProtocolHandshakeHappyTest.FakeClientChannel( handshakeServer );

			  // when
			  _handshakeClient.initiate( clientChannel, _catchupApplicationProtocolRepository, _modifierProtocolRepository );
			  CompletableFuture<ProtocolStack> serverHandshakeFuture = handshakeServer.ProtocolStackFuture();

			  // then
			  try
			  {
					serverHandshakeFuture.getNow( null );
			  }
			  catch ( CompletionException ex )
			  {
					throw ex.InnerException;
			  }
		 }
	}

}
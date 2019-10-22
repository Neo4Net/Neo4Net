using System;
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
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Channel = Neo4Net.causalclustering.messaging.Channel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocolCategory.RAFT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.protocol.handshake.TestProtocols_TestApplicationProtocols.RAFT_1;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.protocol.handshake.TestProtocols_TestModifierProtocols.LZ4;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class HandshakeClientEnsureMagicTest
	public class HandshakeClientEnsureMagicTest
	{
		private bool InstanceFieldsInitialized = false;

		public HandshakeClientEnsureMagicTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_applicationProtocolRepository = new ApplicationProtocolRepository( TestProtocols_TestApplicationProtocols.values(), _supportedApplicationProtocol );
		}

		 private CompletableFuture<ProtocolStack> _protocolStackCompletableFuture;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<ClientMessage> data()
		 public static ICollection<ClientMessage> Data()
		 {
			  return asList(new ApplicationProtocolResponse(StatusCode.Success, "protocol", RAFT_1.implementation()), new ModifierProtocolResponse(StatusCode.Success, "modifier", LZ4.implementation()), new SwitchOverResponse(StatusCode.Success)
			 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public ClientMessage message;
		 public ClientMessage Message;

		 private Channel _channel = mock( typeof( Channel ) );

		 private ApplicationSupportedProtocols _supportedApplicationProtocol = new ApplicationSupportedProtocols( RAFT, TestProtocols_TestApplicationProtocols.listVersionsOf( RAFT ) );
		 private ApplicationProtocolRepository _applicationProtocolRepository;
		 private ModifierProtocolRepository _modifierProtocolRepository = new ModifierProtocolRepository( TestProtocols_TestModifierProtocols.values(), emptyList() );

		 private HandshakeClient _client = new HandshakeClient();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _protocolStackCompletableFuture = _client.initiate( _channel, _applicationProtocolRepository, _modifierProtocolRepository );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void shouldThrowIfMagicHasNotBeenSent()
		 public virtual void ShouldThrowIfMagicHasNotBeenSent()
		 {
			  Message.dispatch( _client );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = ClientHandshakeException.class) public void shouldCompleteExceptionallyIfMagicHasNotBeenSent() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCompleteExceptionallyIfMagicHasNotBeenSent()
		 {
			  try
			  {
					Message.dispatch( _client );
			  }
			  catch ( Exception )
			  {
					// swallow
			  }

			  try
			  {
					_protocolStackCompletableFuture.getNow( null );
			  }
			  catch ( CompletionException ex )
			  {
					throw ex.InnerException;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotThrowIfMagicHasBeenSent()
		 public virtual void ShouldNotThrowIfMagicHasBeenSent()
		 {
			  // given
			  InitialMagicMessage.Instance().dispatch(_client);

			  // when
			  Message.dispatch( _client );

			  // then pass
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCompleteExceptionallyIfMagicHasBeenSent()
		 public virtual void ShouldNotCompleteExceptionallyIfMagicHasBeenSent()
		 {
			  // given
			  InitialMagicMessage.Instance().dispatch(_client);

			  // when
			  Message.dispatch( _client );

			  // then future should either not complete exceptionally or do so for non-magic reasons
			  try
			  {
					_protocolStackCompletableFuture.getNow( null );
			  }
			  catch ( CompletionException ex )
			  {
					assertThat( ex.Message.ToLower(), not(containsString("magic")) );
			  }
		 }
	}

}
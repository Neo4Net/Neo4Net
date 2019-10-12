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
namespace Neo4Net.causalclustering.protocol.handshake
{
	using Matchers = org.hamcrest.Matchers;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Channel = Neo4Net.causalclustering.messaging.Channel;
	using Neo4Net.causalclustering.protocol;
	using Iterators = Neo4Net.Helpers.Collection.Iterators;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ApplicationProtocolCategory.RAFT;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class HandshakeServerEnsureMagicTest
	public class HandshakeServerEnsureMagicTest
	{
		private bool InstanceFieldsInitialized = false;

		public HandshakeServerEnsureMagicTest()
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
			_server = new HandshakeServer( _applicationProtocolRepository, _modifierProtocolRepository, _channel );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<ServerMessage> data()
		 public static ICollection<ServerMessage> Data()
		 {
			  return asList(new ApplicationProtocolRequest(RAFT.canonicalName(), Iterators.asSet(1, 2)), new ModifierProtocolRequest(Neo4Net.causalclustering.protocol.Protocol_ModifierProtocolCategory.Compression.canonicalName(), Iterators.asSet("3", "4")), new SwitchOverRequest(RAFT.canonicalName(), 2, emptyList())
			 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public ServerMessage message;
		 public ServerMessage Message;

		 private readonly ApplicationSupportedProtocols _supportedApplicationProtocol = new ApplicationSupportedProtocols( RAFT, TestProtocols_TestApplicationProtocols.listVersionsOf( RAFT ) );

		 private Channel _channel = mock( typeof( Channel ) );
		 private ApplicationProtocolRepository _applicationProtocolRepository;
		 private ModifierProtocolRepository _modifierProtocolRepository = new ModifierProtocolRepository( TestProtocols_TestModifierProtocols.values(), emptyList() );

		 private HandshakeServer _server;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void shouldThrowIfMagicHasNotBeenSent()
		 public virtual void ShouldThrowIfMagicHasNotBeenSent()
		 {
			  Message.dispatch( _server );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = ServerHandshakeException.class) public void shouldCompleteExceptionallyIfMagicHasNotBeenSent() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCompleteExceptionallyIfMagicHasNotBeenSent()
		 {
			  // when
			  try
			  {
					Message.dispatch( _server );
			  }
			  catch ( Exception )
			  {
					// swallow, tested elsewhere
			  }

			  // then future is completed exceptionally
			  try
			  {
					_server.protocolStackFuture().getNow(null);
			  }
			  catch ( CompletionException completion )
			  {
					throw completion.InnerException;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotThrowIfMagicHasBeenSent()
		 public virtual void ShouldNotThrowIfMagicHasBeenSent()
		 {
			  // given
			  InitialMagicMessage.Instance().dispatch(_server);

			  // when
			  Message.dispatch( _server );

			  // then pass
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCompleteExceptionallyIfMagicHasBeenSent()
		 public virtual void ShouldNotCompleteExceptionallyIfMagicHasBeenSent()
		 {
			  // given
			  InitialMagicMessage.Instance().dispatch(_server);

			  // when
			  Message.dispatch( _server );

			  // then future should either not complete exceptionally or do so for non-magic reasons
			  try
			  {
					_server.protocolStackFuture().getNow(null);
			  }
			  catch ( CompletionException ex )
			  {
					assertThat( ex.Message.ToLower(), Matchers.not(Matchers.containsString("magic")) );
			  }
		 }
	}

}
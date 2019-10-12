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
	using Neo4Net.Helpers.Collection;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
	using static Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocol;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
	using static Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocolCategory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
	using static Neo4Net.causalclustering.protocol.Protocol_ModifierProtocolCategory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.handshake.TestProtocols_TestModifierProtocols.ROT13;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.handshake.TestProtocols_TestModifierProtocols.SNAPPY;

	/// <seealso cref= HandshakeClientEnsureMagicTest </seealso>
	public class HandshakeClientTest
	{
		private bool InstanceFieldsInitialized = false;

		public HandshakeClientTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_supportedApplicationProtocol = new ApplicationSupportedProtocols( _applicationProtocolIdentifier, emptyList() );
			_applicationProtocolRepository = new ApplicationProtocolRepository( TestProtocols_TestApplicationProtocols.values(), _supportedApplicationProtocol );
			_modifierProtocolRepository = new ModifierProtocolRepository( TestProtocols_TestModifierProtocols.values(), _supportedModifierProtocols );
			_expectedApplicationProtocol = _applicationProtocolRepository.select( _applicationProtocolIdentifier.canonicalName(), _raftVersion ).get();
		}

		 private HandshakeClient _client = new HandshakeClient();
		 private Channel _channel = mock( typeof( Channel ) );
		 private ApplicationProtocolCategory _applicationProtocolIdentifier = ApplicationProtocolCategory.RAFT;
		 private ApplicationSupportedProtocols _supportedApplicationProtocol;
		 private ICollection<ModifierSupportedProtocols> _supportedModifierProtocols = Stream.of( Neo4Net.causalclustering.protocol.Protocol_ModifierProtocolCategory.values() ).map(id => new ModifierSupportedProtocols(id, emptyList())).collect(Collectors.toList());
		 private ApplicationProtocolRepository _applicationProtocolRepository;
		 private ModifierProtocolRepository _modifierProtocolRepository;
		 private int _raftVersion = TestProtocols_TestApplicationProtocols.latest( ApplicationProtocolCategory.RAFT ).implementation();
		 private ApplicationProtocol _expectedApplicationProtocol;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendInitialMagicOnInitiation()
		 public virtual void ShouldSendInitialMagicOnInitiation()
		 {
			  _client.initiate( _channel, _applicationProtocolRepository, _modifierProtocolRepository );

			  verify( _channel ).write( InitialMagicMessage.Instance() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendApplicationProtocolRequestOnInitiation()
		 public virtual void ShouldSendApplicationProtocolRequestOnInitiation()
		 {
			  _client.initiate( _channel, _applicationProtocolRepository, _modifierProtocolRepository );

			  ApplicationProtocolRequest expectedMessage = new ApplicationProtocolRequest( _applicationProtocolIdentifier.canonicalName(), _applicationProtocolRepository.getAll(_applicationProtocolIdentifier, emptyList()).versions() );

			  verify( _channel ).writeAndFlush( expectedMessage );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendModifierProtocolRequestsOnInitiation()
		 public virtual void ShouldSendModifierProtocolRequestsOnInitiation()
		 {
			  // when
			  _client.initiate( _channel, _applicationProtocolRepository, _modifierProtocolRepository );

			  // then
			  Stream.of( Neo4Net.causalclustering.protocol.Protocol_ModifierProtocolCategory.values() ).forEach(modifierProtocolIdentifier =>
			  {
						  ISet<string> versions = _modifierProtocolRepository.getAll( modifierProtocolIdentifier, emptyList() ).versions();
						  verify( _channel ).write( new ModifierProtocolRequest( modifierProtocolIdentifier.canonicalName(), versions ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExceptionallyCompleteProtocolStackOnReceivingIncorrectMagic()
		 public virtual void ShouldExceptionallyCompleteProtocolStackOnReceivingIncorrectMagic()
		 {
			  // given
			  CompletableFuture<ProtocolStack> protocolStackCompletableFuture = _client.initiate( _channel, _applicationProtocolRepository, _modifierProtocolRepository );

			  // when
			  _client.handle( new InitialMagicMessage( "totally legit" ) );

			  // then
			  AssertCompletedExceptionally( protocolStackCompletableFuture );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptCorrectMagic()
		 public virtual void ShouldAcceptCorrectMagic()
		 {
			  // given
			  CompletableFuture<ProtocolStack> protocolStackCompletableFuture = _client.initiate( _channel, _applicationProtocolRepository, _modifierProtocolRepository );

			  // when
			  _client.handle( InitialMagicMessage.Instance() );

			  // then
			  assertFalse( protocolStackCompletableFuture.Done );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExceptionallyCompleteProtocolStackWhenApplicationProtocolResponseNotSuccessful()
		 public virtual void ShouldExceptionallyCompleteProtocolStackWhenApplicationProtocolResponseNotSuccessful()
		 {
			  // given
			  CompletableFuture<ProtocolStack> protocolStackCompletableFuture = _client.initiate( _channel, _applicationProtocolRepository, _modifierProtocolRepository );
			  _client.handle( InitialMagicMessage.Instance() );

			  // when
			  _client.handle( new ApplicationProtocolResponse( StatusCode.Failure, _applicationProtocolIdentifier.canonicalName(), _raftVersion ) );

			  // then
			  AssertCompletedExceptionally( protocolStackCompletableFuture );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExceptionallyCompleteProtocolStackWhenApplicationProtocolResponseForIncorrectProtocol()
		 public virtual void ShouldExceptionallyCompleteProtocolStackWhenApplicationProtocolResponseForIncorrectProtocol()
		 {
			  // given
			  CompletableFuture<ProtocolStack> protocolStackCompletableFuture = _client.initiate( _channel, _applicationProtocolRepository, _modifierProtocolRepository );
			  _client.handle( InitialMagicMessage.Instance() );

			  // when
			  _client.handle( new ApplicationProtocolResponse( StatusCode.Success, "zab", _raftVersion ) );

			  // then
			  AssertCompletedExceptionally( protocolStackCompletableFuture );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExceptionallyCompleteProtocolStackWhenApplicationProtocolResponseForUnsupportedVersion()
		 public virtual void ShouldExceptionallyCompleteProtocolStackWhenApplicationProtocolResponseForUnsupportedVersion()
		 {
			  // given
			  CompletableFuture<ProtocolStack> protocolStackCompletableFuture = _client.initiate( _channel, _applicationProtocolRepository, _modifierProtocolRepository );
			  _client.handle( InitialMagicMessage.Instance() );

			  // when
			  _client.handle( new ApplicationProtocolResponse( StatusCode.Success, _applicationProtocolIdentifier.canonicalName(), int.MaxValue ) );

			  // then
			  AssertCompletedExceptionally( protocolStackCompletableFuture );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendSwitchOverRequestIfNoModifierProtocolsToRequestOnApplicationProtocolResponse()
		 public virtual void ShouldSendSwitchOverRequestIfNoModifierProtocolsToRequestOnApplicationProtocolResponse()
		 {
			  ModifierProtocolRepository repo = new ModifierProtocolRepository( TestProtocols_TestModifierProtocols.values(), emptyList() );
			  // given
			  CompletableFuture<ProtocolStack> protocolStackCompletableFuture = _client.initiate( _channel, _applicationProtocolRepository, repo );
			  _client.handle( InitialMagicMessage.Instance() );

			  // when
			  _client.handle( new ApplicationProtocolResponse( StatusCode.Success, _applicationProtocolIdentifier.canonicalName(), _raftVersion ) );

			  // then
			  verify( _channel ).writeAndFlush( new SwitchOverRequest( _applicationProtocolIdentifier.canonicalName(), _raftVersion, emptyList() ) );
			  assertFalse( protocolStackCompletableFuture.Done );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSendSwitchOverRequestOnModifierProtocolResponseIfNotAllModifierProtocolResponsesReceived()
		 public virtual void ShouldNotSendSwitchOverRequestOnModifierProtocolResponseIfNotAllModifierProtocolResponsesReceived()
		 {
			  // given
			  CompletableFuture<ProtocolStack> protocolStackCompletableFuture = _client.initiate( _channel, _applicationProtocolRepository, _modifierProtocolRepository );
			  _client.handle( InitialMagicMessage.Instance() );
			  _client.handle( new ApplicationProtocolResponse( StatusCode.Success, _applicationProtocolIdentifier.canonicalName(), _raftVersion ) );

			  // when
			  _client.handle( new ModifierProtocolResponse( StatusCode.Success, Neo4Net.causalclustering.protocol.Protocol_ModifierProtocolCategory.Compression.canonicalName(), "woot" ) );

			  // then
			  verify( _channel, never() ).writeAndFlush(any(typeof(SwitchOverRequest)));
			  assertFalse( protocolStackCompletableFuture.Done );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSendSwitchOverRequestIfApplicationProtocolResponseNotReceivedOnModifierProtocolResponseReceive()
		 public virtual void ShouldNotSendSwitchOverRequestIfApplicationProtocolResponseNotReceivedOnModifierProtocolResponseReceive()
		 {
			  // given
			  CompletableFuture<ProtocolStack> protocolStackCompletableFuture = _client.initiate( _channel, _applicationProtocolRepository, _modifierProtocolRepository );
			  _client.handle( InitialMagicMessage.Instance() );

			  // when
			  _client.handle( new ModifierProtocolResponse( StatusCode.Success, ModifierProtocolCategory.COMPRESSION.canonicalName(), SNAPPY.implementation() ) );

			  // then
			  verify( _channel, never() ).writeAndFlush(any(typeof(SwitchOverRequest)));
			  assertFalse( protocolStackCompletableFuture.Done );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendSwitchOverRequestOnModifierProtocolResponseIfAllModifierProtocolResponsesReceived()
		 public virtual void ShouldSendSwitchOverRequestOnModifierProtocolResponseIfAllModifierProtocolResponsesReceived()
		 {
			  // given
			  CompletableFuture<ProtocolStack> protocolStackCompletableFuture = _client.initiate( _channel, _applicationProtocolRepository, _modifierProtocolRepository );
			  _client.handle( InitialMagicMessage.Instance() );
			  _client.handle( new ApplicationProtocolResponse( StatusCode.Success, _applicationProtocolIdentifier.canonicalName(), _raftVersion ) );

			  // when
			  _client.handle( new ModifierProtocolResponse( StatusCode.Success, ModifierProtocolCategory.COMPRESSION.canonicalName(), SNAPPY.implementation() ) );
			  _client.handle( new ModifierProtocolResponse( StatusCode.Success, Neo4Net.causalclustering.protocol.Protocol_ModifierProtocolCategory.GratuitousObfuscation.canonicalName(), ROT13.implementation() ) );

			  // then
			  IList<Pair<string, string>> switchOverModifierProtocols = new IList<Pair<string, string>> { Pair.of( ModifierProtocolCategory.COMPRESSION.canonicalName(), SNAPPY.implementation() ), Pair.of(ModifierProtocolCategory.GRATUITOUS_OBFUSCATION.canonicalName(), ROT13.implementation()) };
			  verify( _channel ).writeAndFlush( new SwitchOverRequest( _applicationProtocolIdentifier.canonicalName(), _raftVersion, switchOverModifierProtocols ) );
			  assertFalse( protocolStackCompletableFuture.Done );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotIncludeModifierProtocolInSwitchOverRequestIfNotSuccessful()
		 public virtual void ShouldNotIncludeModifierProtocolInSwitchOverRequestIfNotSuccessful()
		 {
			  // given
			  CompletableFuture<ProtocolStack> protocolStackCompletableFuture = _client.initiate( _channel, _applicationProtocolRepository, _modifierProtocolRepository );
			  _client.handle( InitialMagicMessage.Instance() );
			  _client.handle( new ApplicationProtocolResponse( StatusCode.Success, _applicationProtocolIdentifier.canonicalName(), _raftVersion ) );

			  // when
			  _client.handle( new ModifierProtocolResponse( StatusCode.Success, Neo4Net.causalclustering.protocol.Protocol_ModifierProtocolCategory.Compression.canonicalName(), SNAPPY.implementation() ) );
			  _client.handle( new ModifierProtocolResponse( StatusCode.Failure, ModifierProtocolCategory.GRATUITOUS_OBFUSCATION.canonicalName(), ROT13.implementation() ) );

			  // then
			  IList<Pair<string, string>> switchOverModifierProtocols = new IList<Pair<string, string>> { Pair.of( ModifierProtocolCategory.COMPRESSION.canonicalName(), SNAPPY.implementation() ) };
			  verify( _channel ).writeAndFlush( new SwitchOverRequest( _applicationProtocolIdentifier.canonicalName(), _raftVersion, switchOverModifierProtocols ) );
			  assertFalse( protocolStackCompletableFuture.Done );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotIncludeModifierProtocolInSwitchOverRequestIfUnsupportedProtocol()
		 public virtual void ShouldNotIncludeModifierProtocolInSwitchOverRequestIfUnsupportedProtocol()
		 {
			  // given
			  CompletableFuture<ProtocolStack> protocolStackCompletableFuture = _client.initiate( _channel, _applicationProtocolRepository, _modifierProtocolRepository );
			  _client.handle( InitialMagicMessage.Instance() );
			  _client.handle( new ApplicationProtocolResponse( StatusCode.Success, _applicationProtocolIdentifier.canonicalName(), _raftVersion ) );

			  // when
			  _client.handle( new ModifierProtocolResponse( StatusCode.Success, ModifierProtocolCategory.COMPRESSION.canonicalName(), SNAPPY.implementation() ) );
			  _client.handle( new ModifierProtocolResponse( StatusCode.Success, "not a protocol", "not an implementation" ) );

			  // then
			  IList<Pair<string, string>> switchOverModifierProtocols = new IList<Pair<string, string>> { Pair.of( Neo4Net.causalclustering.protocol.Protocol_ModifierProtocolCategory.Compression.canonicalName(), SNAPPY.implementation() ) };
			  verify( _channel ).writeAndFlush( new SwitchOverRequest( _applicationProtocolIdentifier.canonicalName(), _raftVersion, switchOverModifierProtocols ) );
			  assertFalse( protocolStackCompletableFuture.Done );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotIncludeModifierProtocolInSwitchOverRequestIfUnsupportedVersion()
		 public virtual void ShouldNotIncludeModifierProtocolInSwitchOverRequestIfUnsupportedVersion()
		 {
			  // given
			  CompletableFuture<ProtocolStack> protocolStackCompletableFuture = _client.initiate( _channel, _applicationProtocolRepository, _modifierProtocolRepository );
			  _client.handle( InitialMagicMessage.Instance() );
			  _client.handle( new ApplicationProtocolResponse( StatusCode.Success, _applicationProtocolIdentifier.canonicalName(), _raftVersion ) );

			  // when
			  _client.handle( new ModifierProtocolResponse( StatusCode.Success, Neo4Net.causalclustering.protocol.Protocol_ModifierProtocolCategory.Compression.canonicalName(), SNAPPY.implementation() ) );
			  _client.handle( new ModifierProtocolResponse( StatusCode.Success, Neo4Net.causalclustering.protocol.Protocol_ModifierProtocolCategory.GratuitousObfuscation.canonicalName(), "Rearrange the bytes at random" ) );

			  // then
			  IList<Pair<string, string>> switchOverModifierProtocols = new IList<Pair<string, string>> { Pair.of( Neo4Net.causalclustering.protocol.Protocol_ModifierProtocolCategory.Compression.canonicalName(), SNAPPY.implementation() ) };
			  verify( _channel ).writeAndFlush( new SwitchOverRequest( _applicationProtocolIdentifier.canonicalName(), _raftVersion, switchOverModifierProtocols ) );
			  assertFalse( protocolStackCompletableFuture.Done );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExceptionallyCompleteProtocolStackWhenSwitchOverResponseNotSuccess()
		 public virtual void ShouldExceptionallyCompleteProtocolStackWhenSwitchOverResponseNotSuccess()
		 {
			  // given
			  CompletableFuture<ProtocolStack> protocolStackCompletableFuture = _client.initiate( _channel, _applicationProtocolRepository, _modifierProtocolRepository );
			  _client.handle( InitialMagicMessage.Instance() );
			  _client.handle( new ApplicationProtocolResponse( StatusCode.Success, _applicationProtocolIdentifier.canonicalName(), _raftVersion ) );

			  // when
			  _client.handle( new SwitchOverResponse( StatusCode.Failure ) );

			  // then
			  AssertCompletedExceptionally( protocolStackCompletableFuture );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExceptionallyCompleteProtocolStackWhenProtocolStackNotSet()
		 public virtual void ShouldExceptionallyCompleteProtocolStackWhenProtocolStackNotSet()
		 {
			  // given
			  CompletableFuture<ProtocolStack> protocolStackCompletableFuture = _client.initiate( _channel, _applicationProtocolRepository, _modifierProtocolRepository );
			  _client.handle( InitialMagicMessage.Instance() );

			  // when
			  _client.handle( new SwitchOverResponse( StatusCode.Success ) );

			  // then
			  AssertCompletedExceptionally( protocolStackCompletableFuture );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCompleteProtocolStackOnSwitchoverResponse()
		 public virtual void ShouldCompleteProtocolStackOnSwitchoverResponse()
		 {
			  // given
			  ModifierProtocolRepository repo = new ModifierProtocolRepository( TestProtocols_TestModifierProtocols.values(), asList(new ModifierSupportedProtocols(Neo4Net.causalclustering.protocol.Protocol_ModifierProtocolCategory.Compression, emptyList())) );

			  CompletableFuture<ProtocolStack> protocolStackCompletableFuture = _client.initiate( _channel, _applicationProtocolRepository, repo );
			  _client.handle( InitialMagicMessage.Instance() );
			  _client.handle( new ApplicationProtocolResponse( StatusCode.Success, _applicationProtocolIdentifier.canonicalName(), _raftVersion ) );
			  _client.handle( new ModifierProtocolResponse( StatusCode.Success, SNAPPY.category(), SNAPPY.implementation() ) );

			  // when
			  _client.handle( new SwitchOverResponse( StatusCode.Success ) );

			  // then
			  ProtocolStack protocolStack = protocolStackCompletableFuture.getNow( null );
			  assertThat( protocolStack, equalTo( new ProtocolStack( _expectedApplicationProtocol, singletonList( SNAPPY ) ) ) );
		 }

		 private void AssertCompletedExceptionally( CompletableFuture<ProtocolStack> protocolStackCompletableFuture )
		 {
			  assertTrue( protocolStackCompletableFuture.CompletedExceptionally );
			  try
			  {
					protocolStackCompletableFuture.getNow( null );
			  }
			  catch ( CompletionException ex )
			  {
					assertThat( ex.InnerException, instanceOf( typeof( ClientHandshakeException ) ) );
			  }
		 }
	}

}
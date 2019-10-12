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
namespace Org.Neo4j.causalclustering.protocol.handshake
{
	using Matchers = org.hamcrest.Matchers;
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;
	using Mockito = org.mockito.Mockito;


	using Channel = Org.Neo4j.causalclustering.messaging.Channel;
	using Org.Neo4j.Helpers.Collection;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
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
//	import static org.neo4j.causalclustering.protocol.Protocol_ApplicationProtocolCategory.RAFT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
	using static Org.Neo4j.causalclustering.protocol.Protocol_ModifierProtocol;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ModifierProtocolCategory.COMPRESSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ModifierProtocolCategory.GRATUITOUS_OBFUSCATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.handshake.StatusCode.FAILURE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.handshake.StatusCode.SUCCESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.handshake.TestProtocols_TestApplicationProtocols.RAFT_1;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.handshake.TestProtocols_TestModifierProtocols.LZ4;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.handshake.TestProtocols_TestModifierProtocols.LZO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.handshake.TestProtocols_TestModifierProtocols.ROT13;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.handshake.TestProtocols_TestModifierProtocols.SNAPPY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	/// <seealso cref= HandshakeServerEnsureMagicTest </seealso>
	public class HandshakeServerTest
	{
		private bool InstanceFieldsInitialized = false;

		public HandshakeServerTest()
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

		 private Channel _channel = mock( typeof( Channel ) );
		 private ApplicationSupportedProtocols _supportedApplicationProtocol = new ApplicationSupportedProtocols( RAFT, emptyList() );
		 private ICollection<ModifierSupportedProtocols> supportedModifierProtocols = asList(new ModifierSupportedProtocols(COMPRESSION, TestProtocols_TestModifierProtocols.listVersionsOf(COMPRESSION)), new ModifierSupportedProtocols(GRATUITOUS_OBFUSCATION, TestProtocols_TestModifierProtocols.listVersionsOf(GRATUITOUS_OBFUSCATION))
		);
		 private ApplicationProtocolRepository _applicationProtocolRepository = new ApplicationProtocolRepository( TestProtocols_TestApplicationProtocols.values(), _supportedApplicationProtocol );
		 private ModifierProtocolRepository _modifierProtocolRepository = new ModifierProtocolRepository( TestProtocols_TestModifierProtocols.values(), supportedModifierProtocols );

		 private HandshakeServer _server = new HandshakeServer( _applicationProtocolRepository, _modifierProtocolRepository, _channel );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeclineUnallowedApplicationProtocol()
		 public void shouldDeclineUnallowedApplicationProtocol()
		 {
			  // given
			  _server.handle( InitialMagicMessage.Instance() );

			  // when
			  _server.handle( new ApplicationProtocolRequest( TestProtocols_TestApplicationProtocols.CATCHUP_1.category(), asSet(TestProtocols_TestApplicationProtocols.CATCHUP_1.implementation()) ) );

			  // then
			  verify( _channel ).dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExceptionallyCompleteProtocolStackOnUnallowedApplicationProtocol()
		 public void shouldExceptionallyCompleteProtocolStackOnUnallowedApplicationProtocol()
		 {
			  // given
			  _server.handle( InitialMagicMessage.Instance() );

			  // when
			  _server.handle( new ApplicationProtocolRequest( TestProtocols_TestApplicationProtocols.CATCHUP_1.category(), asSet(TestProtocols_TestApplicationProtocols.CATCHUP_1.implementation()) ) );

			  // then
			  AssertExceptionallyCompletedProtocolStackFuture();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDisconnectOnWrongMagicValue()
		 public void shouldDisconnectOnWrongMagicValue()
		 {
			  // when
			  _server.handle( new InitialMagicMessage( "PLAIN_VALUE" ) );

			  // then
			  verify( _channel ).dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExceptionallyCompleteProtocolStackOnWrongMagicValue()
		 public void shouldExceptionallyCompleteProtocolStackOnWrongMagicValue()
		 {
			  // when
			  _server.handle( new InitialMagicMessage( "PLAIN_VALUE" ) );

			  // then
			  AssertExceptionallyCompletedProtocolStackFuture();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptCorrectMagicValue()
		 public void shouldAcceptCorrectMagicValue()
		 {
			  // when
			  _server.handle( InitialMagicMessage.Instance() );

			  // then
			  AssertUnfinished();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendApplicationProtocolResponseForKnownProtocol()
		 public void shouldSendApplicationProtocolResponseForKnownProtocol()
		 {
			  // given
			  ISet<int> versions = asSet( 1, 2, 3 );
			  _server.handle( InitialMagicMessage.Instance() );

			  // when
			  _server.handle( new ApplicationProtocolRequest( RAFT.canonicalName(), versions ) );

			  // then
			  verify( _channel ).writeAndFlush( new ApplicationProtocolResponse( SUCCESS, TestProtocols_TestApplicationProtocols.RAFT_3.category(), TestProtocols_TestApplicationProtocols.RAFT_3.implementation() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCloseConnectionIfKnownApplicationProtocol()
		 public void shouldNotCloseConnectionIfKnownApplicationProtocol()
		 {
			  // given
			  ISet<int> versions = asSet( 1, 2, 3 );
			  _server.handle( InitialMagicMessage.Instance() );

			  // when
			  _server.handle( new ApplicationProtocolRequest( RAFT.canonicalName(), versions ) );

			  // then
			  AssertUnfinished();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendNegativeResponseAndCloseForUnknownApplicationProtocol()
		 public void shouldSendNegativeResponseAndCloseForUnknownApplicationProtocol()
		 {
			  // given
			  ISet<int> versions = asSet( 1, 2, 3 );
			  _server.handle( InitialMagicMessage.Instance() );

			  // when
			  _server.handle( new ApplicationProtocolRequest( "UNKNOWN", versions ) );

			  // then
			  InOrder inOrder = Mockito.inOrder( _channel );
			  inOrder.verify( _channel ).writeAndFlush( ApplicationProtocolResponse.NoProtocol );
			  inOrder.verify( _channel ).dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExceptionallyCompleteProtocolStackForUnknownApplicationProtocol()
		 public void shouldExceptionallyCompleteProtocolStackForUnknownApplicationProtocol()
		 {
			  // given
			  ISet<int> versions = asSet( 1, 2, 3 );
			  _server.handle( InitialMagicMessage.Instance() );

			  // when
			  _server.handle( new ApplicationProtocolRequest( "UNKNOWN", versions ) );

			  // then
			  AssertExceptionallyCompletedProtocolStackFuture();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendModifierProtocolResponseForGivenProtocol()
		 public void shouldSendModifierProtocolResponseForGivenProtocol()
		 {
			  // given
			  ISet<string> versions = asSet( TestProtocols_TestModifierProtocols.allVersionsOf( COMPRESSION ) );
			  _server.handle( InitialMagicMessage.Instance() );

			  // when
			  _server.handle( new ModifierProtocolRequest( COMPRESSION.canonicalName(), versions ) );

			  // then
			  ModifierProtocol expected = TestProtocols_TestModifierProtocols.latest( COMPRESSION );
			  verify( _channel ).writeAndFlush( new ModifierProtocolResponse( SUCCESS, expected.category(), expected.implementation() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCloseConnectionForGivenModifierProtocol()
		 public void shouldNotCloseConnectionForGivenModifierProtocol()
		 {
			  // given
			  ISet<string> versions = asSet( SNAPPY.implementation(), LZO.implementation(), LZ4.implementation() );
			  _server.handle( InitialMagicMessage.Instance() );

			  // when
			  _server.handle( new ModifierProtocolRequest( COMPRESSION.canonicalName(), versions ) );

			  // then
			  AssertUnfinished();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendFailModifierProtocolResponseForUnknownVersion()
		 public void shouldSendFailModifierProtocolResponseForUnknownVersion()
		 {
			  // given
			  ISet<string> versions = asSet( "Not a real protocol" );
			  _server.handle( InitialMagicMessage.Instance() );

			  // when
			  string protocolName = COMPRESSION.canonicalName();
			  _server.handle( new ModifierProtocolRequest( protocolName, versions ) );

			  // then
			  verify( _channel ).writeAndFlush( new ModifierProtocolResponse( FAILURE, protocolName, "" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCloseConnectionIfUnknownModifierProtocolVersion()
		 public void shouldNotCloseConnectionIfUnknownModifierProtocolVersion()
		 {
			  // given
			  ISet<string> versions = asSet( "not a real algorithm" );
			  _server.handle( InitialMagicMessage.Instance() );

			  // when
			  string protocolName = COMPRESSION.canonicalName();
			  _server.handle( new ModifierProtocolRequest( protocolName, versions ) );

			  // then
			  AssertUnfinished();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendFailModifierProtocolResponseForUnknownProtocol()
		 public void shouldSendFailModifierProtocolResponseForUnknownProtocol()
		 {
			  // given
			  ISet<string> versions = asSet( SNAPPY.implementation(), LZO.implementation(), LZ4.implementation() );
			  _server.handle( InitialMagicMessage.Instance() );

			  // when
			  string protocolName = "let's just randomly reorder all the bytes";
			  _server.handle( new ModifierProtocolRequest( protocolName, versions ) );

			  // then
			  verify( _channel ).writeAndFlush( new ModifierProtocolResponse( FAILURE, protocolName, "" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCloseConnectionIfUnknownModifierProtocol()
		 public void shouldNotCloseConnectionIfUnknownModifierProtocol()
		 {
			  // given
			  ISet<string> versions = asSet( SNAPPY.implementation(), LZO.implementation(), LZ4.implementation() );
			  _server.handle( InitialMagicMessage.Instance() );

			  // when
			  string protocolName = "let's just randomly reorder all the bytes";
			  _server.handle( new ModifierProtocolRequest( protocolName, versions ) );

			  // then
			  AssertUnfinished();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendFailureOnUnknownProtocolSwitchOver()
		 public void shouldSendFailureOnUnknownProtocolSwitchOver()
		 {
			  // given
			  int version = 1;
			  string unknownProtocolName = "UNKNOWN";
			  _server.handle( InitialMagicMessage.Instance() );
			  _server.handle( new ApplicationProtocolRequest( unknownProtocolName, asSet( version ) ) );

			  // when
			  _server.handle( new SwitchOverRequest( unknownProtocolName, version, emptyList() ) );

			  // then
			  InOrder inOrder = Mockito.inOrder( _channel );
			  inOrder.verify( _channel ).writeAndFlush( new SwitchOverResponse( FAILURE ) );
			  inOrder.verify( _channel ).dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExceptionallyCompleteProtocolStackOnUnknownProtocolSwitchOver()
		 public void shouldExceptionallyCompleteProtocolStackOnUnknownProtocolSwitchOver()
		 {
			  // given
			  int version = 1;
			  string unknownProtocolName = "UNKNOWN";
			  _server.handle( InitialMagicMessage.Instance() );
			  _server.handle( new ApplicationProtocolRequest( unknownProtocolName, asSet( version ) ) );

			  // when
			  _server.handle( new SwitchOverRequest( unknownProtocolName, version, emptyList() ) );

			  // then
			  AssertExceptionallyCompletedProtocolStackFuture();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendFailureIfSwitchOverBeforeNegotiation()
		 public void shouldSendFailureIfSwitchOverBeforeNegotiation()
		 {
			  // given
			  int version = 1;
			  _server.handle( InitialMagicMessage.Instance() );

			  // when
			  _server.handle( new SwitchOverRequest( RAFT_1.category(), version, emptyList() ) );

			  // then
			  InOrder inOrder = Mockito.inOrder( _channel );
			  inOrder.verify( _channel ).writeAndFlush( new SwitchOverResponse( FAILURE ) );
			  inOrder.verify( _channel ).dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExceptionallyCompleteProtocolStackIfSwitchOverBeforeNegotiation()
		 public void shouldExceptionallyCompleteProtocolStackIfSwitchOverBeforeNegotiation()
		 {
			  // given
			  int version = 1;
			  _server.handle( InitialMagicMessage.Instance() );

			  // when
			  _server.handle( new SwitchOverRequest( RAFT_1.category(), version, emptyList() ) );

			  // then
			  AssertExceptionallyCompletedProtocolStackFuture();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendFailureIfSwitchOverDiffersFromNegotiatedProtocol()
		 public void shouldSendFailureIfSwitchOverDiffersFromNegotiatedProtocol()
		 {
			  // given
			  int version = 1;
			  _server.handle( InitialMagicMessage.Instance() );
			  _server.handle( new ApplicationProtocolRequest( RAFT.canonicalName(), asSet(version) ) );

			  // when
			  _server.handle( new SwitchOverRequest( RAFT_1.category(), version + 1, emptyList() ) );

			  // then
			  InOrder inOrder = Mockito.inOrder( _channel );
			  inOrder.verify( _channel ).writeAndFlush( new SwitchOverResponse( FAILURE ) );
			  inOrder.verify( _channel ).dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExceptionallyCompleteProtocolStackIfSwitchOverDiffersFromNegotiatedProtocol()
		 public void shouldExceptionallyCompleteProtocolStackIfSwitchOverDiffersFromNegotiatedProtocol()
		 {
			  // given
			  int version = 1;
			  _server.handle( InitialMagicMessage.Instance() );
			  _server.handle( new ApplicationProtocolRequest( RAFT.canonicalName(), asSet(version) ) );

			  // when
			  _server.handle( new SwitchOverRequest( RAFT_1.category(), version + 1, emptyList() ) );

			  // then
			  AssertExceptionallyCompletedProtocolStackFuture();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendFailureIfSwitchOverDiffersByNameFromNegotiatedModifierProtocol()
		 public void shouldSendFailureIfSwitchOverDiffersByNameFromNegotiatedModifierProtocol()
		 {
			  // given
			  string modifierVersion = ROT13.implementation();
			  int applicationVersion = 1;
			  _server.handle( InitialMagicMessage.Instance() );
			  _server.handle( new ApplicationProtocolRequest( RAFT.canonicalName(), asSet(applicationVersion) ) );
			  _server.handle( new ModifierProtocolRequest( COMPRESSION.canonicalName(), asSet(modifierVersion) ) );

			  // when
			  _server.handle( new SwitchOverRequest( RAFT.canonicalName(), applicationVersion, new IList<Pair<string, string>> { Pair.of(GRATUITOUS_OBFUSCATION.canonicalName(), modifierVersion) } ) );

			  // then
			  InOrder inOrder = Mockito.inOrder( _channel );
			  inOrder.verify( _channel ).writeAndFlush( new SwitchOverResponse( FAILURE ) );
			  inOrder.verify( _channel ).dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExceptionallyCompleteProtocolStackIfSwitchOverDiffersByNameFromNegotiatedModifiedProtocol()
		 public void shouldExceptionallyCompleteProtocolStackIfSwitchOverDiffersByNameFromNegotiatedModifiedProtocol()
		 {
			  // given
			  string modifierVersion = ROT13.implementation();
			  int applicationVersion = 1;
			  _server.handle( InitialMagicMessage.Instance() );
			  _server.handle( new ApplicationProtocolRequest( RAFT.canonicalName(), asSet(applicationVersion) ) );
			  _server.handle( new ModifierProtocolRequest( COMPRESSION.canonicalName(), asSet(modifierVersion) ) );

			  // when
			  _server.handle( new SwitchOverRequest( RAFT.canonicalName(), applicationVersion, new IList<Pair<string, string>> { Pair.of(GRATUITOUS_OBFUSCATION.canonicalName(), modifierVersion) } ) );

			  // then
			  AssertExceptionallyCompletedProtocolStackFuture();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendFailureIfSwitchOverChangesOrderOfModifierProtocols()
		 public void shouldSendFailureIfSwitchOverChangesOrderOfModifierProtocols()
		 {
			  // given
			  int version = 1;
			  _server.handle( InitialMagicMessage.Instance() );
			  _server.handle( new ApplicationProtocolRequest( RAFT.canonicalName(), asSet(version) ) );
			  _server.handle( new ModifierProtocolRequest( COMPRESSION.canonicalName(), asSet(SNAPPY.implementation()) ) );
			  _server.handle( new ModifierProtocolRequest( GRATUITOUS_OBFUSCATION.canonicalName(), asSet(ROT13.implementation()) ) );

			  // when
			  _server.handle( new SwitchOverRequest( RAFT.canonicalName(), version, new IList<Pair<string, string>> { Pair.of(GRATUITOUS_OBFUSCATION.canonicalName(), ROT13.implementation()), Pair.of(COMPRESSION.canonicalName(), SNAPPY.implementation()) } ) );

			  // then
			  InOrder inOrder = Mockito.inOrder( _channel );
			  inOrder.verify( _channel ).writeAndFlush( new SwitchOverResponse( FAILURE ) );
			  inOrder.verify( _channel ).dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExceptionallyCompleteProtocolStackIfSwitchOverChangesOrderOfModifierProtocols()
		 public void shouldExceptionallyCompleteProtocolStackIfSwitchOverChangesOrderOfModifierProtocols()
		 {
			  // given
			  int version = 1;
			  _server.handle( InitialMagicMessage.Instance() );
			  _server.handle( new ApplicationProtocolRequest( RAFT.canonicalName(), asSet(version) ) );
			  _server.handle( new ModifierProtocolRequest( COMPRESSION.canonicalName(), asSet(SNAPPY.implementation()) ) );
			  _server.handle( new ModifierProtocolRequest( GRATUITOUS_OBFUSCATION.canonicalName(), asSet(ROT13.implementation()) ) );

			  // when
			  _server.handle( new SwitchOverRequest( RAFT.canonicalName(), version, new IList<Pair<string, string>> { Pair.of(GRATUITOUS_OBFUSCATION.canonicalName(), ROT13.implementation()), Pair.of(COMPRESSION.canonicalName(), SNAPPY.implementation()) } ) );

			  // then
			  AssertExceptionallyCompletedProtocolStackFuture();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendFailureIfSwitchOverDiffersByVersionFromNegotiatedModifierProtocol()
		 public void shouldSendFailureIfSwitchOverDiffersByVersionFromNegotiatedModifierProtocol()
		 {
			  // given
			  int version = 1;
			  _server.handle( InitialMagicMessage.Instance() );
			  _server.handle( new ApplicationProtocolRequest( RAFT.canonicalName(), asSet(version) ) );
			  _server.handle( new ModifierProtocolRequest( COMPRESSION.canonicalName(), asSet(SNAPPY.implementation()) ) );

			  // when
			  _server.handle( new SwitchOverRequest( RAFT_1.category(), version, new IList<Pair<string, string>> { Pair.of(COMPRESSION.canonicalName(), LZ4.implementation()) } ) );

			  // then
			  InOrder inOrder = Mockito.inOrder( _channel );
			  inOrder.verify( _channel ).writeAndFlush( new SwitchOverResponse( FAILURE ) );
			  inOrder.verify( _channel ).dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExceptionallyCompleteProtocolStackIfSwitchOverDiffersByVersionFromNegotiatedModifiedProtocol()
		 public void shouldExceptionallyCompleteProtocolStackIfSwitchOverDiffersByVersionFromNegotiatedModifiedProtocol()
		 {
			  // given
			  int version = 1;
			  _server.handle( InitialMagicMessage.Instance() );
			  _server.handle( new ApplicationProtocolRequest( RAFT.canonicalName(), asSet(version) ) );
			  _server.handle( new ModifierProtocolRequest( COMPRESSION.canonicalName(), asSet(SNAPPY.implementation()) ) );

			  // when
			  _server.handle( new SwitchOverRequest( RAFT_1.category(), version, new IList<Pair<string, string>> { Pair.of(COMPRESSION.canonicalName(), LZ4.implementation()) } ) );

			  // then
			  AssertExceptionallyCompletedProtocolStackFuture();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCompleteProtocolStackOnSuccessfulSwitchOverWithNoModifierProtocols()
		 public void shouldCompleteProtocolStackOnSuccessfulSwitchOverWithNoModifierProtocols()
		 {
			  // given
			  int version = 1;
			  _server.handle( InitialMagicMessage.Instance() );
			  _server.handle( new ApplicationProtocolRequest( RAFT.canonicalName(), asSet(version) ) );

			  // when
			  _server.handle( new SwitchOverRequest( RAFT_1.category(), version, emptyList() ) );

			  // then
			  verify( _channel ).writeAndFlush( InitialMagicMessage.Instance() );
			  verify( _channel ).writeAndFlush( new SwitchOverResponse( SUCCESS ) );
			  ProtocolStack protocolStack = _server.protocolStackFuture().getNow(null);
			  assertThat( protocolStack, equalTo( new ProtocolStack( RAFT_1, emptyList() ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCompleteProtocolStackOnSuccessfulSwitchOverWithModifierProtocols()
		 public void shouldCompleteProtocolStackOnSuccessfulSwitchOverWithModifierProtocols()
		 {
			  // given
			  _server.handle( InitialMagicMessage.Instance() );
			  _server.handle( new ApplicationProtocolRequest( RAFT.canonicalName(), asSet(RAFT_1.implementation()) ) );
			  _server.handle( new ModifierProtocolRequest( COMPRESSION.canonicalName(), asSet(SNAPPY.implementation()) ) );
			  _server.handle( new ModifierProtocolRequest( GRATUITOUS_OBFUSCATION.canonicalName(), asSet(ROT13.implementation()) ) );

			  // when
			  IList<Pair<string, string>> modifierRequest = new IList<Pair<string, string>> { Pair.of( SNAPPY.category(), SNAPPY.implementation() ), Pair.of(ROT13.category(), ROT13.implementation()) };
			  _server.handle( new SwitchOverRequest( RAFT_1.category(), RAFT_1.implementation(), modifierRequest ) );

			  // then
			  verify( _channel ).writeAndFlush( InitialMagicMessage.Instance() );
			  verify( _channel ).writeAndFlush( new SwitchOverResponse( SUCCESS ) );
			  ProtocolStack protocolStack = _server.protocolStackFuture().getNow(null);
			  IList<ModifierProtocol> modifiers = new IList<ModifierProtocol> { SNAPPY, ROT13 };
			  assertThat( protocolStack, equalTo( new ProtocolStack( RAFT_1, modifiers ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCompleteProtocolStackOnSuccessfulSwitchOverWithConfiguredModifierProtocols()
		 public void shouldCompleteProtocolStackOnSuccessfulSwitchOverWithConfiguredModifierProtocols()
		 {
			  // given
			  ISet<string> requestedVersions = asSet( TestProtocols_TestModifierProtocols.allVersionsOf( COMPRESSION ) );
			  string expectedNegotiatedVersion = SNAPPY.implementation();
			  IList<string> configuredVersions = singletonList( expectedNegotiatedVersion );

			  IList<ModifierSupportedProtocols> supportedModifierProtocols = asList( new ModifierSupportedProtocols( COMPRESSION, configuredVersions ) );

			  ModifierProtocolRepository modifierProtocolRepository = new ModifierProtocolRepository( TestProtocols_TestModifierProtocols.values(), supportedModifierProtocols );

			  HandshakeServer server = new HandshakeServer( _applicationProtocolRepository, modifierProtocolRepository, _channel );

			  server.Handle( InitialMagicMessage.Instance() );
			  server.Handle( new ApplicationProtocolRequest( RAFT.canonicalName(), asSet(RAFT_1.implementation()) ) );
			  server.Handle( new ModifierProtocolRequest( COMPRESSION.canonicalName(), requestedVersions ) );

			  // when
			  IList<Pair<string, string>> modifierRequest = new IList<Pair<string, string>> { Pair.of( SNAPPY.category(), SNAPPY.implementation() ) };
			  server.Handle( new SwitchOverRequest( RAFT_1.category(), RAFT_1.implementation(), modifierRequest ) );

			  // then
			  verify( _channel ).writeAndFlush( InitialMagicMessage.Instance() );
			  verify( _channel ).writeAndFlush( new SwitchOverResponse( SUCCESS ) );
			  ProtocolStack protocolStack = server.ProtocolStackFuture().getNow(null);
			  IList<ModifierProtocol> modifiers = new IList<ModifierProtocol> { SNAPPY };
			  assertThat( protocolStack, equalTo( new ProtocolStack( RAFT_1, modifiers ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSuccessfullySwitchOverWhenServerHasConfiguredRaftVersions()
		 public void shouldSuccessfullySwitchOverWhenServerHasConfiguredRaftVersions()
		 {
			  // given
			  ISet<int> requestedVersions = asSet( TestProtocols_TestApplicationProtocols.allVersionsOf( RAFT ) );
			  int? expectedNegotiatedVersion = 1;
			  ApplicationProtocolRepository applicationProtocolRepository = new ApplicationProtocolRepository( TestProtocols_TestApplicationProtocols.values(), new ApplicationSupportedProtocols(RAFT, singletonList(expectedNegotiatedVersion)) );

			  HandshakeServer server = new HandshakeServer( applicationProtocolRepository, _modifierProtocolRepository, _channel );

			  server.Handle( InitialMagicMessage.Instance() );
			  server.Handle( new ApplicationProtocolRequest( RAFT.canonicalName(), requestedVersions ) );

			  // when
			  server.Handle( new SwitchOverRequest( RAFT_1.category(), expectedNegotiatedVersion.Value, emptyList() ) );

			  // then
			  verify( _channel ).writeAndFlush( InitialMagicMessage.Instance() );
			  verify( _channel ).writeAndFlush( new SwitchOverResponse( SUCCESS ) );
			  ProtocolStack protocolStack = server.ProtocolStackFuture().getNow(null);
			  ProtocolStack expectedProtocolStack = new ProtocolStack( applicationProtocolRepository.Select( RAFT.canonicalName(), expectedNegotiatedVersion.Value ).get(), emptyList() );
			  assertThat( protocolStack, equalTo( expectedProtocolStack ) );
		 }

		 private void assertUnfinished()
		 {
			  verify( _channel, never() ).dispose();
			  assertFalse( _server.protocolStackFuture().Done );
		 }

		 private void assertExceptionallyCompletedProtocolStackFuture()
		 {
			  assertTrue( _server.protocolStackFuture().CompletedExceptionally );
			  try
			  {
					_server.protocolStackFuture().getNow(null);
			  }
			  catch ( CompletionException ex )
			  {
					assertThat( ex.InnerException, Matchers.instanceOf( typeof( ServerHandshakeException ) ) );
			  }
		 }
	}

}
using System;
using System.Threading;

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
namespace Neo4Net.com
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using ArgumentCaptor = org.mockito.ArgumentCaptor;
	using Answer = org.mockito.stubbing.Answer;

	using ResponseUnpacker = Neo4Net.com.storecopy.ResponseUnpacker;
	using Neo4Net.Collections.Helpers;
	using MismatchingStoreIdException = Neo4Net.Kernel.impl.store.MismatchingStoreIdException;
	using CommittedTransactionRepresentation = Neo4Net.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using StoreId = Neo4Net.Kernel.Api.StorageEngine.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Thread.sleep;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.com.MadeUpServer.FRAME_LENGTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.com.StoreIdTestFactory.newStoreIdForCurrentVersion;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.com.TxChecksumVerifier_Fields.ALWAYS_MATCH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.com.storecopy.ResponseUnpacker_TxHandler_Fields.NO_OP_TX_HANDLER;

	public class CommunicationIT
	{
		 private const sbyte INTERNAL_PROTOCOL_VERSION = 0;
		 private const sbyte APPLICATION_PROTOCOL_VERSION = 0;
		 private static readonly int _port = PortAuthority.allocatePort();

		 private readonly LifeSupport _life = new LifeSupport();
		 private StoreId _storeIdToUse;
		 private Builder _builder;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void doBefore()
		 public virtual void DoBefore()
		 {
			  _storeIdToUse = newStoreIdForCurrentVersion();
			  _builder = new Builder( this );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void shutdownLife()
		 public virtual void ShutdownLife()
		 {
			  _life.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void clientGetResponseFromServerViaComLayer() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ClientGetResponseFromServerViaComLayer()
		 {
			  MadeUpServerImplementation serverImplementation = new MadeUpServerImplementation( _storeIdToUse );
			  MadeUpServer server = _builder.server( serverImplementation );
			  MadeUpClient client = _builder.client();
			  AddToLifeAndStart( server, client );

			  int value1 = 10;
			  int value2 = 5;
			  Response<int> response = client.Multiply( 10, 5 );
			  WaitUntilResponseHasBeenWritten( server, 1000 );
			  assertEquals( ( int? )( value1 * value2 ), response.ResponseConflict() );
			  assertTrue( serverImplementation.GotCalled() );
			  assertTrue( server.ResponseHasBeenWritten() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void waitUntilResponseHasBeenWritten(MadeUpServer server, int maxTime) throws Exception
		 private void WaitUntilResponseHasBeenWritten( MadeUpServer server, int maxTime )
		 {
			  long time = currentTimeMillis();
			  while ( !server.ResponseHasBeenWritten() && currentTimeMillis() - time < maxTime )
			  {
					Thread.Sleep( 50 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.Neo4Net.kernel.impl.store.MismatchingStoreIdException.class) public void makeSureClientStoreIdsMustMatch()
		 public virtual void MakeSureClientStoreIdsMustMatch()
		 {
			  MadeUpServer server = _builder.server();
			  MadeUpClient client = _builder.storeId( newStoreIdForCurrentVersion( 10, 10, 10, 10 ) ).client();
			  AddToLifeAndStart( server, client );

			  client.Multiply( 1, 2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.Neo4Net.kernel.impl.store.MismatchingStoreIdException.class) public void makeSureServerStoreIdsMustMatch()
		 public virtual void MakeSureServerStoreIdsMustMatch()
		 {
			  MadeUpServer server = _builder.storeId( newStoreIdForCurrentVersion( 10, 10, 10, 10 ) ).server();
			  MadeUpClient client = _builder.client();
			  AddToLifeAndStart( server, client );

			  client.Multiply( 1, 2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureClientCanStreamBigData()
		 public virtual void MakeSureClientCanStreamBigData()
		 {
			  MadeUpServer server = _builder.server();
			  MadeUpClient client = _builder.client();
			  AddToLifeAndStart( server, client );

			  client.FetchDataStream( new ToAssertionWriter(), FRAME_LENGTH * 3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void clientThrowsServerSideErrorMidwayThroughStreaming()
		 public virtual void ClientThrowsServerSideErrorMidwayThroughStreaming()
		 {
			  const string failureMessage = "Just failing";
			  MadeUpServerImplementation serverImplementation = new MadeUpServerImplementationAnonymousInnerClass( this, _storeIdToUse, failureMessage );
			  MadeUpServer server = _builder.server( serverImplementation );
			  MadeUpClient client = _builder.client();
			  AddToLifeAndStart( server, client );

			  try
			  {
					client.FetchDataStream( new ToAssertionWriter(), FRAME_LENGTH * 2 );
					fail( "Should have thrown " + typeof( MadeUpException ).Name );
			  }
			  catch ( MadeUpException e )
			  {
					assertEquals( failureMessage, e.Message );
			  }
		 }

		 private class MadeUpServerImplementationAnonymousInnerClass : MadeUpServerImplementation
		 {
			 private readonly CommunicationIT _outerInstance;

			 private string _failureMessage;

			 public MadeUpServerImplementationAnonymousInnerClass( CommunicationIT outerInstance, StoreId storeIdToUse, string failureMessage ) : base( storeIdToUse )
			 {
				 this.outerInstance = outerInstance;
				 this._failureMessage = failureMessage;
			 }

			 public override Response<Void> fetchDataStream( MadeUpWriter writer, int dataSize )
			 {
				  writer.Write( new FailingByteChannel( dataSize, _failureMessage ) );
				  return new TransactionStreamResponse<Void>( null, _outerInstance.storeIdToUse, TransactionStream_Fields.Empty, ResourceReleaser_Fields.NoOp );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void communicateBetweenJvms()
		 public virtual void CommunicateBetweenJvms()
		 {
			  ServerInterface server = _builder.serverInOtherJvm( _port );
			  server.AwaitStarted();
			  MadeUpClient client = _builder.port( _port ).client();
			  _life.add( client );
			  _life.start();

			  assertEquals( ( int? )( 9 * 5 ), client.Multiply( 9, 5 ).response() );
			  client.FetchDataStream( new ToAssertionWriter(), 1024 * 1024 * 3 );

			  server.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwingServerSideExceptionBackToClient()
		 public virtual void ThrowingServerSideExceptionBackToClient()
		 {
			  MadeUpServer server = _builder.server();
			  MadeUpClient client = _builder.client();
			  AddToLifeAndStart( server, client );

			  string exceptionMessage = "The message";
			  try
			  {
					client.ThrowException( exceptionMessage );
					fail( "Should have thrown " + typeof( MadeUpException ).Name );
			  }
			  catch ( MadeUpException e )
			  { // Good
					assertEquals( exceptionMessage, e.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void applicationProtocolVersionsMustMatch()
		 public virtual void ApplicationProtocolVersionsMustMatch()
		 {
			  MadeUpServer server = _builder.applicationProtocolVersion( ( sbyte )( APPLICATION_PROTOCOL_VERSION + 1 ) ).server();
			  MadeUpClient client = _builder.client();
			  AddToLifeAndStart( server, client );

			  try
			  {
					client.Multiply( 10, 20 );
					fail( "Shouldn't be able to communicate with different application protocol versions" );
			  }
			  catch ( IllegalProtocolVersionException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void applicationProtocolVersionsMustMatchMultiJvm()
		 public virtual void ApplicationProtocolVersionsMustMatchMultiJvm()
		 {
			  ServerInterface server = _builder.applicationProtocolVersion( ( sbyte )( APPLICATION_PROTOCOL_VERSION + 1 ) ).serverInOtherJvm( _port );
			  server.AwaitStarted();
			  MadeUpClient client = _builder.port( _port ).client();
			  _life.add( client );
			  _life.start();

			  try
			  {
					client.Multiply( 10, 20 );
					fail( "Shouldn't be able to communicate with different application protocol versions" );
			  }
			  catch ( IllegalProtocolVersionException )
			  {
			  }

			  server.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void internalProtocolVersionsMustMatch()
		 public virtual void InternalProtocolVersionsMustMatch()
		 {
			  MadeUpServer server = _builder.internalProtocolVersion( ( sbyte ) 1 ).server();
			  MadeUpClient client = _builder.internalProtocolVersion( ( sbyte ) 2 ).client();
			  AddToLifeAndStart( server, client );

			  try
			  {
					client.Multiply( 10, 20 );
					fail( "Shouldn't be able to communicate with different application protocol versions" );
			  }
			  catch ( IllegalProtocolVersionException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void internalProtocolVersionsMustMatchMultiJvm()
		 public virtual void InternalProtocolVersionsMustMatchMultiJvm()
		 {
			  ServerInterface server = _builder.internalProtocolVersion( ( sbyte ) 1 ).serverInOtherJvm( _port );
			  server.AwaitStarted();
			  MadeUpClient client = _builder.port( _port ).internalProtocolVersion( ( sbyte ) 2 ).client();
			  _life.add( client );
			  _life.start();

			  try
			  {
					client.Multiply( 10, 20 );
					fail( "Shouldn't be able to communicate with different application protocol versions" );
			  }
			  catch ( IllegalProtocolVersionException )
			  {
			  }

			  server.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void serverStopsStreamingToDeadClient() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ServerStopsStreamingToDeadClient()
		 {
			  MadeUpServer server = _builder.server();
			  MadeUpClient client = _builder.client();
			  AddToLifeAndStart( server, client );

			  int failAtSize = FRAME_LENGTH / 1024;
			  ClientCrashingWriter writer = new ClientCrashingWriter( client, failAtSize );
			  try
			  {
					client.FetchDataStream( writer, FRAME_LENGTH * 100 );
					assertTrue( writer.SizeRead >= failAtSize );
					fail( "Should fail in the middle" );
			  }
			  catch ( ComException )
			  { // Expected
			  }
			  assertTrue( writer.SizeRead >= failAtSize );

			  long maxWaitUntil = DateTimeHelper.CurrentUnixTimeMillis() + 60_000L;
			  while ( !server.ResponseFailureEncountered() && DateTimeHelper.CurrentUnixTimeMillis() < maxWaitUntil )
			  {
					sleep( 100 );
			  }
			  assertTrue( "Failure writing the response should have been encountered", server.ResponseFailureEncountered() );
			  assertFalse( "Response shouldn't have been successful", server.ResponseHasBeenWritten() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void serverContextVerificationCanThrowException()
		 public virtual void ServerContextVerificationCanThrowException()
		 {
			  const string failureMessage = "I'm failing";
			  TxChecksumVerifier failingVerifier = ( txId, checksum ) =>
			  {
				throw new FailingException( failureMessage );
			  };

			  MadeUpServer server = _builder.verifier( failingVerifier ).server();
			  MadeUpClient client = _builder.client();
			  AddToLifeAndStart( server, client );

			  try
			  {
					client.Multiply( 10, 5 );
					fail( "Should have failed" );
			  }
			  catch ( Exception )
			  { // Good
					// TODO catch FailingException instead of Exception and make Server throw the proper
					// one instead of getting a "channel closed".
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void clientCanReadChunkSizeBiggerThanItsOwn()
		 public virtual void ClientCanReadChunkSizeBiggerThanItsOwn()
		 { // Given that frameLength is the same for both client and server.
			  int serverChunkSize = 20000;
			  int clientChunkSize = serverChunkSize / 10;
			  MadeUpServer server = _builder.chunkSize( serverChunkSize ).server();
			  MadeUpClient client = _builder.chunkSize( clientChunkSize ).client();

			  AddToLifeAndStart( server, client );

			  // Tell server to stream data occupying roughly two chunks. The chunks
			  // from server are 10 times bigger than the clients chunk size.
			  client.FetchDataStream( new ToAssertionWriter(), serverChunkSize * 2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void serverCanReadChunkSizeBiggerThanItsOwn()
		 public virtual void ServerCanReadChunkSizeBiggerThanItsOwn()
		 { // Given that frameLength is the same for both client and server.
			  int serverChunkSize = 1000;
			  int clientChunkSize = serverChunkSize * 10;
			  MadeUpServer server = _builder.chunkSize( serverChunkSize ).server();
			  MadeUpClient client = _builder.chunkSize( clientChunkSize ).client();

			  AddToLifeAndStart( server, client );

			  // Tell server to stream data occupying roughly two chunks. The chunks
			  // from server are 10 times bigger than the clients chunk size.
			  client.SendDataStream( new DataProducer( clientChunkSize * 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void impossibleToHaveBiggerChunkSizeThanFrameSize() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ImpossibleToHaveBiggerChunkSizeThanFrameSize()
		 {
			  Builder myBuilder = _builder.chunkSize( MadeUpServer.FRAME_LENGTH + 10 );
			  try
			  {
					MadeUpServer server = myBuilder.Server();
					server.Init();
					server.Start();
					fail( "Shouldn't be possible" );
			  }
			  catch ( System.ArgumentException )
			  { // Good
			  }

			  try
			  {
					myBuilder.Client();
					fail( "Shouldn't be possible" );
			  }
			  catch ( System.ArgumentException )
			  { // Good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void clientShouldUseHandlersToHandleComExceptions()
		 public virtual void ClientShouldUseHandlersToHandleComExceptions()
		 {
			  // Given
			  const string comExceptionMessage = "The ComException";

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: MadeUpCommunicationInterface communication = mock(MadeUpCommunicationInterface.class, (org.mockito.stubbing.Answer<Response<?>>) ignored ->
			  MadeUpCommunicationInterface communication = mock(typeof(MadeUpCommunicationInterface), (Answer<Response<object>>) ignored =>
			  {
						  throw new ComException( comExceptionMessage );
			  });

			  ComExceptionHandler handler = mock( typeof( ComExceptionHandler ) );

			  _life.add( _builder.server( communication ) );
			  MadeUpClient client = _life.add( _builder.client() );
			  client.ComExceptionHandler = handler;

			  _life.start();

			  // When
			  ComException exceptionThrownOnRequest = null;
			  try
			  {
					client.Multiply( 1, 10 );
			  }
			  catch ( ComException e )
			  {
					exceptionThrownOnRequest = e;
			  }

			  // Then
			  assertNotNull( exceptionThrownOnRequest );
			  assertEquals( comExceptionMessage, exceptionThrownOnRequest.Message );

			  ArgumentCaptor<ComException> exceptionCaptor = ArgumentCaptor.forClass( typeof( ComException ) );
			  verify( handler ).handle( exceptionCaptor.capture() );
			  assertEquals( comExceptionMessage, exceptionCaptor.Value.Message );
			  verifyNoMoreInteractions( handler );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @SuppressWarnings("rawtypes") public void masterResponseShouldBeUnpackedIfRequestTypeRequires() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MasterResponseShouldBeUnpackedIfRequestTypeRequires()
		 {
			  // Given
			  ResponseUnpacker responseUnpacker = mock( typeof( ResponseUnpacker ) );
			  MadeUpClient client = _builder.clientWith( responseUnpacker );
			  AddToLifeAndStart( _builder.server(), client );

			  // When
			  client.Multiply( 42, 42 );

			  // Then
			  ArgumentCaptor<Response> captor = ArgumentCaptor.forClass( typeof( Response ) );
			  verify( responseUnpacker ).unpackResponse( captor.capture(), eq(NO_OP_TX_HANDLER) );
			  assertEquals( _storeIdToUse, captor.Value.StoreId );
			  assertEquals( 42 * 42, captor.Value.response() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void masterResponseShouldNotBeUnpackedIfRequestTypeDoesNotRequire()
		 public virtual void MasterResponseShouldNotBeUnpackedIfRequestTypeDoesNotRequire()
		 {
			  // Given
			  ResponseUnpacker responseUnpacker = mock( typeof( ResponseUnpacker ) );
			  MadeUpClient client = _builder.clientWith( responseUnpacker );
			  AddToLifeAndStart( _builder.server(), client );

			  // When
			  client.SendDataStream( new KnownDataByteChannel( 100 ) );

			  // Then
			  verifyZeroInteractions( responseUnpacker );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStreamBackTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStreamBackTransactions()
		 {
			  // GIVEN
			  int value = 11;
			  int txCount = 5;
			  _life.add( _builder.server() );
			  MadeUpClient client = _life.add( _builder.client() );
			  _life.start();
			  Response<int> response = client.StreamBackTransactions( value, txCount );
			  TransactionStreamVerifyingResponseHandler handler = new TransactionStreamVerifyingResponseHandler( this, txCount );

			  // WHEN
			  response.Accept( handler );
			  int responseValue = response.ResponseConflict();

			  // THEN
			  assertEquals( value, responseValue );
			  assertEquals( txCount, handler.ExpectedTxId - Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAdhereToTransactionObligations() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAdhereToTransactionObligations()
		 {
			  // GIVEN
			  int value = 15;
			  long desiredObligation = 8;
			  _life.add( _builder.server() );
			  MadeUpClient client = _life.add( _builder.client() );
			  _life.start();
			  Response<int> response = client.InformAboutTransactionObligations( value, desiredObligation );
			  TransactionObligationVerifyingResponseHandler handler = new TransactionObligationVerifyingResponseHandler( this );

			  // WHEN
			  response.Accept( handler );
			  int responseValue = response.ResponseConflict();

			  // THEN
			  assertEquals( value, responseValue );
			  assertEquals( desiredObligation, handler.ObligationTxId );
		 }

		 private void AddToLifeAndStart( MadeUpServer server, MadeUpClient client )
		 {
			  _life.add( server );
			  _life.add( client );
			  _life.init();
			  _life.start();
		 }

		 internal class Builder
		 {
			 private readonly CommunicationIT _outerInstance;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly int PortConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly int ChunkSizeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly sbyte InternalProtocolVersionConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly sbyte ApplicationProtocolVersionConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly TxChecksumVerifier VerifierConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly StoreId StoreIdConflict;

			  internal Builder( CommunicationIT outerInstance ) : this( outerInstance, PortAuthority.allocatePort(), FRAME_LENGTH, INTERNAL_PROTOCOL_VERSION, APPLICATION_PROTOCOL_VERSION, ALWAYS_MATCH, outerInstance.storeIdToUse )
			  {
				  this._outerInstance = outerInstance;
			  }

			  internal Builder( CommunicationIT outerInstance, int port, int chunkSize, sbyte internalProtocolVersion, sbyte applicationProtocolVersion, TxChecksumVerifier verifier, StoreId storeId )
			  {
				  this._outerInstance = outerInstance;
					this.PortConflict = port;
					this.ChunkSizeConflict = chunkSize;
					this.InternalProtocolVersionConflict = internalProtocolVersion;
					this.ApplicationProtocolVersionConflict = applicationProtocolVersion;
					this.VerifierConflict = verifier;
					this.StoreIdConflict = storeId;
			  }

			  public virtual Builder Port( int port )
			  {
					return new Builder( _outerInstance, port, ChunkSizeConflict, InternalProtocolVersionConflict, ApplicationProtocolVersionConflict, VerifierConflict, StoreIdConflict );
			  }

			  internal virtual Builder ChunkSize( int chunkSize )
			  {
					return new Builder( _outerInstance, PortConflict, chunkSize, InternalProtocolVersionConflict, ApplicationProtocolVersionConflict, VerifierConflict, StoreIdConflict );
			  }

			  internal virtual Builder InternalProtocolVersion( sbyte internalProtocolVersion )
			  {
					return new Builder( _outerInstance, PortConflict, ChunkSizeConflict, internalProtocolVersion, ApplicationProtocolVersionConflict, VerifierConflict, StoreIdConflict );
			  }

			  internal virtual Builder ApplicationProtocolVersion( sbyte applicationProtocolVersion )
			  {
					return new Builder( _outerInstance, PortConflict, ChunkSizeConflict, InternalProtocolVersionConflict, applicationProtocolVersion, VerifierConflict, StoreIdConflict );
			  }

			  internal virtual Builder Verifier( TxChecksumVerifier verifier )
			  {
					return new Builder( _outerInstance, PortConflict, ChunkSizeConflict, InternalProtocolVersionConflict, ApplicationProtocolVersionConflict, verifier, StoreIdConflict );
			  }

			  public virtual Builder StoreId( StoreId storeId )
			  {
					return new Builder( _outerInstance, PortConflict, ChunkSizeConflict, InternalProtocolVersionConflict, ApplicationProtocolVersionConflict, VerifierConflict, storeId );
			  }

			  public virtual MadeUpServer Server()
			  {
					return new MadeUpServer( new MadeUpServerImplementation( StoreIdConflict ), PortConflict, InternalProtocolVersionConflict, ApplicationProtocolVersionConflict, VerifierConflict, ChunkSizeConflict );
			  }

			  public virtual MadeUpServer Server( MadeUpCommunicationInterface target )
			  {
					return new MadeUpServer( target, PortConflict, InternalProtocolVersionConflict, ApplicationProtocolVersionConflict, VerifierConflict, ChunkSizeConflict );
			  }

			  public virtual MadeUpClient Client()
			  {
					return ClientWith( Neo4Net.com.storecopy.ResponseUnpacker_Fields.NoOpResponseUnpacker );
			  }

			  internal virtual MadeUpClient ClientWith( ResponseUnpacker responseUnpacker )
			  {
					return new MadeUpClientAnonymousInnerClass( this, PortConflict, StoreIdConflict, ChunkSizeConflict, responseUnpacker );
			  }

			  private class MadeUpClientAnonymousInnerClass : MadeUpClient
			  {
				  private readonly Builder _outerInstance;

				  public MadeUpClientAnonymousInnerClass( Builder outerInstance, int port, StoreId storeId, int chunkSize, ResponseUnpacker responseUnpacker ) : base( port, storeId, chunkSize, responseUnpacker )
				  {
					  this.outerInstance = outerInstance;
				  }

				  public override ProtocolVersion ProtocolVersion
				  {
					  get
					  {
							return new ProtocolVersion( _outerInstance.applicationProtocolVersion, _outerInstance.internalProtocolVersion );
					  }
				  }
			  }

			  internal virtual ServerInterface ServerInOtherJvm( int port )
			  {
					ServerInterface server = ( new MadeUpServerProcess() ).Start(new StartupData(StoreIdConflict.CreationTime, StoreIdConflict.RandomId, InternalProtocolVersionConflict, ApplicationProtocolVersionConflict, ChunkSizeConflict, port));
					server.AwaitStarted();
					return server;
			  }
		 }

		 public class TransactionStreamVerifyingResponseHandler : Response.Handler, Visitor<CommittedTransactionRepresentation, Exception>
		 {
			 private readonly CommunicationIT _outerInstance;

			  internal readonly long TxCount;
			  internal long ExpectedTxId = 1;

			  internal TransactionStreamVerifyingResponseHandler( CommunicationIT outerInstance, int txCount )
			  {
				  this._outerInstance = outerInstance;
					this.TxCount = txCount;
			  }

			  public override void Obligation( long txId )
			  {
					fail( "Should not called" );
			  }

			  public override Visitor<CommittedTransactionRepresentation, Exception> Transactions()
			  {
					return this;
			  }

			  public override bool Visit( CommittedTransactionRepresentation element )
			  {
					assertEquals( ExpectedTxId + Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID, element.CommitEntry.TxId );
					ExpectedTxId++;
					assertThat( element.CommitEntry.TxId, lessThanOrEqualTo( TxCount + Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID ) );
					return false;
			  }
		 }

		 public class TransactionObligationVerifyingResponseHandler : Response.Handler
		 {
			 private readonly CommunicationIT _outerInstance;

			 public TransactionObligationVerifyingResponseHandler( CommunicationIT outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal volatile long ObligationTxId;

			  public override void Obligation( long txId )
			  {
					this.ObligationTxId = txId;
			  }

			  public override Visitor<CommittedTransactionRepresentation, Exception> Transactions()
			  {
					throw new System.NotSupportedException( "Should not be called" );
			  }
		 }
	}

}
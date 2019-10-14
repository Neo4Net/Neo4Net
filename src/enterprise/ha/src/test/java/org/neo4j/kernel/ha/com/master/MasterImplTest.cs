using System;
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
namespace Neo4Net.Kernel.ha.com.master
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ArgumentCaptor = org.mockito.ArgumentCaptor;
	using InOrder = org.mockito.InOrder;
	using MockitoHamcrest = org.mockito.hamcrest.MockitoHamcrest;


	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using RequestContext = Neo4Net.com.RequestContext;
	using ResourceReleaser = Neo4Net.com.ResourceReleaser;
	using Neo4Net.com;
	using TransactionNotPresentOnMasterException = Neo4Net.com.TransactionNotPresentOnMasterException;
	using Neo4Net.com;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DefaultConversationSPI = Neo4Net.Kernel.ha.cluster.DefaultConversationSPI;
	using Monitor = Neo4Net.Kernel.ha.com.master.MasterImpl.Monitor;
	using SPI = Neo4Net.Kernel.ha.com.master.MasterImpl.SPI;
	using LockResult = Neo4Net.Kernel.ha.@lock.LockResult;
	using Locks_Client = Neo4Net.Kernel.impl.locking.Locks_Client;
	using ResourceTypes = Neo4Net.Kernel.impl.locking.ResourceTypes;
	using IllegalResourceException = Neo4Net.Kernel.impl.transaction.IllegalResourceException;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using NoSuchEntryException = Neo4Net.Kernel.impl.util.collection.NoSuchEntryException;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using ResourceType = Neo4Net.Storageengine.Api.@lock.ResourceType;
	using Neo4Net.Test.rule.concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.StoreIdTestFactory.newStoreIdForCurrentVersion;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	public class MasterImplTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenStartedAndInaccessibleWhenNewLockSessionThrowException()
		 public virtual void GivenStartedAndInaccessibleWhenNewLockSessionThrowException()
		 {
			  // Given
			  MasterImpl.SPI spi = mock( typeof( MasterImpl.SPI ) );
			  Config config = config();

			  when( spi.Accessible ).thenReturn( false );

			  MasterImpl instance = new MasterImpl( spi, mock( typeof( ConversationManager ) ), mock( typeof( MasterImpl.Monitor ) ), config );
			  instance.Start();

			  // When
			  try
			  {
					instance.NewLockSession( new RequestContext( 0, 1, 2, 0, 0 ) );
					fail();
			  }
			  catch ( TransactionFailureException )
			  {
					// Ok
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenStartedAndAccessibleWhenNewLockSessionThenSucceeds() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void GivenStartedAndAccessibleWhenNewLockSessionThenSucceeds()
		 {
			  // Given
			  MasterImpl.SPI spi = MockedSpi();
			  Config config = config();

			  when( spi.Accessible ).thenReturn( true );
			  when( spi.GetTransactionChecksum( anyLong() ) ).thenReturn(1L);

			  MasterImpl instance = new MasterImpl( spi, mock( typeof( ConversationManager ) ), mock( typeof( MasterImpl.Monitor ) ), config );
			  instance.Start();
			  HandshakeResult handshake = instance.Handshake( 1, newStoreIdForCurrentVersion() ).response();

			  // When
			  try
			  {
					instance.NewLockSession( new RequestContext( handshake.Epoch(), 1, 2, 0, 0 ) );
			  }
			  catch ( Exception e )
			  {
					fail( e.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failingToStartTxShouldNotLeadToNPE() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailingToStartTxShouldNotLeadToNPE()
		 {
			  // Given
			  MasterImpl.SPI spi = MockedSpi();
			  DefaultConversationSPI conversationSpi = MockedConversationSpi();
			  Config config = config();
			  ConversationManager conversationManager = new ConversationManager( conversationSpi, config );

			  when( spi.Accessible ).thenReturn( true );
			  when( conversationSpi.AcquireClient() ).thenThrow(new Exception("Nope"));
			  when( spi.GetTransactionChecksum( anyLong() ) ).thenReturn(1L);
			  MockEmptyResponse( spi );

			  MasterImpl instance = new MasterImpl( spi, conversationManager, mock( typeof( MasterImpl.Monitor ) ), config );
			  instance.Start();
			  Response<HandshakeResult> response = instance.Handshake( 1, newStoreIdForCurrentVersion() );
			  HandshakeResult handshake = response.ResponseConflict();

			  // When
			  try
			  {
					instance.NewLockSession( new RequestContext( handshake.Epoch(), 1, 2, 0, 0 ) );
					fail( "Should have failed." );
			  }
			  catch ( Exception e )
			  {
					// Then
					assertThat( e, instanceOf( typeof( Exception ) ) );
					assertThat( e.Message, equalTo( "Nope" ) );
			  }
		 }

		 private void MockEmptyResponse( SPI spi )
		 {
			  when( spi.PackEmptyResponse( any() ) ).thenAnswer(invocation => new TransactionObligationResponse<>(invocation.getArgument(0), StoreId.DEFAULT, Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID, Neo4Net.com.ResourceReleaser_Fields.NoOp));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotEndLockSessionWhereThereIsAnActiveLockAcquisition() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotEndLockSessionWhereThereIsAnActiveLockAcquisition()
		 {
			  // GIVEN
			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );
			  try
			  {
					Locks_Client client = NewWaitingLocksClient( latch );
					MasterImpl master = NewMasterWithLocksClient( client );
					HandshakeResult handshake = master.Handshake( 1, newStoreIdForCurrentVersion() ).response();

					// WHEN
					RequestContext context = new RequestContext( handshake.Epoch(), 1, 2, 0, 0 );
					master.NewLockSession( context );
					Future<Void> acquireFuture = OtherThread.execute(state =>
					{
					 master.AcquireExclusiveLock( context, ResourceTypes.NODE, 1L );
					 return null;
					});
					OtherThread.get().waitUntilWaiting();
					master.EndLockSession( context, true );
					verify( client, never() ).stop();
					verify( client, never() ).close();
					latch.Signal();
					acquireFuture.get();

					// THEN
					verify( client ).close();
			  }
			  finally
			  {
					latch.Signal();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStopLockSessionOnFailureWhereThereIsAnActiveLockAcquisition() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStopLockSessionOnFailureWhereThereIsAnActiveLockAcquisition()
		 {
			  // GIVEN
			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );
			  try
			  {
					Locks_Client client = NewWaitingLocksClient( latch );
					MasterImpl master = NewMasterWithLocksClient( client );
					HandshakeResult handshake = master.Handshake( 1, newStoreIdForCurrentVersion() ).response();

					// WHEN
					RequestContext context = new RequestContext( handshake.Epoch(), 1, 2, 0, 0 );
					master.NewLockSession( context );
					Future<Void> acquireFuture = OtherThread.execute(state =>
					{
					 master.AcquireExclusiveLock( context, ResourceTypes.NODE, 1L );
					 return null;
					});
					OtherThread.get().waitUntilWaiting();
					master.EndLockSession( context, false );
					verify( client ).stop();
					verify( client, never() ).close();
					latch.Signal();
					acquireFuture.get();

					// THEN
					verify( client ).close();
			  }
			  finally
			  {
					latch.Signal();
			  }
		 }

		 private MasterImpl NewMasterWithLocksClient( Locks_Client client )
		 {
			  SPI spi = MockedSpi();
			  DefaultConversationSPI conversationSpi = MockedConversationSpi();
			  when( spi.Accessible ).thenReturn( true );
			  when( conversationSpi.AcquireClient() ).thenReturn(client);
			  Config config = config();
			  ConversationManager conversationManager = new ConversationManager( conversationSpi, config );

			  MasterImpl master = new MasterImpl( spi, conversationManager, mock( typeof( Monitor ) ), config );
			  master.Start();
			  return master;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.kernel.impl.locking.Locks_Client newWaitingLocksClient(final java.util.concurrent.CountDownLatch latch)
		 private Locks_Client NewWaitingLocksClient( System.Threading.CountdownEvent latch )
		 {
			  Locks_Client client = mock( typeof( Locks_Client ) );

			  doAnswer(invocation =>
			  {
				latch.await();
				return null;
			  }).when( client ).acquireExclusive( eq( LockTracer.NONE ), any( typeof( ResourceType ) ), anyLong() );

			  return client;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowCommitIfThereIsNoMatchingLockSession() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowCommitIfThereIsNoMatchingLockSession()
		 {
			  // Given
			  MasterImpl.SPI spi = mock( typeof( MasterImpl.SPI ) );
			  DefaultConversationSPI conversationSpi = MockedConversationSpi();
			  Config config = config();
			  ConversationManager conversationManager = new ConversationManager( conversationSpi, config );

			  when( spi.Accessible ).thenReturn( true );
			  when( spi.GetTransactionChecksum( anyLong() ) ).thenReturn(1L);
			  MockEmptyResponse( spi );

			  MasterImpl master = new MasterImpl( spi, conversationManager, mock( typeof( MasterImpl.Monitor ) ), config );
			  master.Start();
			  HandshakeResult handshake = master.Handshake( 1, newStoreIdForCurrentVersion() ).response();

			  RequestContext ctx = new RequestContext( handshake.Epoch(), 1, 2, 0, 0 );

			  // When
			  try
			  {
					master.Commit( ctx, mock( typeof( TransactionRepresentation ) ) );
					fail( "Should have failed." );
			  }
			  catch ( TransactionNotPresentOnMasterException e )
			  {
					// Then
					assertThat( e.Message, equalTo( ( new TransactionNotPresentOnMasterException( ctx ) ).Message ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowCommitIfClientHoldsNoLocks() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowCommitIfClientHoldsNoLocks()
		 {
			  // Given
			  MasterImpl.SPI spi = mock( typeof( MasterImpl.SPI ) );
			  Config config = config();
			  DefaultConversationSPI conversationSpi = MockedConversationSpi();
			  ConversationManager conversationManager = new ConversationManager( conversationSpi, config );

			  when( spi.Accessible ).thenReturn( true );
			  when( spi.GetTransactionChecksum( anyLong() ) ).thenReturn(1L);
			  MockEmptyResponse( spi );

			  MasterImpl master = new MasterImpl( spi, conversationManager, mock( typeof( MasterImpl.Monitor ) ), config );
			  master.Start();
			  HandshakeResult handshake = master.Handshake( 1, newStoreIdForCurrentVersion() ).response();

			  const int noLockSession = -1;
			  RequestContext ctx = new RequestContext( handshake.Epoch(), 1, noLockSession, 0, 0 );
			  TransactionRepresentation tx = mock( typeof( TransactionRepresentation ) );

			  // When
			  master.Commit( ctx, tx );

			  // Then
			  verify( spi ).applyPreparedTransaction( tx );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowStartNewTransactionAfterClientSessionWasRemovedOnTimeout() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowStartNewTransactionAfterClientSessionWasRemovedOnTimeout()
		 {
			  //Given
			  MasterImpl.SPI spi = MockedSpi();
			  DefaultConversationSPI conversationSpi = MockedConversationSpi();
			  Monitor monitor = mock( typeof( Monitor ) );
			  Config config = config();
			  Locks_Client client = mock( typeof( Locks_Client ) );
			  ConversationManager conversationManager = new ConversationManager( conversationSpi, config );
			  int machineId = 1;
			  MasterImpl master = new MasterImpl( spi, conversationManager, monitor, config );

			  when( spi.Accessible ).thenReturn( true );
			  when( conversationSpi.AcquireClient() ).thenReturn(client);
			  master.Start();
			  HandshakeResult handshake = master.Handshake( 1, newStoreIdForCurrentVersion() ).response();
			  RequestContext requestContext = new RequestContext( handshake.Epoch(), machineId, 0, 0, 0 );

			  // When
			  master.NewLockSession( requestContext );
			  master.AcquireSharedLock( requestContext, ResourceTypes.NODE, 1L );
			  conversationManager.Stop( requestContext );
			  master.NewLockSession( requestContext );

			  //Then
			  IDictionary<int, ICollection<RequestContext>> transactions = master.OngoingTransactions;
			  assertEquals( 1, transactions.Count );
			  assertThat( transactions[machineId], org.hamcrest.Matchers.hasItem( requestContext ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartStopConversationManager()
		 public virtual void ShouldStartStopConversationManager()
		 {
			  MasterImpl.SPI spi = MockedSpi();
			  ConversationManager conversationManager = mock( typeof( ConversationManager ) );
			  Config config = config();
			  MasterImpl master = new MasterImpl( spi, conversationManager, null, config );

			  master.Start();
			  master.Stop();

			  InOrder order = inOrder( conversationManager );
			  order.verify( conversationManager ).start();
			  order.verify( conversationManager ).stop();
			  verifyNoMoreInteractions( conversationManager );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lockResultMustHaveMessageWhenAcquiringExclusiveLockWithoutConversation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LockResultMustHaveMessageWhenAcquiringExclusiveLockWithoutConversation()
		 {
			  MasterImpl.SPI spi = MockedSpi();
			  ConversationManager conversationManager = mock( typeof( ConversationManager ) );
			  Config config = config();
			  MasterImpl master = new MasterImpl( spi, conversationManager, null, config );

			  RequestContext context = CreateRequestContext( master );
			  when( conversationManager.Acquire( context ) ).thenThrow( new NoSuchEntryException( "" ) );
			  master.AcquireExclusiveLock( context, ResourceTypes.NODE, 1 );

			  ArgumentCaptor<LockResult> captor = ArgumentCaptor.forClass( typeof( LockResult ) );
			  verify( spi ).packTransactionObligationResponse( MockitoHamcrest.argThat( @is( context ) ), captor.capture() );
			  assertThat( captor.Value.Message, @is( not( nullValue() ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lockResultMustHaveMessageWhenAcquiringSharedLockWithoutConversation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LockResultMustHaveMessageWhenAcquiringSharedLockWithoutConversation()
		 {
			  MasterImpl.SPI spi = MockedSpi();
			  ConversationManager conversationManager = mock( typeof( ConversationManager ) );
			  Config config = config();
			  MasterImpl master = new MasterImpl( spi, conversationManager, null, config );

			  RequestContext context = CreateRequestContext( master );
			  when( conversationManager.Acquire( context ) ).thenThrow( new NoSuchEntryException( "" ) );
			  master.AcquireSharedLock( context, ResourceTypes.NODE, 1 );

			  ArgumentCaptor<LockResult> captor = ArgumentCaptor.forClass( typeof( LockResult ) );
			  verify( spi ).packTransactionObligationResponse( MockitoHamcrest.argThat( @is( context ) ), captor.capture() );
			  assertThat( captor.Value.Message, @is( not( nullValue() ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lockResultMustHaveMessageWhenAcquiringExclusiveLockDeadlocks()
		 public virtual void LockResultMustHaveMessageWhenAcquiringExclusiveLockDeadlocks()
		 {
			  MasterImpl.SPI spi = MockedSpi();
			  DefaultConversationSPI conversationSpi = MockedConversationSpi();
			  Config config = config();
			  ConversationManager conversationManager = new ConversationManager( conversationSpi, config );
			  conversationManager.Start();
			  Locks_Client locks = mock( typeof( Locks_Client ) );
			  MasterImpl master = new MasterImpl( spi, conversationManager, null, config );

			  RequestContext context = CreateRequestContext( master );
			  when( conversationSpi.AcquireClient() ).thenReturn(locks);
			  ResourceTypes type = ResourceTypes.NODE;
			  doThrow( new DeadlockDetectedException( "" ) ).when( locks ).acquireExclusive( LockTracer.NONE, type, 1 );
			  master.AcquireExclusiveLock( context, type, 1 );

			  ArgumentCaptor<LockResult> captor = ArgumentCaptor.forClass( typeof( LockResult ) );
			  verify( spi ).packTransactionObligationResponse( MockitoHamcrest.argThat( @is( context ) ), captor.capture() );
			  assertThat( captor.Value.Message, @is( not( nullValue() ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lockResultMustHaveMessageWhenAcquiringSharedLockDeadlocks()
		 public virtual void LockResultMustHaveMessageWhenAcquiringSharedLockDeadlocks()
		 {
			  MasterImpl.SPI spi = MockedSpi();
			  DefaultConversationSPI conversationSpi = MockedConversationSpi();
			  Config config = config();
			  ConversationManager conversationManager = new ConversationManager( conversationSpi, config );
			  conversationManager.Start();
			  Locks_Client locks = mock( typeof( Locks_Client ) );
			  MasterImpl master = new MasterImpl( spi, conversationManager, null, config );

			  RequestContext context = CreateRequestContext( master );
			  when( conversationSpi.AcquireClient() ).thenReturn(locks);
			  ResourceTypes type = ResourceTypes.NODE;
			  doThrow( new DeadlockDetectedException( "" ) ).when( locks ).acquireExclusive( LockTracer.NONE, type, 1 );
			  master.AcquireSharedLock( context, type, 1 );

			  ArgumentCaptor<LockResult> captor = ArgumentCaptor.forClass( typeof( LockResult ) );
			  verify( spi ).packTransactionObligationResponse( MockitoHamcrest.argThat( @is( context ) ), captor.capture() );
			  assertThat( captor.Value.Message, @is( not( nullValue() ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lockResultMustHaveMessageWhenAcquiringExclusiveLockThrowsIllegalResource()
		 public virtual void LockResultMustHaveMessageWhenAcquiringExclusiveLockThrowsIllegalResource()
		 {
			  MasterImpl.SPI spi = MockedSpi();
			  DefaultConversationSPI conversationSpi = MockedConversationSpi();
			  Config config = config();
			  ConversationManager conversationManager = new ConversationManager( conversationSpi, config );
			  conversationManager.Start();
			  Locks_Client locks = mock( typeof( Locks_Client ) );
			  MasterImpl master = new MasterImpl( spi, conversationManager, null, config );

			  RequestContext context = CreateRequestContext( master );
			  when( conversationSpi.AcquireClient() ).thenReturn(locks);
			  ResourceTypes type = ResourceTypes.NODE;
			  doThrow( new IllegalResourceException( "" ) ).when( locks ).acquireExclusive( LockTracer.NONE, type, 1 );
			  master.AcquireExclusiveLock( context, type, 1 );

			  ArgumentCaptor<LockResult> captor = ArgumentCaptor.forClass( typeof( LockResult ) );
			  verify( spi ).packTransactionObligationResponse( MockitoHamcrest.argThat( @is( context ) ), captor.capture() );
			  assertThat( captor.Value.Message, @is( not( nullValue() ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lockResultMustHaveMessageWhenAcquiringSharedLockThrowsIllegalResource()
		 public virtual void LockResultMustHaveMessageWhenAcquiringSharedLockThrowsIllegalResource()
		 {
			  MasterImpl.SPI spi = MockedSpi();
			  DefaultConversationSPI conversationSpi = MockedConversationSpi();
			  Config config = config();
			  ConversationManager conversationManager = new ConversationManager( conversationSpi, config );
			  conversationManager.Start();
			  Locks_Client locks = mock( typeof( Locks_Client ) );
			  MasterImpl master = new MasterImpl( spi, conversationManager, null, config );

			  RequestContext context = CreateRequestContext( master );
			  when( conversationSpi.AcquireClient() ).thenReturn(locks);
			  ResourceTypes type = ResourceTypes.NODE;
			  doThrow( new IllegalResourceException( "" ) ).when( locks ).acquireExclusive( LockTracer.NONE, type, 1 );
			  master.AcquireSharedLock( context, type, 1 );

			  ArgumentCaptor<LockResult> captor = ArgumentCaptor.forClass( typeof( LockResult ) );
			  verify( spi ).packTransactionObligationResponse( MockitoHamcrest.argThat( @is( context ) ), captor.capture() );
			  assertThat( captor.Value.Message, @is( not( nullValue() ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.concurrent.OtherThreadRule<Void> otherThread = new org.neo4j.test.rule.concurrent.OtherThreadRule<>();
		 public readonly OtherThreadRule<Void> OtherThread = new OtherThreadRule<Void>();

		 private Config Config()
		 {
			  return Config.defaults( stringMap( HaSettings.lock_read_timeout.name(), 20 + "s", ClusterSettings.server_id.name(), "1" ) );
		 }

		 public virtual DefaultConversationSPI MockedConversationSpi()
		 {
			  return mock( typeof( DefaultConversationSPI ) );
		 }

		 public static SPI MockedSpi()
		 {
			  return MockedSpi( StoreId.DEFAULT );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.neo4j.kernel.ha.com.master.MasterImpl.SPI mockedSpi(final org.neo4j.storageengine.api.StoreId storeId)
		 public static SPI MockedSpi( StoreId storeId )
		 {
			  MasterImpl.SPI mock = mock( typeof( MasterImpl.SPI ) );
			  when( mock.StoreId() ).thenReturn(storeId);
			  when( mock.PackEmptyResponse( any() ) ).thenAnswer(invocation => new TransactionObligationResponse<>(invocation.getArgument(0), storeId, Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID, Neo4Net.com.ResourceReleaser_Fields.NoOp));
			  return mock;
		 }

		 protected internal virtual RequestContext CreateRequestContext( MasterImpl master )
		 {
			  HandshakeResult handshake = master.Handshake( 1, newStoreIdForCurrentVersion() ).response();
			  return new RequestContext( handshake.Epoch(), 1, 2, 0, 0 );
		 }
	}

}
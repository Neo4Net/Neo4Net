using System;

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
namespace Neo4Net.causalclustering.catchup.tx
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;


	using Neo4Net.causalclustering.catchup;
	using LocalDatabase = Neo4Net.causalclustering.catchup.storecopy.LocalDatabase;
	using StoreCopyProcess = Neo4Net.causalclustering.catchup.storecopy.StoreCopyProcess;
	using CountingTimerService = Neo4Net.causalclustering.core.consensus.schedule.CountingTimerService;
	using Timer = Neo4Net.causalclustering.core.consensus.schedule.Timer;
	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using Suspendable = Neo4Net.causalclustering.helper.Suspendable;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using UpstreamDatabaseStrategySelector = Neo4Net.causalclustering.upstream.UpstreamDatabaseStrategySelector;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using DatabaseHealth = Neo4Net.Kernel.@internal.DatabaseHealth;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using FakeClockJobScheduler = Neo4Net.Test.FakeClockJobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.tx.CatchupPollingProcess.State.PANIC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.tx.CatchupPollingProcess.State.STORE_COPYING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.tx.CatchupPollingProcess.State.TX_PULLING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.tx.CatchupPollingProcess.Timers.TX_PULLER_TIMER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;

	public class CatchupPollingProcessTest
	{
		private bool InstanceFieldsInitialized = false;

		public CatchupPollingProcessTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_timerService = new CountingTimerService( _scheduler, NullLogProvider.Instance );
			when( _localDatabase.storeId() ).thenReturn(_storeId);
			when( _topologyService.findCatchupAddress( _coreMemberId ) ).thenReturn( _coreMemberAddress );
			_txPuller = new CatchupPollingProcess( NullLogProvider.Instance, _localDatabase, _startStopOnStoreCopy, _catchUpClient, _strategyPipeline, _timerService, _txPullIntervalMillis, _txApplier, new Monitors(), _storeCopyProcess, () => mock(typeof(DatabaseHealth)), _topologyService );
		}

		 private readonly CatchUpClient _catchUpClient = mock( typeof( CatchUpClient ) );
		 private readonly UpstreamDatabaseStrategySelector _strategyPipeline = mock( typeof( UpstreamDatabaseStrategySelector ) );
		 private readonly MemberId _coreMemberId = mock( typeof( MemberId ) );
		 private readonly TransactionIdStore _idStore = mock( typeof( TransactionIdStore ) );

		 private readonly BatchingTxApplier _txApplier = mock( typeof( BatchingTxApplier ) );
		 private readonly FakeClockJobScheduler _scheduler = new FakeClockJobScheduler();
		 private CountingTimerService _timerService;

		 private readonly long _txPullIntervalMillis = 100;
		 private readonly StoreCopyProcess _storeCopyProcess = mock( typeof( StoreCopyProcess ) );
		 private readonly StoreId _storeId = new StoreId( 1, 2, 3, 4 );
		 private readonly LocalDatabase _localDatabase = mock( typeof( LocalDatabase ) );
		 private readonly TopologyService _topologyService = mock( typeof( TopologyService ) );
		 private readonly AdvertisedSocketAddress _coreMemberAddress = new AdvertisedSocketAddress( "hostname", 1234 );

		 private readonly Suspendable _startStopOnStoreCopy = mock( typeof( Suspendable ) );

		 private CatchupPollingProcess _txPuller;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Before()
		 {
			  when( _idStore.LastCommittedTransactionId ).thenReturn( BASE_TX_ID );
			  when( _strategyPipeline.bestUpstreamDatabase() ).thenReturn(_coreMemberId);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendPullRequestOnTick() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendPullRequestOnTick()
		 {
			  // given
			  _txPuller.start();
			  long lastAppliedTxId = 99L;
			  when( _txApplier.lastQueuedTxId() ).thenReturn(lastAppliedTxId);

			  // when
			  _timerService.invoke( TX_PULLER_TIMER );

			  // then
			  verify( _catchUpClient ).makeBlockingRequest( any( typeof( AdvertisedSocketAddress ) ), any( typeof( TxPullRequest ) ), any( typeof( CatchUpResponseCallback ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepMakingPullRequestsUntilEndOfStream() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKeepMakingPullRequestsUntilEndOfStream()
		 {
			  // given
			  _txPuller.start();
			  long lastAppliedTxId = 99L;
			  when( _txApplier.lastQueuedTxId() ).thenReturn(lastAppliedTxId);

			  // when
			  when( _catchUpClient.makeBlockingRequest<TxStreamFinishedResponse>( any( typeof( AdvertisedSocketAddress ) ), any( typeof( TxPullRequest ) ), any( typeof( CatchUpResponseCallback ) ) ) ).thenReturn( new TxStreamFinishedResponse( CatchupResult.SUCCESS_END_OF_STREAM, 10 ) );

			  _timerService.invoke( TX_PULLER_TIMER );

			  // then
			  verify( _catchUpClient, times( 1 ) ).makeBlockingRequest( any( typeof( AdvertisedSocketAddress ) ), any( typeof( TxPullRequest ) ), any( typeof( CatchUpResponseCallback ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRenewTxPullTimeoutOnSuccessfulTxPulling() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRenewTxPullTimeoutOnSuccessfulTxPulling()
		 {
			  // when
			  _txPuller.start();
			  when( _catchUpClient.makeBlockingRequest( any( typeof( AdvertisedSocketAddress ) ), any( typeof( TxPullRequest ) ), any( typeof( CatchUpResponseCallback ) ) ) ).thenReturn( new TxStreamFinishedResponse( CatchupResult.SUCCESS_END_OF_STREAM, 0 ) );

			  _timerService.invoke( TX_PULLER_TIMER );

			  // then
			  assertEquals( 1, _timerService.invocationCount( TX_PULLER_TIMER ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nextStateShouldBeStoreCopyingIfRequestedTransactionHasBeenPrunedAway() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NextStateShouldBeStoreCopyingIfRequestedTransactionHasBeenPrunedAway()
		 {
			  // when
			  _txPuller.start();
			  when( _catchUpClient.makeBlockingRequest( any( typeof( AdvertisedSocketAddress ) ), any( typeof( TxPullRequest ) ), any( typeof( CatchUpResponseCallback ) ) ) ).thenReturn( new TxStreamFinishedResponse( CatchupResult.E_TRANSACTION_PRUNED, 0 ) );

			  _timerService.invoke( TX_PULLER_TIMER );

			  // then
			  assertEquals( STORE_COPYING, _txPuller.state() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nextStateShouldBeTxPullingAfterASuccessfulStoreCopy() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NextStateShouldBeTxPullingAfterASuccessfulStoreCopy()
		 {
			  // given
			  _txPuller.start();
			  when( _catchUpClient.makeBlockingRequest( any( typeof( AdvertisedSocketAddress ) ), any( typeof( TxPullRequest ) ), any( typeof( CatchUpResponseCallback ) ) ) ).thenReturn( new TxStreamFinishedResponse( CatchupResult.E_TRANSACTION_PRUNED, 0 ) );

			  // when (tx pull)
			  _timerService.invoke( TX_PULLER_TIMER );

			  // when (store copy)
			  _timerService.invoke( TX_PULLER_TIMER );

			  // then
			  verify( _localDatabase ).stopForStoreCopy();
			  verify( _startStopOnStoreCopy ).disable();
			  verify( _storeCopyProcess ).replaceWithStoreFrom( any( typeof( CatchupAddressProvider ) ), eq( _storeId ) );
			  verify( _localDatabase ).start();
			  verify( _startStopOnStoreCopy ).enable();
			  verify( _txApplier ).refreshFromNewStore();

			  // then
			  assertEquals( TX_PULLING, _txPuller.state() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotRenewTheTimeoutIfInPanicState()
		 public virtual void ShouldNotRenewTheTimeoutIfInPanicState()
		 {
			  // given
			  _txPuller.start();
			  CatchUpResponseCallback callback = mock( typeof( CatchUpResponseCallback ) );

			  doThrow( new Exception( "Panic all the things" ) ).when( callback ).onTxPullResponse( any( typeof( CompletableFuture ) ), any( typeof( TxPullResponse ) ) );
			  Timer timer = Mockito.spy( single( _timerService.getTimers( TX_PULLER_TIMER ) ) );

			  // when
			  _timerService.invoke( TX_PULLER_TIMER );

			  // then
			  assertEquals( PANIC, _txPuller.state() );
			  verify( timer, never() ).reset();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSignalOperationalUntilPulling() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSignalOperationalUntilPulling()
		 {
			  // given
			  when( _catchUpClient.makeBlockingRequest<TxStreamFinishedResponse>( any( typeof( AdvertisedSocketAddress ) ), any( typeof( TxPullRequest ) ), any( typeof( CatchUpResponseCallback ) ) ) ).thenReturn( new TxStreamFinishedResponse( CatchupResult.E_TRANSACTION_PRUNED, 0 ), new TxStreamFinishedResponse( CatchupResult.SUCCESS_END_OF_STREAM, 15 ) );

			  // when
			  _txPuller.start();
			  Future<bool> operationalFuture = _txPuller.upToDateFuture();
			  assertFalse( operationalFuture.Done );

			  _timerService.invoke( TX_PULLER_TIMER ); // realises we need a store copy
			  assertFalse( operationalFuture.Done );

			  _timerService.invoke( TX_PULLER_TIMER ); // does the store copy
			  assertFalse( operationalFuture.Done );

			  _timerService.invoke( TX_PULLER_TIMER ); // does a pulling
			  assertTrue( operationalFuture.Done );
			  assertTrue( operationalFuture.get() );

			  // then
			  assertEquals( TX_PULLING, _txPuller.state() );
		 }
	}

}
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
namespace Org.Neo4j.causalclustering.core.state
{
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;


	using NewLeaderBarrier = Org.Neo4j.causalclustering.core.consensus.NewLeaderBarrier;
	using InMemoryRaftLog = Org.Neo4j.causalclustering.core.consensus.log.InMemoryRaftLog;
	using RaftLogEntry = Org.Neo4j.causalclustering.core.consensus.log.RaftLogEntry;
	using ConsecutiveInFlightCache = Org.Neo4j.causalclustering.core.consensus.log.cache.ConsecutiveInFlightCache;
	using InFlightCache = Org.Neo4j.causalclustering.core.consensus.log.cache.InFlightCache;
	using RaftLogCommitIndexMonitor = Org.Neo4j.causalclustering.core.consensus.log.monitoring.RaftLogCommitIndexMonitor;
	using DistributedOperation = Org.Neo4j.causalclustering.core.replication.DistributedOperation;
	using ProgressTrackerImpl = Org.Neo4j.causalclustering.core.replication.ProgressTrackerImpl;
	using ReplicatedContent = Org.Neo4j.causalclustering.core.replication.ReplicatedContent;
	using GlobalSession = Org.Neo4j.causalclustering.core.replication.session.GlobalSession;
	using GlobalSessionTrackerState = Org.Neo4j.causalclustering.core.replication.session.GlobalSessionTrackerState;
	using LocalOperationId = Org.Neo4j.causalclustering.core.replication.session.LocalOperationId;
	using CoreReplicatedContent = Org.Neo4j.causalclustering.core.state.machines.tx.CoreReplicatedContent;
	using ReplicatedTransaction = Org.Neo4j.causalclustering.core.state.machines.tx.ReplicatedTransaction;
	using Org.Neo4j.causalclustering.core.state.storage;
	using DatabasePanicEventGenerator = Org.Neo4j.Kernel.impl.core.DatabasePanicEventGenerator;
	using DatabaseHealth = Org.Neo4j.Kernel.@internal.DatabaseHealth;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	public class CommandApplicationProcessTest
	{
		private bool InstanceFieldsInitialized = false;

		public CommandApplicationProcessTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_applicationProcess = new CommandApplicationProcess( _raftLog, _batchSize, _flushEvery, () => _dbHealth, NullLogProvider.Instance, new ProgressTrackerImpl(_globalSession), _sessionTracker, _coreState, _inFlightCache, _monitors );
			when( _coreState.commandDispatcher() ).thenReturn(_commandDispatcher);
			when( _coreState.LastAppliedIndex ).thenReturn( -1L );
			when( _coreState.LastFlushed ).thenReturn( -1L );
		}

		 private readonly InMemoryRaftLog _raftLog = spy( new InMemoryRaftLog() );

		 private readonly SessionTracker _sessionTracker = new SessionTracker( new InMemoryStateStorage<GlobalSessionTrackerState>( new GlobalSessionTrackerState() ) );

		 private readonly DatabaseHealth _dbHealth = new DatabaseHealth( mock( typeof( DatabasePanicEventGenerator ) ), NullLogProvider.Instance.getLog( this.GetType() ) );

		 private readonly GlobalSession _globalSession = new GlobalSession( System.Guid.randomUUID(), null );
		 private readonly int _flushEvery = 10;
		 private readonly int _batchSize = 16;

		 private InFlightCache _inFlightCache = spy( new ConsecutiveInFlightCache() );
		 private readonly Monitors _monitors = new Monitors();
		 private CoreState _coreState = mock( typeof( CoreState ) );
		 private CommandApplicationProcess _applicationProcess;

		 private ReplicatedTransaction _nullTx = ReplicatedTransaction.from( new sbyte[0] );

		 private readonly CommandDispatcher _commandDispatcher = mock( typeof( CommandDispatcher ) );

		 private ReplicatedTransaction Tx( sbyte dataValue )
		 {
			  sbyte[] dataArray = new sbyte[30];
			  Arrays.fill( dataArray, dataValue );
			  return ReplicatedTransaction.from( dataArray );
		 }

		 private int _sequenceNumber;

		 private ReplicatedContent Operation( CoreReplicatedContent tx )
		 {
			 lock ( this )
			 {
				  return new DistributedOperation( tx, _globalSession, new LocalOperationId( 0, _sequenceNumber++ ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyCommittedCommand() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyCommittedCommand()
		 {
			  // given
			  RaftLogCommitIndexMonitor listener = mock( typeof( RaftLogCommitIndexMonitor ) );
			  _monitors.addMonitorListener( listener );

			  InOrder inOrder = inOrder( _coreState, _commandDispatcher );

			  _raftLog.append( new RaftLogEntry( 0, Operation( _nullTx ) ) );
			  _raftLog.append( new RaftLogEntry( 0, Operation( _nullTx ) ) );
			  _raftLog.append( new RaftLogEntry( 0, Operation( _nullTx ) ) );

			  // when
			  _applicationProcess.notifyCommitted( 2 );
			  _applicationProcess.start();

			  // then
			  inOrder.verify( _coreState ).commandDispatcher();
			  inOrder.verify( _commandDispatcher ).dispatch( eq( _nullTx ), eq( 0L ), AnyCallback() );
			  inOrder.verify( _commandDispatcher ).dispatch( eq( _nullTx ), eq( 1L ), AnyCallback() );
			  inOrder.verify( _commandDispatcher ).dispatch( eq( _nullTx ), eq( 2L ), AnyCallback() );
			  inOrder.verify( _commandDispatcher ).close();

			  verify( listener ).commitIndex( 2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotApplyUncommittedCommands() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotApplyUncommittedCommands()
		 {
			  // given
			  _raftLog.append( new RaftLogEntry( 0, Operation( _nullTx ) ) );
			  _raftLog.append( new RaftLogEntry( 0, Operation( _nullTx ) ) );

			  // when
			  _applicationProcess.notifyCommitted( -1 );
			  _applicationProcess.start();

			  // then
			  verifyZeroInteractions( _commandDispatcher );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void entriesThatAreNotStateMachineCommandsShouldStillIncreaseCommandIndex() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void EntriesThatAreNotStateMachineCommandsShouldStillIncreaseCommandIndex()
		 {
			  // given
			  _raftLog.append( new RaftLogEntry( 0, new NewLeaderBarrier() ) );
			  _raftLog.append( new RaftLogEntry( 0, Operation( _nullTx ) ) );

			  // when
			  _applicationProcess.notifyCommitted( 1 );
			  _applicationProcess.start();

			  // then
			  InOrder inOrder = inOrder( _coreState, _commandDispatcher );
			  inOrder.verify( _coreState ).commandDispatcher();
			  inOrder.verify( _commandDispatcher ).dispatch( eq( _nullTx ), eq( 1L ), AnyCallback() );
			  inOrder.verify( _commandDispatcher ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void duplicatesShouldBeIgnoredButStillIncreaseCommandIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DuplicatesShouldBeIgnoredButStillIncreaseCommandIndex()
		 {
			  // given
			  _raftLog.append( new RaftLogEntry( 0, new NewLeaderBarrier() ) );
			  _raftLog.append( new RaftLogEntry( 0, new DistributedOperation( _nullTx, _globalSession, new LocalOperationId( 0, 0 ) ) ) );
			  // duplicate
			  _raftLog.append( new RaftLogEntry( 0, new DistributedOperation( _nullTx, _globalSession, new LocalOperationId( 0, 0 ) ) ) );
			  _raftLog.append( new RaftLogEntry( 0, new DistributedOperation( _nullTx, _globalSession, new LocalOperationId( 0, 1 ) ) ) );

			  // when
			  _applicationProcess.notifyCommitted( 3 );
			  _applicationProcess.start();

			  // then
			  InOrder inOrder = inOrder( _coreState, _commandDispatcher );
			  inOrder.verify( _coreState ).commandDispatcher();
			  inOrder.verify( _commandDispatcher ).dispatch( eq( _nullTx ), eq( 1L ), AnyCallback() );
			  // duplicate not dispatched
			  inOrder.verify( _commandDispatcher ).dispatch( eq( _nullTx ), eq( 3L ), AnyCallback() );
			  inOrder.verify( _commandDispatcher ).close();
			  verifyNoMoreInteractions( _commandDispatcher );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void outOfOrderDuplicatesShouldBeIgnoredButStillIncreaseCommandIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void OutOfOrderDuplicatesShouldBeIgnoredButStillIncreaseCommandIndex()
		 {
			  // given
			  _raftLog.append( new RaftLogEntry( 0, new DistributedOperation( Tx( ( sbyte ) 100 ), _globalSession, new LocalOperationId( 0, 0 ) ) ) );
			  _raftLog.append( new RaftLogEntry( 0, new DistributedOperation( Tx( ( sbyte ) 101 ), _globalSession, new LocalOperationId( 0, 1 ) ) ) );
			  _raftLog.append( new RaftLogEntry( 0, new DistributedOperation( Tx( ( sbyte ) 102 ), _globalSession, new LocalOperationId( 0, 2 ) ) ) );
			  // duplicate of tx 101
			  _raftLog.append( new RaftLogEntry( 0, new DistributedOperation( Tx( ( sbyte ) 101 ), _globalSession, new LocalOperationId( 0, 1 ) ) ) );
			  // duplicate of tx 100
			  _raftLog.append( new RaftLogEntry( 0, new DistributedOperation( Tx( ( sbyte ) 100 ), _globalSession, new LocalOperationId( 0, 0 ) ) ) );
			  _raftLog.append( new RaftLogEntry( 0, new DistributedOperation( Tx( ( sbyte ) 103 ), _globalSession, new LocalOperationId( 0, 3 ) ) ) );
			  _raftLog.append( new RaftLogEntry( 0, new DistributedOperation( Tx( ( sbyte ) 104 ), _globalSession, new LocalOperationId( 0, 4 ) ) ) );

			  // when
			  _applicationProcess.notifyCommitted( 6 );
			  _applicationProcess.start();

			  // then
			  InOrder inOrder = inOrder( _coreState, _commandDispatcher );
			  inOrder.verify( _coreState ).commandDispatcher();
			  inOrder.verify( _commandDispatcher ).dispatch( eq( Tx( ( sbyte ) 100 ) ), eq( 0L ), AnyCallback() );
			  inOrder.verify( _commandDispatcher ).dispatch( eq( Tx( ( sbyte ) 101 ) ), eq( 1L ), AnyCallback() );
			  inOrder.verify( _commandDispatcher ).dispatch( eq( Tx( ( sbyte ) 102 ) ), eq( 2L ), AnyCallback() );
			  // duplicate of tx 101 not dispatched, at index 3
			  // duplicate of tx 100 not dispatched, at index 4
			  inOrder.verify( _commandDispatcher ).dispatch( eq( Tx( ( sbyte ) 103 ) ), eq( 5L ), AnyCallback() );
			  inOrder.verify( _commandDispatcher ).dispatch( eq( Tx( ( sbyte ) 104 ) ), eq( 6L ), AnyCallback() );
			  inOrder.verify( _commandDispatcher ).close();
			  verifyNoMoreInteractions( _commandDispatcher );
		 }

		 // TODO: Test recovery, see CoreState#start().

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPeriodicallyFlushState() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPeriodicallyFlushState()
		 {
			  // given
			  int interactions = _flushEvery * 5;
			  for ( int i = 0; i < interactions; i++ )
			  {
					_raftLog.append( new RaftLogEntry( 0, Operation( _nullTx ) ) );
			  }

			  // when
			  _applicationProcess.notifyCommitted( _raftLog.appendIndex() );
			  _applicationProcess.start();

			  // then
			  verify( _coreState ).flush( _batchSize - 1 );
			  verify( _coreState ).flush( 2 * _batchSize - 1 );
			  verify( _coreState ).flush( 3 * _batchSize - 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPanicIfUnableToApply() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPanicIfUnableToApply()
		 {
			  // given
			  doThrow( typeof( Exception ) ).when( _commandDispatcher ).dispatch( any( typeof( ReplicatedTransaction ) ), anyLong(), AnyCallback() );
			  _applicationProcess.start();

			  // when
			  _raftLog.append( new RaftLogEntry( 0, Operation( _nullTx ) ) );
			  _applicationProcess.notifyCommitted( 0 );

			  assertEventually( "failed apply", _dbHealth.isHealthy, @is( false ), 5, SECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyToLogFromCache() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyToLogFromCache()
		 {
			  // given
			  _inFlightCache.put( 0L, new RaftLogEntry( 1, Operation( _nullTx ) ) );

			  //when
			  _applicationProcess.notifyCommitted( 0 );
			  _applicationProcess.start();

			  //then the cache should have had it's get method called.
			  verify( _inFlightCache, times( 1 ) ).get( 0L );
			  verifyZeroInteractions( _raftLog );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void cacheEntryShouldBePurgedAfterBeingApplied() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CacheEntryShouldBePurgedAfterBeingApplied()
		 {
			  // given
			  _inFlightCache.put( 0L, new RaftLogEntry( 0, Operation( _nullTx ) ) );
			  _inFlightCache.put( 1L, new RaftLogEntry( 0, Operation( _nullTx ) ) );
			  _inFlightCache.put( 2L, new RaftLogEntry( 0, Operation( _nullTx ) ) );

			  // when
			  _applicationProcess.notifyCommitted( 0 );
			  _applicationProcess.start();

			  // then the cache should have had its get method called.
			  assertNull( _inFlightCache.get( 0L ) );
			  assertNotNull( _inFlightCache.get( 1L ) );
			  assertNotNull( _inFlightCache.get( 2L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenCacheAndLogMiss() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenCacheAndLogMiss()
		 {
			  // given
			  _inFlightCache.put( 0L, new RaftLogEntry( 0, Operation( _nullTx ) ) );
			  _raftLog.append( new RaftLogEntry( 0, Operation( _nullTx ) ) );
			  _raftLog.append( new RaftLogEntry( 1, Operation( _nullTx ) ) );

			  // when
			  _applicationProcess.notifyCommitted( 2 );
			  try
			  {
					_applicationProcess.start();
					fail();
			  }
			  catch ( System.InvalidOperationException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncreaseLastAppliedForStateMachineCommands() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIncreaseLastAppliedForStateMachineCommands()
		 {
			  // given
			  _raftLog.append( new RaftLogEntry( 0, Operation( _nullTx ) ) );
			  _raftLog.append( new RaftLogEntry( 0, Operation( _nullTx ) ) );
			  _raftLog.append( new RaftLogEntry( 0, Operation( _nullTx ) ) );

			  // when
			  _applicationProcess.notifyCommitted( 2 );
			  _applicationProcess.start();

			  // then
			  assertEquals( 2, _applicationProcess.lastApplied() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncreaseLastAppliedForOtherCommands() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIncreaseLastAppliedForOtherCommands()
		 {
			  // given
			  _raftLog.append( new RaftLogEntry( 0, new NewLeaderBarrier() ) );
			  _raftLog.append( new RaftLogEntry( 0, new NewLeaderBarrier() ) );
			  _raftLog.append( new RaftLogEntry( 0, new NewLeaderBarrier() ) );

			  // when
			  _applicationProcess.notifyCommitted( 2 );
			  _applicationProcess.start();

			  // then
			  assertEquals( 2, _applicationProcess.lastApplied() );
		 }

		 private System.Action<Result> AnyCallback()
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") System.Action<Result> anyCallback = any(System.Action.class);
			  System.Action<Result> anyCallback = any( typeof( System.Action ) );
			  return anyCallback;
		 }
	}

}
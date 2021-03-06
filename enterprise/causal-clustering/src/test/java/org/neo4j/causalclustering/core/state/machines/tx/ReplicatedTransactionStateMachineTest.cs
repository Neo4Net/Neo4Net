﻿using System;

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
namespace Org.Neo4j.causalclustering.core.state.machines.tx
{
	using Test = org.junit.Test;

	using CommandIndexTracker = Org.Neo4j.causalclustering.core.state.machines.id.CommandIndexTracker;
	using ReplicatedLockTokenRequest = Org.Neo4j.causalclustering.core.state.machines.locks.ReplicatedLockTokenRequest;
	using ReplicatedLockTokenStateMachine = Org.Neo4j.causalclustering.core.state.machines.locks.ReplicatedLockTokenStateMachine;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using PageCursorTracer = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracer;
	using PageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using TransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;
	using FakeCommitment = Org.Neo4j.Kernel.impl.transaction.log.FakeCommitment;
	using PhysicalTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using CommitEvent = Org.Neo4j.Kernel.impl.transaction.tracing.CommitEvent;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using TransactionApplicationMode = Org.Neo4j.Storageengine.Api.TransactionApplicationMode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ReplicatedTransactionStateMachineTest
	{
		 private readonly NullLogProvider _logProvider = NullLogProvider.Instance;
		 private readonly CommandIndexTracker _commandIndexTracker = new CommandIndexTracker();
		 private readonly int _batchSize = 16;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCommitTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCommitTransaction()
		 {
			  // given
			  int lockSessionId = 23;

			  ReplicatedTransaction tx = ReplicatedTransaction.from( PhysicalTx( lockSessionId ) );

			  TransactionCommitProcess localCommitProcess = mock( typeof( TransactionCommitProcess ) );
			  PageCursorTracer cursorTracer = mock( typeof( PageCursorTracer ) );

			  ReplicatedTransactionStateMachine stateMachine = new ReplicatedTransactionStateMachine( _commandIndexTracker, LockState( lockSessionId ), _batchSize, _logProvider, () => cursorTracer, EmptyVersionContextSupplier.EMPTY );
			  stateMachine.InstallCommitProcess( localCommitProcess, -1L );

			  // when
			  stateMachine.ApplyCommand(tx, 0, r =>
			  {
			  });
			  stateMachine.EnsuredApplied();

			  // then
			  verify( localCommitProcess, times( 1 ) ).commit( any( typeof( TransactionToApply ) ), any( typeof( CommitEvent ) ), any( typeof( TransactionApplicationMode ) ) );
			  verify( cursorTracer, times( 1 ) ).reportEvents();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailFutureForTransactionCommittedUnderWrongLockSession()
		 public virtual void ShouldFailFutureForTransactionCommittedUnderWrongLockSession()
		 {
			  // given
			  int txLockSessionId = 23;
			  int currentLockSessionId = 24;

			  ReplicatedTransaction tx = ReplicatedTransaction.from( PhysicalTx( txLockSessionId ) );

			  TransactionCommitProcess localCommitProcess = mock( typeof( TransactionCommitProcess ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ReplicatedTransactionStateMachine stateMachine = new ReplicatedTransactionStateMachine(commandIndexTracker, lockState(currentLockSessionId), batchSize, logProvider, org.neo4j.io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.NULL, org.neo4j.io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier.EMPTY);
			  ReplicatedTransactionStateMachine stateMachine = new ReplicatedTransactionStateMachine( _commandIndexTracker, LockState( currentLockSessionId ), _batchSize, _logProvider, Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, EmptyVersionContextSupplier.EMPTY );
			  stateMachine.InstallCommitProcess( localCommitProcess, -1L );

			  AtomicBoolean called = new AtomicBoolean();
			  // when
			  stateMachine.ApplyCommand(tx, 0, result =>
			  {
				// then
				called.set( true );
				try
				{
					 result.consume();
					 fail( "should have thrown" );
				}
				catch ( TransactionFailureException tfe )
				{
					 assertEquals( Status.Transaction.LockSessionExpired, tfe.status() );
				}
				catch ( Exception e )
				{
					 throw new Exception( e );
				}
			  });
			  stateMachine.EnsuredApplied();

			  assertTrue( called.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptTransactionCommittedWithNoLockManager() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcceptTransactionCommittedWithNoLockManager()
		 {
			  // given
			  int txLockSessionId = Org.Neo4j.Kernel.impl.locking.Locks_Client_Fields.NO_LOCK_SESSION_ID;
			  int currentLockSessionId = 24;
			  long txId = 42L;

			  ReplicatedTransaction tx = ReplicatedTransaction.from( PhysicalTx( txLockSessionId ) );

			  TransactionCommitProcess localCommitProcess = CreateFakeTransactionCommitProcess( txId );

			  ReplicatedTransactionStateMachine stateMachine = new ReplicatedTransactionStateMachine( _commandIndexTracker, LockState( currentLockSessionId ), _batchSize, _logProvider, Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, EmptyVersionContextSupplier.EMPTY );
			  stateMachine.InstallCommitProcess( localCommitProcess, -1L );

			  AtomicBoolean called = new AtomicBoolean();

			  // when
			  stateMachine.ApplyCommand(tx, 0, result =>
			  {
				// then
				called.set( true );
				try
				{
					 assertEquals( txId, ( long ) result.consume() );
				}
				catch ( Exception e )
				{
					 throw new Exception( e );
				}
			  });
			  stateMachine.EnsuredApplied();

			  assertTrue( called.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void raftIndexIsRecorded() throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RaftIndexIsRecorded()
		 {
			  // given
			  int txLockSessionId = Org.Neo4j.Kernel.impl.locking.Locks_Client_Fields.NO_LOCK_SESSION_ID;
			  long anyTransactionId = 1234;
			  long lastCommittedIndex = 1357;
			  long updatedCommandIndex = 2468;

			  // and
			  ReplicatedTransactionStateMachine stateMachine = new ReplicatedTransactionStateMachine( _commandIndexTracker, LockState( txLockSessionId ), _batchSize, _logProvider, Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, EmptyVersionContextSupplier.EMPTY );

			  ReplicatedTransaction replicatedTransaction = ReplicatedTransaction.from( PhysicalTx( txLockSessionId ) );

			  // and
			  TransactionCommitProcess localCommitProcess = CreateFakeTransactionCommitProcess( anyTransactionId );

			  // when
			  stateMachine.InstallCommitProcess( localCommitProcess, lastCommittedIndex );

			  // then
			  assertEquals( lastCommittedIndex, _commandIndexTracker.AppliedCommandIndex );

			  // when
			  stateMachine.ApplyCommand(replicatedTransaction, updatedCommandIndex, result =>
			  {
			  });
			  stateMachine.EnsuredApplied();

			  // then
			  assertEquals( updatedCommandIndex, _commandIndexTracker.AppliedCommandIndex );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.api.TransactionCommitProcess createFakeTransactionCommitProcess(long txId) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private TransactionCommitProcess CreateFakeTransactionCommitProcess( long txId )
		 {
			  TransactionCommitProcess localCommitProcess = mock( typeof( TransactionCommitProcess ) );
			  when( localCommitProcess.Commit( any( typeof( TransactionToApply ) ), any( typeof( CommitEvent ) ), any( typeof( TransactionApplicationMode ) ) ) ).thenAnswer(invocation =>
			  {
				TransactionToApply txToApply = invocation.getArgument( 0 );
				txToApply.commitment( new FakeCommitment( txId, mock( typeof( TransactionIdStore ) ) ), txId );
				txToApply.commitment().publishAsCommitted();
				txToApply.commitment().publishAsClosed();
				txToApply.close();
				return txId;
			  });
			  return localCommitProcess;
		 }

		 private PhysicalTransactionRepresentation PhysicalTx( int lockSessionId )
		 {
			  PhysicalTransactionRepresentation physicalTx = mock( typeof( PhysicalTransactionRepresentation ) );
			  when( physicalTx.LockSessionId ).thenReturn( lockSessionId );
			  return physicalTx;
		 }

		 private ReplicatedLockTokenStateMachine LockState( int lockSessionId )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.neo4j.causalclustering.core.state.machines.locks.ReplicatedLockTokenStateMachine lockState = mock(org.neo4j.causalclustering.core.state.machines.locks.ReplicatedLockTokenStateMachine.class);
			  ReplicatedLockTokenStateMachine lockState = mock( typeof( ReplicatedLockTokenStateMachine ) );
			  when( lockState.CurrentToken() ).thenReturn(new ReplicatedLockTokenRequest(null, lockSessionId));
			  return lockState;
		 }
	}

}
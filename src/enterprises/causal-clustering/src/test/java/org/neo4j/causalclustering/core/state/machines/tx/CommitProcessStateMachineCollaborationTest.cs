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
namespace Neo4Net.causalclustering.core.state.machines.tx
{
	using Test = org.junit.Test;

	using Neo4Net.causalclustering.core.replication;
	using CommandIndexTracker = Neo4Net.causalclustering.core.state.machines.id.CommandIndexTracker;
	using ReplicatedLockTokenRequest = Neo4Net.causalclustering.core.state.machines.locks.ReplicatedLockTokenRequest;
	using ReplicatedLockTokenStateMachine = Neo4Net.causalclustering.core.state.machines.locks.ReplicatedLockTokenStateMachine;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using TransactionCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using PhysicalTransactionRepresentation = Neo4Net.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.tracing.CommitEvent.NULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.TransactionApplicationMode.EXTERNAL;

	public class CommitProcessStateMachineCollaborationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailTransactionIfLockSessionChanges()
		 public virtual void ShouldFailTransactionIfLockSessionChanges()
		 {
			  // given
			  int initialLockSessionId = 23;
			  TransactionToApply transactionToApply = new TransactionToApply( PhysicalTx( initialLockSessionId ) );

			  int finalLockSessionId = 24;
			  TransactionCommitProcess localCommitProcess = mock( typeof( TransactionCommitProcess ) );
			  ReplicatedTransactionStateMachine stateMachine = new ReplicatedTransactionStateMachine( mock( typeof( CommandIndexTracker ) ), LockState( finalLockSessionId ), 16, NullLogProvider.Instance, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, EmptyVersionContextSupplier.EMPTY );
			  stateMachine.InstallCommitProcess( localCommitProcess, -1L );

			  DirectReplicator<ReplicatedTransaction> replicator = new DirectReplicator<ReplicatedTransaction>( stateMachine );
			  ReplicatedTransactionCommitProcess commitProcess = new ReplicatedTransactionCommitProcess( replicator );

			  // when
			  try
			  {
					commitProcess.Commit( transactionToApply, NULL, EXTERNAL );
					fail( "Should have thrown exception." );
			  }
			  catch ( TransactionFailureException )
			  {
					// expected
			  }
		 }

		 private PhysicalTransactionRepresentation PhysicalTx( int lockSessionId )
		 {
			  PhysicalTransactionRepresentation physicalTx = mock( typeof( PhysicalTransactionRepresentation ) );
			  when( physicalTx.LockSessionId ).thenReturn( lockSessionId );
			  return physicalTx;
		 }

		 private ReplicatedLockTokenStateMachine LockState( int lockSessionId )
		 {
			  ReplicatedLockTokenStateMachine lockState = mock( typeof( ReplicatedLockTokenStateMachine ) );
			  when( lockState.CurrentToken() ).thenReturn(new ReplicatedLockTokenRequest(null, lockSessionId));
			  return lockState;
		 }
	}

}
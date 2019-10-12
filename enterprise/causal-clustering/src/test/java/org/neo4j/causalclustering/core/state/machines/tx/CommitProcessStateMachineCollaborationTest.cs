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

	using Org.Neo4j.causalclustering.core.replication;
	using CommandIndexTracker = Org.Neo4j.causalclustering.core.state.machines.id.CommandIndexTracker;
	using ReplicatedLockTokenRequest = Org.Neo4j.causalclustering.core.state.machines.locks.ReplicatedLockTokenRequest;
	using ReplicatedLockTokenStateMachine = Org.Neo4j.causalclustering.core.state.machines.locks.ReplicatedLockTokenStateMachine;
	using PageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using TransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using PhysicalTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

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
			  ReplicatedTransactionStateMachine stateMachine = new ReplicatedTransactionStateMachine( mock( typeof( CommandIndexTracker ) ), LockState( finalLockSessionId ), 16, NullLogProvider.Instance, Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, EmptyVersionContextSupplier.EMPTY );
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
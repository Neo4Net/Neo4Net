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
namespace Neo4Net.causalclustering.core.state.machines.tx
{
	using Test = org.junit.Test;

	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using TransactionCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using CommitEvent = Neo4Net.Kernel.impl.transaction.tracing.CommitEvent;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using TransactionApplicationMode = Neo4Net.Storageengine.Api.TransactionApplicationMode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.tracing.CommitEvent.NULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.TransactionApplicationMode.EXTERNAL;

	public class ReplayableCommitProcessTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCommitTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCommitTransactions()
		 {
			  // given
			  TransactionToApply newTx1 = mock( typeof( TransactionToApply ) );
			  TransactionToApply newTx2 = mock( typeof( TransactionToApply ) );
			  TransactionToApply newTx3 = mock( typeof( TransactionToApply ) );

			  StubLocalDatabase localDatabase = new StubLocalDatabase( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ReplayableCommitProcess txListener = new ReplayableCommitProcess(localDatabase, localDatabase);
			  ReplayableCommitProcess txListener = new ReplayableCommitProcess( localDatabase, localDatabase );

			  // when
			  txListener.Commit( newTx1, NULL, EXTERNAL );
			  txListener.Commit( newTx2, NULL, EXTERNAL );
			  txListener.Commit( newTx3, NULL, EXTERNAL );

			  // then
			  verify( localDatabase.CommitProcess, times( 3 ) ).commit( any( typeof( TransactionToApply ) ), any( typeof( CommitEvent ) ), any( typeof( TransactionApplicationMode ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCommitTransactionsThatAreAlreadyCommittedLocally() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCommitTransactionsThatAreAlreadyCommittedLocally()
		 {
			  // given
			  TransactionToApply alreadyCommittedTx1 = mock( typeof( TransactionToApply ) );
			  TransactionToApply alreadyCommittedTx2 = mock( typeof( TransactionToApply ) );
			  TransactionToApply newTx = mock( typeof( TransactionToApply ) );

			  StubLocalDatabase localDatabase = new StubLocalDatabase( 3 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ReplayableCommitProcess txListener = new ReplayableCommitProcess(localDatabase, localDatabase);
			  ReplayableCommitProcess txListener = new ReplayableCommitProcess( localDatabase, localDatabase );

			  // when
			  txListener.Commit( alreadyCommittedTx1, NULL, EXTERNAL );
			  txListener.Commit( alreadyCommittedTx2, NULL, EXTERNAL );
			  txListener.Commit( newTx, NULL, EXTERNAL );

			  // then
			  verify( localDatabase.CommitProcess, times( 1 ) ).commit( eq( newTx ), any( typeof( CommitEvent ) ), any( typeof( TransactionApplicationMode ) ) );
			  verifyNoMoreInteractions( localDatabase.CommitProcess );
		 }

		 private class StubLocalDatabase : LifecycleAdapter, TransactionCounter, TransactionCommitProcess
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long LastCommittedTransactionIdConflict;
			  internal TransactionCommitProcess CommitProcess = mock( typeof( TransactionCommitProcess ) );

			  internal StubLocalDatabase( long lastCommittedTransactionId )
			  {
					this.LastCommittedTransactionIdConflict = lastCommittedTransactionId;
			  }

			  public override long LastCommittedTransactionId()
			  {
					return LastCommittedTransactionIdConflict;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long commit(org.neo4j.kernel.impl.api.TransactionToApply tx, org.neo4j.kernel.impl.transaction.tracing.CommitEvent commitEvent, org.neo4j.storageengine.api.TransactionApplicationMode mode) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
			  public override long Commit( TransactionToApply tx, CommitEvent commitEvent, TransactionApplicationMode mode )
			  {
					LastCommittedTransactionIdConflict++;
					return CommitProcess.commit( tx, commitEvent, mode );
			  }
		 }
	}

}
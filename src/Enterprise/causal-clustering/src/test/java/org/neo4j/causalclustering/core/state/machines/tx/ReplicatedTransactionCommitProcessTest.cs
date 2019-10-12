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
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using Replicator = Neo4Net.causalclustering.core.replication.Replicator;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using CommitEvent = Neo4Net.Kernel.impl.transaction.tracing.CommitEvent;
	using TransactionApplicationMode = Neo4Net.Storageengine.Api.TransactionApplicationMode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ReplicatedTransactionCommitProcessTest
	{
		 private Replicator _replicator = mock( typeof( Replicator ) );
		 private TransactionRepresentation _tx = mock( typeof( TransactionRepresentation ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void tx()
		 public virtual void Tx()
		 {
			  when( _tx.additionalHeader() ).thenReturn(new sbyte[]{});
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReplicateTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReplicateTransaction()
		 {
			  // given
			  CompletableFuture<object> futureTxId = new CompletableFuture<object>();
			  futureTxId.complete( 5L );

			  when( _replicator.replicate( any( typeof( ReplicatedContent ) ), anyBoolean() ) ).thenReturn(futureTxId);
			  ReplicatedTransactionCommitProcess commitProcess = new ReplicatedTransactionCommitProcess( _replicator );

			  // when
			  long txId = commitProcess.Commit( new TransactionToApply( _tx ), CommitEvent.NULL, TransactionApplicationMode.EXTERNAL );

			  // then
			  assertEquals( 5, txId );
		 }
	}

}
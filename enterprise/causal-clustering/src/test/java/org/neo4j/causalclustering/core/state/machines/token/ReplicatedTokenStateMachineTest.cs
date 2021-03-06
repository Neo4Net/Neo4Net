﻿using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.core.state.machines.token
{
	using Test = org.junit.Test;


	using TransactionFailureException = Org.Neo4j.Graphdb.TransactionFailureException;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using TransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionRepresentationCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionRepresentationCommitProcess;
	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using TokenRegistry = Org.Neo4j.Kernel.impl.core.TokenRegistry;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using TransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.TransactionRepresentation;
	using Command = Org.Neo4j.Kernel.impl.transaction.command.Command;
	using TransactionAppender = Org.Neo4j.Kernel.impl.transaction.log.TransactionAppender;
	using CommitEvent = Org.Neo4j.Kernel.impl.transaction.tracing.CommitEvent;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using StorageCommand = Org.Neo4j.Storageengine.Api.StorageCommand;
	using StorageEngine = Org.Neo4j.Storageengine.Api.StorageEngine;
	using TransactionApplicationMode = Org.Neo4j.Storageengine.Api.TransactionApplicationMode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.state.machines.token.ReplicatedTokenRequestSerializer.commandBytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.state.machines.token.TokenType.LABEL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.state.machines.tx.LogIndexTxHeaderEncoding.decodeLogIndexFromTxHeader;

	public class ReplicatedTokenStateMachineTest
	{
		 private readonly int _expectedTokenId = 1;
		 private readonly int _unexpectedTokenId = 1024;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateTokenId()
		 public virtual void ShouldCreateTokenId()
		 {
			  // given
			  TokenRegistry registry = new TokenRegistry( "Label" );
			  ReplicatedTokenStateMachine stateMachine = new ReplicatedTokenStateMachine( registry, NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );
			  stateMachine.InstallCommitProcess( mock( typeof( TransactionCommitProcess ) ), -1 );

			  // when
			  sbyte[] commandBytes = commandBytes( TokenCommands( _expectedTokenId ) );
			  stateMachine.ApplyCommand(new ReplicatedTokenRequest(LABEL, "Person", commandBytes), 1, r =>
			  {
			  });

			  // then
			  assertEquals( _expectedTokenId, ( int ) registry.GetId( "Person" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllocateTokenIdToFirstReplicateRequest()
		 public virtual void ShouldAllocateTokenIdToFirstReplicateRequest()
		 {
			  // given
			  TokenRegistry registry = new TokenRegistry( "Label" );
			  ReplicatedTokenStateMachine stateMachine = new ReplicatedTokenStateMachine( registry, NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );

			  stateMachine.InstallCommitProcess( mock( typeof( TransactionCommitProcess ) ), -1 );

			  ReplicatedTokenRequest winningRequest = new ReplicatedTokenRequest( LABEL, "Person", commandBytes( TokenCommands( _expectedTokenId ) ) );
			  ReplicatedTokenRequest losingRequest = new ReplicatedTokenRequest( LABEL, "Person", commandBytes( TokenCommands( _unexpectedTokenId ) ) );

			  // when
			  stateMachine.ApplyCommand(winningRequest, 1, r =>
			  {
			  });
			  stateMachine.ApplyCommand(losingRequest, 2, r =>
			  {
			  });

			  // then
			  assertEquals( _expectedTokenId, ( int ) registry.GetId( "Person" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStoreRaftLogIndexInTransactionHeader()
		 public virtual void ShouldStoreRaftLogIndexInTransactionHeader()
		 {
			  // given
			  int logIndex = 1;

			  StubTransactionCommitProcess commitProcess = new StubTransactionCommitProcess( null, null );
			  ReplicatedTokenStateMachine stateMachine = new ReplicatedTokenStateMachine( new TokenRegistry( "Token" ), NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );
			  stateMachine.InstallCommitProcess( commitProcess, -1 );

			  // when
			  sbyte[] commandBytes = commandBytes( TokenCommands( _expectedTokenId ) );
			  stateMachine.ApplyCommand(new ReplicatedTokenRequest(LABEL, "Person", commandBytes), logIndex, r =>
			  {
			  });

			  // then
			  IList<TransactionRepresentation> transactions = commitProcess.TransactionsToApply;
			  assertEquals( 1, transactions.Count );
			  assertEquals( logIndex, decodeLogIndexFromTxHeader( transactions[0].AdditionalHeader() ) );
		 }

		 private static IList<StorageCommand> TokenCommands( int expectedTokenId )
		 {
			  return singletonList(new Command.LabelTokenCommand(new LabelTokenRecord(expectedTokenId), new LabelTokenRecord(expectedTokenId)
			 ));
		 }

		 private class StubTransactionCommitProcess : TransactionRepresentationCommitProcess
		 {
			  internal readonly IList<TransactionRepresentation> TransactionsToApply = new List<TransactionRepresentation>();

			  internal StubTransactionCommitProcess( TransactionAppender appender, StorageEngine storageEngine ) : base( appender, storageEngine )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long commit(org.neo4j.kernel.impl.api.TransactionToApply batch, org.neo4j.kernel.impl.transaction.tracing.CommitEvent commitEvent, org.neo4j.storageengine.api.TransactionApplicationMode mode) throws org.neo4j.graphdb.TransactionFailureException
			  public override long Commit( TransactionToApply batch, CommitEvent commitEvent, TransactionApplicationMode mode )
			  {
					TransactionsToApply.Add( batch.TransactionRepresentation() );
					return -1;
			  }
		 }
	}

}
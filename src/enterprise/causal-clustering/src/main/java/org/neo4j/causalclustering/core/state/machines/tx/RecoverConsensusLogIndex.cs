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
namespace Neo4Net.causalclustering.core.state.machines.tx
{
	using LocalDatabase = Neo4Net.causalclustering.catchup.storecopy.LocalDatabase;
	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using LogicalTransactionStore = Neo4Net.Kernel.impl.transaction.log.LogicalTransactionStore;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.DependencyResolver_SelectionStrategy.ONLY;

	/// <summary>
	/// Retrieves last raft log index that was appended to the transaction log, so that raft log replay can recover while
	/// preserving idempotency (avoid appending the same transaction twice).
	/// </summary>
	public class RecoverConsensusLogIndex
	{
		 private readonly LocalDatabase _localDatabase;
		 private readonly LogProvider _logProvider;

		 public RecoverConsensusLogIndex( LocalDatabase localDatabase, LogProvider logProvider )
		 {
			  this._localDatabase = localDatabase;
			  this._logProvider = logProvider;
		 }

		 public virtual long FindLastAppliedIndex()
		 {
			  DependencyResolver dependencies = _localDatabase.dataSource().DependencyResolver;
			  TransactionIdStore transactionIdStore = dependencies.ResolveDependency( typeof( TransactionIdStore ), ONLY );
			  LogicalTransactionStore transactionStore = dependencies.ResolveDependency( typeof( LogicalTransactionStore ), ONLY );

			  return ( new LastCommittedIndexFinder( transactionIdStore, transactionStore, _logProvider ) ).LastCommittedIndex;
		 }
	}

}
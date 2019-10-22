/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Kernel.impl.query
{
	using Lock = Neo4Net.GraphDb.Lock;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using SecurityContext = Neo4Net.Internal.Kernel.Api.security.SecurityContext;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using ResourceTracker = Neo4Net.Kernel.api.ResourceTracker;
	using Statement = Neo4Net.Kernel.api.Statement;
	using DbmsOperations = Neo4Net.Kernel.api.dbms.DbmsOperations;
	using ExecutingQuery = Neo4Net.Kernel.api.query.ExecutingQuery;
	using TxStateHolder = Neo4Net.Kernel.api.txstate.TxStateHolder;
	using StatisticProvider = Neo4Net.Kernel.impl.query.statistic.StatisticProvider;

	public interface TransactionalContext
	{
		 ExecutingQuery ExecutingQuery();

		 DbmsOperations DbmsOperations();

		 KernelTransaction KernelTransaction();

		 bool TopLevelTx { get; }

		 /// <summary>
		 /// This should be called once the query is finished, either successfully or not.
		 /// Should be called from the same thread the query was executing in. </summary>
		 /// <param name="success"> signals if the underlying transaction should be committed or rolled back. </param>
		 void Close( bool success );

		 /// <summary>
		 /// This is used to terminate a currently running query. Can be called from any thread. Will roll back the current
		 /// transaction if it is still open.
		 /// </summary>
		 void Terminate();

		 void CommitAndRestartTx();

		 void CleanForReuse();

		 TransactionalContext OrBeginNewIfClosed { get; }

		 bool Open { get; }

		 GraphDatabaseQueryService Graph();

		 Statement Statement();

		 /// <summary>
		 /// Check that current context satisfy current execution guard.
		 /// In case if guard criteria is not satisfied <seealso cref="org.Neo4Net.graphdb.TransactionGuardException"/> will be
		 /// thrown.
		 /// </summary>
		 void Check();

		 TxStateHolder StateView();

		 Lock AcquireWriteLock( IPropertyContainer p );

		 SecurityContext SecurityContext();

		 StatisticProvider KernelStatisticProvider();

		 Neo4Net.Kernel.api.KernelTransaction_Revertable RestrictCurrentTransaction( SecurityContext context );

		 ResourceTracker ResourceTracker();
	}

}
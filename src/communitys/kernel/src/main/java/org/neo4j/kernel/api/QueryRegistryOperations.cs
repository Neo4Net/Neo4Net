using System.Collections.Generic;

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
namespace Neo4Net.Kernel.api
{

	using ExecutingQuery = Neo4Net.Kernel.api.query.ExecutingQuery;
	using ClientConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.ClientConnectionInfo;
	using MapValue = Neo4Net.Values.@virtual.MapValue;


	/// <summary>
	/// Tracks currently running stream. This is used for listing currently running stream and to make it possible to
	/// terminate a query, not matter which or how many transactions it's working in.
	/// 
	/// If a query uses multiple transactions (think of PERIODIC COMMIT), the query needs to be registered to all
	/// transactions it uses.
	/// </summary>
	public interface QueryRegistryOperations
	{
		 /// <summary>
		 /// Sets the user defined meta data to be associated with started queries. </summary>
		 /// <param name="data"> the meta data </param>
		 IDictionary<string, object> MetaData { set;get; }


		 /// <summary>
		 /// List of all currently running stream in this transaction. An user can have multiple stream running
		 /// simultaneously on the same transaction.
		 /// </summary>
		 Stream<ExecutingQuery> ExecutingQueries();

		 /// <summary>
		 /// Registers a query, and creates the ExecutingQuery object for it.
		 /// </summary>
		 ExecutingQuery StartQueryExecution( ClientConnectionInfo descriptor, string queryText, MapValue queryParameters );

		 /// <summary>
		 /// Registers an already known query to a this transaction.
		 /// 
		 /// This is used solely for supporting PERIODIC COMMIT which requires committing and starting new transactions
		 /// and associating the same ExecutingQuery with those new transactions.
		 /// </summary>
		 void RegisterExecutingQuery( ExecutingQuery executingQuery );

		 /// <summary>
		 /// Disassociates a query with this transaction.
		 /// </summary>
		 void UnregisterExecutingQuery( ExecutingQuery executingQuery );
	}

}
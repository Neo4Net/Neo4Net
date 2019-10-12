using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.Impl.Api
{

	using QueryRegistryOperations = Neo4Net.Kernel.api.QueryRegistryOperations;
	using ExecutingQuery = Neo4Net.Kernel.api.query.ExecutingQuery;
	using QueryRegistrationOperations = Neo4Net.Kernel.Impl.Api.operations.QueryRegistrationOperations;
	using ClientConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.ClientConnectionInfo;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

	public class OperationsFacade : QueryRegistryOperations
	{
		 private readonly KernelStatement _statement;
		 private StatementOperationParts _operations;

		 internal OperationsFacade( KernelStatement statement, StatementOperationParts operationParts )
		 {
			  this._statement = statement;
			  this._operations = operationParts;
		 }

		 internal QueryRegistrationOperations QueryRegistrationOperations()
		 {
			  return _operations.queryRegistrationOperations();
		 }

		 // query monitoring

		 public virtual IDictionary<string, object> MetaData
		 {
			 set
			 {
				  _statement.assertOpen();
				  _statement.Transaction.MetaData = value;
			 }
			 get
			 {
				  _statement.assertOpen();
				  return _statement.Transaction.MetaData;
			 }
		 }


		 public override Stream<ExecutingQuery> ExecutingQueries()
		 {
			  _statement.assertOpen();
			  return QueryRegistrationOperations().executingQueries(_statement);
		 }

		 public override ExecutingQuery StartQueryExecution( ClientConnectionInfo descriptor, string queryText, MapValue queryParameters )
		 {
			  _statement.assertOpen();
			  return QueryRegistrationOperations().startQueryExecution(_statement, descriptor, queryText, queryParameters);
		 }

		 public override void RegisterExecutingQuery( ExecutingQuery executingQuery )
		 {
			  _statement.assertOpen();
			  QueryRegistrationOperations().registerExecutingQuery(_statement, executingQuery);
		 }

		 public override void UnregisterExecutingQuery( ExecutingQuery executingQuery )
		 {
			  QueryRegistrationOperations().unregisterExecutingQuery(_statement, executingQuery);
		 }

		 // query monitoring
	}

}
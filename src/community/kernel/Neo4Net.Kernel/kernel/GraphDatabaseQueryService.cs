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
namespace Neo4Net.Kernel
{

	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using URLAccessValidationError = Neo4Net.Graphdb.security.URLAccessValidationError;
	using LoginContext = Neo4Net.Internal.Kernel.Api.security.LoginContext;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using DbmsOperations = Neo4Net.Kernel.api.dbms.DbmsOperations;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;

	/*
	 * This is a trimmed down version of GraphDatabaseService and GraphDatabaseAPI, limited to a subset of functions needed
	 * by implementations of QueryExecutionEngine.
	 */
	public interface GraphDatabaseQueryService
	{
		 DependencyResolver DependencyResolver { get; }

		 /// <summary>
		 /// Begin new internal transaction with with default timeout.
		 /// </summary>
		 /// <param name="type"> transaction type </param>
		 /// <param name="loginContext"> transaction login context </param>
		 /// <returns> internal transaction </returns>
		 InternalTransaction BeginTransaction( KernelTransaction.Type type, LoginContext loginContext );

		 /// <summary>
		 /// Begin new internal transaction with specified timeout in milliseconds.
		 /// </summary>
		 /// <param name="type"> transaction type </param>
		 /// <param name="loginContext"> transaction login context </param>
		 /// <param name="timeout"> transaction timeout </param>
		 /// <param name="unit"> time unit of timeout argument </param>
		 /// <returns> internal transaction </returns>
		 InternalTransaction BeginTransaction( KernelTransaction.Type type, LoginContext loginContext, long timeout, TimeUnit unit );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.net.URL validateURLAccess(java.net.URL url) throws org.neo4j.graphdb.security.URLAccessValidationError;
		 URL ValidateURLAccess( URL url );

		 DbmsOperations DbmsOperations { get; }
	}

}
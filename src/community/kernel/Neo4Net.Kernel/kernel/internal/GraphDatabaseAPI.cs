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
namespace Neo4Net.Kernel.Internal
{

	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using URLAccessValidationError = Neo4Net.GraphDb.security.URLAccessValidationError;
	using LoginContext = Neo4Net.Kernel.Api.Internal.security.LoginContext;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using StoreId = Neo4Net.Kernel.Api.StorageEngine.StoreId;

	/// <summary>
	/// This API can be used to get access to services.
	/// </summary>
	public interface GraphDatabaseAPI : IGraphDatabaseService
	{
		 /// <summary>
		 /// Look up database components for direct access.
		 /// Usage of this method is generally an indication of architectural error.
		 /// </summary>
		 DependencyResolver DependencyResolver { get; }

		 /// <summary>
		 /// Provides the unique id assigned to this database. </summary>
		 StoreId StoreId();

		 /// <summary>
		 /// Validate whether this database instance is permitted to reach out to the specified URL (e.g. when using {@code LOAD CSV} in Cypher).
		 /// </summary>
		 /// <param name="url"> the URL being validated </param>
		 /// <returns> an updated URL that should be used for accessing the resource </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.net.URL validateURLAccess(java.net.URL url) throws org.Neo4Net.graphdb.security.URLAccessValidationError;
		 URL ValidateURLAccess( URL url );

		 /// <returns> underlying database directory </returns>
		 DatabaseLayout DatabaseLayout();

		 /// <summary>
		 /// Begin internal transaction with specified type and access mode </summary>
		 /// <param name="type"> transaction type </param>
		 /// <param name="loginContext"> transaction login context </param>
		 /// <returns> internal transaction </returns>
		 InternalTransaction BeginTransaction( KernelTransaction.Type type, LoginContext loginContext );

		 /// <summary>
		 /// Begin internal transaction with specified type, access mode and timeout </summary>
		 /// <param name="type"> transaction type </param>
		 /// <param name="loginContext"> transaction login context </param>
		 /// <param name="timeout"> transaction timeout </param>
		 /// <param name="unit"> time unit of timeout argument </param>
		 /// <returns> internal transaction </returns>
		 InternalTransaction BeginTransaction( KernelTransaction.Type type, LoginContext loginContext, long timeout, TimeUnit unit );
	}

}
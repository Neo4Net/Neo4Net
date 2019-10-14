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
namespace Neo4Net.Server.rest.transactional
{

	using LoginContext = Neo4Net.@internal.Kernel.Api.security.LoginContext;
	using GraphDatabaseQueryService = Neo4Net.Kernel.GraphDatabaseQueryService;
	using QueryExecutionEngine = Neo4Net.Kernel.impl.query.QueryExecutionEngine;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using TransactionLifecycleException = Neo4Net.Server.rest.transactional.error.TransactionLifecycleException;
	using TransactionUriScheme = Neo4Net.Server.rest.web.TransactionUriScheme;

	/// <summary>
	/// Transactional actions contains the business logic for executing statements against Neo4j across long-running
	/// transactions.
	/// <para>
	/// The idiom for the public methods here is:
	/// </para>
	/// <para>
	/// response.begin()
	/// try {
	/// // Do internal calls, saving errors into a common error list
	/// } catch ( Neo4jError e )
	/// {
	/// errors.add(e);
	/// } finally
	/// {
	/// response.finish(errors)
	/// }
	/// </para>
	/// <para>
	/// This is done to ensure we stick to the contract of the response handler, which is important, because if we skimp on
	/// it, clients may be left waiting for results that never arrive.
	/// </para>
	/// </summary>
	public class TransactionFacade
	{
		 private readonly TransitionalPeriodTransactionMessContainer _kernel;
		 private readonly QueryExecutionEngine _engine;
		 private readonly TransactionRegistry _registry;
		 private readonly LogProvider _logProvider;
		 private GraphDatabaseQueryService _queryService;

		 public TransactionFacade( TransitionalPeriodTransactionMessContainer kernel, QueryExecutionEngine engine, GraphDatabaseQueryService queryService, TransactionRegistry registry, LogProvider logProvider )
		 {
			  this._kernel = kernel;
			  this._engine = engine;
			  this._queryService = queryService;
			  this._registry = registry;
			  this._logProvider = logProvider;
		 }

		 public virtual TransactionHandle NewTransactionHandle( TransactionUriScheme uriScheme, bool implicitTransaction, LoginContext loginContext, long customTransactionTimeout )
		 {
			  return new TransactionHandle( _kernel, _engine, _queryService, _registry, uriScheme, implicitTransaction, loginContext, customTransactionTimeout, _logProvider );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TransactionHandle findTransactionHandle(long txId) throws org.neo4j.server.rest.transactional.error.TransactionLifecycleException
		 public virtual TransactionHandle FindTransactionHandle( long txId )
		 {
			  return _registry.acquire( txId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TransactionHandle terminate(long txId) throws org.neo4j.server.rest.transactional.error.TransactionLifecycleException
		 public virtual TransactionHandle Terminate( long txId )
		 {
			  return _registry.terminate( txId );
		 }

		 public virtual StatementDeserializer Deserializer( Stream input )
		 {
			  return new StatementDeserializer( input );
		 }

		 public virtual ExecutionResultSerializer Serializer( Stream output, URI baseUri )
		 {
			  return new ExecutionResultSerializer( output, baseUri, _logProvider, _kernel );
		 }
	}

}
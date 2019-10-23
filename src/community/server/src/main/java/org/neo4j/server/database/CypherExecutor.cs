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
namespace Neo4Net.Server.database
{

	using ExecutionEngine = Neo4Net.Cypher.Internal.javacompat.ExecutionEngine;
	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using GraphDatabaseQueryService = Neo4Net.Kernel.GraphDatabaseQueryService;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using IPropertyContainerLocker = Neo4Net.Kernel.impl.coreapi.PropertyContainerLocker;
	using Neo4NetTransactionalContextFactory = Neo4Net.Kernel.impl.query.Neo4NetTransactionalContextFactory;
	using QueryExecutionEngine = Neo4Net.Kernel.impl.query.QueryExecutionEngine;
	using TransactionalContext = Neo4Net.Kernel.impl.query.TransactionalContext;
	using TransactionalContextFactory = Neo4Net.Kernel.impl.query.TransactionalContextFactory;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using HttpConnectionInfoFactory = Neo4Net.Server.rest.web.HttpConnectionInfoFactory;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.security.LoginContext.AUTH_DISABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.web.HttpHeaderUtils.getTransactionTimeout;

	public class CypherExecutor : LifecycleAdapter
	{
		 private readonly Database _database;
		 private readonly Log _log;
		 private ExecutionEngine _executionEngine;
		 private TransactionalContextFactory _contextFactory;

		 private static readonly IPropertyContainerLocker _locker = new IPropertyContainerLocker();
		 private GraphDatabaseQueryService _service;

		 public CypherExecutor( Database database, LogProvider logProvider )
		 {
			  this._database = database;
			  _log = logProvider.getLog( this.GetType() );
		 }

		 public virtual ExecutionEngine ExecutionEngine
		 {
			 get
			 {
				  return _executionEngine;
			 }
		 }

		 public override void Start()
		 {
			  DependencyResolver resolver = _database.Graph.DependencyResolver;
			  this._executionEngine = ( ExecutionEngine ) resolver.ResolveDependency( typeof( QueryExecutionEngine ) );
			  this._service = resolver.ResolveDependency( typeof( GraphDatabaseQueryService ) );
			  this._contextFactory = Neo4NetTransactionalContextFactory.create( this._service, _locker );
		 }

		 public override void Stop()
		 {
			  this._executionEngine = null;
			  this._contextFactory = null;
		 }

		 public virtual TransactionalContext CreateTransactionContext( string query, MapValue parameters, HttpServletRequest request )
		 {
			  InternalTransaction tx = GetInternalTransaction( request );
			  return _contextFactory.newContext( HttpConnectionInfoFactory.create( request ), tx, query, parameters );
		 }

		 private InternalTransaction GetInternalTransaction( HttpServletRequest request )
		 {
			  long customTimeout = getTransactionTimeout( request, _log );
			  return customTimeout > GraphDatabaseSettings.UNSPECIFIED_TIMEOUT ? BeginCustomTransaction( customTimeout ) : BeginDefaultTransaction();
		 }

		 private InternalTransaction BeginCustomTransaction( long customTimeout )
		 {
			  return _service.BeginTransaction( KernelTransaction.Type.@implicit, AUTH_DISABLED, customTimeout, TimeUnit.MILLISECONDS );
		 }

		 private InternalTransaction BeginDefaultTransaction()
		 {
			  return _service.BeginTransaction( KernelTransaction.Type.@implicit, AUTH_DISABLED );
		 }
	}

}
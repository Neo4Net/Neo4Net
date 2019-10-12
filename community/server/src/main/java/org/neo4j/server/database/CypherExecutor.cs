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
namespace Org.Neo4j.Server.database
{

	using ExecutionEngine = Org.Neo4j.Cypher.@internal.javacompat.ExecutionEngine;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using GraphDatabaseQueryService = Org.Neo4j.Kernel.GraphDatabaseQueryService;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using InternalTransaction = Org.Neo4j.Kernel.impl.coreapi.InternalTransaction;
	using PropertyContainerLocker = Org.Neo4j.Kernel.impl.coreapi.PropertyContainerLocker;
	using Neo4jTransactionalContextFactory = Org.Neo4j.Kernel.impl.query.Neo4jTransactionalContextFactory;
	using QueryExecutionEngine = Org.Neo4j.Kernel.impl.query.QueryExecutionEngine;
	using TransactionalContext = Org.Neo4j.Kernel.impl.query.TransactionalContext;
	using TransactionalContextFactory = Org.Neo4j.Kernel.impl.query.TransactionalContextFactory;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using HttpConnectionInfoFactory = Org.Neo4j.Server.rest.web.HttpConnectionInfoFactory;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.LoginContext.AUTH_DISABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.web.HttpHeaderUtils.getTransactionTimeout;

	public class CypherExecutor : LifecycleAdapter
	{
		 private readonly Database _database;
		 private readonly Log _log;
		 private ExecutionEngine _executionEngine;
		 private TransactionalContextFactory _contextFactory;

		 private static readonly PropertyContainerLocker _locker = new PropertyContainerLocker();
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
			  this._contextFactory = Neo4jTransactionalContextFactory.create( this._service, _locker );
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
			  return _service.beginTransaction( KernelTransaction.Type.@implicit, AUTH_DISABLED, customTimeout, TimeUnit.MILLISECONDS );
		 }

		 private InternalTransaction BeginDefaultTransaction()
		 {
			  return _service.beginTransaction( KernelTransaction.Type.@implicit, AUTH_DISABLED );
		 }
	}

}
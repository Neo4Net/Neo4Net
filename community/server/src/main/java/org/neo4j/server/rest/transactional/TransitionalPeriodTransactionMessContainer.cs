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
namespace Org.Neo4j.Server.rest.transactional
{

	using Transaction_Type = Org.Neo4j.@internal.Kernel.Api.Transaction_Type;
	using LoginContext = Org.Neo4j.@internal.Kernel.Api.security.LoginContext;
	using GraphDatabaseQueryService = Org.Neo4j.Kernel.GraphDatabaseQueryService;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using InternalTransaction = Org.Neo4j.Kernel.impl.coreapi.InternalTransaction;
	using PropertyContainerLocker = Org.Neo4j.Kernel.impl.coreapi.PropertyContainerLocker;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using Neo4jTransactionalContextFactory = Org.Neo4j.Kernel.impl.query.Neo4jTransactionalContextFactory;
	using TransactionalContext = Org.Neo4j.Kernel.impl.query.TransactionalContext;
	using TransactionalContextFactory = Org.Neo4j.Kernel.impl.query.TransactionalContextFactory;
	using ClientConnectionInfo = Org.Neo4j.Kernel.impl.query.clientconnection.ClientConnectionInfo;
	using ValueUtils = Org.Neo4j.Kernel.impl.util.ValueUtils;
	using HttpConnectionInfoFactory = Org.Neo4j.Server.rest.web.HttpConnectionInfoFactory;

	public class TransitionalPeriodTransactionMessContainer
	{
		 private static readonly PropertyContainerLocker _locker = new PropertyContainerLocker();

		 private readonly GraphDatabaseFacade _db;
		 private readonly ThreadToStatementContextBridge _txBridge;

		 public TransitionalPeriodTransactionMessContainer( GraphDatabaseFacade db )
		 {
			  this._db = db;
			  this._txBridge = Db.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) );
		 }

		 public virtual TransitionalTxManagementKernelTransaction NewTransaction( Transaction_Type type, LoginContext loginContext, long customTransactionTimeout )
		 {
			  return new TransitionalTxManagementKernelTransaction( _db, type, loginContext, customTransactionTimeout, _txBridge );
		 }

		 internal virtual ThreadToStatementContextBridge Bridge
		 {
			 get
			 {
				  return _txBridge;
			 }
		 }

		 public virtual TransactionalContext Create( HttpServletRequest request, GraphDatabaseQueryService service, Transaction_Type type, LoginContext loginContext, string query, IDictionary<string, object> queryParameters )
		 {
			  TransactionalContextFactory contextFactory = Neo4jTransactionalContextFactory.create( service, _locker );
			  ClientConnectionInfo clientConnection = HttpConnectionInfoFactory.create( request );
			  InternalTransaction transaction = service.BeginTransaction( type, loginContext );
			  return contextFactory.NewContext( clientConnection, transaction, query, ValueUtils.asMapValue( queryParameters ) );
		 }
	}

}
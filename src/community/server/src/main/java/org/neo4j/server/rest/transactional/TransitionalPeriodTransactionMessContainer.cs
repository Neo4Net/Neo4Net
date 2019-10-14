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
namespace Neo4Net.Server.rest.transactional
{

	using Transaction_Type = Neo4Net.Internal.Kernel.Api.Transaction_Type;
	using LoginContext = Neo4Net.Internal.Kernel.Api.security.LoginContext;
	using GraphDatabaseQueryService = Neo4Net.Kernel.GraphDatabaseQueryService;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using PropertyContainerLocker = Neo4Net.Kernel.impl.coreapi.PropertyContainerLocker;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using Neo4jTransactionalContextFactory = Neo4Net.Kernel.impl.query.Neo4jTransactionalContextFactory;
	using TransactionalContext = Neo4Net.Kernel.impl.query.TransactionalContext;
	using TransactionalContextFactory = Neo4Net.Kernel.impl.query.TransactionalContextFactory;
	using ClientConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.ClientConnectionInfo;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using HttpConnectionInfoFactory = Neo4Net.Server.rest.web.HttpConnectionInfoFactory;

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
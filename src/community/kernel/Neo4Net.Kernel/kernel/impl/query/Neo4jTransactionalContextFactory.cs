using System;

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

	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using Kernel = Neo4Net.Kernel.Api.Internal.Kernel;
	using Statement = Neo4Net.Kernel.api.Statement;
	using ExecutingQuery = Neo4Net.Kernel.api.query.ExecutingQuery;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using IPropertyContainerLocker = Neo4Net.Kernel.impl.coreapi.PropertyContainerLocker;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using ClientConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.ClientConnectionInfo;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.function.Suppliers.lazySingleton;

	public class Neo4NetTransactionalContextFactory : TransactionalContextFactory
	{
		 private readonly System.Func<Statement> _statementSupplier;
		 private readonly Neo4NetTransactionalContext.Creator _contextCreator;

		 public static TransactionalContextFactory Create( GraphDatabaseFacade.SPI spi, ThreadToStatementContextBridge txBridge, IPropertyContainerLocker locker )
		 {
			  System.Func<GraphDatabaseQueryService> queryService = lazySingleton( spi.queryService );
			  System.Func<Kernel> kernel = lazySingleton( spi.kernel );
			  Neo4NetTransactionalContext.Creator contextCreator = ( tx, initialStatement, executingQuery ) => new Neo4NetTransactionalContext( queryService(), txBridge, locker, tx, initialStatement, executingQuery, kernel() );

			  return new Neo4NetTransactionalContextFactory( txBridge, contextCreator );
		 }

		 [Obsolete]
		 public static TransactionalContextFactory Create( GraphDatabaseQueryService queryService, IPropertyContainerLocker locker )
		 {
			  DependencyResolver resolver = queryService.DependencyResolver;
			  ThreadToStatementContextBridge txBridge = resolver.ResolveDependency( typeof( ThreadToStatementContextBridge ) );
			  Kernel kernel = resolver.ResolveDependency( typeof( Kernel ) );
			  Neo4NetTransactionalContext.Creator contextCreator = ( tx, initialStatement, executingQuery ) => new Neo4NetTransactionalContext( queryService, txBridge, locker, tx, initialStatement, executingQuery, kernel );

			  return new Neo4NetTransactionalContextFactory( txBridge, contextCreator );
		 }

		 // Please use the factory methods above to actually construct an instance
		 private Neo4NetTransactionalContextFactory( System.Func<Statement> statementSupplier, Neo4NetTransactionalContext.Creator contextCreator )
		 {
			  this._statementSupplier = statementSupplier;
			  this._contextCreator = contextCreator;
		 }

		 public override Neo4NetTransactionalContext NewContext( ClientConnectionInfo clientConnection, InternalTransaction tx, string queryText, MapValue queryParameters )
		 {
			  Statement initialStatement = _statementSupplier.get();
			  ClientConnectionInfo connectionWithUserName = clientConnection.WithUsername( tx.SecurityContext().subject().username() );
			  ExecutingQuery executingQuery = initialStatement.QueryRegistration().startQueryExecution(connectionWithUserName, queryText, queryParameters);
			  return _contextCreator.create( tx, initialStatement, executingQuery );
		 }
	}

}
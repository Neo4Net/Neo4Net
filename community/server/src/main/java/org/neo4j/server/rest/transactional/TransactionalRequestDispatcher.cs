using System;

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
	using HttpContext = com.sun.jersey.api.core.HttpContext;
	using RequestDispatcher = com.sun.jersey.spi.dispatch.RequestDispatcher;

	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using LoginContext = Org.Neo4j.@internal.Kernel.Api.security.LoginContext;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using Database = Org.Neo4j.Server.database.Database;
	using AuthorizedRequestWrapper = Org.Neo4j.Server.rest.dbms.AuthorizedRequestWrapper;
	using RepresentationWriteHandler = Org.Neo4j.Server.rest.repr.RepresentationWriteHandler;
	using BatchOperationService = Org.Neo4j.Server.rest.web.BatchOperationService;
	using CypherService = Org.Neo4j.Server.rest.web.CypherService;
	using DatabaseMetadataService = Org.Neo4j.Server.rest.web.DatabaseMetadataService;
	using ExtensionService = Org.Neo4j.Server.rest.web.ExtensionService;
	using RestfulGraphDatabase = Org.Neo4j.Server.rest.web.RestfulGraphDatabase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.repr.RepresentationWriteHandler.DO_NOTHING;

	public class TransactionalRequestDispatcher : RequestDispatcher
	{
		 private readonly Database _database;
		 private readonly RequestDispatcher _requestDispatcher;

		 public TransactionalRequestDispatcher( Database database, RequestDispatcher requestDispatcher )
		 {
			  this._database = database;
			  this._requestDispatcher = requestDispatcher;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void dispatch(Object o, final com.sun.jersey.api.core.HttpContext httpContext)
		 public override void Dispatch( object o, HttpContext httpContext )
		 {
			  RepresentationWriteHandler representationWriteHandler = DO_NOTHING;

			  LoginContext loginContext = AuthorizedRequestWrapper.getLoginContextFromHttpContext( httpContext );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.factory.GraphDatabaseFacade graph = database.getGraph();
			  GraphDatabaseFacade graph = _database.Graph;
			  if ( o is RestfulGraphDatabase )
			  {
					RestfulGraphDatabase restfulGraphDatabase = ( RestfulGraphDatabase ) o;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Transaction transaction = graph.beginTransaction(org.neo4j.kernel.api.KernelTransaction.Type.implicit, loginContext);
					Transaction transaction = graph.BeginTransaction( KernelTransaction.Type.@implicit, loginContext );

					restfulGraphDatabase.OutputFormat.RepresentationWriteHandler = representationWriteHandler = new CommitOnSuccessfulStatusCodeRepresentationWriteHandler( httpContext, transaction );
			  }
			  else if ( o is BatchOperationService )
			  {
					BatchOperationService batchOperationService = ( BatchOperationService ) o;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Transaction transaction = graph.beginTransaction(org.neo4j.kernel.api.KernelTransaction.Type.explicit, loginContext);
					Transaction transaction = graph.BeginTransaction( KernelTransaction.Type.@explicit, loginContext );

					batchOperationService.RepresentationWriteHandler = representationWriteHandler = new CommitOnSuccessfulStatusCodeRepresentationWriteHandler( httpContext, transaction );
			  }
			  else if ( o is CypherService )
			  {
					CypherService cypherService = ( CypherService ) o;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Transaction transaction = graph.beginTransaction(org.neo4j.kernel.api.KernelTransaction.Type.explicit, loginContext);
					Transaction transaction = graph.BeginTransaction( KernelTransaction.Type.@explicit, loginContext );

					cypherService.OutputFormat.RepresentationWriteHandler = representationWriteHandler = new CommitOnSuccessfulStatusCodeRepresentationWriteHandler( httpContext, transaction );
			  }
			  else if ( o is DatabaseMetadataService )
			  {
					DatabaseMetadataService databaseMetadataService = ( DatabaseMetadataService ) o;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Transaction transaction = graph.beginTransaction(org.neo4j.kernel.api.KernelTransaction.Type.implicit, loginContext);
					Transaction transaction = graph.BeginTransaction( KernelTransaction.Type.@implicit, loginContext );

					databaseMetadataService.RepresentationWriteHandler = representationWriteHandler = new RepresentationWriteHandlerAnonymousInnerClass( this, transaction );
			  }
			  else if ( o is ExtensionService )
			  {
					ExtensionService extensionService = ( ExtensionService ) o;
					extensionService.OutputFormat.RepresentationWriteHandler = representationWriteHandler = new RepresentationWriteHandlerAnonymousInnerClass2( this, loginContext, graph );
			  }

			  try
			  {
					_requestDispatcher.dispatch( o, httpContext );
			  }
			  catch ( Exception e )
			  {
					representationWriteHandler.OnRepresentationFinal();

					throw e;
			  }
		 }

		 private class RepresentationWriteHandlerAnonymousInnerClass : RepresentationWriteHandler
		 {
			 private readonly TransactionalRequestDispatcher _outerInstance;

			 private Transaction _transaction;

			 public RepresentationWriteHandlerAnonymousInnerClass( TransactionalRequestDispatcher outerInstance, Transaction transaction )
			 {
				 this.outerInstance = outerInstance;
				 this._transaction = transaction;
			 }

			 public void onRepresentationStartWriting()
			 {
				  // do nothing
			 }

			 public void onRepresentationWritten()
			 {
				  // doesn't need to commit
			 }

			 public void onRepresentationFinal()
			 {
				  _transaction.close();
			 }
		 }

		 private class RepresentationWriteHandlerAnonymousInnerClass2 : RepresentationWriteHandler
		 {
			 private readonly TransactionalRequestDispatcher _outerInstance;

			 private LoginContext _loginContext;
			 private GraphDatabaseFacade _graph;

			 public RepresentationWriteHandlerAnonymousInnerClass2( TransactionalRequestDispatcher outerInstance, LoginContext loginContext, GraphDatabaseFacade graph )
			 {
				 this.outerInstance = outerInstance;
				 this._loginContext = loginContext;
				 this._graph = graph;
			 }

			 internal Transaction transaction;

			 public void onRepresentationStartWriting()
			 {
				  transaction = _graph.beginTransaction( KernelTransaction.Type.@implicit, _loginContext );
			 }

			 public void onRepresentationWritten()
			 {
				  // doesn't need to commit
			 }

			 public void onRepresentationFinal()
			 {
				  if ( transaction != null )
				  {
						transaction.close();
				  }
			 }
		 }
	}

}
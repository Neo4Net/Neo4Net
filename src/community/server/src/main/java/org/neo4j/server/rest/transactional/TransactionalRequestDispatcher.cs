﻿using System;

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
	using HttpContext = com.sun.jersey.api.core.HttpContext;
	using RequestDispatcher = com.sun.jersey.spi.dispatch.RequestDispatcher;

	using Transaction = Neo4Net.GraphDb.Transaction;
	using LoginContext = Neo4Net.Kernel.Api.Internal.security.LoginContext;
	using KernelTransaction = Neo4Net.Kernel.Api.KernelTransaction;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using Database = Neo4Net.Server.database.Database;
	using AuthorizedRequestWrapper = Neo4Net.Server.rest.dbms.AuthorizedRequestWrapper;
	using RepresentationWriteHandler = Neo4Net.Server.rest.repr.RepresentationWriteHandler;
	using BatchOperationService = Neo4Net.Server.rest.web.BatchOperationService;
	using CypherService = Neo4Net.Server.rest.web.CypherService;
	using DatabaseMetadataService = Neo4Net.Server.rest.web.DatabaseMetadataService;
	using ExtensionService = Neo4Net.Server.rest.web.ExtensionService;
	using RestfulGraphDatabase = Neo4Net.Server.rest.web.RestfulGraphDatabase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.rest.repr.RepresentationWriteHandler.DO_NOTHING;

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
//ORIGINAL LINE: final Neo4Net.kernel.impl.factory.GraphDatabaseFacade graph = database.getGraph();
			  GraphDatabaseFacade graph = _database.Graph;
			  if ( o is RestfulGraphDatabase )
			  {
					RestfulGraphDatabase restfulGraphDatabase = ( RestfulGraphDatabase ) o;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.graphdb.Transaction transaction = graph.BeginTransaction(Neo4Net.kernel.api.KernelTransaction.Type.implicit, loginContext);
					Transaction transaction = graph.BeginTransaction( KernelTransaction.Type.@implicit, loginContext );

					restfulGraphDatabase.OutputFormat.RepresentationWriteHandler = representationWriteHandler = new CommitOnSuccessfulStatusCodeRepresentationWriteHandler( httpContext, transaction );
			  }
			  else if ( o is BatchOperationService )
			  {
					BatchOperationService batchOperationService = ( BatchOperationService ) o;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.graphdb.Transaction transaction = graph.BeginTransaction(Neo4Net.kernel.api.KernelTransaction.Type.explicit, loginContext);
					Transaction transaction = graph.BeginTransaction( KernelTransaction.Type.@explicit, loginContext );

					batchOperationService.RepresentationWriteHandler = representationWriteHandler = new CommitOnSuccessfulStatusCodeRepresentationWriteHandler( httpContext, transaction );
			  }
			  else if ( o is CypherService )
			  {
					CypherService cypherService = ( CypherService ) o;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.graphdb.Transaction transaction = graph.BeginTransaction(Neo4Net.kernel.api.KernelTransaction.Type.explicit, loginContext);
					Transaction transaction = graph.BeginTransaction( KernelTransaction.Type.@explicit, loginContext );

					cypherService.OutputFormat.RepresentationWriteHandler = representationWriteHandler = new CommitOnSuccessfulStatusCodeRepresentationWriteHandler( httpContext, transaction );
			  }
			  else if ( o is DatabaseMetadataService )
			  {
					DatabaseMetadataService databaseMetadataService = ( DatabaseMetadataService ) o;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.graphdb.Transaction transaction = graph.BeginTransaction(Neo4Net.kernel.api.KernelTransaction.Type.implicit, loginContext);
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
				  transaction = _graph.BeginTransaction( KernelTransaction.Type.@implicit, _loginContext );
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
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
namespace Neo4Net.Cypher.@internal.javacompat
{

	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using URLAccessValidationError = Neo4Net.Graphdb.security.URLAccessValidationError;
	using LoginContext = Neo4Net.@internal.Kernel.Api.security.LoginContext;
	using GraphDatabaseQueryService = Neo4Net.Kernel.GraphDatabaseQueryService;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using DbmsOperations = Neo4Net.Kernel.api.dbms.DbmsOperations;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;

	public class GraphDatabaseCypherService : GraphDatabaseQueryService
	{
		 private readonly GraphDatabaseFacade _graph;
		 private readonly DbmsOperations _dbmsOperations;

		 public GraphDatabaseCypherService( GraphDatabaseService graph )
		 {
			  this._graph = ( GraphDatabaseFacade ) graph;
			  this._dbmsOperations = DependencyResolver.resolveDependency( typeof( DbmsOperations ) );
		 }

		 public virtual DependencyResolver DependencyResolver
		 {
			 get
			 {
				  return _graph.DependencyResolver;
			 }
		 }

		 public override InternalTransaction BeginTransaction( KernelTransaction.Type type, LoginContext loginContext )
		 {
			  return _graph.beginTransaction( type, loginContext );
		 }

		 public override InternalTransaction BeginTransaction( KernelTransaction.Type type, LoginContext loginContext, long timeout, TimeUnit unit )
		 {
			  return _graph.beginTransaction( type, loginContext, timeout, unit );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.net.URL validateURLAccess(java.net.URL url) throws org.neo4j.graphdb.security.URLAccessValidationError
		 public override URL ValidateURLAccess( URL url )
		 {
			  return _graph.validateURLAccess( url );
		 }

		 public virtual DbmsOperations DbmsOperations
		 {
			 get
			 {
				  return _dbmsOperations;
			 }
		 }

		 // This provides backwards compatibility to the older API for places that cannot (yet) stop using it.
		 // TODO: Remove this when possible (remove RULE, remove older compilers)
		 public virtual GraphDatabaseFacade GraphDatabaseService
		 {
			 get
			 {
				  return _graph;
			 }
		 }
	}

}
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
namespace Org.Neo4j.Kernel.Impl.Api.dbms
{
	using Org.Neo4j.Collection;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using ProcedureCallContext = Org.Neo4j.@internal.Kernel.Api.procs.ProcedureCallContext;
	using QualifiedName = Org.Neo4j.@internal.Kernel.Api.procs.QualifiedName;
	using SecurityContext = Org.Neo4j.@internal.Kernel.Api.security.SecurityContext;
	using ResourceTracker = Org.Neo4j.Kernel.api.ResourceTracker;
	using DbmsOperations = Org.Neo4j.Kernel.api.dbms.DbmsOperations;
	using BasicContext = Org.Neo4j.Kernel.api.proc.BasicContext;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.proc.Context_Fields.DATABASE_API;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.proc.Context_Fields.DEPENDENCY_RESOLVER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.proc.Context_Fields.PROCEDURE_CALL_CONTEXT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.proc.Context_Fields.SECURITY_CONTEXT;

	public class NonTransactionalDbmsOperations : DbmsOperations
	{

		 private readonly Procedures _procedures;

		 public NonTransactionalDbmsOperations( Procedures procedures )
		 {
			  this._procedures = procedures;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallDbms(org.neo4j.internal.kernel.api.procs.QualifiedName name, Object[] input, org.neo4j.graphdb.DependencyResolver dependencyResolver, org.neo4j.internal.kernel.api.security.SecurityContext securityContext, org.neo4j.kernel.api.ResourceTracker resourceTracker, org.neo4j.internal.kernel.api.procs.ProcedureCallContext procedureCallContext) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallDbms( QualifiedName name, object[] input, DependencyResolver dependencyResolver, SecurityContext securityContext, ResourceTracker resourceTracker, ProcedureCallContext procedureCallContext )
		 {
			  BasicContext ctx = CreateContext( securityContext, dependencyResolver, procedureCallContext );
			  return _procedures.callProcedure( ctx, name, input, resourceTracker );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> procedureCallDbms(int id, Object[] input, org.neo4j.graphdb.DependencyResolver dependencyResolver, org.neo4j.internal.kernel.api.security.SecurityContext securityContext, org.neo4j.kernel.api.ResourceTracker resourceTracker, org.neo4j.internal.kernel.api.procs.ProcedureCallContext procedureCallContext) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> ProcedureCallDbms( int id, object[] input, DependencyResolver dependencyResolver, SecurityContext securityContext, ResourceTracker resourceTracker, ProcedureCallContext procedureCallContext )
		 {
			  BasicContext ctx = CreateContext( securityContext, dependencyResolver, procedureCallContext );
			  return _procedures.callProcedure( ctx, id, input, resourceTracker );
		 }

		 private static BasicContext CreateContext( SecurityContext securityContext, DependencyResolver dependencyResolver, ProcedureCallContext procedureCallContext )
		 {
			  BasicContext ctx = new BasicContext();
			  ctx.Put( SECURITY_CONTEXT, securityContext );
			  ctx.Put( PROCEDURE_CALL_CONTEXT, procedureCallContext );
			  ctx.Put( DEPENDENCY_RESOLVER, dependencyResolver );
			  ctx.Put( DATABASE_API, dependencyResolver.ResolveDependency( typeof( GraphDatabaseAPI ) ) );
			  return ctx;
		 }
	}

}
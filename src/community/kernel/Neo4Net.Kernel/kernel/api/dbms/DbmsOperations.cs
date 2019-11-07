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

namespace Neo4Net.Kernel.Api.dbms
{
   using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
   using ProcedureCallContext = Neo4Net.Kernel.Api.Internal.procs.ProcedureCallContext;
   using ProcedureException = Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
   using QualifiedName = Neo4Net.Kernel.Api.Internal.procs.QualifiedName;
   using SecurityContext = Neo4Net.Kernel.Api.Internal.security.SecurityContext;

   /// <summary>
   /// Defines all types of system-oriented operations - i.e. those which do not read from or
   /// write to the graph - that can be done.
   /// An example of this is changing a user's password
   /// </summary>
   public interface DbmsOperations
   {
      /// <summary>
      /// Invoke a DBMS procedure by name </summary>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: Neo4Net.collection.RawIterator<Object[],Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallDbms(Neo4Net.Kernel.Api.Internal.procs.QualifiedName name, Object[] input, Neo4Net.graphdb.DependencyResolver dependencyResolver, Neo4Net.Kernel.Api.Internal.security.SecurityContext securityContext, Neo4Net.kernel.api.ResourceTracker resourceTracker, Neo4Net.Kernel.Api.Internal.procs.ProcedureCallContext context) throws Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      RawIterator<object[], ProcedureException> ProcedureCallDbms(QualifiedName name, object[] input, DependencyResolver dependencyResolver, SecurityContext securityContext, ResourceTracker resourceTracker, ProcedureCallContext context);

      /// <summary>
      /// Invoke a DBMS procedure by id </summary>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: Neo4Net.collection.RawIterator<Object[],Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException> procedureCallDbms(int id, Object[] input, Neo4Net.graphdb.DependencyResolver dependencyResolver, Neo4Net.Kernel.Api.Internal.security.SecurityContext securityContext, Neo4Net.kernel.api.ResourceTracker resourceTracker, Neo4Net.Kernel.Api.Internal.procs.ProcedureCallContext context) throws Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
      RawIterator<object[], ProcedureException> ProcedureCallDbms(int id, object[] input, DependencyResolver dependencyResolver, SecurityContext securityContext, ResourceTracker resourceTracker, ProcedureCallContext context);
   }
}
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

namespace Neo4Net.Kernel.api.dbms
{
   using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
   using ProcedureCallContext = Neo4Net.Internal.Kernel.Api.procs.ProcedureCallContext;
   using ProcedureException = Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException;
   using QualifiedName = Neo4Net.Internal.Kernel.Api.procs.QualifiedName;
   using SecurityContext = Neo4Net.Internal.Kernel.Api.security.SecurityContext;

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
      //ORIGINAL LINE: org.Neo4Net.collection.RawIterator<Object[],org.Neo4Net.internal.kernel.api.exceptions.ProcedureException> procedureCallDbms(org.Neo4Net.internal.kernel.api.procs.QualifiedName name, Object[] input, org.Neo4Net.graphdb.DependencyResolver dependencyResolver, org.Neo4Net.internal.kernel.api.security.SecurityContext securityContext, org.Neo4Net.kernel.api.ResourceTracker resourceTracker, org.Neo4Net.internal.kernel.api.procs.ProcedureCallContext context) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException;
      RawIterator<object[], ProcedureException> ProcedureCallDbms(QualifiedName name, object[] input, DependencyResolver dependencyResolver, SecurityContext securityContext, ResourceTracker resourceTracker, ProcedureCallContext context);

      /// <summary>
      /// Invoke a DBMS procedure by id </summary>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.collection.RawIterator<Object[],org.Neo4Net.internal.kernel.api.exceptions.ProcedureException> procedureCallDbms(int id, Object[] input, org.Neo4Net.graphdb.DependencyResolver dependencyResolver, org.Neo4Net.internal.kernel.api.security.SecurityContext securityContext, org.Neo4Net.kernel.api.ResourceTracker resourceTracker, org.Neo4Net.internal.kernel.api.procs.ProcedureCallContext context) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException;
      RawIterator<object[], ProcedureException> ProcedureCallDbms(int id, object[] input, DependencyResolver dependencyResolver, SecurityContext securityContext, ResourceTracker resourceTracker, ProcedureCallContext context);
   }
}
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
namespace Neo4Net.Tooling.procedure.visitors.examples
{
	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using SecurityContext = Neo4Net.Kernel.Api.Internal.security.SecurityContext;
	using UserManager = Neo4Net.Kernel.api.security.UserManager;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Log = Neo4Net.Logging.Log;
	using Context = Neo4Net.Procedure.Context;
	using ProcedureTransaction = Neo4Net.Procedure.ProcedureTransaction;
	using TerminationGuard = Neo4Net.Procedure.TerminationGuard;

	public class RestrictedContextTypes
	{

		 // BELOW ARE TYPES ALLOWED FOR ANY PROCEDURE|FUNCTION

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.Neo4Net.graphdb.GraphDatabaseService IGraphDatabaseService;
		 public IGraphDatabaseService IGraphDatabaseService;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.Neo4Net.logging.Log log;
		 public Log Log;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.Neo4Net.procedure.TerminationGuard terminationGuard;
		 public TerminationGuard TerminationGuard;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.Neo4Net.Kernel.Api.Internal.security.SecurityContext securityContext;
		 public SecurityContext SecurityContext;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.Neo4Net.procedure.ProcedureTransaction procedureTransaction;
		 public ProcedureTransaction ProcedureTransaction;

		 // BELOW ARE RESTRICTED TYPES, THESE ARE UNSUPPORTED AND SUBJECT TO CHANGE

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.Neo4Net.kernel.internal.GraphDatabaseAPI graphDatabaseAPI;
		 public GraphDatabaseAPI GraphDatabaseAPI;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.Neo4Net.kernel.api.KernelTransaction kernelTransaction;
		 public KernelTransaction KernelTransaction;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.Neo4Net.graphdb.DependencyResolver dependencyResolver;
		 public DependencyResolver DependencyResolver;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.Neo4Net.kernel.api.security.UserManager userManager;
		 public UserManager UserManager;
	}

}
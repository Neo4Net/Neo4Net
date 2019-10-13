﻿/*
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
namespace Neo4Net.Tooling.procedure.visitors.examples
{
	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using SecurityContext = Neo4Net.@internal.Kernel.Api.security.SecurityContext;
	using UserManager = Neo4Net.Kernel.api.security.UserManager;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using Log = Neo4Net.Logging.Log;
	using Context = Neo4Net.Procedure.Context;
	using ProcedureTransaction = Neo4Net.Procedure.ProcedureTransaction;
	using TerminationGuard = Neo4Net.Procedure.TerminationGuard;

	public class RestrictedContextTypes
	{

		 // BELOW ARE TYPES ALLOWED FOR ANY PROCEDURE|FUNCTION

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.graphdb.GraphDatabaseService graphDatabaseService;
		 public GraphDatabaseService GraphDatabaseService;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.logging.Log log;
		 public Log Log;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.procedure.TerminationGuard terminationGuard;
		 public TerminationGuard TerminationGuard;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.internal.kernel.api.security.SecurityContext securityContext;
		 public SecurityContext SecurityContext;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.procedure.ProcedureTransaction procedureTransaction;
		 public ProcedureTransaction ProcedureTransaction;

		 // BELOW ARE RESTRICTED TYPES, THESE ARE UNSUPPORTED AND SUBJECT TO CHANGE

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.kernel.internal.GraphDatabaseAPI graphDatabaseAPI;
		 public GraphDatabaseAPI GraphDatabaseAPI;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.kernel.api.KernelTransaction kernelTransaction;
		 public KernelTransaction KernelTransaction;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.graphdb.DependencyResolver dependencyResolver;
		 public DependencyResolver DependencyResolver;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.kernel.api.security.UserManager userManager;
		 public UserManager UserManager;
	}

}
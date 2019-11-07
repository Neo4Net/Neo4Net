﻿/*
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
namespace Neo4Net.Kernel.builtinprocs
{

	using ProcedureSignature = Neo4Net.Kernel.Api.Internal.procs.ProcedureSignature;
	using UserFunctionSignature = Neo4Net.Kernel.Api.Internal.procs.UserFunctionSignature;
	using SecurityContext = Neo4Net.Kernel.Api.Internal.security.SecurityContext;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using QueryExecutionEngine = Neo4Net.Kernel.impl.query.QueryExecutionEngine;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Log = Neo4Net.Logging.Log;
	using Admin = Neo4Net.Procedure.Admin;
	using Context = Neo4Net.Procedure.Context;
	using Description = Neo4Net.Procedure.Description;
	using Name = Neo4Net.Procedure.Name;
	using Procedure = Neo4Net.Procedure.Procedure;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.procedure.Mode.DBMS;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public class BuiltInDbmsProcedures
	public class BuiltInDbmsProcedures
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public Neo4Net.logging.Log log;
		 public Log Log;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public Neo4Net.kernel.internal.GraphDatabaseAPI graph;
		 public GraphDatabaseAPI Graph;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public Neo4Net.Kernel.Api.Internal.security.SecurityContext securityContext;
		 public SecurityContext SecurityContext;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("List the currently active config of Neo4Net.") @Procedure(name = "dbms.listConfig", mode = DBMS) public java.util.stream.Stream<ConfigResult> listConfig(@Name(value = "searchString", defaultValue = "") String searchString)
		 [Description("List the currently active config of Neo4Net."), Procedure(name : "dbms.listConfig", mode : DBMS)]
		 public virtual Stream<ConfigResult> ListConfig( string searchString )
		 {
			  Config config = Graph.DependencyResolver.resolveDependency( typeof( Config ) );
			  string lowerCasedSearchString = searchString.ToLower();
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return config.ConfigValues.Values.Where( c => !c.Internal() ).Where(c => c.name().ToLower().Contains(lowerCasedSearchString)).Select(ConfigResult::new).OrderBy(System.Collections.IComparer.comparing(c => c.name));
		 }

		 [Description("List all procedures in the DBMS."), Procedure(name : "dbms.procedures", mode : DBMS)]
		 public virtual Stream<ProcedureResult> ListProcedures()
		 {
			  SecurityContext.assertCredentialsNotExpired();
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Graph.DependencyResolver.resolveDependency( typeof( Procedures ) ).AllProcedures.OrderBy( System.Collections.IComparer.comparing( a => a.name().ToString() ) ).Select(ProcedureResult::new);
		 }

		 [Description("List all user functions in the DBMS."), Procedure(name : "dbms.functions", mode : DBMS)]
		 public virtual Stream<FunctionResult> ListFunctions()
		 {
			  SecurityContext.assertCredentialsNotExpired();
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Graph.DependencyResolver.resolveDependency( typeof( Procedures ) ).AllFunctions.OrderBy( System.Collections.IComparer.comparing( a => a.name().ToString() ) ).Select(FunctionResult::new);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Clears all query caches.") @Procedure(name = "dbms.clearQueryCaches", mode = DBMS) public java.util.stream.Stream<StringResult> clearAllQueryCaches()
		 [Description("Clears all query caches."), Procedure(name : "dbms.clearQueryCaches", mode : DBMS)]
		 public virtual Stream<StringResult> ClearAllQueryCaches()
		 {
			  QueryExecutionEngine queryExecutionEngine = Graph.DependencyResolver.resolveDependency( typeof( QueryExecutionEngine ) );
			  long numberOfClearedQueries = queryExecutionEngine.ClearQueryCaches() - 1; // this query itself does not count

			  string result = numberOfClearedQueries == 0 ? "Query cache already empty." : "Query caches successfully cleared of " + numberOfClearedQueries + " queries.";
			  Log.info( "Called dbms.clearQueryCaches(): " + result );
			  return Stream.of( new StringResult( result ) );
		 }

		 public class FunctionResult
		 {
			  public readonly string Name;
			  public readonly string Signature;
			  public readonly string Description;

			  internal FunctionResult( UserFunctionSignature signature )
			  {
					this.Name = signature.Name().ToString();
					this.Signature = signature.ToString();
					this.Description = signature.Description().orElse("");
			  }
		 }

		 public class ProcedureResult
		 {
			  public readonly string Name;
			  public readonly string Signature;
			  public readonly string Description;
			  public readonly string Mode;

			  internal ProcedureResult( ProcedureSignature signature )
			  {
					this.Name = signature.Name().ToString();
					this.Signature = signature.ToString();
					this.Description = signature.Description().orElse("");
					this.Mode = signature.Mode().ToString();
			  }
		 }

		 public class StringResult
		 {
			  public readonly string Value;

			  internal StringResult( string value )
			  {
					this.Value = value;
			  }
		 }
	}

}
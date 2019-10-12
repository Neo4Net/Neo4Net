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
namespace Neo4Net.Tooling.procedure.procedures.invalid.bad_context_field
{
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Context = Neo4Net.Procedure.Context;
	using Name = Neo4Net.Procedure.Name;
	using Procedure = Neo4Net.Procedure.Procedure;
	using UserAggregationFunction = Neo4Net.Procedure.UserAggregationFunction;
	using UserAggregationResult = Neo4Net.Procedure.UserAggregationResult;
	using UserAggregationUpdate = Neo4Net.Procedure.UserAggregationUpdate;
	using UserFunction = Neo4Net.Procedure.UserFunction;

	public class BadContextFields
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public static org.neo4j.graphdb.GraphDatabaseService shouldBeNonStatic;
		 public static GraphDatabaseService ShouldBeNonStatic;
		 public static string Value;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public final org.neo4j.graphdb.GraphDatabaseService shouldBeNonFinal = null;
		 public readonly GraphDatabaseService ShouldBeNonFinal = null;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.graphdb.GraphDatabaseService db;
		 public GraphDatabaseService Db;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context protected org.neo4j.graphdb.GraphDatabaseService shouldBePublic;
		 protected internal GraphDatabaseService ShouldBePublic;
		 internal string ShouldBeStatic;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void sproc1()
		 public virtual void Sproc1()
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void sproc2()
		 public virtual void Sproc2()
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public System.Nullable<long> function()
		 public virtual long? Function()
		 {
			  return 42L;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public MyAggregation aggregation()
		 public virtual MyAggregation Aggregation()
		 {
			  return new MyAggregation();
		 }

		 public class MyAggregation
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public System.Nullable<long> result()
			  public virtual long? Result()
			  {
					return 42L;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void woot(@Name("undostres") String onetwothree)
			  public virtual void Woot( string onetwothree )
			  {

			  }
		 }
	}

}
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
namespace Org.Neo4j.Tooling.procedure.procedures.invalid.bad_return_type
{

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Context = Org.Neo4j.Procedure.Context;
	using Name = Org.Neo4j.Procedure.Name;
	using UserFunction = Org.Neo4j.Procedure.UserFunction;

	public class BadReturnTypeUserFunction
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.graphdb.GraphDatabaseService db;
		 public GraphDatabaseService Db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public java.util.stream.Stream<long> wrongReturnTypeFunction(@Name("foo") String parameter)
		 public virtual Stream<long> WrongReturnTypeFunction( string parameter )
		 {
			  return Stream.empty();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public System.Nullable<long> niceFunction(@Name("foo") String parameter)
		 public virtual long? NiceFunction( string parameter )
		 {
			  return 3L;
		 }
	}

}
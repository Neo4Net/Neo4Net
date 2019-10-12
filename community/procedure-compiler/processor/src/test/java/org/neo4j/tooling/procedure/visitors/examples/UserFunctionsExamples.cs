using System.Threading;

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
namespace Org.Neo4j.Tooling.procedure.visitors.examples
{
	using Name = Org.Neo4j.Procedure.Name;
	using UserFunction = Org.Neo4j.Procedure.UserFunction;

	public class UserFunctionsExamples
	{
		 [UserFunction(name : "in_root_namespace")]
		 public virtual string FunctionWithName()
		 {
			  return "42";
		 }

		 [UserFunction(value : "in_root_namespace_again")]
		 public virtual string FunctionWithValue()
		 {
			  return "42";
		 }

		 [UserFunction(name : "not.in.root.namespace")]
		 public virtual string Ok()
		 {
			  return "42";
		 }

		 [UserFunction(name : "com.acme.foobar")]
		 public virtual void WrongReturnType()
		 {

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction(name = "com.acme.foobar") public String wrongParameterType(@Name("foo") Thread foo)
		 [UserFunction(name : "com.acme.foobar")]
		 public virtual string WrongParameterType( Thread foo )
		 {
			  return "42";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction(name = "com.acme.foobar") public String missingParameterAnnotation(@Name("foo") String foo, String oops)
		 [UserFunction(name : "com.acme.foobar")]
		 public virtual string MissingParameterAnnotation( string foo, string oops )
		 {
			  return "42";
		 }
	}

}
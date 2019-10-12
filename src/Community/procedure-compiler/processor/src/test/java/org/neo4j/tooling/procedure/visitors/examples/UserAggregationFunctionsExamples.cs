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
namespace Neo4Net.Tooling.procedure.visitors.examples
{
	using Name = Neo4Net.Procedure.Name;
	using UserAggregationFunction = Neo4Net.Procedure.UserAggregationFunction;
	using UserAggregationResult = Neo4Net.Procedure.UserAggregationResult;
	using UserAggregationUpdate = Neo4Net.Procedure.UserAggregationUpdate;

	public class UserAggregationFunctionsExamples
	{
		 [UserAggregationFunction(name : "in_root_namespace")]
		 public virtual StringAggregator FunctionWithName()
		 {
			  return new StringAggregator();
		 }

		 [UserAggregationFunction(value : "in_root_namespace_again")]
		 public virtual StringAggregator FunctionWithValue()
		 {
			  return new StringAggregator();
		 }

		 [UserAggregationFunction(name : "not.in.root.namespace")]
		 public virtual StringAggregator Ok()
		 {
			  return new StringAggregator();
		 }

		 [UserAggregationFunction(name : "com.acme.foobar")]
		 public virtual void WrongReturnType()
		 {

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction(name = "com.acme.foobar") public StringAggregator shouldNotHaveParameters(@Name("hello") String hello)
		 [UserAggregationFunction(name : "com.acme.foobar")]
		 public virtual StringAggregator ShouldNotHaveParameters( string hello )
		 {
			  return new StringAggregator();
		 }

		 [UserAggregationFunction(name : "com.acme.foobar")]
		 public virtual StringAggregatorWithWrongUpdateParameterType UpdateWithWrongParameterType()
		 {
			  return new StringAggregatorWithWrongUpdateParameterType();
		 }

		 [UserAggregationFunction(name : "com.acme.foobar")]
		 public virtual StringAggregatorWithMissingAnnotationOnParameterType MissingParameterAnnotation()
		 {
			  return new StringAggregatorWithMissingAnnotationOnParameterType();
		 }

		 [UserAggregationFunction(name : "com.acme.foobar")]
		 public virtual StringAggregatorWithWrongResultReturnType ResultWithWrongReturnType()
		 {
			  return new StringAggregatorWithWrongResultReturnType();
		 }

		 [UserAggregationFunction(name : "com.acme.foobar")]
		 public virtual StringAggregatorWithResultMethodWithParameters ResultWithParams()
		 {
			  return new StringAggregatorWithResultMethodWithParameters();
		 }

		 public class StringAggregator
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void doSomething(@Name("foo") String foo)
			  public virtual void DoSomething( string foo )
			  {

			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public long result()
			  public virtual long Result()
			  {
					return 42L;
			  }
		 }

		 public class StringAggregatorWithWrongUpdateParameterType
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void doSomething(@Name("foo") Thread foo)
			  public virtual void DoSomething( Thread foo )
			  {

			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public long result()
			  public virtual long Result()
			  {
					return 42L;
			  }
		 }

		 public class StringAggregatorWithMissingAnnotationOnParameterType
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void doSomething(long foo)
			  public virtual void DoSomething( long foo )
			  {

			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public long result()
			  public virtual long Result()
			  {
					return 42L;
			  }
		 }

		 public class StringAggregatorWithWrongResultReturnType
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void doSomething(@Name("foo") long foo)
			  public virtual void DoSomething( long foo )
			  {

			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public Thread result()
			  public virtual Thread Result()
			  {
					return new Thread();
			  }
		 }

		 public class StringAggregatorWithResultMethodWithParameters
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void doSomething(@Name("foo") long foo)
			  public virtual void DoSomething( long foo )
			  {

			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public long result(String shouldNotHaveAnyParam)
			  public virtual long Result( string shouldNotHaveAnyParam )
			  {
					return 42L;
			  }
		 }
	}

}
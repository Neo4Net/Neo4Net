using System;

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
namespace Neo4Net.Harness
{
	using JsonNode = org.codehaus.jackson.JsonNode;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using UserAggregationFunction = Neo4Net.Procedure.UserAggregationFunction;
	using UserAggregationResult = Neo4Net.Procedure.UserAggregationResult;
	using UserAggregationUpdate = Neo4Net.Procedure.UserAggregationUpdate;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.RawPayload.quotedJson;

	public class JavaAggregationFunctionsTestIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

		 public class MyFunctions
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public EliteAggregator myFunc()
			  public virtual EliteAggregator MyFunc()
			  {
					return new EliteAggregator();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public EliteAggregator funcThatThrows()
			  public virtual EliteAggregator FuncThatThrows()
			  {
					throw new Exception( "This is an exception" );
			  }

		 }

		 public class EliteAggregator
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update()
			  public virtual void Update()
			  {
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public long result()
			  public virtual long Result()
			  {
					return 1337L;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLaunchWithDeclaredFunctions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLaunchWithDeclaredFunctions()
		 {
			  // When
			  using ( ServerControls server = CreateServer( typeof( MyFunctions ) ).newServer() )
			  {
					// Then
					HTTP.Response response = HTTP.POST( server.HttpURI().resolve("db/data/transaction/commit").ToString(), quotedJson("{ 'statements': [ { 'statement': 'RETURN org.neo4j.harness.myFunc() AS someNumber' } ] " + "}") );

					JsonNode result = response.Get( "results" ).get( 0 );
					assertEquals( "someNumber", result.get( "columns" ).get( 0 ).asText() );
					assertEquals( 1337, result.get( "data" ).get( 0 ).get( "row" ).get( 0 ).asInt() );
					assertEquals( "[]", response.Get( "errors" ).ToString() );
			  }
		 }

		 private TestServerBuilder CreateServer( Type functionClass )
		 {
			  return TestServerBuilders.NewInProcessBuilder().withAggregationFunction(functionClass);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetHelpfulErrorOnProcedureThrowsException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetHelpfulErrorOnProcedureThrowsException()
		 {
			  // When
			  using ( ServerControls server = CreateServer( typeof( MyFunctions ) ).newServer() )
			  {
					// Then
					HTTP.Response response = HTTP.POST( server.HttpURI().resolve("db/data/transaction/commit").ToString(), quotedJson("{ 'statements': [ { 'statement': 'RETURN org.neo4j.harness.funcThatThrows()' } ] }") );

					string error = response.Get( "errors" ).get( 0 ).get( "message" ).asText();
					assertEquals( "Failed to invoke function `org.neo4j.harness.funcThatThrows`: Caused by: java.lang" + ".RuntimeException: This is an exception", error );
			  }
		 }
	}

}
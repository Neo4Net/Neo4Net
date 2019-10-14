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

	using ProcedureException = Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException;
	using Context = Neo4Net.Procedure.Context;
	using UserFunction = Neo4Net.Procedure.UserFunction;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.RawPayload.quotedJson;

	public class JavaFunctionsTestIT
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
//ORIGINAL LINE: @UserFunction public long myFunc()
			  public virtual long MyFunc()
			  {
					return 1337L;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public long funcThatThrows()
			  public virtual long FuncThatThrows()
			  {
					throw new Exception( "This is an exception" );
			  }
		 }

		 public class MyFunctionsUsingMyService
		 {

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public SomeService service;
			  public SomeService Service;

			  [UserFunction("my.hello")]
			  public virtual string Hello()
			  {
					return Service.hello();
			  }
		 }

		 public class MyFunctionsUsingMyCoreAPI
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public MyCoreAPI myCoreAPI;
			  public MyCoreAPI MyCoreAPI;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long willFail() throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
			  [UserFunction(value : "my.willFail")]
			  public virtual long WillFail()
			  {
					return MyCoreAPI.makeNode( "Test" );
			  }

			  [UserFunction("my.countNodes")]
			  public virtual long CountNodes()
			  {
					return MyCoreAPI.countNodes();
			  }
		 }

		 private TestServerBuilder CreateServer( Type functionClass )
		 {
			  return TestServerBuilders.NewInProcessBuilder().withFunction(functionClass);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLaunchWithDeclaredFunctions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLaunchWithDeclaredFunctions()
		 {
			  // When
			  Type<MyFunctions> functionClass = typeof( MyFunctions );
			  using ( ServerControls server = CreateServer( functionClass ).newServer() )
			  {
					// Then
					HTTP.Response response = HTTP.POST( server.HttpURI().resolve("db/data/transaction/commit").ToString(), quotedJson("{ 'statements': [ { 'statement': 'RETURN org.neo4j.harness.myFunc() AS someNumber' } ] " + "}") );

					JsonNode result = response.Get( "results" ).get( 0 );
					assertEquals( "someNumber", result.get( "columns" ).get( 0 ).asText() );
					assertEquals( 1337, result.get( "data" ).get( 0 ).get( "row" ).get( 0 ).asInt() );
					assertEquals( "[]", response.Get( "errors" ).ToString() );
			  }
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

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkWithInjectableFromKernelExtension() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWorkWithInjectableFromKernelExtension()
		 {
			  // When
			  using ( ServerControls server = CreateServer( typeof( MyFunctionsUsingMyService ) ).newServer() )
			  {
					// Then
					HTTP.Response response = HTTP.POST( server.HttpURI().resolve("db/data/transaction/commit").ToString(), quotedJson("{ 'statements': [ { 'statement': 'RETURN my.hello() AS result' } ] }") );

					assertEquals( "[]", response.Get( "errors" ).ToString() );
					JsonNode result = response.Get( "results" ).get( 0 );
					assertEquals( "result", result.get( "columns" ).get( 0 ).asText() );
					assertEquals( "world", result.get( "data" ).get( 0 ).get( "row" ).get( 0 ).asText() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkWithInjectableFromKernelExtensionWithMorePower() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWorkWithInjectableFromKernelExtensionWithMorePower()
		 {
			  // When
			  using ( ServerControls server = CreateServer( typeof( MyFunctionsUsingMyCoreAPI ) ).newServer() )
			  {
					HTTP.POST( server.HttpURI().resolve("db/data/transaction/commit").ToString(), quotedJson("{ 'statements': [ { 'statement': 'CREATE (), (), ()' } ] }") );

					// Then
					AssertQueryGetsValue( server, "RETURN my.countNodes() AS value", 3L );
					AssertQueryGetsError( server, "RETURN my.willFail() AS value", "Write operations are not allowed" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertQueryGetsValue(ServerControls server, String query, long value) throws Throwable
		 private void AssertQueryGetsValue( ServerControls server, string query, long value )
		 {
			  HTTP.Response response = HTTP.POST( server.HttpURI().resolve("db/data/transaction/commit").ToString(), quotedJson("{ 'statements': [ { 'statement': '" + query + "' } ] }") );

			  assertEquals( "[]", response.Get( "errors" ).ToString() );
			  JsonNode result = response.Get( "results" ).get( 0 );
			  assertEquals( "value", result.get( "columns" ).get( 0 ).asText() );
			  assertEquals( value, result.get( "data" ).get( 0 ).get( "row" ).get( 0 ).asLong() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertQueryGetsError(ServerControls server, String query, String error) throws Throwable
		 private void AssertQueryGetsError( ServerControls server, string query, string error )
		 {
			  HTTP.Response response = HTTP.POST( server.HttpURI().resolve("db/data/transaction/commit").ToString(), quotedJson("{ 'statements': [ { 'statement': '" + query + "' } ] }") );

			  assertThat( response.Get( "errors" ).ToString(), containsString(error) );
		 }
	}

}
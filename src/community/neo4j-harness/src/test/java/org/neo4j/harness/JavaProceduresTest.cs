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

	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using ProcedureException = Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException;
	using Context = Neo4Net.Procedure.Context;
	using Mode = Neo4Net.Procedure.Mode;
	using Name = Neo4Net.Procedure.Name;
	using Procedure = Neo4Net.Procedure.Procedure;
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

	public class JavaProceduresTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

		 public class MyProcedures
		 {
			  public class OutputRecord
			  {
					public long SomeNumber = 1337;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<OutputRecord> myProc()
			  public virtual Stream<OutputRecord> MyProc()
			  {
					return Stream.of( new OutputRecord() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<OutputRecord> procThatThrows()
			  public virtual Stream<OutputRecord> ProcThatThrows()
			  {
					throw new Exception( "This is an exception" );
			  }
		 }

		 public class MyProceduresUsingMyService
		 {
			  public class OutputRecord
			  {
					public string Result;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public SomeService service;
			  public SomeService Service;

			  [Procedure("hello")]
			  public virtual Stream<OutputRecord> Hello()
			  {
					OutputRecord t = new OutputRecord();
					t.Result = Service.hello();
					return Stream.of( t );
			  }
		 }

		 public class MyProceduresUsingMyCoreAPI
		 {
			  public class LongResult
			  {
					public long? Value;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public MyCoreAPI myCoreAPI;
			  public MyCoreAPI MyCoreAPI;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure(value = "makeNode", mode = org.neo4j.procedure.Mode.WRITE) public java.util.stream.Stream<LongResult> makeNode(@Name("label") String label) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  [Procedure(value : "makeNode", mode : Neo4Net.Procedure.Mode.WRITE)]
			  public virtual Stream<LongResult> MakeNode( string label )
			  {
					LongResult t = new LongResult();
					t.Value = MyCoreAPI.makeNode( label );
					return Stream.of( t );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.stream.Stream<LongResult> willFail() throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
			  [Procedure(value : "willFail", mode : Neo4Net.Procedure.Mode.READ)]
			  public virtual Stream<LongResult> WillFail()
			  {
					LongResult t = new LongResult();
					t.Value = MyCoreAPI.makeNode( "Test" );
					return Stream.of( t );
			  }

			  [Procedure("countNodes")]
			  public virtual Stream<LongResult> CountNodes()
			  {
					LongResult t = new LongResult();
					t.Value = MyCoreAPI.countNodes();
					return Stream.of( t );
			  }
		 }

		 private TestServerBuilder CreateServer( Type procedureClass )
		 {
			  return TestServerBuilders.NewInProcessBuilder().withProcedure(procedureClass);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLaunchWithDeclaredProcedures() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLaunchWithDeclaredProcedures()
		 {
			  // When
			  using ( ServerControls server = CreateServer( typeof( MyProcedures ) ).newServer() )
			  {
					// Then
					HTTP.Response response = HTTP.POST( server.HttpURI().resolve("db/data/transaction/commit").ToString(), quotedJson("{ 'statements': [ { 'statement': 'CALL org.neo4j.harness.myProc' } ] }") );

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
			  using ( ServerControls server = CreateServer( typeof( MyProcedures ) ).newServer() )
			  {
					// Then
					HTTP.Response response = HTTP.POST( server.HttpURI().resolve("db/data/transaction/commit").ToString(), quotedJson("{ 'statements': [ { 'statement': 'CALL org.neo4j.harness.procThatThrows' } ] }") );

					string error = response.Get( "errors" ).get( 0 ).get( "message" ).asText();
					assertEquals( "Failed to invoke procedure `org.neo4j.harness.procThatThrows`: " + "Caused by: java.lang.RuntimeException: This is an exception", error );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkWithInjectableFromKernelExtension() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWorkWithInjectableFromKernelExtension()
		 {
			  // When
			  using ( ServerControls server = CreateServer( typeof( MyProceduresUsingMyService ) ).newServer() )
			  {
					// Then
					HTTP.Response response = HTTP.POST( server.HttpURI().resolve("db/data/transaction/commit").ToString(), quotedJson("{ 'statements': [ { 'statement': 'CALL hello' } ] }") );

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
			  using ( ServerControls server = CreateServer( typeof( MyProceduresUsingMyCoreAPI ) ).withConfig( GraphDatabaseSettings.record_id_batch_size, "1" ).newServer() )
			  {
					// Then
					AssertQueryGetsValue( server, "CALL makeNode(\\'Test\\')", 0L );
					AssertQueryGetsValue( server, "CALL makeNode(\\'Test\\')", 1L );
					AssertQueryGetsValue( server, "CALL makeNode(\\'Test\\')", 2L );
					AssertQueryGetsValue( server, "CALL countNodes", 3L );
					AssertQueryGetsError( server, "CALL willFail", "Write operations are not allowed" );
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
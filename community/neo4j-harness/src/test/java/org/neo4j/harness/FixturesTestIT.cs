using System;

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
namespace Org.Neo4j.Harness
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Label = Org.Neo4j.Graphdb.Label;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using FileUtils = Org.Neo4j.Io.fs.FileUtils;
	using LegacySslPolicyConfig = Org.Neo4j.Kernel.configuration.ssl.LegacySslPolicyConfig;
	using ServerTestUtils = Org.Neo4j.Server.ServerTestUtils;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using HTTP = Org.Neo4j.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.lineSeparator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.harness.TestServerBuilders.newInProcessBuilder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.RawPayload.quotedJson;

	public class FixturesTestIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAccepSingleCypherFileAsFixture() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAccepSingleCypherFileAsFixture()
		 {
			  // Given
			  File targetFolder = TestDir.directory();
			  File fixture = new File( targetFolder, "fixture.cyp" );
			  FileUtils.writeToFile( fixture, "CREATE (u:User)" + "CREATE (a:OtherUser)", false );

			  // When
			  using ( ServerControls server = GetServerBuilder( targetFolder ).withFixture( fixture ).newServer() )
			  {
					// Then
					HTTP.Response response = HTTP.POST( server.HttpURI().ToString() + "db/data/transaction/commit", quotedJson("{'statements':[{'statement':'MATCH (n:User) RETURN n'}]}") );

					assertThat( response.Status(), equalTo(200) );
					assertThat( response.Get( "results" ).get( 0 ).get( "data" ).size(), equalTo(1) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptFolderWithCypFilesAsFixtures() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcceptFolderWithCypFilesAsFixtures()
		 {
			  // Given two files in the root folder
			  File targetFolder = TestDir.directory();
			  FileUtils.writeToFile( new File( targetFolder, "fixture1.cyp" ), "CREATE (u:User)\n" + "CREATE (a:OtherUser)", false );
			  FileUtils.writeToFile( new File( targetFolder, "fixture2.cyp" ), "CREATE (u:User)\n" + "CREATE (a:OtherUser)", false );

			  // And given one file in a sub directory
			  File subDir = new File( targetFolder, "subdirectory" );
			  subDir.mkdir();
			  FileUtils.writeToFile( new File( subDir, "subDirFixture.cyp" ), "CREATE (u:User)\n" + "CREATE (a:OtherUser)", false );

			  // When
			  using ( ServerControls server = GetServerBuilder( targetFolder ).withFixture( targetFolder ).newServer() )
			  {
					// Then
					HTTP.Response response = HTTP.POST( server.HttpURI().ToString() + "db/data/transaction/commit", quotedJson("{'statements':[{'statement':'MATCH (n:User) RETURN n'}]}") );

					assertThat( response.ToString(), response.Get("results").get(0).get("data").size(), equalTo(3) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMultipleFixtures() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleMultipleFixtures()
		 {
			  // Given two files in the root folder
			  File targetFolder = TestDir.directory();
			  File fixture1 = new File( targetFolder, "fixture1.cyp" );
			  FileUtils.writeToFile( fixture1, "CREATE (u:User)\n" + "CREATE (a:OtherUser)", false );
			  File fixture2 = new File( targetFolder, "fixture2.cyp" );
			  FileUtils.writeToFile( fixture2, "CREATE (u:User)\n" + "CREATE (a:OtherUser)", false );

			  // When
			  using ( ServerControls server = GetServerBuilder( targetFolder ).withFixture( fixture1 ).withFixture( fixture2 ).newServer() )
			  {
					// Then
					HTTP.Response response = HTTP.POST( server.HttpURI().ToString() + "db/data/transaction/commit", quotedJson("{'statements':[{'statement':'MATCH (n:User) RETURN n'}]}") );

					assertThat( response.Get( "results" ).get( 0 ).get( "data" ).size(), equalTo(2) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleStringFixtures() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleStringFixtures()
		 {
			  // Given two files in the root folder
			  File targetFolder = TestDir.directory();

			  // When
			  using ( ServerControls server = GetServerBuilder( targetFolder ).withFixture( "CREATE (a:User)" ).newServer() )
			  {
					// Then
					HTTP.Response response = HTTP.POST( server.HttpURI().ToString() + "db/data/transaction/commit", quotedJson("{'statements':[{'statement':'MATCH (n:User) RETURN n'}]}") );

					assertThat( response.Get( "results" ).get( 0 ).get( "data" ).size(), equalTo(1) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreEmptyFixtureFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreEmptyFixtureFiles()
		 {
			  // Given two files in the root folder
			  File targetFolder = TestDir.directory();
			  FileUtils.writeToFile( new File( targetFolder, "fixture1.cyp" ), "CREATE (u:User)\n" + "CREATE (a:OtherUser)", false );
			  FileUtils.writeToFile( new File( targetFolder, "fixture2.cyp" ), "", false );

			  // When
			  using ( ServerControls server = GetServerBuilder( targetFolder ).withFixture( targetFolder ).newServer() )
			  {
					// Then
					HTTP.Response response = HTTP.POST( server.HttpURI().ToString() + "db/data/transaction/commit", quotedJson("{'statements':[{'statement':'MATCH (n:User) RETURN n'}]}") );

					assertThat( response.Get( "results" ).get( 0 ).get( "data" ).size(), equalTo(1) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleFixturesWithSyntaxErrorsGracefully() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleFixturesWithSyntaxErrorsGracefully()
		 {
			  // Given two files in the root folder
			  File targetFolder = TestDir.directory();
			  FileUtils.writeToFile( new File( targetFolder, "fixture1.cyp" ), "this is not a valid cypher statement", false );

			  // When
			  try
			  {
					  using ( ServerControls ignore = GetServerBuilder( targetFolder ).withFixture( targetFolder ).newServer() )
					  {
						fail( "Should have thrown exception" );
					  }
			  }
			  catch ( Exception e )
			  {
					assertThat( e.Message, equalTo( "Invalid input 't': expected <init> (line 1, column 1 (offset: 0))" + lineSeparator() + "\"this is not a valid cypher statement\"" + lineSeparator() + " ^" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleFunctionFixtures() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleFunctionFixtures()
		 {
			  // Given two files in the root folder
			  File targetFolder = TestDir.directory();

			  // When
			  try (ServerControls server = GetServerBuilder(targetFolder).withFixture(graphDatabaseService =>
			  {
						  using ( Transaction tx = graphDatabaseService.beginTx() )
						  {
								graphDatabaseService.createNode( Label.label( "User" ) );
								tx.success();
						  }
						  return null;
			  }
						).newServer())
						{
					// Then
					HTTP.Response response = HTTP.POST( server.httpURI().ToString() + "db/data/transaction/commit", quotedJson("{'statements':[{'statement':'MATCH (n:User) RETURN n'}]}") );

					assertThat( response.Get( "results" ).get( 0 ).get( "data" ).size(), equalTo(1) );
						}
		 }

		 private TestServerBuilder GetServerBuilder( File targetFolder )
		 {
			  TestServerBuilder serverBuilder = newInProcessBuilder( targetFolder ).withConfig( LegacySslPolicyConfig.certificates_directory.name(), ServerTestUtils.getRelativePath(TestDir.directory(), LegacySslPolicyConfig.certificates_directory) );
			  return serverBuilder;
		 }

	}

}
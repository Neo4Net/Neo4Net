﻿using System.Collections.Generic;

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
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using Statement = org.junit.runners.model.Statement;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Result = Neo4Net.GraphDb.Result;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using MyUnmanagedExtension = Neo4Net.Harness.extensionpackage.MyUnmanagedExtension;
	using Neo4NetRule = Neo4Net.Harness.junit.Neo4NetRule;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LegacySslPolicyConfig = Neo4Net.Kernel.configuration.ssl.LegacySslPolicyConfig;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using LogTimeZone = Neo4Net.Logging.LogTimeZone;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.ServerTestUtils.getRelativePath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.ServerTestUtils.getSharedTestTemporaryFolder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.server.HTTP.RawPayload.quotedJson;

	public class JUnitRuleTestIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.TestDirectory testDirectory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.SuppressOutput suppressOutput = org.Neo4Net.test.rule.SuppressOutput.suppressAll();
		 public SuppressOutput SuppressOutput = SuppressOutput.suppressAll();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.harness.junit.Neo4NetRule Neo4Net = new org.Neo4Net.harness.junit.Neo4NetRule().withFixture("CREATE (u:User)").withConfig(org.Neo4Net.graphdb.factory.GraphDatabaseSettings.db_timezone.name(), org.Neo4Net.logging.LogTimeZone.SYSTEM.toString()).withConfig(org.Neo4Net.kernel.configuration.ssl.LegacySslPolicyConfig.certificates_directory.name(), getRelativePath(getSharedTestTemporaryFolder(), org.Neo4Net.kernel.configuration.ssl.LegacySslPolicyConfig.certificates_directory)).withFixture(graphDatabaseService ->
		 public Neo4NetRule Neo4Net = new Neo4NetRule().withFixture("CREATE (u:User)").withConfig(GraphDatabaseSettings.db_timezone.name(), LogTimeZone.SYSTEM.ToString()).withConfig(LegacySslPolicyConfig.certificates_directory.name(), getRelativePath(SharedTestTemporaryFolder, LegacySslPolicyConfig.certificates_directory)).withFixture(graphDatabaseService =>
		 {
					 using ( Transaction tx = IGraphDatabaseService.beginTx() )
					 {
						  IGraphDatabaseService.createNode( Label.label( "User" ) );
						  tx.success();
					 }
					 return null;
		 }).withExtension( "/test", typeof( MyUnmanagedExtension ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExtensionWork()
		 public virtual void ShouldExtensionWork()
		 {
			  // Given the rule in the beginning of this class

			  // When I run this test

			  // Then
			  assertThat( HTTP.GET( Neo4Net.httpURI().resolve("test/myExtension").ToString() ).status(), equalTo(234) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFixturesWork() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFixturesWork()
		 {
			  // Given the rule in the beginning of this class

			  // When I run this test

			  // Then
			  HTTP.Response response = HTTP.POST( Neo4Net.httpURI().ToString() + "db/data/transaction/commit", quotedJson("{'statements':[{'statement':'MATCH (n:User) RETURN n'}]}") );

			  assertThat( response.Get( "results" ).get( 0 ).get( "data" ).size(), equalTo(2) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGraphDatabaseServiceBeAccessible()
		 public virtual void ShouldGraphDatabaseServiceBeAccessible()
		 {
			  // Given the rule in the beginning of this class

			  // When I run this test

			  // Then
			  assertEquals( 2, Iterators.count( Neo4Net.GraphDatabaseService.execute( "MATCH (n:User) RETURN n" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRuleWorkWithExistingDirectory() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRuleWorkWithExistingDirectory()
		 {
			  // given a root folder, create /databases/graph.db folders.
			  File oldDir = TestDirectory.directory( "old" );
			  File storeDir = Config.defaults( GraphDatabaseSettings.data_directory, oldDir.toPath().ToString() ).get(GraphDatabaseSettings.database_path);
			  IGraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(storeDir);

			  try
			  {
					Db.execute( "CREATE ()" );
			  }
			  finally
			  {
					Db.shutdown();
			  }

			  // When a rule with an pre-populated graph db directory is used
			  File newDir = TestDirectory.directory( "new" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.harness.junit.Neo4NetRule ruleWithDirectory = new org.Neo4Net.harness.junit.Neo4NetRule(newDir).copyFrom(oldDir);
			  Neo4NetRule ruleWithDirectory = ( new Neo4NetRule( newDir ) ).copyFrom( oldDir );
			  Statement statement = ruleWithDirectory.apply(new StatementAnonymousInnerClass(this, ruleWithDirectory)
			 , null);

			  // Then
			  statement.evaluate();
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private readonly JUnitRuleTestIT _outerInstance;

			 private Neo4NetRule _ruleWithDirectory;

			 public StatementAnonymousInnerClass( JUnitRuleTestIT outerInstance, Neo4NetRule ruleWithDirectory )
			 {
				 this.outerInstance = outerInstance;
				 this._ruleWithDirectory = ruleWithDirectory;
			 }

			 public override void evaluate()
			 {
				  // Then the database is not empty
				  Result result = _ruleWithDirectory.GraphDatabaseService.execute( "MATCH (n) RETURN count(n) AS " + "count" );

				  IList<object> column = Iterators.asList( result.ColumnAs( "count" ) );
				  assertEquals( 1, column.Count );
				  assertEquals( 1L, column[0] );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseSystemTimeZoneForLogging() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUseSystemTimeZoneForLogging()
		 {
			  string currentOffset = CurrentTimeZoneOffsetString();

			  assertThat( ContentOf( "Neo4Net.log" ), containsString( currentOffset ) );
			  assertThat( ContentOf( "debug.log" ), containsString( currentOffset ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String contentOf(String file) throws java.io.IOException
		 private string ContentOf( string file )
		 {
			  GraphDatabaseAPI api = ( GraphDatabaseAPI ) Neo4Net.GraphDatabaseService;
			  Config config = api.DependencyResolver.resolveDependency( typeof( Config ) );
			  File dataDirectory = config.Get( GraphDatabaseSettings.data_directory );
			  return new string( Files.readAllBytes( ( new File( dataDirectory, file ) ).toPath() ), UTF_8 );
		 }

		 private static string CurrentTimeZoneOffsetString()
		 {
			  ZoneOffset offset = OffsetDateTime.now().Offset;
			  return offset.Equals( UTC ) ? "+0000" : offset.ToString().Replace(":", "");
		 }
	}

}
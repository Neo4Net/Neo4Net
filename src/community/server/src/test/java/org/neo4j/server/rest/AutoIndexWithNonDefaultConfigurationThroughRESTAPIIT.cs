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
namespace Neo4Net.Server.rest
{
	using AfterClass = org.junit.AfterClass;
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using TemporaryFolder = org.junit.rules.TemporaryFolder;

	using CommunityServerBuilder = Neo4Net.Server.helpers.CommunityServerBuilder;
	using FunctionalTestHelper = Neo4Net.Server.helpers.FunctionalTestHelper;
	using ServerHelper = Neo4Net.Server.helpers.ServerHelper;
	using Neo4Net.Test;
	using ExclusiveServerTestBase = Neo4Net.Test.server.ExclusiveServerTestBase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class AutoIndexWithNonDefaultConfigurationThroughRESTAPIIT : ExclusiveServerTestBase
	{
		 private static CommunityNeoServer _server;
		 private static FunctionalTestHelper _functionalTestHelper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static org.junit.rules.TemporaryFolder staticFolder = new org.junit.rules.TemporaryFolder();
		 public static TemporaryFolder StaticFolder = new TemporaryFolder();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.TestData<RESTRequestGenerator> gen = Neo4Net.test.TestData.producedThrough(RESTRequestGenerator.PRODUCER);
		 public TestData<RESTRequestGenerator> Gen = TestData.producedThrough( RESTRequestGenerator.PRODUCER );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void allocateServer() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void AllocateServer()
		 {
			  _server = CommunityServerBuilder.serverOnRandomPorts().usingDataDir(StaticFolder.Root.AbsolutePath).withAutoIndexingEnabledForNodes("foo", "bar").build();
			  _server.start();
			  _functionalTestHelper = new FunctionalTestHelper( _server );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void cleanTheDatabase()
		 public virtual void CleanTheDatabase()
		 {
			  ServerHelper.cleanTheDatabase( _server );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void stopServer()
		 public static void StopServer()
		 {
			  _server.stop();
		 }

		 /// <summary>
		 /// Create an auto index for nodes with specific configuration.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateANodeAutoIndexWithGivenFullTextConfiguration()
		 public virtual void ShouldCreateANodeAutoIndexWithGivenFullTextConfiguration()
		 {
			  string responseBody = Gen.get().expectedStatus(201).payload("{\"name\":\"node_auto_index\", \"config\":{\"type\":\"fulltext\",\"provider\":\"lucene\"}}").post(_functionalTestHelper.nodeIndexUri()).entity();

			  assertThat( responseBody, containsString( "\"type\" : \"fulltext\"" ) );
		 }

		 /// <summary>
		 /// Create an auto index for relationships with specific configuration.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateARelationshipAutoIndexWithGivenFullTextConfiguration()
		 public virtual void ShouldCreateARelationshipAutoIndexWithGivenFullTextConfiguration()
		 {
			  string responseBody = Gen.get().expectedStatus(201).payload("{\"name\":\"relationship_auto_index\", \"config\":{\"type\":\"fulltext\"," + "\"provider\":\"lucene\"}}").post(_functionalTestHelper.relationshipIndexUri()).entity();

			  assertThat( responseBody, containsString( "\"type\" : \"fulltext\"" ) );
		 }

	}

}
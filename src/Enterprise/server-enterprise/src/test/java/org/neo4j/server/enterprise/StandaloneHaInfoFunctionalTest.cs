using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Server.enterprise
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using FunctionalTestHelper = Neo4Net.Server.helpers.FunctionalTestHelper;
	using JaxRsResponse = Neo4Net.Server.rest.JaxRsResponse;
	using RestRequest = Neo4Net.Server.rest.RestRequest;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.MasterInfoService.BASE_PATH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.MasterInfoService.IS_MASTER_PATH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.MasterInfoService.IS_SLAVE_PATH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.ha.EnterpriseServerHelper.createNonPersistentServer;

	public class StandaloneHaInfoFunctionalTest
	{
		 private static OpenEnterpriseNeoServer _server;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory target = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Target = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Before()
		 {
			  _server = createNonPersistentServer( Target.directory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  if ( _server != null )
			  {
					_server.stop();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testHaDiscoveryOnStandaloneReturns403()
		 public virtual void TestHaDiscoveryOnStandaloneReturns403()
		 {
			  FunctionalTestHelper helper = new FunctionalTestHelper( _server );

			  JaxRsResponse response = RestRequest.req().get(GetBasePath(helper));
			  assertEquals( SC_FORBIDDEN, response.Status );
		 }

		 private string GetBasePath( FunctionalTestHelper helper )
		 {
			  return helper.ManagementUri() + "/" + BASE_PATH;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIsMasterOnStandaloneReturns403()
		 public virtual void TestIsMasterOnStandaloneReturns403()
		 {
			  FunctionalTestHelper helper = new FunctionalTestHelper( _server );

			  JaxRsResponse response = RestRequest.req().get(GetBasePath(helper) + IS_MASTER_PATH);
			  assertEquals( SC_FORBIDDEN, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIsSlaveOnStandaloneReturns403()
		 public virtual void TestIsSlaveOnStandaloneReturns403()
		 {
			  FunctionalTestHelper helper = new FunctionalTestHelper( _server );

			  JaxRsResponse response = RestRequest.req().get(GetBasePath(helper) + IS_SLAVE_PATH);
			  assertEquals( SC_FORBIDDEN, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDiscoveryListingOnStandaloneDoesNotContainHA() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestDiscoveryListingOnStandaloneDoesNotContainHA()
		 {
			  FunctionalTestHelper helper = new FunctionalTestHelper( _server );

			  JaxRsResponse response = RestRequest.req().get(helper.ManagementUri());

			  IDictionary<string, object> map = JsonHelper.jsonToMap( response.Entity );

			 assertEquals( 2, ( ( System.Collections.IDictionary ) map["services"] ).Count );
		 }
	}

}
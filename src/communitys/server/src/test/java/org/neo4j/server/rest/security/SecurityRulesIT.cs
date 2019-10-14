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
namespace Neo4Net.Server.rest.security
{
	using DummyThirdPartyWebService = Org.Dummy.Web.Service.DummyThirdPartyWebService;
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using CommunityServerBuilder = Neo4Net.Server.helpers.CommunityServerBuilder;
	using FunctionalTestHelper = Neo4Net.Server.helpers.FunctionalTestHelper;
	using Neo4Net.Test;
	using Title = Neo4Net.Test.TestData.Title;
	using ExclusiveServerTestBase = Neo4Net.Test.server.ExclusiveServerTestBase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class SecurityRulesIT : ExclusiveServerTestBase
	{
		 private CommunityNeoServer _server;

		 private FunctionalTestHelper _functionalTestHelper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.TestData<org.neo4j.server.rest.RESTRequestGenerator> gen = org.neo4j.test.TestData.producedThrough(org.neo4j.server.rest.RESTRequestGenerator.PRODUCER);
		 public TestData<RESTRequestGenerator> Gen = TestData.producedThrough( RESTRequestGenerator.PRODUCER );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void stopServer()
		 public virtual void StopServer()
		 {
			  if ( _server != null )
			  {
					_server.stop();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Title("Enforcing Server Authorization Rules") @Documented("In this example, a (dummy) failing security rule is registered to deny\n" + "access to all URIs to the server by listing the rules class in\n" + "'neo4j.conf':\n" + "\n" + "@@config\n" + "\n" + "with the rule source code of:\n" + "\n" + "@@failingRule\n" + "\n" + "With this rule registered, any access to the server will be\n" + "denied. In a production-quality implementation the rule\n" + "will likely lookup credentials/claims in a 3rd-party\n" + "directory service (e.g. LDAP) or in a local database of\n" + "authorized users.") public void should401WithBasicChallengeWhenASecurityRuleFails() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Title("Enforcing Server Authorization Rules"), Documented("In this example, a (dummy) failing security rule is registered to deny\n" + "access to all URIs to the server by listing the rules class in\n" + "'neo4j.conf':\n" + "\n" + "@@config\n" + "\n" + "with the rule source code of:\n" + "\n" + "@@failingRule\n" + "\n" + "With this rule registered, any access to the server will be\n" + "denied. In a production-quality implementation the rule\n" + "will likely lookup credentials/claims in a 3rd-party\n" + "directory service (e.g. LDAP) or in a local database of\n" + "authorized users.")]
		 public virtual void Should401WithBasicChallengeWhenASecurityRuleFails()
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
			  _server = CommunityServerBuilder.serverOnRandomPorts().withDefaultDatabaseTuning().withSecurityRules(typeof(PermanentlyFailingSecurityRule).FullName).usingDataDir(Folder.directory(Name.MethodName).AbsolutePath).build();
			  _server.start();
			  _functionalTestHelper = new FunctionalTestHelper( _server );
			  JaxRsResponse response = Gen.get().expectedStatus(401).expectedHeader("WWW-Authenticate").post(_functionalTestHelper.nodeUri()).response();

			  assertThat( response.Headers.getFirst( "WWW-Authenticate" ), containsString( "Basic realm=\"" + PermanentlyFailingSecurityRule.REALM + "\"" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void should401WithBasicChallengeIfAnyOneOfTheRulesFails() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Should401WithBasicChallengeIfAnyOneOfTheRulesFails()
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
			  _server = CommunityServerBuilder.serverOnRandomPorts().withDefaultDatabaseTuning().withSecurityRules(typeof(PermanentlyFailingSecurityRule).FullName, typeof(PermanentlyPassingSecurityRule).FullName).usingDataDir(Folder.directory(Name.MethodName).AbsolutePath).build();
			  _server.start();
			  _functionalTestHelper = new FunctionalTestHelper( _server );

			  JaxRsResponse response = Gen.get().expectedStatus(401).expectedHeader("WWW-Authenticate").post(_functionalTestHelper.nodeUri()).response();

			  assertThat( response.Headers.getFirst( "WWW-Authenticate" ), containsString( "Basic realm=\"" + PermanentlyFailingSecurityRule.REALM + "\"" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeAllSecurityRules() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeAllSecurityRules()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
			  _server = CommunityServerBuilder.serverOnRandomPorts().withDefaultDatabaseTuning().withSecurityRules(typeof(NoAccessToDatabaseSecurityRule).FullName).usingDataDir(Folder.directory(Name.MethodName).AbsolutePath).build();
			  _server.start();
			  _functionalTestHelper = new FunctionalTestHelper( _server );

			  // when
			  Gen.get().expectedStatus(401).get(_functionalTestHelper.dataUri()).response();

			  // then
			  assertTrue( NoAccessToDatabaseSecurityRule.WasInvoked() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith201IfAllTheRulesPassWhenCreatingANode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith201IfAllTheRulesPassWhenCreatingANode()
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
			  _server = CommunityServerBuilder.serverOnRandomPorts().withDefaultDatabaseTuning().withSecurityRules(typeof(PermanentlyPassingSecurityRule).FullName).usingDataDir(Folder.directory(Name.MethodName).AbsolutePath).build();
			  _server.start();
			  _functionalTestHelper = new FunctionalTestHelper( _server );

			  Gen.get().expectedStatus(201).expectedHeader("Location").post(_functionalTestHelper.nodeUri()).response();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Title("Using Wildcards to Target Security Rules") @Documented("In this example, a security rule is registered to deny\n" + "access to all URIs to the server by listing the rule(s) class(es) in\n" + "'neo4j.conf'.\n" + "In this case, the rule is registered\n" + "using a wildcard URI path (where `*` characters can be used to signify\n" + "any part of the path). For example `/users*` means the rule\n" + "will be bound to any resources under the `/users` root path. Similarly\n" + "`/users*type*` will bind the rule to resources matching\n" + "URIs like `/users/fred/type/premium`.\n" + "\n" + "@@config\n" + "\n" + "with the rule source code of:\n" + "\n" + "@@failingRuleWithWildcardPath\n" + "\n" + "With this rule registered, any access to URIs under /protected/ will be\n" + "denied by the server. Using wildcards allows flexible targeting of security rules to\n" + "arbitrary parts of the server's API, including any unmanaged extensions or managed\n" + "plugins that have been registered.") public void aSimpleWildcardUriPathShould401OnAccessToProtectedSubPath() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Title("Using Wildcards to Target Security Rules"), Documented("In this example, a security rule is registered to deny\n" + "access to all URIs to the server by listing the rule(s) class(es) in\n" + "'neo4j.conf'.\n" + "In this case, the rule is registered\n" + "using a wildcard URI path (where `*` characters can be used to signify\n" + "any part of the path). For example `/users*` means the rule\n" + "will be bound to any resources under the `/users` root path. Similarly\n" + "`/users*type*` will bind the rule to resources matching\n" + "URIs like `/users/fred/type/premium`.\n" + "\n" + "@@config\n" + "\n" + "with the rule source code of:\n" + "\n" + "@@failingRuleWithWildcardPath\n" + "\n" + "With this rule registered, any access to URIs under /protected/ will be\n" + "denied by the server. Using wildcards allows flexible targeting of security rules to\n" + "arbitrary parts of the server's API, including any unmanaged extensions or managed\n" + "plugins that have been registered.")]
		 public virtual void ASimpleWildcardUriPathShould401OnAccessToProtectedSubPath()
		 {
			  string mountPoint = "/protected/tree/starts/here" + DummyThirdPartyWebService.DUMMY_WEB_SERVICE_MOUNT_POINT;
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
			  _server = CommunityServerBuilder.serverOnRandomPorts().withDefaultDatabaseTuning().withThirdPartyJaxRsPackage("org.dummy.web.service", mountPoint).withSecurityRules(typeof(PermanentlyFailingSecurityRuleWithWildcardPath).FullName).usingDataDir(Folder.directory(Name.MethodName).AbsolutePath).build();
			  _server.start();

			  _functionalTestHelper = new FunctionalTestHelper( _server );

			  JaxRsResponse clientResponse = Gen.get().expectedStatus(401).expectedType(MediaType.APPLICATION_JSON_TYPE).expectedHeader("WWW-Authenticate").get(TrimTrailingSlash(_functionalTestHelper.baseUri()) + mountPoint + "/more/stuff").response();

			  assertEquals( 401, clientResponse.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Title("Using Complex Wildcards to Target Security Rules") @Documented("In this example, a security rule is registered to deny\n" + "access to all URIs matching a complex pattern.\n" + "The config looks like this:\n" + "\n" + "@@config\n" + "\n" + "with the rule source code of:\n" + "\n" + "@@failingRuleWithComplexWildcardPath") public void aComplexWildcardUriPathShould401OnAccessToProtectedSubPath() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Title("Using Complex Wildcards to Target Security Rules"), Documented("In this example, a security rule is registered to deny\n" + "access to all URIs matching a complex pattern.\n" + "The config looks like this:\n" + "\n" + "@@config\n" + "\n" + "with the rule source code of:\n" + "\n" + "@@failingRuleWithComplexWildcardPath")]
		 public virtual void AComplexWildcardUriPathShould401OnAccessToProtectedSubPath()
		 {
			  string mountPoint = "/protected/wildcard_replacement/x/y/z/something/else/more_wildcard_replacement/a/b/c" +
						 "/final/bit";
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
			  _server = CommunityServerBuilder.serverOnRandomPorts().withDefaultDatabaseTuning().withThirdPartyJaxRsPackage("org.dummy.web.service", mountPoint).withSecurityRules(typeof(PermanentlyFailingSecurityRuleWithComplexWildcardPath).FullName).usingDataDir(Folder.directory(Name.MethodName).AbsolutePath).build();
			  _server.start();

			  _functionalTestHelper = new FunctionalTestHelper( _server );

			  JaxRsResponse clientResponse = Gen.get().expectedStatus(401).expectedType(MediaType.APPLICATION_JSON_TYPE).expectedHeader("WWW-Authenticate").get(TrimTrailingSlash(_functionalTestHelper.baseUri()) + mountPoint + "/more/stuff").response();

			  assertEquals( 401, clientResponse.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void should403WhenAuthenticatedButForbidden() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Should403WhenAuthenticatedButForbidden()
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
			  _server = CommunityServerBuilder.serverOnRandomPorts().withDefaultDatabaseTuning().withSecurityRules(typeof(PermanentlyForbiddenSecurityRule).FullName, typeof(PermanentlyPassingSecurityRule).FullName).usingDataDir(Folder.directory(Name.MethodName).AbsolutePath).build();
			  _server.start();
			  _functionalTestHelper = new FunctionalTestHelper( _server );

			  JaxRsResponse clientResponse = Gen.get().expectedStatus(403).expectedType(MediaType.APPLICATION_JSON_TYPE).get(TrimTrailingSlash(_functionalTestHelper.baseUri())).response();

			  assertEquals( 403, clientResponse.Status );
		 }

		 private string TrimTrailingSlash( URI uri )
		 {
			  string result = uri.ToString();
			  if ( result.EndsWith( "/", StringComparison.Ordinal ) )
			  {
					return result.Substring( 0, result.Length - 1 );
			  }
			  else
			  {
					return result;
			  }
		 }
	}

}
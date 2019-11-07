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
namespace Neo4Net.Server.modules
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ArgumentMatcher = org.mockito.ArgumentMatcher;


	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Config = Neo4Net.Kernel.configuration.Config;
	using UserService = Neo4Net.Server.rest.dbms.UserService;
	using DiscoverableURIs = Neo4Net.Server.rest.discovery.DiscoverableURIs;
	using WebServer = Neo4Net.Server.web.WebServer;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyCollection;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.argThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.isNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class DBMSModuleTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.SuppressOutput suppressOutput = Neo4Net.test.rule.SuppressOutput.suppress(Neo4Net.test.rule.SuppressOutput.System.err, Neo4Net.test.rule.SuppressOutput.System.out);
		 public SuppressOutput SuppressOutput = SuppressOutput.suppress( SuppressOutput.System.err, SuppressOutput.System.out );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldRegisterAtRootByDefault() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRegisterAtRootByDefault()
		 {
			  WebServer webServer = mock( typeof( WebServer ) );
			  Config config = mock( typeof( Config ) );

			  CommunityNeoServer neoServer = mock( typeof( CommunityNeoServer ) );
			  when( neoServer.BaseUri() ).thenReturn(new URI("http://localhost:7575"));
			  when( neoServer.WebServer ).thenReturn( webServer );
			  when( config.Get( GraphDatabaseSettings.auth_enabled ) ).thenReturn( true );

			  DBMSModule module = new DBMSModule( webServer, config, () => (new DiscoverableURIs.Builder()).build() );

			  module.Start();

			  verify( webServer ).addJAXRSClasses( anyList(), anyString(), Null );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldNotRegisterUserServiceWhenAuthDisabled() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotRegisterUserServiceWhenAuthDisabled()
		 {
			  WebServer webServer = mock( typeof( WebServer ) );
			  Config config = mock( typeof( Config ) );

			  CommunityNeoServer neoServer = mock( typeof( CommunityNeoServer ) );
			  when( neoServer.BaseUri() ).thenReturn(new URI("http://localhost:7575"));
			  when( neoServer.WebServer ).thenReturn( webServer );
			  when( config.Get( GraphDatabaseSettings.auth_enabled ) ).thenReturn( false );

			  DBMSModule module = new DBMSModule( webServer, config, () => (new DiscoverableURIs.Builder()).build() );

			  module.Start();

			  verify( webServer ).addJAXRSClasses( anyList(), anyString(), Null );
			  verify( webServer, never() ).addJAXRSClasses(argThat(new ArgumentMatcherAnonymousInnerClass(this))}}
			 , anyString(), anyCollection());
		 }
using System.Collections.Generic;

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
namespace Neo4Net.Server.modules
{
	using Test = org.junit.Test;


	using Config = Neo4Net.Kernel.configuration.Config;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;
	using ThirdPartyJaxRsPackage = Neo4Net.Server.configuration.ThirdPartyJaxRsPackage;
	using Database = Neo4Net.Server.database.Database;
	using WebServer = Neo4Net.Server.web.WebServer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyCollection;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ThirdPartyJAXRSModuleTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportThirdPartyPackagesAtSpecifiedMount() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportThirdPartyPackagesAtSpecifiedMount()
		 {
			  // Given
			  WebServer webServer = mock( typeof( WebServer ) );

			  CommunityNeoServer neoServer = mock( typeof( CommunityNeoServer ) );
			  when( neoServer.BaseUri() ).thenReturn(new URI("http://localhost:7575"));
			  when( neoServer.WebServer ).thenReturn( webServer );
			  Database database = mock( typeof( Database ) );
			  when( neoServer.Database ).thenReturn( database );

			  Config config = mock( typeof( Config ) );
			  IList<ThirdPartyJaxRsPackage> jaxRsPackages = new List<ThirdPartyJaxRsPackage>();
			  string path = "/third/party/package";
			  jaxRsPackages.Add( new ThirdPartyJaxRsPackage( "org.example.neo4j", path ) );
			  when( config.Get( ServerSettings.third_party_packages ) ).thenReturn( jaxRsPackages );

			  // When
			  ThirdPartyJAXRSModule module = new ThirdPartyJAXRSModule( webServer, config, NullLogProvider.Instance, neoServer );
			  module.Start();

			  // Then
			  verify( webServer ).addJAXRSPackages( any( typeof( System.Collections.IList ) ), anyString(), anyCollection() );
		 }
	}

}
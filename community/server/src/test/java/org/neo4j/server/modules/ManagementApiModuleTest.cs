﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Server.modules
{
	using Test = org.junit.Test;


	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ServerSettings = Org.Neo4j.Server.configuration.ServerSettings;
	using WebServer = Org.Neo4j.Server.web.WebServer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyListOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ManagementApiModuleTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRegisterASingleUri() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRegisterASingleUri()
		 {
			  WebServer webServer = mock( typeof( WebServer ) );

			  CommunityNeoServer neoServer = mock( typeof( CommunityNeoServer ) );
			  when( neoServer.BaseUri() ).thenReturn(new URI("http://localhost:7575"));
			  when( neoServer.WebServer ).thenReturn( webServer );

			  IDictionary<string, string> @params = new Dictionary<string, string>();
			  string managementPath = "/db/manage";
			  @params[ServerSettings.management_api_path.name()] = managementPath;
			  Config config = Config.defaults( @params );

			  when( neoServer.Config ).thenReturn( config );

			  ManagementApiModule module = new ManagementApiModule( webServer, config );
			  module.Start();

			  verify( webServer ).addJAXRSClasses( anyListOf( typeof( string ) ), anyString(), any() );
		 }
	}

}
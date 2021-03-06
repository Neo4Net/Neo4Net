﻿using System;

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
namespace Org.Neo4j.Server.rest.security
{
	using After = org.junit.After;

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using ServerSettings = Org.Neo4j.Server.configuration.ServerSettings;
	using CommunityServerBuilder = Org.Neo4j.Server.helpers.CommunityServerBuilder;
	using ExclusiveServerTestBase = Org.Neo4j.Test.server.ExclusiveServerTestBase;

	public class CommunityServerTestBase : ExclusiveServerTestBase
	{
		 protected internal CommunityNeoServer Server;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup()
		 public virtual void Cleanup()
		 {
			  if ( Server != null )
			  {
					Server.stop();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void startServer(boolean authEnabled) throws java.io.IOException
		 protected internal virtual void StartServer( bool authEnabled )
		 {
			  Server = CommunityServerBuilder.serverOnRandomPorts().withProperty(GraphDatabaseSettings.auth_enabled.name(), Convert.ToString(authEnabled)).build();
			  Server.start();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void startServer(boolean authEnabled, String accessControlAllowOrigin) throws java.io.IOException
		 protected internal virtual void StartServer( bool authEnabled, string accessControlAllowOrigin )
		 {
			  Server = CommunityServerBuilder.serverOnRandomPorts().withProperty(GraphDatabaseSettings.auth_enabled.name(), Convert.ToString(authEnabled)).withProperty(ServerSettings.http_access_control_allow_origin.name(), accessControlAllowOrigin).build();
			  Server.start();
		 }

		 protected internal virtual string DataURL()
		 {
			  return Server.baseUri().resolve("db/data/").ToString();
		 }

		 protected internal virtual string UserURL( string username )
		 {
			  return Server.baseUri().resolve("user/" + username).ToString();
		 }

		 protected internal virtual string PasswordURL( string username )
		 {
			  return Server.baseUri().resolve("user/" + username + "/password").ToString();
		 }

		 protected internal virtual string TxCommitURL()
		 {
			  return Server.baseUri().resolve("db/data/transaction/commit").ToString();
		 }
	}

}
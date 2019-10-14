using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Server.security.enterprise.auth.integration.bolt
{

	using AuthenticationIT = Neo4Net.Bolt.v1.transport.integration.AuthenticationIT;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using TestEnterpriseGraphDatabaseFactory = Neo4Net.Test.TestEnterpriseGraphDatabaseFactory;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

	public class EnterpriseAuthenticationIT : AuthenticationIT
	{
		 protected internal override TestGraphDatabaseFactory TestGraphDatabaseFactory
		 {
			 get
			 {
				  return new TestEnterpriseGraphDatabaseFactory( LogProvider );
			 }
		 }

		 protected internal override System.Action<IDictionary<string, string>> SettingsFunction
		 {
			 get
			 {
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final java.nio.file.Path homeDir;
				  Path homeDir;
				  try
				  {
						homeDir = Files.createTempDirectory( "logs" );
				  }
				  catch ( IOException e )
				  {
						throw new Exception( "Test setup failed to create temporary directory", e );
				  }
   
				  return settings =>
				  {
					settings.put( GraphDatabaseSettings.auth_enabled.name(), "true" );
					settings.put( GraphDatabaseSettings.logs_directory.name(), homeDir.toAbsolutePath().ToString() );
				  };
			 }
		 }

		 public override void ShouldFailIfMalformedAuthTokenUnknownScheme()
		 {
			  // Ignore this test in enterprise since custom schemes may be allowed
		 }
	}

}
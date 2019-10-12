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
namespace Neo4Net.Test.server.ha
{

	using OpenEnterpriseNeoServer = Neo4Net.Server.enterprise.OpenEnterpriseNeoServer;
	using EnterpriseServerBuilder = Neo4Net.Server.enterprise.helpers.EnterpriseServerBuilder;

	public class EnterpriseServerHelper
	{
		 private EnterpriseServerHelper()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.server.enterprise.OpenEnterpriseNeoServer createNonPersistentServer(java.io.File databaseDir) throws java.io.IOException
		 public static OpenEnterpriseNeoServer CreateNonPersistentServer( File databaseDir )
		 {
			  return CreateServer( databaseDir, false );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.server.enterprise.OpenEnterpriseNeoServer createServer(java.io.File databaseDir, boolean persistent) throws java.io.IOException
		 private static OpenEnterpriseNeoServer CreateServer( File databaseDir, bool persistent )
		 {
			  EnterpriseServerBuilder builder = EnterpriseServerBuilder.serverOnRandomPorts().usingDataDir(databaseDir.AbsolutePath);
			  if ( persistent )
			  {
					builder = ( EnterpriseServerBuilder ) builder.Persistent();
			  }
			  builder.WithDefaultDatabaseTuning();
			  OpenEnterpriseNeoServer server = builder.Build();
			  server.Start();
			  return server;
		 }
	}

}
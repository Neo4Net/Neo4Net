using System;
using System.Threading;

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
namespace Neo4Net.Server.enterprise.jmx
{
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.@internal.LogService;

	public sealed class ServerManagement : ServerManagementMBean
	{
		 private readonly NeoServer _server;

		 public ServerManagement( NeoServer server )
		 {
			  this._server = server;
		 }

		 public override void RestartServer()
		 {
			 lock ( this )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.logging.Log log = server.getDatabase().getGraph().getDependencyResolver().resolveDependency(org.neo4j.logging.internal.LogService.class).getUserLog(getClass());
				  Log log = _server.Database.Graph.DependencyResolver.resolveDependency( typeof( LogService ) ).getUserLog( this.GetType() );
      
				  Thread thread = new Thread("Restart server thread"() =>
				  {
					 log.Info( "Restarting server" );
					 _server.stop();
					 _server.start();
				  });
				  thread.Daemon = false;
				  thread.Start();
      
				  try
				  {
						thread.Join();
				  }
				  catch ( InterruptedException e )
				  {
						throw new Exception( e );
				  }
			 }
		 }
	}

}
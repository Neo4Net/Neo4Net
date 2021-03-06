﻿using System;

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
namespace Org.Neo4j.Server.enterprise.modules
{

	using ServerManagement = Org.Neo4j.Server.enterprise.jmx.ServerManagement;
	using ServerModule = Org.Neo4j.Server.modules.ServerModule;

	public class JMXManagementModule : ServerModule
	{
		 private readonly NeoServer _server;

		 public JMXManagementModule( NeoServer server )
		 {
			  this._server = server;
		 }

		 public override void Start()
		 {
			  try
			  {
					ServerManagement serverManagement = new ServerManagement( _server );
					MBeanServer beanServer = ManagementFactory.PlatformMBeanServer;
					beanServer.registerMBean( serverManagement, CreateObjectName() );
			  }
			  catch ( Exception e )
			  {
					throw new Exception( "Unable to initialize jmx management, see nested exception.", e );
			  }
		 }

		 public override void Stop()
		 {
			  try
			  {
					MBeanServer beanServer = ManagementFactory.PlatformMBeanServer;
					beanServer.unregisterMBean( CreateObjectName() );
			  }
			  catch ( InstanceNotFoundException )
			  {
					// ok
			  }
			  catch ( Exception e )
			  {
					throw new Exception( "Unable to shut down jmx management, see nested exception.", e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private javax.management.ObjectName createObjectName() throws javax.management.MalformedObjectNameException
		 private ObjectName CreateObjectName()
		 {
			  return new ObjectName( "org.neo4j.ServerManagement", "restartServer", "lifecycle" );
		 }
	}

}
﻿/*
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
namespace Neo4Net.Harness.Internal
{

	using Dependencies = Neo4Net.Graphdb.facade.GraphDatabaseFacadeFactory.Dependencies;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using AbstractNeoServer = Neo4Net.Server.AbstractNeoServer;
	using EnterpriseGraphFactory = Neo4Net.Server.database.EnterpriseGraphFactory;
	using GraphFactory = Neo4Net.Server.database.GraphFactory;
	using OpenEnterpriseNeoServer = Neo4Net.Server.enterprise.OpenEnterpriseNeoServer;

	public class EnterpriseInProcessServerBuilder : AbstractInProcessServerBuilder
	{
		 public EnterpriseInProcessServerBuilder() : this(new File(System.getProperty("java.io.tmpdir")))
		 {
		 }

		 public EnterpriseInProcessServerBuilder( File workingDir ) : base( workingDir )
		 {
		 }

		 public EnterpriseInProcessServerBuilder( File workingDir, string dataSubDir ) : base( workingDir, dataSubDir )
		 {
		 }

		 protected internal override GraphFactory CreateGraphFactory( Config config )
		 {
			  return new EnterpriseGraphFactory();
		 }

		 protected internal override AbstractNeoServer CreateNeoServer( GraphFactory graphFactory, Config config, Dependencies dependencies )
		 {
			  return new OpenEnterpriseNeoServer( config, graphFactory, dependencies );
		 }
	}

}
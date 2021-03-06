﻿/*
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
namespace Org.Neo4j.Harness.@internal
{

	using Dependencies = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory.Dependencies;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using AbstractNeoServer = Org.Neo4j.Server.AbstractNeoServer;
	using CommunityNeoServer = Org.Neo4j.Server.CommunityNeoServer;
	using CommunityGraphFactory = Org.Neo4j.Server.database.CommunityGraphFactory;
	using GraphFactory = Org.Neo4j.Server.database.GraphFactory;

	public class InProcessServerBuilder : AbstractInProcessServerBuilder
	{
		 public InProcessServerBuilder() : this(new File(System.getProperty("java.io.tmpdir")))
		 {
		 }

		 public InProcessServerBuilder( File workingDir ) : base( workingDir )
		 {
		 }

		 protected internal override GraphFactory CreateGraphFactory( Config config )
		 {
			  return new CommunityGraphFactory();
		 }

		 protected internal override AbstractNeoServer CreateNeoServer( GraphFactory graphFactory, Config config, Dependencies dependencies )
		 {
			  return new CommunityNeoServer( config, graphFactory, dependencies );
		 }
	}

}
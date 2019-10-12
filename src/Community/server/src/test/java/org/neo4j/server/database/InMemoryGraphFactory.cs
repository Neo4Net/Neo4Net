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
namespace Neo4Net.Server.database
{

	using GraphDatabaseDependencies = Neo4Net.Graphdb.facade.GraphDatabaseDependencies;
	using GraphDatabaseFacadeFactory = Neo4Net.Graphdb.facade.GraphDatabaseFacadeFactory;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using Config = Neo4Net.Kernel.configuration.Config;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using ImpermanentGraphDatabase = Neo4Net.Test.ImpermanentGraphDatabase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	public class InMemoryGraphFactory : GraphFactory
	{
		 public override GraphDatabaseFacade NewGraphDatabase( Config config, GraphDatabaseFacadeFactory.Dependencies dependencies )
		 {
			  File storeDir = config.Get( GraphDatabaseSettings.database_path );
			  config.Augment( stringMap( GraphDatabaseSettings.ephemeral.name(), "true", (new BoltConnector("bolt")).listen_address.name(), "localhost:0" ) );
			  return new ImpermanentGraphDatabase( storeDir, config, GraphDatabaseDependencies.newDependencies( dependencies ) );
		 }
	}

}
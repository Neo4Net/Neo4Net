using System;
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
namespace Org.Neo4j.Server
{

	using Config = Org.Neo4j.Kernel.configuration.Config;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using Database = Org.Neo4j.Server.database.Database;
	using GraphFactory = Org.Neo4j.Server.database.GraphFactory;
	using LifecycleManagingDatabase = Org.Neo4j.Server.database.LifecycleManagingDatabase;
	using DisabledPluginManager = Org.Neo4j.Server.plugins.DisabledPluginManager;
	using PluginManager = Org.Neo4j.Server.plugins.PluginManager;
	using AdvertisableService = Org.Neo4j.Server.rest.management.AdvertisableService;
	using DisabledTransactionRegistry = Org.Neo4j.Server.rest.transactional.DisabledTransactionRegistry;
	using TransactionRegistry = Org.Neo4j.Server.rest.transactional.TransactionRegistry;

	using static Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory.Dependencies;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.AbstractNeoServer.NEO4J_IS_STARTING_MESSAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.exception.ServerStartupErrors.translateToServerStartupError;

	public class DisabledNeoServer : NeoServer
	{
		 private readonly Database _db;
		 private readonly Config _config;

		 private readonly LifeSupport _life = new LifeSupport();

		 public DisabledNeoServer( GraphFactory graphFactory, Dependencies dependencies, Config config )
		 {
			  this._db = new LifecycleManagingDatabase( config, graphFactory, dependencies );
			  this._config = config;

			  _life.add( _db );
			  dependencies.userLogProvider().getLog(this.GetType()).info(NEO4J_IS_STARTING_MESSAGE);
		 }

		 public override void Start()
		 {
			  try
			  {
					_life.start();
			  }
			  catch ( Exception t )
			  {
					_life.shutdown();
					throw translateToServerStartupError( t );
			  }
		 }

		 public override void Stop()
		 {
			  _life.stop();
		 }

		 public virtual Config Config
		 {
			 get
			 {
				  return _config;
			 }
		 }

		 public virtual Database Database
		 {
			 get
			 {
				  return _db;
			 }
		 }

		 public virtual TransactionRegistry TransactionRegistry
		 {
			 get
			 {
				  return DisabledTransactionRegistry.INSTANCE;
			 }
		 }

		 public virtual PluginManager ExtensionManager
		 {
			 get
			 {
				  return DisabledPluginManager.INSTANCE;
			 }
		 }

		 public override URI BaseUri()
		 {
			  throw new System.NotSupportedException( "Neo4j server is disabled" );
		 }

		 public virtual IEnumerable<AdvertisableService> Services
		 {
			 get
			 {
				  return Collections.emptyList();
			 }
		 }
	}

}
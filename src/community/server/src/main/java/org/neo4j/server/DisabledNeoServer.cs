using System;
using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Server
{

	using Config = Neo4Net.Kernel.configuration.Config;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Database = Neo4Net.Server.database.Database;
	using GraphFactory = Neo4Net.Server.database.GraphFactory;
	using LifecycleManagingDatabase = Neo4Net.Server.database.LifecycleManagingDatabase;
	using DisabledPluginManager = Neo4Net.Server.plugins.DisabledPluginManager;
	using PluginManager = Neo4Net.Server.plugins.PluginManager;
	using AdvertisableService = Neo4Net.Server.rest.management.AdvertisableService;
	using DisabledTransactionRegistry = Neo4Net.Server.rest.transactional.DisabledTransactionRegistry;
	using TransactionRegistry = Neo4Net.Server.rest.transactional.TransactionRegistry;

	using static Neo4Net.GraphDb.facade.GraphDatabaseFacadeFactory.Dependencies;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.AbstractNeoServer.Neo4Net_IS_STARTING_MESSAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.exception.ServerStartupErrors.translateToServerStartupError;

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
			  dependencies.userLogProvider().getLog(this.GetType()).info(Neo4Net_IS_STARTING_MESSAGE);
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
			  throw new System.NotSupportedException( "Neo4Net server is disabled" );
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
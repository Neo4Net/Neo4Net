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
namespace Neo4Net.Dmbs.Database
{

	using DatabaseManager = Neo4Net.Dbms.database.DatabaseManager;
	using ClassicCoreSPI = Neo4Net.Graphdb.facade.spi.ClassicCoreSPI;
	using DataSourceModule = Neo4Net.Graphdb.factory.module.DataSourceModule;
	using PlatformModule = Neo4Net.Graphdb.factory.module.PlatformModule;
	using AbstractEditionModule = Neo4Net.Graphdb.factory.module.edition.AbstractEditionModule;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Logger = Neo4Net.Logging.Logger;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.checkState;

	public sealed class DefaultDatabaseManager : LifecycleAdapter, DatabaseManager
	{
		 private GraphDatabaseFacade _database;
		 private readonly PlatformModule _platform;
		 private readonly AbstractEditionModule _edition;
		 private readonly Procedures _procedures;
		 private readonly Logger _log;
		 private readonly GraphDatabaseFacade _graphDatabaseFacade;
		 private string _databaseName;

		 public DefaultDatabaseManager( PlatformModule platform, AbstractEditionModule edition, Procedures procedures, Logger log, GraphDatabaseFacade graphDatabaseFacade )
		 {
			  this._platform = platform;
			  this._edition = edition;
			  this._procedures = procedures;
			  this._log = log;
			  this._graphDatabaseFacade = graphDatabaseFacade;
		 }

		 public override Optional<GraphDatabaseFacade> GetDatabaseFacade( string name )
		 {
			  return Optional.ofNullable( _database );
		 }

		 public override GraphDatabaseFacade CreateDatabase( string databaseName )
		 {
			  checkState( _database == null, "Database is already created, fail to create another one." );
			  _log.log( "Creating '%s' database.", databaseName );
			  DataSourceModule dataSource = new DataSourceModule( databaseName, _platform, _edition, _procedures, _graphDatabaseFacade );
			  ClassicCoreSPI spi = new ClassicCoreSPI( _platform, dataSource, _log, dataSource.CoreAPIAvailabilityGuard, _edition.ThreadToTransactionBridge );
			  _graphDatabaseFacade.init( spi, _edition.ThreadToTransactionBridge, _platform.config, dataSource.NeoStoreDataSource.TokenHolders );
			  _platform.dataSourceManager.register( dataSource.NeoStoreDataSource );
			  _database = _graphDatabaseFacade;
			  this._databaseName = databaseName;
			  return _database;
		 }

		 public override void ShutdownDatabase( string ignore )
		 {
			  ShutdownDatabase();
		 }

		 public override void Stop()
		 {
			  ShutdownDatabase();
		 }

		 private void ShutdownDatabase()
		 {
			  if ( _database != null )
			  {
					_log.log( "Shutting down '%s' database.", _database.databaseLayout().DatabaseName );
					_database.shutdown();
			  }
		 }

		 public override IList<string> ListDatabases()
		 {
			  return string.ReferenceEquals( _databaseName, null ) ? Collections.emptyList() : Collections.singletonList(_databaseName);
		 }
	}

}
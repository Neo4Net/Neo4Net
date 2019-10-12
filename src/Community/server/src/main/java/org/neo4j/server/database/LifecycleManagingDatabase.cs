using System;
using System.Diagnostics;

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

	using Result = Neo4Net.Graphdb.Result;
	using GraphDatabaseFacadeFactory = Neo4Net.Graphdb.facade.GraphDatabaseFacadeFactory;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using AvailabilityGuard = Neo4Net.Kernel.availability.AvailabilityGuard;
	using Config = Neo4Net.Kernel.configuration.Config;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using Log = Neo4Net.Logging.Log;

	/// <summary>
	/// Wraps a neo4j database in lifecycle management. This is intermediate, and will go away once we have an internal
	/// database that exposes lifecycle cleanly.
	/// </summary>
	public class LifecycleManagingDatabase : Database
	{
		 internal const string CYPHER_WARMUP_QUERY = "MATCH (a:` This query is just used to load the cypher compiler during warmup. Please ignore `) RETURN a LIMIT 0";

		 private readonly Config _config;
		 private readonly GraphFactory _dbFactory;
		 private readonly GraphDatabaseFacadeFactory.Dependencies _dependencies;
		 private readonly Log _log;
		 private volatile AvailabilityGuard _availabilityGuard;

		 private bool _isRunning;
		 private GraphDatabaseFacade _graph;

		 public LifecycleManagingDatabase( Config config, GraphFactory dbFactory, GraphDatabaseFacadeFactory.Dependencies dependencies )
		 {
			  this._config = config;
			  this._dbFactory = dbFactory;
			  this._dependencies = new AvailabiltyGuardCapturingDependencies( this.setAvailabilityGuard, dependencies );
			  this._log = dependencies.UserLogProvider().getLog(this.GetType());
		 }

		 public virtual File Location
		 {
			 get
			 {
				  return _config.get( GraphDatabaseSettings.database_path );
			 }
		 }

		 public virtual GraphDatabaseFacade Graph
		 {
			 get
			 {
				  return _graph;
			 }
		 }

		 public virtual AvailabilityGuard AvailabilityGuard
		 {
			 get
			 {
				  return _availabilityGuard;
			 }
			 set
			 {
				 lock ( this )
				 {
					  this._availabilityGuard = value;
				 }
			 }
		 }


		 public override void Init()
		 {
		 }

		 public override void Start()
		 {
			  _log.info( "Starting..." );
			  this._graph = _dbFactory( _config, _dependencies );
			  // in order to speed up testing, they should not run the preload, but in production it pays to do it.
			  if ( !InTestMode )
			  {
					PreLoadCypherCompiler();
			  }

			  _isRunning = true;
			  _log.info( "Started." );
		 }

		 public override void Stop()
		 {
			  if ( _graph != null )
			  {
					_log.info( "Stopping..." );
					_graph.shutdown();
					_isRunning = false;
					_graph = null;
					_log.info( "Stopped." );
			  }
		 }

		 public override void Shutdown()
		 {
		 }

		 public virtual bool Running
		 {
			 get
			 {
				  return _isRunning;
			 }
		 }

		 private void PreLoadCypherCompiler()
		 {
			  // Execute a single Cypher query to pre-load the compiler to make the first user-query snappy
			  try
			  {
					//noinspection EmptyTryBlock
					using ( Result ignore = this._graph.execute( CYPHER_WARMUP_QUERY ) )
					{
						 // empty by design
					}
			  }
			  catch ( Exception )
			  {
					// This is only an attempt at warming up the database.
					// It's not a critical failure.
			  }
		 }

		 protected internal virtual bool InTestMode
		 {
			 get
			 {
				  // The assumption here is that assertions are only enabled during testing.
				  bool testing = false;
				  Debug.Assert( testing = true, "yes, this should be an assignment!" );
				  return testing;
			 }
		 }
	}

}
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
namespace Org.Neo4j.Kernel.impl.transaction.state
{

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Org.Neo4j.Helpers;
	using Kernel = Org.Neo4j.@internal.Kernel.Api.Kernel;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;
	using LifecycleStatus = Org.Neo4j.Kernel.Lifecycle.LifecycleStatus;

	/// <summary>
	/// Adds change listener features to a <seealso cref="NeoStoreDataSource"/>.
	/// <p/>
	/// TODO This being a <seealso cref="Kernel"/> <seealso cref="Supplier"/> is a smell, it comes from established bad dependency hierarchy
	/// where <seealso cref="NeoStoreDataSource"/> and <seealso cref="Kernel"/> are needed before they exist.
	/// </summary>
	public class DataSourceManager : Lifecycle, System.Func<Kernel>
	{
		 private readonly Config _config;

		 public DataSourceManager( Config config )
		 {
			  this._config = config;
		 }

		 public interface Listener
		 {
			  void Registered( NeoStoreDataSource dataSource );

			  void Unregistered( NeoStoreDataSource dataSource );
		 }

		 private LifeSupport _life = new LifeSupport();
		 private readonly Listeners<Listener> _dsRegistrationListeners = new Listeners<Listener>();
		 private readonly IList<NeoStoreDataSource> _dataSources = new List<NeoStoreDataSource>();

		 public virtual void AddListener( Listener listener )
		 {
			  if ( _life.Status.Equals( LifecycleStatus.STARTED ) )
			  {
					try
					{
						 _dataSources.ForEach( listener.registered );
					}
					catch ( Exception )
					{ // OK
					}
			  }
			  _dsRegistrationListeners.add( listener );
		 }

		 public virtual void Register( NeoStoreDataSource dataSource )
		 {
			  _dataSources.Add( dataSource );
			  if ( _life.Status.Equals( LifecycleStatus.STARTED ) )
			  {
					_life.add( dataSource );
					_dsRegistrationListeners.notify( listener => listener.registered( dataSource ) );
			  }
		 }

		 public virtual void Unregister( NeoStoreDataSource dataSource )
		 {
			  _dataSources.Remove( dataSource );
			  _dsRegistrationListeners.notify( listener => listener.unregistered( dataSource ) );
			  _life.remove( dataSource );
		 }

		 public virtual NeoStoreDataSource DataSource
		 {
			 get
			 {
				  string activeDatabase = _config.get( GraphDatabaseSettings.active_database );
				  foreach ( NeoStoreDataSource dataSource in _dataSources )
				  {
						if ( activeDatabase.Equals( dataSource.DatabaseLayout.DatabaseName ) )
						{
							 return dataSource;
						}
				  }
				  throw new System.InvalidOperationException( "Default database not found" );
			 }
		 }

		 public override void Init()
		 {
			  _life = new LifeSupport();
			  _dataSources.ForEach( _life.add );
		 }

		 public override void Start()
		 {
			  _life.start();

			  foreach ( Listener listener in _dsRegistrationListeners )
			  {
					try
					{
						 _dataSources.ForEach( listener.registered );
					}
					catch ( Exception )
					{ // OK
					}
			  }
		 }

		 public override void Stop()
		 {
			  _life.stop();
		 }

		 public override void Shutdown()
		 {
			  _life.shutdown();
			  _dataSources.Clear();
		 }

		 public override Kernel Get()
		 {
			  return DataSource.Kernel;
		 }
	}

}
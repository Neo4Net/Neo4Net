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
namespace Neo4Net.Kernel.impl.transaction.state
{

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Neo4Net.Helpers;
	using Kernel = Neo4Net.Kernel.Api.Internal.Kernel;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using LifecycleStatus = Neo4Net.Kernel.Lifecycle.LifecycleStatus;

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
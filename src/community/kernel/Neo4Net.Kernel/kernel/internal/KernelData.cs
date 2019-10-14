using System;
using System.Collections.Concurrent;
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
namespace Neo4Net.Kernel.Internal
{
	using StringUtils = org.apache.commons.lang3.StringUtils;


	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.forced_kernel_id;

	public class KernelData : LifecycleAdapter
	{
		 private static readonly IDictionary<string, KernelData> _instances = new ConcurrentDictionary<string, KernelData>();

		 private readonly string _instanceId;
		 private readonly PageCache _pageCache;
		 private readonly FileSystemAbstraction _fs;
		 private readonly File _storeDir;
		 private readonly Config _configuration;
		 private readonly DataSourceManager _dataSourceManager;

		 public KernelData( FileSystemAbstraction fs, PageCache pageCache, File storeDir, Config configuration, DataSourceManager dataSourceManager )
		 {
			  this._pageCache = pageCache;
			  this._fs = fs;
			  this._storeDir = storeDir;
			  this._configuration = configuration;
			  this._dataSourceManager = dataSourceManager;
			  this._instanceId = NewInstance( this );
		 }

		 public string InstanceId()
		 {
			  return _instanceId;
		 }

		 public virtual Version Version()
		 {
			  return Version.Kernel;
		 }

		 public virtual File StoreDir
		 {
			 get
			 {
				  return _storeDir;
			 }
		 }

		 public virtual Config Config
		 {
			 get
			 {
				  return _configuration;
			 }
		 }

		 public virtual PageCache PageCache
		 {
			 get
			 {
				  return _pageCache;
			 }
		 }

		 public virtual FileSystemAbstraction FilesystemAbstraction
		 {
			 get
			 {
				  return _fs;
			 }
		 }

		 public virtual DataSourceManager DataSourceManager
		 {
			 get
			 {
				  return _dataSourceManager;
			 }
		 }

		 public override void Shutdown()
		 {
			  RemoveInstance( _instanceId );
		 }

		 public override sealed int GetHashCode()
		 {
			  return _instanceId.GetHashCode();
		 }

		 public override sealed bool Equals( object obj )
		 {
			  return obj is KernelData && _instanceId.Equals( ( ( KernelData ) obj )._instanceId );
		 }

		 private static string NewInstance( KernelData instance )
		 {
			 lock ( typeof( KernelData ) )
			 {
				  string instanceId = instance._configuration.get( forced_kernel_id );
				  if ( StringUtils.isEmpty( instanceId ) )
				  {
						for ( int i = 0; i < _instances.Count + 1; i++ )
						{
							 instanceId = Convert.ToString( i );
							 if ( !_instances.ContainsKey( instanceId ) )
							 {
								  break;
							 }
						}
				  }
				  if ( _instances.ContainsKey( instanceId ) )
				  {
						throw new System.InvalidOperationException( "There is already a kernel started with " + forced_kernel_id.name() + "='" + instanceId + "'." );
				  }
				  _instances[instanceId] = instance;
				  return instanceId;
			 }
		 }

		 private static void RemoveInstance( string instanceId )
		 {
			 lock ( typeof( KernelData ) )
			 {
				  if ( _instances.Remove( instanceId ) == null )
				  {
						throw new System.ArgumentException( "No kernel found with instance id " + instanceId );
				  }
			 }
		 }

	}

}
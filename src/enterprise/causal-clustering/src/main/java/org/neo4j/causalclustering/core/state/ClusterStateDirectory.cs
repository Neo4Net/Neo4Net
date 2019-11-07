using System;
using System.Diagnostics;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.core.state
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;

	/// <summary>
	/// This represents the base directory for cluster state and contains
	/// functionality capturing the migration paths.
	/// </summary>
	public class ClusterStateDirectory
	{
		 internal const string CLUSTER_STATE_DIRECTORY_NAME = "cluster-state";

		 private readonly File _stateDir;
		 private readonly File _storeDir;
		 private readonly bool _readOnly;

		 private bool _initialized;

		 public ClusterStateDirectory( File dataDir ) : this( dataDir, null, true )
		 {
		 }

		 public ClusterStateDirectory( File dataDir, bool readOnly ) : this( dataDir, dataDir, readOnly )
		 {
		 }

		 public ClusterStateDirectory( File dataDir, File storeDir, bool readOnly )
		 {
			  this._storeDir = storeDir;
			  this._readOnly = readOnly;
			  this._stateDir = new File( dataDir, CLUSTER_STATE_DIRECTORY_NAME );
		 }

		 /// <summary>
		 /// Returns true if the cluster state base directory exists or
		 /// could be created. This method also takes care of any necessary
		 /// migration.
		 /// 
		 /// It is a requirement to initialize before using the class, unless
		 /// the non-migrating version is used.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ClusterStateDirectory initialize(Neo4Net.io.fs.FileSystemAbstraction fs) throws ClusterStateException
		 public virtual ClusterStateDirectory Initialize( FileSystemAbstraction fs )
		 {
			  Debug.Assert( !_initialized );
			  if ( !_readOnly )
			  {
					MigrateIfNeeded( fs );
			  }
			  EnsureDirectoryExists( fs );
			  _initialized = true;
			  return this;
		 }

		 /// <summary>
		 /// For use by special tooling which does not need the functionality
		 /// of migration or ensuring the directory for cluster state actually
		 /// exists.
		 /// </summary>
		 public static ClusterStateDirectory WithoutInitializing( File dataDir )
		 {
			  ClusterStateDirectory clusterStateDirectory = new ClusterStateDirectory( dataDir );
			  clusterStateDirectory._initialized = true;
			  return clusterStateDirectory;
		 }

		 /// <summary>
		 /// The cluster state directory was previously badly placed under the
		 /// store directory, and this method takes care of the migration path from
		 /// that. It will now reside under the data directory.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void migrateIfNeeded(Neo4Net.io.fs.FileSystemAbstraction fs) throws ClusterStateException
		 private void MigrateIfNeeded( FileSystemAbstraction fs )
		 {
			  File oldStateDir = new File( _storeDir, CLUSTER_STATE_DIRECTORY_NAME );
			  if ( !fs.FileExists( oldStateDir ) || oldStateDir.Equals( _stateDir ) )
			  {
					return;
			  }

			  if ( fs.FileExists( _stateDir ) )
			  {
					throw new ClusterStateException( "Cluster state exists in both old and new locations" );
			  }

			  try
			  {
					fs.MoveToDirectory( oldStateDir, _stateDir.ParentFile );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( "Failed to migrate cluster state directory", e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void ensureDirectoryExists(Neo4Net.io.fs.FileSystemAbstraction fs) throws ClusterStateException
		 private void EnsureDirectoryExists( FileSystemAbstraction fs )
		 {
			  if ( !fs.FileExists( _stateDir ) )
			  {
					if ( _readOnly )
					{
						 throw new ClusterStateException( "Cluster state directory does not exist" );
					}
					else
					{
						 try
						 {
							  fs.Mkdirs( _stateDir );
						 }
						 catch ( IOException e )
						 {
							  throw new ClusterStateException( e );
						 }
					}
			  }
		 }

		 public virtual File Get()
		 {
			  if ( !_initialized )
			  {
					throw new System.InvalidOperationException( "Cluster state has not been initialized" );
			  }
			  return _stateDir;
		 }
	}

}
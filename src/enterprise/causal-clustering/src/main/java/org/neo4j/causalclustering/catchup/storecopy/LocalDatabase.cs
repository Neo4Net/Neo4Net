using System;
using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.catchup.storecopy
{

	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using AvailabilityGuard = Neo4Net.Kernel.availability.AvailabilityGuard;
	using AvailabilityRequirement = Neo4Net.Kernel.availability.AvailabilityRequirement;
	using DatabaseAvailabilityGuard = Neo4Net.Kernel.availability.DatabaseAvailabilityGuard;
	using DescriptiveAvailabilityRequirement = Neo4Net.Kernel.availability.DescriptiveAvailabilityRequirement;
	using TransactionCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionRepresentationCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionRepresentationCommitProcess;
	using TransactionAppender = Neo4Net.Kernel.impl.transaction.log.TransactionAppender;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using DatabaseHealth = Neo4Net.Kernel.Internal.DatabaseHealth;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using StorageEngine = Neo4Net.Storageengine.Api.StorageEngine;

	public class LocalDatabase : Lifecycle
	{
		 private static readonly AvailabilityRequirement _notStopped = new DescriptiveAvailabilityRequirement( "Database is stopped" );
		 private static readonly AvailabilityRequirement _notCopyingStore = new DescriptiveAvailabilityRequirement( "Database is stopped to copy store from another cluster member" );

		 private readonly DatabaseLayout _databaseLayout;

		 private readonly StoreFiles _storeFiles;
		 private readonly DataSourceManager _dataSourceManager;
		 private readonly System.Func<DatabaseHealth> _databaseHealthSupplier;
		 private readonly AvailabilityGuard _availabilityGuard;
		 private readonly Log _log;

		 private volatile StoreId _storeId;
		 private volatile DatabaseHealth _databaseHealth;
		 private volatile AvailabilityRequirement _currentRequirement;

		 private volatile TransactionCommitProcess _localCommit;
		 private readonly LogFiles _logFiles;

		 public LocalDatabase( DatabaseLayout databaseLayout, StoreFiles storeFiles, LogFiles logFiles, DataSourceManager dataSourceManager, System.Func<DatabaseHealth> databaseHealthSupplier, AvailabilityGuard availabilityGuard, LogProvider logProvider )
		 {
			  this._databaseLayout = databaseLayout;
			  this._storeFiles = storeFiles;
			  this._logFiles = logFiles;
			  this._dataSourceManager = dataSourceManager;
			  this._databaseHealthSupplier = databaseHealthSupplier;
			  this._availabilityGuard = availabilityGuard;
			  this._log = logProvider.getLog( this.GetType() );
			  RaiseAvailabilityGuard( _notStopped );
		 }

		 public override void Init()
		 {
			  _dataSourceManager.init();
		 }

		 public override void Start()
		 {
			 lock ( this )
			 {
				  if ( Available )
				  {
						return;
				  }
				  _storeId = ReadStoreIdFromDisk();
				  _log.info( "Starting with storeId: " + _storeId );
      
				  _dataSourceManager.start();
      
				  DropAvailabilityGuard();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
		 public override void Stop()
		 {
			  StopWithRequirement( _notStopped );
		 }

		 /// <summary>
		 /// Stop database to perform a store copy. This will raise <seealso cref="DatabaseAvailabilityGuard"/> with
		 /// a more friendly blocking requirement.
		 /// </summary>
		 /// <exception cref="Throwable"> if any of the components are unable to stop. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stopForStoreCopy() throws Throwable
		 public virtual void StopForStoreCopy()
		 {
			  StopWithRequirement( _notCopyingStore );
		 }

		 public virtual bool Available
		 {
			 get
			 {
				  return _currentRequirement == null;
			 }
		 }

		 public override void Shutdown()
		 {
			  _dataSourceManager.shutdown();
		 }

		 public virtual StoreId StoreId()
		 {
			 lock ( this )
			 {
				  if ( Available )
				  {
						return _storeId;
				  }
				  else
				  {
						return ReadStoreIdFromDisk();
				  }
			 }
		 }

		 private StoreId ReadStoreIdFromDisk()
		 {
			  try
			  {
					return _storeFiles.readStoreId( _databaseLayout );
			  }
			  catch ( IOException e )
			  {
					_log.error( "Failure reading store id", e );
					return null;
			  }
		 }

		 public virtual void Panic( Exception cause )
		 {
			  DatabaseHealth.panic( cause );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <EXCEPTION extends Throwable> void assertHealthy(Class<EXCEPTION> cause) throws EXCEPTION
		 public virtual void AssertHealthy<EXCEPTION>( Type cause ) where EXCEPTION : Exception
		 {
				 cause = typeof( EXCEPTION );
			  DatabaseHealth.assertHealthy( cause );
		 }

		 private DatabaseHealth DatabaseHealth
		 {
			 get
			 {
				  if ( _databaseHealth == null )
				  {
						_databaseHealth = _databaseHealthSupplier.get();
				  }
				  return _databaseHealth;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void delete() throws java.io.IOException
		 public virtual void Delete()
		 {
			  _storeFiles.delete( _databaseLayout.databaseDirectory(), _logFiles );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean isEmpty() throws java.io.IOException
		 public virtual bool Empty
		 {
			 get
			 {
				  ISet<File> filesToLookFor = _databaseLayout.storeFiles();
				  return _storeFiles.isEmpty( _databaseLayout.databaseDirectory(), filesToLookFor );
			 }
		 }

		 public virtual DatabaseLayout DatabaseLayout()
		 {
			  return _databaseLayout;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void replaceWith(java.io.File sourceDir) throws java.io.IOException
		 internal virtual void ReplaceWith( File sourceDir )
		 {
			  _storeFiles.delete( _databaseLayout.databaseDirectory(), _logFiles );
			  _storeFiles.moveTo( sourceDir, _databaseLayout.databaseDirectory(), _logFiles );
		 }

		 public virtual NeoStoreDataSource DataSource()
		 {
			  return _dataSourceManager.DataSource;
		 }

		 /// <summary>
		 /// Called by the DataSourceManager during start.
		 /// </summary>
		 public virtual void RegisterCommitProcessDependencies( TransactionAppender appender, StorageEngine applier )
		 {
			  _localCommit = new TransactionRepresentationCommitProcess( appender, applier );
		 }

		 public virtual TransactionCommitProcess CommitProcess
		 {
			 get
			 {
				  return _localCommit;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private synchronized void stopWithRequirement(org.Neo4Net.kernel.availability.AvailabilityRequirement requirement) throws Throwable
		 private void StopWithRequirement( AvailabilityRequirement requirement )
		 {
			 lock ( this )
			 {
				  _log.info( "Stopping, reason: " + requirement() );
				  RaiseAvailabilityGuard( requirement );
				  _databaseHealth = null;
				  _localCommit = null;
				  _dataSourceManager.stop();
			 }
		 }

		 private void RaiseAvailabilityGuard( AvailabilityRequirement requirement )
		 {
			  // it is possible for the local database to be created and stopped right after that to perform a store copy
			  // in this case we need to impose new requirement and drop the old one
			  _availabilityGuard.require( requirement );
			  if ( _currentRequirement != null )
			  {
					DropAvailabilityGuard();
			  }
			  _currentRequirement = requirement;
		 }

		 private void DropAvailabilityGuard()
		 {
			  _availabilityGuard.fulfill( _currentRequirement );
			  _currentRequirement = null;
		 }
	}

}
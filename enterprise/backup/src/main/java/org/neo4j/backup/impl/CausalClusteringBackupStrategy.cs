/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.backup.impl
{

	using CatchupResult = Org.Neo4j.causalclustering.catchup.CatchupResult;
	using StoreCopyFailedException = Org.Neo4j.causalclustering.catchup.storecopy.StoreCopyFailedException;
	using StoreFiles = Org.Neo4j.causalclustering.catchup.storecopy.StoreFiles;
	using StoreIdDownloadFailedException = Org.Neo4j.causalclustering.catchup.storecopy.StoreIdDownloadFailedException;
	using StoreId = Org.Neo4j.causalclustering.identity.StoreId;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using OptionalHostnamePort = Org.Neo4j.Kernel.impl.util.OptionalHostnamePort;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	internal class CausalClusteringBackupStrategy : LifecycleAdapter, BackupStrategy
	{
		 private readonly BackupDelegator _backupDelegator;
		 private readonly AddressResolver _addressResolver;
		 private readonly Log _log;
		 private readonly StoreFiles _storeFiles;

		 internal CausalClusteringBackupStrategy( BackupDelegator backupDelegator, AddressResolver addressResolver, LogProvider logProvider, StoreFiles storeFiles )
		 {
			  this._backupDelegator = backupDelegator;
			  this._addressResolver = addressResolver;
			  this._log = logProvider.GetLog( typeof( CausalClusteringBackupStrategy ) );
			  this._storeFiles = storeFiles;
		 }

		 public override Fallible<BackupStageOutcome> PerformFullBackup( DatabaseLayout targetDatabaseLayout, Config config, OptionalHostnamePort userProvidedAddress )
		 {
			  AdvertisedSocketAddress fromAddress = _addressResolver.resolveCorrectCCAddress( config, userProvidedAddress );
			  _log.info( "Resolved address for catchup protocol is " + fromAddress );
			  StoreId storeId;
			  try
			  {
					storeId = _backupDelegator.fetchStoreId( fromAddress );
					_log.info( "Remote store id is " + storeId );
			  }
			  catch ( StoreIdDownloadFailedException e )
			  {
					return new Fallible<BackupStageOutcome>( BackupStageOutcome.WrongProtocol, e );
			  }

			  Optional<StoreId> expectedStoreId = ReadLocalStoreId( targetDatabaseLayout );
			  if ( expectedStoreId.Present )
			  {
					return new Fallible<BackupStageOutcome>( BackupStageOutcome.Failure, new StoreIdDownloadFailedException( format( "Cannot perform a full backup onto preexisting backup. Remote store id was %s but local is %s", storeId, expectedStoreId ) ) );
			  }

			  try
			  {
					_backupDelegator.copy( fromAddress, storeId, targetDatabaseLayout );
					return new Fallible<BackupStageOutcome>( BackupStageOutcome.Success, null );
			  }
			  catch ( StoreCopyFailedException e )
			  {
					return new Fallible<BackupStageOutcome>( BackupStageOutcome.Failure, e );
			  }
		 }

		 public override Fallible<BackupStageOutcome> PerformIncrementalBackup( DatabaseLayout databaseLayout, Config config, OptionalHostnamePort userProvidedAddress )
		 {
			  AdvertisedSocketAddress fromAddress = _addressResolver.resolveCorrectCCAddress( config, userProvidedAddress );
			  _log.info( "Resolved address for catchup protocol is " + fromAddress );
			  StoreId storeId;
			  try
			  {
					storeId = _backupDelegator.fetchStoreId( fromAddress );
					_log.info( "Remote store id is " + storeId );
			  }
			  catch ( StoreIdDownloadFailedException e )
			  {
					return new Fallible<BackupStageOutcome>( BackupStageOutcome.WrongProtocol, e );
			  }
			  Optional<StoreId> expectedStoreId = ReadLocalStoreId( databaseLayout );
			  if ( !expectedStoreId.Present || !expectedStoreId.get().Equals(storeId) )
			  {
					return new Fallible<BackupStageOutcome>( BackupStageOutcome.Failure, new StoreIdDownloadFailedException( format( "Remote store id was %s but local is %s", storeId, expectedStoreId ) ) );
			  }
			  return Catchup( fromAddress, storeId, databaseLayout );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {
			  base.Start();
			  _backupDelegator.start();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
		 public override void Stop()
		 {
			  _backupDelegator.stop();
			  base.Stop();
		 }

		 private Optional<StoreId> ReadLocalStoreId( DatabaseLayout databaseLayout )
		 {
			  try
			  {
					return _storeFiles.readStoreId( databaseLayout );
			  }
			  catch ( IOException )
			  {
					return null;
			  }
		 }

		 private Fallible<BackupStageOutcome> Catchup( AdvertisedSocketAddress fromAddress, StoreId storeId, DatabaseLayout databaseLayout )
		 {
			  CatchupResult catchupResult;
			  try
			  {
					catchupResult = _backupDelegator.tryCatchingUp( fromAddress, storeId, databaseLayout );
			  }
			  catch ( StoreCopyFailedException e )
			  {
					return new Fallible<BackupStageOutcome>( BackupStageOutcome.Failure, e );
			  }
			  if ( catchupResult == CatchupResult.SUCCESS_END_OF_STREAM )
			  {
					return new Fallible<BackupStageOutcome>( BackupStageOutcome.Success, null );
			  }
			  return new Fallible<BackupStageOutcome>( BackupStageOutcome.Failure, new StoreCopyFailedException( "End state of catchup was not a successful end of stream" ) );
		 }
	}

}
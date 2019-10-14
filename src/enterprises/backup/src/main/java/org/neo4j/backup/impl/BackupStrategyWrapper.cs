using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.backup.impl
{

	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using OptionalHostnamePort = Neo4Net.Kernel.impl.util.OptionalHostnamePort;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// Individual backup strategies can perform incremental backups and full backups. The logic of how and when to perform full/incremental is identical.
	/// This class describes the behaviour of a single strategy and is used to wrap an interface providing incremental/full backup functionality
	/// </summary>
	internal class BackupStrategyWrapper
	{
		 private readonly BackupStrategy _backupStrategy;
		 private readonly BackupCopyService _backupCopyService;
		 private readonly BackupRecoveryService _backupRecoveryService;
		 private readonly Log _log;

		 private readonly PageCache _pageCache;
		 private readonly Config _config;

		 internal BackupStrategyWrapper( BackupStrategy backupStrategy, BackupCopyService backupCopyService, PageCache pageCache, Config config, BackupRecoveryService backupRecoveryService, LogProvider logProvider )
		 {
			  this._backupStrategy = backupStrategy;
			  this._backupCopyService = backupCopyService;
			  this._pageCache = pageCache;
			  this._config = config;
			  this._backupRecoveryService = backupRecoveryService;
			  this._log = logProvider.GetLog( typeof( BackupStrategyWrapper ) );
		 }

		 /// <summary>
		 /// Try to do a backup using the given strategy (ex. BackupProtocol). This covers all stages (starting with incremental and falling back to a a full backup).
		 /// The end result of this method will either be a successful backup or any other return type with the reason why the backup wasn't successful
		 /// </summary>
		 /// <param name="onlineBackupContext"> the command line arguments, configuration, flags </param>
		 /// <returns> the ultimate outcome of trying to do a backup with the given strategy </returns>
		 internal virtual Fallible<BackupStrategyOutcome> DoBackup( OnlineBackupContext onlineBackupContext )
		 {
			  LifeSupport lifeSupport = new LifeSupport();
			  lifeSupport.Add( _backupStrategy );
			  lifeSupport.Start();
			  Fallible<BackupStrategyOutcome> state = PerformBackupWithoutLifecycle( onlineBackupContext );
			  lifeSupport.Shutdown();
			  return state;
		 }

		 private Fallible<BackupStrategyOutcome> PerformBackupWithoutLifecycle( OnlineBackupContext onlineBackupContext )
		 {
			  Path backupLocation = onlineBackupContext.ResolvedLocationFromName;
			  OptionalHostnamePort userSpecifiedAddress = onlineBackupContext.RequiredArguments.Address;
			  _log.debug( "User specified address is %s:%s", userSpecifiedAddress.Hostname.ToString(), userSpecifiedAddress.Port.ToString() );
			  Config config = onlineBackupContext.Config;

			  bool previousBackupExists = _backupCopyService.backupExists( DatabaseLayout.of( backupLocation.toFile() ) );
			  if ( previousBackupExists )
			  {
					_log.info( "Previous backup found, trying incremental backup." );
					Fallible<BackupStageOutcome> state = _backupStrategy.performIncrementalBackup( DatabaseLayout.of( backupLocation.toFile() ), config, userSpecifiedAddress );
					bool fullBackupWontWork = BackupStageOutcome.WrongProtocol.Equals( state.State );
					bool incrementalWasSuccessful = BackupStageOutcome.Success.Equals( state.State );
					if ( incrementalWasSuccessful )
					{
						 _backupRecoveryService.recoverWithDatabase( backupLocation, _pageCache, config );
					}

					if ( fullBackupWontWork || incrementalWasSuccessful )
					{
						 ClearIdFiles( backupLocation );
						 return DescribeOutcome( state );
					}
					if ( !onlineBackupContext.RequiredArguments.FallbackToFull )
					{
						 return DescribeOutcome( state );
					}
			  }
			  if ( onlineBackupContext.RequiredArguments.FallbackToFull )
			  {
					if ( !previousBackupExists )
					{
						 _log.info( "Previous backup not found, a new full backup will be performed." );
					}
					return DescribeOutcome( FullBackupWithTemporaryFolderResolutions( onlineBackupContext ) );
			  }
			  return new Fallible<BackupStrategyOutcome>( BackupStrategyOutcome.IncorrectStrategy, null );
		 }

		 private void ClearIdFiles( Path backupLocation )
		 {
			  try
			  {
					_backupCopyService.clearIdFiles( backupLocation );
			  }
			  catch ( IOException e )
			  {
					_log.warn( "Failed to delete some or all id files.", e );
			  }
		 }

		 /// <summary>
		 /// This will perform a full backup with some directory renaming if necessary.
		 /// <para>
		 /// If there is no existing backup, then no renaming will occur.
		 /// Otherwise the full backup will be done into a temporary directory and renaming
		 /// will occur if everything was successful.
		 /// </para>
		 /// </summary>
		 /// <param name="onlineBackupContext"> command line arguments, config etc. </param>
		 /// <returns> outcome of full backup </returns>
		 private Fallible<BackupStageOutcome> FullBackupWithTemporaryFolderResolutions( OnlineBackupContext onlineBackupContext )
		 {
			  Path userSpecifiedBackupLocation = onlineBackupContext.ResolvedLocationFromName;
			  Path temporaryFullBackupLocation = _backupCopyService.findAnAvailableLocationForNewFullBackup( userSpecifiedBackupLocation );

			  OptionalHostnamePort address = onlineBackupContext.RequiredArguments.Address;
			  Fallible<BackupStageOutcome> state = _backupStrategy.performFullBackup( DatabaseLayout.of( temporaryFullBackupLocation.toFile() ), _config, address );

			  // NOTE temporaryFullBackupLocation can be equal to desired
			  bool backupWasMadeToATemporaryLocation = !userSpecifiedBackupLocation.Equals( temporaryFullBackupLocation );

			  if ( BackupStageOutcome.Success.Equals( state.State ) )
			  {
					_backupRecoveryService.recoverWithDatabase( temporaryFullBackupLocation, _pageCache, _config );
					if ( backupWasMadeToATemporaryLocation )
					{
						 try
						 {
							  RenameTemporaryBackupToExpected( temporaryFullBackupLocation, userSpecifiedBackupLocation );
						 }
						 catch ( IOException e )
						 {
							  return new Fallible<BackupStageOutcome>( BackupStageOutcome.UnrecoverableFailure, e );
						 }
					}
					ClearIdFiles( userSpecifiedBackupLocation );
			  }
			  return state;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void renameTemporaryBackupToExpected(java.nio.file.Path temporaryFullBackupLocation, java.nio.file.Path userSpecifiedBackupLocation) throws java.io.IOException
		 private void RenameTemporaryBackupToExpected( Path temporaryFullBackupLocation, Path userSpecifiedBackupLocation )
		 {
			  Path newBackupLocationForPreExistingBackup = _backupCopyService.findNewBackupLocationForBrokenExisting( userSpecifiedBackupLocation );
			  _backupCopyService.moveBackupLocation( userSpecifiedBackupLocation, newBackupLocationForPreExistingBackup );
			  _backupCopyService.moveBackupLocation( temporaryFullBackupLocation, userSpecifiedBackupLocation );
		 }

		 private static Fallible<BackupStrategyOutcome> DescribeOutcome( Fallible<BackupStageOutcome> strategyStageOutcome )
		 {
			  BackupStageOutcome stageOutcome = strategyStageOutcome.State;
			  if ( stageOutcome == BackupStageOutcome.Success )
			  {
					return new Fallible<BackupStrategyOutcome>( BackupStrategyOutcome.Success, null );
			  }
			  if ( stageOutcome == BackupStageOutcome.WrongProtocol )
			  {
					return new Fallible<BackupStrategyOutcome>( BackupStrategyOutcome.IncorrectStrategy, strategyStageOutcome.Cause.orElse( null ) );
			  }
			  if ( stageOutcome == BackupStageOutcome.Failure )
			  {
					return new Fallible<BackupStrategyOutcome>( BackupStrategyOutcome.CorrectStrategyFailed, strategyStageOutcome.Cause.orElse( null ) );
			  }
			  if ( stageOutcome == BackupStageOutcome.UnrecoverableFailure )
			  {
					return new Fallible<BackupStrategyOutcome>( BackupStrategyOutcome.AbsoluteFailure, strategyStageOutcome.Cause.orElse( null ) );
			  }
			  throw new Exception( "Not all enums covered: " + stageOutcome );
		 }
	}

}
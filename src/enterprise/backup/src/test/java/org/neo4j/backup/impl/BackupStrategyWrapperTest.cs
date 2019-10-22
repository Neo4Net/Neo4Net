using System;

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
namespace Neo4Net.backup.impl
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;
	using Mockito = org.mockito.Mockito;


	using OutsideWorld = Neo4Net.CommandLine.Admin.OutsideWorld;
	using ConsistencyFlags = Neo4Net.Consistency.checking.full.ConsistencyFlags;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using OptionalHostnamePort = Neo4Net.Kernel.impl.util.OptionalHostnamePort;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class BackupStrategyWrapperTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory testDirectory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private readonly BackupStrategy _backupStrategyImplementation = mock( typeof( BackupStrategy ) );
		 private readonly OutsideWorld _outsideWorld = mock( typeof( OutsideWorld ) );
		 private readonly BackupCopyService _backupCopyService = mock( typeof( BackupCopyService ) );

		 private BackupStrategyWrapper _subject;

		 private OnlineBackupContext _onlineBackupContext;

		 private readonly FileSystemAbstraction _fileSystemAbstraction = mock( typeof( FileSystemAbstraction ) );
		 private DatabaseLayout _desiredBackupLayout;
		 private Path _reportDir;
		 private Path _availableFreshBackupLocation;
		 private OnlineBackupRequiredArguments _requiredArguments;
		 private Config _config = mock( typeof( Config ) );
		 private readonly OptionalHostnamePort _userProvidedAddress = new OptionalHostnamePort( ( string ) null, null, null );
		 private readonly Fallible<BackupStageOutcome> _success = new Fallible<BackupStageOutcome>( BackupStageOutcome.Success, null );
		 private readonly Fallible<BackupStageOutcome> _failure = new Fallible<BackupStageOutcome>( BackupStageOutcome.Failure, null );
		 private readonly PageCache _pageCache = mock( typeof( PageCache ) );
		 private readonly BackupRecoveryService _backupRecoveryService = mock( typeof( BackupRecoveryService ) );
		 private readonly LogProvider _logProvider = mock( typeof( LogProvider ) );
		 private readonly Log _log = mock( typeof( Log ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _desiredBackupLayout = TestDirectory.databaseLayout( "desiredBackupLayout" );
			  _reportDir = TestDirectory.directory( "reportDir" ).toPath();
			  _availableFreshBackupLocation = TestDirectory.directory( "availableFreshBackupLocation" ).toPath();
			  Path availableOldBackupLocation = TestDirectory.directory( "availableOldBackupLocation" ).toPath();

			  when( _outsideWorld.fileSystem() ).thenReturn(_fileSystemAbstraction);
			  when( _backupCopyService.findAnAvailableLocationForNewFullBackup( any() ) ).thenReturn(_availableFreshBackupLocation);
			  when( _backupCopyService.findNewBackupLocationForBrokenExisting( any() ) ).thenReturn(availableOldBackupLocation);
			  when( _backupStrategyImplementation.performFullBackup( any(), any(), any() ) ).thenReturn(_success);
			  when( _logProvider.getLog( ( Type ) any() ) ).thenReturn(_log);

			  _subject = new BackupStrategyWrapper( _backupStrategyImplementation, _backupCopyService, _pageCache, _config, _backupRecoveryService, _logProvider );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lifecycleIsRun() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LifecycleIsRun()
		 {
			  // given
			  _onlineBackupContext = new OnlineBackupContext( RequiredArguments( true ), _config, ConsistencyFlags() );

			  // when
			  _subject.doBackup( _onlineBackupContext );

			  // then
			  verify( _backupStrategyImplementation ).init();
			  verify( _backupStrategyImplementation ).start();
			  verify( _backupStrategyImplementation ).stop();
			  verify( _backupStrategyImplementation ).shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fullBackupIsPerformedWhenNoOtherBackupExists()
		 public virtual void FullBackupIsPerformedWhenNoOtherBackupExists()
		 {
			  // given
			  _requiredArguments = _requiredArguments( true );
			  _onlineBackupContext = new OnlineBackupContext( _requiredArguments, _config, ConsistencyFlags() );

			  // and
			  when( _backupCopyService.backupExists( any() ) ).thenReturn(false);

			  // when
			  _subject.doBackup( _onlineBackupContext );

			  // then
			  verify( _backupStrategyImplementation ).performFullBackup( any(), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fullBackupIsIgnoredIfNoOtherBackupAndNotFallback()
		 public virtual void FullBackupIsIgnoredIfNoOtherBackupAndNotFallback()
		 {
			  // given there is an existing backup
			  when( _backupCopyService.backupExists( any() ) ).thenReturn(false);

			  // and we don't want to fallback to full backups
			  _requiredArguments = _requiredArguments( false );
			  _onlineBackupContext = new OnlineBackupContext( _requiredArguments, _config, ConsistencyFlags() );

			  // and incremental backup fails because it's a different store
			  when( _backupStrategyImplementation.performIncrementalBackup( any(), any(), any() ) ).thenReturn(_failure);

			  // when
			  _subject.doBackup( _onlineBackupContext );

			  // then full backup wasnt performed
			  verify( _backupStrategyImplementation, never() ).performFullBackup(any(), any(), any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fullBackupIsNotPerformedWhenAnIncrementalBackupIsSuccessful()
		 public virtual void FullBackupIsNotPerformedWhenAnIncrementalBackupIsSuccessful()
		 {
			  // given
			  when( _backupCopyService.backupExists( any() ) ).thenReturn(true);
			  when( _backupStrategyImplementation.performIncrementalBackup( any(), any(), any() ) ).thenReturn(_success);

			  // and
			  _requiredArguments = _requiredArguments( true );
			  _onlineBackupContext = new OnlineBackupContext( _requiredArguments, _config, ConsistencyFlags() );

			  // when
			  _subject.doBackup( _onlineBackupContext );

			  // then
			  verify( _backupStrategyImplementation, never() ).performFullBackup(_desiredBackupLayout, _config, _userProvidedAddress);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failedIncrementalFallsBackToFullWhenOptionSet()
		 public virtual void FailedIncrementalFallsBackToFullWhenOptionSet()
		 {
			  // given conditions for incremental exist
			  when( _backupCopyService.backupExists( any() ) ).thenReturn(true);
			  _requiredArguments = _requiredArguments( true );
			  _onlineBackupContext = new OnlineBackupContext( _requiredArguments, _config, ConsistencyFlags() );

			  // and incremental fails
			  when( _backupStrategyImplementation.performIncrementalBackup( any(), any(), any() ) ).thenReturn(new Fallible<>(BackupStageOutcome.Failure, null));

			  // when
			  _subject.doBackup( _onlineBackupContext );

			  // then
			  verify( _backupStrategyImplementation ).performFullBackup( any(), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fallbackDoesNotHappenIfNotSpecified()
		 public virtual void FallbackDoesNotHappenIfNotSpecified()
		 {
			  // given
			  when( _backupCopyService.backupExists( any() ) ).thenReturn(true);
			  when( _backupStrategyImplementation.performIncrementalBackup( any(), any(), any() ) ).thenReturn(new Fallible<>(BackupStageOutcome.Failure, null));

			  // and
			  _requiredArguments = _requiredArguments( false );
			  _onlineBackupContext = new OnlineBackupContext( _requiredArguments, _config, ConsistencyFlags() );

			  // when
			  _subject.doBackup( _onlineBackupContext );

			  // then
			  verify( _backupStrategyImplementation, never() ).performFullBackup(any(), any(), any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failedBackupsDontMoveExisting() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailedBackupsDontMoveExisting()
		 {
			  // given a backup already exists
			  when( _backupCopyService.backupExists( any() ) ).thenReturn(true);

			  // and fallback to full is true
			  _requiredArguments = _requiredArguments( true );
			  _onlineBackupContext = new OnlineBackupContext( _requiredArguments, _config, ConsistencyFlags() );

			  // and an incremental backup fails
			  when( _backupStrategyImplementation.performIncrementalBackup( any(), any(), any() ) ).thenReturn(new Fallible<>(BackupStageOutcome.Failure, null));

			  // and full backup fails
			  when( _backupStrategyImplementation.performFullBackup( any(), any(), any() ) ).thenReturn(new Fallible<>(BackupStageOutcome.Failure, null));

			  // when backup is performed
			  _subject.doBackup( _onlineBackupContext );

			  // then existing backup hasn't moved
			  verify( _backupStrategyImplementation ).performFullBackup( any(), any(), any() );
			  verify( _backupCopyService, never() ).moveBackupLocation(any(), any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void successfulFullBackupsMoveExistingBackup() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SuccessfulFullBackupsMoveExistingBackup()
		 {
			  // given backup exists
			  _desiredBackupLayout = TestDirectory.databaseLayout( "some-preexisting-backup" );
			  when( _backupCopyService.backupExists( _desiredBackupLayout ) ).thenReturn( true );

			  // and fallback to full flag has been set
			  _requiredArguments = _requiredArguments( true );
			  _onlineBackupContext = new OnlineBackupContext( _requiredArguments, _config, ConsistencyFlags() );

			  // and a new location for the existing backup is found
			  Path newLocationForExistingBackup = TestDirectory.directory( "new-backup-location" ).toPath();
			  when( _backupCopyService.findNewBackupLocationForBrokenExisting( _desiredBackupLayout.databaseDirectory().toPath() ) ).thenReturn(newLocationForExistingBackup);

			  // and there is a generated location for where to store a new full backup so the original is not destroyed
			  Path temporaryFullBackupLocation = TestDirectory.directory( "temporary-full-backup" ).toPath();
			  when( _backupCopyService.findAnAvailableLocationForNewFullBackup( _desiredBackupLayout.databaseDirectory().toPath() ) ).thenReturn(temporaryFullBackupLocation);

			  // and incremental fails
			  when( _backupStrategyImplementation.performIncrementalBackup( any(), any(), any() ) ).thenReturn(new Fallible<>(BackupStageOutcome.Failure, null));

			  // and full passes
			  when( _backupStrategyImplementation.performFullBackup( any(), any(), any() ) ).thenReturn(new Fallible<>(BackupStageOutcome.Success, null));

			  // when
			  Fallible<BackupStrategyOutcome> state = _subject.doBackup( _onlineBackupContext );

			  // then original existing backup is moved to err directory
			  verify( _backupCopyService ).moveBackupLocation( eq( _desiredBackupLayout.databaseDirectory().toPath() ), eq(newLocationForExistingBackup) );

			  // and new successful backup is renamed to original expected name
			  verify( _backupCopyService ).moveBackupLocation( eq( temporaryFullBackupLocation ), eq( _desiredBackupLayout.databaseDirectory().toPath() ) );

			  // and backup was successful
			  assertEquals( BackupStrategyOutcome.Success, state.State );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failureDuringMoveCausesAbsoluteFailure() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailureDuringMoveCausesAbsoluteFailure()
		 {
			  // given moves fail
			  doThrow( typeof( IOException ) ).when( _backupCopyService ).moveBackupLocation( any(), any() );

			  // and fallback to full
			  _requiredArguments = _requiredArguments( true );
			  _onlineBackupContext = new OnlineBackupContext( _requiredArguments, _config, ConsistencyFlags() );

			  // and backup exists
			  when( _backupCopyService.backupExists( any() ) ).thenReturn(true);

			  // and incremental fails
			  when( _backupStrategyImplementation.performIncrementalBackup( any(), any(), any() ) ).thenReturn(new Fallible<>(BackupStageOutcome.Failure, null));

			  // and full passes
			  when( _backupStrategyImplementation.performFullBackup( any(), any(), any() ) ).thenReturn(new Fallible<>(BackupStageOutcome.Success, null));

			  // when
			  Fallible<BackupStrategyOutcome> state = _subject.doBackup( _onlineBackupContext );

			  // then result was catastrophic and contained reason
			  assertEquals( BackupStrategyOutcome.AbsoluteFailure, state.State );
			  assertEquals( typeof( IOException ), state.Cause.get().GetType() );

			  // and full backup was definitely performed
			  verify( _backupStrategyImplementation ).performFullBackup( any(), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void performingFullBackupInvokesRecovery()
		 public virtual void PerformingFullBackupInvokesRecovery()
		 {
			  // given full backup flag is set
			  _requiredArguments = _requiredArguments( true );
			  _onlineBackupContext = new OnlineBackupContext( _requiredArguments, _config, ConsistencyFlags() );

			  // when
			  _subject.doBackup( _onlineBackupContext );

			  // then
			  verify( _backupRecoveryService ).recoverWithDatabase( any(), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void performingIncrementalBackupInvokesRecovery()
		 public virtual void PerformingIncrementalBackupInvokesRecovery()
		 {
			  // given backup exists
			  when( _backupCopyService.backupExists( any() ) ).thenReturn(true);
			  _requiredArguments = _requiredArguments( true );
			  _onlineBackupContext = new OnlineBackupContext( _requiredArguments, _config, ConsistencyFlags() );

			  // and incremental backups are successful
			  when( _backupStrategyImplementation.performIncrementalBackup( any(), any(), any() ) ).thenReturn(_success);

			  // when
			  _subject.doBackup( _onlineBackupContext );

			  // then
			  verify( _backupRecoveryService ).recoverWithDatabase( any(), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void successfulBackupsAreRecovered()
		 public virtual void SuccessfulBackupsAreRecovered()
		 {
			  // given
			  FallbackToFullPasses();
			  _onlineBackupContext = new OnlineBackupContext( _requiredArguments, _config, ConsistencyFlags() );

			  // when
			  _subject.doBackup( _onlineBackupContext );

			  // then
			  verify( _backupRecoveryService ).recoverWithDatabase( any(), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unsuccessfulBackupsAreNotRecovered()
		 public virtual void UnsuccessfulBackupsAreNotRecovered()
		 {
			  // given
			  BothBackupsFail();
			  _onlineBackupContext = new OnlineBackupContext( _requiredArguments, _config, ConsistencyFlags() );

			  // when
			  _subject.doBackup( _onlineBackupContext );

			  // then
			  verify( _backupRecoveryService, never() ).recoverWithDatabase(any(), any(), any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void successfulFullBackupsAreRecoveredEvenIfNoBackupExisted()
		 public virtual void SuccessfulFullBackupsAreRecoveredEvenIfNoBackupExisted()
		 {
			  // given a backup exists
			  when( _backupCopyService.backupExists( _desiredBackupLayout ) ).thenReturn( false );
			  when( _backupCopyService.findAnAvailableLocationForNewFullBackup( _desiredBackupLayout.databaseDirectory().toPath() ) ).thenReturn(_desiredBackupLayout.databaseDirectory().toPath());

			  // and
			  FallbackToFullPasses();
			  _onlineBackupContext = new OnlineBackupContext( _requiredArguments, _config, ConsistencyFlags() );

			  // when
			  _subject.doBackup( _onlineBackupContext );

			  // then
			  verify( _backupRecoveryService ).recoverWithDatabase( any(), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recoveryIsPerformedBeforeRename() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RecoveryIsPerformedBeforeRename()
		 {
			  // given
			  FallbackToFullPasses();
			  _onlineBackupContext = new OnlineBackupContext( _requiredArguments, _config, ConsistencyFlags() );

			  // when
			  _subject.doBackup( _onlineBackupContext );

			  // then
			  InOrder recoveryBeforeRenameOrder = Mockito.inOrder( _backupRecoveryService, _backupCopyService );
			  recoveryBeforeRenameOrder.verify( _backupRecoveryService ).recoverWithDatabase( eq( _availableFreshBackupLocation ), any(), any() );
			  recoveryBeforeRenameOrder.verify( _backupCopyService ).moveBackupLocation( eq( _availableFreshBackupLocation ), eq( _desiredBackupLayout.databaseDirectory().toPath() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void logsAreClearedAfterIncrementalBackup() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LogsAreClearedAfterIncrementalBackup()
		 {
			  // given backup exists
			  when( _backupCopyService.backupExists( any() ) ).thenReturn(true);

			  // and
			  IncrementalBackupIsSuccessful( true );

			  // and
			  _requiredArguments = _requiredArguments( false );
			  _onlineBackupContext = new OnlineBackupContext( _requiredArguments, _config, ConsistencyFlags() );

			  // when
			  _subject.doBackup( _onlineBackupContext );

			  // then
			  verify( _backupCopyService ).clearIdFiles( any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void logsAreNotClearedWhenIncrementalNotSuccessful() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LogsAreNotClearedWhenIncrementalNotSuccessful()
		 {
			  // given backup exists
			  when( _backupCopyService.backupExists( any() ) ).thenReturn(true);

			  // and incremental is not successful
			  IncrementalBackupIsSuccessful( false );

			  // and
			  _requiredArguments = _requiredArguments( false );
			  _onlineBackupContext = new OnlineBackupContext( _requiredArguments, _config, ConsistencyFlags() );

			  // when backups are performed
			  _subject.doBackup( _onlineBackupContext );

			  // then do not
			  verify( _backupCopyService, never() ).clearIdFiles(any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void logsAreClearedWhenFullBackupIsSuccessful() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LogsAreClearedWhenFullBackupIsSuccessful()
		 {
			  // given a backup doesn't exist
			  when( _backupCopyService.backupExists( any() ) ).thenReturn(false);

			  // and
			  FallbackToFullPasses();
			  _onlineBackupContext = new OnlineBackupContext( _requiredArguments, _config, ConsistencyFlags() );

			  // when
			  _subject.doBackup( _onlineBackupContext );

			  // then
			  verify( _backupCopyService ).clearIdFiles( any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void logsAreNotClearedWhenFullBackupIsNotSuccessful() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LogsAreNotClearedWhenFullBackupIsNotSuccessful()
		 {
			  // given a backup doesn't exist
			  when( _backupCopyService.backupExists( any() ) ).thenReturn(false);

			  // and
			  BothBackupsFail();
			  _onlineBackupContext = new OnlineBackupContext( _requiredArguments, _config, ConsistencyFlags() );

			  // when
			  _subject.doBackup( _onlineBackupContext );

			  // then
			  verify( _backupCopyService, never() ).clearIdFiles(any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void logsWhenIncrementalFailsAndFallbackToFull()
		 public virtual void LogsWhenIncrementalFailsAndFallbackToFull()
		 {
			  // given backup exists
			  when( _backupCopyService.backupExists( any() ) ).thenReturn(false);

			  // and fallback to full
			  FallbackToFullPasses();
			  _onlineBackupContext = new OnlineBackupContext( _requiredArguments, _config, ConsistencyFlags() );

			  // when
			  _subject.doBackup( _onlineBackupContext );

			  // then
			  verify( _log ).info( "Previous backup not found, a new full backup will be performed." );
		 }

		 // ====================================================================================================

		 private void IncrementalBackupIsSuccessful( bool isSuccessful )
		 {
			  if ( isSuccessful )
			  {
					when( _backupStrategyImplementation.performIncrementalBackup( any(), any(), any() ) ).thenReturn(new Fallible<>(BackupStageOutcome.Success, null));
					return;
			  }
			  when( _backupStrategyImplementation.performIncrementalBackup( any(), any(), any() ) ).thenReturn(new Fallible<>(BackupStageOutcome.Failure, null));
		 }

		 private void BothBackupsFail()
		 {
			  _requiredArguments = _requiredArguments( true );
			  when( _backupStrategyImplementation.performIncrementalBackup( any(), any(), any() ) ).thenReturn(_failure);
			  when( _backupStrategyImplementation.performFullBackup( any(), any(), any() ) ).thenReturn(_failure);
		 }

		 private void FallbackToFullPasses()
		 {
			  _requiredArguments = _requiredArguments( true );
			  when( _backupStrategyImplementation.performIncrementalBackup( any(), any(), any() ) ).thenReturn(_failure);
			  when( _backupStrategyImplementation.performFullBackup( any(), any(), any() ) ).thenReturn(_success);
		 }

		 private OnlineBackupRequiredArguments RequiredArguments( bool fallbackToFull )
		 {
			  File databaseDirectory = _desiredBackupLayout.databaseDirectory();
			  return new OnlineBackupRequiredArguments( _userProvidedAddress, _desiredBackupLayout.StoreLayout.storeDirectory().toPath(), databaseDirectory.Name, SelectedBackupProtocol.Any, fallbackToFull, true, 1000, _reportDir );
		 }

		 private static ConsistencyFlags ConsistencyFlags()
		 {
			  return new ConsistencyFlags( true, true, true, true, true );
		 }
	}

}
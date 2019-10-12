using System;

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
namespace Neo4Net.backup.impl
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using CommandFailed = Neo4Net.Commandline.Admin.CommandFailed;
	using OutsideWorld = Neo4Net.Commandline.Admin.OutsideWorld;
	using ConsistencyCheckService = Neo4Net.Consistency.ConsistencyCheckService;
	using ConsistencyCheckIncompleteException = Neo4Net.Consistency.checking.full.ConsistencyCheckIncompleteException;
	using ConsistencyFlags = Neo4Net.Consistency.checking.full.ConsistencyFlags;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.backup.ExceptionMatchers.exceptionContainsSuppressedThrowable;

	public class BackupStrategyCoordinatorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public readonly ExpectedException ExpectedException = ExpectedException.none();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

		 // dependencies
		 private readonly ConsistencyCheckService _consistencyCheckService = mock( typeof( ConsistencyCheckService ) );
		 private readonly OutsideWorld _outsideWorld = mock( typeof( OutsideWorld ) );
		 private readonly FileSystemAbstraction _fileSystem = mock( typeof( FileSystemAbstraction ) );
		 private readonly LogProvider _logProvider = mock( typeof( LogProvider ) );
		 private readonly BackupStrategyWrapper _firstStrategy = mock( typeof( BackupStrategyWrapper ) );
		 private readonly BackupStrategyWrapper _secondStrategy = mock( typeof( BackupStrategyWrapper ) );

		 private BackupStrategyCoordinator _subject;

		 // test method parameter mocks
		 private readonly OnlineBackupContext _onlineBackupContext = mock( typeof( OnlineBackupContext ) );
		 private readonly OnlineBackupRequiredArguments _requiredArguments = mock( typeof( OnlineBackupRequiredArguments ) );

		 // mock returns
		 private readonly ProgressMonitorFactory _progressMonitorFactory = mock( typeof( ProgressMonitorFactory ) );
		 private readonly Path _reportDir = mock( typeof( Path ) );
		 private readonly ConsistencyCheckService.Result _consistencyCheckResult = mock( typeof( ConsistencyCheckService.Result ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  when( _reportDir.toFile() ).thenReturn(TestDirectory.databaseLayout().databaseDirectory());
			  when( _outsideWorld.fileSystem() ).thenReturn(_fileSystem);
			  when( _onlineBackupContext.RequiredArguments ).thenReturn( _requiredArguments );
			  when( _onlineBackupContext.ResolvedLocationFromName ).thenReturn( _reportDir );
			  when( _requiredArguments.ReportDir ).thenReturn( _reportDir );
			  _subject = new BackupStrategyCoordinator( _consistencyCheckService, _outsideWorld, _logProvider, _progressMonitorFactory, Arrays.asList( _firstStrategy, _secondStrategy ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void backupIsValidIfAnySingleStrategyPasses_secondFails() throws org.neo4j.commandline.admin.CommandFailed
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BackupIsValidIfAnySingleStrategyPassesSecondFails()
		 {
			  // given
			  when( _firstStrategy.doBackup( any() ) ).thenReturn(new Fallible<>(BackupStrategyOutcome.Success, null));
			  when( _secondStrategy.doBackup( any() ) ).thenReturn(new Fallible<>(BackupStrategyOutcome.IncorrectStrategy, null));

			  // when
			  _subject.performBackup( _onlineBackupContext );

			  // then no exception
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void backupIsValidIfAnySingleStrategyPasses_firstFails() throws org.neo4j.commandline.admin.CommandFailed
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BackupIsValidIfAnySingleStrategyPassesFirstFails()
		 {
			  // given
			  when( _firstStrategy.doBackup( any() ) ).thenReturn(new Fallible<>(BackupStrategyOutcome.IncorrectStrategy, null));
			  when( _secondStrategy.doBackup( any() ) ).thenReturn(new Fallible<>(BackupStrategyOutcome.Success, null));

			  // when
			  _subject.performBackup( _onlineBackupContext );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void backupIsInvalidIfTheCorrectMethodFailed_firstFails() throws org.neo4j.commandline.admin.CommandFailed
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BackupIsInvalidIfTheCorrectMethodFailedFirstFails()
		 {
			  // given
			  when( _firstStrategy.doBackup( any() ) ).thenReturn(new Fallible<>(BackupStrategyOutcome.CorrectStrategyFailed, null));
			  when( _secondStrategy.doBackup( any() ) ).thenReturn(new Fallible<>(BackupStrategyOutcome.IncorrectStrategy, null));

			  // then
			  ExpectedException.expect( typeof( CommandFailed ) );
			  ExpectedException.expectMessage( containsString( "Execution of backup failed" ) );

			  // when
			  _subject.performBackup( _onlineBackupContext );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void backupIsInvalidIfTheCorrectMethodFailed_secondFails() throws org.neo4j.commandline.admin.CommandFailed
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BackupIsInvalidIfTheCorrectMethodFailedSecondFails()
		 {
			  // given
			  when( _firstStrategy.doBackup( any() ) ).thenReturn(new Fallible<>(BackupStrategyOutcome.IncorrectStrategy, null));
			  when( _secondStrategy.doBackup( any() ) ).thenReturn(new Fallible<>(BackupStrategyOutcome.CorrectStrategyFailed, null));

			  // then
			  ExpectedException.expect( typeof( CommandFailed ) );
			  ExpectedException.expectMessage( containsString( "Execution of backup failed" ) );

			  // when
			  _subject.performBackup( _onlineBackupContext );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void backupFailsIfAllStrategiesAreIncorrect() throws org.neo4j.commandline.admin.CommandFailed
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BackupFailsIfAllStrategiesAreIncorrect()
		 {
			  // given
			  when( _firstStrategy.doBackup( any() ) ).thenReturn(new Fallible<>(BackupStrategyOutcome.IncorrectStrategy, null));
			  when( _secondStrategy.doBackup( any() ) ).thenReturn(new Fallible<>(BackupStrategyOutcome.IncorrectStrategy, null));

			  // then
			  ExpectedException.expect( typeof( CommandFailed ) );
			  ExpectedException.expectMessage( equalTo( "Failed to run a backup using the available strategies." ) );

			  // when
			  _subject.performBackup( _onlineBackupContext );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void consistencyCheckIsRunIfSpecified() throws org.neo4j.commandline.admin.CommandFailed, org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConsistencyCheckIsRunIfSpecified()
		 {
			  // given
			  AnyStrategyPasses();
			  when( _requiredArguments.DoConsistencyCheck ).thenReturn( true );
			  when( _consistencyCheckService.runFullConsistencyCheck( any(), any(), eq(_progressMonitorFactory), any(typeof(LogProvider)), any(), eq(false), any(), any() ) ).thenReturn(_consistencyCheckResult);
			  when( _consistencyCheckResult.Successful ).thenReturn( true );

			  // when
			  _subject.performBackup( _onlineBackupContext );

			  // then
			  verify( _consistencyCheckService ).runFullConsistencyCheck( any(), any(), any(), any(), any(), eq(false), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void consistencyCheckIsNotRunIfNotSpecified() throws org.neo4j.commandline.admin.CommandFailed, org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConsistencyCheckIsNotRunIfNotSpecified()
		 {
			  // given
			  AnyStrategyPasses();
			  when( _requiredArguments.DoConsistencyCheck ).thenReturn( false );

			  // when
			  _subject.performBackup( _onlineBackupContext );

			  // then
			  verify( _consistencyCheckService, never() ).runFullConsistencyCheck(any(), any(), any(), any(), any(), eq(false), any(), any(typeof(ConsistencyFlags)));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allFailureCausesAreCollectedAndAttachedToCommandFailedException() throws org.neo4j.commandline.admin.CommandFailed
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AllFailureCausesAreCollectedAndAttachedToCommandFailedException()
		 {
			  // given expected causes for failure
			  Exception firstCause = new Exception( "First cause" );
			  Exception secondCause = new Exception( "Second cause" );

			  // and strategies fail with given causes
			  when( _firstStrategy.doBackup( any() ) ).thenReturn(new Fallible<>(BackupStrategyOutcome.IncorrectStrategy, firstCause));
			  when( _secondStrategy.doBackup( any() ) ).thenReturn(new Fallible<>(BackupStrategyOutcome.IncorrectStrategy, secondCause));

			  // then the command failed exception contains the specified causes
			  ExpectedException.expect( exceptionContainsSuppressedThrowable( firstCause ) );
			  ExpectedException.expect( exceptionContainsSuppressedThrowable( secondCause ) );
			  ExpectedException.expect( typeof( CommandFailed ) );
			  ExpectedException.expectMessage( "Failed to run a backup using the available strategies." );

			  // when
			  _subject.performBackup( _onlineBackupContext );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void commandFailedWhenConsistencyCheckFails() throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException, org.neo4j.commandline.admin.CommandFailed
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CommandFailedWhenConsistencyCheckFails()
		 {
			  // given
			  AnyStrategyPasses();
			  when( _requiredArguments.DoConsistencyCheck ).thenReturn( true );
			  when( _consistencyCheckResult.Successful ).thenReturn( false );
			  when( _consistencyCheckService.runFullConsistencyCheck( any(), any(), eq(_progressMonitorFactory), any(typeof(LogProvider)), any(), eq(false), any(), any() ) ).thenReturn(_consistencyCheckResult);

			  // then
			  ExpectedException.expect( typeof( CommandFailed ) );
			  ExpectedException.expectMessage( "Inconsistencies found" );

			  // when
			  _subject.performBackup( _onlineBackupContext );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void havingNoStrategiesCausesAllSolutionsFailedException() throws org.neo4j.commandline.admin.CommandFailed
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void HavingNoStrategiesCausesAllSolutionsFailedException()
		 {
			  // given there are no strategies in the solution
			  _subject = new BackupStrategyCoordinator( _consistencyCheckService, _outsideWorld, _logProvider, _progressMonitorFactory, Collections.emptyList() );

			  // then we want a predictable exception (instead of NullPointer)
			  ExpectedException.expect( typeof( CommandFailed ) );
			  ExpectedException.expectMessage( "Failed to run a backup using the available strategies." );

			  // when
			  _subject.performBackup( _onlineBackupContext );
		 }

		 /// <summary>
		 /// Fixture for other tests
		 /// </summary>
		 private void AnyStrategyPasses()
		 {
			  when( _firstStrategy.doBackup( any() ) ).thenReturn(new Fallible<>(BackupStrategyOutcome.Success, null));
			  when( _secondStrategy.doBackup( any() ) ).thenReturn(new Fallible<>(BackupStrategyOutcome.Success, null));
		 }
	}

}
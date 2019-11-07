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
	using ExpectedException = org.junit.rules.ExpectedException;

	using CatchupResult = Neo4Net.causalclustering.catchup.CatchupResult;
	using StoreCopyFailedException = Neo4Net.causalclustering.catchup.storecopy.StoreCopyFailedException;
	using StoreFiles = Neo4Net.causalclustering.catchup.storecopy.StoreFiles;
	using StoreIdDownloadFailedException = Neo4Net.causalclustering.catchup.storecopy.StoreIdDownloadFailedException;
	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using OptionalHostnamePort = Neo4Net.Kernel.impl.util.OptionalHostnamePort;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

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

	public class CausalClusteringBackupStrategyTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expected = org.junit.rules.ExpectedException.none();
		 public ExpectedException Expected = ExpectedException.none();

		 internal BackupDelegator BackupDelegator = mock( typeof( BackupDelegator ) );
		 internal AddressResolver AddressResolver = mock( typeof( AddressResolver ) );
		 internal AdvertisedSocketAddress ResolvedFromAddress = new AdvertisedSocketAddress( "resolved-host", 1358 );

		 internal CausalClusteringBackupStrategy Subject;

		 internal DatabaseLayout DesiredBackupLayout = mock( typeof( DatabaseLayout ) );
		 internal Config Config = mock( typeof( Config ) );
		 internal OptionalHostnamePort UserProvidedAddress = new OptionalHostnamePort( ( string ) null, null, null );
		 internal StoreFiles StoreFiles = mock( typeof( StoreFiles ) );
		 internal StoreId ExpectedStoreId = new StoreId( 11, 22, 33, 44 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws java.io.IOException, Neo4Net.causalclustering.catchup.storecopy.StoreIdDownloadFailedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  when( AddressResolver.resolveCorrectCCAddress( any(), any() ) ).thenReturn(ResolvedFromAddress);
			  when( StoreFiles.readStoreId( any() ) ).thenReturn(ExpectedStoreId);
			  when( BackupDelegator.fetchStoreId( any() ) ).thenReturn(ExpectedStoreId);
			  Subject = new CausalClusteringBackupStrategy( BackupDelegator, AddressResolver, NullLogProvider.Instance, StoreFiles );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void incrementalBackupsUseCorrectResolvedAddress() throws Neo4Net.causalclustering.catchup.storecopy.StoreCopyFailedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IncrementalBackupsUseCorrectResolvedAddress()
		 {
			  // given
			  AdvertisedSocketAddress expectedAddress = new AdvertisedSocketAddress( "expected-host", 1298 );
			  when( AddressResolver.resolveCorrectCCAddress( any(), any() ) ).thenReturn(expectedAddress);

			  // when
			  Subject.performIncrementalBackup( DesiredBackupLayout, Config, UserProvidedAddress );

			  // then
			  verify( BackupDelegator ).tryCatchingUp( eq( expectedAddress ), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fullBackupUsesCorrectResolvedAddress() throws Neo4Net.causalclustering.catchup.storecopy.StoreIdDownloadFailedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FullBackupUsesCorrectResolvedAddress()
		 {
			  // given
			  AdvertisedSocketAddress expectedAddress = new AdvertisedSocketAddress( "expected-host", 1578 );
			  when( AddressResolver.resolveCorrectCCAddress( any(), any() ) ).thenReturn(expectedAddress);

			  // when
			  Subject.performFullBackup( DesiredBackupLayout, Config, UserProvidedAddress );

			  // then
			  verify( BackupDelegator ).fetchStoreId( expectedAddress );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void incrementalRunsCatchupWithTargetsStoreId() throws Neo4Net.causalclustering.catchup.storecopy.StoreIdDownloadFailedException, Neo4Net.causalclustering.catchup.storecopy.StoreCopyFailedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IncrementalRunsCatchupWithTargetsStoreId()
		 {

			  // when
			  Subject.performIncrementalBackup( DesiredBackupLayout, Config, UserProvidedAddress );

			  // then
			  verify( BackupDelegator ).fetchStoreId( ResolvedFromAddress );
			  verify( BackupDelegator ).tryCatchingUp( eq( ResolvedFromAddress ), eq( ExpectedStoreId ), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fullRunsRetrieveStoreWithTargetsStoreId() throws Neo4Net.causalclustering.catchup.storecopy.StoreIdDownloadFailedException, Neo4Net.causalclustering.catchup.storecopy.StoreCopyFailedException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FullRunsRetrieveStoreWithTargetsStoreId()
		 {
			  // given
			  when( StoreFiles.readStoreId( any() ) ).thenThrow(typeof(IOException));

			  // when
			  Subject.performFullBackup( DesiredBackupLayout, Config, UserProvidedAddress );

			  // then
			  verify( BackupDelegator ).fetchStoreId( ResolvedFromAddress );
			  verify( BackupDelegator ).copy( ResolvedFromAddress, ExpectedStoreId, DesiredBackupLayout );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failingToRetrieveStoreIdCausesFailWithStatus_incrementalBackup() throws Neo4Net.causalclustering.catchup.storecopy.StoreIdDownloadFailedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailingToRetrieveStoreIdCausesFailWithStatusIncrementalBackup()
		 {
			  // given
			  StoreIdDownloadFailedException storeIdDownloadFailedException = new StoreIdDownloadFailedException( "Expected description" );
			  when( BackupDelegator.fetchStoreId( any() ) ).thenThrow(storeIdDownloadFailedException);

			  // when
			  Fallible<BackupStageOutcome> state = Subject.performIncrementalBackup( DesiredBackupLayout, Config, UserProvidedAddress );

			  // then
			  assertEquals( BackupStageOutcome.WrongProtocol, state.State );
			  assertEquals( storeIdDownloadFailedException, state.Cause.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failingToCopyStoresCausesFailWithStatus_incrementalBackup() throws Neo4Net.causalclustering.catchup.storecopy.StoreIdDownloadFailedException, Neo4Net.causalclustering.catchup.storecopy.StoreCopyFailedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailingToCopyStoresCausesFailWithStatusIncrementalBackup()
		 {
			  // given
			  when( BackupDelegator.tryCatchingUp( any(), eq(ExpectedStoreId), any() ) ).thenThrow(typeof(StoreCopyFailedException));

			  // when
			  Fallible state = Subject.performIncrementalBackup( DesiredBackupLayout, Config, UserProvidedAddress );

			  // then
			  assertEquals( BackupStageOutcome.Failure, state.State );
			  assertEquals( typeof( StoreCopyFailedException ), state.Cause.get().GetType() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failingToRetrieveStoreIdCausesFailWithStatus_fullBackup() throws Neo4Net.causalclustering.catchup.storecopy.StoreIdDownloadFailedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailingToRetrieveStoreIdCausesFailWithStatusFullBackup()
		 {
			  // given
			  StoreIdDownloadFailedException storeIdDownloadFailedException = new StoreIdDownloadFailedException( "Expected description" );
			  when( BackupDelegator.fetchStoreId( any() ) ).thenThrow(storeIdDownloadFailedException);

			  // when
			  Fallible state = Subject.performFullBackup( DesiredBackupLayout, Config, UserProvidedAddress );

			  // then
			  assertEquals( BackupStageOutcome.WrongProtocol, state.State );
			  assertEquals( storeIdDownloadFailedException, state.Cause.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failingToCopyStoresCausesFailWithStatus_fullBackup() throws Neo4Net.causalclustering.catchup.storecopy.StoreCopyFailedException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailingToCopyStoresCausesFailWithStatusFullBackup()
		 {
			  // given
			  doThrow( typeof( StoreCopyFailedException ) ).when( BackupDelegator ).copy( any(), any(), any() );

			  // and
			  when( StoreFiles.readStoreId( any() ) ).thenThrow(typeof(IOException));

			  // when
			  Fallible state = Subject.performFullBackup( DesiredBackupLayout, Config, UserProvidedAddress );

			  // then
			  assertEquals( BackupStageOutcome.Failure, state.State );
			  Console.WriteLine( state.Cause );
			  assertEquals( typeof( StoreCopyFailedException ), state.Cause.get().GetType() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void incrementalBackupsEndingInUnacceptedCatchupStateCauseFailures() throws Neo4Net.causalclustering.catchup.storecopy.StoreCopyFailedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IncrementalBackupsEndingInUnacceptedCatchupStateCauseFailures()
		 {
			  // given
			  when( BackupDelegator.tryCatchingUp( any(), any(), any() ) ).thenReturn(CatchupResult.E_STORE_UNAVAILABLE);

			  // when
			  Fallible<BackupStageOutcome> state = Subject.performIncrementalBackup( DesiredBackupLayout, Config, UserProvidedAddress );

			  // then
			  assertEquals( BackupStageOutcome.Failure, state.State );
			  assertEquals( typeof( StoreCopyFailedException ), state.Cause.get().GetType() );
			  assertEquals( "End state of catchup was not a successful end of stream", state.Cause.get().Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lifecycleDelegatesToNecessaryServices() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LifecycleDelegatesToNecessaryServices()
		 {
			  // when
			  Subject.start();

			  // then
			  verify( BackupDelegator ).start();
			  verify( BackupDelegator, never() ).stop();

			  // when
			  Subject.stop();

			  // then
			  verify( BackupDelegator ).start(); // still total 1 calls
			  verify( BackupDelegator ).stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exceptionWhenStoreMismatchNoExistingBackup() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ExceptionWhenStoreMismatchNoExistingBackup()
		 {
			  // given
			  when( StoreFiles.readStoreId( any() ) ).thenThrow(typeof(IOException));

			  // when
			  Fallible<BackupStageOutcome> state = Subject.performIncrementalBackup( DesiredBackupLayout, Config, UserProvidedAddress );

			  // then
			  assertEquals( typeof( StoreIdDownloadFailedException ), state.Cause.get().GetType() );
			  assertEquals( BackupStageOutcome.Failure, state.State );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void exceptionWhenStoreMismatch() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ExceptionWhenStoreMismatch()
		 {
			  // given
			  when( StoreFiles.readStoreId( any() ) ).thenReturn(new StoreId(5, 4, 3, 2));

			  // when
			  Fallible<BackupStageOutcome> state = Subject.performIncrementalBackup( DesiredBackupLayout, Config, UserProvidedAddress );

			  // then
			  assertEquals( typeof( StoreIdDownloadFailedException ), state.Cause.get().GetType() );
			  assertEquals( BackupStageOutcome.Failure, state.State );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fullBackupFailsWhenTargetHasStoreId() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FullBackupFailsWhenTargetHasStoreId()
		 {
			  // given
			  when( StoreFiles.readStoreId( any() ) ).thenReturn(ExpectedStoreId);

			  // when
			  Fallible<BackupStageOutcome> state = Subject.performFullBackup( DesiredBackupLayout, Config, UserProvidedAddress );

			  // then
			  assertEquals( typeof( StoreIdDownloadFailedException ), state.Cause.get().GetType() );
			  assertEquals( BackupStageOutcome.Failure, state.State );
		 }
	}

}
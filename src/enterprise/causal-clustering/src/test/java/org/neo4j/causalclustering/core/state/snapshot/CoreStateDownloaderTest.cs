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
namespace Neo4Net.causalclustering.core.state.snapshot
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using CatchUpClient = Neo4Net.causalclustering.catchup.CatchUpClient;
	using CatchupAddressProvider = Neo4Net.causalclustering.catchup.CatchupAddressProvider;
	using CommitStateHelper = Neo4Net.causalclustering.catchup.storecopy.CommitStateHelper;
	using LocalDatabase = Neo4Net.causalclustering.catchup.storecopy.LocalDatabase;
	using RemoteStore = Neo4Net.causalclustering.catchup.storecopy.RemoteStore;
	using StoreCopyProcess = Neo4Net.causalclustering.catchup.storecopy.StoreCopyProcess;
	using CoreStateMachines = Neo4Net.causalclustering.core.state.machines.CoreStateMachines;
	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using Suspendable = Neo4Net.causalclustering.helper.Suspendable;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.CatchupResult.E_TRANSACTION_PRUNED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.CatchupResult.SUCCESS_END_OF_STREAM;

	public class CoreStateDownloaderTest
	{
		private bool InstanceFieldsInitialized = false;

		public CoreStateDownloaderTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_catchupAddressProvider = CatchupAddressProvider.fromSingleAddress( _remoteAddress );
			_downloader = new CoreStateDownloader( _localDatabase, _startStopLife, _remoteStore, _catchUpClient, _logProvider, _storeCopyProcess, _coreStateMachines, _snapshotService, _commitStateHelper );
		}

		 private readonly LocalDatabase _localDatabase = mock( typeof( LocalDatabase ) );
		 private readonly Suspendable _startStopLife = mock( typeof( Suspendable ) );
		 private readonly RemoteStore _remoteStore = mock( typeof( RemoteStore ) );
		 private readonly CatchUpClient _catchUpClient = mock( typeof( CatchUpClient ) );
		 private readonly StoreCopyProcess _storeCopyProcess = mock( typeof( StoreCopyProcess ) );
		 private CoreSnapshotService _snapshotService = mock( typeof( CoreSnapshotService ) );
		 private TopologyService _topologyService = mock( typeof( TopologyService ) );
		 private CommitStateHelper _commitStateHelper = mock( typeof( CommitStateHelper ) );
		 private readonly CoreStateMachines _coreStateMachines = mock( typeof( CoreStateMachines ) );

		 private readonly NullLogProvider _logProvider = NullLogProvider.Instance;

		 private readonly MemberId _remoteMember = new MemberId( System.Guid.randomUUID() );
		 private readonly AdvertisedSocketAddress _remoteAddress = new AdvertisedSocketAddress( "remoteAddress", 1234 );
		 private CatchupAddressProvider _catchupAddressProvider;
		 private readonly StoreId _storeId = new StoreId( 1, 2, 3, 4 );
		 private readonly DatabaseLayout _databaseLayout = DatabaseLayout.of( new File( "." ) );

		 private CoreStateDownloader _downloader;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void commonMocking()
		 public virtual void CommonMocking()
		 {
			  when( _localDatabase.storeId() ).thenReturn(_storeId);
			  when( _localDatabase.databaseLayout() ).thenReturn(_databaseLayout);
			  when( _topologyService.findCatchupAddress( _remoteMember ) ).thenReturn( _remoteAddress );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDownloadCompleteStoreWhenEmpty() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDownloadCompleteStoreWhenEmpty()
		 {
			  // given
			  StoreId remoteStoreId = new StoreId( 5, 6, 7, 8 );
			  when( _remoteStore.getStoreId( _remoteAddress ) ).thenReturn( remoteStoreId );
			  when( _localDatabase.Empty ).thenReturn( true );

			  // when
			  _downloader.downloadSnapshot( _catchupAddressProvider );

			  // then
			  verify( _remoteStore, never() ).tryCatchingUp(any(), any(), any(), anyBoolean(), anyBoolean());
			  verify( _storeCopyProcess ).replaceWithStoreFrom( _catchupAddressProvider, remoteStoreId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStopDatabaseDuringDownload() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStopDatabaseDuringDownload()
		 {
			  // given
			  when( _localDatabase.Empty ).thenReturn( true );

			  // when
			  _downloader.downloadSnapshot( _catchupAddressProvider );

			  // then
			  verify( _startStopLife ).disable();
			  verify( _localDatabase ).stopForStoreCopy();
			  verify( _localDatabase ).start();
			  verify( _startStopLife ).enable();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotOverwriteNonEmptyMismatchingStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotOverwriteNonEmptyMismatchingStore()
		 {
			  // given
			  when( _localDatabase.Empty ).thenReturn( false );
			  StoreId remoteStoreId = new StoreId( 5, 6, 7, 8 );
			  when( _remoteStore.getStoreId( _remoteAddress ) ).thenReturn( remoteStoreId );

			  // when
			  assertFalse( _downloader.downloadSnapshot( _catchupAddressProvider ) );

			  // then
			  verify( _remoteStore, never() ).copy(any(), any(), any(), anyBoolean());
			  verify( _remoteStore, never() ).tryCatchingUp(any(), any(), any(), anyBoolean(), anyBoolean());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCatchupIfPossible() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCatchupIfPossible()
		 {
			  // given
			  when( _localDatabase.Empty ).thenReturn( false );
			  when( _remoteStore.getStoreId( _remoteAddress ) ).thenReturn( _storeId );
			  when( _remoteStore.tryCatchingUp( _remoteAddress, _storeId, _databaseLayout, false, false ) ).thenReturn( SUCCESS_END_OF_STREAM );

			  // when
			  _downloader.downloadSnapshot( _catchupAddressProvider );

			  // then
			  verify( _remoteStore ).tryCatchingUp( _remoteAddress, _storeId, _databaseLayout, false, false );
			  verify( _remoteStore, never() ).copy(any(), any(), any(), anyBoolean());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDownloadWholeStoreIfCannotCatchUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDownloadWholeStoreIfCannotCatchUp()
		 {
			  // given
			  when( _localDatabase.Empty ).thenReturn( false );
			  when( _remoteStore.getStoreId( _remoteAddress ) ).thenReturn( _storeId );
			  when( _remoteStore.tryCatchingUp( _remoteAddress, _storeId, _databaseLayout, false, false ) ).thenReturn( E_TRANSACTION_PRUNED );

			  // when
			  _downloader.downloadSnapshot( _catchupAddressProvider );

			  // then
			  verify( _remoteStore ).tryCatchingUp( _remoteAddress, _storeId, _databaseLayout, false, false );
			  verify( _storeCopyProcess ).replaceWithStoreFrom( _catchupAddressProvider, _storeId );
		 }
	}

}
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
namespace Neo4Net.causalclustering.readreplica
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using LocalDatabase = Neo4Net.causalclustering.catchup.storecopy.LocalDatabase;
	using RemoteStore = Neo4Net.causalclustering.catchup.storecopy.RemoteStore;
	using StoreCopyProcess = Neo4Net.causalclustering.catchup.storecopy.StoreCopyProcess;
	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using CoreServerInfo = Neo4Net.causalclustering.discovery.CoreServerInfo;
	using CoreTopology = Neo4Net.causalclustering.discovery.CoreTopology;
	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using ConstantTimeTimeoutStrategy = Neo4Net.causalclustering.helper.ConstantTimeTimeoutStrategy;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using UpstreamDatabaseSelectionStrategy = Neo4Net.causalclustering.upstream.UpstreamDatabaseSelectionStrategy;
	using UpstreamDatabaseStrategySelector = Neo4Net.causalclustering.upstream.UpstreamDatabaseStrategySelector;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Service = Neo4Net.Helpers.Service;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ReadReplicaStartupProcessTest
	{
		 private ConstantTimeTimeoutStrategy _retryStrategy = new ConstantTimeTimeoutStrategy( 1, MILLISECONDS );
		 private StoreCopyProcess _storeCopyProcess = mock( typeof( StoreCopyProcess ) );
		 private RemoteStore _remoteStore = mock( typeof( RemoteStore ) );
		 private readonly PageCache _pageCache = mock( typeof( PageCache ) );
		 private LocalDatabase _localDatabase = mock( typeof( LocalDatabase ) );
		 private TopologyService _topologyService = mock( typeof( TopologyService ) );
		 private CoreTopology _clusterTopology = mock( typeof( CoreTopology ) );
		 private Lifecycle _txPulling = mock( typeof( Lifecycle ) );

		 private MemberId _memberId = new MemberId( System.Guid.randomUUID() );
		 private AdvertisedSocketAddress _fromAddress = new AdvertisedSocketAddress( "127.0.0.1", 123 );
		 private StoreId _localStoreId = new StoreId( 1, 2, 3, 4 );
		 private StoreId _otherStoreId = new StoreId( 5, 6, 7, 8 );
		 private DatabaseLayout _databaseLayout = DatabaseLayout.of( new File( "store-dir" ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void commonMocking() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CommonMocking()
		 {
			  IDictionary<MemberId, CoreServerInfo> members = new Dictionary<MemberId, CoreServerInfo>();
			  members[_memberId] = mock( typeof( CoreServerInfo ) );

			  FileSystemAbstraction fileSystemAbstraction = mock( typeof( FileSystemAbstraction ) );
			  when( fileSystemAbstraction.StreamFilesRecursive( any( typeof( File ) ) ) ).thenAnswer( f => Stream.empty() );
			  when( _localDatabase.databaseLayout() ).thenReturn(_databaseLayout);
			  when( _localDatabase.storeId() ).thenReturn(_localStoreId);
			  when( _topologyService.allCoreServers() ).thenReturn(_clusterTopology);
			  when( _clusterTopology.members() ).thenReturn(members);
			  when( _topologyService.findCatchupAddress( _memberId ) ).thenReturn( _fromAddress );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReplaceEmptyStoreWithRemote() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReplaceEmptyStoreWithRemote()
		 {
			  // given
			  when( _localDatabase.Empty ).thenReturn( true );
			  when( _topologyService.findCatchupAddress( any() ) ).thenReturn(_fromAddress);
			  when( _remoteStore.getStoreId( any() ) ).thenReturn(_otherStoreId);

			  ReadReplicaStartupProcess readReplicaStartupProcess = new ReadReplicaStartupProcess( _remoteStore, _localDatabase, _txPulling, ChooseFirstMember(), _retryStrategy, NullLogProvider.Instance, NullLogProvider.Instance, _storeCopyProcess, _topologyService );

			  // when
			  readReplicaStartupProcess.Start();

			  // then
			  verify( _storeCopyProcess ).replaceWithStoreFrom( any(), any() );
			  verify( _localDatabase ).start();
			  verify( _txPulling ).start();
		 }

		 private UpstreamDatabaseStrategySelector ChooseFirstMember()
		 {
			  AlwaysChooseFirstMember firstMember = new AlwaysChooseFirstMember();
			  Config config = mock( typeof( Config ) );
			  when( config.Get( CausalClusteringSettings.database ) ).thenReturn( "default" );
			  firstMember.Inject( _topologyService, config, NullLogProvider.Instance, null );

			  return new UpstreamDatabaseStrategySelector( firstMember );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotStartWithMismatchedNonEmptyStore() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotStartWithMismatchedNonEmptyStore()
		 {
			  // given
			  when( _localDatabase.Empty ).thenReturn( false );
			  when( _remoteStore.getStoreId( any() ) ).thenReturn(_otherStoreId);

			  ReadReplicaStartupProcess readReplicaStartupProcess = new ReadReplicaStartupProcess( _remoteStore, _localDatabase, _txPulling, ChooseFirstMember(), _retryStrategy, NullLogProvider.Instance, NullLogProvider.Instance, _storeCopyProcess, _topologyService );

			  // when
			  try
			  {
					readReplicaStartupProcess.Start();
					fail( "should have thrown" );
			  }
			  catch ( Exception ex )
			  {
					//expected.
					assertThat( ex.Message, containsString( "This read replica cannot join the cluster. The local database is not empty and has a " + "mismatching storeId" ) );
			  }

			  // then
			  verify( _txPulling, never() ).start();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartWithMatchingDatabase() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStartWithMatchingDatabase()
		 {
			  // given
			  when( _remoteStore.getStoreId( any() ) ).thenReturn(_localStoreId);
			  when( _localDatabase.Empty ).thenReturn( false );

			  ReadReplicaStartupProcess readReplicaStartupProcess = new ReadReplicaStartupProcess( _remoteStore, _localDatabase, _txPulling, ChooseFirstMember(), _retryStrategy, NullLogProvider.Instance, NullLogProvider.Instance, _storeCopyProcess, _topologyService );

			  // when
			  readReplicaStartupProcess.Start();

			  // then
			  verify( _localDatabase ).start();
			  verify( _txPulling ).start();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stopShouldStopTheDatabaseAndStopPolling() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StopShouldStopTheDatabaseAndStopPolling()
		 {
			  // given
			  when( _remoteStore.getStoreId( any() ) ).thenReturn(_localStoreId);
			  when( _localDatabase.Empty ).thenReturn( false );

			  ReadReplicaStartupProcess readReplicaStartupProcess = new ReadReplicaStartupProcess( _remoteStore, _localDatabase, _txPulling, ChooseFirstMember(), _retryStrategy, NullLogProvider.Instance, NullLogProvider.Instance, _storeCopyProcess, _topologyService );

			  readReplicaStartupProcess.Start();

			  // when
			  readReplicaStartupProcess.Stop();

			  // then
			  verify( _txPulling ).stop();
			  verify( _localDatabase ).stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(UpstreamDatabaseSelectionStrategy.class) public static class AlwaysChooseFirstMember extends org.Neo4Net.causalclustering.upstream.UpstreamDatabaseSelectionStrategy
		 public class AlwaysChooseFirstMember : UpstreamDatabaseSelectionStrategy
		 {
			  public AlwaysChooseFirstMember() : base("always-choose-first-member")
			  {
			  }

			  public override Optional<MemberId> UpstreamDatabase()
			  {
					CoreTopology coreTopology = TopologyService.allCoreServers();
					return Optional.ofNullable( coreTopology.Members().Keys.GetEnumerator().next() );
			  }
		 }
	}

}
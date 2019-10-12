using System;
using System.Threading;

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
namespace Org.Neo4j.Kernel.ha.cluster
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;
	using Mockito = org.mockito.Mockito;


	using OnlineBackupKernelExtension = Org.Neo4j.backup.OnlineBackupKernelExtension;
	using ClusterSettings = Org.Neo4j.cluster.ClusterSettings;
	using InstanceId = Org.Neo4j.cluster.InstanceId;
	using ClusterMemberAvailability = Org.Neo4j.cluster.member.ClusterMemberAvailability;
	using Org.Neo4j.com;
	using MoveAfterCopy = Org.Neo4j.com.storecopy.MoveAfterCopy;
	using StoreCopyClient = Org.Neo4j.com.storecopy.StoreCopyClient;
	using TransactionCommittingResponseUnpacker = Org.Neo4j.com.storecopy.TransactionCommittingResponseUnpacker;
	using TransactionObligationFulfiller = Org.Neo4j.com.storecopy.TransactionObligationFulfiller;
	using Suppliers = Org.Neo4j.Function.Suppliers;
	using CancellationRequest = Org.Neo4j.Helpers.CancellationRequest;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using PagedFile = Org.Neo4j.Io.pagecache.PagedFile;
	using DatabaseAvailability = Org.Neo4j.Kernel.availability.DatabaseAvailability;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Org.Neo4j.Kernel.ha;
	using ClusterMember = Org.Neo4j.Kernel.ha.cluster.member.ClusterMember;
	using ClusterMembers = Org.Neo4j.Kernel.ha.cluster.member.ClusterMembers;
	using HighAvailabilityModeSwitcher = Org.Neo4j.Kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher;
	using RequestContextFactory = Org.Neo4j.Kernel.ha.com.RequestContextFactory;
	using HandshakeResult = Org.Neo4j.Kernel.ha.com.master.HandshakeResult;
	using MasterClient = Org.Neo4j.Kernel.ha.com.slave.MasterClient;
	using MasterClientResolver = Org.Neo4j.Kernel.ha.com.slave.MasterClientResolver;
	using SlaveServer = Org.Neo4j.Kernel.ha.com.slave.SlaveServer;
	using HaIdGeneratorFactory = Org.Neo4j.Kernel.ha.id.HaIdGeneratorFactory;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using MismatchingStoreIdException = Org.Neo4j.Kernel.impl.store.MismatchingStoreIdException;
	using TransactionId = Org.Neo4j.Kernel.impl.store.TransactionId;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using DataSourceManager = Org.Neo4j.Kernel.impl.transaction.state.DataSourceManager;
	using DatabaseTransactionStats = Org.Neo4j.Kernel.impl.transaction.stats.DatabaseTransactionStats;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using FileSystemWatcherService = Org.Neo4j.Kernel.impl.util.watcher.FileSystemWatcherService;
	using StoreLockerLifecycleAdapter = Org.Neo4j.Kernel.@internal.locker.StoreLockerLifecycleAdapter;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using NullLogService = Org.Neo4j.Logging.@internal.NullLogService;
	using Group = Org.Neo4j.Scheduler.Group;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.argThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.withSettings;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.StoreIdTestFactory.newStoreIdForCurrentVersion;

	public class SwitchToSlaveCopyThenBranchTest
	{
		 private readonly UpdatePuller _updatePuller = MockWithLifecycle( typeof( SlaveUpdatePuller ) );
		 private readonly PullerFactory _pullerFactory = mock( typeof( PullerFactory ) );
		 private readonly FileSystemAbstraction _fs = mock( typeof( FileSystemAbstraction ) );
		 private readonly MasterClient _masterClient = mock( typeof( MasterClient ) );
		 private readonly RequestContextFactory _requestContextFactory = mock( typeof( RequestContextFactory ) );
		 private readonly StoreId _storeId = newStoreIdForCurrentVersion( 42, 42, 42, 42 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRestartServicesIfCopyStoreFails() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRestartServicesIfCopyStoreFails()
		 {
			  when( _updatePuller.tryPullUpdates() ).thenReturn(true);

			  PageCache pageCacheMock = MockPageCache();

			  StoreCopyClient storeCopyClient = mock( typeof( StoreCopyClient ) );

			  doThrow( new Exception() ).doNothing().when(storeCopyClient).copyStore(any(typeof(StoreCopyClient.StoreCopyRequester)), any(typeof(CancellationRequest)), any(typeof(MoveAfterCopy)));

			  SwitchToSlaveCopyThenBranch switchToSlave = NewSwitchToSlaveSpy( pageCacheMock, storeCopyClient );

			  URI localhost = LocalhostUri;
			  try
			  {
					switchToSlave.SwitchToSlaveConflict( mock( typeof( LifeSupport ) ), localhost, localhost, mock( typeof( CancellationRequest ) ) );
					fail( "Should have thrown an Exception" );
			  }
			  catch ( Exception )
			  {
					verify( _requestContextFactory, never() ).start();
					// Store should have been deleted due to failure in copy
					verify( switchToSlave ).cleanStoreDir();

					// Try again, should succeed
					switchToSlave.SwitchToSlaveConflict( mock( typeof( LifeSupport ) ), localhost, localhost, mock( typeof( CancellationRequest ) ) );
					verify( _requestContextFactory ).start();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.io.pagecache.PageCache mockPageCache() throws java.io.IOException
		 private PageCache MockPageCache()
		 {
			  PageCache pageCacheMock = mock( typeof( PageCache ) );
			  PagedFile pagedFileMock = mock( typeof( PagedFile ) );
			  when( pagedFileMock.LastPageId ).thenReturn( 1L );
			  when( pageCacheMock.Map( any( typeof( File ) ), anyInt() ) ).thenThrow(new IOException()).thenThrow(new IOException()).thenReturn(pagedFileMock);
			  return pageCacheMock;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @SuppressWarnings("unchecked") public void shouldHandleBranchedStoreWhenMyStoreIdDiffersFromMasterStoreId() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleBranchedStoreWhenMyStoreIdDiffersFromMasterStoreId()
		 {
			  // Given
			  SwitchToSlaveCopyThenBranch switchToSlave = NewSwitchToSlaveSpy();
			  URI me = new URI( "cluster://localhost?serverId=2" );

			  MasterClient masterClient = mock( typeof( MasterClient ) );
			  Response<HandshakeResult> response = mock( typeof( Response ) );
			  when( response.ResponseConflict() ).thenReturn(new HandshakeResult(1, 2));
			  when( masterClient.Handshake( anyLong(), any(typeof(StoreId)) ) ).thenReturn(response);

			  StoreId storeId = newStoreIdForCurrentVersion( 1, 2, 3, 4 );

			  TransactionIdStore transactionIdStore = mock( typeof( TransactionIdStore ) );
			  when( transactionIdStore.LastCommittedTransaction ).thenReturn( new TransactionId( 42, 42, 42 ) );
			  when( transactionIdStore.LastCommittedTransactionId ).thenReturn( Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID );

			  // When
			  try
			  {
					switchToSlave.CheckDataConsistency( masterClient, transactionIdStore, storeId, new URI( "cluster://localhost?serverId=1" ), me, Org.Neo4j.Helpers.CancellationRequest_Fields.NeverCancelled );
					fail( "Should have thrown " + typeof( MismatchingStoreIdException ).Name + " exception" );
			  }
			  catch ( MismatchingStoreIdException )
			  {
					// good we got the expected exception
			  }

			  // Then
			  verify( switchToSlave ).stopServicesAndHandleBranchedStore( any( typeof( BranchedDataPolicy ) ), any( typeof( URI ) ), any( typeof( URI ) ), any( typeof( CancellationRequest ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleBranchedStoreWhenHandshakeFailsWithBranchedDataException() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleBranchedStoreWhenHandshakeFailsWithBranchedDataException()
		 {
			  // Given
			  SwitchToSlaveCopyThenBranch switchToSlave = NewSwitchToSlaveSpy();
			  URI masterUri = new URI( "cluster://localhost?serverId=1" );
			  URI me = new URI( "cluster://localhost?serverId=2" );

			  MasterClient masterClient = mock( typeof( MasterClient ) );
			  when( masterClient.Handshake( anyLong(), any(typeof(StoreId)) ) ).thenThrow(new BranchedDataException(""));

			  TransactionIdStore transactionIdStore = mock( typeof( TransactionIdStore ) );
			  when( transactionIdStore.LastCommittedTransaction ).thenReturn( new TransactionId( 42, 42, 42 ) );
			  when( transactionIdStore.LastCommittedTransactionId ).thenReturn( Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID );

			  // When
			  try
			  {
					switchToSlave.CheckDataConsistency( masterClient, transactionIdStore, _storeId, masterUri, me, Org.Neo4j.Helpers.CancellationRequest_Fields.NeverCancelled );
					fail( "Should have thrown " + typeof( BranchedDataException ).Name + " exception" );
			  }
			  catch ( BranchedDataException )
			  {
					// good we got the expected exception
			  }

			  // Then
			  verify( switchToSlave ).stopServicesAndHandleBranchedStore( any( typeof( BranchedDataPolicy ) ), any( typeof( URI ) ), any( typeof( URI ) ), any( typeof( CancellationRequest ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBranchStoreUnlessWeHaveCopiedDownAReplacement() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBranchStoreUnlessWeHaveCopiedDownAReplacement()
		 {
			  // Given
			  StoreCopyClient storeCopyClient = mock( typeof( StoreCopyClient ) );
			  doAnswer(invocation =>
			  {
				MoveAfterCopy moveAfterCopy = invocation.getArgument( 2 );
				moveAfterCopy.move( Stream.empty(), new File(""), Function.identity() );
				return null;
			  }).when( storeCopyClient ).copyStore( any( typeof( StoreCopyClient.StoreCopyRequester ) ), any( typeof( CancellationRequest ) ), any( typeof( MoveAfterCopy ) ) );

			  PageCache pageCacheMock = mock( typeof( PageCache ) );
			  PagedFile pagedFileMock = mock( typeof( PagedFile ) );
			  when( pagedFileMock.LastPageId ).thenReturn( 1L );
			  when( pageCacheMock.Map( any( typeof( File ) ), anyInt() ) ).thenReturn(pagedFileMock);

			  SwitchToSlaveCopyThenBranch switchToSlave = NewSwitchToSlaveSpy( pageCacheMock, storeCopyClient );

			  URI masterUri = new URI( "cluster://localhost?serverId=1" );
			  URI me = new URI( "cluster://localhost?serverId=2" );
			  CancellationRequest cancellationRequest = Org.Neo4j.Helpers.CancellationRequest_Fields.NeverCancelled;

			  MasterClient masterClient = mock( typeof( MasterClient ) );
			  when( masterClient.Handshake( anyLong(), any(typeof(StoreId)) ) ).thenThrow(new BranchedDataException(""));

			  TransactionIdStore transactionIdStore = mock( typeof( TransactionIdStore ) );
			  when( transactionIdStore.LastCommittedTransaction ).thenReturn( new TransactionId( 42, 42, 42 ) );
			  when( transactionIdStore.LastCommittedTransactionId ).thenReturn( Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID );

			  // When
			  BranchedDataPolicy branchPolicy = mock( typeof( BranchedDataPolicy ) );
			  switchToSlave.StopServicesAndHandleBranchedStore( branchPolicy, masterUri, me, cancellationRequest );

			  // Then
			  InOrder inOrder = Mockito.inOrder( storeCopyClient, branchPolicy );

			  inOrder.verify( storeCopyClient ).copyStore( any( typeof( StoreCopyClient.StoreCopyRequester ) ), any( typeof( CancellationRequest ) ), any( typeof( MoveAfterCopy ) ) );
			  inOrder.verify( branchPolicy ).handle( TestDirectory.databaseDir(), pageCacheMock, NullLogService.Instance );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNullIfWhenFailingToPullingUpdatesFromMaster() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnNullIfWhenFailingToPullingUpdatesFromMaster()
		 {
			  // Given
			  SwitchToSlaveCopyThenBranch switchToSlave = NewSwitchToSlaveSpy();

			  when( _fs.fileExists( any( typeof( File ) ) ) ).thenReturn( true );
			  when( _updatePuller.tryPullUpdates() ).thenReturn(false);

			  // when
			  URI localhost = LocalhostUri;
			  URI uri = switchToSlave.SwitchToSlaveConflict( mock( typeof( LifeSupport ) ), localhost, localhost, mock( typeof( CancellationRequest ) ) );

			  // then
			  assertNull( uri );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void updatesPulledAndPullingScheduledOnSwitchToSlave() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UpdatesPulledAndPullingScheduledOnSwitchToSlave()
		 {
			  SwitchToSlaveCopyThenBranch switchToSlave = NewSwitchToSlaveSpy();

			  when( _fs.fileExists( any( typeof( File ) ) ) ).thenReturn( true );
			  JobScheduler jobScheduler = mock( typeof( JobScheduler ) );
			  LifeSupport communicationLife = mock( typeof( LifeSupport ) );
			  URI localhost = LocalhostUri;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.ha.UpdatePullerScheduler pullerScheduler = new org.neo4j.kernel.ha.UpdatePullerScheduler(jobScheduler, org.neo4j.logging.NullLogProvider.getInstance(), updatePuller, 10L);
			  UpdatePullerScheduler pullerScheduler = new UpdatePullerScheduler( jobScheduler, NullLogProvider.Instance, _updatePuller, 10L );

			  when( _pullerFactory.createUpdatePullerScheduler( _updatePuller ) ).thenReturn( pullerScheduler );
			  // emulate lifecycle start call on scheduler
			  doAnswer(invocationOnMock =>
			  {
				pullerScheduler.Init();
				return null;
			  }).when( communicationLife ).start();

			  switchToSlave.SwitchToSlaveConflict( communicationLife, localhost, localhost, mock( typeof( CancellationRequest ) ) );

			  verify( _updatePuller ).tryPullUpdates();
			  verify( communicationLife ).add( pullerScheduler );
			  verify( jobScheduler ).scheduleRecurring( eq( Group.PULL_UPDATES ), any( typeof( ThreadStart ) ), eq( 10L ), eq( 10L ), eq( TimeUnit.MILLISECONDS ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.net.URI getLocalhostUri() throws java.net.URISyntaxException
		 private URI LocalhostUri
		 {
			 get
			 {
				  return new URI( "cluster://127.0.0.1?serverId=1" );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private SwitchToSlaveCopyThenBranch newSwitchToSlaveSpy() throws Exception
		 private SwitchToSlaveCopyThenBranch NewSwitchToSlaveSpy()
		 {
			  PageCache pageCacheMock = mock( typeof( PageCache ) );
			  PagedFile pagedFileMock = mock( typeof( PagedFile ) );
			  when( pagedFileMock.LastPageId ).thenReturn( 1L );
			  when( pageCacheMock.Map( any( typeof( File ) ), anyInt() ) ).thenReturn(pagedFileMock);

			  StoreCopyClient storeCopyClient = mock( typeof( StoreCopyClient ) );
			  Stream mockStream = mock( typeof( Stream ) );
			  when( mockStream.filter( any( typeof( System.Predicate ) ) ) ).thenReturn( mockStream );
			  FileSystemAbstraction fileSystemAbstraction = mock( typeof( FileSystemAbstraction ) );
			  when( fileSystemAbstraction.StreamFilesRecursive( any( typeof( File ) ) ) ).thenReturn( mockStream );
			  return NewSwitchToSlaveSpy( pageCacheMock, storeCopyClient );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private SwitchToSlaveCopyThenBranch newSwitchToSlaveSpy(org.neo4j.io.pagecache.PageCache pageCacheMock, org.neo4j.com.storecopy.StoreCopyClient storeCopyClient)
		 private SwitchToSlaveCopyThenBranch NewSwitchToSlaveSpy( PageCache pageCacheMock, StoreCopyClient storeCopyClient )
		 {
			  ClusterMembers clusterMembers = mock( typeof( ClusterMembers ) );
			  ClusterMember master = mock( typeof( ClusterMember ) );
			  when( master.StoreId ).thenReturn( _storeId );
			  when( master.HARole ).thenReturn( HighAvailabilityModeSwitcher.MASTER );
			  when( master.HasRole( eq( HighAvailabilityModeSwitcher.MASTER ) ) ).thenReturn( true );
			  when( master.InstanceId ).thenReturn( new InstanceId( 1 ) );
			  when( clusterMembers.Members ).thenReturn( asList( master ) );

			  Dependencies resolver = new Dependencies();
			  DatabaseAvailability databaseAvailability = mock( typeof( DatabaseAvailability ) );
			  when( databaseAvailability.Started ).thenReturn( true );
			  resolver.SatisfyDependencies( _requestContextFactory, clusterMembers, mock( typeof( TransactionObligationFulfiller ) ), mock( typeof( OnlineBackupKernelExtension ) ), mock( typeof( IndexConfigStore ) ), mock( typeof( TransactionCommittingResponseUnpacker ) ), mock( typeof( DataSourceManager ) ), mock( typeof( StoreLockerLifecycleAdapter ) ), mock( typeof( FileSystemWatcherService ) ), databaseAvailability );

			  NeoStoreDataSource dataSource = mock( typeof( NeoStoreDataSource ) );
			  when( dataSource.StoreId ).thenReturn( _storeId );
			  when( dataSource.DependencyResolver ).thenReturn( resolver );

			  DatabaseTransactionStats transactionCounters = mock( typeof( DatabaseTransactionStats ) );
			  when( transactionCounters.NumberOfActiveTransactions ).thenReturn( 0L );

			  Response<HandshakeResult> response = mock( typeof( Response ) );
			  when( response.ResponseConflict() ).thenReturn(new HandshakeResult(42, 2));
			  when( _masterClient.handshake( anyLong(), any(typeof(StoreId)) ) ).thenReturn(response);
			  when( _masterClient.ProtocolVersion ).thenReturn( Org.Neo4j.Kernel.ha.com.slave.MasterClient_Fields.Current );

			  TransactionIdStore transactionIdStoreMock = mock( typeof( TransactionIdStore ) );
			  // note that the checksum (the second member of the array) is the same as the one in the handshake mock above
			  when( transactionIdStoreMock.LastCommittedTransaction ).thenReturn( new TransactionId( 42, 42, 42 ) );

			  MasterClientResolver masterClientResolver = mock( typeof( MasterClientResolver ) );
			  when( masterClientResolver.Instantiate( anyString(), anyInt(), anyString(), any(typeof(Monitors)), argThat(_storeId => true), any(typeof(LifeSupport)) ) ).thenReturn(_masterClient);

			  return spy(new SwitchToSlaveCopyThenBranch(TestDirectory.databaseLayout(), NullLogService.Instance, ConfigMock(), mock(typeof(HaIdGeneratorFactory)), mock(typeof(DelegateInvocationHandler)), mock(typeof(ClusterMemberAvailability)), _requestContextFactory, _pullerFactory, masterClientResolver, mock(typeof(SwitchToSlave.Monitor)), storeCopyClient, Suppliers.singleton(dataSource), Suppliers.singleton(transactionIdStoreMock), slave =>
			  {
						  SlaveServer server = mock( typeof( SlaveServer ) );
						  InetSocketAddress inetSocketAddress = InetSocketAddress.createUnresolved( "localhost", 42 );

						  when( server.SocketAddress ).thenReturn( inetSocketAddress );
						  return server;
			  }, _updatePuller, pageCacheMock, mock( typeof( Monitors ) ), () => transactionCounters));
		 }

		 private static Config ConfigMock()
		 {
			  return Config.defaults( ClusterSettings.server_id, "1" );
		 }

		 private static T MockWithLifecycle<T>( Type clazz )
		 {
				 clazz = typeof( T );
			  return mock( clazz, withSettings().extraInterfaces(typeof(Lifecycle)) );
		 }
	}

}
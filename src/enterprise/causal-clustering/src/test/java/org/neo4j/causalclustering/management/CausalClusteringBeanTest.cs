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
namespace Neo4Net.causalclustering.management
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using RaftMachine = Neo4Net.causalclustering.core.consensus.RaftMachine;
	using Role = Neo4Net.causalclustering.core.consensus.roles.Role;
	using ClusterStateDirectory = Neo4Net.causalclustering.core.state.ClusterStateDirectory;
	using EphemeralFileSystemAbstraction = Neo4Net.GraphDb.mockfs.EphemeralFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using ManagementData = Neo4Net.Jmx.impl.ManagementData;
	using ManagementSupport = Neo4Net.Jmx.impl.ManagementSupport;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using KernelData = Neo4Net.Kernel.Internal.KernelData;
	using CausalClustering = Neo4Net.management.CausalClustering;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.log.RaftLog_Fields.RAFT_LOG_DIRECTORY_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.state.machines.CoreStateMachinesModule.ID_ALLOCATION_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.state.machines.CoreStateMachinesModule.LOCK_TOKEN_NAME;

	public class CausalClusteringBeanTest
	{
		private bool InstanceFieldsInitialized = false;

		public CausalClusteringBeanTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_clusterStateDirectory = ClusterStateDirectory.withoutInitializing( _dataDir );
		}

		 private readonly FileSystemAbstraction _fs = new EphemeralFileSystemAbstraction();
		 private readonly File _dataDir = new File( "dataDir" );
		 private ClusterStateDirectory _clusterStateDirectory;
		 private readonly RaftMachine _raftMachine = mock( typeof( RaftMachine ) );
		 private CausalClustering _ccBean;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory testDirectory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  DataSourceManager dataSourceManager = new DataSourceManager( Config.defaults() );
			  NeoStoreDataSource dataSource = mock( typeof( NeoStoreDataSource ) );
			  when( dataSource.DatabaseLayout ).thenReturn( TestDirectory.databaseLayout() );
			  dataSourceManager.Register( dataSource );
			  KernelData kernelData = new KernelData( _fs, mock( typeof( PageCache ) ), new File( "storeDir" ), Config.defaults(), dataSourceManager );

			  Dependencies dependencies = new Dependencies();
			  dependencies.SatisfyDependency( _clusterStateDirectory );
			  dependencies.SatisfyDependency( _raftMachine );
			  dependencies.SatisfyDependency( DatabaseInfo.CORE );

			  when( dataSource.DependencyResolver ).thenReturn( dependencies );
			  ManagementData data = new ManagementData( new CausalClusteringBean(), kernelData, ManagementSupport.load() );

			  _ccBean = ( CausalClustering ) ( new CausalClusteringBean() ).CreateMBean(data);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getCurrentRoleFromRaftMachine()
		 public virtual void getCurrentRoleFromRaftMachine()
		 {
			  when( _raftMachine.currentRole() ).thenReturn(Role.LEADER, Role.FOLLOWER, Role.CANDIDATE);
			  assertEquals( "LEADER", _ccBean.Role );
			  assertEquals( "FOLLOWER", _ccBean.Role );
			  assertEquals( "CANDIDATE", _ccBean.Role );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void returnSumOfRaftLogDirectory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReturnSumOfRaftLogDirectory()
		 {
			  File raftLogDirectory = new File( _clusterStateDirectory.get(), RAFT_LOG_DIRECTORY_NAME );
			  _fs.mkdirs( raftLogDirectory );

			  CreateFileOfSize( new File( raftLogDirectory, "raftLog1" ), 5 );
			  CreateFileOfSize( new File( raftLogDirectory, "raftLog2" ), 10 );

			  assertEquals( 15L, _ccBean.RaftLogSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void excludeRaftLogFromReplicatedStateSize() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ExcludeRaftLogFromReplicatedStateSize()
		 {
			  File stateDir = _clusterStateDirectory.get();

			  // Raft log
			  File raftLogDirectory = new File( stateDir, RAFT_LOG_DIRECTORY_NAME );
			  _fs.mkdirs( raftLogDirectory );
			  CreateFileOfSize( new File( raftLogDirectory, "raftLog1" ), 5 );

			  // Other state
			  File idAllocationDir = new File( stateDir, ID_ALLOCATION_NAME );
			  _fs.mkdirs( idAllocationDir );
			  CreateFileOfSize( new File( idAllocationDir, "state" ), 10 );
			  File lockTokenDir = new File( stateDir, LOCK_TOKEN_NAME );
			  _fs.mkdirs( lockTokenDir );
			  CreateFileOfSize( new File( lockTokenDir, "state" ), 20 );

			  assertEquals( 30L, _ccBean.ReplicatedStateSize );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createFileOfSize(java.io.File file, int size) throws java.io.IOException
		 private void CreateFileOfSize( File file, int size )
		 {
			  using ( StoreChannel storeChannel = _fs.create( file ) )
			  {
					sbyte[] bytes = new sbyte[size];
					ByteBuffer buffer = ByteBuffer.wrap( bytes );
					storeChannel.WriteAll( buffer );
			  }
		 }
	}

}
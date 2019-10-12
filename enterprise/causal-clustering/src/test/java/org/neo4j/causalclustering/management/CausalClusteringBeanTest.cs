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
namespace Org.Neo4j.causalclustering.management
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using RaftMachine = Org.Neo4j.causalclustering.core.consensus.RaftMachine;
	using Role = Org.Neo4j.causalclustering.core.consensus.roles.Role;
	using ClusterStateDirectory = Org.Neo4j.causalclustering.core.state.ClusterStateDirectory;
	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using ManagementData = Org.Neo4j.Jmx.impl.ManagementData;
	using ManagementSupport = Org.Neo4j.Jmx.impl.ManagementSupport;
	using NeoStoreDataSource = Org.Neo4j.Kernel.NeoStoreDataSource;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using DataSourceManager = Org.Neo4j.Kernel.impl.transaction.state.DataSourceManager;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using KernelData = Org.Neo4j.Kernel.@internal.KernelData;
	using CausalClustering = Org.Neo4j.management.CausalClustering;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.log.RaftLog_Fields.RAFT_LOG_DIRECTORY_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.state.machines.CoreStateMachinesModule.ID_ALLOCATION_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.state.machines.CoreStateMachinesModule.LOCK_TOKEN_NAME;

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
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
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
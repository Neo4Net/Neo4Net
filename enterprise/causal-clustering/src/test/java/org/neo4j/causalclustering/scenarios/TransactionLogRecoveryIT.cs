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
namespace Org.Neo4j.causalclustering.scenarios
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Org.Neo4j.causalclustering.discovery;
	using CoreClusterMember = Org.Neo4j.causalclustering.discovery.CoreClusterMember;
	using ReadReplica = Org.Neo4j.causalclustering.discovery.ReadReplica;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using LogEntryWriter = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryWriter;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Org.Neo4j.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using Lifespan = Org.Neo4j.Kernel.Lifecycle.Lifespan;
	using ClusterRule = Org.Neo4j.Test.causalclustering.ClusterRule;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.Cluster.dataMatchesEventually;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.helpers.DataCreator.createEmptyNodes;

	/// <summary>
	/// Recovery scenarios where the transaction log was only partially written.
	/// </summary>
	public class TransactionLogRecoveryIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.PageCacheRule pageCache = new org.neo4j.test.rule.PageCacheRule();
		 public readonly PageCacheRule PageCache = new PageCacheRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(3);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(3);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;
		 private FileSystemAbstraction _fs = new DefaultFileSystemAbstraction();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _cluster = ClusterRule.startCluster();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void coreShouldStartAfterPartialTransactionWriteCrash() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CoreShouldStartAfterPartialTransactionWriteCrash()
		 {
			  // given: a fully synced cluster with some data
			  dataMatchesEventually( createEmptyNodes( _cluster, 10 ), _cluster.coreMembers() );

			  // when: shutting down a core
			  CoreClusterMember core = _cluster.getCoreMemberById( 0 );
			  core.Shutdown();

			  // and making sure there will be something new to pull
			  CoreClusterMember lastWrites = createEmptyNodes( _cluster, 10 );

			  // and writing a partial tx
			  WritePartialTx( core.DatabaseDirectory() );

			  // then: we should still be able to start
			  core.Start();

			  // and become fully synced again
			  dataMatchesEventually( lastWrites, singletonList( core ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void coreShouldStartWithSeedHavingPartialTransactionWriteCrash() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CoreShouldStartWithSeedHavingPartialTransactionWriteCrash()
		 {
			  // given: a fully synced cluster with some data
			  dataMatchesEventually( createEmptyNodes( _cluster, 10 ), _cluster.coreMembers() );

			  // when: shutting down a core
			  CoreClusterMember core = _cluster.getCoreMemberById( 0 );
			  core.Shutdown();

			  // and making sure there will be something new to pull
			  CoreClusterMember lastWrites = createEmptyNodes( _cluster, 10 );

			  // and writing a partial tx
			  WritePartialTx( core.DatabaseDirectory() );

			  // and deleting the cluster state, making sure a snapshot is required during startup
			  // effectively a seeding scenario -- representing the use of the unbind command on a crashed store
			  _fs.deleteRecursively( core.ClusterStateDirectory() );

			  // then: we should still be able to start
			  core.Start();

			  // and become fully synced again
			  dataMatchesEventually( lastWrites, singletonList( core ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readReplicaShouldStartAfterPartialTransactionWriteCrash() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadReplicaShouldStartAfterPartialTransactionWriteCrash()
		 {
			  // given: a fully synced cluster with some data
			  dataMatchesEventually( createEmptyNodes( _cluster, 10 ), _cluster.readReplicas() );

			  // when: shutting down a read replica
			  ReadReplica readReplica = _cluster.getReadReplicaById( 0 );
			  readReplica.Shutdown();

			  // and making sure there will be something new to pull
			  CoreClusterMember lastWrites = createEmptyNodes( _cluster, 10 );
			  dataMatchesEventually( lastWrites, _cluster.coreMembers() );

			  // and writing a partial tx
			  WritePartialTx( readReplica.DatabaseDirectory() );

			  // then: we should still be able to start
			  readReplica.Start();

			  // and become fully synced again
			  dataMatchesEventually( lastWrites, singletonList( readReplica ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writePartialTx(java.io.File storeDir) throws java.io.IOException
		 private void WritePartialTx( File storeDir )
		 {
			  using ( PageCache pageCache = this.PageCache.getPageCache( _fs ) )
			  {
					LogFiles logFiles = LogFilesBuilder.activeFilesBuilder( DatabaseLayout.of( storeDir ), _fs, pageCache ).build();
					using ( Lifespan ignored = new Lifespan( logFiles ) )
					{
						 LogEntryWriter writer = new LogEntryWriter( logFiles.LogFile.Writer );
						 writer.WriteStartEntry( 0, 0, 0x123456789ABCDEFL, logFiles.LogFileInformation.LastEntryId + 1, new sbyte[]{ 0 } );
					}
			  }
		 }
	}

}
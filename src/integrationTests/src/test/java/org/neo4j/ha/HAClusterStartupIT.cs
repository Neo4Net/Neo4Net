﻿using System;

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
namespace Neo4Net.ha
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using Enclosed = org.junit.experimental.runners.Enclosed;
	using RunWith = org.junit.runner.RunWith;


	using ConsistencyCheckIncompleteException = Neo4Net.Consistency.checking.full.ConsistencyCheckIncompleteException;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Result = Neo4Net.GraphDb.Result;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using EnterpriseGraphDatabaseFactory = Neo4Net.GraphDb.factory.EnterpriseGraphDatabaseFactory;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using EnterpriseLoginContext = Neo4Net.Kernel.enterprise.api.security.EnterpriseLoginContext;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.consistency.store.StoreAssertions.assertConsistentStore;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.allSeesAllAsAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.clusterOfSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.EMPTY_MAP;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Enclosed.class) public class HAClusterStartupIT
	public class HAClusterStartupIT
	{
		 public class SimpleCluster
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.ha.ClusterRule clusterRule = new org.Neo4Net.test.ha.ClusterRule().withCluster(clusterOfSize(3));
			  public readonly ClusterRule ClusterRule = new ClusterRule().withCluster(clusterOfSize(3));
			  internal HighlyAvailableGraphDatabase OldMaster;
			  internal HighlyAvailableGraphDatabase OldSlave1;
			  internal HighlyAvailableGraphDatabase OldSlave2;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void Setup()
			  {
					// setup a cluster with some data and entries in log files in fully functional and shutdown state
					ClusterManager.ManagedCluster cluster = ClusterRule.startCluster();

					try
					{
						 cluster.Await( allSeesAllAsAvailable() );

						 OldMaster = cluster.Master;
						 CreateSomeData( OldMaster );
						 cluster.Sync();

						 OldSlave1 = cluster.AnySlave;
						 OldSlave2 = cluster.GetAnySlave( OldSlave1 );
					}
					finally
					{
						 ClusterRule.shutdownCluster();
					}
					AssertAllStoreConsistent( cluster );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allClusterNodesShouldSupportTheBuiltInProcedures() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void AllClusterNodesShouldSupportTheBuiltInProcedures()
			  {
					ClusterManager.ManagedCluster cluster = ClusterRule.startCluster();
					try
					{
						 foreach ( HighlyAvailableGraphDatabase gdb in cluster.AllMembers )
						 {
							  {
							  // (1) BuiltInProcedures from community
									Result result = gdb.Execute( "CALL dbms.procedures()" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
									assertTrue( result.HasNext() );
									result.Close();
							  }

							  // (2) BuiltInProcedures from enterprise
							  using ( InternalTransaction tx = gdb.BeginTransaction( KernelTransaction.Type.@explicit, EnterpriseLoginContext.AUTH_DISABLED ) )
							  {
									Result result = gdb.execute( tx, "CALL dbms.listQueries()", EMPTY_MAP );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
									assertTrue( result.HasNext() );
									result.Close();

									tx.Success();
							  }
						 }
					}
					finally
					{
						 cluster.Shutdown();
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void aSlaveWithoutAnyGraphDBFilesShouldBeAbleToJoinACluster() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ASlaveWithoutAnyGraphDBFilesShouldBeAbleToJoinACluster()
			  {
					// WHEN removing all the files in graphdb on the slave and restarting the cluster
					DeleteAllFilesOn( OldSlave1 );

					// THEN the cluster should work
					RestartingTheClusterShouldWork( ClusterRule );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void bothSlavesWithoutAnyGraphDBFilesShouldBeAbleToJoinACluster() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void BothSlavesWithoutAnyGraphDBFilesShouldBeAbleToJoinACluster()
			  {
					// WHEN removing all the files in graphdb on both slaves and restarting the cluster
					DeleteAllFilesOn( OldSlave1 );
					DeleteAllFilesOn( OldSlave2 );

					// THEN the cluster should work
					RestartingTheClusterShouldWork( ClusterRule );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void theMasterWithoutAnyGraphDBFilesShouldBeAbleToJoinACluster() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void TheMasterWithoutAnyGraphDBFilesShouldBeAbleToJoinACluster()
			  {
					// WHEN removing all the files in graphdb on the db that was master and restarting the cluster
					DeleteAllFilesOn( OldMaster );

					// THEN the cluster should work
					RestartingTheClusterShouldWork( ClusterRule );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void aSlaveWithoutLogicalLogFilesShouldBeAbleToJoinACluster() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ASlaveWithoutLogicalLogFilesShouldBeAbleToJoinACluster()
			  {
					// WHEN removing all logical log files in graphdb on the slave and restarting the cluster
					DeleteAllLogsOn( OldSlave1 );

					// THEN the cluster should work
					RestartingTheClusterShouldWork( ClusterRule );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void bothSlaveWithoutLogicalLogFilesShouldBeAbleToJoinACluster() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void BothSlaveWithoutLogicalLogFilesShouldBeAbleToJoinACluster()
			  {
					// WHEN removing all logical log files in graphdb on the slave and restarting the cluster
					DeleteAllLogsOn( OldSlave1 );
					DeleteAllLogsOn( OldSlave2 );

					// THEN the cluster should work
					RestartingTheClusterShouldWork( ClusterRule );
			  }
		 }

		 public class ClusterWithSeed
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.ha.ClusterRule clusterRule = new org.Neo4Net.test.ha.ClusterRule().withCluster(clusterOfSize(3)).withSeedDir(dbWithOutLogs());
			  public readonly ClusterRule ClusterRule = new ClusterRule().withCluster(clusterOfSize(3)).withSeedDir(DbWithOutLogs());

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ClusterWithSeed() throws java.io.IOException
			  public ClusterWithSeed()
			  {
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void aClusterShouldStartAndRunWhenSeededWithAStoreHavingNoLogicalLogFiles() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void AClusterShouldStartAndRunWhenSeededWithAStoreHavingNoLogicalLogFiles()
			  {
					// WHEN removing all logical log files in graphdb on the slave and restarting a new cluster
					// THEN the new cluster should work
					RestartingTheClusterShouldWork( ClusterRule );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.io.File dbWithOutLogs() throws java.io.IOException
			  internal static File DbWithOutLogs()
			  {
					File seedDir;
					try
					{
						 seedDir = Files.createTempDirectory( "seed-database" ).toFile();
						 seedDir.deleteOnExit();
					}
					catch ( IOException e )
					{
						 throw new Exception( e );
					}

					GraphDatabaseService db = null;
					try
					{
						 db = ( new EnterpriseGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(seedDir).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();
						 CreateSomeData( db );
					}
					finally
					{
						 if ( db != null )
						 {
							  Db.shutdown();
						 }
					}

					DeleteAllLogsOn( seedDir );

					return seedDir;
			  }
		 }

		 private static void CreateSomeData( IGraphDatabaseService oldMaster )
		 {
			  using ( Transaction tx = oldMaster.BeginTx() )
			  {
					oldMaster.CreateNode();
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void deleteAllFilesOn(org.Neo4Net.kernel.ha.HighlyAvailableGraphDatabase instance) throws java.io.IOException
		 private static void DeleteAllFilesOn( HighlyAvailableGraphDatabase instance )
		 {
			  FileUtils.deleteRecursively( instance.DatabaseLayout().databaseDirectory() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void deleteAllLogsOn(org.Neo4Net.kernel.ha.HighlyAvailableGraphDatabase instance) throws java.io.IOException
		 private static void DeleteAllLogsOn( HighlyAvailableGraphDatabase instance )
		 {
			  DeleteAllLogsOn( instance.DatabaseLayout().databaseDirectory() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void deleteAllLogsOn(java.io.File storeDirectory) throws java.io.IOException
		 private static void DeleteAllLogsOn( File storeDirectory )
		 {
			  using ( DefaultFileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction() )
			  {
					LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( storeDirectory, fileSystem ).build();
					foreach ( File file in logFiles.LogFilesConflict() )
					{
						 fileSystem.DeleteFile( file );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void restartingTheClusterShouldWork(org.Neo4Net.test.ha.ClusterRule clusterRule) throws Exception
		 private static void RestartingTheClusterShouldWork( ClusterRule clusterRule )
		 {
			  ClusterManager.ManagedCluster cluster = clusterRule.StartCluster();
			  try
			  {
					cluster.Await( allSeesAllAsAvailable(), 180 );
			  }
			  finally
			  {
					clusterRule.ShutdownCluster();
			  }

			  AssertAllStoreConsistent( cluster );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void assertAllStoreConsistent(org.Neo4Net.kernel.impl.ha.ClusterManager.ManagedCluster cluster) throws org.Neo4Net.consistency.checking.full.ConsistencyCheckIncompleteException
		 private static void AssertAllStoreConsistent( ClusterManager.ManagedCluster cluster )
		 {
			  foreach ( HighlyAvailableGraphDatabase slave in cluster.AllMembers )
			  {
					assertConsistentStore( slave.DatabaseLayout() );
			  }
		 }
	}

}
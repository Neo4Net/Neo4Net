﻿/*
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
namespace Org.Neo4j.causalclustering.core.state
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class ClusterStateDirectoryTest
	{
		private bool InstanceFieldsInitialized = false;

		public ClusterStateDirectoryTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_fs = FsRule.get();
			TestDirectory = TestDirectory.testDirectory( _fs );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.fs.DefaultFileSystemRule fsRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public DefaultFileSystemRule FsRule = new DefaultFileSystemRule();
		 private FileSystemAbstraction _fs;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory(fs);
		 public TestDirectory TestDirectory;

		 private File _dataDir;
		 private File _stateDir;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _dataDir = TestDirectory.directory( "data" );
			  _stateDir = new File( _dataDir, ClusterStateDirectory.CLUSTER_STATE_DIRECTORY_NAME );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMigrateClusterStateFromStoreDir() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMigrateClusterStateFromStoreDir()
		 {
			  // given
			  File storeDir = new File( new File( _dataDir, "databases" ), GraphDatabaseSettings.DEFAULT_DATABASE_NAME );

			  string fileName = "file";

			  File oldStateDir = new File( storeDir, ClusterStateDirectory.CLUSTER_STATE_DIRECTORY_NAME );
			  File oldClusterStateFile = new File( oldStateDir, fileName );

			  _fs.mkdirs( oldStateDir );
			  _fs.create( oldClusterStateFile ).close();

			  // when
			  ClusterStateDirectory clusterStateDirectory = new ClusterStateDirectory( _dataDir, storeDir, false );
			  clusterStateDirectory.Initialize( _fs );

			  // then
			  assertEquals( clusterStateDirectory.Get(), _stateDir );
			  assertTrue( _fs.fileExists( new File( clusterStateDirectory.Get(), fileName ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleCaseOfStoreDirBeingDataDir() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleCaseOfStoreDirBeingDataDir()
		 {
			  // given
			  File storeDir = _dataDir;

			  string fileName = "file";

			  File oldStateDir = new File( storeDir, ClusterStateDirectory.CLUSTER_STATE_DIRECTORY_NAME );
			  File oldClusterStateFile = new File( oldStateDir, fileName );

			  _fs.mkdirs( oldStateDir );
			  _fs.create( oldClusterStateFile ).close();

			  // when
			  ClusterStateDirectory clusterStateDirectory = new ClusterStateDirectory( _dataDir, storeDir, false );
			  clusterStateDirectory.Initialize( _fs );

			  // then
			  assertEquals( clusterStateDirectory.Get(), _stateDir );
			  assertTrue( _fs.fileExists( new File( clusterStateDirectory.Get(), fileName ) ) );
		 }
	}

}
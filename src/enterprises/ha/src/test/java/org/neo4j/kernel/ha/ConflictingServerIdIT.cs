using System;

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
namespace Neo4Net.Kernel.ha
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using TestHighlyAvailableGraphDatabaseFactory = Neo4Net.Graphdb.factory.TestHighlyAvailableGraphDatabaseFactory;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class ConflictingServerIdIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testConflictingIdDoesNotSilentlyFail()
		 public virtual void TestConflictingIdDoesNotSilentlyFail()
		 {
			  HighlyAvailableGraphDatabase master = null;
			  HighlyAvailableGraphDatabase dbWithId21 = null;
			  HighlyAvailableGraphDatabase dbWithId22 = null;
			  try
			  {

					int masterClusterPort = PortAuthority.allocatePort();

					GraphDatabaseBuilder masterBuilder = ( new TestHighlyAvailableGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(Path(1)).setConfig(ClusterSettings.initial_hosts, "127.0.0.1:" + masterClusterPort).setConfig(ClusterSettings.cluster_server, "127.0.0.1:" + masterClusterPort).setConfig(ClusterSettings.server_id, "" + 1).setConfig(HaSettings.HaServer, ":" + PortAuthority.allocatePort()).setConfig(HaSettings.TxPushFactor, "0").setConfig(OnlineBackupSettings.online_backup_enabled, false.ToString());
					master = ( HighlyAvailableGraphDatabase ) masterBuilder.NewGraphDatabase();

					GraphDatabaseBuilder db21Builder = ( new TestHighlyAvailableGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(Path(2)).setConfig(ClusterSettings.initial_hosts, "127.0.0.1:" + masterClusterPort).setConfig(ClusterSettings.cluster_server, "127.0.0.1:" + PortAuthority.allocatePort()).setConfig(ClusterSettings.server_id, "" + 2).setConfig(HaSettings.HaServer, ":" + PortAuthority.allocatePort()).setConfig(HaSettings.TxPushFactor, "0").setConfig(OnlineBackupSettings.online_backup_enabled, false.ToString());
					dbWithId21 = ( HighlyAvailableGraphDatabase ) db21Builder.NewGraphDatabase();

					GraphDatabaseBuilder db22Builder = ( new TestHighlyAvailableGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(Path(3)).setConfig(ClusterSettings.initial_hosts, "127.0.0.1:" + masterClusterPort).setConfig(ClusterSettings.cluster_server, "127.0.0.1:" + PortAuthority.allocatePort()).setConfig(ClusterSettings.server_id, "" + 2).setConfig(HaSettings.HaServer, ":" + PortAuthority.allocatePort()).setConfig(HaSettings.TxPushFactor, "0").setConfig(OnlineBackupSettings.online_backup_enabled, false.ToString());

					try
					{
						 dbWithId22 = ( HighlyAvailableGraphDatabase ) db22Builder.NewGraphDatabase();
						 fail( "Should not be able to startup when a cluster already has my id" );
					}
					catch ( Exception )
					{
						 // awesome
					}

					assertTrue( master.Master );
					assertTrue( !dbWithId21.Master );

					using ( Transaction transaction = dbWithId21.BeginTx() )
					{
						 transaction.Success();
					}
			  }
			  finally
			  {
					if ( dbWithId21 != null )
					{
						 dbWithId21.Shutdown();
					}
					if ( dbWithId22 != null )
					{
						 dbWithId22.Shutdown();
					}
					if ( master != null )
					{
						 master.Shutdown();
					}
			  }
		 }

		 private File Path( int i )
		 {
			  return TestDirectory.databaseDir( "" + i );
		 }
	}

}
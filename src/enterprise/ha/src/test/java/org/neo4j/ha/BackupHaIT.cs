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
namespace Neo4Net.ha
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using ManagedCluster = Neo4Net.Kernel.impl.ha.ClusterManager.ManagedCluster;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using DbRepresentation = Neo4Net.Test.DbRepresentation;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.ClusterHelper.createSomeData;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.util.TestHelpers.runBackupToolFromOtherJvmToGetExitCode;

	public class BackupHaIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.ha.ClusterRule clusterRule = new Neo4Net.test.ha.ClusterRule().withSharedSetting(Neo4Net.kernel.impl.enterprise.configuration.OnlineBackupSettings.online_backup_enabled, Neo4Net.kernel.configuration.Settings.TRUE).withInstanceSetting(Neo4Net.kernel.impl.enterprise.configuration.OnlineBackupSettings.online_backup_server, serverId -> ":" + Neo4Net.ports.allocation.PortAuthority.allocatePort());
		 public ClusterRule ClusterRule = new ClusterRule().withSharedSetting(OnlineBackupSettings.online_backup_enabled, Settings.TRUE).withInstanceSetting(OnlineBackupSettings.online_backup_server, serverId => ":" + PortAuthority.allocatePort());
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.SuppressOutput suppressOutput = Neo4Net.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

		 private File _backupPath;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _backupPath = ClusterRule.TestDirectory.storeDir( "backup-db" );
			  createSomeData( ClusterRule.startCluster().Master );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureBackupCanBePerformed() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MakeSureBackupCanBePerformed()
		 {
			  // Run backup
			  ManagedCluster cluster = ClusterRule.startCluster();
			  DbRepresentation beforeChange = DbRepresentation.of( cluster.Master );
			  HighlyAvailableGraphDatabase hagdb = cluster.AllMembers.GetEnumerator().next();
			  HostnamePort address = cluster.GetBackupAddress( hagdb );
			  string databaseName = "basic";
			  assertEquals( 0, runBackupToolFromOtherJvmToGetExitCode( _backupPath, BackupArguments( address.ToString(), _backupPath, databaseName ) ) );

			  // Add some new data
			  DbRepresentation afterChange = createSomeData( cluster.Master );
			  cluster.Sync();

			  // Verify that backed up database can be started and compare representation
			  Config config = Config.builder().withSetting(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).withSetting(GraphDatabaseSettings.active_database, databaseName).build();
			  DbRepresentation backupRepresentation = DbRepresentation.of( DatabaseLayout.of( _backupPath, databaseName ).databaseDirectory(), config );
			  assertEquals( beforeChange, backupRepresentation );
			  assertNotEquals( backupRepresentation, afterChange );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureBackupCanBePerformedFromAnyInstance() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MakeSureBackupCanBePerformedFromAnyInstance()
		 {
			  ManagedCluster cluster = ClusterRule.startCluster();

			  foreach ( HighlyAvailableGraphDatabase hagdb in cluster.AllMembers )
			  {
					HostnamePort address = cluster.GetBackupAddress( hagdb );

					// Run backup
					DbRepresentation beforeChange = DbRepresentation.of( cluster.Master );
					string databaseName = "anyinstance";
					assertEquals( 0, runBackupToolFromOtherJvmToGetExitCode( _backupPath, BackupArguments( address.ToString(), _backupPath, databaseName ) ) );

					// Add some new data
					DbRepresentation afterChange = createSomeData( cluster.Master );
					cluster.Sync();

					// Verify that old data is back
					Config config = Config.builder().withSetting(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).withSetting(GraphDatabaseSettings.active_database, databaseName).build();
					DbRepresentation backupRepresentation = DbRepresentation.of( DatabaseLayout.of( _backupPath, databaseName ).databaseDirectory(), config );
					assertEquals( beforeChange, backupRepresentation );
					assertNotEquals( backupRepresentation, afterChange );
			  }
		 }

		 private static string[] BackupArguments( string from, File backupDir, string databaseName )
		 {
			  IList<string> args = new List<string>();
			  args.Add( "--from=" + from );
			  args.Add( "--cc-report-dir=" + backupDir );
			  args.Add( "--backup-dir=" + backupDir );
			  args.Add( "--name=" + databaseName );
			  return args.ToArray();
		 }
	}

}
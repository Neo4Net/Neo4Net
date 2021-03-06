﻿using System.Collections.Generic;

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
namespace Org.Neo4j.ha
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using HostnamePort = Org.Neo4j.Helpers.HostnamePort;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using HighlyAvailableGraphDatabase = Org.Neo4j.Kernel.ha.HighlyAvailableGraphDatabase;
	using OnlineBackupSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using ManagedCluster = Org.Neo4j.Kernel.impl.ha.ClusterManager.ManagedCluster;
	using PortAuthority = Org.Neo4j.Ports.Allocation.PortAuthority;
	using DbRepresentation = Org.Neo4j.Test.DbRepresentation;
	using ClusterRule = Org.Neo4j.Test.ha.ClusterRule;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.ClusterHelper.createSomeData;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.TestHelpers.runBackupToolFromOtherJvmToGetExitCode;

	public class BackupHaIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.ha.ClusterRule clusterRule = new org.neo4j.test.ha.ClusterRule().withSharedSetting(org.neo4j.kernel.impl.enterprise.configuration.OnlineBackupSettings.online_backup_enabled, org.neo4j.kernel.configuration.Settings.TRUE).withInstanceSetting(org.neo4j.kernel.impl.enterprise.configuration.OnlineBackupSettings.online_backup_server, serverId -> ":" + org.neo4j.ports.allocation.PortAuthority.allocatePort());
		 public ClusterRule ClusterRule = new ClusterRule().withSharedSetting(OnlineBackupSettings.online_backup_enabled, Settings.TRUE).withInstanceSetting(OnlineBackupSettings.online_backup_server, serverId => ":" + PortAuthority.allocatePort());
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
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
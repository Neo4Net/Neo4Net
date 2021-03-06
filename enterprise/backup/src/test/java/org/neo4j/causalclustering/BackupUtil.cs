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
namespace Org.Neo4j.causalclustering
{

	using Org.Neo4j.causalclustering.discovery;
	using CoreClusterMember = Org.Neo4j.causalclustering.discovery.CoreClusterMember;
	using CommandFailed = Org.Neo4j.Commandline.admin.CommandFailed;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using RestoreDatabaseCommand = Org.Neo4j.restore.RestoreDatabaseCommand;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.BackupCoreIT.backupArguments;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.TestHelpers.runBackupToolFromOtherJvmToGetExitCode;

	public class BackupUtil
	{
		 private static string BackupAddress( CoreClusterMember core )
		 {
			  return core.SettingValue( "causal_clustering.transaction_listen_address" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.io.File createBackupFromCore(org.neo4j.causalclustering.discovery.CoreClusterMember core, String backupName, java.io.File baseBackupDir) throws Exception
		 public static File CreateBackupFromCore( CoreClusterMember core, string backupName, File baseBackupDir )
		 {
			  string[] args = backupArguments( BackupAddress( core ), baseBackupDir, backupName );
			  assertEquals( 0, runBackupToolFromOtherJvmToGetExitCode( baseBackupDir, args ) );
			  return new File( baseBackupDir, backupName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void restoreFromBackup(java.io.File backup, org.neo4j.io.fs.FileSystemAbstraction fsa, org.neo4j.causalclustering.discovery.ClusterMember clusterMember) throws java.io.IOException, org.neo4j.commandline.admin.CommandFailed
		 public static void RestoreFromBackup( File backup, FileSystemAbstraction fsa, ClusterMember clusterMember )
		 {
			  Config config = clusterMember.config();
			  RestoreDatabaseCommand restoreDatabaseCommand = new RestoreDatabaseCommand( fsa, backup, config, GraphDatabaseSettings.DEFAULT_DATABASE_NAME, true );
			  restoreDatabaseCommand.Execute();
		 }
	}

}
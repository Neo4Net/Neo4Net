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
namespace Neo4Net.causalclustering
{

	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using CommandFailed = Neo4Net.CommandLine.Admin.CommandFailed;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using RestoreDatabaseCommand = Neo4Net.restore.RestoreDatabaseCommand;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.BackupCoreIT.backupArguments;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.util.TestHelpers.runBackupToolFromOtherJvmToGetExitCode;

	public class BackupUtil
	{
		 private static string BackupAddress( CoreClusterMember core )
		 {
			  return core.SettingValue( "causal_clustering.transaction_listen_address" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.io.File createBackupFromCore(Neo4Net.causalclustering.discovery.CoreClusterMember core, String backupName, java.io.File baseBackupDir) throws Exception
		 public static File CreateBackupFromCore( CoreClusterMember core, string backupName, File baseBackupDir )
		 {
			  string[] args = backupArguments( BackupAddress( core ), baseBackupDir, backupName );
			  assertEquals( 0, runBackupToolFromOtherJvmToGetExitCode( baseBackupDir, args ) );
			  return new File( baseBackupDir, backupName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void restoreFromBackup(java.io.File backup, Neo4Net.io.fs.FileSystemAbstraction fsa, Neo4Net.causalclustering.discovery.ClusterMember clusterMember) throws java.io.IOException, Neo4Net.commandline.admin.CommandFailed
		 public static void RestoreFromBackup( File backup, FileSystemAbstraction fsa, ClusterMember clusterMember )
		 {
			  Config config = clusterMember.config();
			  RestoreDatabaseCommand restoreDatabaseCommand = new RestoreDatabaseCommand( fsa, backup, config, GraphDatabaseSettings.DEFAULT_DATABASE_NAME, true );
			  restoreDatabaseCommand.Execute();
		 }
	}

}
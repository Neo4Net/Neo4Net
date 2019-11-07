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
namespace Neo4Net.causalclustering
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using CoreGraphDatabase = Neo4Net.causalclustering.core.CoreGraphDatabase;
	using Role = Neo4Net.causalclustering.core.consensus.roles.Role;
	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using Node = Neo4Net.GraphDb.Node;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using MapUtil = Neo4Net.Collections.Helpers.MapUtil;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using DbRepresentation = Neo4Net.Test.DbRepresentation;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.assertion.Assert.assertEventually;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.util.TestHelpers.runBackupToolFromOtherJvmToGetExitCode;

	public class BackupCoreIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.SuppressOutput suppressOutput = Neo4Net.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.causalclustering.ClusterRule clusterRule = new Neo4Net.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private Neo4Net.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;
		 private File _backupsDir;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _backupsDir = ClusterRule.testDirectory().cleanDirectory("backups");
			  _cluster = ClusterRule.startCluster();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureBackupCanBePerformedFromAnyInstance() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MakeSureBackupCanBePerformedFromAnyInstance()
		 {
			  foreach ( CoreClusterMember db in _cluster.coreMembers() )
			  {
					// Run backup
					DbRepresentation beforeChange = DbRepresentation.of( CreateSomeData( _cluster ) );
					string[] args = BackupArguments( BackupAddress( _cluster ), _backupsDir, "" + Db.serverId() );
					assertEventually( () => runBackupToolFromOtherJvmToGetExitCode(ClusterRule.clusterDirectory(), args), equalTo(0), 5, TimeUnit.SECONDS );

					// Add some new data
					DbRepresentation afterChange = DbRepresentation.of( CreateSomeData( _cluster ) );

					// Verify that old data is back
					DbRepresentation backupRepresentation = DbRepresentation.of( DatabaseLayout.of( _backupsDir, "" + Db.serverId() ).databaseDirectory(), Config );
					assertEquals( beforeChange, backupRepresentation );
					assertNotEquals( backupRepresentation, afterChange );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static Neo4Net.causalclustering.core.CoreGraphDatabase createSomeData(Neo4Net.causalclustering.discovery.Cluster<?> cluster) throws Exception
		 internal static CoreGraphDatabase CreateSomeData<T1>( Cluster<T1> cluster )
		 {
			  return cluster.CoreTx((db, tx) =>
			  {
				Node node = Db.createNode( label( "boo" ) );
				node.setProperty( "foobar", "baz_bat" );
				tx.success();
			  }).database();
		 }

		 private static string BackupAddress<T1>( Cluster<T1> cluster )
		 {
			  return cluster.GetMemberWithRole( Role.LEADER ).settingValue( "causal_clustering.transaction_listen_address" );
		 }

		 internal static string[] BackupArguments( string from, File backupsDir, string name )
		 {
			  IList<string> args = new List<string>();
			  args.Add( "--from=" + from );
			  args.Add( "--cc-report-dir=" + backupsDir );
			  args.Add( "--backup-dir=" + backupsDir );
			  args.Add( "--protocol=catchup" );
			  args.Add( "--name=" + name );
			  return args.ToArray();
		 }

		 internal static Config Config
		 {
			 get
			 {
				  IDictionary<string, string> config = MapUtil.stringMap( GraphDatabaseSettings.record_format.name(), Standard.LATEST_NAME, OnlineBackupSettings.online_backup_server.name(), "127.0.0.1:" + PortAuthority.allocatePort() );
   
				  return Config.defaults( config );
			 }
		 }
	}

}
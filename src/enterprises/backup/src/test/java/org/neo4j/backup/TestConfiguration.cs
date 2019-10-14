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
namespace Neo4Net.backup
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class TestConfiguration
	{
		private bool InstanceFieldsInitialized = false;

		public TestConfiguration()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Rules = RuleChain.outerRule( Dir ).around( SuppressOutput );
		}

		 public SuppressOutput SuppressOutput = SuppressOutput.suppressAll();
		 public TestDirectory Dir = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain rules = org.junit.rules.RuleChain.outerRule(dir).around(suppressOutput);
		 public RuleChain Rules;

		 private const string HOST_ADDRESS = "127.0.0.1";

		 private File _storeDir;
		 private string _backupDir;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Before()
		 {
			  _storeDir = Dir.databaseDir();
			  _backupDir = Dir.cleanDirectory( "full-backup" ).AbsolutePath;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testOnByDefault()
		 public virtual void TestOnByDefault()
		 {
			  int port = PortAuthority.allocatePort();

			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(_storeDir).setConfig(OnlineBackupSettings.online_backup_server, "localhost:" + port).newGraphDatabase();
			  OnlineBackup.From( HOST_ADDRESS, port ).full( _backupDir );
			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testOffByConfig()
		 public virtual void TestOffByConfig()
		 {
			  int port = PortAuthority.allocatePort();

			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(_storeDir).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).setConfig(OnlineBackupSettings.online_backup_server, "localhost:" + port).newGraphDatabase();
			  try
			  {
					OnlineBackup.From( HOST_ADDRESS, port ).full( _backupDir );
					fail( "Shouldn't be possible" );
			  }
			  catch ( Exception )
			  { // Good
			  }
			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testEnableDefaultsInConfig()
		 public virtual void TestEnableDefaultsInConfig()
		 {
			  int port = PortAuthority.allocatePort();

			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(_storeDir).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.TRUE).setConfig(OnlineBackupSettings.online_backup_server, "localhost:" + port).newGraphDatabase();

			  OnlineBackup.From( HOST_ADDRESS, port ).full( _backupDir );
			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testEnableCustomPortInConfig()
		 public virtual void TestEnableCustomPortInConfig()
		 {
			  int customPort = PortAuthority.allocatePort();
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(_storeDir).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.TRUE).setConfig(OnlineBackupSettings.online_backup_server, ":" + customPort).newGraphDatabase();
			  try
			  {
					OnlineBackup.From( HOST_ADDRESS, PortAuthority.allocatePort() ).full(_backupDir);
					fail( "Shouldn't be possible" );
			  }
			  catch ( Exception )
			  { // Good
			  }

			  OnlineBackup.From( HOST_ADDRESS, customPort ).full( _backupDir );
			  Db.shutdown();
		 }
	}

}
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
namespace Neo4Net.Index
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using TestEnterpriseGraphDatabaseFactory = Neo4Net.Test.TestEnterpriseGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

	public class StartOnExistingDbWithIndexIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void startStopDatabaseWithIndex()
		 public virtual void StartStopDatabaseWithIndex()
		 {
			  Label label = Label.label( "string" );
			  string property = "property";
			  AssertableLogProvider logProvider = new AssertableLogProvider( true );
			  GraphDatabaseService db = PrepareDb( label, property, logProvider );
			  Db.shutdown();
			  db = GetDatabase( logProvider );
			  Db.shutdown();

			  logProvider.FormattedMessageMatcher().assertNotContains("Failed to open index");
		 }

		 private GraphDatabaseService PrepareDb( Label label, string propertyName, LogProvider logProvider )
		 {
			  GraphDatabaseService db = GetDatabase( logProvider );
			  using ( Transaction transaction = Db.beginTx() )
			  {
					Db.schema().constraintFor(label).assertPropertyIsUnique(propertyName).create();
					transaction.Success();
			  }
			  WaitIndexes( db );
			  return db;
		 }

		 private GraphDatabaseService GetDatabase( LogProvider logProvider )
		 {
			  return ( new TestEnterpriseGraphDatabaseFactory() ).setInternalLogProvider(logProvider).newEmbeddedDatabaseBuilder(TestDirectory.storeDir()).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();
		 }

		 private static void WaitIndexes( GraphDatabaseService db )
		 {
			  using ( Transaction transaction = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(5, TimeUnit.SECONDS);
					transaction.Success();
			  }
		 }
	}

}
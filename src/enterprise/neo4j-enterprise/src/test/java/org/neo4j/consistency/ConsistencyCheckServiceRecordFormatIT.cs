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
namespace Neo4Net.Consistency
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameter = org.junit.runners.Parameterized.Parameter;
	using Parameters = org.junit.runners.Parameterized.Parameters;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using HighLimit = Neo4Net.Kernel.impl.store.format.highlimit.HighLimit;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class ConsistencyCheckServiceRecordFormatIT
	public class ConsistencyCheckServiceRecordFormatIT
	{
		private bool InstanceFieldsInitialized = false;

		public ConsistencyCheckServiceRecordFormatIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( SuppressOutput.suppressAll() ).around(_db);
		}

		 private readonly DatabaseRule _db = new EmbeddedDatabaseRule().withSetting(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).startLazily();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(Neo4Net.test.rule.SuppressOutput.suppressAll()).around(db);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameter public String recordFormat;
		 public string RecordFormat;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters(name = "{0}") public static java.util.List<String> recordFormats()
		 public static IList<string> RecordFormats()
		 {
			  return Arrays.asList( Standard.LATEST_NAME, HighLimit.NAME );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void configureRecordFormat()
		 public virtual void ConfigureRecordFormat()
		 {
			  _db.withSetting( GraphDatabaseSettings.record_format, RecordFormat );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void checkTinyConsistentStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CheckTinyConsistentStore()
		 {
			  _db.ensureStarted();
			  CreateLinkedList( _db, 1_000 );
			  _db.shutdownAndKeepStore();

			  AssertConsistentStore( _db );
		 }

		 private static void CreateLinkedList( IGraphDatabaseService db, int size )
		 {
			  Node previous = null;
			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 0; i < size; i++ )
					{
						 Label label = ( i % 2 == 0 ) ? TestLabel.Foo : TestLabel.Bar;
						 Node current = Db.createNode( label );
						 current.SetProperty( "value", ThreadLocalRandom.current().nextLong() );

						 if ( previous != null )
						 {
							  previous.CreateRelationshipTo( current, TestRelType.Forward );
							  current.CreateRelationshipTo( previous, TestRelType.Backward );
						 }
						 previous = current;
					}
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void assertConsistentStore(Neo4Net.kernel.internal.GraphDatabaseAPI db) throws Exception
		 private static void AssertConsistentStore( GraphDatabaseAPI db )
		 {
			  ConsistencyCheckService service = new ConsistencyCheckService();

			  ConsistencyCheckService.Result result = service.RunFullConsistencyCheck( Db.databaseLayout(), Config.defaults(), ProgressMonitorFactory.textual(System.out), FormattedLogProvider.toOutputStream(System.out), true );

			  assertTrue( "Store is inconsistent", result.Successful );
		 }

		 private enum TestLabel
		 {
			  Foo,
			  Bar
		 }

		 private enum TestRelType
		 {
			  Forward,
			  Backward
		 }
	}

}
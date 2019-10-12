using System;
using System.Collections.Generic;
using System.Threading;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Schema
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using ConsistencyCheckService = Org.Neo4j.Consistency.ConsistencyCheckService;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using ProgressMonitorFactory = Org.Neo4j.Helpers.progress.ProgressMonitorFactory;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using FormattedLogProvider = Org.Neo4j.Logging.FormattedLogProvider;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using RandomValues = Org.Neo4j.Values.Storable.RandomValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.DoubleLatch.awaitLatch;

	public class DynamicIndexStoreViewIT
	{
		private bool InstanceFieldsInitialized = false;

		public DynamicIndexStoreViewIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _testDirectory ).around( _suppressOutput );
		}

		 private readonly SuppressOutput _suppressOutput = SuppressOutput.suppressAll();
		 private readonly TestDirectory _testDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(testDirectory).around(suppressOutput);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void populateDbWithConcurrentUpdates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PopulateDbWithConcurrentUpdates()
		 {
			  GraphDatabaseService database = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(_testDirectory.databaseDir());
			  try
			  {
					RandomValues randomValues = RandomValues.create();
					int counter = 1;
					for ( int j = 0; j < 100; j++ )
					{
						 using ( Transaction transaction = database.BeginTx() )
						 {
							  for ( int i = 0; i < 5; i++ )
							  {
									Node node = database.CreateNode( Label.label( "label" + counter ) );
									node.SetProperty( "property", randomValues.NextValue().asObject() );
							  }
							  transaction.Success();
						 }
						 counter++;
					}

					int populatorCount = 5;
					ExecutorService executor = Executors.newFixedThreadPool( populatorCount );
					System.Threading.CountdownEvent startSignal = new System.Threading.CountdownEvent( 1 );
					AtomicBoolean endSignal = new AtomicBoolean();
					for ( int i = 0; i < populatorCount; i++ )
					{
						 executor.submit( new Populator( this, database, counter, startSignal, endSignal ) );
					}

					try
					{
						 using ( Transaction transaction = database.BeginTx() )
						 {
							  database.Schema().indexFor(Label.label("label10")).on("property").create();
							  transaction.Success();
						 }
						 startSignal.Signal();

						 using ( Transaction transaction = database.BeginTx() )
						 {
							  database.Schema().awaitIndexesOnline(populatorCount, TimeUnit.MINUTES);
							  transaction.Success();
						 }
					}
					finally
					{
						 endSignal.set( true );
						 executor.shutdown();
						 // Basically we don't care to await their completion because they've done their job
					}
			  }
			  finally
			  {
					database.Shutdown();
					ConsistencyCheckService consistencyCheckService = new ConsistencyCheckService();
					Config config = Config.defaults( GraphDatabaseSettings.pagecache_memory, "8m" );
					consistencyCheckService.RunFullConsistencyCheck( _testDirectory.databaseLayout(), config, ProgressMonitorFactory.NONE, FormattedLogProvider.toOutputStream(System.out), false );
			  }
		 }

		 private class Populator : ThreadStart
		 {
			 private readonly DynamicIndexStoreViewIT _outerInstance;

			  internal readonly GraphDatabaseService DatabaseService;
			  internal readonly long TotalNodes;
			  internal readonly System.Threading.CountdownEvent StartSignal;
			  internal readonly AtomicBoolean EndSignal;

			  internal Populator( DynamicIndexStoreViewIT outerInstance, GraphDatabaseService databaseService, long totalNodes, System.Threading.CountdownEvent startSignal, AtomicBoolean endSignal )
			  {
				  this._outerInstance = outerInstance;
					this.DatabaseService = databaseService;
					this.TotalNodes = totalNodes;
					this.StartSignal = startSignal;
					this.EndSignal = endSignal;
			  }

			  public override void Run()
			  {
					RandomValues randomValues = RandomValues.create();
					awaitLatch( StartSignal );
					while ( !EndSignal.get() )
					{
						 using ( Transaction transaction = DatabaseService.beginTx() )
						 {
							  try
							  {
									int operationType = randomValues.NextIntValue( 3 ).value();
									switch ( operationType )
									{
									case 0:
										 long targetNodeId = randomValues.NextLongValue( TotalNodes ).value();
										 DatabaseService.getNodeById( targetNodeId ).delete();
										 break;
									case 1:
										 long nodeId = randomValues.NextLongValue( TotalNodes ).value();
										 Node node = DatabaseService.getNodeById( nodeId );
										 IDictionary<string, object> allProperties = node.AllProperties;
										 foreach ( string key in allProperties.Keys )
										 {
											  node.SetProperty( key, randomValues.NextValue().asObject() );
										 }
										 break;
									case 2:
										 Node nodeToUpdate = DatabaseService.createNode( Label.label( "label10" ) );
										 nodeToUpdate.SetProperty( "property", randomValues.NextValue().asObject() );
										 break;
									default:
										 throw new System.NotSupportedException( "Unknown type of index operation" );
									}
									transaction.Success();
							  }
							  catch ( Exception )
							  {
									transaction.Failure();
							  }
						 }
					}
			  }
		 }
	}

}
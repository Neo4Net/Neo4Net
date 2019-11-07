using System.Threading;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using AfterClass = org.junit.AfterClass;
	using BeforeClass = org.junit.BeforeClass;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.Kernel.Impl.Index.Schema;
	using GenericNativeIndexPopulator = Neo4Net.Kernel.Impl.Index.Schema.GenericNativeIndexPopulator;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;
	using FeatureToggles = Neo4Net.Utils.FeatureToggles;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.ByteUnit.kibiBytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.BlockBasedIndexPopulator.BLOCK_SIZE_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.GenericNativeIndexProvider.BLOCK_BASED_POPULATION_NAME;

	public class BlockBasedIndexPopulationMemoryUsageIT
	{
		 private static readonly long _testBlockSize = kibiBytes( 64 );
		 private static readonly string[] _keys = new string[] { "key1", "key2", "key3", "key4" };
		 private static readonly Label[] _labels = new Label[] { label( "Label1" ), label( "Label2" ), label( "Label3" ), label( "Label4" ) };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.EmbeddedDatabaseRule db = new Neo4Net.test.rule.EmbeddedDatabaseRule();
		 public readonly EmbeddedDatabaseRule Db = new EmbeddedDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setUpFeatureToggles()
		 public static void SetUpFeatureToggles()
		 {
			  // Configure populator so that it will use block-based population and reduce batch size and increase number of workers
			  // so that population will very likely create more batches in more threads (affecting number of buffers used)
			  FeatureToggles.set( typeof( GenericNativeIndexPopulator ), BLOCK_BASED_POPULATION_NAME, true );
			  FeatureToggles.set( typeof( BlockBasedIndexPopulator ), BLOCK_SIZE_NAME, _testBlockSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void restoreFeatureToggles()
		 public static void RestoreFeatureToggles()
		 {
			  FeatureToggles.clear( typeof( GenericNativeIndexPopulator ), BLOCK_BASED_POPULATION_NAME );
			  FeatureToggles.clear( typeof( BlockBasedIndexPopulator ), BLOCK_SIZE_NAME );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepMemoryConsumptionLowDuringPopulation() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKeepMemoryConsumptionLowDuringPopulation()
		 {
			  // given
			  IndexPopulationMemoryUsageMonitor monitor = new IndexPopulationMemoryUsageMonitor();
			  Db.DependencyResolver.resolveDependency( typeof( Monitors ) ).addMonitorListener( monitor );
			  SomeData();

			  // when
			  CreateLotsOfIndexesInOneTransaction();
			  monitor.Called.await();

			  // then all in all the peak memory usage with the introduction of the more sophisticated ByteBufferFactory
			  // given all parameters of data size, number of workers and number of indexes will amount
			  // to a maximum of 10 MiB. Previously this would easily be 10-fold of that for this scenario.
			  long targetMemoryConsumption = _testBlockSize * ( 8 + 1 ) * 8;
			  assertThat( monitor.PeakDirectMemoryUsage, lessThan( targetMemoryConsumption + 1 ) );
		 }

		 private void CreateLotsOfIndexesInOneTransaction()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( Label label in _labels )
					{
						 foreach ( string key in _keys )
						 {
							  Db.schema().indexFor(label).on(key).create();
						 }
					}
					tx.Success();
			  }
			  while ( true )
			  {
					try
					{
							using ( Transaction tx = Db.beginTx() )
							{
							 Db.schema().awaitIndexesOnline(1, SECONDS);
							 break;
							}
					}
					catch ( System.InvalidOperationException )
					{
						 // Just wait longer
						 try
						 {
							  Thread.Sleep( 100 );
						 }
						 catch ( InterruptedException )
						 {
							  // Not sure we can do anything about this, other than just break this loop
							  break;
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void someData() throws InterruptedException
		 private void SomeData()
		 {
			  int threads = Runtime.Runtime.availableProcessors();
			  ExecutorService executor = Executors.newFixedThreadPool( threads );
			  for ( int i = 0; i < threads; i++ )
			  {
					executor.submit(() =>
					{
					 for ( int t = 0; t < 100; t++ )
					 {
						  using ( Transaction tx = Db.beginTx() )
						  {
								for ( int n = 0; n < 100; n++ )
								{
									 Node node = Db.createNode( _labels );
									 foreach ( string key in _keys )
									 {
										  node.setProperty( key, format( "some value %d", n ) );
									 }
								}
								tx.success();
						  }
					 }
					});
			  }
			  executor.shutdown();
			  while ( !executor.awaitTermination( 1, SECONDS ) )
			  {
					// Just wait longer
			  }
		 }

		 private class IndexPopulationMemoryUsageMonitor : IndexingService.MonitorAdapter
		 {
			  internal volatile long PeakDirectMemoryUsage;
			  internal readonly System.Threading.CountdownEvent Called = new System.Threading.CountdownEvent( 1 );

			  public override void PopulationJobCompleted( long peakDirectMemoryUsage )
			  {
					this.PeakDirectMemoryUsage = peakDirectMemoryUsage;
					// We need a count on this one because index will come online slightly before we get a call to this method
					Called.Signal();
			  }
		 }
	}

}
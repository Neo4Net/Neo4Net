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
namespace Neo4Net.@unsafe.Impl.Batchimport
{

	using NamedThreadFactory = Neo4Net.Helpers.NamedThreadFactory;
	using RelationshipTypeCount = Neo4Net.@unsafe.Impl.Batchimport.DataStatistics.RelationshipTypeCount;
	using IdMapper = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.IdMapper;
	using Collector = Neo4Net.@unsafe.Impl.Batchimport.input.Collector;
	using Input = Neo4Net.@unsafe.Impl.Batchimport.input.Input;
	using InputChunk = Neo4Net.@unsafe.Impl.Batchimport.input.InputChunk;
	using InputEntityVisitor = Neo4Net.@unsafe.Impl.Batchimport.input.InputEntityVisitor;
	using ExecutionMonitor = Neo4Net.@unsafe.Impl.Batchimport.staging.ExecutionMonitor;
	using StageExecution = Neo4Net.@unsafe.Impl.Batchimport.staging.StageExecution;
	using Neo4Net.@unsafe.Impl.Batchimport.staging;
	using Key = Neo4Net.@unsafe.Impl.Batchimport.stats.Key;
	using Keys = Neo4Net.@unsafe.Impl.Batchimport.stats.Keys;
	using Stat = Neo4Net.@unsafe.Impl.Batchimport.stats.Stat;
	using StatsProvider = Neo4Net.@unsafe.Impl.Batchimport.stats.StatsProvider;
	using StepStats = Neo4Net.@unsafe.Impl.Batchimport.stats.StepStats;
	using BatchingNeoStores = Neo4Net.@unsafe.Impl.Batchimport.store.BatchingNeoStores;
	using IoMonitor = Neo4Net.@unsafe.Impl.Batchimport.store.io.IoMonitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.stats.Stats.longStat;

	/// <summary>
	/// Imports data from <seealso cref="Input"/> into a store. Only linkage between property records is done, not between nodes/relationships
	/// or any other types of records.
	/// 
	/// Main design goal here is low garbage and letting multiple threads import with as little as possible shared between threads.
	/// So importing consists of instantiating an input source reader, optimal number of threads and letting each thread:
	/// <ol>
	/// <li>Get <seealso cref="InputChunk chunk"/> of data and for every entity in it:</li>
	/// <li>Parse its data, filling current record with data using <seealso cref="InputEntityVisitor"/> callback from parsing</li>
	/// <li>Write record(s)</li>
	/// <li>Repeat until no more chunks from input.</li>
	/// </ol>
	/// </summary>
	public class DataImporter
	{
		 public const string NODE_IMPORT_NAME = "Nodes";
		 public const string RELATIONSHIP_IMPORT_NAME = "Relationships";

		 public class Monitor
		 {
			  internal readonly LongAdder Nodes = new LongAdder();
			  internal readonly LongAdder Relationships = new LongAdder();
			  internal readonly LongAdder Properties = new LongAdder();

			  public virtual void NodesImported( long nodes )
			  {
					this.Nodes.add( nodes );
			  }

			  public virtual void NodesRemoved( long nodes )
			  {
					this.Nodes.add( -nodes );
			  }

			  public virtual void RelationshipsImported( long relationships )
			  {
					this.Relationships.add( relationships );
			  }

			  public virtual void PropertiesImported( long properties )
			  {
					this.Properties.add( properties );
			  }

			  public virtual void PropertiesRemoved( long properties )
			  {
					this.Properties.add( -properties );
			  }

			  public virtual long NodesImported()
			  {
					return this.Nodes.sum();
			  }

			  public virtual long PropertiesImported()
			  {
					return this.Properties.sum();
			  }

			  public override string ToString()
			  {
					return format( "Imported:%n  %d nodes%n  %d relationships%n  %d properties", Nodes.sum(), Relationships.sum(), Properties.sum() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static long importData(String title, int numRunners, InputIterable data, org.neo4j.unsafe.impl.batchimport.store.BatchingNeoStores stores, System.Func<EntityImporter> visitors, org.neo4j.unsafe.impl.batchimport.staging.ExecutionMonitor executionMonitor, org.neo4j.unsafe.impl.batchimport.stats.StatsProvider memoryStatsProvider) throws java.io.IOException
		 private static long ImportData( string title, int numRunners, InputIterable data, BatchingNeoStores stores, System.Func<EntityImporter> visitors, ExecutionMonitor executionMonitor, StatsProvider memoryStatsProvider )
		 {
			  LongAdder roughEntityCountProgress = new LongAdder();
			  ExecutorService pool = Executors.newFixedThreadPool( numRunners, new NamedThreadFactory( title + "Importer" ) );
			  IoMonitor writeMonitor = new IoMonitor( stores.IoTracer );
			  ControllableStep step = new ControllableStep( title, roughEntityCountProgress, Configuration.DEFAULT, writeMonitor, memoryStatsProvider );
			  StageExecution execution = new StageExecution( title, null, Configuration.DEFAULT, Collections.singletonList( step ), 0 );
			  long startTime = currentTimeMillis();
			  using ( InputIterator dataIterator = data.GetEnumerator() )
			  {
					executionMonitor.Start( execution );
					for ( int i = 0; i < numRunners; i++ )
					{
						 pool.submit( new ExhaustingEntityImporterRunnable( execution, dataIterator, visitors(), roughEntityCountProgress ) );
					}
					pool.shutdown();

					long nextWait = 0;
					try
					{
						 while ( !pool.awaitTermination( nextWait, TimeUnit.MILLISECONDS ) )
						 {
							  executionMonitor.Check( execution );
							  nextWait = executionMonitor.NextCheckTime() - currentTimeMillis();
						 }
					}
					catch ( InterruptedException e )
					{
						 Thread.CurrentThread.Interrupt();
						 throw new IOException( e );
					}
			  }

			  execution.AssertHealthy();
			  step.MarkAsCompleted();
			  writeMonitor.Stop();
			  executionMonitor.End( execution, currentTimeMillis() - startTime );
			  execution.AssertHealthy();

			  return roughEntityCountProgress.sum();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void importNodes(int numRunners, org.neo4j.unsafe.impl.batchimport.input.Input input, org.neo4j.unsafe.impl.batchimport.store.BatchingNeoStores stores, org.neo4j.unsafe.impl.batchimport.cache.idmapping.IdMapper idMapper, org.neo4j.unsafe.impl.batchimport.staging.ExecutionMonitor executionMonitor, Monitor monitor) throws java.io.IOException
		 public static void ImportNodes( int numRunners, Input input, BatchingNeoStores stores, IdMapper idMapper, ExecutionMonitor executionMonitor, Monitor monitor )
		 {
			  System.Func<EntityImporter> importers = () => new NodeImporter(stores, idMapper, monitor);
			  ImportData( NODE_IMPORT_NAME, numRunners, input.Nodes(), stores, importers, executionMonitor, new MemoryUsageStatsProvider(stores, idMapper) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static DataStatistics importRelationships(int numRunners, org.neo4j.unsafe.impl.batchimport.input.Input input, org.neo4j.unsafe.impl.batchimport.store.BatchingNeoStores stores, org.neo4j.unsafe.impl.batchimport.cache.idmapping.IdMapper idMapper, org.neo4j.unsafe.impl.batchimport.input.Collector badCollector, org.neo4j.unsafe.impl.batchimport.staging.ExecutionMonitor executionMonitor, Monitor monitor, boolean validateRelationshipData) throws java.io.IOException
		 public static DataStatistics ImportRelationships( int numRunners, Input input, BatchingNeoStores stores, IdMapper idMapper, Collector badCollector, ExecutionMonitor executionMonitor, Monitor monitor, bool validateRelationshipData )
		 {
			  DataStatistics typeDistribution = new DataStatistics( monitor, new RelationshipTypeCount[0] );
			  System.Func<EntityImporter> importers = () => new RelationshipImporter(stores, idMapper, typeDistribution, monitor, badCollector, validateRelationshipData, stores.UsesDoubleRelationshipRecordUnits());
			  ImportData( RELATIONSHIP_IMPORT_NAME, numRunners, input.Relationships(), stores, importers, executionMonitor, new MemoryUsageStatsProvider(stores, idMapper) );
			  return typeDistribution;
		 }

		 /// <summary>
		 /// Here simply to be able to fit into the ExecutionMonitor thing
		 /// </summary>
		 private class ControllableStep : Step<Void>, StatsProvider
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string NameConflict;
			  internal readonly LongAdder Progress;
			  internal readonly int BatchSize;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Key[] KeysConflict = new Key[] { Keys.done_batches, Keys.avg_processing_time };
			  internal readonly ICollection<StatsProvider> StatsProviders = new List<StatsProvider>();

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly System.Threading.CountdownEvent CompletedConflict = new System.Threading.CountdownEvent( 1 );

			  internal ControllableStep( string name, LongAdder progress, Configuration config, params StatsProvider[] additionalStatsProviders )
			  {
					this.NameConflict = name;
					this.Progress = progress;
					this.BatchSize = config.BatchSize(); // just to be able to report correctly

					StatsProviders.Add( this );
					StatsProviders.addAll( Arrays.asList( additionalStatsProviders ) );
			  }

			  internal virtual void MarkAsCompleted()
			  {
					this.CompletedConflict.Signal();
			  }

			  public override void ReceivePanic( Exception cause )
			  {
			  }

			  public override void Start( int orderingGuarantees )
			  {
			  }

			  public override string Name()
			  {
					return NameConflict;
			  }

			  public override long Receive( long ticket, Void batch )
			  {
					return 0;
			  }

			  public override StepStats Stats()
			  {
					return new StepStats( NameConflict, !Completed, StatsProviders );
			  }

			  public override void EndOfUpstream()
			  {
			  }

			  public virtual bool Completed
			  {
				  get
				  {
						return CompletedConflict.CurrentCount == 0;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void awaitCompleted() throws InterruptedException
			  public override void AwaitCompleted()
			  {
					CompletedConflict.await();
			  }

			  public virtual Step<T1> Downstream<T1>
			  {
				  set
				  {
				  }
			  }

			  public override void Close()
			  {
			  }

			  public override Stat Stat( Key key )
			  {
					if ( key == Keys.done_batches )
					{
						 return longStat( Progress.sum() / BatchSize );
					}
					if ( key == Keys.avg_processing_time )
					{
						 return longStat( 10 );
					}
					return null;
			  }

			  public override Key[] Keys()
			  {
					return KeysConflict;
			  }
		 }
	}

}
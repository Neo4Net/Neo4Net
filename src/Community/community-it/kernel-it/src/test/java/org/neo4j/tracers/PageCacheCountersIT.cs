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
namespace Neo4Net.Tracers
{
	using RandomStringUtils = org.apache.commons.lang3.RandomStringUtils;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Cancelable = Neo4Net.Helpers.Cancelable;
	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using PageCursorCounters = Neo4Net.Io.pagecache.tracing.cursor.PageCursorCounters;
	using KernelStatement = Neo4Net.Kernel.Impl.Api.KernelStatement;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using Tracers = Neo4Net.Kernel.monitoring.tracing.Tracers;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using RepeatRule = Neo4Net.Test.rule.RepeatRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class PageCacheCountersIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();
		 private GraphDatabaseService _db;
		 private ExecutorService _executors;
		 private int _numberOfWorkers;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(TestDirectory.storeDir());
			  _numberOfWorkers = Runtime.Runtime.availableProcessors();
			  _executors = Executors.newFixedThreadPool( _numberOfWorkers );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _executors.shutdown();
			  _executors.awaitTermination( 5, TimeUnit.SECONDS );
			  _db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 60_000) @RepeatRule.Repeat(times = 5) public void pageCacheCountersAreSumOfPageCursorCounters() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PageCacheCountersAreSumOfPageCursorCounters()
		 {
			  IList<NodeCreator> nodeCreators = new List<NodeCreator>( _numberOfWorkers );
			  IList<Future> nodeCreatorFutures = new List<Future>( _numberOfWorkers );
			  PageCacheTracer pageCacheTracer = GetPageCacheTracer( _db );

			  long initialPins = pageCacheTracer.Pins();
			  long initialHits = pageCacheTracer.Hits();
			  long initialUnpins = pageCacheTracer.Unpins();
			  long initialBytesRead = pageCacheTracer.BytesRead();
			  long initialBytesWritten = pageCacheTracer.BytesWritten();
			  long initialEvictions = pageCacheTracer.Evictions();
			  long initialFaults = pageCacheTracer.Faults();
			  long initialFlushes = pageCacheTracer.Flushes();

			  StartNodeCreators( nodeCreators, nodeCreatorFutures );
			  while ( pageCacheTracer.Pins() == 0 || pageCacheTracer.Faults() == 0 || pageCacheTracer.Unpins() == 0 )
			  {
					TimeUnit.MILLISECONDS.sleep( 10 );
			  }
			  StopNodeCreators( nodeCreators, nodeCreatorFutures );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( "Number of pins events in page cache tracer should equal to the sum of pin events in " + "page cursor tracers.", pageCacheTracer.Pins(), greaterThanOrEqualTo(SumCounters(nodeCreators, NodeCreator::getPins, initialPins)) );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( "Number of unpins events in page cache tracer should equal to the sum of unpin events in " + "page cursor tracers.", pageCacheTracer.Unpins(), greaterThanOrEqualTo(SumCounters(nodeCreators, NodeCreator::getUnpins, initialUnpins)) );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( "Number of initialBytesRead in page cache tracer should equal to the sum of initialBytesRead " + "in page cursor tracers.", pageCacheTracer.BytesRead(), greaterThanOrEqualTo(SumCounters(nodeCreators, NodeCreator::getBytesRead, initialBytesRead)) );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( "Number of bytesWritten in page cache tracer should equal to the sum of bytesWritten in " + "page cursor tracers.", pageCacheTracer.BytesWritten(), greaterThanOrEqualTo(SumCounters(nodeCreators, NodeCreator::getBytesWritten, initialBytesWritten)) );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( "Number of evictions in page cache tracer should equal to the sum of evictions in " + "page cursor tracers.", pageCacheTracer.Evictions(), greaterThanOrEqualTo(SumCounters(nodeCreators, NodeCreator::getEvictions, initialEvictions)) );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( "Number of faults in page cache tracer should equal to the sum of faults in page cursor tracers.", pageCacheTracer.Faults(), greaterThanOrEqualTo(SumCounters(nodeCreators, NodeCreator::getFaults, initialFaults)) );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( "Number of flushes in page cache tracer should equal to the sum of flushes in page cursor tracers.", pageCacheTracer.Flushes(), greaterThanOrEqualTo(SumCounters(nodeCreators, NodeCreator::getFlushes, initialFlushes)) );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertThat( "Number of hits in page cache tracer should equal to the sum of hits in page cursor tracers.", pageCacheTracer.Hits(), greaterThanOrEqualTo(SumCounters(nodeCreators, NodeCreator::getHits, initialHits)) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void stopNodeCreators(java.util.List<NodeCreator> nodeCreators, java.util.List<java.util.concurrent.Future> nodeCreatorFutures) throws InterruptedException, java.util.concurrent.ExecutionException
		 private void StopNodeCreators( IList<NodeCreator> nodeCreators, IList<Future> nodeCreatorFutures )
		 {
			  nodeCreators.ForEach( NodeCreator.cancel );
			  foreach ( Future creatorFuture in nodeCreatorFutures )
			  {
					creatorFuture.get();
			  }
		 }

		 private void StartNodeCreators( IList<NodeCreator> nodeCreators, IList<Future> nodeCreatorFutures )
		 {
			  for ( int i = 0; i < _numberOfWorkers; i++ )
			  {
					NodeCreator nodeCreator = new NodeCreator( this, _db );
					nodeCreators.Add( nodeCreator );
					nodeCreatorFutures.Add( _executors.submit( nodeCreator ) );
			  }
		 }

		 private long SumCounters( IList<NodeCreator> nodeCreators, System.Func<NodeCreator, long> mapper, long initialValue )
		 {
			  return nodeCreators.Select( mapper ).Sum() + initialValue;
		 }

		 private PageCacheTracer GetPageCacheTracer( GraphDatabaseService db )
		 {
			  Tracers tracers = ( ( GraphDatabaseAPI ) db ).DependencyResolver.resolveDependency( typeof( Tracers ) );
			  return tracers.PageCacheTracer;
		 }

		 private class NodeCreator : ThreadStart, Cancelable
		 {
			 private readonly PageCacheCountersIT _outerInstance;

			  internal volatile bool Canceled;

			  internal readonly GraphDatabaseService Db;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long PinsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long UnpinsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long HitsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long BytesReadConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long BytesWrittenConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long EvictionsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long FaultsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long FlushesConflict;
			  internal NodeCreator( PageCacheCountersIT outerInstance, GraphDatabaseService db )
			  {
				  this._outerInstance = outerInstance;
					this.Db = db;
			  }

			  public override void Run()
			  {
					ThreadLocalRandom localRandom = ThreadLocalRandom.current();
					while ( !Canceled )
					{
						 PageCursorCounters pageCursorCounters;
						 using ( Transaction transaction = Db.beginTx(), KernelStatement kernelStatement = GetKernelStatement((GraphDatabaseAPI) Db) )
						 {
							  pageCursorCounters = kernelStatement.PageCursorTracer;
							  Node node = Db.createNode();
							  node.SetProperty( "name", RandomStringUtils.random( localRandom.Next( 100 ) ) );
							  node.SetProperty( "surname", RandomStringUtils.random( localRandom.Next( 100 ) ) );
							  node.SetProperty( "age", localRandom.Next( 100 ) );
							  transaction.Success();
							  StoreCounters( pageCursorCounters );
						 }
					}
			  }

			  internal virtual void StoreCounters( PageCursorCounters pageCursorCounters )
			  {
					Objects.requireNonNull( pageCursorCounters );
					PinsConflict += pageCursorCounters.Pins();
					UnpinsConflict += pageCursorCounters.Unpins();
					HitsConflict += pageCursorCounters.Hits();
					BytesReadConflict += pageCursorCounters.BytesRead();
					BytesWrittenConflict += pageCursorCounters.BytesWritten();
					EvictionsConflict += pageCursorCounters.Evictions();
					FaultsConflict += pageCursorCounters.Faults();
					FlushesConflict += pageCursorCounters.Flushes();
			  }

			  public override void Cancel()
			  {
					Canceled = true;
			  }

			  internal virtual long Pins
			  {
				  get
				  {
						return PinsConflict;
				  }
			  }

			  internal virtual long Unpins
			  {
				  get
				  {
						return UnpinsConflict;
				  }
			  }

			  public virtual long Hits
			  {
				  get
				  {
						return HitsConflict;
				  }
			  }

			  internal virtual long BytesRead
			  {
				  get
				  {
						return BytesReadConflict;
				  }
			  }

			  internal virtual long BytesWritten
			  {
				  get
				  {
						return BytesWrittenConflict;
				  }
			  }

			  internal virtual long Evictions
			  {
				  get
				  {
						return EvictionsConflict;
				  }
			  }

			  internal virtual long Faults
			  {
				  get
				  {
						return FaultsConflict;
				  }
			  }

			  internal virtual long Flushes
			  {
				  get
				  {
						return FlushesConflict;
				  }
			  }

			  internal virtual KernelStatement GetKernelStatement( GraphDatabaseAPI db )
			  {
					ThreadToStatementContextBridge statementBridge = Db.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) );
					return ( KernelStatement ) statementBridge.Get();
			  }
		 }
	}

}
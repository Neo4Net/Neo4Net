using System.Collections.Generic;

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
namespace Neo4Net.@unsafe.Impl.Batchimport
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Direction = Neo4Net.GraphDb.Direction;
	using PageCacheAndDependenciesRule = Neo4Net.Test.rule.PageCacheAndDependenciesRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using RelationshipTypeCount = Neo4Net.@unsafe.Impl.Batchimport.DataStatistics.RelationshipTypeCount;
	using NodeRelationshipCache = Neo4Net.@unsafe.Impl.Batchimport.cache.NodeRelationshipCache;
	using NumberArrayFactory = Neo4Net.@unsafe.Impl.Batchimport.cache.NumberArrayFactory;
	using ExecutionMonitor = Neo4Net.@unsafe.Impl.Batchimport.staging.ExecutionMonitor;
	using BatchingNeoStores = Neo4Net.@unsafe.Impl.Batchimport.store.BatchingNeoStores;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.pagecache.tracing.PageCacheTracer.NULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Config.defaults;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.format.RecordFormatSelector.defaultFormat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.logging.Internal.NullLogService.getInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.AdditionalInitialIds.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.Configuration.DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.ImportLogic.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.store.BatchingNeoStores.batchingNeoStoresWithExternalPageCache;

	public class ImportLogicTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.PageCacheAndDependenciesRule storage = new org.Neo4Net.test.rule.PageCacheAndDependenciesRule();
		 public readonly PageCacheAndDependenciesRule Storage = new PageCacheAndDependenciesRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.RandomRule random = new org.Neo4Net.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void closeImporterWithoutDiagnosticState() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseImporterWithoutDiagnosticState()
		 {
			  ExecutionMonitor monitor = mock( typeof( ExecutionMonitor ) );
			  using ( BatchingNeoStores stores = batchingNeoStoresWithExternalPageCache( Storage.fileSystem(), Storage.pageCache(), NULL, Storage.directory().directory(), defaultFormat(), DEFAULT, Instance, EMPTY, defaults() ) )
			  {
					//noinspection EmptyTryBlock
					using ( ImportLogic logic = new ImportLogic( Storage.directory().directory(), Storage.fileSystem(), stores, DEFAULT, Instance, monitor, defaultFormat(), NO_MONITOR ) )
					{
						 // nothing to run in this import
						 logic.Success();
					}
			  }

			  verify( monitor ).done( eq( true ), anyLong(), contains("Data statistics is not available.") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSplitUpRelationshipTypesInBatches()
		 public virtual void ShouldSplitUpRelationshipTypesInBatches()
		 {
			  // GIVEN
			  int denseNodeThreshold = 5;
			  int numberOfNodes = 100;
			  int numberOfTypes = 10;
			  NodeRelationshipCache cache = new NodeRelationshipCache( NumberArrayFactory.HEAP, denseNodeThreshold );
			  cache.NodeCount = numberOfNodes + 1;
			  Direction[] directions = Direction.values();
			  for ( int i = 0; i < numberOfNodes; i++ )
			  {
					int count = Random.Next( 1, denseNodeThreshold * 2 );
					cache.setCount( i, count, Random.Next( numberOfTypes ), Random.among( directions ) );
			  }
			  cache.CountingCompleted();
			  IList<RelationshipTypeCount> types = new List<RelationshipTypeCount>();
			  int numberOfRelationships = 0;
			  for ( int i = 0; i < numberOfTypes; i++ )
			  {
					int count = Random.Next( 1, 100 );
					types.Add( new RelationshipTypeCount( i, count ) );
					numberOfRelationships += count;
			  }
			  types.sort( ( t1, t2 ) => Long.compare( t2.Count, t1.Count ) );
			  DataStatistics typeDistribution = new DataStatistics( 0, 0, types.ToArray() );

			  {
			  // WHEN enough memory for all types
					long memory = cache.CalculateMaxMemoryUsage( numberOfRelationships ) * numberOfTypes;
					int upToType = ImportLogic.NextSetOfTypesThatFitInMemory( typeDistribution, 0, memory, cache.NumberOfDenseNodes );

					// THEN
					assertEquals( types.Count, upToType );
			  }

			  {
			  // and WHEN less than enough memory for all types
					long memory = cache.CalculateMaxMemoryUsage( numberOfRelationships ) * numberOfTypes / 3;
					int startingFromType = 0;
					int rounds = 0;
					while ( startingFromType < types.Count )
					{
						 rounds++;
						 startingFromType = ImportLogic.NextSetOfTypesThatFitInMemory( typeDistribution, startingFromType, memory, cache.NumberOfDenseNodes );
					}
					assertEquals( types.Count, startingFromType );
					assertThat( rounds, greaterThan( 1 ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseDataStatisticsCountsForPrintingFinalStats() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUseDataStatisticsCountsForPrintingFinalStats()
		 {
			  // given
			  ExecutionMonitor monitor = mock( typeof( ExecutionMonitor ) );
			  using ( BatchingNeoStores stores = batchingNeoStoresWithExternalPageCache( Storage.fileSystem(), Storage.pageCache(), NULL, Storage.directory().directory(), defaultFormat(), DEFAULT, Instance, EMPTY, defaults() ) )
			  {
					// when
					RelationshipTypeCount[] relationshipTypeCounts = new RelationshipTypeCount[]
					{
						new RelationshipTypeCount( 0, 33 ),
						new RelationshipTypeCount( 1, 66 )
					};
					DataStatistics dataStatistics = new DataStatistics( 100123, 100456, relationshipTypeCounts );
					using ( ImportLogic logic = new ImportLogic( Storage.directory().directory(), Storage.fileSystem(), stores, DEFAULT, Instance, monitor, defaultFormat(), NO_MONITOR ) )
					{
						 logic.PutState( dataStatistics );
						 logic.Success();
					}

					// then
					verify( monitor ).done( eq( true ), anyLong(), contains(dataStatistics.ToString()) );
			  }
		 }
	}

}
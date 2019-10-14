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
	using Test = org.junit.Test;

	using CountsAccessor = Neo4Net.Kernel.Impl.Api.CountsAccessor;
	using MemoryStatsVisitor = Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor;
	using NodeLabelsCache = Neo4Net.@unsafe.Impl.Batchimport.cache.NodeLabelsCache;
	using SimpleStageControl = Neo4Net.@unsafe.Impl.Batchimport.staging.SimpleStageControl;
	using StageControl = Neo4Net.@unsafe.Impl.Batchimport.staging.StageControl;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.ByteUnit.mebiBytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.ByteUnit.tebiBytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.monitoring.SilentProgressReporter.INSTANCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory.OFF_HEAP;

	public class ProcessRelationshipCountsDataStepTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLetProcessorsBeZeroIfEnoughMemory()
		 public virtual void ShouldLetProcessorsBeZeroIfEnoughMemory()
		 {
			  // given
			  ProcessRelationshipCountsDataStep step = InstantiateStep( 10, 10, 10_000, 4, mebiBytes( 10 ) );

			  // then
			  assertEquals( 0, step.MaxProcessors );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotOverflowWhenTooMuchMemoryAvailable()
		 public virtual void ShouldNotOverflowWhenTooMuchMemoryAvailable()
		 {
			  // given
			  ProcessRelationshipCountsDataStep step = InstantiateStep( 1, 1, 10_000, 64, tebiBytes( 10 ) );

			  // then
			  assertEquals( 0, step.MaxProcessors );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLimitProcessorsIfScarceMemory()
		 public virtual void ShouldLimitProcessorsIfScarceMemory()
		 {
			  // given labels/types amounting to ~360k, 2MiB max mem and 1MiB in use by node-label cache
			  ProcessRelationshipCountsDataStep step = InstantiateStep( 100, 220, mebiBytes( 1 ), 4, mebiBytes( 2 ) );

			  // then
			  assertEquals( 2, step.MaxProcessors );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAtLeastHaveOneProcessorEvenIfLowMemory()
		 public virtual void ShouldAtLeastHaveOneProcessorEvenIfLowMemory()
		 {
			  // given labels/types amounting to ~1.6MiB, 2MiB max mem and 1MiB in use by node-label cache
			  ProcessRelationshipCountsDataStep step = InstantiateStep( 1_000, 1_000, mebiBytes( 1 ), 4, mebiBytes( 2 ) );

			  // then
			  assertEquals( 1, step.MaxProcessors );
		 }

		 private ProcessRelationshipCountsDataStep InstantiateStep( int highLabelId, int highRelationshipTypeId, long labelCacheSize, int maxProcessors, long maxMemory )
		 {
			  StageControl control = new SimpleStageControl();
			  NodeLabelsCache cache = NodeLabelsCache( labelCacheSize );
			  Configuration config = mock( typeof( Configuration ) );
			  when( config.MaxNumberOfProcessors() ).thenReturn(maxProcessors);
			  when( config.MaxMemoryUsage() ).thenReturn(maxMemory);
			  return new ProcessRelationshipCountsDataStep( control, cache, config, highLabelId, highRelationshipTypeId, mock( typeof( Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater ) ), OFF_HEAP, INSTANCE );
		 }

		 private NodeLabelsCache NodeLabelsCache( long sizeInBytes )
		 {
			  NodeLabelsCache cache = mock( typeof( NodeLabelsCache ) );
			  doAnswer(invocation =>
			  {
				MemoryStatsVisitor visitor = invocation.getArgument( 0 );
				visitor.offHeapUsage( sizeInBytes );
				return null;
			  }).when( cache ).acceptMemoryStatsVisitor( any() );
			  return cache;
		 }
	}

}
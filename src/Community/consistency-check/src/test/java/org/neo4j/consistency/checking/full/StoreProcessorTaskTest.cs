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
namespace Neo4Net.Consistency.checking.full
{
	using Test = org.junit.jupiter.api.Test;

	using CacheAccess = Neo4Net.Consistency.checking.cache.CacheAccess;
	using Statistics = Neo4Net.Consistency.statistics.Statistics;
	using ProgressListener = Neo4Net.Helpers.progress.ProgressListener;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.same;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") class StoreProcessorTaskTest
	internal class StoreProcessorTaskTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void singlePassShouldOnlyProcessTheStoreOnce()
		 internal virtual void SinglePassShouldOnlyProcessTheStoreOnce()
		 {
			  // given
			  StoreProcessor singlePassProcessor = mock( typeof( StoreProcessor ) );
			  when( singlePassProcessor.Stage ).thenReturn( Stage_Fields.SequentialForward );

			  NodeStore store = mock( typeof( NodeStore ) );
			  when( store.StorageFile ).thenReturn( new File( "node-store" ) );

			  StoreProcessorTask<NodeRecord> task = new StoreProcessorTask<NodeRecord>( "nodes", Statistics.NONE, 1, store, null, "nodes", ProgressMonitorFactory.NONE.multipleParts( "check" ), CacheAccess.EMPTY, singlePassProcessor, QueueDistribution.ROUND_ROBIN );

			  // when
			  task.Run();

			  // then
			  verify( singlePassProcessor ).applyFiltered( same( store ), any( typeof( ProgressListener ) ) );
		 }
	}

}
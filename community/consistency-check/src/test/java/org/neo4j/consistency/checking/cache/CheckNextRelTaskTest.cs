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
namespace Org.Neo4j.Consistency.checking.cache
{
	using Test = org.junit.jupiter.api.Test;
	using Mockito = org.mockito.Mockito;

	using CheckStage = Org.Neo4j.Consistency.checking.full.CheckStage;
	using Stage = Org.Neo4j.Consistency.checking.full.Stage;
	using StoreProcessor = Org.Neo4j.Consistency.checking.full.StoreProcessor;
	using Counts = Org.Neo4j.Consistency.statistics.Counts;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using NodeStore = Org.Neo4j.Kernel.impl.store.NodeStore;
	using StoreAccess = Org.Neo4j.Kernel.impl.store.StoreAccess;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using RecordLoad = Org.Neo4j.Kernel.impl.store.record.RecordLoad;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class CheckNextRelTaskTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void scanForHighIdOnlyOnceWhenProcessCache()
		 internal virtual void ScanForHighIdOnlyOnceWhenProcessCache()
		 {
			  NeoStores neoStores = mock( typeof( NeoStores ), Mockito.RETURNS_MOCKS );
			  NodeStore nodeStore = mock( typeof( NodeStore ) );
			  NodeRecord nodeRecord = mock( typeof( NodeRecord ) );
			  StoreProcessor storeProcessor = mock( typeof( StoreProcessor ) );

			  when( neoStores.NodeStore ).thenReturn( nodeStore );
			  when( nodeStore.HighId ).thenReturn( 10L );
			  when( nodeStore.GetRecord( anyLong(), any(typeof(NodeRecord)), any(typeof(RecordLoad)) ) ).thenReturn(nodeRecord);
			  when( nodeStore.NewRecord() ).thenReturn(nodeRecord);

			  StoreAccess storeAccess = new StoreAccess( neoStores );
			  storeAccess.Initialize();

			  DefaultCacheAccess cacheAccess = new DefaultCacheAccess( Counts.NONE, 1 );
			  CacheTask.CheckNextRel cacheTask = new CacheTask.CheckNextRel( Org.Neo4j.Consistency.checking.full.Stage_Fields.SequentialForward, cacheAccess, storeAccess, storeProcessor );

			  cacheAccess.CacheSlotSizes = CheckStage.Stage5_Check_NextRel.CacheSlotSizes;
			  cacheTask.ProcessCache();

			  verify( nodeStore, times( 1 ) ).HighId;
		 }
	}

}
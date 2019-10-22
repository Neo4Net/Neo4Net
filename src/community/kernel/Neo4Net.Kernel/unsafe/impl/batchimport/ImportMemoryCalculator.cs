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
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using MemoryStatsVisitor = Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor;
	using Input = Neo4Net.@unsafe.Impl.Batchimport.input.Input;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.ByteUnit.gibiBytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.NoStoreHeader.NO_STORE_HEADER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.cache.GatheringMemoryStatsVisitor.highestMemoryUsageOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.cache.GatheringMemoryStatsVisitor.totalMemoryUsageOf;

	/// <summary>
	/// Aims to collect logic for calculating memory usage, optimal heap size and more, mostly based on
	/// <seealso cref="org.Neo4Net.unsafe.impl.batchimport.input.Input.Estimates"/>. The reason why we're trying so hard to calculate
	/// these things is that for large imports... getting the balance between heap and off-heap memory just right
	/// will allow the importer to use available memory and can mean difference between a failed and successful import.
	/// 
	/// The calculated numbers are a bit on the defensive side, generally adding 10% to the numbers.
	/// </summary>
	public class ImportMemoryCalculator
	{
		 public static long EstimatedStoreSize( Neo4Net.@unsafe.Impl.Batchimport.input.Input_Estimates estimates, RecordFormats recordFormats )
		 {
			  long nodeSize = estimates.NumberOfNodes() * recordFormats.Node().getRecordSize(NO_STORE_HEADER);
			  long relationshipSize = estimates.NumberOfRelationships() * recordFormats.Relationship().getRecordSize(NO_STORE_HEADER);
			  long propertySize = estimates.SizeOfNodeProperties() + estimates.SizeOfRelationshipProperties();
			  long tempIdPropertySize = estimates.NumberOfNodes() * recordFormats.Property().getRecordSize(NO_STORE_HEADER);

			  return DefensivelyPadMemoryEstimate( nodeSize + relationshipSize + propertySize + tempIdPropertySize );
		 }

		 public static long EstimatedCacheSize( Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor_Visitable baseMemory, params Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor_Visitable[] memoryUsers )
		 {
			  long neoStoreSize = totalMemoryUsageOf( baseMemory );
			  long importCacheSize = highestMemoryUsageOf( memoryUsers );
			  return neoStoreSize + DefensivelyPadMemoryEstimate( importCacheSize );
		 }

		 /// <summary>
		 /// Calculates optimal and minimal heap size for an import. A minimal heap for an import has enough room for some amount
		 /// of working memory and the part of the page cache meta data living in the heap.
		 /// 
		 /// At the time of writing this the heap size is really only a function of store size, where parts of the page cache
		 /// meta data lives in the heap. For reference page cache meta data of a store of ~18TiB takes up ~10GiB of heap,
		 /// so pageCacheHeapUsage ~= storeSize / 2000. On top of that there must be some good old working memory of ~1-2 GiB
		 /// for handling objects created and operating during the import.
		 /// </summary>
		 /// <param name="estimates"> input estimates. </param>
		 /// <param name="recordFormats"> <seealso cref="RecordFormats"/>, containing record sizes. </param>
		 /// <returns> an optimal minimal heap size to use for this import. </returns>
		 public static long OptimalMinimalHeapSize( Neo4Net.@unsafe.Impl.Batchimport.input.Input_Estimates estimates, RecordFormats recordFormats )
		 {
			  long estimatedStoreSize = estimatedStoreSize( estimates, recordFormats );

			  return gibiBytes( 1 ) + estimatedStoreSize / 2_000;
		 }

		 public static long DefensivelyPadMemoryEstimate( long bytes )
		 {
			  return ( long )( bytes * 1.1 );
		 }

		 public static long DefensivelyPadMemoryEstimate( params Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor_Visitable[] memoryUsers )
		 {
			  return defensivelyPadMemoryEstimate( totalMemoryUsageOf( memoryUsers ) );
		 }
	}

}
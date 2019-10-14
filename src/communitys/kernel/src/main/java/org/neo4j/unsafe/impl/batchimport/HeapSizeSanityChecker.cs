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

	using OsBeanUtil = Neo4Net.Io.os.OsBeanUtil;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using MemoryStatsVisitor = Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor;
	using Input = Neo4Net.@unsafe.Impl.Batchimport.input.Input;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.os.OsBeanUtil.VALUE_UNAVAILABLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.ImportMemoryCalculator.estimatedCacheSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.ImportMemoryCalculator.optimalMinimalHeapSize;

	/// <summary>
	/// Sanity checking of <seealso cref="org.neo4j.unsafe.impl.batchimport.input.Input.Estimates"/> against heap size and free memory.
	/// Registers warnings onto a <seealso cref="ImportLogic.Monitor"/>.
	/// </summary>
	internal class HeapSizeSanityChecker
	{
		 private readonly ImportLogic.Monitor _monitor;
		 private readonly System.Func<long> _freeMemoryLookup;
		 private readonly System.Func<long> _actualHeapSizeLookup;

		 internal HeapSizeSanityChecker( ImportLogic.Monitor monitor ) : this( monitor, OsBeanUtil.getFreePhysicalMemory, Runtime.Runtime.maxMemory )
		 {
		 }

		 internal HeapSizeSanityChecker( ImportLogic.Monitor monitor, System.Func<long> freeMemoryLookup, System.Func<long> actualHeapSizeLookup )
		 {
			  this._monitor = monitor;
			  this._freeMemoryLookup = freeMemoryLookup;
			  this._actualHeapSizeLookup = actualHeapSizeLookup;
		 }

		 internal virtual void SanityCheck( Neo4Net.@unsafe.Impl.Batchimport.input.Input_Estimates inputEstimates, RecordFormats recordFormats, Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor_Visitable baseMemory, params Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor_Visitable[] memoryVisitables )
		 {
			  // At this point in time the store hasn't started so it won't show up in free memory reported from OS,
			  // i.e. we have to include it here in the calculations.
			  long estimatedCacheSize = estimatedCacheSize( baseMemory, memoryVisitables );
			  long freeMemory = _freeMemoryLookup.AsLong;
			  long optimalMinimalHeapSize = optimalMinimalHeapSize( inputEstimates, recordFormats );
			  long actualHeapSize = _actualHeapSizeLookup.AsLong;
			  bool freeMemoryIsKnown = freeMemory != VALUE_UNAVAILABLE;

			  // Check if there's enough memory for the import
			  if ( freeMemoryIsKnown && actualHeapSize + freeMemory < estimatedCacheSize + optimalMinimalHeapSize )
			  {
					_monitor.insufficientAvailableMemory( estimatedCacheSize, optimalMinimalHeapSize, freeMemory );
					return; // there's likely not available memory, no need to warn about anything else
			  }

			  // Check if the heap is big enough to handle the import
			  if ( actualHeapSize < optimalMinimalHeapSize )
			  {
					_monitor.insufficientHeapSize( optimalMinimalHeapSize, actualHeapSize );
					return; // user have been warned about heap size issue
			  }

			  // Check if heap size could be tweaked
			  if ( ( !freeMemoryIsKnown || freeMemory < estimatedCacheSize ) && actualHeapSize > optimalMinimalHeapSize * 1.2 )
			  {
					_monitor.abundantHeapSize( optimalMinimalHeapSize, actualHeapSize );
			  }
		 }
	}

}
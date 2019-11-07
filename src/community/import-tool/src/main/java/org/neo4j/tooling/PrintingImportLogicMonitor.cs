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
namespace Neo4Net.Tooling
{

	using ImportLogic = Neo4Net.@unsafe.Impl.Batchimport.ImportLogic;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.Format.bytes;

	internal class PrintingImportLogicMonitor : ImportLogic.Monitor
	{
		 private readonly PrintStream @out;
		 private readonly PrintStream _err;

		 internal PrintingImportLogicMonitor( PrintStream @out, PrintStream err )
		 {
			  this.@out = @out;
			  this._err = err;
		 }

		 public override void DoubleRelationshipRecordUnitsEnabled()
		 {
			  @out.println( "Will use double record units for all relationships" );
		 }

		 public override void MayExceedNodeIdCapacity( long capacity, long estimatedCount )
		 {
			  _err.printf( "WARNING: estimated number of relationships %d may exceed capacity %d of selected record format%n", estimatedCount, capacity );
		 }

		 public override void MayExceedRelationshipIdCapacity( long capacity, long estimatedCount )
		 {
			  _err.printf( "WARNING: estimated number of nodes %d may exceed capacity %d of selected record format%n", estimatedCount, capacity );
		 }

		 public override void InsufficientHeapSize( long optimalMinimalHeapSize, long heapSize )
		 {
			  _err.printf( "WARNING: heap size %s may be too small to complete this import. Suggested heap size is %s", bytes( heapSize ), bytes( optimalMinimalHeapSize ) );
		 }

		 public override void AbundantHeapSize( long optimalMinimalHeapSize, long heapSize )
		 {
			  _err.printf( "WARNING: heap size %s is unnecessarily large for completing this import.%n" + "The abundant heap memory will leave less memory for off-heap importer caches. Suggested heap size is %s", bytes( heapSize ), bytes( optimalMinimalHeapSize ) );
		 }

		 public override void InsufficientAvailableMemory( long estimatedCacheSize, long optimalMinimalHeapSize, long availableMemory )
		 {
			  _err.printf( "WARNING: %s memory may not be sufficient to complete this import. Suggested memory distribution is:%n" + "heap size: %s%n" + "minimum free and available memory excluding heap size: %s", bytes( availableMemory ), bytes( optimalMinimalHeapSize ), bytes( estimatedCacheSize ) );
		 }
	}

}
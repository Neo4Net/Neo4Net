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
	using Test = org.junit.jupiter.api.Test;


	using ImportLogic = Neo4Net.@unsafe.Impl.Batchimport.ImportLogic;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Format.bytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.ByteUnit.gibiBytes;

	/// <summary>
	/// Why test a silly thing like this? This implementation contains some printf calls that needs to get arguments correct
	/// or will otherwise throw exception. It's surprisingly easy to get those wrong.
	/// </summary>
	internal class PrintingImportLogicMonitorTest
	{
		private bool InstanceFieldsInitialized = false;

		public PrintingImportLogicMonitorTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			@out = new PrintStream( _outBuffer );
			_err = new PrintStream( _errBuffer );
			_monitor = new PrintingImportLogicMonitor( @out, _err );
		}

		 private readonly MemoryStream _outBuffer = new MemoryStream();
		 private PrintStream @out;
		 private readonly MemoryStream _errBuffer = new MemoryStream();
		 private PrintStream _err;
		 private ImportLogic.Monitor _monitor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mayExceedNodeIdCapacity()
		 internal virtual void MayExceedNodeIdCapacity()
		 {
			  // given
			  long capacity = 10_000_000;
			  long estimatedCount = 12_000_000;

			  // when
			  _monitor.mayExceedNodeIdCapacity( capacity, estimatedCount );

			  // then
			  string text = _errBuffer.ToString();
			  assertTrue( text.Contains( "WARNING" ) );
			  assertTrue( text.Contains( "exceed" ) );
			  assertTrue( text.Contains( capacity.ToString() ) );
			  assertTrue( text.Contains( estimatedCount.ToString() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mayExceedRelationshipIdCapacity()
		 internal virtual void MayExceedRelationshipIdCapacity()
		 {
			  // given
			  long capacity = 10_000_000;
			  long estimatedCount = 12_000_000;

			  // when
			  _monitor.mayExceedRelationshipIdCapacity( capacity, estimatedCount );

			  // then
			  string text = _errBuffer.ToString();
			  assertTrue( text.Contains( "WARNING" ) );
			  assertTrue( text.Contains( "exceed" ) );
			  assertTrue( text.Contains( capacity.ToString() ) );
			  assertTrue( text.Contains( estimatedCount.ToString() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void insufficientHeapSize()
		 internal virtual void InsufficientHeapSize()
		 {
			  // given
			  long optimalHeapSize = gibiBytes( 2 );
			  long heapSize = gibiBytes( 1 );

			  // when
			  _monitor.insufficientHeapSize( optimalHeapSize, heapSize );

			  // then
			  string text = _errBuffer.ToString();
			  assertTrue( text.Contains( "WARNING" ) );
			  assertTrue( text.Contains( "too small" ) );
			  assertTrue( text.Contains( bytes( heapSize ) ) );
			  assertTrue( text.Contains( bytes( optimalHeapSize ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void abundantHeapSize()
		 internal virtual void AbundantHeapSize()
		 {
			  // given
			  long optimalHeapSize = gibiBytes( 2 );
			  long heapSize = gibiBytes( 10 );

			  // when
			  _monitor.abundantHeapSize( optimalHeapSize, heapSize );

			  // then
			  string text = _errBuffer.ToString();
			  assertTrue( text.Contains( "WARNING" ) );
			  assertTrue( text.Contains( "unnecessarily large" ) );
			  assertTrue( text.Contains( bytes( heapSize ) ) );
			  assertTrue( text.Contains( bytes( optimalHeapSize ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void insufficientAvailableMemory()
		 internal virtual void InsufficientAvailableMemory()
		 {
			  // given
			  long estimatedCacheSize = gibiBytes( 2 );
			  long optimalHeapSize = gibiBytes( 2 );
			  long availableMemory = gibiBytes( 1 );

			  // when
			  _monitor.insufficientAvailableMemory( estimatedCacheSize, optimalHeapSize, availableMemory );

			  // then
			  string text = _errBuffer.ToString();
			  assertTrue( text.Contains( "WARNING" ) );
			  assertTrue( text.Contains( "may not be sufficient" ) );
			  assertTrue( text.Contains( bytes( estimatedCacheSize ) ) );
			  assertTrue( text.Contains( bytes( optimalHeapSize ) ) );
			  assertTrue( text.Contains( bytes( availableMemory ) ) );
		 }
	}

}
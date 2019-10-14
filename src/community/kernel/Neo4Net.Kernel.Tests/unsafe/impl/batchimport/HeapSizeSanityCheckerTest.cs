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

	using MemoryStatsVisitor = Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor;
	using Input = Neo4Net.@unsafe.Impl.Batchimport.input.Input;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.ByteUnit.gibiBytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.format.standard.Standard.LATEST_RECORD_FORMATS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.Inputs.knownEstimates;

	public class HeapSizeSanityCheckerTest
	{
		private bool InstanceFieldsInitialized = false;

		public HeapSizeSanityCheckerTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_checker = new HeapSizeSanityChecker( _monitor, _freeMemorySupplier, _actualHeapSizeSupplier );
			_baseMemory = visitor => visitor.offHeapUsage( _baseMemorySupplier.AsLong );
			_memoryUser1 = visitor => visitor.offHeapUsage( _memoryUser1Supplier.AsLong );
			_memoryUser2 = visitor => visitor.offHeapUsage( _memoryUser2Supplier.AsLong );
		}

		 private readonly System.Func<long> _freeMemorySupplier = mock( typeof( System.Func<long> ) );
		 private readonly System.Func<long> _actualHeapSizeSupplier = mock( typeof( System.Func<long> ) );
		 private readonly ImportLogic.Monitor _monitor = mock( typeof( ImportLogic.Monitor ) );
		 private HeapSizeSanityChecker _checker;
		 private readonly System.Func<long> _baseMemorySupplier = mock( typeof( System.Func<long> ) );
		 private Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor_Visitable _baseMemory;
		 private readonly System.Func<long> _memoryUser1Supplier = mock( typeof( System.Func<long> ) );
		 private Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor_Visitable _memoryUser1;
		 private readonly System.Func<long> _memoryUser2Supplier = mock( typeof( System.Func<long> ) );
		 private Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor_Visitable _memoryUser2;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportInsufficientAvailableMemory()
		 public virtual void ShouldReportInsufficientAvailableMemory()
		 {
			  // given
			  when( _freeMemorySupplier.AsLong ).thenReturn( gibiBytes( 2 ) );
			  when( _actualHeapSizeSupplier.AsLong ).thenReturn( gibiBytes( 1 ) );
			  when( _baseMemorySupplier.AsLong ).thenReturn( gibiBytes( 1 ) );
			  when( _memoryUser1Supplier.AsLong ).thenReturn( gibiBytes( 1 ) );
			  when( _memoryUser2Supplier.AsLong ).thenReturn( gibiBytes( 1 ) );
			  Neo4Net.@unsafe.Impl.Batchimport.input.Input_Estimates estimates = knownEstimates( 1_000_000_000, 10_000_000_000L, 2_000_000_000L, 0, gibiBytes( 50 ), gibiBytes( 100 ), 0 );

			  // when
			  _checker.sanityCheck( estimates, LATEST_RECORD_FORMATS, _baseMemory, _memoryUser1, _memoryUser2 );

			  // then
			  verify( _monitor ).insufficientAvailableMemory( anyLong(), anyLong(), anyLong() );
			  verifyNoMoreInteractions( _monitor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportInsufficientHeapSize()
		 public virtual void ShouldReportInsufficientHeapSize()
		 {
			  // given
			  when( _freeMemorySupplier.AsLong ).thenReturn( gibiBytes( 20 ) );
			  when( _actualHeapSizeSupplier.AsLong ).thenReturn( gibiBytes( 1 ) );
			  when( _baseMemorySupplier.AsLong ).thenReturn( gibiBytes( 1 ) );
			  when( _memoryUser1Supplier.AsLong ).thenReturn( gibiBytes( 1 ) );
			  when( _memoryUser2Supplier.AsLong ).thenReturn( gibiBytes( 1 ) );
			  Neo4Net.@unsafe.Impl.Batchimport.input.Input_Estimates estimates = knownEstimates( 1_000_000_000, 10_000_000_000L, 2_000_000_000L, 0, gibiBytes( 50 ), gibiBytes( 100 ), 0 );

			  // when
			  _checker.sanityCheck( estimates, LATEST_RECORD_FORMATS, _baseMemory, _memoryUser1, _memoryUser2 );

			  // then
			  verify( _monitor ).insufficientHeapSize( anyLong(), anyLong() );
			  verifyNoMoreInteractions( _monitor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportAbundantHeapSize()
		 public virtual void ShouldReportAbundantHeapSize()
		 {
			  // given
			  when( _freeMemorySupplier.AsLong ).thenReturn( gibiBytes( 2 ) );
			  when( _actualHeapSizeSupplier.AsLong ).thenReturn( gibiBytes( 20 ) );
			  when( _baseMemorySupplier.AsLong ).thenReturn( gibiBytes( 1 ) );
			  when( _memoryUser1Supplier.AsLong ).thenReturn( gibiBytes( 1 ) );
			  when( _memoryUser2Supplier.AsLong ).thenReturn( gibiBytes( 1 ) );
			  Neo4Net.@unsafe.Impl.Batchimport.input.Input_Estimates estimates = knownEstimates( 1_000_000_000, 10_000_000_000L, 2_000_000_000L, 0, gibiBytes( 50 ), gibiBytes( 100 ), 0 );

			  // when
			  _checker.sanityCheck( estimates, LATEST_RECORD_FORMATS, _baseMemory, _memoryUser1, _memoryUser2 );

			  // then
			  verify( _monitor ).abundantHeapSize( anyLong(), anyLong() );
			  verifyNoMoreInteractions( _monitor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportNothingOnGoodSetup()
		 public virtual void ShouldReportNothingOnGoodSetup()
		 {
			  // given
			  when( _freeMemorySupplier.AsLong ).thenReturn( gibiBytes( 10 ) );
			  when( _baseMemorySupplier.AsLong ).thenReturn( gibiBytes( 1 ) );
			  when( _memoryUser1Supplier.AsLong ).thenReturn( gibiBytes( 1 ) );
			  when( _memoryUser2Supplier.AsLong ).thenReturn( gibiBytes( 1 ) );

			  when( _actualHeapSizeSupplier.AsLong ).thenReturn( gibiBytes( 2 ) );
			  Neo4Net.@unsafe.Impl.Batchimport.input.Input_Estimates estimates = knownEstimates( 1_000_000_000, 10_000_000_000L, 2_000_000_000L, 0, gibiBytes( 50 ), gibiBytes( 100 ), 0 );

			  // when
			  _checker.sanityCheck( estimates, LATEST_RECORD_FORMATS, _baseMemory, _memoryUser1, _memoryUser2 );

			  // then
			  verifyNoMoreInteractions( _monitor );
		 }
	}

}
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
namespace Neo4Net.Kernel.impl.store.allocator
{
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;

	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class ReusableRecordsCompositeAllocatorTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allocateReusableRecordsAndSwitchToDefaultWhenExhausted()
		 public virtual void AllocateReusableRecordsAndSwitchToDefaultWhenExhausted()
		 {
			  DynamicRecord dynamicRecord1 = new DynamicRecord( 1 );
			  DynamicRecord dynamicRecord2 = new DynamicRecord( 2 );
			  DynamicRecordAllocator recordAllocator = mock( typeof( DynamicRecordAllocator ) );
			  Mockito.when( recordAllocator.NextRecord() ).thenReturn(dynamicRecord2);
			  ReusableRecordsCompositeAllocator compositeAllocator = new ReusableRecordsCompositeAllocator( singletonList( dynamicRecord1 ), recordAllocator );

			  assertSame( "Same as pre allocated record.", dynamicRecord1, compositeAllocator.NextRecord() );
			  assertSame( "Same as expected allocated record.", dynamicRecord2, compositeAllocator.NextRecord() );

		 }
	}

}
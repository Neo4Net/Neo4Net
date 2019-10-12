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
namespace Neo4Net.Kernel.impl.store.allocator
{
	using Test = org.junit.Test;

	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class ReusableRecordsAllocatorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allocatePreviouslyNotUsedRecord()
		 public virtual void AllocatePreviouslyNotUsedRecord()
		 {
			  DynamicRecord dynamicRecord = new DynamicRecord( 1 );
			  dynamicRecord.InUse = false;

			  ReusableRecordsAllocator recordsAllocator = new ReusableRecordsAllocator( 10, dynamicRecord );
			  DynamicRecord allocatedRecord = recordsAllocator.NextRecord();

			  assertSame( "Records should be the same.", allocatedRecord, dynamicRecord );
			  assertTrue( "Record should be marked as used.", allocatedRecord.InUse() );
			  assertTrue( "Record should be marked as created.", allocatedRecord.Created );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allocatePreviouslyUsedRecord()
		 public virtual void AllocatePreviouslyUsedRecord()
		 {
			  DynamicRecord dynamicRecord = new DynamicRecord( 1 );
			  dynamicRecord.InUse = true;

			  ReusableRecordsAllocator recordsAllocator = new ReusableRecordsAllocator( 10, dynamicRecord );
			  DynamicRecord allocatedRecord = recordsAllocator.NextRecord();

			  assertSame( "Records should be the same.", allocatedRecord, dynamicRecord );
			  assertTrue( "Record should be marked as used.", allocatedRecord.InUse() );
			  assertFalse( "Record should be marked as created.", allocatedRecord.Created );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void trackRecordsAvailability()
		 public virtual void TrackRecordsAvailability()
		 {
			  DynamicRecord dynamicRecord1 = new DynamicRecord( 1 );
			  DynamicRecord dynamicRecord2 = new DynamicRecord( 1 );

			  ReusableRecordsAllocator recordsAllocator = new ReusableRecordsAllocator( 10, dynamicRecord1, dynamicRecord2 );
			  assertSame( "Should be the same as first available record.", dynamicRecord1, recordsAllocator.NextRecord() );
			  assertTrue( "Should have second record.", recordsAllocator.HasNext() );
			  assertSame( "Should be the same as second available record.", dynamicRecord2, recordsAllocator.NextRecord() );
			  assertFalse( "Should be out of available records", recordsAllocator.HasNext() );
		 }
	}

}
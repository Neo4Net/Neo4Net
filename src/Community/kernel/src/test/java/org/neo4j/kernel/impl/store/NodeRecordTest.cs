using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.store
{
	using Test = org.junit.Test;

	using ReusableRecordsAllocator = Neo4Net.Kernel.impl.store.allocator.ReusableRecordsAllocator;
	using IdSequence = Neo4Net.Kernel.impl.store.id.IdSequence;
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsEqual.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.DynamicNodeLabels.allocateRecordsForDynamicLabels;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.DynamicNodeLabels.dynamicPointer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.DynamicRecord.dynamicRecord;

	public class NodeRecordTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void cloneShouldProduceExactCopy()
		 public virtual void CloneShouldProduceExactCopy()
		 {
			  // Given
			  long relId = 1337L;
			  long propId = 1338L;
			  long inlinedLabels = 12L;

			  NodeRecord node = new NodeRecord( 1L, false, relId, propId );
			  node.SetLabelField( inlinedLabels, asList( new DynamicRecord( 1L ), new DynamicRecord( 2L ) ) );
			  node.InUse = true;

			  // When
			  NodeRecord clone = node.Clone();

			  // Then
			  assertEquals( node.InUse(), clone.InUse() );
			  assertEquals( node.LabelField, clone.LabelField );
			  assertEquals( node.NextProp, clone.NextProp );
			  assertEquals( node.NextRel, clone.NextRel );

			  assertThat( clone.DynamicLabelRecords, equalTo( node.DynamicLabelRecords ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListLabelRecordsInUse()
		 public virtual void ShouldListLabelRecordsInUse()
		 {
			  // Given
			  NodeRecord node = new NodeRecord( 1, false, -1, -1 );
			  long inlinedLabels = 12L;
			  DynamicRecord dynamic1 = dynamicRecord( 1L, true );
			  DynamicRecord dynamic2 = dynamicRecord( 2L, true );
			  DynamicRecord dynamic3 = dynamicRecord( 3L, true );

			  node.SetLabelField( inlinedLabels, asList( dynamic1, dynamic2, dynamic3 ) );

			  dynamic3.InUse = false;

			  // When
			  IEnumerable<DynamicRecord> usedRecords = node.UsedDynamicLabelRecords;

			  // Then
			  assertThat( asList( usedRecords ), equalTo( asList( dynamic1, dynamic2 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldToStringBothUsedAndUnusedDynamicLabelRecords()
		 public virtual void ShouldToStringBothUsedAndUnusedDynamicLabelRecords()
		 {
			  // GIVEN
			  IdSequence ids = mock( typeof( IdSequence ) );
			  when( ids.NextId() ).thenReturn(1L, 2L);
			  ReusableRecordsAllocator recordAllocator = new ReusableRecordsAllocator( 30, new DynamicRecord( 1 ), new DynamicRecord( 2 ) );
			  NodeRecord node = NewUsedNodeRecord( 0 );
			  long labelId = 10_123;
			  // A dynamic label record
			  ICollection<DynamicRecord> existing = allocateRecordsForDynamicLabels( node.Id, new long[]{ labelId }, recordAllocator );
			  // and a deleted one as well (simulating some deleted labels)
			  DynamicRecord unused = NewDeletedDynamicRecord( ids.NextId() );
			  unused.InUse = false;
			  existing.Add( unused );
			  node.SetLabelField( dynamicPointer( existing ), existing );

			  // WHEN
			  string toString = node.ToString();

			  // THEN
			  assertThat( toString, containsString( labelId.ToString() ) );
			  assertThat( toString, containsString( unused.ToString() ) );
		 }

		 private DynamicRecord NewDeletedDynamicRecord( long id )
		 {
			  DynamicRecord record = new DynamicRecord( id );
			  record.InUse = false;
			  return record;
		 }

		 private NodeRecord NewUsedNodeRecord( long id )
		 {
			  NodeRecord node = new NodeRecord( id );
			  node.InUse = true;
			  return node;
		 }
	}

}
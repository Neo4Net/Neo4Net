using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.store
{
	using Test = org.junit.Test;


	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using PropertyBlock = Neo4Net.Kernel.impl.store.record.PropertyBlock;
	using PropertyRecord = Neo4Net.Kernel.impl.store.record.PropertyRecord;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class PropertyRecordTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addingDuplicatePropertyBlockShouldOverwriteExisting()
		 public virtual void AddingDuplicatePropertyBlockShouldOverwriteExisting()
		 {
			  // Given these things...
			  PropertyRecord record = new PropertyRecord( 1 );
			  PropertyBlock blockA = new PropertyBlock();
			  blockA.ValueBlocks = new long[1];
			  blockA.KeyIndexId = 2;
			  PropertyBlock blockB = new PropertyBlock();
			  blockB.ValueBlocks = new long[1];
			  blockB.KeyIndexId = 2; // also 2, thus a duplicate

			  // When we set the property block twice that have the same key
			  record.PropertyBlock = blockA;
			  record.PropertyBlock = blockB;

			  // Then the record should only contain a single block, because blockB overwrote blockA
			  IList<PropertyBlock> propertyBlocks = Iterables.asList( record );
			  assertThat( propertyBlocks, hasItem( blockB ) );
			  assertThat( propertyBlocks, hasSize( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIterateOverBlocks()
		 public virtual void ShouldIterateOverBlocks()
		 {
			  // GIVEN
			  PropertyRecord record = new PropertyRecord( 0 );
			  PropertyBlock[] blocks = new PropertyBlock[3];
			  for ( int i = 0; i < blocks.Length; i++ )
			  {
					blocks[i] = new PropertyBlock();
					record.AddPropertyBlock( blocks[i] );
			  }

			  // WHEN
			  IEnumerator<PropertyBlock> iterator = record.GetEnumerator();

			  // THEN
			  foreach ( PropertyBlock block in blocks )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( block, iterator.next() );
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( iterator.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRemoveBlocksDuringIteration()
		 public virtual void ShouldBeAbleToRemoveBlocksDuringIteration()
		 {
			  // GIVEN
			  PropertyRecord record = new PropertyRecord( 0 );
			  ISet<PropertyBlock> blocks = new HashSet<PropertyBlock>();
			  for ( int i = 0; i < 4; i++ )
			  {
					PropertyBlock block = new PropertyBlock();
					record.AddPropertyBlock( block );
					blocks.Add( block );
			  }

			  // WHEN
			  IEnumerator<PropertyBlock> iterator = record.GetEnumerator();
			  AssertIteratorRemoveThrowsIllegalState( iterator );

			  // THEN
			  int size = blocks.Count;
			  for ( int i = 0; i < size; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					PropertyBlock block = iterator.next();
					if ( i % 2 == 1 )
					{
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						 iterator.remove();
						 AssertIteratorRemoveThrowsIllegalState( iterator );
						 blocks.remove( block );
					}
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( iterator.hasNext() );

			  // and THEN there should only be the non-removed blocks left
			  assertEquals( blocks, Iterables.asSet( record ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addLoadedBlock()
		 public virtual void AddLoadedBlock()
		 {
			  PropertyRecord record = new PropertyRecord( 42 );

			  AddBlock( record, 1, 2 );
			  AddBlock( record, 3, 4 );

			  IList<PropertyBlock> blocks = Iterables.asList( record );
			  assertEquals( 2, blocks.Count );
			  assertEquals( 1, blocks[0].KeyIndexId );
			  assertEquals( 2, blocks[0].SingleValueInt );
			  assertEquals( 3, blocks[1].KeyIndexId );
			  assertEquals( 4, blocks[1].SingleValueInt );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addLoadedBlockFailsWhenTooManyBlocksAdded()
		 public virtual void AddLoadedBlockFailsWhenTooManyBlocksAdded()
		 {
			  PropertyRecord record = new PropertyRecord( 42 );

			  AddBlock( record, 1, 2 );
			  AddBlock( record, 3, 4 );
			  AddBlock( record, 5, 6 );
			  AddBlock( record, 7, 8 );

			  bool validationErrorDetected = false;
			  try
			  {
					AddBlock( record, 9, 10 );
			  }
			  catch ( AssertionError )
			  {
					validationErrorDetected = true;
			  }
			  assertTrue( "Assertion failure expected", validationErrorDetected );
		 }

		 private void AssertIteratorRemoveThrowsIllegalState( IEnumerator<PropertyBlock> iterator )
		 {
			  try
			  {
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
					iterator.remove();
					fail( "Should have failed" );
			  }
			  catch ( System.InvalidOperationException )
			  { // OK
			  }
		 }

		 private static void AddBlock( PropertyRecord record, int key, int value )
		 {
			  PropertyBlock block = new PropertyBlock();
			  PropertyStore.EncodeValue( block, key, Values.of( value ), null, null, true );
			  foreach ( long valueBlock in block.ValueBlocks )
			  {
					record.AddLoadedBlock( valueBlock );
			  }
		 }
	}

}
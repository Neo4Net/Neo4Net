using System.Text;

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
namespace Neo4Net.Kernel.impl.core
{
	using Test = org.junit.Test;

	using Node = Neo4Net.Graphdb.Node;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.format.standard.PropertyRecordFormat.DEFAULT_DATA_BLOCK_SIZE;

	public class TestLengthyArrayPacking : AbstractNeo4jTestCase
	{
		private bool InstanceFieldsInitialized = false;

		public TestLengthyArrayPacking()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_arrayRecordCounter = new ArrayRecordCounter( this );
			_stringRecordCounter = new StringRecordCounter( this );
		}

		 private const string SOME_MIXED_CHARS = "abc421#¤åäö(/&€";
		 private const string SOME_LATIN_1_CHARS = "abcdefghijklmnopqrstuvwxyz";
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void bitPackingOfLengthyArrays()
		 public virtual void BitPackingOfLengthyArrays()
		 {
			  long arrayRecordsBefore = DynamicArrayRecordsInUse();

			  // Store an int array which would w/o packing require two dynamic records
			  // 4*40 = 160B (assuming data size of 120B)
			  int[] arrayWhichUnpackedWouldFillTwoDynamicRecords = new int[40];
			  for ( int i = 0; i < arrayWhichUnpackedWouldFillTwoDynamicRecords.Length; i++ )
			  {
					arrayWhichUnpackedWouldFillTwoDynamicRecords[i] = i * i;
			  }
			  Node node = GraphDb.createNode();
			  string key = "the array";
			  node.SetProperty( key, arrayWhichUnpackedWouldFillTwoDynamicRecords );
			  NewTransaction();

			  // Make sure it only requires one dynamic record
			  assertEquals( arrayRecordsBefore + 1, DynamicArrayRecordsInUse() );
			  assertTrue( Arrays.Equals( arrayWhichUnpackedWouldFillTwoDynamicRecords, ( int[] ) node.GetProperty( key ) ) );
		 }

		 // Tests for strings, although the test class name suggests otherwise

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureLongLatin1StringUsesOneBytePerChar()
		 public virtual void MakeSureLongLatin1StringUsesOneBytePerChar()
		 {
			  string @string = StringOfLength( SOME_LATIN_1_CHARS, DEFAULT_DATA_BLOCK_SIZE * 2 - 1 );
			  MakeSureRightAmountOfDynamicRecordsUsed( @string, 2, _stringRecordCounter );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureLongUtf8StringUsesLessThanTwoBytesPerChar()
		 public virtual void MakeSureLongUtf8StringUsesLessThanTwoBytesPerChar()
		 {
			  string @string = StringOfLength( SOME_MIXED_CHARS, DEFAULT_DATA_BLOCK_SIZE + 10 );
			  MakeSureRightAmountOfDynamicRecordsUsed( @string, 2, _stringRecordCounter );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureLongLatin1StringArrayUsesOneBytePerChar()
		 public virtual void MakeSureLongLatin1StringArrayUsesOneBytePerChar()
		 {
			  // Exactly 120 bytes: 5b header + (19+4)*5. w/o compression 5+(19*2 + 4)*5
			  string[] stringArray = new string[5];
			  for ( int i = 0; i < stringArray.Length; i++ )
			  {
					stringArray[i] = StringOfLength( SOME_LATIN_1_CHARS, 19 );
			  }
			  MakeSureRightAmountOfDynamicRecordsUsed( stringArray, 1, _arrayRecordCounter );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureLongUtf8StringArrayUsesLessThanTwoBytePerChar()
		 public virtual void MakeSureLongUtf8StringArrayUsesLessThanTwoBytePerChar()
		 {
			  string[] stringArray = new string[7];
			  for ( int i = 0; i < stringArray.Length; i++ )
			  {
					stringArray[i] = StringOfLength( SOME_MIXED_CHARS, 20 );
			  }
			  MakeSureRightAmountOfDynamicRecordsUsed( stringArray, 2, _arrayRecordCounter );
		 }

		 private void MakeSureRightAmountOfDynamicRecordsUsed( object value, int expectedAddedDynamicRecords, DynamicRecordCounter recordCounter )
		 {
			  long stringRecordsBefore = recordCounter.Count();
			  Node node = GraphDb.createNode();
			  node.SetProperty( "name", value );
			  NewTransaction();
			  long stringRecordsAfter = recordCounter.Count();
			  assertEquals( stringRecordsBefore + expectedAddedDynamicRecords, stringRecordsAfter );
		 }

		 private string StringOfLength( string possibilities, int length )
		 {
			  StringBuilder builder = new StringBuilder();
			  for ( int i = 0; i < length; i++ )
			  {
					builder.Append( possibilities[i % possibilities.Length] );
			  }
			  return builder.ToString();
		 }

		 private interface DynamicRecordCounter
		 {
			  long Count();
		 }

		 private class ArrayRecordCounter : DynamicRecordCounter
		 {
			 private readonly TestLengthyArrayPacking _outerInstance;

			 public ArrayRecordCounter( TestLengthyArrayPacking outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override long Count()
			  {
					return outerInstance.DynamicArrayRecordsInUse();
			  }
		 }

		 private class StringRecordCounter : DynamicRecordCounter
		 {
			 private readonly TestLengthyArrayPacking _outerInstance;

			 public StringRecordCounter( TestLengthyArrayPacking outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override long Count()
			  {
					return outerInstance.DynamicStringRecordsInUse();
			  }
		 }

		 private DynamicRecordCounter _arrayRecordCounter;
		 private DynamicRecordCounter _stringRecordCounter;
	}

}
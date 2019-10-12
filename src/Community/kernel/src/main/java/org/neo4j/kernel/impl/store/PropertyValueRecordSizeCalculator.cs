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

	using BatchingIdSequence = Neo4Net.Kernel.impl.store.id.BatchingIdSequence;
	using PropertyBlock = Neo4Net.Kernel.impl.store.record.PropertyBlock;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;

	/// <summary>
	/// Calculates record size that property values will occupy if encoded into a <seealso cref="PropertyStore"/>.
	/// Contains state and is designed for multiple uses from a single thread only.
	/// Does actual encoding of property values, dry-run style.
	/// </summary>
	public class PropertyValueRecordSizeCalculator : System.Func<Value[], int>
	{
		 private readonly BatchingIdSequence _stringRecordIds = new BatchingIdSequence();
		 private readonly DynamicRecordAllocator _stringRecordCounter;
		 private readonly BatchingIdSequence _arrayRecordIds = new BatchingIdSequence();
		 private readonly DynamicRecordAllocator _arrayRecordCounter;

		 private readonly int _propertyRecordSize;
		 private readonly int _stringRecordSize;
		 private readonly int _arrayRecordSize;

		 public PropertyValueRecordSizeCalculator( PropertyStore propertyStore ) : this( propertyStore.RecordSize, propertyStore.StringStore.RecordSize, propertyStore.StringStore.RecordDataSize, propertyStore.ArrayStore.RecordSize, propertyStore.ArrayStore.RecordDataSize )
		 {
		 }

		 public PropertyValueRecordSizeCalculator( int propertyRecordSize, int stringRecordSize, int stringRecordDataSize, int arrayRecordSize, int arrayRecordDataSize )
		 {
			  this._propertyRecordSize = propertyRecordSize;
			  this._stringRecordSize = stringRecordSize;
			  this._arrayRecordSize = arrayRecordSize;
			  this._stringRecordCounter = new StandardDynamicRecordAllocator( _stringRecordIds, stringRecordDataSize );
			  this._arrayRecordCounter = new StandardDynamicRecordAllocator( _arrayRecordIds, arrayRecordDataSize );
		 }

		 public override int ApplyAsInt( Value[] values )
		 {
			  _stringRecordIds.reset();
			  _arrayRecordIds.reset();

			  int propertyRecordsUsed = 0;
			  int freeBlocksInCurrentRecord = 0;
			  foreach ( Value value in values )
			  {
					PropertyBlock block = new PropertyBlock();
					PropertyStore.EncodeValue( block, 0, value, _stringRecordCounter, _arrayRecordCounter, true );
					if ( block.ValueBlocks.Length > freeBlocksInCurrentRecord )
					{
						 propertyRecordsUsed++;
						 freeBlocksInCurrentRecord = PropertyType.PayloadSizeLongs;
					}
					freeBlocksInCurrentRecord -= block.ValueBlocks.Length;
			  }

			  int size = propertyRecordsUsed * _propertyRecordSize;
			  size += toIntExact( _stringRecordIds.peek() ) * _stringRecordSize;
			  size += toIntExact( _arrayRecordIds.peek() ) * _arrayRecordSize;
			  return size;
		 }
	}

}
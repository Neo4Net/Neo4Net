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
namespace Neo4Net.Kernel.impl.store.format
{
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.impl.store.record.LabelTokenRecord;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using PropertyBlock = Neo4Net.Kernel.impl.store.record.PropertyBlock;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.impl.store.record.PropertyRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.impl.store.record.RelationshipTypeTokenRecord;
	using RandomValues = Neo4Net.Values.Storable.RandomValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.abs;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;

	public class LimitedRecordGenerators : RecordGenerators
	{
		 internal const long NULL = -1;

		 private readonly RandomValues _random;
		 private readonly int _entityBits;
		 private readonly int _propertyBits;
		 private readonly int _nodeLabelBits;
		 private readonly int _tokenBits;
		 private readonly long _nullValue;
		 private readonly float _fractionNullValues;

		 public LimitedRecordGenerators( RandomValues random, int entityBits, int propertyBits, int nodeLabelBits, int tokenBits, long nullValue ) : this( random, entityBits, propertyBits, nodeLabelBits, tokenBits, nullValue, 0.2f )
		 {
		 }

		 public LimitedRecordGenerators( RandomValues random, int entityBits, int propertyBits, int nodeLabelBits, int tokenBits, long nullValue, float fractionNullValues )
		 {
			  this._random = random;
			  this._entityBits = entityBits;
			  this._propertyBits = propertyBits;
			  this._nodeLabelBits = nodeLabelBits;
			  this._tokenBits = tokenBits;
			  this._nullValue = nullValue;
			  this._fractionNullValues = fractionNullValues;
		 }

		 public override RecordGenerators_Generator<RelationshipTypeTokenRecord> RelationshipTypeToken()
		 {
			  return ( recordSize, format, recordId ) => ( new RelationshipTypeTokenRecord( toIntExact( recordId ) ) ).initialize( _random.nextBoolean(), RandomInt(_tokenBits) );
		 }

		 public override RecordGenerators_Generator<RelationshipGroupRecord> RelationshipGroup()
		 {
			  return ( recordSize, format, recordId ) => ( new RelationshipGroupRecord( recordId ) ).initialize( _random.nextBoolean(), RandomInt(_tokenBits), RandomLongOrOccasionallyNull(_entityBits), RandomLongOrOccasionallyNull(_entityBits), RandomLongOrOccasionallyNull(_entityBits), RandomLongOrOccasionallyNull(_entityBits), RandomLongOrOccasionallyNull(_entityBits) );
		 }

		 public override RecordGenerators_Generator<RelationshipRecord> Relationship()
		 {
			  return ( recordSize, format, recordId ) => ( new RelationshipRecord( recordId ) ).initialize( _random.nextBoolean(), RandomLongOrOccasionallyNull(_propertyBits), _random.nextLong(_entityBits), _random.nextLong(_entityBits), RandomInt(_tokenBits), RandomLongOrOccasionallyNull(_entityBits), RandomLongOrOccasionallyNull(_entityBits), RandomLongOrOccasionallyNull(_entityBits), RandomLongOrOccasionallyNull(_entityBits), _random.nextBoolean(), _random.nextBoolean() );
		 }

		 public override RecordGenerators_Generator<PropertyKeyTokenRecord> PropertyKeyToken()
		 {
			  return ( recordSize, format, recordId ) => ( new PropertyKeyTokenRecord( toIntExact( recordId ) ) ).initialize( _random.nextBoolean(), _random.Next(_tokenBits), abs(_random.Next()) );
		 }

		 public override RecordGenerators_Generator<PropertyRecord> Property()
		 {
			  return ( recordSize, format, recordId ) =>
			  {
				PropertyRecord record = new PropertyRecord( recordId );
				int maxProperties = _random.intBetween( 1, 4 );
				StandaloneDynamicRecordAllocator stringAllocator = new StandaloneDynamicRecordAllocator();
				StandaloneDynamicRecordAllocator arrayAllocator = new StandaloneDynamicRecordAllocator();
				record.InUse = true;
				int blocksOccupied = 0;
				for ( int i = 0; i < maxProperties && blocksOccupied < 4; )
				{
					 PropertyBlock block = new PropertyBlock();
					 // Dynamic records will not be written and read by the property record format,
					 // that happens in the store where it delegates to a "sub" store.
					 PropertyStore.EncodeValue( block, _random.Next( _tokenBits ), _random.nextValue(), stringAllocator, arrayAllocator, true );
					 int tentativeBlocksWithThisOne = blocksOccupied + block.ValueBlocks.length;
					 if ( tentativeBlocksWithThisOne <= 4 )
					 {
						  record.addPropertyBlock( block );
						  blocksOccupied = tentativeBlocksWithThisOne;
					 }
				}
				record.PrevProp = RandomLongOrOccasionallyNull( _propertyBits );
				record.NextProp = RandomLongOrOccasionallyNull( _propertyBits );
				return record;
			  };
		 }

		 public override RecordGenerators_Generator<NodeRecord> Node()
		 {
			  return ( recordSize, format, recordId ) => ( new NodeRecord( recordId ) ).initialize( _random.nextBoolean(), RandomLongOrOccasionallyNull(_propertyBits), _random.nextBoolean(), RandomLongOrOccasionallyNull(_entityBits), RandomLongOrOccasionallyNull(_nodeLabelBits, 0) );
		 }

		 public override RecordGenerators_Generator<LabelTokenRecord> LabelToken()
		 {
			  return ( recordSize, format, recordId ) => ( new LabelTokenRecord( toIntExact( recordId ) ) ).initialize( _random.nextBoolean(), _random.Next(_tokenBits) );
		 }

		 public override RecordGenerators_Generator<DynamicRecord> Dynamic()
		 {
			  return ( recordSize, format, recordId ) =>
			  {
				int dataSize = recordSize - format.RecordHeaderSize;
				int length = _random.nextBoolean() ? dataSize : _random.Next(dataSize);
				long next = length == dataSize ? RandomLong( _propertyBits ) : _nullValue;
				DynamicRecord record = ( new DynamicRecord( max( 1, recordId ) ) ).initialize( _random.nextBoolean(), _random.nextBoolean(), next, _random.Next(PropertyType.values().length), length );
				sbyte[] bytes = _random.nextByteArray( record.Length, record.Length ).asObjectCopy();
				record.Data = bytes;
				return record;
			  };
		 }

		 private int RandomInt( int maxBits )
		 {
			  int bits = _random.Next( maxBits + 1 );
			  int max = 1 << bits;
			  return _random.Next( max );
		 }

		 private long RandomLong( int maxBits )
		 {
			  int bits = _random.Next( maxBits + 1 );
			  long max = 1L << bits;
			  return _random.nextLong( max );
		 }

		 private long RandomLongOrOccasionallyNull( int maxBits )
		 {
			  return RandomLongOrOccasionallyNull( maxBits, NULL );
		 }

		 private long RandomLongOrOccasionallyNull( int maxBits, long nullValue )
		 {
			  return _random.nextFloat() < _fractionNullValues ? nullValue : RandomLong(maxBits);
		 }
	}

}
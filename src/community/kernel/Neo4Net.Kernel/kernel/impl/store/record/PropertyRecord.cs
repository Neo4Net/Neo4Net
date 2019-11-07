using System;
using System.Collections.Generic;
using System.Diagnostics;
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
namespace Neo4Net.Kernel.Impl.Store.Records
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.record.Record.NO_NEXT_PROPERTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.record.Record.NO_PREVIOUS_PROPERTY;

	/// <summary>
	/// PropertyRecord is a container for PropertyBlocks. PropertyRecords form
	/// a double linked list and each one holds one or more PropertyBlocks that
	/// are the actual property key/value pairs. Because PropertyBlocks are of
	/// variable length, a full PropertyRecord can be holding just one
	/// PropertyBlock.
	/// </summary>
	public class PropertyRecord : AbstractBaseRecord, IEnumerable<PropertyBlock>
	{
		 private const sbyte TYPE_NODE = 1;
		 private const sbyte TYPE_REL = 2;

		 private long _nextProp;
		 private long _prevProp;
		 // Holds the purely physical representation of the loaded properties in this record. This is so that
		 // RecordPropertyCursor is able to use this raw data without the rather heavy and bloated data structures
		 // of PropertyBlock and thereabouts. So when a property record is loaded only these blocks are read,
		 // the construction of all PropertyBlock instances are loaded lazily when they are first needed, loaded
		 // by ensureBlocksLoaded().
		 // Modifications to a property record are still done on the PropertyBlock abstraction and so it's also
		 // that data that gets written to the log and record when it's time to do so.
		 private readonly long[] _blocks = new long[PropertyType.PayloadSizeLongs];
		 private int _blocksCursor;

		 // These MUST ONLY be populated if we're accessing PropertyBlocks. On just loading this record only the
		 // next/prev and blocks should be filled.
		 private readonly PropertyBlock[] _blockRecords = new PropertyBlock[PropertyType.PayloadSizeLongs];
		 private bool _blocksLoaded;
		 private int _blockRecordsCursor;
		 private long _entityId;
		 private sbyte _entityType;
		 private IList<DynamicRecord> _deletedRecords;

		 public PropertyRecord( long id ) : base( id )
		 {
		 }

		 public PropertyRecord( long id, PrimitiveRecord primitive ) : base( id )
		 {
			  primitive.IdTo = this;
		 }

		 public virtual PropertyRecord Initialize( bool inUse, long prevProp, long nextProp )
		 {
			  base.Initialize( inUse );
			  this._prevProp = prevProp;
			  this._nextProp = nextProp;
			  this._deletedRecords = null;
			  this._blockRecordsCursor = _blocksCursor = 0;
			  this._blocksLoaded = false;
			  return this;
		 }

		 public override void Clear()
		 {
			  base.Initialize( false );
			  this._entityId = -1;
			  this._entityType = 0;
			  this._prevProp = NO_PREVIOUS_PROPERTY.intValue();
			  this._nextProp = NO_NEXT_PROPERTY.intValue();
			  this._deletedRecords = null;
			  this._blockRecordsCursor = _blocksCursor = 0;
			  this._blocksLoaded = false;
		 }

		 public virtual long NodeId
		 {
			 set
			 {
				  _entityType = TYPE_NODE;
				  _entityId = value;
			 }
			 get
			 {
				  if ( NodeSet )
				  {
						return _entityId;
				  }
				  return -1;
			 }
		 }

		 public virtual long RelId
		 {
			 set
			 {
				  _entityType = TYPE_REL;
				  _entityId = value;
			 }
			 get
			 {
				  if ( RelSet )
				  {
						return _entityId;
				  }
				  return -1;
			 }
		 }

		 public virtual bool NodeSet
		 {
			 get
			 {
				  return _entityType == TYPE_NODE;
			 }
		 }

		 public virtual bool RelSet
		 {
			 get
			 {
				  return _entityType == TYPE_REL;
			 }
		 }



		 public virtual long IEntityId
		 {
			 get
			 {
				  return _entityId;
			 }
		 }

		 /// <summary>
		 /// Gets the sum of the sizes of the blocks in this record, in bytes.
		 /// </summary>
		 public virtual int Size()
		 {
			  EnsureBlocksLoaded();
			  int result = 0;
			  for ( int i = 0; i < _blockRecordsCursor; i++ )
			  {
					result += _blockRecords[i].Size;
			  }
			  return result;
		 }

		 public virtual int NumberOfProperties()
		 {
			  EnsureBlocksLoaded();
			  return _blockRecordsCursor;
		 }

		 public override IEnumerator<PropertyBlock> Iterator()
		 {
			  EnsureBlocksLoaded();
			  return new IteratorAnonymousInnerClass( this );
		 }

		 private class IteratorAnonymousInnerClass : IEnumerator<PropertyBlock>
		 {
			 private readonly PropertyRecord _outerInstance;

			 public IteratorAnonymousInnerClass( PropertyRecord outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

						 // state for the Iterator aspect of this class.
			 private int blockRecordsIteratorCursor;
			 private bool canRemoveFromIterator;

			 public bool hasNext()
			 {
				  return blockRecordsIteratorCursor < _outerInstance.blockRecordsCursor;
			 }

			 public PropertyBlock next()
			 {
				  if ( !hasNext() )
				  {
						throw new NoSuchElementException();
				  }
				  canRemoveFromIterator = true;
				  return _outerInstance.blockRecords[blockRecordsIteratorCursor++];
			 }

			 public void remove()
			 {
				  if ( !canRemoveFromIterator )
				  {
						throw new System.InvalidOperationException( "cursor:" + blockRecordsIteratorCursor + " canRemove:" + canRemoveFromIterator );
				  }

				  if ( --_outerInstance.blockRecordsCursor > --blockRecordsIteratorCursor )
				  {
						_outerInstance.blockRecords[blockRecordsIteratorCursor] = _outerInstance.blockRecords[_outerInstance.blockRecordsCursor];
				  }
				  canRemoveFromIterator = false;
			 }
		 }

		 public virtual IList<DynamicRecord> DeletedRecords
		 {
			 get
			 {
				  return _deletedRecords != null ? _deletedRecords : Collections.emptyList();
			 }
		 }

		 public virtual void AddDeletedRecord( DynamicRecord record )
		 {
			  Debug.Assert( !record.InUse() );
			  if ( _deletedRecords == null )
			  {
					_deletedRecords = new LinkedList<DynamicRecord>();
			  }
			  _deletedRecords.Add( record );
		 }

		 public virtual void AddPropertyBlock( PropertyBlock block )
		 {
			  EnsureBlocksLoaded();
			  Debug.Assert( Size() + block.Size <= PropertyType.PayloadSize, "Exceeded capacity of property record " + this + ". My current size is reported as " + Size() + "The added block was " + block + );
						 " (note that size is " + block.Size + ")";

			  _blockRecords[_blockRecordsCursor++] = block;
		 }

		 /// <summary>
		 /// Reads blocks[] and constructs <seealso cref="PropertyBlock"/> instances from them, making that abstraction
		 /// available to the outside. Done the first time any PropertyBlock is needed or manipulated.
		 /// </summary>
		 private void EnsureBlocksLoaded()
		 {
			  if ( !_blocksLoaded )
			  {
					Debug.Assert( _blockRecordsCursor == 0 );
					// We haven't loaded the blocks yet, please do so now
					int index = 0;
					while ( index < _blocksCursor )
					{
						 PropertyType type = PropertyType.getPropertyTypeOrThrow( _blocks[index] );
						 PropertyBlock block = new PropertyBlock();
						 int length = type.calculateNumberOfBlocksUsed( _blocks[index] );
						 block.ValueBlocks = Arrays.copyOfRange( _blocks, index, index + length );
						 _blockRecords[_blockRecordsCursor++] = block;
						 index += length;
					}
					_blocksLoaded = true;
			  }
		 }

		 public virtual PropertyBlock PropertyBlock
		 {
			 set
			 {
				  RemovePropertyBlock( value.KeyIndexId );
				  AddPropertyBlock( value );
			 }
		 }

		 public virtual PropertyBlock GetPropertyBlock( int keyIndex )
		 {
			  EnsureBlocksLoaded();
			  for ( int i = 0; i < _blockRecordsCursor; i++ )
			  {
					PropertyBlock block = _blockRecords[i];
					if ( block.KeyIndexId == keyIndex )
					{
						 return block;
					}
			  }
			  return null;
		 }

		 public virtual PropertyBlock RemovePropertyBlock( int keyIndex )
		 {
			  EnsureBlocksLoaded();
			  for ( int i = 0; i < _blockRecordsCursor; i++ )
			  {
					if ( _blockRecords[i].KeyIndexId == keyIndex )
					{
						 PropertyBlock block = _blockRecords[i];
						 if ( --_blockRecordsCursor > i )
						 {
							  _blockRecords[i] = _blockRecords[_blockRecordsCursor];
						 }
						 return block;
					}
			  }
			  return null;
		 }

		 public virtual void ClearPropertyBlocks()
		 {
			  _blockRecordsCursor = 0;
		 }

		 public virtual long NextProp
		 {
			 get
			 {
				  return _nextProp;
			 }
			 set
			 {
				  this._nextProp = value;
			 }
		 }


		 public override string ToString()
		 {
			  StringBuilder buf = new StringBuilder();
			  buf.Append( "Property[" ).Append( Id ).Append( ",used=" ).Append( InUse() ).Append(",prev=").Append(_prevProp).Append(",next=").Append(_nextProp);

			  if ( _entityId != -1 )
			  {
					buf.Append( _entityType == TYPE_NODE ? ",node=" : ",rel=" ).Append( _entityId );
			  }

			  if ( _blocksLoaded )
			  {
					for ( int i = 0; i < _blockRecordsCursor; i++ )
					{
						 buf.Append( ',' ).Append( _blockRecords[i] );
					}
			  }
			  else
			  {
					buf.Append( ", (blocks not loaded)" );
			  }

			  if ( _deletedRecords != null )
			  {
					foreach ( DynamicRecord dyn in _deletedRecords )
					{
						 buf.Append( ", del:" ).Append( dyn );
					}
			  }

			  buf.Append( "]" );
			  return buf.ToString();
		 }

		 public virtual PrimitiveRecord Changed
		 {
			 set
			 {
				  value.IdTo = this;
			 }
		 }

		 public virtual long PrevProp
		 {
			 get
			 {
				  return _prevProp;
			 }
			 set
			 {
				  _prevProp = value;
			 }
		 }


		 public override PropertyRecord Clone()
		 {
			  PropertyRecord result = ( PropertyRecord ) ( new PropertyRecord( Id ) ).initialize( InUse() );
			  result._nextProp = _nextProp;
			  result._prevProp = _prevProp;
			  result._entityId = _entityId;
			  result._entityType = _entityType;
			  Array.Copy( _blocks, 0, result._blocks, 0, _blocks.Length );
			  result._blocksCursor = _blocksCursor;
			  for ( int i = 0; i < _blockRecordsCursor; i++ )
			  {
					result._blockRecords[i] = _blockRecords[i].clone();
			  }
			  result._blockRecordsCursor = _blockRecordsCursor;
			  result._blocksLoaded = _blocksLoaded;
			  if ( _deletedRecords != null )
			  {
					foreach ( DynamicRecord deletedRecord in _deletedRecords )
					{
						 result.AddDeletedRecord( deletedRecord.Clone() );
					}
			  }
			  return result;
		 }

		 public virtual long[] Blocks
		 {
			 get
			 {
				  return _blocks;
			 }
		 }

		 public virtual void AddLoadedBlock( long block )
		 {
			  Debug.Assert( _blocksCursor + 1 <= _blocks.Length, "Capacity of " + _blocks.Length + " exceeded" );
			  _blocks[_blocksCursor++] = block;
		 }

		 public virtual int BlockCapacity
		 {
			 get
			 {
				  return _blocks.Length;
			 }
		 }

		 public virtual int NumberOfBlocks
		 {
			 get
			 {
				  return _blocksCursor;
			 }
		 }
	}

}
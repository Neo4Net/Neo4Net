using System;
using System.Text;

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
namespace Neo4Net.Kernel.impl.store.record
{

	public class DynamicRecord : AbstractBaseRecord
	{
		 public static readonly sbyte[] NoData = new sbyte[0];
		 private const int MAX_BYTES_IN_TO_STRING = 8;
		 private const int MAX_CHARS_IN_TO_STRING = 16;

		 private sbyte[] _data;
		 private int _length;
		 private long _nextBlock;
		 private int _type;
		 private bool _startRecord;

		 /// @deprecated use <seealso cref="initialize(bool, bool, long, int, int)"/> instead. 
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 [Obsolete("use <seealso cref=\"initialize(bool, bool, long, int, int)\"/> instead.")]
		 public static DynamicRecord DynamicRecordConflict( long id, bool inUse )
		 {
			  DynamicRecord record = new DynamicRecord( id );
			  record.InUse = inUse;
			  return record;
		 }

		 /// @deprecated use <seealso cref="initialize(bool, bool, long, int, int)"/> instead. 
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 [Obsolete("use <seealso cref=\"initialize(bool, bool, long, int, int)\"/> instead.")]
		 public static DynamicRecord DynamicRecordConflict( long id, bool inUse, bool isStartRecord, long nextBlock, int type, sbyte[] data )
		 {
			  DynamicRecord record = new DynamicRecord( id );
			  record.InUse = inUse;
			  record.StartRecord = isStartRecord;
			  record.NextBlock = nextBlock;
			  record.SetType( type );
			  record.Data = data;
			  return record;
		 }

		 public DynamicRecord( long id ) : base( id )
		 {
		 }

		 public virtual DynamicRecord Initialize( bool inUse, bool isStartRecord, long nextBlock, int type, int length )
		 {
			  base.Initialize( inUse );
			  this._startRecord = isStartRecord;
			  this._nextBlock = nextBlock;
			  this._type = type;
			  this._data = NoData;
			  this._length = length;
			  return this;
		 }

		 public override void Clear()
		 {
			  Initialize( false, true, Record.NoNextBlock.intValue(), -1, 0 );
		 }

		 public virtual bool StartRecord
		 {
			 set
			 {
				  this._startRecord = value;
			 }
			 get
			 {
				  return _startRecord;
			 }
		 }


		 /// <returns> The <seealso cref="PropertyType"/> of this record or null if unset or non valid </returns>
		 public virtual PropertyType getType()
		 {
			  return PropertyType.getPropertyTypeOrNull( ( long )( this._type << 24 ) );
		 }

		 /// <returns> The <seealso cref="type"/> field of this record, as set by previous invocations to <seealso cref="setType(int)"/> or
		 /// <seealso cref="initialize(bool, bool, long, int, int)"/> </returns>
		 public virtual int TypeAsInt
		 {
			 get
			 {
				  return _type;
			 }
		 }

		 public virtual void SetType( int type )
		 {
			  this._type = type;
		 }

		 public virtual int Length
		 {
			 set
			 {
				  this._length = value;
			 }
			 get
			 {
				  return _length;
			 }
		 }

		 public virtual void SetInUse( bool inUse, int type )
		 {
			  this._type = type;
			  this.InUse = inUse;
		 }

		 public virtual sbyte[] Data
		 {
			 set
			 {
				  this._length = value.Length;
				  this._data = value;
			 }
			 get
			 {
				  return _data;
			 }
		 }



		 public virtual long NextBlock
		 {
			 get
			 {
				  return _nextBlock;
			 }
			 set
			 {
				  this._nextBlock = value;
			 }
		 }


		 public override string ToString()
		 {
			  StringBuilder buf = new StringBuilder();
			  buf.Append( "DynamicRecord[" ).Append( Id ).Append( ",used=" ).Append( InUse() ).Append(',').Append('(').Append(_length).Append("),type=");
			  PropertyType type = getType();
			  if ( type == null )
			  {
					buf.Append( this._type );
			  }
			  else
			  {
					buf.Append( type.name() );
			  }
			  buf.Append( ",data=" );
			  if ( type == PropertyType.STRING && _data.Length <= MAX_CHARS_IN_TO_STRING )
			  {
					buf.Append( '"' );
					buf.Append( PropertyStore.decodeString( _data ) );
					buf.Append( "\"," );
			  }
			  else
			  {
					buf.Append( "byte[" );
					if ( _data.Length <= MAX_BYTES_IN_TO_STRING )
					{
						 for ( int i = 0; i < _data.Length; i++ )
						 {
							  if ( i != 0 )
							  {
									buf.Append( ',' );
							  }
							  buf.Append( _data[i] );
						 }
					}
					else
					{
						 buf.Append( "size=" ).Append( _data.Length );
					}
					buf.Append( "]," );
			  }
			  buf.Append( "start=" ).Append( _startRecord );
			  buf.Append( ",next=" ).Append( _nextBlock ).Append( ']' );
			  return buf.ToString();
		 }

		 public override DynamicRecord Clone()
		 {
			  DynamicRecord clone = ( new DynamicRecord( Id ) ).initialize( InUse(), _startRecord, _nextBlock, _type, _length );
			  if ( _data != null )
			  {
					clone.Data = _data.Clone();
			  }
			  return clone;
		 }
	}

}
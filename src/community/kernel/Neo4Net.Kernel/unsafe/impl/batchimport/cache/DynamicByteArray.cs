using System;

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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.cache.HeapByteArray.get3ByteIntFromByteBuffer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.cache.HeapByteArray.get6BLongFromByteBuffer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.cache.HeapByteArray.get5BLongFromByteBuffer;

	public class DynamicByteArray : DynamicNumberArray<ByteArray>, ByteArray
	{
		 private readonly sbyte[] _defaultValue;
		 private readonly ByteBuffer _defaultValueConvenienceBuffer;

		 public DynamicByteArray( NumberArrayFactory factory, long chunkSize, sbyte[] defaultValue ) : base( factory, chunkSize, new ByteArray[0] )
		 {
			  this._defaultValue = defaultValue;
			  this._defaultValueConvenienceBuffer = ByteBuffer.wrap( defaultValue );
		 }

		 public override void Swap( long fromIndex, long toIndex )
		 {
			  ByteArray fromArray = At( fromIndex );
			  ByteArray toArray = At( toIndex );

			  // Byte-wise swap
			  for ( int i = 0; i < _defaultValue.Length; i++ )
			  {
					sbyte intermediary = fromArray.GetByte( fromIndex, i );
					fromArray.SetByte( fromIndex, i, toArray.GetByte( toIndex, i ) );
					toArray.SetByte( toIndex, i, intermediary );
			  }
		 }

		 public override void Get( long index, sbyte[] into )
		 {
			  ByteArray chunk = ChunkOrNullAt( index );
			  if ( chunk != null )
			  {
					chunk.Get( index, into );
			  }
			  else
			  {
					Array.Copy( _defaultValue, 0, into, 0, _defaultValue.Length );
			  }
		 }

		 public override sbyte GetByte( long index, int offset )
		 {
			  ByteArray chunk = ChunkOrNullAt( index );
			  return chunk != null ? chunk.GetByte( index, offset ) : _defaultValueConvenienceBuffer.get( offset );
		 }

		 public override short GetShort( long index, int offset )
		 {
			  ByteArray chunk = ChunkOrNullAt( index );
			  return chunk != null ? chunk.GetShort( index, offset ) : _defaultValueConvenienceBuffer.getShort( offset );
		 }

		 public override int GetInt( long index, int offset )
		 {
			  ByteArray chunk = ChunkOrNullAt( index );
			  return chunk != null ? chunk.GetInt( index, offset ) : _defaultValueConvenienceBuffer.getInt( offset );
		 }

		 public override int Get3ByteInt( long index, int offset )
		 {
			  ByteArray chunk = ChunkOrNullAt( index );
			  return chunk != null ? chunk.Get3ByteInt( index, offset ) : get3ByteIntFromByteBuffer( _defaultValueConvenienceBuffer, offset );
		 }

		 public override long Get5ByteLong( long index, int offset )
		 {
			  ByteArray chunk = ChunkOrNullAt( index );
			  return chunk != null ? chunk.Get5ByteLong( index, offset ) : get5BLongFromByteBuffer( _defaultValueConvenienceBuffer, offset );
		 }

		 public override long Get6ByteLong( long index, int offset )
		 {
			  ByteArray chunk = ChunkOrNullAt( index );
			  return chunk != null ? chunk.Get6ByteLong( index, offset ) : get6BLongFromByteBuffer( _defaultValueConvenienceBuffer, offset );
		 }

		 public override long GetLong( long index, int offset )
		 {
			  ByteArray chunk = ChunkOrNullAt( index );
			  return chunk != null ? chunk.GetLong( index, offset ) : _defaultValueConvenienceBuffer.getLong( offset );
		 }

		 public override void Set( long index, sbyte[] value )
		 {
			  At( index ).set( index, value );
		 }

		 public override void SetByte( long index, int offset, sbyte value )
		 {
			  At( index ).setByte( index, offset, value );
		 }

		 public override void SetShort( long index, int offset, short value )
		 {
			  At( index ).setShort( index, offset, value );
		 }

		 public override void SetInt( long index, int offset, int value )
		 {
			  At( index ).setInt( index, offset, value );
		 }

		 public override void Set5ByteLong( long index, int offset, long value )
		 {
			  At( index ).set5ByteLong( index, offset, value );
		 }

		 public override void Set6ByteLong( long index, int offset, long value )
		 {
			  At( index ).set6ByteLong( index, offset, value );
		 }

		 public override void SetLong( long index, int offset, long value )
		 {
			  At( index ).setLong( index, offset, value );
		 }

		 public override void Set3ByteInt( long index, int offset, int value )
		 {
			  At( index ).set3ByteInt( index, offset, value );
		 }

		 protected internal override ByteArray AddChunk( long chunkSize, long @base )
		 {
			  return Factory.newByteArray( chunkSize, _defaultValue, @base );
		 }
	}

}
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
namespace Neo4Net.Kernel.impl.store.kvstore
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.kvstore.BigEndianByteArrayBuffer.newBuffer;

	internal class KeyValueMerger : DataProvider
	{
		 private readonly DataProvider _first;
		 private readonly DataProvider _other;
		 // We copy from the two sources to these extra buffers in order to compare the keys,
		 // is there any way we could avoid this extra buffering?
		 private readonly BigEndianByteArrayBuffer _firstKey;
		 private readonly BigEndianByteArrayBuffer _firstValue;
		 private readonly BigEndianByteArrayBuffer _otherKey;
		 private readonly BigEndianByteArrayBuffer _otherValue;
		 private bool _firstAvail;
		 private bool _otherAvail;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: KeyValueMerger(DataProvider first, DataProvider other, int keySize, int valueSize) throws java.io.IOException
		 internal KeyValueMerger( DataProvider first, DataProvider other, int keySize, int valueSize )
		 {
			  this._firstAvail = ( this._first = first ).visit( _firstKey = newBuffer( keySize ), _firstValue = newBuffer( valueSize ) );
			  this._otherAvail = ( this._other = other ).visit( _otherKey = newBuffer( keySize ), _otherValue = newBuffer( valueSize ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visit(WritableBuffer key, WritableBuffer value) throws java.io.IOException
		 public override bool Visit( WritableBuffer key, WritableBuffer value )
		 {
			  if ( _firstAvail && _otherAvail )
			  {
					int cmp = _firstKey.compareTo( _otherKey.buffer );
					if ( cmp < 0 )
					{
						 _firstKey.read( key );
						 _firstValue.read( value );
						 _firstAvail = _first.visit( _firstKey, _firstValue );
					}
					else
					{
						 _otherKey.read( key );
						 _otherValue.read( value );
						 _otherAvail = _other.visit( _otherKey, _otherValue );
						 if ( cmp == 0 )
						 {
							  _firstAvail = _first.visit( _firstKey, _firstValue );
						 }
					}
					return true;
			  }
			  else if ( _firstAvail )
			  {
					_firstKey.read( key );
					_firstValue.read( value );
					_firstAvail = _first.visit( _firstKey, _firstValue );
					return true;
			  }
			  else if ( _otherAvail )
			  {
					_otherKey.read( key );
					_otherValue.read( value );
					_otherAvail = _other.visit( _otherKey, _otherValue );
					return true;
			  }
			  else
			  {
					return false;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  using ( System.IDisposable ignored = _other )
			  {
					_first.Dispose();
			  }
		 }
	}

}
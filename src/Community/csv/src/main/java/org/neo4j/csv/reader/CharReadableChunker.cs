using System;
using System.Diagnostics;

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
namespace Neo4Net.Csv.Reader
{

	/// <summary>
	/// Chunks up a <seealso cref="CharReadable"/>.
	/// </summary>
	public abstract class CharReadableChunker : Chunker
	{
		public abstract bool NextChunk( Source_Chunk chunk );
		 protected internal readonly CharReadable Reader;
		 protected internal readonly int ChunkSize;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal volatile long PositionConflict;
		 private char[] _backBuffer; // grows on demand
		 private int _backBufferCursor;

		 public CharReadableChunker( CharReadable reader, int chunkSize )
		 {
			  this.Reader = reader;
			  this.ChunkSize = chunkSize;
			  this._backBuffer = new char[chunkSize >> 4];
		 }

		 public override ChunkImpl NewChunk()
		 {
			  return new ChunkImpl( new char[ChunkSize] );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  Reader.Dispose();
		 }

		 public override long Position()
		 {
			  return PositionConflict;
		 }

		 protected internal virtual int FillFromBackBuffer( char[] into )
		 {
			  if ( _backBufferCursor > 0 )
			  { // Read from and reset back buffer
					Debug.Assert( _backBufferCursor < ChunkSize );
					Array.Copy( _backBuffer, 0, into, 0, _backBufferCursor );
					int result = _backBufferCursor;
					_backBufferCursor = 0;
					return result;
			  }
			  return 0;
		 }

		 protected internal virtual int StoreInBackBuffer( char[] data, int offset, int length )
		 {
			  Array.Copy( data, offset, BackBuffer( length ), _backBufferCursor, length );
			  _backBufferCursor += length;
			  return length;
		 }

		 private char[] BackBuffer( int length )
		 {
			  if ( _backBufferCursor + length > _backBuffer.Length )
			  {
					_backBuffer = Arrays.copyOf( _backBuffer, _backBufferCursor + length );
			  }
			  return _backBuffer;
		 }

		 public class ChunkImpl : Source_Chunk
		 {
			  internal readonly char[] Buffer;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int LengthConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal string SourceDescriptionConflict;

			  public ChunkImpl( char[] buffer )
			  {
					this.Buffer = buffer;
			  }

			  public virtual void Initialize( int length, string sourceDescription )
			  {
					this.LengthConflict = length;
					this.SourceDescriptionConflict = sourceDescription;
			  }

			  public override int StartPosition()
			  {
					return 0;
			  }

			  public override string SourceDescription()
			  {
					return SourceDescriptionConflict;
			  }

			  public override int MaxFieldSize()
			  {
					return Buffer.Length;
			  }

			  public override int Length()
			  {
					return LengthConflict;
			  }

			  public override char[] Data()
			  {
					return Buffer;
			  }

			  public override int BackPosition()
			  {
					return 0;
			  }
		 }
	}

}
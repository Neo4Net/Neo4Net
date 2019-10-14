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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using Neo4Net.Index.Internal.gbptree;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

	internal class IndexKeyStorage<KEY> : SimpleEntryStorage<KEY, IndexKeyStorage.KeyEntryCursor<KEY>> where KEY : NativeIndexKey<KEY>
	{
		 private const sbyte KEY_TYPE = 1;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.neo4j.index.internal.gbptree.Layout<KEY,?> layout;
		 private readonly Layout<KEY, ?> _layout;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: IndexKeyStorage(org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File file, ByteBufferFactory.Allocator byteBufferFactory, int blockSize, org.neo4j.index.internal.gbptree.Layout<KEY,?> layout) throws java.io.IOException
		 internal IndexKeyStorage<T1>( FileSystemAbstraction fs, File file, ByteBufferFactory.Allocator byteBufferFactory, int blockSize, Layout<T1> layout ) : base( fs, file, byteBufferFactory, blockSize )
		 {
			  this._layout = layout;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void add(KEY key, org.neo4j.io.pagecache.PageCursor pageCursor) throws java.io.IOException
		 internal override void Add( KEY key, PageCursor pageCursor )
		 {
			  int entrySize = TYPE_SIZE + BlockEntry.KeySize( _layout, key );
			  prepareWrite( entrySize );
			  pageCursor.PutByte( KEY_TYPE );
			  BlockEntry.Write( pageCursor, _layout, key );
		 }

		 internal override KeyEntryCursor<KEY> Reader( PageCursor pageCursor )
		 {
			  return new KeyEntryCursor<KEY>( pageCursor, _layout );
		 }

		 internal class KeyEntryCursor<KEY> : BlockEntryCursor<KEY, Void>
		 {
			  internal readonly PageCursor PageCursor;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.neo4j.index.internal.gbptree.Layout<KEY,?> layout;
			  internal readonly Layout<KEY, ?> Layout;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly KEY KeyConflict;

			  internal KeyEntryCursor<T1>( PageCursor pageCursor, Layout<T1> layout )
			  {
					this.PageCursor = pageCursor;
					this.Layout = layout;
					this.KeyConflict = layout.NewKey();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
			  public override bool Next()
			  {
					sbyte type = PageCursor.Byte;
					if ( type == STOP_TYPE )
					{
						 return false;
					}
					if ( type != KEY_TYPE )
					{
						 throw new Exception( format( "Unexpected entry type. Expected %d or %d, but was %d.", STOP_TYPE, KEY_TYPE, type ) );
					}
					BlockEntry.Read( PageCursor, Layout, KeyConflict );
					return true;
			  }

			  public override KEY Key()
			  {
					return KeyConflict;
			  }

			  public override Void Value()
			  {
					return null;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
			  public override void Close()
			  {
					PageCursor.close();
			  }
		 }
	}

}
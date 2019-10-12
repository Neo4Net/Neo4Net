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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{

	using Org.Neo4j.Index.@internal.gbptree;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using Org.Neo4j.Kernel.Api.Index;
	using UpdateMode = Org.Neo4j.Kernel.Impl.Api.index.UpdateMode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.NativeIndexUpdater.initializeKeyAndValueFromUpdate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.NativeIndexUpdater.initializeKeyFromUpdate;

	/// <summary>
	/// Buffer <seealso cref="IndexEntryUpdate"/> by writing them out to a file. Can be read back in insert order through <seealso cref="reader()"/>.
	/// </summary>
	public class IndexUpdateStorage<KEY, VALUE> : SimpleEntryStorage<IndexEntryUpdate<JavaToDotNetGenericWildcard>, IndexUpdateCursor<KEY, VALUE>> where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		 private readonly Layout<KEY, VALUE> _layout;
		 private readonly KEY _key1;
		 private readonly KEY _key2;
		 private readonly VALUE _value;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: IndexUpdateStorage(org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File file, ByteBufferFactory.Allocator byteBufferFactory, int blockSize, org.neo4j.index.internal.gbptree.Layout<KEY,VALUE> layout) throws java.io.IOException
		 internal IndexUpdateStorage( FileSystemAbstraction fs, File file, ByteBufferFactory.Allocator byteBufferFactory, int blockSize, Layout<KEY, VALUE> layout ) : base( fs, file, byteBufferFactory, blockSize )
		 {
			  this._layout = layout;
			  this._key1 = layout.NewKey();
			  this._key2 = layout.NewKey();
			  this._value = layout.NewValue();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void add(org.neo4j.kernel.api.index.IndexEntryUpdate<?> update, org.neo4j.io.pagecache.PageCursor pageCursor) throws java.io.IOException
		 public override void Add<T1>( IndexEntryUpdate<T1> update, PageCursor pageCursor )
		 {
			  int entrySize = TYPE_SIZE;
			  UpdateMode updateMode = update.UpdateMode();
			  switch ( updateMode.innerEnumValue )
			  {
			  case UpdateMode.InnerEnum.ADDED:
					initializeKeyAndValueFromUpdate( _key1, _value, update.EntityId, update.Values() );
					entrySize += BlockEntry.EntrySize( _layout, _key1, _value );
					break;
			  case UpdateMode.InnerEnum.REMOVED:
					initializeKeyFromUpdate( _key1, update.EntityId, update.Values() );
					entrySize += BlockEntry.KeySize( _layout, _key1 );
					break;
			  case UpdateMode.InnerEnum.CHANGED:
					initializeKeyFromUpdate( _key1, update.EntityId, update.BeforeValues() );
					initializeKeyAndValueFromUpdate( _key2, _value, update.EntityId, update.Values() );
					entrySize += BlockEntry.KeySize( _layout, _key1 ) + BlockEntry.EntrySize( _layout, _key2, _value );
					break;
			  default:
					throw new System.ArgumentException( "Unknown update mode " + updateMode );
			  }

			  prepareWrite( entrySize );

			  pageCursor.PutByte( ( sbyte ) updateMode.ordinal() );
			  IndexUpdateEntry.Write( pageCursor, _layout, updateMode, _key1, _key2, _value );
		 }

		 public override IndexUpdateCursor<KEY, VALUE> Reader( PageCursor pageCursor )
		 {
			  return new IndexUpdateCursor<KEY, VALUE>( pageCursor, _layout );
		 }
	}

}
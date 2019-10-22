using System.Collections.Generic;

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

	using Neo4Net.GraphDb;
	using Neo4Net.Helpers.Collections;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexUpdateMode = Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asResourceIterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.iterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.NativeIndexPopulator.BYTE_ONLINE;

	public abstract class NativeIndexAccessor<KEY, VALUE> : NativeIndex<KEY, VALUE>, IndexAccessor where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		public override abstract bool ConsistencyCheck( Neo4Net.Kernel.Impl.Annotations.ReporterFactory reporterFactory );
		public abstract void PutAllNoOverwrite( IDictionary<string, Neo4Net.Values.Storable.Value> target, IDictionary<string, Neo4Net.Values.Storable.Value> source );
		public abstract IDictionary<string, Neo4Net.Values.Storable.Value> IndexConfig();
		public abstract void ValidateBeforeCommit( Neo4Net.Values.Storable.Value[] tuple );
		 private readonly NativeIndexUpdater<KEY, VALUE> _singleUpdater;
		 internal readonly NativeIndexHeaderWriter HeaderWriter;

		 internal NativeIndexAccessor( PageCache pageCache, FileSystemAbstraction fs, File storeFile, IndexLayout<KEY, VALUE> layout, IndexProvider.Monitor monitor, StoreIndexDescriptor descriptor, System.Action<PageCursor> additionalHeaderWriter, bool readOnly ) : base( pageCache, fs, storeFile, layout, monitor, descriptor, readOnly )
		 {
			  _singleUpdater = new NativeIndexUpdater<KEY, VALUE>( layout.newKey(), layout.NewValue() );
			  HeaderWriter = new NativeIndexHeaderWriter( BYTE_ONLINE, additionalHeaderWriter );
		 }

		 public override void Drop()
		 {
			  closeTree();
			  try
			  {
					fileSystem.deleteFileOrThrow( storeFile );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override NativeIndexUpdater<KEY, VALUE> NewUpdater( IndexUpdateMode mode )
		 {
			  assertOpen();
			  try
			  {
					return _singleUpdater.initialize( tree.writer() );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void Force( IOLimiter ioLimiter )
		 {
			  tree.checkpoint( ioLimiter );
		 }

		 public override void Refresh()
		 {
			  // not required in this implementation
		 }

		 public override void Close()
		 {
			  closeTree();
		 }

		 public virtual bool Dirty
		 {
			 get
			 {
				  return tree.wasDirtyOnStartup();
			 }
		 }

		 public override abstract IndexReader NewReader();

		 public override BoundedIterable<long> NewAllEntriesReader()
		 {
			  return new NativeAllEntriesReader<long>( tree, layout );
		 }

		 public override ResourceIterator<File> SnapshotFiles()
		 {
			  return asResourceIterator( iterator( storeFile ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyDeferredConstraints(org.Neo4Net.storageengine.api.NodePropertyAccessor nodePropertyAccessor) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void VerifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor )
		 { // Not needed since uniqueness is verified automatically w/o cost for every update.
		 }
	}

}
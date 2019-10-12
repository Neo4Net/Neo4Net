using System.Collections.Generic;

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

	using Org.Neo4j.Graphdb;
	using Org.Neo4j.Helpers.Collection;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexAccessor = Org.Neo4j.Kernel.Api.Index.IndexAccessor;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using IndexUpdateMode = Org.Neo4j.Kernel.Impl.Api.index.IndexUpdateMode;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asResourceIterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.iterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.NativeIndexPopulator.BYTE_ONLINE;

	public abstract class NativeIndexAccessor<KEY, VALUE> : NativeIndex<KEY, VALUE>, IndexAccessor where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		public override abstract bool ConsistencyCheck( Org.Neo4j.Kernel.Impl.Annotations.ReporterFactory reporterFactory );
		public abstract void PutAllNoOverwrite( IDictionary<string, Org.Neo4j.Values.Storable.Value> target, IDictionary<string, Org.Neo4j.Values.Storable.Value> source );
		public abstract IDictionary<string, Org.Neo4j.Values.Storable.Value> IndexConfig();
		public abstract void ValidateBeforeCommit( Org.Neo4j.Values.Storable.Value[] tuple );
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
//ORIGINAL LINE: public void verifyDeferredConstraints(org.neo4j.storageengine.api.NodePropertyAccessor nodePropertyAccessor) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void VerifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor )
		 { // Not needed since uniqueness is verified automatically w/o cost for every update.
		 }
	}

}
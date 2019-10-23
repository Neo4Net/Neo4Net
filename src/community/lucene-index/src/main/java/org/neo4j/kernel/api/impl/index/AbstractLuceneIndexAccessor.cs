using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.Api.Impl.Index
{
	using Document = org.apache.lucene.document.Document;


	using Neo4Net.GraphDb;
	using Neo4Net.Helpers.Collections;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using LuceneIndexReaderAcquisitionException = Neo4Net.Kernel.Api.Impl.Schema.LuceneIndexReaderAcquisitionException;
	using LuceneAllEntriesIndexAccessorReader = Neo4Net.Kernel.Api.Impl.Schema.reader.LuceneAllEntriesIndexAccessorReader;
	using LuceneIndexWriter = Neo4Net.Kernel.Api.Impl.Schema.writer.LuceneIndexWriter;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using Neo4Net.Kernel.Api.Index;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using ReporterFactory = Neo4Net.Kernel.Impl.Annotations.ReporterFactory;
	using IndexUpdateMode = Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode;
	using NodePropertyAccessor = Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using IndexReader = Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader;
	using Value = Neo4Net.Values.Storable.Value;

	public abstract class AbstractLuceneIndexAccessor<READER, INDEX> : IndexAccessor where READER : Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader where INDEX : DatabaseIndex<READER>
	{
		public abstract void PutAllNoOverwrite( IDictionary<string, Value> target, IDictionary<string, Value> source );
		public abstract IDictionary<string, Value> IndexConfig();
		public abstract void ValidateBeforeCommit( Value[] tuple );
		public abstract BoundedIterable<long> NewAllEntriesReader();
		 protected internal readonly LuceneIndexWriter Writer;
		 protected internal readonly INDEX LuceneIndex;
		 protected internal readonly IndexDescriptor Descriptor;

		 protected internal AbstractLuceneIndexAccessor( INDEX luceneIndex, IndexDescriptor descriptor )
		 {
			  this.Writer = luceneIndex.ReadOnly ? null : luceneIndex.IndexWriter;
			  this.LuceneIndex = luceneIndex;
			  this.Descriptor = descriptor;
		 }

		 public override IndexUpdater NewUpdater( IndexUpdateMode mode )
		 {
			  if ( LuceneIndex.ReadOnly )
			  {
					throw new System.NotSupportedException( "Can't create updater for read only index." );
			  }
			  return GetIndexUpdater( mode );
		 }

		 protected internal abstract IndexUpdater GetIndexUpdater( IndexUpdateMode mode );

		 public override void Drop()
		 {
			  LuceneIndex.drop();
		 }

		 public override void Force( IOLimiter ioLimiter )
		 {
			  try
			  {
					// We never change status of read-only indexes.
					if ( !LuceneIndex.ReadOnly )
					{
						 LuceneIndex.markAsOnline();
					}
					LuceneIndex.maybeRefreshBlocking();
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void Refresh()
		 {
			  try
			  {
					LuceneIndex.maybeRefreshBlocking();
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void Close()
		 {
			  try
			  {
					LuceneIndex.close();
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override READER NewReader()
		 {
			  try
			  {
					return LuceneIndex.IndexReader;
			  }
			  catch ( IOException e )
			  {
					throw new LuceneIndexReaderAcquisitionException( "Can't acquire index reader", e );
			  }
		 }

		 public virtual BoundedIterable<long> NewAllEntriesReader( System.Func<Document, long> IEntityIdReader )
		 {
			  return new LuceneAllEntriesIndexAccessorReader( LuceneIndex.allDocumentsReader(), IEntityIdReader );
		 }

		 public override ResourceIterator<File> SnapshotFiles()
		 {
			  try
			  {
					return LuceneIndex.snapshot();
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void verifyDeferredConstraints(org.Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor propertyAccessor) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException;
		 public override abstract void VerifyDeferredConstraints( NodePropertyAccessor propertyAccessor );

		 public virtual bool Dirty
		 {
			 get
			 {
				  return !LuceneIndex.Valid;
			 }
		 }

		 public override bool ConsistencyCheck( ReporterFactory reporterFactory )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LuceneIndexConsistencyCheckVisitor visitor = reporterFactory.getClass(LuceneIndexConsistencyCheckVisitor.class);
			  LuceneIndexConsistencyCheckVisitor visitor = reporterFactory.GetClass( typeof( LuceneIndexConsistencyCheckVisitor ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean isConsistent = !isDirty();
			  bool isConsistent = !Dirty;
			  if ( !isConsistent )
			  {
					visitor.IsInconsistent( Descriptor );
			  }
			  return isConsistent;
		 }

		 protected internal abstract class AbstractLuceneIndexUpdater : IndexUpdater
		 {
			 private readonly AbstractLuceneIndexAccessor<READER, INDEX> _outerInstance;

			  internal readonly bool Idempotent;
			  internal readonly bool Refresh;

			  internal bool HasChanges;

			  protected internal AbstractLuceneIndexUpdater( AbstractLuceneIndexAccessor<READER, INDEX> outerInstance, bool idempotent, bool refresh )
			  {
				  this._outerInstance = outerInstance;
					this.Idempotent = idempotent;
					this.Refresh = refresh;
			  }

			  public override void Process<T1>( IndexEntryUpdate<T1> update )
			  {
					// we do not support adding partial entries
					Debug.Assert( update.IndexKey().schema().Equals(outerInstance.Descriptor.schema()) );

					switch ( update.UpdateMode() )
					{
					case ADDED:
						 if ( Idempotent )
						 {
							  AddIdempotent( update.EntityId, update.Values() );
						 }
						 else
						 {
							  Add( update.EntityId, update.Values() );
						 }
						 break;
					case CHANGED:
						 Change( update.EntityId, update.Values() );
						 break;
					case REMOVED:
						 Remove( update.EntityId );
						 break;
					default:
						 throw new System.NotSupportedException();
					}
					HasChanges = true;
			  }

			  public override void Close()
			  {
					if ( HasChanges && Refresh )
					{
						 try
						 {
							  outerInstance.LuceneIndex.maybeRefreshBlocking();
						 }
						 catch ( IOException e )
						 {
							  throw new UncheckedIOException( e );
						 }
					}
			  }

			  protected internal abstract void AddIdempotent( long nodeId, Value[] values );

			  protected internal abstract void Add( long nodeId, Value[] values );

			  protected internal abstract void Change( long nodeId, Value[] values );

			  protected internal abstract void Remove( long nodeId );
		 }
	}

}
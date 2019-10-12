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
namespace Org.Neo4j.Kernel.Impl.Index.Schema.tracking
{

	using Org.Neo4j.Graphdb;
	using Org.Neo4j.Helpers.Collection;
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexAccessor = Org.Neo4j.Kernel.Api.Index.IndexAccessor;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;
	using ReporterFactory = Org.Neo4j.Kernel.Impl.Annotations.ReporterFactory;
	using IndexUpdateMode = Org.Neo4j.Kernel.Impl.Api.index.IndexUpdateMode;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using Value = Org.Neo4j.Values.Storable.Value;

	public class TrackingReadersIndexAccessor : IndexAccessor
	{
		 private readonly IndexAccessor _accessor;
		 private static readonly AtomicLong _openReaders = new AtomicLong();
		 private static readonly AtomicLong _closedReaders = new AtomicLong();

		 public static long NumberOfOpenReaders()
		 {
			  return _openReaders.get();
		 }

		 public static long NumberOfClosedReaders()
		 {
			  return _closedReaders.get();
		 }

		 internal TrackingReadersIndexAccessor( IndexAccessor accessor )
		 {
			  this._accessor = accessor;
		 }

		 public override void Drop()
		 {
			  _accessor.drop();
		 }

		 public override IndexUpdater NewUpdater( IndexUpdateMode mode )
		 {
			  return _accessor.newUpdater( mode );
		 }

		 public override void Force( IOLimiter ioLimiter )
		 {
			  _accessor.force( ioLimiter );
		 }

		 public override void Refresh()
		 {
			  _accessor.refresh();
		 }

		 public override void Close()
		 {
			  _accessor.Dispose();
		 }

		 public override IndexReader NewReader()
		 {
			  _openReaders.incrementAndGet();
			  return new TrackingIndexReader( _accessor.newReader(), _closedReaders );
		 }

		 public override BoundedIterable<long> NewAllEntriesReader()
		 {
			  return _accessor.newAllEntriesReader();
		 }

		 public override ResourceIterator<File> SnapshotFiles()
		 {
			  return _accessor.snapshotFiles();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyDeferredConstraints(org.neo4j.storageengine.api.NodePropertyAccessor nodePropertyAccessor) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void VerifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor )
		 {
			  _accessor.verifyDeferredConstraints( nodePropertyAccessor );
		 }

		 public virtual bool Dirty
		 {
			 get
			 {
				  return _accessor.Dirty;
			 }
		 }

		 public override void ValidateBeforeCommit( Value[] tuple )
		 {
			  _accessor.validateBeforeCommit( tuple );
		 }

		 public override bool ConsistencyCheck( ReporterFactory reporterFactory )
		 {
			  return _accessor.consistencyCheck( reporterFactory );
		 }
	}

}
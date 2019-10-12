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
namespace Org.Neo4j.Kernel.Api.Index
{

	using Org.Neo4j.Graphdb;
	using Org.Neo4j.Helpers.Collection;
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using ReporterFactory = Org.Neo4j.Kernel.Impl.Annotations.ReporterFactory;
	using IndexUpdateMode = Org.Neo4j.Kernel.Impl.Api.index.IndexUpdateMode;
	using SwallowingIndexUpdater = Org.Neo4j.Kernel.Impl.Api.index.updater.SwallowingIndexUpdater;
	using ConsistencyCheckable = Org.Neo4j.Kernel.Impl.Index.Schema.ConsistencyCheckable;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using Value = Org.Neo4j.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.emptyResourceIterator;

	/// <summary>
	/// Used for online operation of an index.
	/// </summary>
	public interface IndexAccessor : System.IDisposable, IndexConfigProvider, ConsistencyCheckable
	{

		 /// <summary>
		 /// Deletes this index as well as closes all used external resources.
		 /// There will not be any interactions after this call.
		 /// </summary>
		 /// <exception cref="UncheckedIOException"> if unable to drop index. </exception>
		 void Drop();

		 /// <summary>
		 /// Return an updater for applying a set of changes to this index.
		 /// Updates must be visible in <seealso cref="newReader() readers"/> created after this update.
		 /// <para>
		 /// This is called with IndexUpdateMode.RECOVERY when starting up after
		 /// a crash or similar. Updates given then may have already been applied to this index, so
		 /// additional checks must be in place so that data doesn't get duplicated, but is idempotent.
		 /// </para>
		 /// </summary>
		 IndexUpdater NewUpdater( IndexUpdateMode mode );

		 /// <summary>
		 /// Forces this index to disk. Called at certain points from within Neo4j for example when
		 /// rotating the logical log. After completion of this call there cannot be any essential state that
		 /// hasn't been forced to disk.
		 /// </summary>
		 /// <param name="ioLimiter"> The <seealso cref="IOLimiter"/> to use for implementations living on top of <seealso cref="org.neo4j.io.pagecache.PageCache"/>. </param>
		 /// <exception cref="UncheckedIOException"> if there was a problem forcing the state to persistent storage. </exception>
		 void Force( IOLimiter ioLimiter );

		 /// <summary>
		 /// Refreshes this index, so that <seealso cref="newReader() readers"/> created after completion of this call
		 /// will see the latest updates. This happens automatically on closing <seealso cref="newUpdater(IndexUpdateMode)"/>
		 /// w/ <seealso cref="IndexUpdateMode.ONLINE"/>, but not guaranteed for <seealso cref="IndexUpdateMode.RECOVERY"/>.
		 /// Therefore this call is complementary for updates that has taken place with <seealso cref="IndexUpdateMode.RECOVERY"/>.
		 /// </summary>
		 /// <exception cref="UncheckedIOException"> if there was a problem refreshing the index. </exception>
		 void Refresh();

		 /// <summary>
		 /// Closes this index accessor. There will not be any interactions after this call.
		 /// After completion of this call there cannot be any essential state that hasn't been forced to disk.
		 /// </summary>
		 /// <exception cref="UncheckedIOException"> if unable to close index. </exception>
		 void Close();

		 /// <returns> a new <seealso cref="IndexReader"/> responsible for looking up results in the index. The returned
		 /// reader must honor repeatable reads. </returns>
		 IndexReader NewReader();

		 BoundedIterable<long> NewAllEntriesReader();

		 /// <summary>
		 /// Should return a full listing of all files needed by this index accessor to work with the index. The files
		 /// need to remain available until the resource iterator returned here is closed. This is used to duplicate created
		 /// indexes across clusters, among other things.
		 /// </summary>
		 ResourceIterator<File> SnapshotFiles();

		 /// <summary>
		 /// Verifies that each value in this index is unique.
		 /// Index is guaranteed to not change while this call executes.
		 /// </summary>
		 /// <param name="nodePropertyAccessor"> <seealso cref="NodePropertyAccessor"/> for accessing properties from database storage
		 /// in the event of conflicting values. </param>
		 /// <exception cref="IndexEntryConflictException"> for first detected uniqueness conflict, if any. </exception>
		 /// <exception cref="UncheckedIOException"> on error reading from source files. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void verifyDeferredConstraints(org.neo4j.storageengine.api.NodePropertyAccessor nodePropertyAccessor) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException;
		 void VerifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor );

		 /// <returns> true if index was not shutdown properly and its internal state is dirty, false otherwise </returns>
		 bool Dirty { get; }

		 /// <summary>
		 /// Validates the <seealso cref="Value value tuple"/> before transaction determines that it can commit.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void validateBeforeCommit(org.neo4j.values.storable.Value[] tuple)
	//	 {
	//		  // For most value types there are no specific validations to be made.
	//	 }
	}

	public static class IndexAccessor_Fields
	{
		 public static readonly IndexAccessor Empty = new IndexAccessor_Adapter();
	}

	 public class IndexAccessor_Adapter : IndexAccessor
	 {
		  public override void Drop()
		  {
		  }

		  public override IndexUpdater NewUpdater( IndexUpdateMode mode )
		  {
				return SwallowingIndexUpdater.INSTANCE;
		  }

		  public override void Force( IOLimiter ioLimiter )
		  {
		  }

		  public override void Refresh()
		  {
		  }

		  public override void Close()
		  {
		  }

		  public override IndexReader NewReader()
		  {
				return IndexReader.EMPTY;
		  }

		  public override BoundedIterable<long> NewAllEntriesReader()
		  {
				return new BoundedIterableAnonymousInnerClass( this );
		  }

		  private class BoundedIterableAnonymousInnerClass : BoundedIterable<long>
		  {
			  private readonly IndexAccessor_Adapter _outerInstance;

			  public BoundedIterableAnonymousInnerClass( IndexAccessor_Adapter outerInstance )
			  {
				  this.outerInstance = outerInstance;
			  }

			  public long maxCount()
			  {
					return 0;
			  }

			  public void close()
			  {
			  }

			  public IEnumerator<long> iterator()
			  {
					return emptyIterator();
			  }
		  }

		  public override ResourceIterator<File> SnapshotFiles()
		  {
				return emptyResourceIterator();
		  }

		  public override void VerifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor )
		  {
		  }

		  public virtual bool Dirty
		  {
			  get
			  {
					return false;
			  }
		  }

		  public override bool ConsistencyCheck( ReporterFactory reporterFactory )
		  {
				return true;
		  }
	 }

	 public class IndexAccessor_Delegator : IndexAccessor
	 {
		  internal readonly IndexAccessor Delegate;

		  public IndexAccessor_Delegator( IndexAccessor @delegate )
		  {
				this.Delegate = @delegate;
		  }

		  public override void Drop()
		  {
				Delegate.drop();
		  }

		  public override IndexUpdater NewUpdater( IndexUpdateMode mode )
		  {
				return Delegate.newUpdater( mode );
		  }

		  public override void Force( IOLimiter ioLimiter )
		  {
				Delegate.force( ioLimiter );
		  }

		  public override void Refresh()
		  {
				Delegate.refresh();
		  }

		  public override void Close()
		  {
				Delegate.Dispose();
		  }

		  public override IndexReader NewReader()
		  {
				return Delegate.newReader();
		  }

		  public override BoundedIterable<long> NewAllEntriesReader()
		  {
				return Delegate.newAllEntriesReader();
		  }

		  public override ResourceIterator<File> SnapshotFiles()
		  {
				return Delegate.snapshotFiles();
		  }

		  public override IDictionary<string, Value> IndexConfig()
		  {
				return Delegate.indexConfig();
		  }

		  public override string ToString()
		  {
				return Delegate.ToString();
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyDeferredConstraints(org.neo4j.storageengine.api.NodePropertyAccessor nodePropertyAccessor) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		  public override void VerifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor )
		  {
				Delegate.verifyDeferredConstraints( nodePropertyAccessor );
		  }

		  public virtual bool Dirty
		  {
			  get
			  {
					return Delegate.Dirty;
			  }
		  }

		  public override void ValidateBeforeCommit( Value[] tuple )
		  {
				Delegate.validateBeforeCommit( tuple );
		  }

		  public override bool ConsistencyCheck( ReporterFactory reporterFactory )
		  {
				return Delegate.consistencyCheck( reporterFactory );
		  }
	 }

}
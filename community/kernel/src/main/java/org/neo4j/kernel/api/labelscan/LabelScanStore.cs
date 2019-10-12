using System;

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
namespace Org.Neo4j.Kernel.api.labelscan
{

	using Org.Neo4j.Graphdb;
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using ConsistencyCheckable = Org.Neo4j.Kernel.Impl.Index.Schema.ConsistencyCheckable;
	using UnderlyingStorageException = Org.Neo4j.Kernel.impl.store.UnderlyingStorageException;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;
	using LabelScanReader = Org.Neo4j.Storageengine.Api.schema.LabelScanReader;

	/// <summary>
	/// Stores label-->nodes mappings. It receives updates in the form of condensed label->node transaction data
	/// and can iterate through all nodes for any given label.
	/// </summary>
	public interface LabelScanStore : Lifecycle, ConsistencyCheckable
	{

		 /// <summary>
		 /// From the point a <seealso cref="LabelScanReader"/> is created till it's <seealso cref="LabelScanReader.close() closed"/> the
		 /// contents it returns cannot change, i.e. it honors repeatable reads.
		 /// </summary>
		 /// <returns> a <seealso cref="LabelScanReader"/> capable of retrieving nodes for labels. </returns>
		 LabelScanReader NewReader();

		 /// <summary>
		 /// Acquire a writer for updating the store.
		 /// </summary>
		 /// <returns> <seealso cref="LabelScanWriter"/> which can modify the <seealso cref="LabelScanStore"/>. </returns>
		 LabelScanWriter NewWriter();

		 /// <summary>
		 /// Forces all changes to disk. Called at certain points from within Neo4j for example when
		 /// rotating the logical log. After completion of this call there cannot be any essential state that
		 /// hasn't been forced to disk.
		 /// </summary>
		 /// <exception cref="UnderlyingStorageException"> if there was a problem forcing the state to persistent storage. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void force(org.neo4j.io.pagecache.IOLimiter limiter) throws org.neo4j.kernel.impl.store.UnderlyingStorageException;
		 void Force( IOLimiter limiter );

		 /// <summary>
		 /// Acquire a reader for all <seealso cref="NodeLabelRange node label"/> ranges.
		 /// </summary>
		 /// <returns> the <seealso cref="AllEntriesLabelScanReader reader"/>. </returns>
		 AllEntriesLabelScanReader AllNodeLabelRanges();

		 ResourceIterator<File> SnapshotStoreFiles();

		 /// <returns> {@code true} if there's no data at all in this label scan store, otherwise {@code false}. </returns>
		 /// <exception cref="IOException"> on I/O error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean isEmpty() throws java.io.IOException;
		 bool Empty { get; }

		 /// <summary>
		 /// Initializes the store. After this has been called recovery updates can be processed.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void init() throws java.io.IOException;
		 void Init();

		 /// <summary>
		 /// Starts the store. After this has been called updates can be processed.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void start() throws java.io.IOException;
		 void Start();

		 void Stop();

		 /// <summary>
		 /// Shuts down the store and all resources acquired by it.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void shutdown() throws java.io.IOException;
		 void Shutdown();

		 /// <summary>
		 /// Drops any persistent storage backing this store.
		 /// </summary>
		 /// <exception cref="IOException"> on I/O error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void drop() throws java.io.IOException;
		 void Drop();

		 /// <returns> whether or not this index is read-only. </returns>
		 bool ReadOnly { get; }

		 /// <returns> whether or not there's an existing store present for this label scan store. </returns>
		 /// <exception cref="IOException"> on I/O error checking the presence of a store. </exception>
		 bool HasStore();

		 /// <summary>
		 /// Returns the path to label scan store, might be a directory or a file depending on the implementation.
		 /// </summary>
		 /// <returns> the directory or file where the label scan store is persisted. </returns>
		 File LabelScanStoreFile { get; }
	}

	 public interface LabelScanStore_Monitor
	 {

		  void Init();

		  void NoIndex();

		  void NotValidIndex();

		  void Rebuilding();

		  void Rebuilt( long roughNodeCount );

		  void RecoveryCleanupRegistered();

		  void RecoveryCleanupStarted();

		  void RecoveryCleanupFinished( long numberOfPagesVisited, long numberOfCleanedCrashPointers, long durationMillis );

		  void RecoveryCleanupClosed();

		  void RecoveryCleanupFailed( Exception throwable );
	 }

	 public static class LabelScanStore_Monitor_Fields
	 {
		  public static readonly LabelScanStore_Monitor Empty = new LabelScanStore_Monitor_Adaptor();
	 }

	  public class LabelScanStore_Monitor_Adaptor : LabelScanStore_Monitor
	  {
			public override void Init()
			{ // empty
			}

			public override void NoIndex()
			{ // empty
			}

			public override void NotValidIndex()
			{ // empty
			}

			public override void Rebuilding()
			{ // empty
			}

			public override void Rebuilt( long roughNodeCount )
			{ // empty
			}

			public override void RecoveryCleanupRegistered()
			{ // empty
			}

			public override void RecoveryCleanupStarted()
			{ // empty
			}

			public override void RecoveryCleanupFinished( long numberOfPagesVisited, long numberOfCleanedCrashPointers, long durationMillis )
			{ // empty
			}

			public override void RecoveryCleanupClosed()
			{ // empty
			}

			public override void RecoveryCleanupFailed( Exception throwable )
			{ // empty
			}
	  }

}
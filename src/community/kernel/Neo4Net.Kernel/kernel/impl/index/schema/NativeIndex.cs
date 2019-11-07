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
	using Neo4Net.Index.Internal.gbptree;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.Internal.gbptree.RecoveryCleanupWorkCollector;
	using IOUtils = Neo4Net.Io.IOUtils;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using ReporterFactory = Neo4Net.Kernel.Impl.Annotations.ReporterFactory;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.GBPTree.NO_HEADER_READER;

	internal abstract class NativeIndex<KEY, VALUE> : ConsistencyCheckable where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		 internal readonly PageCache PageCache;
		 internal readonly File StoreFile;
		 internal readonly IndexLayout<KEY, VALUE> Layout;
		 internal readonly FileSystemAbstraction FileSystem;
		 internal readonly StoreIndexDescriptor Descriptor;
		 private readonly IndexProvider.Monitor _monitor;
		 private readonly bool _readOnly;

		 protected internal GBPTree<KEY, VALUE> Tree;

		 internal NativeIndex( PageCache pageCache, FileSystemAbstraction fs, File storeFile, IndexLayout<KEY, VALUE> layout, IndexProvider.Monitor monitor, StoreIndexDescriptor descriptor, bool readOnly )
		 {
			  this.PageCache = pageCache;
			  this.StoreFile = storeFile;
			  this.Layout = layout;
			  this.FileSystem = fs;
			  this.Descriptor = descriptor;
			  this._monitor = monitor;
			  this._readOnly = readOnly;
		 }

		 internal virtual void InstantiateTree( RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, System.Action<PageCursor> headerWriter )
		 {
			  EnsureDirectoryExist();
			  GBPTree.Monitor monitor = TreeMonitor();
			  Tree = new GBPTree<KEY, VALUE>( PageCache, StoreFile, Layout, 0, monitor, NO_HEADER_READER, headerWriter, recoveryCleanupWorkCollector, _readOnly );
			  AfterTreeInstantiation( Tree );
		 }

		 protected internal virtual void AfterTreeInstantiation( GBPTree<KEY, VALUE> tree )
		 { // no-op per default
		 }

		 private GBPTree.Monitor TreeMonitor()
		 {
			  return new NativeIndexTreeMonitor( this );
		 }

		 private void EnsureDirectoryExist()
		 {
			  try
			  {
					FileSystem.mkdirs( StoreFile.ParentFile );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 internal virtual void CloseTree()
		 {
			  IOUtils.closeAllUnchecked( Tree );
			  Tree = null;
		 }

		 internal virtual void AssertOpen()
		 {
			  if ( Tree == null )
			  {
					throw new System.InvalidOperationException( "Index has been closed" );
			  }
		 }

		 public override bool ConsistencyCheck( ReporterFactory reporterFactory )
		 {
			  return ConsistencyCheck( reporterFactory.GetClass( typeof( GBPTreeConsistencyCheckVisitor ) ) );
		 }

		 private bool ConsistencyCheck( GBPTreeConsistencyCheckVisitor<KEY> visitor )
		 {
			  try
			  {
					return Tree.consistencyCheck( visitor );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 private class NativeIndexTreeMonitor : GBPTree.Monitor_Adaptor
		 {
			 private readonly NativeIndex<KEY, VALUE> _outerInstance;

			 public NativeIndexTreeMonitor( NativeIndex<KEY, VALUE> outerInstance ) : base( outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void CleanupRegistered()
			  {
					outerInstance.monitor.RecoveryCleanupRegistered( outerInstance.StoreFile, outerInstance.Descriptor );
			  }

			  public override void CleanupStarted()
			  {
					outerInstance.monitor.RecoveryCleanupStarted( outerInstance.StoreFile, outerInstance.Descriptor );
			  }

			  public override void CleanupFinished( long numberOfPagesVisited, long numberOfCleanedCrashPointers, long durationMillis )
			  {
					outerInstance.monitor.RecoveryCleanupFinished( outerInstance.StoreFile, outerInstance.Descriptor, numberOfPagesVisited, numberOfCleanedCrashPointers, durationMillis );
			  }

			  public override void CleanupClosed()
			  {
					outerInstance.monitor.RecoveryCleanupClosed( outerInstance.StoreFile, outerInstance.Descriptor );
			  }

			  public override void CleanupFailed( Exception throwable )
			  {
					outerInstance.monitor.RecoveryCleanupFailed( outerInstance.StoreFile, outerInstance.Descriptor, throwable );
			  }
		 }
	}

}
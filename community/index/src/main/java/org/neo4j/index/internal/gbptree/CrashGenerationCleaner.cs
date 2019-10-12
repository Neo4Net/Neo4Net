using System;
using System.Diagnostics;
using System.Threading;

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
namespace Org.Neo4j.Index.@internal.gbptree
{

	using Exceptions = Org.Neo4j.Helpers.Exceptions;
	using Monitor = Org.Neo4j.Index.@internal.gbptree.GBPTree.Monitor;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using PagedFile = Org.Neo4j.Io.pagecache.PagedFile;
	using FeatureToggles = Org.Neo4j.Util.FeatureToggles;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;

	/// <summary>
	/// Scans the entire tree and checks all GSPPs, replacing all CRASH gen GSPs with zeros.
	/// </summary>
	internal class CrashGenerationCleaner
	{
		 private const string NUMBER_OF_WORKERS_NAME = "number_of_workers";
		 private static readonly int _numberOfWorkersDefault = min( 8, Runtime.Runtime.availableProcessors() );
		 private static readonly int _numberOfWorkers = FeatureToggles.getInteger( typeof( CrashGenerationCleaner ), NUMBER_OF_WORKERS_NAME, _numberOfWorkersDefault );
		 private const string BATCH_TIMEOUT_NAME = "batch_timeout";
		 private const int BATCH_TIMEOUT_DEFAULT = 30;
		 private static readonly int _batchTimeout = FeatureToggles.getInteger( typeof( CrashGenerationCleaner ), BATCH_TIMEOUT_NAME, BATCH_TIMEOUT_DEFAULT );

		 private const long MIN_BATCH_SIZE = 10;
		 internal const long MAX_BATCH_SIZE = 100;
		 private readonly PagedFile _pagedFile;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final TreeNode<?,?> treeNode;
		 private readonly TreeNode<object, ?> _treeNode;
		 private readonly long _lowTreeNodeId;
		 private readonly long _highTreeNodeId;
		 private readonly long _stableGeneration;
		 private readonly long _unstableGeneration;
		 private readonly Monitor _monitor;

		 internal CrashGenerationCleaner<T1>( PagedFile pagedFile, TreeNode<T1> treeNode, long lowTreeNodeId, long highTreeNodeId, long stableGeneration, long unstableGeneration, Monitor monitor )
		 {
			  this._pagedFile = pagedFile;
			  this._treeNode = treeNode;
			  this._lowTreeNodeId = lowTreeNodeId;
			  this._highTreeNodeId = highTreeNodeId;
			  this._stableGeneration = stableGeneration;
			  this._unstableGeneration = unstableGeneration;
			  this._monitor = monitor;
		 }

		 private static long BatchSize( long pagesToClean, int threads )
		 {
			  // Batch size at most maxBatchSize, at least minBatchSize and trying to give each thread 100 batches each
			  return min( MAX_BATCH_SIZE, max( MIN_BATCH_SIZE, pagesToClean / ( 100L * threads ) ) );
		 }

		 // === Methods about the execution and threading ===

		 public virtual void Clean( ExecutorService executor )
		 {
			  _monitor.cleanupStarted();
			  Debug.Assert( _unstableGeneration > _stableGeneration, UnexpectedGenerations() );
			  Debug.Assert( _unstableGeneration - _stableGeneration > 1, UnexpectedGenerations() );

			  long startTime = currentTimeMillis();
			  long pagesToClean = _highTreeNodeId - _lowTreeNodeId;
			  int threads = _numberOfWorkers;
			  long batchSize = batchSize( pagesToClean, threads );
			  AtomicLong nextId = new AtomicLong( _lowTreeNodeId );
			  AtomicReference<Exception> error = new AtomicReference<Exception>();
			  AtomicInteger cleanedPointers = new AtomicInteger();
			  System.Threading.CountdownEvent activeThreadLatch = new System.Threading.CountdownEvent( threads );
			  for ( int i = 0; i < threads; i++ )
			  {
					executor.submit( Cleaner( nextId, batchSize, cleanedPointers, activeThreadLatch, error ) );
			  }

			  try
			  {
					long lastProgression = nextId.get();
					// Have max no-progress-timeout quite high to be able to cope with huge
					// I/O congestion spikes w/o failing in vain.
					while ( !activeThreadLatch.await( _batchTimeout, SECONDS ) )
					{
						 if ( lastProgression == nextId.get() )
						 {
							  // No progression at all, abort
							  error.compareAndSet( null, new IOException( "No progress, so forcing abort" ) );
						 }
						 lastProgression = nextId.get();
					}
			  }
			  catch ( InterruptedException )
			  {
					Thread.CurrentThread.Interrupt();
			  }

			  Exception finalError = error.get();
			  if ( finalError != null )
			  {
					Exceptions.throwIfUnchecked( finalError );
					throw new Exception( finalError );
			  }

			  long endTime = currentTimeMillis();
			  _monitor.cleanupFinished( pagesToClean, cleanedPointers.get(), endTime - startTime );
		 }

		 private ThreadStart Cleaner( AtomicLong nextId, long batchSize, AtomicInteger cleanedPointers, System.Threading.CountdownEvent activeThreadLatch, AtomicReference<Exception> error )
		 {
			  return () =>
			  {
				try
				{
					using ( PageCursor cursor = _pagedFile.io( 0, PagedFile.PF_SHARED_READ_LOCK ), PageCursor writeCursor = _pagedFile.io( 0, PagedFile.PF_SHARED_WRITE_LOCK ) )
					{
						 long localNextId;
						 while ( ( localNextId = nextId.getAndAdd( batchSize ) ) < _highTreeNodeId )
						 {
							  for ( int i = 0; i < batchSize && localNextId < _highTreeNodeId; i++, localNextId++ )
							  {
									PageCursorUtil.GoTo( cursor, "clean", localNextId );
   
									if ( HasCrashedGSPP( _treeNode, cursor ) )
									{
										 writeCursor.next( cursor.CurrentPageId );
										 CleanTreeNode( _treeNode, writeCursor, cleanedPointers );
									}
							  }
   
							  // Check error status after a batch, to reduce volatility overhead.
							  // Is this over thinking things? Perhaps
							  if ( error.get() != null )
							  {
									break;
							  }
						 }
					}
				}
				catch ( Exception e )
				{
					 error.accumulateAndGet( e, Exceptions.chain );
				}
				finally
				{
					 activeThreadLatch.Signal();
				}
			  };
		 }

		 // === Methods about checking if a tree node has crashed pointers ===

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean hasCrashedGSPP(TreeNode<?,?> treeNode, org.neo4j.io.pagecache.PageCursor cursor) throws java.io.IOException
		 private bool HasCrashedGSPP<T1>( TreeNode<T1> treeNode, PageCursor cursor )
		 {
			  bool isTreeNode;
			  int keyCount;
			  do
			  {
					isTreeNode = TreeNode.NodeType( cursor ) == TreeNode.NODE_TYPE_TREE_NODE;
					keyCount = TreeNode.KeyCount( cursor );
			  } while ( cursor.ShouldRetry() );
			  PageCursorUtil.CheckOutOfBounds( cursor );

			  if ( !isTreeNode )
			  {
					return false;
			  }

			  bool hasCrashed;
			  do
			  {
					hasCrashed = HasCrashedGSPP( cursor, TreeNode.BytePosSuccessor ) || HasCrashedGSPP( cursor, TreeNode.BytePosLeftsibling ) || HasCrashedGSPP( cursor, TreeNode.BytePosRightsibling );

					if ( !hasCrashed && TreeNode.IsInternal( cursor ) )
					{
						 for ( int i = 0; i <= keyCount && treeNode.ReasonableChildCount( i ) && !hasCrashed; i++ )
						 {
							  hasCrashed = HasCrashedGSPP( cursor, treeNode.ChildOffset( i ) );
						 }
					}
			  } while ( cursor.ShouldRetry() );
			  PageCursorUtil.CheckOutOfBounds( cursor );
			  return hasCrashed;
		 }

		 private bool HasCrashedGSPP( PageCursor cursor, int gsppOffset )
		 {
			  return HasCrashedGSP( cursor, gsppOffset ) || HasCrashedGSP( cursor, gsppOffset + GenerationSafePointer.Size );
		 }

		 private bool HasCrashedGSP( PageCursor cursor, int offset )
		 {
			  cursor.Offset = offset;
			  long generation = GenerationSafePointer.ReadGeneration( cursor );
			  return generation > _stableGeneration && generation < _unstableGeneration;
		 }

		 // === Methods about actually cleaning a discovered crashed tree node ===

		 private void CleanTreeNode<T1>( TreeNode<T1> treeNode, PageCursor cursor, AtomicInteger cleanedPointers )
		 {
			  CleanCrashedGSPP( cursor, TreeNode.BytePosSuccessor, cleanedPointers );
			  CleanCrashedGSPP( cursor, TreeNode.BytePosLeftsibling, cleanedPointers );
			  CleanCrashedGSPP( cursor, TreeNode.BytePosRightsibling, cleanedPointers );

			  if ( TreeNode.IsInternal( cursor ) )
			  {
					int keyCount = TreeNode.KeyCount( cursor );
					for ( int i = 0; i <= keyCount && treeNode.ReasonableChildCount( i ); i++ )
					{
						 CleanCrashedGSPP( cursor, treeNode.ChildOffset( i ), cleanedPointers );
					}
			  }
		 }

		 private void CleanCrashedGSPP( PageCursor cursor, int gsppOffset, AtomicInteger cleanedPointers )
		 {
			  CleanCrashedGSP( cursor, gsppOffset, cleanedPointers );
			  CleanCrashedGSP( cursor, gsppOffset + GenerationSafePointer.Size, cleanedPointers );
		 }

		 /// <summary>
		 /// NOTE: No shouldRetry is used because cursor is assumed to be a write cursor.
		 /// </summary>
		 private void CleanCrashedGSP( PageCursor cursor, int gspOffset, AtomicInteger cleanedPointers )
		 {
			  if ( HasCrashedGSP( cursor, gspOffset ) )
			  {
					cursor.Offset = gspOffset;
					GenerationSafePointer.Clean( cursor );
					cleanedPointers.incrementAndGet();
			  }
		 }

		 private string UnexpectedGenerations()
		 {
			  return "Unexpected generations, stableGeneration=" + _stableGeneration + ", unstableGeneration=" + _unstableGeneration;
		 }
	}

}
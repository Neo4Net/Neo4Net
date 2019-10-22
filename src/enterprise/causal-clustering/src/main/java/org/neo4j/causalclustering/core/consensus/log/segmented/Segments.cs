using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{

	using Neo4Net.causalclustering.core.consensus.log.segmented.OpenEndRangeMap;
	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using Neo4Net.causalclustering.messaging.marshalling;
	using Neo4Net.Helpers.Collections;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// Keeps track of all the segments that the RAFT log consists of.
	/// </summary>
	internal class Segments : IDisposable
	{
		 private readonly OpenEndRangeMap<long, SegmentFile> _rangeMap = new OpenEndRangeMap<long, SegmentFile>();
		 private readonly IList<SegmentFile> _allSegments;
		 private readonly Log _log;

		 private FileSystemAbstraction _fileSystem;
		 private readonly FileNames _fileNames;
		 private readonly ChannelMarshal<ReplicatedContent> _contentMarshal;
		 private readonly LogProvider _logProvider;
		 private long _currentVersion;
		 private readonly ReaderPool _readerPool;

		 internal Segments( FileSystemAbstraction fileSystem, FileNames fileNames, ReaderPool readerPool, IList<SegmentFile> allSegments, ChannelMarshal<ReplicatedContent> contentMarshal, LogProvider logProvider, long currentVersion )
		 {
			  this._fileSystem = fileSystem;
			  this._fileNames = fileNames;
			  this._allSegments = new List<SegmentFile>( allSegments );
			  this._contentMarshal = contentMarshal;
			  this._logProvider = logProvider;
			  this._log = logProvider.getLog( this.GetType() );
			  this._currentVersion = currentVersion;
			  this._readerPool = readerPool;

			  PopulateRangeMap();
		 }

		 private void PopulateRangeMap()
		 {
			  foreach ( SegmentFile segment in _allSegments )
			  {
					_rangeMap.replaceFrom( segment.Header().prevIndex() + 1, segment );
			  }
		 }

		 /*
		  * Simple chart demonstrating valid and invalid value combinations for the following three calls. All three
		  * result in the same action, but they demand different invariants. Whether we choose to fail hard when they are
		  * invalidated or to simply log a warning, we should still make some sort of check against them.
		  *
		  * Valid truncate: prevFileLast = 100, prevIndex = 80
		  * Invalid truncate: prevFileLast = 100, prevIndex = 101
		  *
		  * Valid rotate: prevFileLast = 100, prevIndex = 100
		  * Invalid rotate: prevFileLast = 100, prevIndex = 80
		  * Invalid rotate: prevFileLast = 100, prevIndex = 101
		  *
		  * Valid skip: prevFileLast = 100, prevIndex = 101
		  * Invalid skip: prevFileLast = 100, prevIndex = 80
		  */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: synchronized SegmentFile truncate(long prevFileLastIndex, long prevIndex, long prevTerm) throws java.io.IOException
		 internal virtual SegmentFile Truncate( long prevFileLastIndex, long prevIndex, long prevTerm )
		 {
			 lock ( this )
			 {
				  if ( prevFileLastIndex < prevIndex )
				  {
						throw new System.ArgumentException( format( "Cannot truncate at index %d which is after current " + "append index %d", prevIndex, prevFileLastIndex ) );
				  }
				  if ( prevFileLastIndex == prevIndex )
				  {
						_log.warn( format( "Truncating at current log append index %d", prevIndex ) );
				  }
				  return CreateNext( prevFileLastIndex, prevIndex, prevTerm );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: synchronized SegmentFile rotate(long prevFileLastIndex, long prevIndex, long prevTerm) throws java.io.IOException
		 internal virtual SegmentFile Rotate( long prevFileLastIndex, long prevIndex, long prevTerm )
		 {
			 lock ( this )
			 {
				  if ( prevFileLastIndex != prevIndex )
				  {
						throw new System.ArgumentException( format( "Cannot rotate file and have append index go from %d " + "to %d. Going backwards is a truncation operation, going " + "forwards is a skip operation.", prevFileLastIndex, prevIndex ) );
				  }
				  return CreateNext( prevFileLastIndex, prevIndex, prevTerm );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: synchronized SegmentFile skip(long prevFileLastIndex, long prevIndex, long prevTerm) throws java.io.IOException
		 internal virtual SegmentFile Skip( long prevFileLastIndex, long prevIndex, long prevTerm )
		 {
			 lock ( this )
			 {
				  if ( prevFileLastIndex > prevIndex )
				  {
						throw new System.ArgumentException( format( "Cannot skip from index %d backwards to index %d", prevFileLastIndex, prevIndex ) );
				  }
				  if ( prevFileLastIndex == prevIndex )
				  {
						_log.warn( format( "Skipping at current log append index %d", prevIndex ) );
				  }
				  return CreateNext( prevFileLastIndex, prevIndex, prevTerm );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private synchronized SegmentFile createNext(long prevFileLastIndex, long prevIndex, long prevTerm) throws java.io.IOException
		 private SegmentFile CreateNext( long prevFileLastIndex, long prevIndex, long prevTerm )
		 {
			 lock ( this )
			 {
				  _currentVersion++;
				  SegmentHeader header = new SegmentHeader( prevFileLastIndex, _currentVersion, prevIndex, prevTerm );
      
				  File file = _fileNames.getForVersion( _currentVersion );
				  SegmentFile segment = SegmentFile.Create( _fileSystem, file, _readerPool, _currentVersion, _contentMarshal, _logProvider, header );
				  // TODO: Force base directory... probably not possible using fsa.
				  segment.Flush();
      
				  _allSegments.Add( segment );
				  _rangeMap.replaceFrom( prevIndex + 1, segment );
      
				  return segment;
			 }
		 }

		 internal virtual ValueRange<long, SegmentFile> GetForIndex( long logIndex )
		 {
			 lock ( this )
			 {
				  return _rangeMap.lookup( logIndex );
			 }
		 }

		 internal virtual SegmentFile Last()
		 {
			 lock ( this )
			 {
				  return _rangeMap.last();
			 }
		 }

		 public virtual SegmentFile Prune( long pruneIndex )
		 {
			 lock ( this )
			 {
				  IEnumerator<SegmentFile> itr = _allSegments.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  SegmentFile notDisposed = itr.next(); // we should always leave at least one segment
				  int firstRemaining = 0;
      
				  while ( itr.MoveNext() )
				  {
						SegmentFile current = itr.Current;
						if ( current.Header().prevFileLastIndex() > pruneIndex )
						{
							 break;
						}
      
						if ( !notDisposed.TryClose() )
						{
							 break;
						}
      
						_log.info( "Pruning %s", notDisposed );
						if ( !notDisposed.Delete() )
						{
							 _log.error( "Failed to delete %s", notDisposed );
							 break;
						}
      
						// TODO: Sync the parent directory. Also consider handling fs operations under its own lock.
      
						firstRemaining++;
						notDisposed = current;
				  }
      
				  _rangeMap.remove( notDisposed.Header().prevIndex() + 1 );
				  _allSegments.RemoveRange( 0, firstRemaining );
      
				  return notDisposed;
			 }
		 }

		 internal virtual void Visit( Visitor<SegmentFile, Exception> visitor )
		 {
			 lock ( this )
			 {
//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
				  IEnumerator<SegmentFile> itr = _allSegments.GetEnumerator();
      
				  bool terminate = false;
				  while ( itr.MoveNext() && !terminate )
				  {
						terminate = visitor.Visit( itr.Current );
				  }
			 }
		 }

		 internal virtual void VisitBackwards( Visitor<SegmentFile, Exception> visitor )
		 {
			 lock ( this )
			 {
				  IEnumerator<SegmentFile> itr = _allSegments.listIterator( _allSegments.Count );
      
				  bool terminate = false;
				  while ( itr.hasPrevious() && !terminate )
				  {
						terminate = visitor.Visit( itr.previous() );
				  }
			 }
		 }

		 public override void Close()
		 {
			 lock ( this )
			 {
				  Exception error = null;
				  foreach ( SegmentFile segment in _allSegments )
				  {
						try
						{
							 segment.Close();
						}
						catch ( Exception ex )
						{
							 if ( error == null )
							 {
								  error = ex;
							 }
							 else
							 {
								  error.addSuppressed( ex );
							 }
						}
				  }
      
				  if ( error != null )
				  {
						throw error;
				  }
			 }
		 }
	}

}
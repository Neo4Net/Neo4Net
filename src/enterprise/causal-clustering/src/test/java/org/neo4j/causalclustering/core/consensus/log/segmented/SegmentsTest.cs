using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using Neo4Net.causalclustering.messaging.marshalling;
	using Neo4Net.Cursors;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.RETURNS_MOCKS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.NullLogProvider.getInstance;

	public class SegmentsTest
	{
		private bool InstanceFieldsInitialized = false;

		public SegmentsTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_fileNames = new FileNames( _baseDirectory );
			_readerPool = new ReaderPool( 0, Instance, _fileNames, _fsa, Clocks.fakeClock() );
			_fileA = spy( new SegmentFile( _fsa, _fileNames.getForVersion( 0 ), _readerPool, 0, _contentMarshal, _logProvider, _header ) );
			_fileB = spy( new SegmentFile( _fsa, _fileNames.getForVersion( 1 ), _readerPool, 1, _contentMarshal, _logProvider, _header ) );
			_segmentFiles = new IList<SegmentFile> { _fileA, _fileB };
		}

		 private readonly FileSystemAbstraction _fsa = mock( typeof( FileSystemAbstraction ), RETURNS_MOCKS );
		 private readonly File _baseDirectory = new File( "." );
		 private FileNames _fileNames;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private final org.neo4j.causalclustering.messaging.marshalling.ChannelMarshal<org.neo4j.causalclustering.core.replication.ReplicatedContent> contentMarshal = mock(org.neo4j.causalclustering.messaging.marshalling.ChannelMarshal.class);
		 private readonly ChannelMarshal<ReplicatedContent> _contentMarshal = mock( typeof( ChannelMarshal ) );
		 private readonly LogProvider _logProvider = NullLogProvider.Instance;
		 private readonly SegmentHeader _header = mock( typeof( SegmentHeader ) );
		 private ReaderPool _readerPool;

		 private SegmentFile _fileA;
		 private SegmentFile _fileB;

		 private IList<SegmentFile> _segmentFiles;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  when( _fsa.deleteFile( any() ) ).thenReturn(true);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateNext() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateNext()
		 {
			  // Given
			  using ( Segments segments = new Segments( _fsa, _fileNames, _readerPool, _segmentFiles, _contentMarshal, _logProvider, -1 ) )
			  {
					// When
					segments.Rotate( 10, 10, 12 );
					segments.Last().closeWriter();
					SegmentFile last = segments.Last();

					// Then
					assertEquals( 10, last.Header().prevFileLastIndex() );
					assertEquals( 10, last.Header().prevIndex() );
					assertEquals( 12, last.Header().prevTerm() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeleteOnPrune() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDeleteOnPrune()
		 {
			  verifyZeroInteractions( _fsa );
			  // Given
			  using ( Segments segments = new Segments( _fsa, _fileNames, _readerPool, _segmentFiles, _contentMarshal, _logProvider, -1 ) )
			  {
					// this is version 0 and will be deleted on prune later
					SegmentFile toPrune = segments.Rotate( -1, -1, -1 );
					segments.Last().closeWriter(); // need to close writer otherwise dispose will not be called
					segments.Rotate( 10, 10, 2 );
					segments.Last().closeWriter(); // ditto
					segments.Rotate( 20, 20, 2 );

					// When
					segments.Prune( 11 );

					verify( _fsa, times( _segmentFiles.Count ) ).deleteFile( _fileNames.getForVersion( toPrune.Header().version() ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNeverDeleteOnTruncate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNeverDeleteOnTruncate()
		 {
			  // Given
			  using ( Segments segments = new Segments( _fsa, _fileNames, _readerPool, _segmentFiles, _contentMarshal, _logProvider, -1 ) )
			  {
					segments.Rotate( -1, -1, -1 );
					segments.Last().closeWriter(); // need to close writer otherwise dispose will not be called
					segments.Rotate( 10, 10, 2 ); // we will truncate this whole file away
					segments.Last().closeWriter();

					// When
					segments.Truncate( 20, 9, 4 );

					// Then
					verify( _fsa, never() ).deleteFile(any());
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeleteTruncatedFilesOnPrune() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDeleteTruncatedFilesOnPrune()
		 {
			  // Given
			  using ( Segments segments = new Segments( _fsa, _fileNames, _readerPool, _segmentFiles, _contentMarshal, _logProvider, -1 ) )
			  {
					SegmentFile toBePruned = segments.Rotate( -1, -1, -1 );
					segments.Last().closeWriter(); // need to close writer otherwise dispose will not be called
					// we will truncate this whole file away
					SegmentFile toBeTruncated = segments.Rotate( 10, 10, 2 );
					segments.Last().closeWriter();

					// When
					// We truncate a whole file
					segments.Truncate( 20, 9, 4 );
					// And we prune all files before that file
					segments.Prune( 10 );

					// Then
					// the truncate file is part of the deletes that happen while pruning
					verify( _fsa, times( _segmentFiles.Count ) ).deleteFile( _fileNames.getForVersion( toBePruned.Header().version() ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseTheSegments()
		 public virtual void ShouldCloseTheSegments()
		 {
			  // Given
			  Segments segments = new Segments( _fsa, _fileNames, _readerPool, _segmentFiles, _contentMarshal, _logProvider, -1 );

			  // When
			  segments.Close();

			  // Then
			  foreach ( SegmentFile file in _segmentFiles )
			  {
					verify( file ).close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSwallowExceptionOnClose()
		 public virtual void ShouldNotSwallowExceptionOnClose()
		 {
			  // Given
			  doThrow( new Exception() ).when(_fileA).close();
			  doThrow( new Exception() ).when(_fileB).close();

			  Segments segments = new Segments( _fsa, _fileNames, _readerPool, _segmentFiles, _contentMarshal, _logProvider, -1 );

			  // When
			  try
			  {
					segments.Close();
					fail( "should have thrown" );
			  }
			  catch ( Exception ex )
			  {
					// Then
					Exception[] suppressed = ex.Suppressed;
					assertEquals( 1, suppressed.Length );
					assertTrue( suppressed[0] is Exception );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowOutOfBoundsPruneIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowOutOfBoundsPruneIndex()
		 {
			  //Given a prune index of n, if the smallest value for a segment file is n+c, the pruning should not remove
			  // any files and not result in a failure.
			  Segments segments = new Segments( _fsa, _fileNames, _readerPool, _segmentFiles, _contentMarshal, _logProvider, -1 );

			  segments.Rotate( -1, -1, -1 );
			  segments.Last().closeWriter(); // need to close writer otherwise dispose will not be called
			  segments.Rotate( 10, 10, 2 ); // we will truncate this whole file away
			  segments.Last().closeWriter();

			  segments.Prune( 11 );

			  segments.Rotate( 20, 20, 3 ); // we will truncate this whole file away
			  segments.Last().closeWriter();

			  //when
			  SegmentFile oldestNotDisposed = segments.Prune( -1 );

			  //then
			  SegmentHeader header = oldestNotDisposed.Header();
			  assertEquals( 10, header.PrevFileLastIndex() );
			  assertEquals( 10, header.PrevIndex() );
			  assertEquals( 2, header.PrevTerm() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void attemptsPruningUntilOpenFileIsFound() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AttemptsPruningUntilOpenFileIsFound()
		 {
			  /// <summary>
			  /// prune stops attempting to prune files after finding one that is open.
			  /// </summary>

			  // Given
			  Segments segments = new Segments( _fsa, _fileNames, _readerPool, Collections.emptyList(), _contentMarshal, _logProvider, -1 );

			  /*
			  create 0
			  create 1
			  create 2
			  create 3
	
			  closeWriter on all
			  create reader on 1
			  prune on 3
	
			  only 0 should be deleted
			   */

			  segments.Rotate( -1, -1, -1 );
			  segments.Last().closeWriter(); // need to close writer otherwise dispose will not be called

			  segments.Rotate( 10, 10, 2 ); // we will truncate this whole file away
			  segments.Last().closeWriter(); // need to close writer otherwise dispose will not be called
			  IOCursor<EntryRecord> reader = segments.Last().getCursor(11);

			  segments.Rotate( 20, 20, 3 ); // we will truncate this whole file away
			  segments.Last().closeWriter();

			  segments.Rotate( 30, 30, 4 ); // we will truncate this whole file away
			  segments.Last().closeWriter();

			  segments.Prune( 31 );

			  //when
			  OpenEndRangeMap.ValueRange<long, SegmentFile> shouldBePruned = segments.GetForIndex( 5 );
			  OpenEndRangeMap.ValueRange<long, SegmentFile> shouldNotBePruned = segments.GetForIndex( 15 );
			  OpenEndRangeMap.ValueRange<long, SegmentFile> shouldAlsoNotBePruned = segments.GetForIndex( 25 );

			  //then
			  assertFalse( shouldBePruned.Value().Present );
			  assertTrue( shouldNotBePruned.Value().Present );
			  assertTrue( shouldAlsoNotBePruned.Value().Present );

			  //when
			  reader.close();
			  segments.Prune( 31 );

			  shouldBePruned = segments.GetForIndex( 5 );
			  shouldNotBePruned = segments.GetForIndex( 15 );
			  shouldAlsoNotBePruned = segments.GetForIndex( 25 );

			  //then
			  assertFalse( shouldBePruned.Value().Present );
			  assertFalse( shouldNotBePruned.Value().Present );
			  assertFalse( shouldAlsoNotBePruned.Value().Present );
		 }
	}

}
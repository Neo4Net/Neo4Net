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
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4Net.Cursors;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;
	using Clocks = Neo4Net.Time.Clocks;

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
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.consensus.ReplicatedString.ValueOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.consensus.log.segmented.SegmentFile.create;

	public class SegmentFileTest
	{
		private bool InstanceFieldsInitialized = false;

		public SegmentFileTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_fileNames = new FileNames( _baseDir );
			_readerPool = spy( new ReaderPool( 0, _logProvider, _fileNames, FsRule.get(), Clocks.fakeClock() ) );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.fs.EphemeralFileSystemRule fsRule = new Neo4Net.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule FsRule = new EphemeralFileSystemRule();
		 private readonly File _baseDir = new File( "raft-log" );
		 private FileNames _fileNames;
		 private readonly DummyRaftableContentSerializer _contentMarshal = new DummyRaftableContentSerializer();
		 private readonly NullLogProvider _logProvider = NullLogProvider.Instance;
		 private readonly SegmentHeader _segmentHeader = new SegmentHeader( -1, 0, -1, -1 );

		 // various constants used throughout tests
		 private readonly RaftLogEntry _entry1 = new RaftLogEntry( 30, ValueOf( "contentA" ) );
		 private readonly RaftLogEntry _entry2 = new RaftLogEntry( 31, ValueOf( "contentB" ) );
		 private readonly RaftLogEntry _entry3 = new RaftLogEntry( 32, ValueOf( "contentC" ) );
		 private readonly RaftLogEntry _entry4 = new RaftLogEntry( 33, ValueOf( "contentD" ) );
		 private readonly int _version = 0;

		 private ReaderPool _readerPool;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  FsRule.get().mkdirs(_baseDir);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportCorrectInitialValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportCorrectInitialValues()
		 {
			  using ( SegmentFile segment = create( FsRule.get(), _fileNames.getForVersion(0), _readerPool, _version, _contentMarshal, _logProvider, _segmentHeader ) )
			  {
					assertEquals( 0, segment.Header().version() );

					IOCursor<EntryRecord> cursor = segment.GetCursor( 0 );
					assertFalse( cursor.next() );

					cursor.close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToWriteAndRead() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToWriteAndRead()
		 {
			  using ( SegmentFile segment = create( FsRule.get(), _fileNames.getForVersion(0), _readerPool, 0, _contentMarshal, _logProvider, _segmentHeader ) )
			  {
					// given
					segment.Write( 0, _entry1 );
					segment.Flush();

					// when
					IOCursor<EntryRecord> cursor = segment.GetCursor( 0 );

					// then
					assertTrue( cursor.next() );
					assertEquals( _entry1, cursor.get().logEntry() );

					cursor.close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToReadFromOffset() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToReadFromOffset()
		 {
			  using ( SegmentFile segment = create( FsRule.get(), _fileNames.getForVersion(0), _readerPool, 0, _contentMarshal, _logProvider, _segmentHeader ) )
			  {
					// given
					segment.Write( 0, _entry1 );
					segment.Write( 1, _entry2 );
					segment.Write( 2, _entry3 );
					segment.Write( 3, _entry4 );
					segment.Flush();

					// when
					IOCursor<EntryRecord> cursor = segment.GetCursor( 2 );

					// then
					assertTrue( cursor.next() );
					assertEquals( _entry3, cursor.get().logEntry() );

					cursor.close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRepeatedlyReadWrittenValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRepeatedlyReadWrittenValues()
		 {
			  using ( SegmentFile segment = create( FsRule.get(), _fileNames.getForVersion(0), _readerPool, 0, _contentMarshal, _logProvider, _segmentHeader ) )
			  {
					// given
					segment.Write( 0, _entry1 );
					segment.Write( 1, _entry2 );
					segment.Write( 2, _entry3 );
					segment.Flush();

					for ( int i = 0; i < 3; i++ )
					{
						 // when
						 IOCursor<EntryRecord> cursor = segment.GetCursor( 0 );

						 // then
						 assertTrue( cursor.next() );
						 assertEquals( _entry1, cursor.get().logEntry() );
						 assertTrue( cursor.next() );
						 assertEquals( _entry2, cursor.get().logEntry() );
						 assertTrue( cursor.next() );
						 assertEquals( _entry3, cursor.get().logEntry() );
						 assertFalse( cursor.next() );

						 cursor.close();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToCloseOnlyAfterWriterIsClosed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToCloseOnlyAfterWriterIsClosed()
		 {
			  using ( SegmentFile segment = create( FsRule.get(), _fileNames.getForVersion(0), _readerPool, 0, _contentMarshal, _logProvider, _segmentHeader ) )
			  {
					// given
					assertFalse( segment.TryClose() );

					// when
					segment.CloseWriter();

					// then
					assertTrue( segment.TryClose() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallDisposeHandlerAfterLastReaderIsClosed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallDisposeHandlerAfterLastReaderIsClosed()
		 {
			  using ( SegmentFile segment = create( FsRule.get(), _fileNames.getForVersion(0), _readerPool, 0, _contentMarshal, _logProvider, _segmentHeader ) )
			  {
					// given
					IOCursor<EntryRecord> cursor0 = segment.GetCursor( 0 );
					IOCursor<EntryRecord> cursor1 = segment.GetCursor( 0 );

					// when
					segment.CloseWriter();
					cursor0.close();

					// then
					assertFalse( segment.TryClose() );

					// when
					cursor1.close();

					// then
					assertTrue( segment.TryClose() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleReaderPastEndCorrectly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleReaderPastEndCorrectly()
		 {
			  using ( SegmentFile segment = create( FsRule.get(), _fileNames.getForVersion(0), _readerPool, 0, _contentMarshal, _logProvider, _segmentHeader ) )
			  {
					// given
					segment.Write( 0, _entry1 );
					segment.Write( 1, _entry2 );
					segment.Flush();
					segment.CloseWriter();

					IOCursor<EntryRecord> cursor = segment.GetCursor( 3 );

					// then
					assertFalse( cursor.next() );

					// when
					cursor.close();

					// then
					assertTrue( segment.TryClose() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveIdempotentCloseMethods() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaveIdempotentCloseMethods()
		 {
			  // given
			  SegmentFile segment = create( FsRule.get(), _fileNames.getForVersion(0), _readerPool, 0, _contentMarshal, _logProvider, _segmentHeader );
			  IOCursor<EntryRecord> cursor = segment.GetCursor( 0 );

			  // when
			  segment.CloseWriter();
			  cursor.close();

			  // then
			  assertTrue( segment.TryClose() );
			  segment.Close();
			  assertTrue( segment.TryClose() );
			  segment.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCatchDoubleCloseReaderErrors() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCatchDoubleCloseReaderErrors()
		 {
			  try
			  {
					  using ( SegmentFile segment = create( FsRule.get(), _fileNames.getForVersion(0), _readerPool, 0, _contentMarshal, _logProvider, _segmentHeader ) )
					  {
						// given
						IOCursor<EntryRecord> cursor = segment.GetCursor( 0 );
      
						cursor.close();
						cursor.close();
						fail( "Should have caught double close error" );
					  }
			  }
			  catch ( System.InvalidOperationException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReturnReaderExperiencingErrorToPool() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotReturnReaderExperiencingErrorToPool()
		 {
			  // given
			  StoreChannel channel = mock( typeof( StoreChannel ) );
			  Reader reader = mock( typeof( Reader ) );
			  ReaderPool readerPool = mock( typeof( ReaderPool ) );

			  when( channel.read( any( typeof( ByteBuffer ) ) ) ).thenThrow( new IOException() );
			  when( reader.Channel() ).thenReturn(channel);
			  when( readerPool.Acquire( anyLong(), anyLong() ) ).thenReturn(reader);

			  using ( SegmentFile segment = create( FsRule.get(), _fileNames.getForVersion(0), readerPool, 0, _contentMarshal, _logProvider, _segmentHeader ) )
			  {
					// given
					IOCursor<EntryRecord> cursor = segment.GetCursor( 0 );

					try
					{
						 cursor.next();
						 fail();
					}
					catch ( IOException )
					{
						 // expected from mocking
					}

					// when
					cursor.close();

					// then
					verify( readerPool, never() ).release(reader);
					verify( reader ).close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPruneReaderPoolOnClose() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPruneReaderPoolOnClose()
		 {
			  using ( SegmentFile segment = create( FsRule.get(), _fileNames.getForVersion(0), _readerPool, 0, _contentMarshal, _logProvider, _segmentHeader ) )
			  {
					segment.Write( 0, _entry1 );
					segment.Flush();
					segment.CloseWriter();

					IOCursor<EntryRecord> cursor = segment.GetCursor( 0 );
					cursor.next();
					cursor.close();
			  }

			  verify( _readerPool ).prune( 0 );
		 }
	}

}
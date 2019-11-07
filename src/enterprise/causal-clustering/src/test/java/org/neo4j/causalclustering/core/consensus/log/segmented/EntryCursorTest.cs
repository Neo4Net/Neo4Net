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
	using After = org.junit.After;
	using Test = org.junit.Test;

	using Neo4Net.causalclustering.messaging.marshalling;
	using EphemeralFileSystemAbstraction = Neo4Net.GraphDb.mockfs.EphemeralFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.logging.NullLogProvider.getInstance;

	public class EntryCursorTest
	{
		private bool InstanceFieldsInitialized = false;

		public EntryCursorTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_fileNames = new FileNames( _bam );
			_readerPool = new ReaderPool( 0, Instance, _fileNames, _fsa, Clocks.fakeClock() );
			_segments = new Segments( _fsa, _fileNames, _readerPool, emptyList(), mock(typeof(ChannelMarshal)), NullLogProvider.Instance, -1 );
			_fsa.mkdir( _bam );
		}

		 private readonly FileSystemAbstraction _fsa = new EphemeralFileSystemAbstraction();
		 private readonly File _bam = new File( "bam" );
		 private FileNames _fileNames;
		 private ReaderPool _readerPool;
		 private Segments _segments;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _fsa.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void ifFileExistsButEntryDoesNotExist() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IfFileExistsButEntryDoesNotExist()
		 {
			  // When
			  _segments.rotate( -1, -1, -1 );
			  _segments.rotate( 10, 10, 10 );
			  _segments.last().closeWriter();

			  EntryCursor entryCursor = new EntryCursor( _segments, 1L );

			  bool next = entryCursor.Next();

			  assertFalse( next );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void requestedSegmentHasBeenPruned() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RequestedSegmentHasBeenPruned()
		 {
			  // When
			  _segments.rotate( -1, -1, -1 );
			  _segments.rotate( 10, 10, 10 );
			  _segments.rotate( 20, 20, 20 );
			  _segments.prune( 12 );
			  _segments.last().closeWriter();

			  EntryCursor entryCursor = new EntryCursor( _segments, 1L );

			  bool next = entryCursor.Next();

			  assertFalse( next );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void requestedSegmentHasNotExistedYet() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RequestedSegmentHasNotExistedYet()
		 {
			  // When
			  _segments.rotate( -1, -1, -1 );
			  _segments.rotate( 10, 10, 10 );
			  _segments.rotate( 20, 20, 20 );
			  _segments.last().closeWriter();

			  EntryCursor entryCursor = new EntryCursor( _segments, 100L );

			  bool next = entryCursor.Next();

			  assertFalse( next );
		 }
	}

}
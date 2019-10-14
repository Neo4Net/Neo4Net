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
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using Neo4Net.causalclustering.messaging.marshalling;
	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using PhysicalFlushableChannel = Neo4Net.Kernel.impl.transaction.log.PhysicalFlushableChannel;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.NullLogProvider.getInstance;

	public class RecoveryProtocolTest
	{
		private bool InstanceFieldsInitialized = false;

		public RecoveryProtocolTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_fsa = FileSystemRule.get();
			_fileNames = new FileNames( _root );
			_readerPool = new ReaderPool( 0, Instance, _fileNames, _fsa, Clocks.fakeClock() );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.EphemeralFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule FileSystemRule = new EphemeralFileSystemRule();

		 private EphemeralFileSystemAbstraction _fsa;
		 private ChannelMarshal<ReplicatedContent> _contentMarshal = new DummyRaftableContentSerializer();
		 private readonly File _root = new File( "root" );
		 private FileNames _fileNames;
		 private SegmentHeader.Marshal _headerMarshal = new SegmentHeader.Marshal();
		 private ReaderPool _readerPool;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _fsa.mkdirs( _root );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnEmptyStateOnEmptyDirectory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnEmptyStateOnEmptyDirectory()
		 {
			  // given
			  RecoveryProtocol protocol = new RecoveryProtocol( _fsa, _fileNames, _readerPool, _contentMarshal, NullLogProvider.Instance );

			  // when
			  State state = protocol.Run();

			  // then
			  assertEquals( -1, state.AppendIndex );
			  assertEquals( -1, state.Terms.latest() );
			  assertEquals( -1, state.PrevIndex );
			  assertEquals( -1, state.PrevTerm );
			  assertEquals( 0, state.Segments.last().header().version() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfThereAreGapsInVersionNumberSequence() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfThereAreGapsInVersionNumberSequence()
		 {
			  // given
			  CreateLogFile( _fsa, -1, 0, 0, -1, -1 );
			  CreateLogFile( _fsa, 5, 2, 2, 5, 0 );

			  RecoveryProtocol protocol = new RecoveryProtocol( _fsa, _fileNames, _readerPool, _contentMarshal, NullLogProvider.Instance );

			  try
			  {
					// when
					protocol.Run();
					fail( "Expected an exception" );
			  }
			  catch ( DamagedLogStorageException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfTheVersionNumberInTheHeaderAndFileNameDiffer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfTheVersionNumberInTheHeaderAndFileNameDiffer()
		 {
			  // given
			  CreateLogFile( _fsa, -1, 0, 1, -1, -1 );

			  RecoveryProtocol protocol = new RecoveryProtocol( _fsa, _fileNames, _readerPool, _contentMarshal, NullLogProvider.Instance );

			  try
			  {
					// when
					protocol.Run();
					fail( "Expected an exception" );
			  }
			  catch ( DamagedLogStorageException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfANonLastFileIsMissingHeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfANonLastFileIsMissingHeader()
		 {
			  // given
			  CreateLogFile( _fsa, -1, 0, 0, -1, -1 );
			  CreateEmptyLogFile( _fsa, 1 );
			  CreateLogFile( _fsa, -1, 2, 2, -1, -1 );

			  RecoveryProtocol protocol = new RecoveryProtocol( _fsa, _fileNames, _readerPool, _contentMarshal, NullLogProvider.Instance );

			  try
			  {
					// when
					protocol.Run();
					fail( "Expected an exception" );
			  }
			  catch ( DamagedLogStorageException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverEvenIfLastHeaderIsMissing() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecoverEvenIfLastHeaderIsMissing()
		 {
			  // given
			  CreateLogFile( _fsa, -1, 0, 0, -1, -1 );
			  CreateEmptyLogFile( _fsa, 1 );

			  RecoveryProtocol protocol = new RecoveryProtocol( _fsa, _fileNames, _readerPool, _contentMarshal, NullLogProvider.Instance );

			  // when
			  protocol.Run();

			  // then
			  assertNotEquals( 0, _fsa.getFileSize( _fileNames.getForVersion( 1 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverAndBeAbleToRotate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecoverAndBeAbleToRotate()
		 {
			  // given
			  CreateLogFile( _fsa, -1, 0, 0, -1, -1 );
			  CreateLogFile( _fsa, 10, 1, 1, 10, 0 );
			  CreateLogFile( _fsa, 20, 2, 2, 20, 1 );

			  RecoveryProtocol protocol = new RecoveryProtocol( _fsa, _fileNames, _readerPool, _contentMarshal, NullLogProvider.Instance );

			  // when
			  State state = protocol.Run();
			  SegmentFile newFile = state.Segments.rotate( 20, 20, 1 );

			  // then
			  assertEquals( 20, newFile.Header().prevFileLastIndex() );
			  assertEquals( 3, newFile.Header().version() );
			  assertEquals( 20, newFile.Header().prevIndex() );
			  assertEquals( 1, newFile.Header().prevTerm() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverAndBeAbleToTruncate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecoverAndBeAbleToTruncate()
		 {
			  // given
			  CreateLogFile( _fsa, -1, 0, 0, -1, -1 );
			  CreateLogFile( _fsa, 10, 1, 1, 10, 0 );
			  CreateLogFile( _fsa, 20, 2, 2, 20, 1 );

			  RecoveryProtocol protocol = new RecoveryProtocol( _fsa, _fileNames, _readerPool, _contentMarshal, NullLogProvider.Instance );

			  // when
			  State state = protocol.Run();
			  SegmentFile newFile = state.Segments.truncate( 20, 15, 0 );

			  // then
			  assertEquals( 20, newFile.Header().prevFileLastIndex() );
			  assertEquals( 3, newFile.Header().version() );
			  assertEquals( 15, newFile.Header().prevIndex() );
			  assertEquals( 0, newFile.Header().prevTerm() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverAndBeAbleToSkip() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecoverAndBeAbleToSkip()
		 {
			  // given
			  CreateLogFile( _fsa, -1, 0, 0, -1, -1 );
			  CreateLogFile( _fsa, 10, 1, 1, 10, 0 );
			  CreateLogFile( _fsa, 20, 2, 2, 20, 1 );

			  RecoveryProtocol protocol = new RecoveryProtocol( _fsa, _fileNames, _readerPool, _contentMarshal, NullLogProvider.Instance );

			  // when
			  State state = protocol.Run();
			  SegmentFile newFile = state.Segments.skip( 20, 40, 2 );

			  // then
			  assertEquals( 20, newFile.Header().prevFileLastIndex() );
			  assertEquals( 3, newFile.Header().version() );
			  assertEquals( 40, newFile.Header().prevIndex() );
			  assertEquals( 2, newFile.Header().prevTerm() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverBootstrappedEntry() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecoverBootstrappedEntry()
		 {
			  for ( int bootstrapIndex = 0; bootstrapIndex < 5; bootstrapIndex++ )
			  {
					for ( long bootstrapTerm = 0; bootstrapTerm < 5; bootstrapTerm++ )
					{
						 TestRecoveryOfBootstrappedEntry( bootstrapIndex, bootstrapTerm );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testRecoveryOfBootstrappedEntry(long bootstrapIndex, long bootstrapTerm) throws java.io.IOException, DamagedLogStorageException, DisposedException
		 private void TestRecoveryOfBootstrappedEntry( long bootstrapIndex, long bootstrapTerm )
		 {
			  // given
			  CreateLogFile( _fsa, -1, 0, 0, -1, -1 );
			  CreateLogFile( _fsa, -1, 1, 1, bootstrapIndex, bootstrapTerm );

			  RecoveryProtocol protocol = new RecoveryProtocol( _fsa, _fileNames, _readerPool, _contentMarshal, NullLogProvider.Instance );

			  // when
			  State state = protocol.Run();

			  // then
			  assertEquals( bootstrapIndex, state.PrevIndex );
			  assertEquals( bootstrapTerm, state.PrevTerm );

			  assertEquals( -1, state.Terms.get( -1 ) );
			  assertEquals( -1, state.Terms.get( bootstrapIndex - 1 ) );
			  assertEquals( bootstrapTerm, state.Terms.get( bootstrapIndex ) );
			  assertEquals( -1, state.Terms.get( bootstrapIndex + 1 ) );

			  assertEquals( bootstrapTerm, state.Terms.latest() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverSeveralSkips() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecoverSeveralSkips()
		 {
			  // given
			  CreateLogFile( _fsa, 10, 1, 1, 20, 9 );
			  CreateLogFile( _fsa, 100, 2, 2, 200, 99 );
			  CreateLogFile( _fsa, 1000, 3, 3, 2000, 999 );

			  RecoveryProtocol protocol = new RecoveryProtocol( _fsa, _fileNames, _readerPool, _contentMarshal, NullLogProvider.Instance );

			  // when
			  State state = protocol.Run();

			  // then
			  assertEquals( 2000, state.PrevIndex );
			  assertEquals( 999, state.PrevTerm );

			  assertEquals( -1, state.Terms.get( 20 ) );
			  assertEquals( -1, state.Terms.get( 200 ) );
			  assertEquals( -1, state.Terms.get( 1999 ) );

			  assertEquals( 999, state.Terms.get( 2000 ) );
			  assertEquals( -1, state.Terms.get( 2001 ) );

			  assertEquals( 999, state.Terms.latest() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createLogFile(org.neo4j.graphdb.mockfs.EphemeralFileSystemAbstraction fsa, long prevFileLastIndex, long fileNameVersion, long headerVersion, long prevIndex, long prevTerm) throws java.io.IOException
		 private void CreateLogFile( EphemeralFileSystemAbstraction fsa, long prevFileLastIndex, long fileNameVersion, long headerVersion, long prevIndex, long prevTerm )
		 {
			  StoreChannel channel = fsa.Open( _fileNames.getForVersion( fileNameVersion ), OpenMode.READ_WRITE );
			  PhysicalFlushableChannel writer = new PhysicalFlushableChannel( channel );
			  _headerMarshal.marshal( new SegmentHeader( prevFileLastIndex, headerVersion, prevIndex, prevTerm ), writer );
			  writer.PrepareForFlush().flush();
			  channel.close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createEmptyLogFile(org.neo4j.graphdb.mockfs.EphemeralFileSystemAbstraction fsa, long fileNameVersion) throws java.io.IOException
		 private void CreateEmptyLogFile( EphemeralFileSystemAbstraction fsa, long fileNameVersion )
		 {
			  StoreChannel channel = fsa.Open( _fileNames.getForVersion( fileNameVersion ), OpenMode.READ_WRITE );
			  channel.close();
		 }
	}

}
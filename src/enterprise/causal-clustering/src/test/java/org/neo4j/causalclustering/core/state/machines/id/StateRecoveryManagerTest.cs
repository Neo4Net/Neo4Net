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
namespace Neo4Net.causalclustering.core.state.machines.id
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using Neo4Net.causalclustering.core.state;
	using Neo4Net.causalclustering.core.state.storage;
	using EphemeralFileSystemAbstraction = Neo4Net.GraphDb.mockfs.EphemeralFileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using ReadableChannel = Neo4Net.Kernel.Api.StorageEngine.ReadableChannel;
	using WritableChannel = Neo4Net.Kernel.Api.StorageEngine.WritableChannel;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class StateRecoveryManagerTest
	{
		private bool InstanceFieldsInitialized = false;

		public StateRecoveryManagerTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _fileSystemRule ).around( _testDir );
		}


		 private readonly TestDirectory _testDir = TestDirectory.testDirectory();
		 private readonly EphemeralFileSystemRule _fileSystemRule = new EphemeralFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fileSystemRule).around(testDir);
		 public RuleChain RuleChain;

		 private readonly int _numberOfRecordsPerFile = 100;
		 private readonly int _numberOfBytesPerRecord = 10;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void checkArgs()
		 public virtual void CheckArgs()
		 {
			  assertEquals( 0, _numberOfRecordsPerFile % _numberOfBytesPerRecord );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfBothFilesAreEmpty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfBothFilesAreEmpty()
		 {
			  // given
			  EphemeralFileSystemAbstraction fsa = _fileSystemRule.get();
			  fsa.Mkdir( _testDir.directory() );

			  File fileA = fileA();
			  fsa.Create( fileA );

			  File fileB = fileB();
			  fsa.Create( fileB );

			  StateRecoveryManager<long> manager = new StateRecoveryManager<long>( fsa, new LongMarshal() );

			  try
			  {
					// when
					StateRecoveryManager.RecoveryStatus recoveryStatus = manager.Recover( fileA, fileB );
					fail();
			  }
			  catch ( System.InvalidOperationException )
			  {
					// then
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnPreviouslyInactiveWhenOneFileFullAndOneEmpty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnPreviouslyInactiveWhenOneFileFullAndOneEmpty()
		 {
			  // given
			  EphemeralFileSystemAbstraction fsa = _fileSystemRule.get();
			  fsa.Mkdir( _testDir.directory() );

			  File fileA = fileA();
			  StoreChannel channel = fsa.Create( fileA );

			  FillUpAndForce( channel );

			  File fileB = fileB();
			  fsa.Create( fileB );

			  StateRecoveryManager<long> manager = new StateRecoveryManager<long>( fsa, new LongMarshal() );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.core.state.StateRecoveryManager.RecoveryStatus recoveryStatus = manager.recover(fileA, fileB);
			  StateRecoveryManager.RecoveryStatus recoveryStatus = manager.Recover( fileA, fileB );

			  // then
			  assertEquals( fileB, recoveryStatus.activeFile() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnTheEmptyFileAsPreviouslyInactiveWhenActiveContainsCorruptEntry() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnTheEmptyFileAsPreviouslyInactiveWhenActiveContainsCorruptEntry()
		 {
			  // given
			  EphemeralFileSystemAbstraction fsa = _fileSystemRule.get();
			  fsa.Mkdir( _testDir.directory() );

			  File fileA = fileA();
			  StoreChannel channel = fsa.Create( fileA );

			  ByteBuffer buffer = WriteLong( 999 );
			  channel.WriteAll( buffer );
			  channel.Force( false );

			  File fileB = fileB();
			  channel = fsa.Create( fileB );
			  channel.close();

			  StateRecoveryManager<long> manager = new StateRecoveryManager<long>( fsa, new LongMarshal() );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.core.state.StateRecoveryManager.RecoveryStatus recoveryStatus = manager.recover(fileA, fileB);
			  StateRecoveryManager.RecoveryStatus recoveryStatus = manager.Recover( fileA, fileB );

			  // then
			  assertEquals( 999L, recoveryStatus.recoveredState() );
			  assertEquals( fileB, recoveryStatus.activeFile() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnTheFullFileAsPreviouslyInactiveWhenActiveContainsCorruptEntry() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnTheFullFileAsPreviouslyInactiveWhenActiveContainsCorruptEntry()
		 {
			  // given
			  EphemeralFileSystemAbstraction fsa = _fileSystemRule.get();
			  fsa.Mkdir( _testDir.directory() );

			  File fileA = fileA();
			  StoreChannel channel = fsa.Create( fileA );

			  ByteBuffer buffer = WriteLong( 42 );
			  channel.WriteAll( buffer );
			  channel.Force( false );

			  buffer.clear();
			  buffer.putLong( 101 ); // extraneous bytes
			  buffer.flip();
			  channel.WriteAll( buffer );
			  channel.Force( false );

			  File fileB = fileB();
			  fsa.Create( fileB );

			  StateRecoveryManager<long> manager = new StateRecoveryManager<long>( fsa, new LongMarshal() );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.core.state.StateRecoveryManager.RecoveryStatus recoveryStatus = manager.recover(fileA, fileB);
			  StateRecoveryManager.RecoveryStatus recoveryStatus = manager.Recover( fileA, fileB );

			  // then
			  assertEquals( fileB, recoveryStatus.activeFile() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverFromPartiallyWrittenEntriesInBothFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecoverFromPartiallyWrittenEntriesInBothFiles()
		 {
			  // given
			  EphemeralFileSystemAbstraction fsa = _fileSystemRule.get();
			  fsa.Mkdir( _testDir.directory() );

			  StateRecoveryManager<long> manager = new StateRecoveryManager<long>( fsa, new LongMarshal() );

			  WriteSomeLongsIn( fsa, FileA(), 3, 4 );
			  WriteSomeLongsIn( fsa, FileB(), 5, 6 );
			  WriteSomeGarbage( fsa, FileA() );
			  WriteSomeGarbage( fsa, FileB() );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.core.state.StateRecoveryManager.RecoveryStatus recovered = manager.recover(fileA(), fileB());
			  StateRecoveryManager.RecoveryStatus recovered = manager.Recover( FileA(), FileB() );

			  // then
			  assertEquals( FileA(), recovered.activeFile() );
			  assertEquals( 6L, recovered.recoveredState() );
		 }

		 private File FileA()
		 {
			  return new File( _testDir.directory(), "file.A" );
		 }

		 private File FileB()
		 {
			  return new File( _testDir.directory(), "file.B" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeSomeGarbage(org.Neo4Net.graphdb.mockfs.EphemeralFileSystemAbstraction fsa, java.io.File file) throws java.io.IOException
		 private void WriteSomeGarbage( EphemeralFileSystemAbstraction fsa, File file )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.io.fs.StoreChannel channel = fsa.open(file, org.Neo4Net.io.fs.OpenMode.READ_WRITE);
			  StoreChannel channel = fsa.Open( file, OpenMode.READ_WRITE );
			  ByteBuffer buffer = ByteBuffer.allocate( 4 );
			  buffer.putInt( 9876 );
			  buffer.flip();
			  channel.WriteAll( buffer );
			  channel.Force( false );
			  channel.close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeSomeLongsIn(org.Neo4Net.graphdb.mockfs.EphemeralFileSystemAbstraction fsa, java.io.File file, long... longs) throws java.io.IOException
		 private void WriteSomeLongsIn( EphemeralFileSystemAbstraction fsa, File file, params long[] longs )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.io.fs.StoreChannel channel = fsa.open(file, org.Neo4Net.io.fs.OpenMode.READ_WRITE);
			  StoreChannel channel = fsa.Open( file, OpenMode.READ_WRITE );
			  ByteBuffer buffer = ByteBuffer.allocate( longs.Length * 8 );

			  foreach ( long aLong in longs )
			  {
					buffer.putLong( aLong );
			  }

			  buffer.flip();
			  channel.WriteAll( buffer );
			  channel.Force( false );
			  channel.close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void fillUpAndForce(org.Neo4Net.io.fs.StoreChannel channel) throws java.io.IOException
		 private void FillUpAndForce( StoreChannel channel )
		 {
			  for ( int i = 0; i < _numberOfRecordsPerFile; i++ )
			  {
					ByteBuffer buffer = WriteLong( i );
					channel.WriteAll( buffer );
					channel.Force( false );
			  }
		 }

		 private ByteBuffer WriteLong( long logIndex )
		 {
			  ByteBuffer buffer = ByteBuffer.allocate( 8 );
			  buffer.putLong( logIndex );
			  buffer.flip();
			  return buffer;
		 }

		 private class LongMarshal : SafeStateMarshal<long>
		 {
			  public override long? StartState()
			  {
					return 0L;
			  }

			  public override long Ordinal( long? aLong )
			  {
					return aLong.Value;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(System.Nullable<long> aLong, org.Neo4Net.Kernel.Api.StorageEngine.WritableChannel channel) throws java.io.IOException
			  public override void Marshal( long? aLong, WritableChannel channel )
			  {
					channel.PutLong( aLong.Value );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected System.Nullable<long> unmarshal0(org.Neo4Net.Kernel.Api.StorageEngine.ReadableChannel channel) throws java.io.IOException
			  protected internal override long? Unmarshal0( ReadableChannel channel )
			  {
					return channel.Long;
			  }
		 }
	}

}
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
namespace Neo4Net.causalclustering.core.state.storage
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using LifeRule = Neo4Net.Kernel.Lifecycle.LifeRule;
	using Lifespan = Neo4Net.Kernel.Lifecycle.Lifespan;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using ReadableChannel = Neo4Net.Storageengine.Api.ReadableChannel;
	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;

	public class DurableStateStorageTest
	{
		private bool InstanceFieldsInitialized = false;

		public DurableStateStorageTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _fileSystemRule ).around( _lifeRule ).around( _testDir );
		}

		 private readonly TestDirectory _testDir = TestDirectory.testDirectory();
		 private readonly EphemeralFileSystemRule _fileSystemRule = new EphemeralFileSystemRule();
		 private readonly LifeRule _lifeRule = new LifeRule( true );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fileSystemRule).around(lifeRule).around(testDir);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMaintainStateGivenAnEmptyInitialStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMaintainStateGivenAnEmptyInitialStore()
		 {
			  // given
			  EphemeralFileSystemAbstraction fsa = _fileSystemRule.get();
			  fsa.Mkdir( _testDir.directory() );

			  DurableStateStorage<AtomicInteger> storage = _lifeRule.add( new DurableStateStorage<AtomicInteger>( fsa, _testDir.directory(), "state", new AtomicIntegerMarshal(), 100, NullLogProvider.Instance ) );

			  // when
			  storage.PersistStoreData( new AtomicInteger( 99 ) );

			  // then
			  assertEquals( 4, fsa.GetFileSize( StateFileA() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRotateToOtherStoreFileAfterSufficientEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRotateToOtherStoreFileAfterSufficientEntries()
		 {
			  // given
			  EphemeralFileSystemAbstraction fsa = _fileSystemRule.get();
			  fsa.Mkdir( _testDir.directory() );

			  const int numberOfEntriesBeforeRotation = 100;
			  DurableStateStorage<AtomicInteger> storage = _lifeRule.add( new DurableStateStorage<AtomicInteger>( fsa, _testDir.directory(), "state", new AtomicIntegerMarshal(), numberOfEntriesBeforeRotation, NullLogProvider.Instance ) );

			  // when
			  for ( int i = 0; i < numberOfEntriesBeforeRotation; i++ )
			  {
					storage.PersistStoreData( new AtomicInteger( i ) );
			  }

			  // Force the rotation
			  storage.PersistStoreData( new AtomicInteger( 9999 ) );

			  // then
			  assertEquals( 4, fsa.GetFileSize( StateFileB() ) );
			  assertEquals( numberOfEntriesBeforeRotation * 4, fsa.GetFileSize( StateFileA() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRotateBackToFirstStoreFileAfterSufficientEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRotateBackToFirstStoreFileAfterSufficientEntries()
		 {
			  // given
			  EphemeralFileSystemAbstraction fsa = _fileSystemRule.get();
			  fsa.Mkdir( _testDir.directory() );

			  const int numberOfEntriesBeforeRotation = 100;
			  DurableStateStorage<AtomicInteger> storage = _lifeRule.add( new DurableStateStorage<AtomicInteger>( fsa, _testDir.directory(), "state", new AtomicIntegerMarshal(), numberOfEntriesBeforeRotation, NullLogProvider.Instance ) );

			  // when
			  for ( int i = 0; i < numberOfEntriesBeforeRotation * 2; i++ )
			  {
					storage.PersistStoreData( new AtomicInteger( i ) );
			  }

			  // Force the rotation back to the first store
			  storage.PersistStoreData( new AtomicInteger( 9999 ) );

			  // then
			  assertEquals( 4, fsa.GetFileSize( StateFileA() ) );
			  assertEquals( numberOfEntriesBeforeRotation * 4, fsa.GetFileSize( StateFileB() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearFileOnFirstUse() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldClearFileOnFirstUse()
		 {
			  // given
			  EphemeralFileSystemAbstraction fsa = _fileSystemRule.get();
			  fsa.Mkdir( _testDir.directory() );

			  int rotationCount = 10;

			  DurableStateStorage<AtomicInteger> storage = new DurableStateStorage<AtomicInteger>( fsa, _testDir.directory(), "state", new AtomicIntegerMarshal(), rotationCount, NullLogProvider.Instance );
			  int largestValueWritten = 0;
			  using ( Lifespan lifespan = new Lifespan( storage ) )
			  {
					for ( ; largestValueWritten < rotationCount * 2; largestValueWritten++ )
					{
						 storage.PersistStoreData( new AtomicInteger( largestValueWritten ) );
					}
			  }

			  // now both files are full. We reopen, then write some more.
			  storage = _lifeRule.add( new DurableStateStorage<>( fsa, _testDir.directory(), "state", new AtomicIntegerMarshal(), rotationCount, NullLogProvider.Instance ) );

			  storage.PersistStoreData( new AtomicInteger( largestValueWritten++ ) );
			  storage.PersistStoreData( new AtomicInteger( largestValueWritten++ ) );
			  storage.PersistStoreData( new AtomicInteger( largestValueWritten ) );

			  /*
			   * We have written stuff in fileA but not gotten to the end (resulting in rotation). The largestValueWritten
			   * should nevertheless be correct
			   */
			  ByteBuffer forReadingBackIn = ByteBuffer.allocate( 10_000 );
			  StoreChannel lastWrittenTo = fsa.Open( StateFileA(), OpenMode.READ );
			  lastWrittenTo.read( forReadingBackIn );
			  forReadingBackIn.flip();

			  AtomicInteger lastRead = null;
			  while ( true )
			  {
					try
					{
						 lastRead = new AtomicInteger( forReadingBackIn.Int );
					}
					catch ( BufferUnderflowException )
					{
						 break;
					}
			  }

			  // then
			  assertNotNull( lastRead );
			  assertEquals( largestValueWritten, lastRead.get() );
		 }

		 private class AtomicIntegerMarshal : SafeStateMarshal<AtomicInteger>
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(java.util.concurrent.atomic.AtomicInteger state, org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
			  public override void Marshal( AtomicInteger state, WritableChannel channel )
			  {
					channel.PutInt( state.intValue() );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.concurrent.atomic.AtomicInteger unmarshal0(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
			  public override AtomicInteger Unmarshal0( ReadableChannel channel )
			  {
					return new AtomicInteger( channel.Int );
			  }

			  public override AtomicInteger StartState()
			  {
					return new AtomicInteger( 0 );
			  }

			  public override long Ordinal( AtomicInteger atomicInteger )
			  {
					return atomicInteger.get();
			  }
		 }

		 private File StateFileA()
		 {
			  return new File( new File( _testDir.directory(), "state-state" ), "state.a" );
		 }

		 private File StateFileB()
		 {
			  return new File( new File( _testDir.directory(), "state-state" ), "state.b" );
		 }
	}

}
using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.causalclustering.core.state.machines.id
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;


	using LeaderInfo = Org.Neo4j.causalclustering.core.consensus.LeaderInfo;
	using RaftMachine = Org.Neo4j.causalclustering.core.consensus.RaftMachine;
	using ExposedRaftState = Org.Neo4j.causalclustering.core.consensus.state.ExposedRaftState;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using IdGeneratorContractTest = Org.Neo4j.Kernel.impl.store.IdGeneratorContractTest;
	using IdGenerator = Org.Neo4j.Kernel.impl.store.id.IdGenerator;
	using IdRange = Org.Neo4j.Kernel.impl.store.id.IdRange;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;
	using Org.Neo4j.Test.rule.fs;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ReplicatedIdGeneratorTest : IdGeneratorContractTest
	{
		 private NullLogProvider _logProvider = NullLogProvider.Instance;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.fs.FileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public FileSystemRule FileSystemRule = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();
		 private File _file;
		 private FileSystemAbstraction _fs;
		 private MemberId _myself = new MemberId( System.Guid.randomUUID() );
		 private RaftMachine _raftMachine = Mockito.mock( typeof( RaftMachine ) );
		 private ExposedRaftState _state = mock( typeof( ExposedRaftState ) );
		 private readonly CommandIndexTracker _commandIndexTracker = mock( typeof( CommandIndexTracker ) );
		 private IdReusabilityCondition _idReusabilityCondition;
		 private ReplicatedIdGenerator _idGenerator;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _file = TestDirectory.file( "idgen" );
			  _fs = FileSystemRule.get();
			  when( _raftMachine.state() ).thenReturn(_state);
			  _idReusabilityCondition = IdReusabilityCondition;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  if ( _idGenerator != null )
			  {
					_idGenerator.Dispose();
			  }
		 }

		 protected internal override IdGenerator CreateIdGenerator( int grabSize )
		 {
			  return OpenIdGenerator( grabSize );
		 }

		 protected internal override IdGenerator OpenIdGenerator( int grabSize )
		 {
			  ReplicatedIdGenerator replicatedIdGenerator = GetReplicatedIdGenerator( grabSize, 0L, StubAcquirer() );
			  return new FreeIdFilteredIdGenerator( replicatedIdGenerator, _idReusabilityCondition );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateIdFileForPersistence()
		 public virtual void ShouldCreateIdFileForPersistence()
		 {
			  ReplicatedIdRangeAcquirer rangeAcquirer = SimpleRangeAcquirer( IdType.NODE, 0, 1024 );

			  _idGenerator = GetReplicatedIdGenerator( 10, 0L, rangeAcquirer );

			  assertTrue( _fs.fileExists( _file ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotStepBeyondAllocationBoundaryWithoutBurnedId()
		 public virtual void ShouldNotStepBeyondAllocationBoundaryWithoutBurnedId()
		 {
			  ReplicatedIdRangeAcquirer rangeAcquirer = SimpleRangeAcquirer( IdType.NODE, 0, 1024 );

			  _idGenerator = GetReplicatedIdGenerator( 10, 0L, rangeAcquirer );

			  ISet<long> idsGenerated = CollectGeneratedIds( _idGenerator, 1024 );

			  long minId = min( idsGenerated );
			  long maxId = max( idsGenerated );

			  assertEquals( 0L, minId );
			  assertEquals( 1023L, maxId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotStepBeyondAllocationBoundaryWithBurnedId()
		 public virtual void ShouldNotStepBeyondAllocationBoundaryWithBurnedId()
		 {
			  ReplicatedIdRangeAcquirer rangeAcquirer = SimpleRangeAcquirer( IdType.NODE, 0, 1024 );

			  long burnedIds = 23L;
			  _idGenerator = GetReplicatedIdGenerator( 10, burnedIds, rangeAcquirer );

			  ISet<long> idsGenerated = CollectGeneratedIds( _idGenerator, 1024 - burnedIds );

			  long minId = min( idsGenerated );
			  long maxId = max( idsGenerated );

			  assertEquals( burnedIds, minId );
			  assertEquals( 1023, maxId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalStateException.class) public void shouldThrowIfAdjustmentFailsDueToInconsistentValues()
		 public virtual void ShouldThrowIfAdjustmentFailsDueToInconsistentValues()
		 {
			  ReplicatedIdRangeAcquirer rangeAcquirer = mock( typeof( ReplicatedIdRangeAcquirer ) );
			  when( rangeAcquirer.AcquireIds( IdType.NODE ) ).thenReturn( Allocation( 3, 21, 21 ) );
			  _idGenerator = GetReplicatedIdGenerator( 10, 42L, rangeAcquirer );

			  _idGenerator.nextId();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReuseIdOnlyWhenLeader()
		 public virtual void ShouldReuseIdOnlyWhenLeader()
		 {
			  ReplicatedIdRangeAcquirer rangeAcquirer = SimpleRangeAcquirer( IdType.NODE, 0, 1024 );

			  long burnedIds = 23L;
			  using ( FreeIdFilteredIdGenerator idGenerator = new FreeIdFilteredIdGenerator( GetReplicatedIdGenerator( 10, burnedIds, rangeAcquirer ), _idReusabilityCondition ) )
			  {

					idGenerator.FreeId( 10 );
					assertEquals( 0, idGenerator.DefragCount );
					assertEquals( 23, idGenerator.NextId() );

					when( _commandIndexTracker.AppliedCommandIndex ).thenReturn( 6L ); // gap-free
					when( _state.lastLogIndexBeforeWeBecameLeader() ).thenReturn(5L);
					_idReusabilityCondition.onLeaderSwitch( new LeaderInfo( _myself, 1 ) );

					idGenerator.FreeId( 10 );
					assertEquals( 1, idGenerator.DefragCount );
					assertEquals( 10, idGenerator.NextId() );
					assertEquals( 0, idGenerator.DefragCount );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReuseIdBeforeHighId()
		 public virtual void ShouldReuseIdBeforeHighId()
		 {
			  ReplicatedIdRangeAcquirer rangeAcquirer = SimpleRangeAcquirer( IdType.NODE, 0, 1024 );

			  long burnedIds = 23L;
			  _idGenerator = GetReplicatedIdGenerator( 10, burnedIds, rangeAcquirer );

			  assertEquals( 23, _idGenerator.nextId() );

			  _idGenerator.freeId( 10 );
			  _idGenerator.freeId( 5 );

			  assertEquals( 10, _idGenerator.nextId() );
			  assertEquals( 5, _idGenerator.nextId() );
			  assertEquals( 24, _idGenerator.nextId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void freeIdOnlyWhenReusabilityConditionAllows()
		 public virtual void FreeIdOnlyWhenReusabilityConditionAllows()
		 {
			  ReplicatedIdRangeAcquirer rangeAcquirer = SimpleRangeAcquirer( IdType.NODE, 0, 1024 );

			  IdReusabilityCondition idReusabilityCondition = IdReusabilityCondition;

			  long burnedIds = 23L;
			  using ( FreeIdFilteredIdGenerator idGenerator = new FreeIdFilteredIdGenerator( GetReplicatedIdGenerator( 10, burnedIds, rangeAcquirer ), idReusabilityCondition ) )
			  {

					idGenerator.FreeId( 10 );
					assertEquals( 0, idGenerator.DefragCount );
					assertEquals( 23, idGenerator.NextId() );

					when( _commandIndexTracker.AppliedCommandIndex ).thenReturn( 4L, 6L ); // gap-free
					when( _state.lastLogIndexBeforeWeBecameLeader() ).thenReturn(5L);
					idReusabilityCondition.OnLeaderSwitch( new LeaderInfo( _myself, 1 ) );

					assertEquals( 24, idGenerator.NextId() );
					idGenerator.FreeId( 11 );
					assertEquals( 25, idGenerator.NextId() );
					idGenerator.FreeId( 6 );
					assertEquals( 6, idGenerator.NextId() );
			  }
		 }

		 private IdReusabilityCondition IdReusabilityCondition
		 {
			 get
			 {
				  return new IdReusabilityCondition( _commandIndexTracker, _raftMachine, _myself );
			 }
		 }

		 private ISet<long> CollectGeneratedIds( ReplicatedIdGenerator idGenerator, long expectedIds )
		 {
			  ISet<long> idsGenerated = new HashSet<long>();

			  long nextId;
			  for ( int i = 0; i < expectedIds; i++ )
			  {
					nextId = idGenerator.NextId();
					assertThat( nextId, greaterThanOrEqualTo( 0L ) );
					idsGenerated.Add( nextId );
			  }

			  try
			  {
					idGenerator.NextId();
					fail( "Too many ids produced, expected " + expectedIds );
			  }
			  catch ( NoMoreIds )
			  {
					// rock and roll!
			  }

			  return idsGenerated;
		 }

		 private ReplicatedIdRangeAcquirer SimpleRangeAcquirer( IdType idType, long start, int length )
		 {
			  ReplicatedIdRangeAcquirer rangeAcquirer = mock( typeof( ReplicatedIdRangeAcquirer ) );
			  //noinspection unchecked
			  when( rangeAcquirer.AcquireIds( idType ) ).thenReturn( Allocation( start, length, -1 ) ).thenThrow( typeof( NoMoreIds ) );
			  return rangeAcquirer;
		 }

		 private class NoMoreIds : Exception
		 {
		 }

		 private IdAllocation Allocation( long start, int length, int highestIdInUse )
		 {
			  return new IdAllocation( new IdRange( new long[0], start, length ), highestIdInUse, 0 );
		 }

		 private ReplicatedIdRangeAcquirer StubAcquirer()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ReplicatedIdRangeAcquirer rangeAcquirer = mock(ReplicatedIdRangeAcquirer.class);
			  ReplicatedIdRangeAcquirer rangeAcquirer = mock( typeof( ReplicatedIdRangeAcquirer ) );
			  when( rangeAcquirer.AcquireIds( IdType.NODE ) ).thenReturn( Allocation( 0, 1024, -1 ) ).thenReturn( Allocation( 1024, 1024, 1023 ) ).thenReturn( Allocation( 2048, 1024, 2047 ) ).thenReturn( Allocation( 3072, 1024, 3071 ) ).thenReturn( Allocation( 4096, 1024, 4095 ) ).thenReturn( Allocation( 5120, 1024, 5119 ) ).thenReturn( Allocation( 6144, 1024, 6143 ) ).thenReturn( Allocation( 7168, 1024, 7167 ) ).thenReturn( Allocation( 8192, 1024, 8191 ) ).thenReturn( Allocation( 9216, 1024, 9215 ) ).thenReturn( Allocation( -1, 0, 9216 + 1024 ) );
			  return rangeAcquirer;
		 }

		 private ReplicatedIdGenerator GetReplicatedIdGenerator( int grabSize, long l, ReplicatedIdRangeAcquirer replicatedIdRangeAcquirer )
		 {
			  return new ReplicatedIdGenerator( _fs, _file, IdType.NODE, () => l, replicatedIdRangeAcquirer, _logProvider, grabSize, true );
		 }

	}

}
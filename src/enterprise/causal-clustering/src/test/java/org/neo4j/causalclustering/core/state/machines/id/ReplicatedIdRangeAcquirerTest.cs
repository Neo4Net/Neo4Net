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
namespace Neo4Net.causalclustering.core.state.machines.id
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4Net.causalclustering.core.replication;
	using Neo4Net.causalclustering.core.state.storage;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using Neo4Net.Test.rule.fs;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class ReplicatedIdRangeAcquirerTest
	{
		private bool InstanceFieldsInitialized = false;

		public ReplicatedIdRangeAcquirerTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_replicator = new DirectReplicator<ReplicatedIdAllocationRequest>( _idAllocationStateMachine );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.TestDirectory testDirectory = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.fs.FileSystemRule defaultFileSystemRule = new Neo4Net.test.rule.fs.DefaultFileSystemRule();
		 public FileSystemRule DefaultFileSystemRule = new DefaultFileSystemRule();

		 private readonly MemberId _memberA = new MemberId( System.Guid.randomUUID() );
		 private readonly MemberId _memberB = new MemberId( System.Guid.randomUUID() );

		 private readonly ReplicatedIdAllocationStateMachine _idAllocationStateMachine = new ReplicatedIdAllocationStateMachine( new InMemoryStateStorage<IdAllocationState>( new IdAllocationState() ) );

		 private DirectReplicator<ReplicatedIdAllocationRequest> _replicator;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void consecutiveAllocationsFromSeparateIdGeneratorsForSameIdTypeShouldNotDuplicateWhenInitialIdIsZero()
		 public virtual void ConsecutiveAllocationsFromSeparateIdGeneratorsForSameIdTypeShouldNotDuplicateWhenInitialIdIsZero()
		 {
			  ConsecutiveAllocationFromSeparateIdGeneratorsForSameIdTypeShouldNotDuplicateForGivenInitialHighId( 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void consecutiveAllocationsFromSeparateIdGeneratorsForSameIdTypeShouldNotDuplicateWhenInitialIdIsNotZero()
		 public virtual void ConsecutiveAllocationsFromSeparateIdGeneratorsForSameIdTypeShouldNotDuplicateWhenInitialIdIsNotZero()
		 {
			  ConsecutiveAllocationFromSeparateIdGeneratorsForSameIdTypeShouldNotDuplicateForGivenInitialHighId( 1 );
		 }

		 private void ConsecutiveAllocationFromSeparateIdGeneratorsForSameIdTypeShouldNotDuplicateForGivenInitialHighId( long initialHighId )
		 {
			  ISet<long> idAllocations = new HashSet<long>();
			  int idRangeLength = 8;

			  FileSystemAbstraction fs = DefaultFileSystemRule.get();
			  File generatorFile1 = TestDirectory.file( "gen1" );
			  File generatorFile2 = TestDirectory.file( "gen2" );
			  using ( ReplicatedIdGenerator generatorOne = CreateForMemberWithInitialIdAndRangeLength( _memberA, initialHighId, idRangeLength, fs, generatorFile1 ), ReplicatedIdGenerator generatorTwo = CreateForMemberWithInitialIdAndRangeLength( _memberB, initialHighId, idRangeLength, fs, generatorFile2 ), )
			  {
					// First iteration is bootstrapping the set, so we do it outside the loop to avoid an if check in there
					long newId = generatorOne.NextId();
					idAllocations.Add( newId );

					for ( int i = 1; i < idRangeLength - initialHighId; i++ )
					{
						 newId = generatorOne.NextId();
						 bool wasNew = idAllocations.Add( newId );
						 assertTrue( "Id " + newId + " has already been returned", wasNew );
						 assertTrue( "Detected gap in id generation, missing " + ( newId - 1 ), idAllocations.Contains( newId - 1 ) );
					}

					for ( int i = 0; i < idRangeLength; i++ )
					{
						 newId = generatorTwo.NextId();
						 bool wasNew = idAllocations.Add( newId );
						 assertTrue( "Id " + newId + " has already been returned", wasNew );
						 assertTrue( "Detected gap in id generation, missing " + ( newId - 1 ), idAllocations.Contains( newId - 1 ) );
					}
			  }

		 }

		 private ReplicatedIdGenerator CreateForMemberWithInitialIdAndRangeLength( MemberId member, long initialHighId, int idRangeLength, FileSystemAbstraction fs, File file )
		 {
			  IDictionary<IdType, int> allocationSizes = java.util.Enum.GetValues( typeof( IdType ) ).ToDictionary( idType => idType, idType => idRangeLength );
			  ReplicatedIdRangeAcquirer acquirer = new ReplicatedIdRangeAcquirer( _replicator, _idAllocationStateMachine, allocationSizes, member, NullLogProvider.Instance );

			  return new ReplicatedIdGenerator( fs, file, IdType.ARRAY_BLOCK, () => initialHighId, acquirer, NullLogProvider.Instance, 10, true );
		 }
	}

}
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
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;

	using LifeRule = Neo4Net.Kernel.Lifecycle.LifeRule;
	using Lifespan = Neo4Net.Kernel.Lifecycle.Lifespan;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using OnDemandJobScheduler = Neo4Net.Test.OnDemandJobScheduler;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.raft_log_pruning_strategy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.NullLogProvider.getInstance;

	public class SegmentedRaftLogRotationTest
	{
		private bool InstanceFieldsInitialized = false;

		public SegmentedRaftLogRotationTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _testDirectory ).around( _fileSystemRule ).around( _life );
		}

		 private const int ROTATE_AT_SIZE_IN_BYTES = 100;

		 private readonly TestDirectory _testDirectory = TestDirectory.testDirectory();
		 private readonly LifeRule _life = new LifeRule( true );
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(testDirectory).around(fileSystemRule).around(life);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRotateOnAppendWhenRotateSizeIsReached() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRotateOnAppendWhenRotateSizeIsReached()
		 {
			  // When
			  SegmentedRaftLog log = _life.add( CreateRaftLog( ROTATE_AT_SIZE_IN_BYTES ) );
			  log.Append( new RaftLogEntry( 0, ReplicatedStringOfBytes( ROTATE_AT_SIZE_IN_BYTES ) ) );

			  // Then
			  File[] files = _fileSystemRule.get().listFiles(_testDirectory.directory(), (dir, name) => name.StartsWith("raft"));
			  assertEquals( 2, Files.Length );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRecoverToLatestStateAfterRotation() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRecoverToLatestStateAfterRotation()
		 {
			  // Given
			  int term = 0;
			  long indexToRestoreTo;
			  using ( Lifespan lifespan = new Lifespan() )
			  {
					SegmentedRaftLog log = lifespan.Add( CreateRaftLog( ROTATE_AT_SIZE_IN_BYTES ) );
					log.Append( new RaftLogEntry( term, ReplicatedStringOfBytes( ROTATE_AT_SIZE_IN_BYTES - 40 ) ) );
					indexToRestoreTo = log.Append( new RaftLogEntry( term, ReplicatedInteger.valueOf( 1 ) ) );
			  }

			  // When
			  SegmentedRaftLog log = _life.add( CreateRaftLog( ROTATE_AT_SIZE_IN_BYTES ) );

			  // Then
			  assertEquals( indexToRestoreTo, log.AppendIndex() );
			  assertEquals( term, log.ReadEntryTerm( indexToRestoreTo ) );
		 }

		 private static ReplicatedString ReplicatedStringOfBytes( int size )
		 {
			  return new ReplicatedString( StringUtils.repeat( "i", size ) );
		 }

		 private SegmentedRaftLog CreateRaftLog( long rotateAtSize )
		 {
			  LogProvider logProvider = Instance;
			  CoreLogPruningStrategy pruningStrategy = ( new CoreLogPruningStrategyFactory( raft_log_pruning_strategy.DefaultValue, logProvider ) ).newInstance();
			  return new SegmentedRaftLog( _fileSystemRule.get(), _testDirectory.directory(), rotateAtSize, new DummyRaftableContentSerializer(), logProvider, 0, Clocks.fakeClock(), new OnDemandJobScheduler(), pruningStrategy );
		 }
	}

}
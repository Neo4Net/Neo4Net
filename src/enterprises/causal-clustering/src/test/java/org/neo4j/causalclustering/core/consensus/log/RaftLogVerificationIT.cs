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
namespace Neo4Net.causalclustering.core.consensus.log
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.ReplicatedInteger.valueOf;

	public abstract class RaftLogVerificationIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.EphemeralFileSystemRule fsRule = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule FsRule = new EphemeralFileSystemRule();

		 private VerifyingRaftLog _raftLog;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract RaftLog createRaftLog() throws Throwable;
		 protected internal abstract RaftLog CreateRaftLog();

		 protected internal abstract long Operations();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Before()
		 {
			  _raftLog = new VerifyingRaftLog( CreateRaftLog() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void After()
		 {
			  _raftLog.verify();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void verifyAppend() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void VerifyAppend()
		 {
			  for ( int i = 0; i < Operations(); i++ )
			  {
					_raftLog.append( new RaftLogEntry( i * 3, valueOf( i * 7 ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void verifyAppendWithIntermittentTruncation() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void VerifyAppendWithIntermittentTruncation()
		 {
			  for ( int i = 0; i < Operations(); i++ )
			  {
					_raftLog.append( new RaftLogEntry( i * 3, valueOf( i * 7 ) ) );
					if ( i > 0 && i % 13 == 0 )
					{
						 _raftLog.truncate( _raftLog.appendIndex() - 10 );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void randomAppendAndTruncate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RandomAppendAndTruncate()
		 {
			  ThreadLocalRandom tlr = ThreadLocalRandom.current();
			  // given
			  for ( int i = 0; i < Operations(); i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int finalAppendIndex = tlr.nextInt(10) + 1;
					int finalAppendIndex = tlr.Next( 10 ) + 1;
					int appendIndex = finalAppendIndex;
					while ( appendIndex-- > 0 )
					{
						 _raftLog.append( new RaftLogEntry( i, valueOf( i ) ) );
					}

					int truncateIndex = tlr.Next( finalAppendIndex ); // truncate index must be strictly less than append index
					while ( truncateIndex-- > 0 )
					{
						 _raftLog.truncate( truncateIndex );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToAppendAfterSkip() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToAppendAfterSkip()
		 {
			  int term = 0;
			  _raftLog.append( new RaftLogEntry( term, valueOf( 10 ) ) );

			  int newTerm = 3;
			  _raftLog.skip( 5, newTerm );
			  RaftLogEntry newEntry = new RaftLogEntry( newTerm, valueOf( 20 ) );
			  _raftLog.append( newEntry ); // this will be logIndex 6

			  assertEquals( newEntry, RaftLogHelper.ReadLogEntry( _raftLog, 6 ) );
		 }
	}

}
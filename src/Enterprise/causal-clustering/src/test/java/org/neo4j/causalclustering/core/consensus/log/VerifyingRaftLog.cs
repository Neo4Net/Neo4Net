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
namespace Neo4Net.causalclustering.core.consensus.log
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	/// <summary>
	/// A log that uses the in-memory RAFT log as the reference implementation
	/// for verifying another RAFT log that is under test. All operations are
	/// mirrored to both logs and return values are checked for equality.
	/// 
	/// At the end of a test the content of both logs can be compared for
	/// equality using <seealso cref="VerifyingRaftLog.verify()"/>.
	/// </summary>
	internal class VerifyingRaftLog : RaftLog
	{
		 private InMemoryRaftLog _expected = new InMemoryRaftLog();
		 private readonly RaftLog _other;

		 internal VerifyingRaftLog( RaftLog other )
		 {
			  this._other = other;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long append(RaftLogEntry... entries) throws java.io.IOException
		 public override long Append( params RaftLogEntry[] entries )
		 {
			  long appendIndex = _expected.append( entries );
			  assertEquals( appendIndex, _other.append( entries ) );
			  return appendIndex;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void truncate(long fromIndex) throws java.io.IOException
		 public override void Truncate( long fromIndex )
		 {
			  _expected.truncate( fromIndex );
			  _other.truncate( fromIndex );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long prune(long safeIndex) throws java.io.IOException
		 public override long Prune( long safeIndex )
		 {
			  // the expected one should be able to prune exactly, while others are not required to
			  long pruneIndex = _other.prune( safeIndex );
			  assertEquals( pruneIndex, _expected.prune( pruneIndex ) );
			  return pruneIndex;
		 }

		 public override long AppendIndex()
		 {
			  long appendIndex = _expected.appendIndex();
			  assertEquals( appendIndex, _other.appendIndex() );
			  return appendIndex;
		 }

		 public override long PrevIndex()
		 {
			  long prevIndex = _expected.appendIndex();
			  assertEquals( prevIndex, _other.appendIndex() );
			  return prevIndex;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long readEntryTerm(long logIndex) throws java.io.IOException
		 public override long ReadEntryTerm( long logIndex )
		 {
			  long term = _expected.readEntryTerm( logIndex );
			  assertEquals( term, _other.readEntryTerm( logIndex ) );
			  return term;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public RaftLogCursor getEntryCursor(long fromIndex) throws java.io.IOException
		 public override RaftLogCursor GetEntryCursor( long fromIndex )
		 {
			  return _other.getEntryCursor( fromIndex );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long skip(long index, long term) throws java.io.IOException
		 public override long Skip( long index, long term )
		 {
			  long expectedAppendIndex = _expected.skip( index, term );
			  assertEquals( expectedAppendIndex, _other.skip( index, term ) );
			  return expectedAppendIndex;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verify() throws java.io.IOException
		 public virtual void Verify()
		 {
			  VerifyUsing( _other );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyUsing(RaftLog other) throws java.io.IOException
		 public virtual void VerifyUsing( RaftLog other )
		 {
			  assertEquals( _expected.appendIndex(), other.AppendIndex() );

			  VerifyTraversalUsingCursor( _expected, other );
			  VerifyDirectLookupForwards( _expected, other );
			  VerifyDirectLookupBackwards( _expected, other );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyDirectLookupForwards(InMemoryRaftLog expected, RaftLog other) throws java.io.IOException
		 private void VerifyDirectLookupForwards( InMemoryRaftLog expected, RaftLog other )
		 {
			  for ( long logIndex = expected.PrevIndex() + 1; logIndex <= expected.AppendIndex(); logIndex++ )
			  {
					DirectAssertions( expected, other, logIndex );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyDirectLookupBackwards(InMemoryRaftLog expected, RaftLog other) throws java.io.IOException
		 private void VerifyDirectLookupBackwards( InMemoryRaftLog expected, RaftLog other )
		 {
			  for ( long logIndex = expected.AppendIndex(); logIndex > expected.PrevIndex(); logIndex-- )
			  {
					DirectAssertions( expected, other, logIndex );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void directAssertions(InMemoryRaftLog expected, RaftLog other, long logIndex) throws java.io.IOException
		 private void DirectAssertions( InMemoryRaftLog expected, RaftLog other, long logIndex )
		 {
			  assertEquals( expected.ReadEntryTerm( logIndex ), other.ReadEntryTerm( logIndex ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyTraversalUsingCursor(RaftLog expected, RaftLog other) throws java.io.IOException
		 private void VerifyTraversalUsingCursor( RaftLog expected, RaftLog other )
		 {
			  long startIndex = expected.PrevIndex() + 1;
			  using ( RaftLogCursor expectedCursor = expected.GetEntryCursor( startIndex ) )
			  {
					using ( RaftLogCursor otherCursor = other.GetEntryCursor( startIndex ) )
					{
						 bool expectedNext;
						 do
						 {
							  expectedNext = expectedCursor.Next();
							  assertEquals( expectedNext, otherCursor.Next() );
							  if ( expectedNext )
							  {
									assertEquals( expectedCursor.get(), otherCursor.get() );
									assertEquals( expectedCursor.Index(), otherCursor.Index() );
							  }
						 } while ( expectedNext );
					}
			  }
		 }
	}

}
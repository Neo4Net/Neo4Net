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
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	using Test = org.junit.Test;

	using EndOfStreamException = Neo4Net.causalclustering.messaging.EndOfStreamException;
	using InMemoryClosableChannel = Neo4Net.Kernel.impl.transaction.log.InMemoryClosableChannel;

	public class SegmentHeaderTest
	{
		 private SegmentHeader.Marshal _marshal = new SegmentHeader.Marshal();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteAndReadHeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteAndReadHeader()
		 {
			  // given
			  long prevFileLastIndex = 1;
			  long version = 2;
			  long prevIndex = 3;
			  long prevTerm = 4;

			  SegmentHeader writtenHeader = new SegmentHeader( prevFileLastIndex, version, prevIndex, prevTerm );

			  InMemoryClosableChannel channel = new InMemoryClosableChannel();

			  // when
			  _marshal.marshal( writtenHeader, channel );
			  SegmentHeader readHeader = _marshal.unmarshal( channel );

			  // then
			  assertEquals( writtenHeader, readHeader );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionWhenReadingIncompleteHeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowExceptionWhenReadingIncompleteHeader()
		 {
			  // given
			  long prevFileLastIndex = 1;
			  long version = 2;
			  long prevIndex = 3;
			  long prevTerm = 4;

			  SegmentHeader writtenHeader = new SegmentHeader( prevFileLastIndex, version, prevIndex, prevTerm );
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();

			  channel.PutLong( writtenHeader.Version() );
			  channel.PutLong( writtenHeader.PrevIndex() );

			  // when
			  try
			  {
					_marshal.unmarshal( channel );
					fail();
			  }
			  catch ( EndOfStreamException )
			  {
					// expected
			  }
		 }
	}

}
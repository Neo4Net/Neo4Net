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
namespace Neo4Net.causalclustering.core.state.machines.id
{
	using Test = org.junit.Test;

	using EndOfStreamException = Neo4Net.causalclustering.messaging.EndOfStreamException;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using InMemoryVersionableReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.InMemoryVersionableReadableClosablePositionAwareChannel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class IdAllocationStateTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRoundtripToChannel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRoundtripToChannel()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IdAllocationState state = new IdAllocationState();
			  IdAllocationState state = new IdAllocationState();

			  for ( int i = 1; i <= 3; i++ )
			  {
					state.FirstUnallocated( IdType.NODE, 1024 * i );
					state.LogIndex( i );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IdAllocationState.Marshal marshal = new IdAllocationState.Marshal();
			  IdAllocationState.Marshal marshal = new IdAllocationState.Marshal();
			  // when
			  InMemoryVersionableReadableClosablePositionAwareChannel channel = new InMemoryVersionableReadableClosablePositionAwareChannel();
			  marshal.MarshalConflict( state, channel );
			  IdAllocationState unmarshalled = marshal.unmarshal( channel );

			  // then
			  assertEquals( state, unmarshalled );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionForHalfWrittenEntries() throws java.io.IOException, org.neo4j.causalclustering.messaging.EndOfStreamException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowExceptionForHalfWrittenEntries()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IdAllocationState state = new IdAllocationState();
			  IdAllocationState state = new IdAllocationState();

			  for ( int i = 1; i <= 3; i++ )
			  {
					state.FirstUnallocated( IdType.NODE, 1024 * i );
					state.LogIndex( i );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IdAllocationState.Marshal marshal = new IdAllocationState.Marshal();
			  IdAllocationState.Marshal marshal = new IdAllocationState.Marshal();
			  // when
			  InMemoryVersionableReadableClosablePositionAwareChannel channel = new InMemoryVersionableReadableClosablePositionAwareChannel();
			  marshal.MarshalConflict( state, channel );
			  // append some garbage
			  channel.PutInt( 1 ).putInt( 2 ).putInt( 3 ).putLong( 4L );
			  // read back in the first one
			  marshal.unmarshal( channel );

			  // the second one will be half read (the ints and longs appended above).
			  try
			  {
					marshal.unmarshal( channel );
					fail();
			  }
			  catch ( EndOfStreamException )
			  {
					// expected
			  }
		 }
	}

}
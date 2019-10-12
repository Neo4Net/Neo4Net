/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Kernel.impl.transaction.command
{
	using Test = org.junit.Test;

	using Record = Org.Neo4j.Kernel.impl.store.record.Record;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using CommandReader = Org.Neo4j.Storageengine.Api.CommandReader;
	using ReadableChannel = Org.Neo4j.Storageengine.Api.ReadableChannel;
	using StorageCommand = Org.Neo4j.Storageengine.Api.StorageCommand;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
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
	using static Org.Neo4j.Kernel.impl.transaction.command.Command.RelationshipGroupCommand;

	public class PhysicalLogCommandReadersTest
	{
		 private const long ID = 42;
		 private static readonly sbyte _inUseFlag = Record.IN_USE.byteValue();
		 private static readonly short _type = ( short )( short.MaxValue + 42 );
		 private static readonly int _typeAsInt = _type & 0xFFFF;
		 private const long NEXT = 42;
		 private const long FIRST_OUT = 42;
		 private const long FIRST_IN = 42;
		 private const long FIRST_LOOP = 42;
		 private const long OWNING_NODE = 42;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readRelGroupWithHugeTypeInV2_2_4() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadRelGroupWithHugeTypeInV2_2_4()
		 {
			  AssertCanReadRelGroup( new PhysicalLogCommandReaderV2_2_4() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readRelGroupWithHugeTypeInV2_2_10() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadRelGroupWithHugeTypeInV2_2_10()
		 {
			  AssertCanReadRelGroup( new PhysicalLogCommandReaderV2_2_10() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readRelGroupWithHugeTypeInV3_0() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadRelGroupWithHugeTypeInV3_0()
		 {
			  AssertCanReadRelGroup( new PhysicalLogCommandReaderV3_0() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readRelGroupWithHugeTypeInV3_0_2() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadRelGroupWithHugeTypeInV3_0_2()
		 {
			  AssertCanReadRelGroup( new PhysicalLogCommandReaderV3_0_2() );
		 }

		 private static void AssertDoesNotKnowAboutRelGroups( CommandReader reader )
		 {
			  try
			  {
					reader.Read( ChannelWithRelGroupRecord() );
					fail( "Exception expected" );
			  }
			  catch ( IOException e )
			  {
					assertEquals( "Unknown command type[" + NeoCommandType_Fields.RelGroupCommand + "]", e.Message );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertCanReadRelGroup(org.neo4j.storageengine.api.CommandReader reader) throws java.io.IOException
		 private void AssertCanReadRelGroup( CommandReader reader )
		 {
			  StorageCommand command = reader.Read( ChannelWithRelGroupRecord() );
			  AssertValidRelGroupCommand( command );
		 }

		 private static void AssertValidRelGroupCommand( StorageCommand command )
		 {
			  assertThat( command, instanceOf( typeof( RelationshipGroupCommand ) ) );
			  RelationshipGroupCommand relGroupCommand = ( RelationshipGroupCommand ) command;
			  RelationshipGroupRecord record = relGroupCommand.After;

			  assertEquals( ID, record.Id );
			  if ( _inUseFlag == Record.IN_USE.byteValue() )
			  {
					assertTrue( record.InUse() );
			  }
			  else if ( _inUseFlag == Record.NOT_IN_USE.byteValue() )
			  {
					assertFalse( record.InUse() );
			  }
			  else
			  {
					throw new System.InvalidOperationException( "Illegal inUse flag: " + _inUseFlag );
			  }
			  assertEquals( _typeAsInt, record.Type );
			  assertEquals( NEXT, record.Next );
			  assertEquals( FIRST_OUT, record.FirstOut );
			  assertEquals( FIRST_IN, record.FirstIn );
			  assertEquals( FIRST_LOOP, record.Next );
			  assertEquals( OWNING_NODE, record.OwningNode );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.storageengine.api.ReadableChannel channelWithRelGroupRecord() throws java.io.IOException
		 private static ReadableChannel ChannelWithRelGroupRecord()
		 {
			  return ChannelWithRelGroupRecord( ID, _inUseFlag, _type, NEXT, FIRST_OUT, FIRST_IN, FIRST_LOOP, OWNING_NODE );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.storageengine.api.ReadableChannel channelWithRelGroupRecord(long id, byte inUse, short type, long next, long firstOut, long firstIn, long firstLoop, long owningNode) throws java.io.IOException
		 private static ReadableChannel ChannelWithRelGroupRecord( long id, sbyte inUse, short type, long next, long firstOut, long firstIn, long firstLoop, long owningNode )
		 {
			  ReadableChannel channel = mock( typeof( ReadableChannel ) );

			  when( channel.Get() ).thenReturn(NeoCommandType_Fields.RelGroupCommand).thenReturn(inUse);
			  when( channel.Long ).thenReturn( id ).thenReturn( next ).thenReturn( firstOut ).thenReturn( firstIn ).thenReturn( firstLoop ).thenReturn( owningNode );
			  when( channel.Short ).thenReturn( type );

			  return channel;
		 }
	}

}
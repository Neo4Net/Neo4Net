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
namespace Neo4Net.Kernel.impl.transaction.command
{
	using Test = org.junit.Test;

	using AddRelationshipCommand = Neo4Net.Kernel.impl.index.IndexCommand.AddRelationshipCommand;
	using InMemoryClosableChannel = Neo4Net.Kernel.impl.transaction.log.InMemoryClosableChannel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class PhysicalLogNeoCommandReaderV2Test
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadIndexCommandHeaderCorrectly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadIndexCommandHeaderCorrectly()
		 {
			  // This bug manifested in header byte[1] {0,1,2}, which contains:
			  // [x   ,    ] start node needs long
			  // [ x  ,    ] end node needs long
			  // [  xx,xxxx] index name id
			  // would have the mask for reading "start node needs long" to 0x8, where it should have been 0x80.
			  // So we need an index name id which has the 0x8 bit set to falsely read that value as "true".
			  // Number 12 will do just fine.

			  // GIVEN
			  PhysicalLogCommandReaderV2_2_4 reader = new PhysicalLogCommandReaderV2_2_4();
			  InMemoryClosableChannel data = new InMemoryClosableChannel();
			  AddRelationshipCommand command = new AddRelationshipCommand();
			  sbyte indexNameId = ( sbyte )12;
			  long entityId = 123;
			  sbyte keyId = ( sbyte )1;
			  object value = "test value";
			  long startNode = 14;
			  long endNode = 15;

			  // WHEN
			  command.Init( indexNameId, entityId, keyId, value, startNode, endNode );
			  command.Serialize( data );

			  // THEN
			  AddRelationshipCommand readCommand = ( AddRelationshipCommand ) reader.Read( data );
			  assertEquals( indexNameId, readCommand.IndexNameId );
			  assertEquals( entityId, readCommand.EntityId );
			  assertEquals( keyId, readCommand.KeyId );
			  assertEquals( value, readCommand.Value );
			  assertEquals( startNode, readCommand.StartNode );
			  assertEquals( endNode, readCommand.EndNode );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProperlyMaskIndexIdFieldInIndexHeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProperlyMaskIndexIdFieldInIndexHeader()
		 {
			  /* This is how the index command header is laid out
			   * [x   ,    ] start node needs long
			   * [ x  ,    ] end node needs long
			   * [  xx,xxxx] index name id
			   * This means that the index name id can be in the range of 0 to 63. This test verifies that
			   * this constraint is actually respected
			   */

			  // GIVEN
			  PhysicalLogCommandReaderV2_2_4 reader = new PhysicalLogCommandReaderV2_2_4();
			  InMemoryClosableChannel data = new InMemoryClosableChannel();
			  // Here we take advantage of the fact that all index commands have the same header written out
			  AddRelationshipCommand command = new AddRelationshipCommand();
			  long entityId = 123;
			  sbyte keyId = ( sbyte )1;
			  object value = "test value";
			  long startNode = 14;
			  long endNode = 15;

			  for ( sbyte indexByteId = 0; indexByteId < 63; indexByteId++ )
			  {
					// WHEN
					command.Init( indexByteId, entityId, keyId, value, startNode, endNode );
					command.Serialize( data );

					// THEN
					AddRelationshipCommand readCommand = ( AddRelationshipCommand ) reader.Read( data );
					assertEquals( indexByteId, readCommand.IndexNameId );
					assertEquals( entityId, readCommand.EntityId );
					assertEquals( keyId, readCommand.KeyId );
					assertEquals( value, readCommand.Value );
					assertEquals( startNode, readCommand.StartNode );
					assertEquals( endNode, readCommand.EndNode );

					data.Reset();
			  }
		 }
	}

}
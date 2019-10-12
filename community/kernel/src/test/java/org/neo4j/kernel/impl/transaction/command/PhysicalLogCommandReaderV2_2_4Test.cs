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
	using ObjectIntHashMap = org.eclipse.collections.impl.map.mutable.primitive.ObjectIntHashMap;
	using Test = org.junit.Test;

	using IndexCommand = Org.Neo4j.Kernel.impl.index.IndexCommand;
	using RemoveCommand = Org.Neo4j.Kernel.impl.index.IndexCommand.RemoveCommand;
	using IndexDefineCommand = Org.Neo4j.Kernel.impl.index.IndexDefineCommand;
	using IndexEntityType = Org.Neo4j.Kernel.impl.index.IndexEntityType;
	using InMemoryClosableChannel = Org.Neo4j.Kernel.impl.transaction.log.InMemoryClosableChannel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class PhysicalLogCommandReaderV2_2_4Test
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadNoKeyIdAsMinusOne() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadNoKeyIdAsMinusOne()
		 {
			  // GIVEN
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();
			  IndexDefineCommand definitions = new IndexDefineCommand();
			  int indexNameId = 10;
			  definitions.Init( ObjectIntHashMap.newWithKeysValues( "myindex", indexNameId ), new ObjectIntHashMap<string>() );
			  definitions.Serialize( channel );
			  IndexCommand.RemoveCommand removeCommand = new IndexCommand.RemoveCommand();
			  removeCommand.Init( indexNameId, IndexEntityType.Node.id(), 1234, -1, null );
			  removeCommand.Serialize( channel );

			  // WHEN
			  PhysicalLogCommandReaderV2_2_4 reader = new PhysicalLogCommandReaderV2_2_4();
			  assertTrue( reader.Read( channel ) is IndexDefineCommand );
			  IndexCommand.RemoveCommand readRemoveCommand = ( IndexCommand.RemoveCommand ) reader.Read( channel );

			  // THEN
			  assertEquals( removeCommand.IndexNameId, readRemoveCommand.IndexNameId );
			  assertEquals( removeCommand.EntityType, readRemoveCommand.EntityType );
			  assertEquals( removeCommand.EntityId, readRemoveCommand.EntityId );
			  assertEquals( removeCommand.KeyId, readRemoveCommand.KeyId );
			  assertNull( removeCommand.Value );
		 }
	}

}
﻿/*
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
namespace Org.Neo4j.Kernel.Impl.Newapi
{
	using Test = org.junit.Test;

	using EntityNotFoundException = Org.Neo4j.@internal.Kernel.Api.exceptions.EntityNotFoundException;
	using StubNodeCursor = Org.Neo4j.@internal.Kernel.Api.helpers.StubNodeCursor;
	using StubPropertyCursor = Org.Neo4j.@internal.Kernel.Api.helpers.StubPropertyCursor;
	using StubRead = Org.Neo4j.@internal.Kernel.Api.helpers.StubRead;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.genericMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;

	public class CursorPropertyAccessorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLookupProperty() throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLookupProperty()
		 {
			  // given
			  long nodeId = 10;
			  Value value = Values.of( "abc" );
			  int propertyKeyId = 0;
			  StubNodeCursor nodeCursor = ( new StubNodeCursor() ).withNode(nodeId, new long[]{}, genericMap(999, Values.of(12345), propertyKeyId, value));
			  CursorPropertyAccessor accessor = new CursorPropertyAccessor( nodeCursor, new StubPropertyCursor(), new StubRead() );

			  // when
			  Value readValue = accessor.GetNodePropertyValue( nodeId, propertyKeyId );

			  // then
			  assertEquals( value, readValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNoValueOnMissingProperty() throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnNoValueOnMissingProperty()
		 {
			  // given
			  long nodeId = 10;
			  StubNodeCursor nodeCursor = ( new StubNodeCursor() ).withNode(nodeId, new long[]{}, genericMap(999, Values.of(12345)));
			  CursorPropertyAccessor accessor = new CursorPropertyAccessor( nodeCursor, new StubPropertyCursor(), new StubRead() );

			  // when
			  Value readValue = accessor.GetNodePropertyValue( nodeId, 0 );

			  // then
			  assertEquals( NO_VALUE, readValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOnEntityNotFound()
		 public virtual void ShouldThrowOnEntityNotFound()
		 {
			  // given
			  long nodeId = 10;
			  Value value = Values.of( "abc" );
			  int propertyKeyId = 0;
			  StubNodeCursor nodeCursor = ( new StubNodeCursor() ).withNode(nodeId, new long[]{}, genericMap(999, Values.of(12345), propertyKeyId, value));
			  CursorPropertyAccessor accessor = new CursorPropertyAccessor( nodeCursor, new StubPropertyCursor(), new StubRead() );

			  // when
			  try
			  {
					accessor.GetNodePropertyValue( nodeId + 1, propertyKeyId );
					fail();
			  }
			  catch ( EntityNotFoundException )
			  {
					// then good
			  }
		 }
	}

}
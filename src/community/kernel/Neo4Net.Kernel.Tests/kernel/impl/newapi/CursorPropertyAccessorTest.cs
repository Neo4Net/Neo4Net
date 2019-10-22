/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using Test = org.junit.Test;

	using IEntityNotFoundException = Neo4Net.Internal.Kernel.Api.exceptions.EntityNotFoundException;
	using StubNodeCursor = Neo4Net.Internal.Kernel.Api.helpers.StubNodeCursor;
	using StubPropertyCursor = Neo4Net.Internal.Kernel.Api.helpers.StubPropertyCursor;
	using StubRead = Neo4Net.Internal.Kernel.Api.helpers.StubRead;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.genericMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.NO_VALUE;

	public class CursorPropertyAccessorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLookupProperty() throws org.Neo4Net.internal.kernel.api.exceptions.EntityNotFoundException
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
//ORIGINAL LINE: @Test public void shouldReturnNoValueOnMissingProperty() throws org.Neo4Net.internal.kernel.api.exceptions.EntityNotFoundException
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
			  catch ( IEntityNotFoundException )
			  {
					// then good
			  }
		 }
	}

}
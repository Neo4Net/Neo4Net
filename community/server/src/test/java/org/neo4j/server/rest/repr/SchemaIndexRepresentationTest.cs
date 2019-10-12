using System.Collections.Generic;

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
namespace Org.Neo4j.Server.rest.repr
{
	using Test = org.junit.Test;

	using IndexDefinition = Org.Neo4j.Graphdb.schema.IndexDefinition;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;

	public class SchemaIndexRepresentationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeLabel()
		 public virtual void ShouldIncludeLabel()
		 {
			  // GIVEN
			  string labelName = "person";
			  string propertyKey = "name";
			  IndexDefinition definition = mock( typeof( IndexDefinition ) );
			  when( definition.NodeIndex ).thenReturn( true );
			  when( definition.Labels ).thenReturn( singletonList( label( labelName ) ) );
			  when( definition.PropertyKeys ).thenReturn( singletonList( propertyKey ) );
			  IndexDefinitionRepresentation representation = new IndexDefinitionRepresentation( definition );
			  IDictionary<string, object> serialized = RepresentationTestAccess.Serialize( representation );

			  // THEN
			  assertEquals( singletonList( propertyKey ), serialized["property_keys"] );
			  assertEquals( labelName, serialized["label"] );
		 }
	}

}
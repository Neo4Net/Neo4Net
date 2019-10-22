using System.Collections.Generic;

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
namespace Neo4Net.Server.rest.repr
{
	using Test = org.junit.Test;


	using MapWrappingWriter = Neo4Net.Server.rest.repr.formats.MapWrappingWriter;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.notNullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasEntry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class DatabaseRepresentationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeExpectedResourcePaths()
		 public virtual void ShouldIncludeExpectedResourcePaths()
		 {
			  // Given
			  DatabaseRepresentation repr = new DatabaseRepresentation();

			  // When
			  Dictionary<string, object> output = new Dictionary<string, object>();
			  repr.Serialize( new MappingSerializer( new MapWrappingWriter( output ), URI.create( "http://steveformayor.org" ), mock( typeof( ExtensionInjector ) ) ) );

			  // Then
			  assertThat( output, hasEntry( "relationship_index", "http://steveformayor.org/index/relationship" ) );
			  assertThat( output, hasEntry( "relationship_index", "http://steveformayor.org/index/relationship" ) );
			  assertThat( output, hasEntry( "node_index", "http://steveformayor.org/index/node" ) );
			  assertThat( output, hasEntry( "batch", "http://steveformayor.org/batch" ) );
			  assertThat( output, hasEntry( "constraints", "http://steveformayor.org/schema/constraint" ) );
			  assertThat( output, hasEntry( "node", "http://steveformayor.org/node" ) );
			  assertThat( output, hasEntry( "extensions_info", "http://steveformayor.org/ext" ) );
			  assertThat( output, hasEntry( "node_labels", "http://steveformayor.org/labels" ) );
			  assertThat( output, hasEntry( "indexes", "http://steveformayor.org/schema/index" ) );
			  assertThat( output, hasEntry( "cypher", "http://steveformayor.org/cypher" ) );
			  assertThat( output, hasEntry( "relationship_types", "http://steveformayor.org/relationship/types" ) );
			  assertThat( output, hasEntry( "relationship", "http://steveformayor.org/relationship" ) );
			  assertThat( output, hasEntry( "transaction", "http://steveformayor.org/transaction" ) );
			  assertThat( output, hasEntry( equalTo( "Neo4Net_version" ), notNullValue() ) );
		 }
	}

}
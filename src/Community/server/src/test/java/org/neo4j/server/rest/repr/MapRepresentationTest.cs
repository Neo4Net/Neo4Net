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


	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JsonFormat = Neo4Net.Server.rest.repr.formats.JsonFormat;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static false;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static true;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsNull.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;

	public class MapRepresentationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeMapWithSimpleTypes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeMapWithSimpleTypes()
		 {
			  MapRepresentation rep = new MapRepresentation( map( "nulls", null, "strings", "a string", "numbers", 42, "booleans", true ) );
			  OutputFormat format = new OutputFormat( new JsonFormat(), new URI("http://localhost/"), null );

			  string serializedMap = format.Assemble( rep );

			  IDictionary<string, object> map = JsonHelper.jsonToMap( serializedMap );
			  assertThat( map["nulls"], @is( nullValue() ) );
			  assertThat( map["strings"], @is( "a string" ) );
			  assertThat( map["numbers"], @is( 42 ) );
			  assertThat( map["booleans"], @is( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @SuppressWarnings("unchecked") public void shouldSerializeMapWithArrayTypes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeMapWithArrayTypes()
		 {
			  MapRepresentation rep = new MapRepresentation( map( "strings", new string[]{ "a string", "another string" }, "numbers", new int[]{ 42, 87 }, "booleans", new bool[]{ true, false }, "Booleans", new bool?[]{ TRUE, FALSE } ) );
			  OutputFormat format = new OutputFormat( new JsonFormat(), new URI("http://localhost/"), null );

			  string serializedMap = format.Assemble( rep );

			  IDictionary<string, object> map = JsonHelper.jsonToMap( serializedMap );
			  assertThat( map["strings"], @is( asList( "a string", "another string" ) ) );
			  assertThat( map["numbers"], @is( asList( 42, 87 ) ) );
			  assertThat( map["booleans"], @is( asList( true, false ) ) );
			  assertThat( map["Booleans"], @is( asList( true, false ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @SuppressWarnings("unchecked") public void shouldSerializeMapWithListsOfSimpleTypes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeMapWithListsOfSimpleTypes()
		 {
			  MapRepresentation rep = new MapRepresentation( map( "lists of nulls", asList( null, null ), "lists of strings", asList( "a string", "another string" ), "lists of numbers", asList( 23, 87, 42 ), "lists of booleans", asList( true, false, true ) ) );
			  OutputFormat format = new OutputFormat( new JsonFormat(), new URI("http://localhost/"), null );

			  string serializedMap = format.Assemble( rep );

			  IDictionary<string, object> map = JsonHelper.jsonToMap( serializedMap );
			  assertThat( map["lists of nulls"], @is( asList( null, null ) ) );
			  assertThat( map["lists of strings"], @is( asList( "a string", "another string" ) ) );
			  assertThat( map["lists of numbers"], @is( asList( 23, 87, 42 ) ) );
			  assertThat( map["lists of booleans"], @is( asList( true, false, true ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeMapWithMapsOfSimpleTypes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeMapWithMapsOfSimpleTypes()
		 {
			  MapRepresentation rep = new MapRepresentation( map( "maps with nulls", map( "nulls", null ), "maps with strings", map( "strings", "a string" ), "maps with numbers", map( "numbers", 42 ), "maps with booleans", map( "booleans", true ) ) );
			  OutputFormat format = new OutputFormat( new JsonFormat(), new URI("http://localhost/"), null );

			  string serializedMap = format.Assemble( rep );

			  IDictionary<string, object> map = JsonHelper.jsonToMap( serializedMap );
			  assertThat( ( ( System.Collections.IDictionary ) map["maps with nulls"] )["nulls"], @is( nullValue() ) );
			  assertThat( ( ( System.Collections.IDictionary ) map["maps with strings"] )["strings"], @is( "a string" ) );
			  assertThat( ( ( System.Collections.IDictionary ) map["maps with numbers"] )["numbers"], @is( 42 ) );
			  assertThat( ( ( System.Collections.IDictionary ) map["maps with booleans"] )["booleans"], @is( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @SuppressWarnings("unchecked") public void shouldSerializeArbitrarilyNestedMapsAndLists() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeArbitrarilyNestedMapsAndLists()
		 {
			  MapRepresentation rep = new MapRepresentation( map( "a map with a list in it", map( "a list", asList( 42, 87 ) ), "a list with a map in it", asList( map( "foo", "bar", "baz", false ) ) ) );
			  OutputFormat format = new OutputFormat( new JsonFormat(), new URI("http://localhost/"), null );

			  string serializedMap = format.Assemble( rep );

			  IDictionary<string, object> map = JsonHelper.jsonToMap( serializedMap );
			  assertThat( ( ( System.Collections.IDictionary ) map["a map with a list in it"] )["a list"], @is( asList( 42, 87 ) ) );
			  assertThat( ( ( System.Collections.IDictionary )( ( System.Collections.IList ) map["a list with a map in it"] )[0] )["foo"], @is( "bar" ) );
			  assertThat( ( ( System.Collections.IDictionary )( ( System.Collections.IList ) map["a list with a map in it"] )[0] )["baz"], @is( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeMapsWithNullKeys() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSerializeMapsWithNullKeys()
		 {
			  object[] values = new object[] { null, "string", 42, true, new string[]{ "a string", "another string" }, new int[]{ 42, 87 }, new bool[]{ true, false }, asList( true, false, true ), map( "numbers", 42, null, "something" ), map( "a list", asList( 42, 87 ), null, asList( "a", "b" ) ), asList( map( "foo", "bar", null, false ) ) };

			  foreach ( object value in values )
			  {
					MapRepresentation rep = new MapRepresentation( map( ( object ) null, value ) );
					OutputFormat format = new OutputFormat( new JsonFormat(), new URI("http://localhost/"), null );

					string serializedMap = format.Assemble( rep );

					IDictionary<string, object> map = JsonHelper.jsonToMap( serializedMap );

					assertEquals( 1, map.Count );
					object actual = map["null"];
					if ( value == null )
					{
						 assertNull( actual );
					}
					else
					{
						 assertNotNull( actual );
					}
			  }
		 }
	}

}
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
namespace Neo4Net.Server.rest.repr.formats
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;



//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasEntry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class DefaultFormatTest
	{
		 private DefaultFormat _input;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  JsonFormat inner = new JsonFormat();
			  List<MediaType> supported = new List<MediaType>();
			  MediaType requested = MediaType.APPLICATION_JSON_TYPE;
			  _input = new DefaultFormat( inner, supported, requested );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canReadEmptyMap() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanReadEmptyMap()
		 {
			  IDictionary<string, object> map = _input.readMap( "{}" );
			  assertNotNull( map );
			  assertTrue( "map is not empty", map.Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canReadMapWithTwoValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanReadMapWithTwoValues()
		 {
			  IDictionary<string, object> map = _input.readMap( "{\"key1\":\"value1\",     \"key2\":\"value11\"}" );
			  assertNotNull( map );
			  assertThat( map, hasEntry( "key1", "value1" ) );
			  assertThat( map, hasEntry( "key2", "value11" ) );
			  assertEquals( "map contained extra values", 2, map.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canReadMapWithNestedMap() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanReadMapWithNestedMap()
		 {
			  IDictionary<string, object> map = _input.readMap( "{\"nested\": {\"key\": \"valuable\"}}" );
			  assertNotNull( map );
			  assertThat( map, hasKey( "nested" ) );
			  assertEquals( "map contained extra values", 1, map.Count );
			  object nested = map["nested"];
			  assertThat( nested, instanceOf( typeof( System.Collections.IDictionary ) ) );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.Map<String, String> nestedMap = (java.util.Map<String, String>) nested;
			  IDictionary<string, string> nestedMap = ( IDictionary<string, string> ) nested;
			  assertThat( nestedMap, hasEntry( "key", "valuable" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.neo4j.server.rest.repr.MediaTypeNotSupportedException.class) public void failsWithTheCorrectExceptionWhenGettingTheWrongInput()
		 public virtual void FailsWithTheCorrectExceptionWhenGettingTheWrongInput()
		 {
			  _input.readValue( "<xml />" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.neo4j.server.rest.repr.MediaTypeNotSupportedException.class) public void failsWithTheCorrectExceptionWhenGettingTheWrongInput2() throws org.neo4j.server.rest.repr.BadInputException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailsWithTheCorrectExceptionWhenGettingTheWrongInput2()
		 {
			  _input.readMap( "<xml />" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.neo4j.server.rest.repr.MediaTypeNotSupportedException.class) public void failsWithTheCorrectExceptionWhenGettingTheWrongInput3()
		 public virtual void FailsWithTheCorrectExceptionWhenGettingTheWrongInput3()
		 {
			  _input.readUri( "<xml />" );
		 }
	}

}
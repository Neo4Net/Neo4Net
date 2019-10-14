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
namespace Neo4Net.Kernel.api.schema
{
	using TokenNameLookup = Neo4Net.@internal.Kernel.Api.TokenNameLookup;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class SchemaTestUtil
	{
		 private SchemaTestUtil()
		 {
		 }

		 public static void AssertEquality( object o1, object o2 )
		 {
			  assertEquals( o1.GetType().Name + "s are not equal", o1, o2 );
			  assertEquals( o1.GetType().Name + "s do not have the same hashcode", o1.GetHashCode(), o2.GetHashCode() );
		 }

		 public static void AssertArray( int[] values, params int[] expected )
		 {
			  assertThat( values.Length, equalTo( expected.Length ) );
			  for ( int i = 0; i < values.Length; i++ )
			  {
					assertEquals( format( "Expected %d, got %d at index %d", expected[i], values[i], i ), values[i], expected[i] );
			  }
		 }

		 public static TokenNameLookup simpleNameLookup = new TokenNameLookupAnonymousInnerClass();

		 private class TokenNameLookupAnonymousInnerClass : TokenNameLookup
		 {
			 public string labelGetName( int labelId )
			 {
				  return "Label" + labelId;
			 }

			 public string relationshipTypeGetName( int relationshipTypeId )
			 {
				  return "RelType" + relationshipTypeId;
			 }

			 public string propertyKeyGetName( int propertyKeyId )
			 {
				  return "property" + propertyKeyId;
			 }
		 }
	}

}
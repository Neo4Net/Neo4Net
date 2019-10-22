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


	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.repr.RepresentationTestAccess.serialize;

	public class PropertiesRepresentationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldContainAddedPropertiesWhenCreatedFromPropertyContainer()
		 public virtual void ShouldContainAddedPropertiesWhenCreatedFromPropertyContainer()
		 {
			  IDictionary<string, object> values = new Dictionary<string, object>();
			  values["foo"] = "bar";
			  IDictionary<string, object> serialized = serialize( new PropertiesRepresentation( Container( values ) ) );
			  assertEquals( "bar", serialized["foo"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeToMapWithSamePropertiesWhenCreatedFromPropertyContainer()
		 public virtual void ShouldSerializeToMapWithSamePropertiesWhenCreatedFromPropertyContainer()
		 {
			  IDictionary<string, object> values = new Dictionary<string, object>();
			  values["foo"] = "bar";
			  PropertiesRepresentation properties = new PropertiesRepresentation( Container( values ) );
			  IDictionary<string, object> map = serialize( properties );
			  assertEquals( values, map );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeToMap()
		 public virtual void ShouldSerializeToMap()
		 {
			  IDictionary<string, object> values = new Dictionary<string, object>();
			  values["string"] = "value";
			  values["int"] = 5;
			  values["long"] = 17L;
			  values["double"] = 3.14;
			  values["float"] = 42.0f;
			  values["string array"] = new string[] { "one", "two" };
			  values["long array"] = new long[] { 5L, 17L };
			  values["double array"] = new double[] { 3.14, 42.0 };

			  PropertiesRepresentation properties = new PropertiesRepresentation( Container( values ) );
			  IDictionary<string, object> map = serialize( properties );

			  assertEquals( "value", map["string"] );
			  assertEquals( 5, ( ( Number ) map["int"] ).longValue() );
			  assertEquals( 17, ( ( Number ) map["long"] ).longValue() );
			  assertEquals( 3.14, ( ( Number ) map["double"] ).doubleValue(), 0.0 );
			  assertEquals( 42.0, ( ( Number ) map["float"] ).doubleValue(), 0.0 );
			  AssertEqualContent( Arrays.asList( "one", "two" ), ( System.Collections.IList ) map["string array"] );
			  AssertEqualContent( Arrays.asList( 5L, 17L ), ( System.Collections.IList ) map["long array"] );
			  AssertEqualContent( Arrays.asList( 3.14, 42.0 ), ( System.Collections.IList ) map["double array"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToSignalEmptiness()
		 public virtual void ShouldBeAbleToSignalEmptiness()
		 {
			  PropertiesRepresentation properties = new PropertiesRepresentation( Container( new Dictionary<string, object>() ) );
			  IDictionary<string, object> values = new Dictionary<string, object>();
			  values["key"] = "value";
			  assertTrue( properties.Empty );
			  properties = new PropertiesRepresentation( Container( values ) );
			  assertFalse( properties.Empty );
		 }

		 private void AssertEqualContent<T1, T2>( IList<T1> expected, IList<T2> actual )
		 {
			  assertEquals( expected.Count, actual.Count );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.Iterator<?> ex = expected.iterator(), ac = actual.iterator(); ex.hasNext() && ac.hasNext();)
			  for ( IEnumerator<object> ex = expected.GetEnumerator(), ac = actual.GetEnumerator(); ex.hasNext() && ac.hasNext(); )
			  {
					assertEquals( ex.Current, ac.next() );
			  }
		 }

		 internal static IPropertyContainer Container( IDictionary<string, object> values )
		 {
			  IPropertyContainer container = mock( typeof( IPropertyContainer ) );
			  when( container.PropertyKeys ).thenReturn( values.Keys );
			  when( container.AllProperties ).thenReturn( values );
			  foreach ( KeyValuePair<string, object> entry in values.SetOfKeyValuePairs() )
			  {
					when( container.GetProperty( entry.Key, null ) ).thenReturn( entry.Value );
			  }
			  return container;
		 }
	}

}
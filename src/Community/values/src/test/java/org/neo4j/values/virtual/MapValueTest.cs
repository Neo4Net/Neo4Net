using System.Diagnostics;

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
namespace Neo4Net.Values.@virtual
{
	using Test = org.junit.jupiter.api.Test;

	using Iterables = Neo4Net.Helpers.Collection.Iterables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.numberValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_MAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.map;

	internal class MapValueTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFilterOnKeys()
		 internal virtual void ShouldFilterOnKeys()
		 {
			  // Given
			  MapValue @base = MapValue( "k1", stringValue( "v1" ), "k2", stringValue( "v2" ), "k3", stringValue( "v3" ) );

			  // When
			  MapValue filtered = @base.Filter( ( k, ignore ) => k.Equals( "k2" ) );

			  // Then
			  AssertMapValueEquals( filtered, MapValue( "k2", stringValue( "v2" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFilterOnValues()
		 internal virtual void ShouldFilterOnValues()
		 {
			  // Given
			  MapValue @base = MapValue( "k1", stringValue( "v1" ), "k2", stringValue( "v2" ), "k3", stringValue( "v3" ) );

			  // When
			  MapValue filtered = @base.Filter( ( ignore, v ) => v.Equals( stringValue( "v2" ) ) );

			  // Then
			  AssertMapValueEquals( filtered, MapValue( "k2", stringValue( "v2" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFilterOnKeysAndValues()
		 internal virtual void ShouldFilterOnKeysAndValues()
		 {
			  // Given
			  MapValue @base = MapValue( "k1", stringValue( "v1" ), "k2", stringValue( "v2" ), "k3", stringValue( "v3" ) );

			  // When
			  MapValue filtered = @base.Filter( ( k, v ) => k.Equals( "k1" ) && v.Equals( stringValue( "v2" ) ) );

			  // Then
			  AssertMapValueEquals( filtered, EMPTY_MAP );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUpdateWithIdenticalValues()
		 internal virtual void ShouldUpdateWithIdenticalValues()
		 {
			  // Given
			  MapValue @base = MapValue( "k1", stringValue( "v1" ), "k2", stringValue( "v2" ), "k3", stringValue( "v3" ) );

			  // When
			  MapValue updated = @base.UpdatedWith( "k3", stringValue( "v3" ) );

			  // Then
			  AssertMapValueEquals( updated, @base );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUpdateWithExistingKey()
		 internal virtual void ShouldUpdateWithExistingKey()
		 {
			  // Given
			  MapValue @base = MapValue( "k1", stringValue( "v1" ), "k2", stringValue( "v2" ), "k3", stringValue( "v3" ) );

			  // When
			  MapValue updated = @base.UpdatedWith( "k3", stringValue( "version3" ) );

			  // Then
			  AssertMapValueEquals( updated, MapValue( "k1", stringValue( "v1" ), "k2", stringValue( "v2" ), "k3", stringValue( "version3" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUpdateWithNewKey()
		 internal virtual void ShouldUpdateWithNewKey()
		 {
			  // Given
			  MapValue @base = MapValue( "k1", stringValue( "v1" ), "k2", stringValue( "v2" ), "k3", stringValue( "v3" ) );

			  // When
			  MapValue updated = @base.UpdatedWith( "k4", stringValue( "v4" ) );

			  // Then
			  AssertMapValueEquals( updated, MapValue( "k1", stringValue( "v1" ), "k2", stringValue( "v2" ), "k3", stringValue( "v3" ), "k4", stringValue( "v4" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUpdateWithOtherMapValue()
		 internal virtual void ShouldUpdateWithOtherMapValue()
		 {
			  // Given
			  MapValue a = MapValue( "k1", stringValue( "v1" ), "k2", stringValue( "v2" ), "k3", stringValue( "v3" ) );
			  MapValue b = MapValue( "k1", stringValue( "version1" ), "k2", stringValue( "version2" ), "k4", stringValue( "version4" ) );

			  // When
			  MapValue updated = a.UpdatedWith( b );

			  // Then
			  AssertMapValueEquals( updated, MapValue( "k1", stringValue( "version1" ), "k2", stringValue( "version2" ), "k3", stringValue( "v3" ), "k4", stringValue( "version4" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUpdateWithOtherMapValueWithSeveralNewKeys()
		 internal virtual void ShouldUpdateWithOtherMapValueWithSeveralNewKeys()
		 {
			  // Given
			  MapValue a = MapValue( "k1", stringValue( "v1" ), "k2", stringValue( "v2" ), "k3", stringValue( "v3" ) );
			  MapValue b = MapValue( "k1", stringValue( "version1" ), "k4", stringValue( "version4" ), "k5", stringValue( "version5" ) );

			  // When
			  MapValue updated = a.UpdatedWith( b );

			  // Then
			  AssertMapValueEquals( updated, MapValue( "k1", stringValue( "version1" ), "k2", stringValue( "v2" ), "k3", stringValue( "v3" ), "k4", stringValue( "version4" ), "k5", stringValue( "version5" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUpdateMultipleTimesMapValue()
		 internal virtual void ShouldUpdateMultipleTimesMapValue()
		 {
			  // Given
			  MapValue a = MapValue( "k1", stringValue( "v1" ), "k2", stringValue( "v2" ) );
			  MapValue b = MapValue( "k1", stringValue( "version1" ), "k4", stringValue( "version4" ) );
			  MapValue c = MapValue( "k3", stringValue( "v3" ) );

			  // When
			  MapValue updated = a.UpdatedWith( b ).updatedWith( c );

			  // Then
			  AssertMapValueEquals( updated, MapValue( "k1", stringValue( "version1" ), "k2", stringValue( "v2" ), "k3", stringValue( "v3" ), "k4", stringValue( "version4" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCompareTwoCombinedMapValues()
		 internal virtual void ShouldCompareTwoCombinedMapValues()
		 {
			  // Given
			  MapValue a = MapValue( "note", stringValue( "test" ), "Id", numberValue( 8 ) );
			  MapValue b = MapValue( "note", stringValue( "test" ), "Id", numberValue( 14 ) );

			  // When
			  MapValue x = MapValue().updatedWith(a);
			  MapValue y = MapValue().updatedWith(b);

			  assertThat( "Two simple maps should be different", a, not( equalTo( b ) ) );
			  assertThat( "Two combined maps should be different", x, not( equalTo( y ) ) );
		 }

		 private void AssertMapValueEquals( MapValue a, MapValue b )
		 {
			  assertThat( a, equalTo( b ) );
			  assertThat( a.Size(), equalTo(b.Size()) );
			  assertThat( a.GetHashCode(), equalTo(b.GetHashCode()) );
			  assertThat( a.Keys, containsInAnyOrder( Iterables.asArray( typeof( string ), b.Keys ) ) );
			  assertThat( Arrays.asList( a.Keys().asArray() ), containsInAnyOrder(b.Keys().asArray()) );
			  a.Foreach( ( k, v ) => assertThat( b.Get( k ), equalTo( v ) ) );
			  b.Foreach( ( k, v ) => assertThat( a.Get( k ), equalTo( v ) ) );
		 }

		 private MapValue MapValue( params object[] kv )
		 {
			  Debug.Assert( kv.Length % 2 == 0 );
			  string[] keys = new string[kv.Length / 2];
			  AnyValue[] values = new AnyValue[kv.Length / 2];
			  for ( int i = 0; i < kv.Length; i += 2 )
			  {
					keys[i / 2] = ( string ) kv[i];
					values[i / 2] = ( AnyValue ) kv[i + 1];
			  }
			  return map( keys, values );
		 }
	}

}
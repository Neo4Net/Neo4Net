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
namespace Neo4Net.Helpers
{
	using Test = org.junit.jupiter.api.Test;


	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using AnyValue = Neo4Net.Values.AnyValue;
	using ListValue = Neo4Net.Values.@virtual.ListValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsInstanceOf.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.intValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.stringValue;

	internal class ValueUtilsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleCollection()
		 internal virtual void ShouldHandleCollection()
		 {
			  // Given
			  ICollection<int> collection = Arrays.asList( 1, 2, 3 );

			  // When
			  AnyValue of = ValueUtils.of( collection );

			  // Then
			  assertThat( of, instanceOf( typeof( ListValue ) ) );
			  ListValue listValue = ( ListValue ) of;
			  assertThat( listValue.Value( 0 ), equalTo( intValue( 1 ) ) );
			  assertThat( listValue.Value( 1 ), equalTo( intValue( 2 ) ) );
			  assertThat( listValue.Value( 2 ), equalTo( intValue( 3 ) ) );
			  assertThat( listValue.Size(), equalTo(3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleIterator()
		 internal virtual void ShouldHandleIterator()
		 {
			  // Given
			  IEnumerator<int> iterator = Arrays.asList( 1, 2, 3 ).GetEnumerator();

			  // When
			  AnyValue of = ValueUtils.of( iterator );

			  // Then
			  assertThat( of, instanceOf( typeof( ListValue ) ) );
			  ListValue listValue = ( ListValue ) of;
			  assertThat( listValue.Value( 0 ), equalTo( intValue( 1 ) ) );
			  assertThat( listValue.Value( 1 ), equalTo( intValue( 2 ) ) );
			  assertThat( listValue.Value( 2 ), equalTo( intValue( 3 ) ) );
			  assertThat( listValue.Size(), equalTo(3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleMaps()
		 internal virtual void ShouldHandleMaps()
		 {
			  // Given
			  IDictionary<string, object> map = MapUtil.map( "a", Arrays.asList( "foo", 42 ) );

			  // When
			  AnyValue anyValue = ValueUtils.of( map );

			  // Then
			  assertThat( anyValue, instanceOf( typeof( MapValue ) ) );
			  MapValue mapValue = ( MapValue ) anyValue;
			  assertThat( mapValue.Get( "a" ), equalTo( VirtualValues.list( stringValue( "foo" ), intValue( 42 ) ) ) );
			  assertThat( mapValue.Size(), equalTo(1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleIterable()
		 internal virtual void ShouldHandleIterable()
		 {
			  // Given
			  IEnumerable<int> collection = Arrays.asList( 1, 2, 3 );

			  // When
			  AnyValue of = ValueUtils.of( collection );

			  // Then
			  assertThat( of, instanceOf( typeof( ListValue ) ) );
			  ListValue listValue = ( ListValue ) of;
			  assertThat( listValue.Value( 0 ), equalTo( intValue( 1 ) ) );
			  assertThat( listValue.Value( 1 ), equalTo( intValue( 2 ) ) );
			  assertThat( listValue.Value( 2 ), equalTo( intValue( 3 ) ) );
			  assertThat( listValue.Size(), equalTo(3) );
		 }
	}

}
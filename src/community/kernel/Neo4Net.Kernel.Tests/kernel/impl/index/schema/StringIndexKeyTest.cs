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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using Inject = Neo4Net.Test.extension.Inject;
	using RandomExtension = Neo4Net.Test.extension.RandomExtension;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.GenericKey.NO_ENTITY_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.NativeIndexKey.Inclusion.NEUTRAL;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(RandomExtension.class) class StringIndexKeyTest
	internal class StringIndexKeyTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.test.rule.RandomRule random;
		 private RandomRule _random;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReuseByteArrayForFairlySimilarSizedKeys()
		 internal virtual void ShouldReuseByteArrayForFairlySimilarSizedKeys()
		 {
			  // given
			  StringIndexKey key = new StringIndexKey();
			  key.BytesLength = 20;
			  sbyte[] first = key.Bytes;

			  // when
			  key.BytesLength = 25;
			  sbyte[] second = key.Bytes;

			  // then
			  assertSame( first, second );
			  assertThat( first.Length, greaterThanOrEqualTo( 25 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateNewByteArrayForVastlyDifferentKeySizes()
		 internal virtual void ShouldCreateNewByteArrayForVastlyDifferentKeySizes()
		 {
			  // given
			  StringIndexKey key = new StringIndexKey();
			  key.BytesLength = 20;
			  sbyte[] first = key.Bytes;

			  // when
			  key.BytesLength = 100;
			  sbyte[] second = key.Bytes;

			  // then
			  assertNotSame( first, second );
			  assertThat( first.Length, greaterThanOrEqualTo( 20 ) );
			  assertThat( second.Length, greaterThanOrEqualTo( 100 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDereferenceByteArrayWhenMaterializingValue()
		 internal virtual void ShouldDereferenceByteArrayWhenMaterializingValue()
		 {
			  // given
			  StringIndexKey key = new StringIndexKey();
			  key.BytesLength = 20;
			  sbyte[] first = key.Bytes;

			  // when
			  key.AsValue();
			  key.BytesLength = 25;
			  sbyte[] second = key.Bytes;

			  // then
			  assertNotSame( first, second );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void minimalSplitterForSameValueShouldDivideLeftAndRight()
		 internal virtual void MinimalSplitterForSameValueShouldDivideLeftAndRight()
		 {
			  // Given
			  StringLayout layout = new StringLayout();
			  StringIndexKey left = layout.NewKey();
			  StringIndexKey right = layout.NewKey();
			  StringIndexKey minimalSplitter = layout.NewKey();

			  // keys with same value but different IEntityId
			  TextValue value = _random.randomValues().nextTextValue();
			  left.Initialize( 1 );
			  right.Initialize( 2 );
			  left.initFromValue( 0, value, NEUTRAL );
			  right.initFromValue( 0, value, NEUTRAL );

			  // When creating minimal splitter
			  layout.MinimalSplitter( left, right, minimalSplitter );

			  // Then that minimal splitter need to correctly divide left and right
			  assertTrue( layout.Compare( left, minimalSplitter ) < 0, "Expected minimal splitter to be strictly greater than left but wasn't for value " + value );
			  assertTrue( layout.Compare( minimalSplitter, right ) <= 0, "Expected right to be greater than or equal to minimal splitter but wasn't for value " + value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void minimalSplitterShouldRemoveEntityIdIfPossible()
		 internal virtual void MinimalSplitterShouldRemoveEntityIdIfPossible()
		 {
			  // Given
			  StringLayout layout = new StringLayout();
			  StringIndexKey left = layout.NewKey();
			  StringIndexKey right = layout.NewKey();
			  StringIndexKey minimalSplitter = layout.NewKey();

			  string @string = _random.nextString();
			  TextValue leftValue = Values.stringValue( @string );
			  TextValue rightValue = Values.stringValue( @string + _random.randomValues().nextCharRaw() );

			  // keys with unique values
			  left.Initialize( 1 );
			  left.initFromValue( 0, leftValue, NEUTRAL );
			  right.Initialize( 2 );
			  right.initFromValue( 0, rightValue, NEUTRAL );

			  // When creating minimal splitter
			  layout.MinimalSplitter( left, right, minimalSplitter );

			  // Then that minimal splitter should have IEntity id shaved off
			  assertEquals( NO_ENTITY_ID, minimalSplitter.EntityId, "Expected minimal splitter to have IEntityId removed when constructed from keys with unique values: " + "left=" + leftValue + ", right=" + rightValue );
		 }
	}

}
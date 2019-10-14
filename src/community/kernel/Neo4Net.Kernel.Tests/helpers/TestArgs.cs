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


	using Neo4Net.Helpers.Args;
	using Neo4Net.Kernel.impl.util;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.Converters.mandatory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.Converters.optional;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.Converters.toInt;

	internal class TestArgs
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testInterleavedParametersWithValuesAndNot()
		 internal virtual void TestInterleavedParametersWithValuesAndNot()
		 {
			  string[] line = new string[] { "-host", "machine.foo.com", "-port", "1234", "-v", "-name", "othershell" };
			  Args args = Args.Parse( line );
			  assertEquals( "machine.foo.com", args.Get( "host", null ) );
			  assertEquals( "1234", args.Get( "port", null ) );
			  assertEquals( 1234, args.GetNumber( "port", null ).intValue() );
			  assertEquals( "othershell", args.Get( "name", null ) );
			  assertTrue( args.Has( "v" ) );
			  assertTrue( args.Orphans().Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testInterleavedEqualsArgsAndSplitKeyValue()
		 internal virtual void TestInterleavedEqualsArgsAndSplitKeyValue()
		 {
			  string[] line = new string[] { "-host=localhost", "-v", "--port", "1234", "param1", "-name=Something", "param2" };
			  Args args = Args.Parse( line );
			  assertEquals( "localhost", args.Get( "host", null ) );
			  assertTrue( args.Has( "v" ) );
			  assertEquals( 1234, args.GetNumber( "port", null ).intValue() );
			  assertEquals( "Something", args.Get( "name", null ) );

			  assertEquals( 2, args.Orphans().Count );
			  assertEquals( "param1", args.Orphans()[0] );
			  assertEquals( "param2", args.Orphans()[1] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testParameterWithDashValue()
		 internal virtual void TestParameterWithDashValue()
		 {
			  string[] line = new string[] { "-file", "-" };
			  Args args = Args.Parse( line );
			  assertEquals( 1, args.AsMap().Count );
			  assertEquals( "-", args.Get( "file", null ) );
			  assertTrue( args.Orphans().Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testEnum()
		 internal virtual void TestEnum()
		 {
			  string[] line = new string[] { "--enum=" + MyEnum.Second.name() };
			  Args args = Args.Parse( line );
			  Enum<MyEnum> result = args.GetEnum( typeof( MyEnum ), "enum", MyEnum.First );
			  assertEquals( MyEnum.Second, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testEnumWithDefault()
		 internal virtual void TestEnumWithDefault()
		 {
			  string[] line = new string[] {};
			  Args args = Args.Parse( line );
			  MyEnum result = args.GetEnum( typeof( MyEnum ), "enum", MyEnum.Third );
			  assertEquals( MyEnum.Third, result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testEnumWithInvalidValue()
		 internal virtual void TestEnumWithInvalidValue()
		 {
			  string[] line = new string[] { "--myenum=something" };
			  Args args = Args.Parse( line );
			  assertThrows( typeof( System.ArgumentException ), () => args.GetEnum(typeof(MyEnum), "myenum", MyEnum.Third) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldInterpretOption()
		 internal virtual void ShouldInterpretOption()
		 {
			  // GIVEN
			  int expectedValue = 42;
			  Args args = Args.Parse( "--arg", expectedValue.ToString() );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.neo4j.kernel.impl.util.Validator<int> validator = mock(org.neo4j.kernel.impl.util.Validator.class);
			  Validator<int> validator = mock( typeof( Validator ) );

			  // WHEN
			  int value = args.InterpretOption( "arg", mandatory(), toInt(), validator );

			  // THEN
			  assertEquals( expectedValue, value );
			  verify( validator ).validate( expectedValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldInterpretOrphan()
		 internal virtual void ShouldInterpretOrphan()
		 {
			  // GIVEN
			  int expectedValue = 42;
			  Args args = Args.Parse( expectedValue.ToString() );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.neo4j.kernel.impl.util.Validator<int> validator = mock(org.neo4j.kernel.impl.util.Validator.class);
			  Validator<int> validator = mock( typeof( Validator ) );

			  // WHEN
			  int value = args.InterpretOrphan( 0, mandatory(), toInt(), validator );

			  // THEN
			  assertEquals( expectedValue, value );
			  verify( validator ).validate( expectedValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldInterpretMultipleOptionValues()
		 internal virtual void ShouldInterpretMultipleOptionValues()
		 {
			  // GIVEN
			  ICollection<int> expectedValues = Arrays.asList( 12, 34, 56 );
			  IList<string> argList = new List<string>();
			  string key = "number";
			  foreach ( int value in expectedValues )
			  {
					argList.Add( "--" + key );
					argList.Add( value.ToString() );
			  }
			  Args args = Args.Parse( argList.ToArray() );

			  // WHEN
			  assertThrows( typeof( System.ArgumentException ), () => args.Get(key) );
			  ICollection<int> numbers = args.InterpretOptions( key, optional(), toInt() );

			  // THEN
			  assertEquals( expectedValues, numbers );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testBooleanWithDefault()
		 internal virtual void TestBooleanWithDefault()
		 {
			  // Given
			  Args args = Args.Parse( "--no_value" );

			  // When & then
			  assertThat( args.GetBoolean( "not_set", true, true ), equalTo( true ) );
			  assertThat( args.GetBoolean( "not_set", false, true ), equalTo( false ) );
			  assertThat( args.GetBoolean( "not_set", false, false ), equalTo( false ) );
			  assertThat( args.GetBoolean( "not_set", true, false ), equalTo( true ) );

			  assertThat( args.GetBoolean( "no_value", true, true ), equalTo( true ) );
			  assertThat( args.GetBoolean( "no_value", false, true ), equalTo( true ) );
			  assertThat( args.GetBoolean( "no_value", false, false ), equalTo( false ) );
			  assertThat( args.GetBoolean( "no_value", true, false ), equalTo( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGetAsMap()
		 internal virtual void ShouldGetAsMap()
		 {
			  // GIVEN
			  Args args = Args.Parse( "--with-value", "value", "--without-value" );

			  // WHEN
			  IDictionary<string, string> map = args.AsMap();

			  // THEN
			  assertEquals( stringMap( "with-value", "value", "without-value", null ), map );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldInterpretOptionMetadata()
		 internal virtual void ShouldInterpretOptionMetadata()
		 {
			  // GIVEN
			  Args args = Args.Parse( "--my-option:Meta", "my value", "--my-option:Other", "other value" );

			  // WHEN
			  ICollection<Option<string>> options = args.InterpretOptionsWithMetadata( "my-option", mandatory(), value => value );

			  // THEN
			  assertEquals( 2, options.Count );
			  IEnumerator<Option<string>> optionIterator = options.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  Option<string> first = optionIterator.next();
			  assertEquals( "my value", first.Value() );
			  assertEquals( "Meta", first.Metadata() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  Option<string> second = optionIterator.next();
			  assertEquals( "other value", second.Value() );
			  assertEquals( "Other", second.Metadata() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleLastOrphanParam()
		 internal virtual void ShouldHandleLastOrphanParam()
		 {
			  // Given
			  Args args = Args.WithFlags( "recovery" ).parse( "--recovery", "/tmp/graph.db" );

			  // When
			  IList<string> orphans = args.Orphans();

			  // Then
			  assertEquals( Arrays.asList( "/tmp/graph.db" ), orphans );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleOnlyFlagsAndNoArgs()
		 internal virtual void ShouldHandleOnlyFlagsAndNoArgs()
		 {
			  // Given
			  Args args = Args.WithFlags( "foo", "bar" ).parse( "-foo", "--bar" );

			  // When
			  IList<string> orphans = args.Orphans();

			  // Then
			  assertEquals( System.Linq.Enumerable.Empty<string>(), orphans );
			  assertTrue( args.GetBoolean( "foo", false, true ) );
			  assertTrue( args.GetBoolean( "bar", false, true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldStillAllowExplicitValuesForFlags()
		 internal virtual void ShouldStillAllowExplicitValuesForFlags()
		 {
			  // Given
			  Args args = Args.WithFlags( "foo", "bar" ).parse( "-foo=false", "--bar" );

			  // When
			  IList<string> orphans = args.Orphans();

			  // Then
			  assertEquals( Arrays.asList<string>(), orphans );
			  assertFalse( args.GetBoolean( "foo", false, false ) );
			  assertTrue( args.GetBoolean( "bar", false, true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleMixtureOfFlagsAndOrphanParams()
		 internal virtual void ShouldHandleMixtureOfFlagsAndOrphanParams()
		 {
			  // Given
			  Args args = Args.WithFlags( "big", "soft", "saysMeow" ).parse( "-big", "-size=120", "-soft=true", "withStripes", "-saysMeow=false", "-name=ShereKhan", "badTiger" );

			  // When
			  IList<string> orphans = args.Orphans();

			  // Then
			  assertEquals( Arrays.asList( "withStripes", "badTiger" ), orphans );

			  assertEquals( 120, args.GetNumber( "size", 0 ).intValue() );
			  assertEquals( "ShereKhan", args.Get( "name" ) );

			  assertTrue( args.GetBoolean( "big", false, true ) );
			  assertTrue( args.GetBoolean( "soft", false, false ) );
			  assertFalse( args.GetBoolean( "saysMeow", true, true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleFlagSpecifiedAsLastArgument()
		 internal virtual void ShouldHandleFlagSpecifiedAsLastArgument()
		 {
			  // Given
			  Args args = Args.WithFlags( "flag1", "flag2" ).parse( "-key=Foo", "-flag1", "false", "-value", "Bar", "-flag2", "false" );

			  // When
			  IList<string> orphans = args.Orphans();

			  // Then
			  assertTrue( orphans.Count == 0, "Orphan args expected to be empty, but were: " + orphans );
			  assertEquals( "Foo", args.Get( "key" ) );
			  assertEquals( "Bar", args.Get( "value" ) );
			  assertFalse( args.GetBoolean( "flag1", true ) );
			  assertFalse( args.GetBoolean( "flag2", true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRecognizeFlagsOfAnyForm()
		 internal virtual void ShouldRecognizeFlagsOfAnyForm()
		 {
			  // Given
			  Args args = Args.WithFlags( "flag1", "flag2", "flag3" ).parse( "-key1=Foo", "-flag1", "-key1", "Bar", "-flag2=true", "-key3=Baz", "-flag3", "true" );

			  // When
			  IList<string> orphans = args.Orphans();

			  // Then
			  assertTrue( orphans.Count == 0, "Orphan args expected to be empty, but were: " + orphans );
			  assertTrue( args.GetBoolean( "flag1", false, true ) );
			  assertTrue( args.GetBoolean( "flag2", false, false ) );
			  assertTrue( args.GetBoolean( "flag3", false, false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnEmptyCollectionForOptionalMissingOption()
		 internal virtual void ShouldReturnEmptyCollectionForOptionalMissingOption()
		 {
			  // Given
			  Args args = Args.WithFlags().parse();

			  // When
			  ICollection<string> interpreted = args.InterpretOptions( "something", optional(), value => value );

			  // Then
			  assertTrue( interpreted.Count == 0 );
		 }

		 private enum MyEnum
		 {
			  First,
			  Second,
			  Third
		 }
	}

}
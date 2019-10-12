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
namespace Neo4Net.Kernel.configuration
{
	using Test = org.junit.jupiter.api.Test;


	using InvalidSettingException = Neo4Net.Graphdb.config.InvalidSettingException;
	using Neo4Net.Graphdb.config;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.StringUtils.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.DURATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.INTEGER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.LONG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.NORMALIZED_RELATIVE_URI;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.NO_DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.PATH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.STRING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.STRING_LIST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.buildSetting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.except;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.list;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.matches;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.powerOf2;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.pathSetting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.range;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.setting;

	internal class SettingsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void parsesAbsolutePaths()
		 internal virtual void ParsesAbsolutePaths()
		 {
			  File absolutePath = ( new File( "some/path" ) ).AbsoluteFile;
			  File thePath = Settings.PATH.apply( absolutePath.ToString() );

			  assertEquals( absolutePath, thePath );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void doesntAllowRelativePaths()
		 internal virtual void DoesntAllowRelativePaths()
		 {
			  File relativePath = new File( "some/path" );

			  assertThrows( typeof( System.ArgumentException ), () => Settings.PATH.apply(relativePath.ToString()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pathSettingsProvideDefaultValues()
		 internal virtual void PathSettingsProvideDefaultValues()
		 {
			  File theDefault = ( new File( "/some/path" ) ).AbsoluteFile;
			  Setting<File> setting = pathSetting( "some.setting", theDefault.AbsolutePath );
			  assertThat( Config.Defaults().get(setting), @is(theDefault) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pathSettingsAreNullIfThereIsNoValueAndNoDefault()
		 internal virtual void PathSettingsAreNullIfThereIsNoValueAndNoDefault()
		 {
			  Setting<File> setting = pathSetting( "some.setting", NO_DEFAULT );
			  assertThat( Config.Defaults().get(setting), @is(nullValue()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHaveAUsefulToStringWhichIsUsedAsTheValidValuesInDocumentation()
		 internal virtual void ShouldHaveAUsefulToStringWhichIsUsedAsTheValidValuesInDocumentation()
		 {
			  assertThat( pathSetting( EMPTY, NO_DEFAULT ).ToString(), containsString("A filesystem path") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testInteger()
		 internal virtual void TestInteger()
		 {
			  Setting<int> setting = setting( "foo", INTEGER, "3" );

			  // Ok
			  assertThat( setting.apply( Map( stringMap( "foo", "4" ) ) ), equalTo( 4 ) );

			  // Bad
			  assertThrows( typeof( InvalidSettingException ), () => setting.apply(Map(stringMap("foo", "bar"))) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testList()
		 internal virtual void TestList()
		 {
			  Setting<IList<int>> setting = setting( "foo", list( ",", INTEGER ), "1,2,3,4" );
			  assertThat( setting.apply( Map( stringMap() ) ).ToString(), equalTo("[1, 2, 3, 4]") );

			  Setting<IList<int>> setting2 = setting( "foo", list( ",", INTEGER ), "1,2,3,4," );
			  assertThat( setting2.apply( Map( stringMap() ) ).ToString(), equalTo("[1, 2, 3, 4]") );

			  Setting<IList<int>> setting3 = setting( "foo", list( ",", INTEGER ), "" );
			  assertThat( setting3.apply( Map( stringMap() ) ).ToString(), equalTo("[]") );

			  Setting<IList<int>> setting4 = setting( "foo", list( ",", INTEGER ), "1,    2,3, 4,   5  " );
			  assertThat( setting4.apply( Map( stringMap() ) ).ToString(), equalTo("[1, 2, 3, 4, 5]") );

			  Setting<IList<int>> setting5 = setting( "foo", list( ",", INTEGER ), "1,    2,3, 4,   " );
			  assertThat( setting5.apply( Map( stringMap() ) ).ToString(), equalTo("[1, 2, 3, 4]") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testStringList()
		 internal virtual void TestStringList()
		 {
			  Setting<IList<string>> setting1 = setting( "apa", STRING_LIST, "foo,bar,baz" );
			  assertEquals( Arrays.asList( "foo", "bar", "baz" ), setting1.apply( Map( stringMap() ) ) );

			  Setting<IList<string>> setting2 = setting( "apa", STRING_LIST, "foo,  bar, BAZ   " );
			  assertEquals( Arrays.asList( "foo", "bar", "BAZ" ), setting2.apply( Map( stringMap() ) ) );

			  Setting<IList<string>> setting3 = setting( "apa", STRING_LIST, "" );
			  assertEquals( Collections.emptyList(), setting3.apply(Map(stringMap())) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testPowerOf2()
		 internal virtual void TestPowerOf2()
		 {
			  Setting<long> setting = buildSetting( "foo", LONG, "2" ).constraint( powerOf2() ).build();

			  // Ok
			  assertThat( setting.apply( Map( stringMap( "foo", "256" ) ) ), equalTo( 256L ) );

			  // Bad
			  assertThrows( typeof( InvalidSettingException ), () => setting.apply(Map(stringMap("foo", "255"))) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testMin()
		 internal virtual void TestMin()
		 {
			  Setting<int> setting = buildSetting( "foo", INTEGER, "3" ).constraint( min( 2 ) ).build();

			  // Ok
			  assertThat( setting.apply( Map( stringMap( "foo", "4" ) ) ), equalTo( 4 ) );

			  // Bad
			  assertThrows( typeof( InvalidSettingException ), () => setting.apply(Map(stringMap("foo", "1"))) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void exceptDoesNotAllowForbiddenValues()
		 internal virtual void ExceptDoesNotAllowForbiddenValues()
		 {
			  Setting<string> restrictedSetting = buildSetting( "foo", STRING, "test" ).constraint( except( "a", "b", "c" ) ).build();
			  assertEquals( "test", restrictedSetting.apply( Map( stringMap() ) ) );
			  assertEquals( "d", restrictedSetting.apply( Map( stringMap( "foo", "d" ) ) ) );
			  assertThrows( typeof( InvalidSettingException ), () => restrictedSetting.apply(Map(stringMap("foo", "a"))) );
			  assertThrows( typeof( InvalidSettingException ), () => restrictedSetting.apply(Map(stringMap("foo", "b"))) );
			  InvalidSettingException exception = assertThrows( typeof( InvalidSettingException ), () => restrictedSetting.apply(Map(stringMap("foo", "c"))) );
			  assertThat( exception.Message, containsString( "not allowed value is: c" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testMax()
		 internal virtual void TestMax()
		 {
			  Setting<int> setting = buildSetting( "foo", INTEGER, "3" ).constraint( max( 5 ) ).build();

			  // Ok
			  assertThat( setting.apply( Map( stringMap( "foo", "4" ) ) ), equalTo( 4 ) );

			  // Bad
			  assertThrows( typeof( InvalidSettingException ), () => setting.apply(Map(stringMap("foo", "7"))) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testRange()
		 internal virtual void TestRange()
		 {
			  Setting<int> setting = buildSetting( "foo", INTEGER, "3" ).constraint( range( 2, 5 ) ).build();

			  // Ok
			  assertThat( setting.apply( Map( stringMap( "foo", "4" ) ) ), equalTo( 4 ) );

			  // Bad
			  assertThrows( typeof( InvalidSettingException ), () => setting.apply(Map(stringMap("foo", "1"))) );
			  assertThrows( typeof( InvalidSettingException ), () => setting.apply(Map(stringMap("foo", "6"))) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testMatches()
		 internal virtual void TestMatches()
		 {
			  Setting<string> setting = buildSetting( "foo", STRING, "abc" ).constraint( matches( "a*b*c*" ) ).build();

			  // Ok
			  assertThat( setting.apply( Map( stringMap( "foo", "aaabbbccc" ) ) ), equalTo( "aaabbbccc" ) );

			  // Bad
			  assertThrows( typeof( InvalidSettingException ), () => setting.apply(Map(stringMap("foo", "cba"))) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testDurationWithBrokenDefault()
		 internal virtual void TestDurationWithBrokenDefault()
		 {
			  // Notice that the default value is less that the minimum
			  Setting<Duration> setting = buildSetting( "foo.bar", DURATION, "1s" ).constraint( min( DURATION.apply( "3s" ) ) ).build();
			  assertThrows( typeof( InvalidSettingException ), () => setting.apply(Map(stringMap())) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testDurationWithValueNotWithinConstraint()
		 internal virtual void TestDurationWithValueNotWithinConstraint()
		 {
			  Setting<Duration> setting = buildSetting( "foo.bar", DURATION, "3s" ).constraint( min( DURATION.apply( "3s" ) ) ).build();
			  assertThrows( typeof( InvalidSettingException ), () => setting.apply(Map(stringMap("foo.bar", "2s"))) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testDuration()
		 internal virtual void TestDuration()
		 {
			  Setting<Duration> setting = buildSetting( "foo.bar", DURATION, "3s" ).constraint( min( DURATION.apply( "3s" ) ) ).build();
			  assertThat( setting.apply( Map( stringMap( "foo.bar", "4s" ) ) ), equalTo( Duration.ofSeconds( 4 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void badDurationMissingNumber()
		 internal virtual void BadDurationMissingNumber()
		 {
			  Setting<Duration> setting = buildSetting( "foo.bar", DURATION ).build();
			  InvalidSettingException exception = assertThrows( typeof( InvalidSettingException ), () => setting.apply(Map(stringMap("foo.bar", "ms"))) );
			  assertThat( exception.Message, containsString( "Missing numeric value" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void badDurationInvalidUnit()
		 internal virtual void BadDurationInvalidUnit()
		 {
			  Setting<Duration> setting = buildSetting( "foo.bar", DURATION ).build();
			  InvalidSettingException exception = assertThrows( typeof( InvalidSettingException ), () => setting.apply(Map(stringMap("foo.bar", "2gigaseconds"))) );
			  assertThat( exception.Message, containsString( "Unrecognized unit 'gigaseconds'" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testDefault()
		 internal virtual void TestDefault()
		 {
			  Setting<int> setting = setting( "foo", INTEGER, "3" );

			  // Ok
			  assertThat( setting.apply( Map( stringMap() ) ), equalTo(3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testPaths()
		 internal virtual void TestPaths()
		 {
			  File directory = new File( "myDirectory" );
			  Setting<File> config = buildSetting( "config", PATH, ( new File( directory, "config.properties" ) ).AbsolutePath ).constraint( _isFile ).build();
			  assertThat( config.apply( Map( stringMap() ) ).AbsolutePath, equalTo((new File(directory, "config.properties")).AbsolutePath) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testInheritOneLevel()
		 internal virtual void TestInheritOneLevel()
		 {
			  Setting<int> root = setting( "root", INTEGER, "4" );
			  Setting<int> setting = buildSetting( "foo", INTEGER ).inherits( root ).build();

			  // Ok
			  assertThat( setting.apply( Map( stringMap( "foo", "1" ) ) ), equalTo( 1 ) );
			  assertThat( setting.apply( Map( stringMap() ) ), equalTo(4) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testInheritHierarchy()
		 internal virtual void TestInheritHierarchy()
		 {
			  // Test hierarchies
			  Setting<string> a = setting( "A", STRING, "A" ); // A defaults to A
			  Setting<string> b = buildSetting( "B", STRING, "B" ).inherits( a ).build(); // B defaults to B unless A is defined
			  Setting<string> c = buildSetting( "C", STRING, "C" ).inherits( b ).build(); // C defaults to C unless B is defined
			  Setting<string> d = buildSetting( "D", STRING ).inherits( b ).build(); // D defaults to B
			  Setting<string> e = buildSetting( "E", STRING ).inherits( d ).build(); // E defaults to D (hence B)

			  assertThat( c.apply( Map( stringMap( "C", "X" ) ) ), equalTo( "X" ) );
			  assertThat( c.apply( Map( stringMap( "B", "X" ) ) ), equalTo( "X" ) );
			  assertThat( c.apply( Map( stringMap( "A", "X" ) ) ), equalTo( "X" ) );
			  assertThat( c.apply( Map( stringMap( "A", "Y", "B", "X" ) ) ), equalTo( "X" ) );

			  assertThat( d.apply( Map( stringMap() ) ), equalTo("B") );
			  assertThat( e.apply( Map( stringMap() ) ), equalTo("B") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testLogicalLogRotationThreshold()
		 internal virtual void TestLogicalLogRotationThreshold()
		 {
			  // WHEN
			  Setting<long> setting = GraphDatabaseSettings.logical_log_rotation_threshold;
			  long defaultValue = setting.apply( Map( stringMap() ) );
			  long megaValue = setting.apply( Map( stringMap( setting.Name(), "10M" ) ) );
			  long gigaValue = setting.apply( Map( stringMap( setting.Name(), "10g" ) ) );

			  // THEN
			  assertThat( defaultValue, greaterThan( 0L ) );
			  assertEquals( 10 * 1024 * 1024, megaValue );
			  assertEquals( 10L * 1024 * 1024 * 1024, gigaValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testNormalizedRelativeURI()
		 internal virtual void TestNormalizedRelativeURI()
		 {
			  // Given
			  Setting<URI> uri = setting( "mySetting", NORMALIZED_RELATIVE_URI, "http://localhost:7474///db///data///" );

			  // When && then
			  assertThat( uri.apply( always => null ).ToString(), equalTo("/db/data") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void onlySingleInheritanceShouldBeAllowed()
		 internal virtual void OnlySingleInheritanceShouldBeAllowed()
		 {
			  Setting<string> a = setting( "A", STRING, "A" );
			  Setting<string> b = setting( "B", STRING, "B" );
			  assertThrows( typeof( AssertionError ), () => buildSetting("C", STRING, "C").inherits(a).inherits(b).build() );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static <From, To> System.Func<From,To> map(final java.util.Map<From,To> map)
		 private static System.Func<From, To> Map<From, To>( IDictionary<From, To> map )
		 {
			  return map.get;
		 }

		 private static System.Func<File, System.Func<string, string>, File> _isFile = ( path, settings ) =>
		 {
		  if ( path.exists() && !path.File )
		  {
				throw new System.ArgumentException( string.Format( "{0} must point to a file, not a directory", path.ToString() ) );
		  }

		  return path;
		 };
	}

}
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
namespace Neo4Net.CommandLine.Args
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;

	using IncorrectUsage = Neo4Net.CommandLine.Admin.IncorrectUsage;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class ArgumentsTest
	{
		 private Arguments _builder;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setup()
		 internal virtual void Setup()
		 {
			  _builder = new Arguments();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void throwsOnUnexpectedLongArgument()
		 internal virtual void ThrowsOnUnexpectedLongArgument()
		 {
			  IncorrectUsage incorrectUsage = assertThrows( typeof( IncorrectUsage ), () => _builder.withDatabase().parse(new string[]{ "--stacktrace" }) );
			  assertEquals( "unrecognized option: 'stacktrace'", incorrectUsage.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void throwsOnUnexpectedLongArgumentWithValue()
		 internal virtual void ThrowsOnUnexpectedLongArgumentWithValue()
		 {
			  IncorrectUsage incorrectUsage = assertThrows( typeof( IncorrectUsage ), () => _builder.withDatabase().parse(new string[]{ "--stacktrace=true" }) );
			  assertEquals( "unrecognized option: 'stacktrace'", incorrectUsage.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void throwsOnUnexpectedShortArgument()
		 internal virtual void ThrowsOnUnexpectedShortArgument()
		 {
			  IncorrectUsage incorrectUsage = assertThrows( typeof( IncorrectUsage ), () => _builder.withDatabase().parse(new string[]{ "-f" }) );
			  assertEquals( "unrecognized option: 'f'", incorrectUsage.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void throwsOnUnexpectedShortArgumentWithValue()
		 internal virtual void ThrowsOnUnexpectedShortArgumentWithValue()
		 {
			  IncorrectUsage incorrectUsage = assertThrows( typeof( IncorrectUsage ), () => _builder.withDatabase().parse(new string[]{ "-f=bob" }) );
			  assertEquals( "unrecognized option: 'f'", incorrectUsage.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void throwsOnUnexpectedPositionalArgument()
		 internal virtual void ThrowsOnUnexpectedPositionalArgument()
		 {
			  IncorrectUsage incorrectUsage = assertThrows( typeof( IncorrectUsage ), () => _builder.withDatabase().parse(new string[]{ "bob", "sob" }) );
			  assertEquals( "unrecognized arguments: 'bob sob'", incorrectUsage.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void throwsOnUnexpectedPositionalArgumentWhenExpectingSome()
		 internal virtual void ThrowsOnUnexpectedPositionalArgumentWhenExpectingSome()
		 {
			  IncorrectUsage incorrectUsage = assertThrows( typeof( IncorrectUsage ), () => _builder.withMandatoryPositionalArgument(0, "first").withOptionalPositionalArgument(1, "second").parse(new string[]{ "one", "two", "three", "four" }) );
			  assertEquals( "unrecognized arguments: 'three four'", incorrectUsage.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void throwsOnTooFewPositionalArguments()
		 internal virtual void ThrowsOnTooFewPositionalArguments()
		 {
			  IncorrectUsage incorrectUsage = assertThrows( typeof( IncorrectUsage ), () => _builder.withMandatoryPositionalArgument(0, "first").withOptionalPositionalArgument(1, "second").parse(new string[]{}) );
			  assertEquals( "not enough arguments", incorrectUsage.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void argumentNoValue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ArgumentNoValue()
		 {
			  Arguments args = _builder.withArgument( new OptionalBooleanArg( "flag", false, "description" ) );

			  args.Parse( new string[]{ "--flag" } );
			  assertTrue( args.GetBoolean( "flag" ) );

			  args.Parse( new string[0] );
			  assertFalse( args.GetBoolean( "flag" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void argumentWithEquals() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ArgumentWithEquals()
		 {
			  Arguments args = _builder.withArgument( new OptionalBooleanArg( "flag", false, "description" ) );

			  args.Parse( new string[]{ "--flag=true" } );
			  assertTrue( args.GetBoolean( "flag" ) );

			  args.Parse( new string[]{ "--flag=false" } );
			  assertFalse( args.GetBoolean( "flag" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void argumentWithSpace() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ArgumentWithSpace()
		 {
			  Arguments args = _builder.withArgument( new OptionalBooleanArg( "flag", false, "description" ) );

			  args.Parse( new string[]{ "--flag", "true" } );
			  assertTrue( args.GetBoolean( "flag" ) );

			  args.Parse( new string[]{ "--flag", "false" } );
			  assertFalse( args.GetBoolean( "flag" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void withDatabaseUsage()
		 internal virtual void WithDatabaseUsage()
		 {
			  assertEquals( "[--database=<name>]", _builder.withDatabase().usage() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void withDatabaseDescription()
		 internal virtual void WithDatabaseDescription()
		 {
			  assertEquals( string.Format( "How to use%n%noptions:%n" + "  --database=<name>   Name of database. [default:" + GraphDatabaseSettings.DEFAULT_DATABASE_NAME + "]" ), _builder.withDatabase().description("How to use") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void withDatabaseToUsage()
		 internal virtual void WithDatabaseToUsage()
		 {
			  assertEquals( "[--database=<name>] --to=<destination-path>", _builder.withDatabase().withTo("Destination file.").usage() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void withDatabaseToDescription()
		 internal virtual void WithDatabaseToDescription()
		 {
			  assertEquals( string.Format( "How to use%n%noptions:%n" + "  --database=<name>         Name of database. [default:" + GraphDatabaseSettings.DEFAULT_DATABASE_NAME + "]%n" + "  --to=<destination-path>   Destination file." ), _builder.withDatabase().withTo("Destination file.").description("How to use") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void withDatabaseToMultilineDescription()
		 internal virtual void WithDatabaseToMultilineDescription()
		 {
			  assertEquals( string.Format( "How to use%n%noptions:%n" + "  --database=<name>         Name of database. [default:" + GraphDatabaseSettings.DEFAULT_DATABASE_NAME + "]%n" + "  --to=<destination-path>   This is a long string which should wrap on right%n" + "                            col." ), _builder.withDatabase().withTo("This is a long string which should wrap on right col.").description("How to use") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void longNamesTriggerNewLineFormatting()
		 internal virtual void LongNamesTriggerNewLineFormatting()
		 {
			  assertEquals( string.Format( "How to use%n%noptions:%n" + "  --database=<name>%n" + "      Name of database. [default:" + GraphDatabaseSettings.DEFAULT_DATABASE_NAME + "]%n" + "  --to=<destination-path>%n" + "      This is a long string which should not wrap on right col.%n" + "  --loooooooooooooong-variable-name=<loooooooooooooong-variable-value>%n" + "      This is also a long string which should be printed on a new line because%n" + "      of long names." ), _builder.withDatabase().withTo("This is a long string which should not wrap on right col.").withArgument(new MandatoryNamedArg("loooooooooooooong-variable-name", "loooooooooooooong-variable-value", "This is also a long string which should be printed on a new line because of long " + "names.")).description("How to use") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void descriptionShouldHandleExistingNewlines()
		 internal virtual void DescriptionShouldHandleExistingNewlines()
		 {
			  assertEquals( string.Format( "This is the first line%n" + "And this is the second line%n" + "The third line is so long that it requires some wrapping by the code itself%n" + "because as you can see it just keeps going ang going and going and going and%n" + "going and going." ), _builder.description( string.Format( "This is the first line%n" + "And this is the second line%n" + "The third line is so long that it requires some wrapping by the code itself because " + "as you " + "can see it just keeps going ang going and going and going and going and going." ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void wrappingHandlesBothKindsOfLineEndingsAndOutputsPlatformDependentOnes()
		 internal virtual void WrappingHandlesBothKindsOfLineEndingsAndOutputsPlatformDependentOnes()
		 {
			  assertEquals( string.Format( "One with Linux%n" + "One with Windows%n" + "And one which is%n" + "just long and should%n" + "be wrapped by the%n" + "function" ), Arguments.WrapText( "One with Linux\n" + "One with Windows\r\n" + "And one which is just long and should be wrapped by the function", 20 ) );
		 }
	}

}
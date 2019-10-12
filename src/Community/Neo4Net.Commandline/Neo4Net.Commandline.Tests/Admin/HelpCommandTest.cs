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
namespace Neo4Net.Commandline.Admin
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using Mock = org.mockito.Mock;
	using MockitoAnnotations = org.mockito.MockitoAnnotations;


	using Arguments = Neo4Net.Commandline.Args.Arguments;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class HelpCommandTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock private System.Action<String> out;
		 private System.Action<string> @out;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  MockitoAnnotations.initMocks( this );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void printsUnknownCommandWhenUnknownCommandIsProvided()
		 internal virtual void PrintsUnknownCommandWhenUnknownCommandIsProvided()
		 {
			  CommandLocator commandLocator = mock( typeof( CommandLocator ) );
			  when( commandLocator.AllProviders ).thenReturn( Collections.emptyList() );
			  when( commandLocator.FindProvider( "foobar" ) ).thenThrow( new NoSuchElementException( "foobar" ) );

			  HelpCommand helpCommand = new HelpCommand( mock( typeof( Usage ) ), @out, commandLocator );

			  IncorrectUsage incorrectUsage = assertThrows( typeof( IncorrectUsage ), () => helpCommand.execute("foobar") );
			  assertThat( incorrectUsage.Message, containsString( "Unknown command: foobar" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void printsAvailableCommandsWhenUnknownCommandIsProvided()
		 internal virtual void PrintsAvailableCommandsWhenUnknownCommandIsProvided()
		 {
			  CommandLocator commandLocator = mock( typeof( CommandLocator ) );
			  IList<AdminCommand_Provider> mockCommands = new IList<AdminCommand_Provider> { MockCommand( "foo" ), MockCommand( "bar" ), MockCommand( "baz" ) };
			  when( commandLocator.AllProviders ).thenReturn( mockCommands );
			  when( commandLocator.FindProvider( "foobar" ) ).thenThrow( new NoSuchElementException( "foobar" ) );

			  HelpCommand helpCommand = new HelpCommand( mock( typeof( Usage ) ), @out, commandLocator );

			  IncorrectUsage incorrectUsage = assertThrows( typeof( IncorrectUsage ), () => helpCommand.execute("foobar") );
			  assertThat( incorrectUsage.Message, containsString( "Available commands are: foo bar baz" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testAdminUsage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TestAdminUsage()
		 {
			  CommandLocator commandLocator = mock( typeof( CommandLocator ) );
			  IList<AdminCommand_Provider> mockCommands = new IList<AdminCommand_Provider> { MockCommand( "foo" ), MockCommand( "bar" ), MockCommand( "baz" ) };
			  when( commandLocator.AllProviders ).thenReturn( mockCommands );

			  using ( MemoryStream baos = new MemoryStream() )
			  {
					PrintStream ps = new PrintStream( baos );

					Usage usage = new Usage( "neo4j-admin", commandLocator );

					HelpCommand helpCommand = new HelpCommand( usage, ps.println, commandLocator );

					helpCommand.Execute();

					assertEquals( string.Format( "usage: neo4j-admin <command>%n" + "%n" + "Manage your Neo4j instance.%n" + "%n" + "environment variables:%n" + "    NEO4J_CONF    Path to directory which contains neo4j.conf.%n" + "    NEO4J_DEBUG   Set to anything to enable debug output.%n" + "    NEO4J_HOME    Neo4j home directory.%n" + "    HEAP_SIZE     Set JVM maximum heap size during command execution.%n" + "                  Takes a number and a unit, for example 512m.%n" + "%n" + "available commands:%n" + "%n" + "General%n" + "    bar%n" + "        null%n" + "    baz%n" + "        null%n" + "    foo%n" + "        null%n" + "%n" + "Use neo4j-admin help <command> for more details.%n" ), baos.ToString() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void showsArgumentsAndDescriptionForSpecifiedCommand() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShowsArgumentsAndDescriptionForSpecifiedCommand()
		 {
			  CommandLocator commandLocator = mock( typeof( CommandLocator ) );
			  AdminCommand_Provider commandProvider = mock( typeof( AdminCommand_Provider ) );
			  when( commandProvider.Name() ).thenReturn("foobar");
			  Arguments arguments = ( new Arguments() ).withDatabase();
			  when( commandProvider.AllArguments() ).thenReturn(arguments);
			  when( commandProvider.PossibleArguments() ).thenReturn(Collections.singletonList(arguments));
			  when( commandProvider.Description() ).thenReturn("This is a description of the foobar command.");
			  when( commandLocator.FindProvider( "foobar" ) ).thenReturn( commandProvider );

			  using ( MemoryStream baos = new MemoryStream() )
			  {
					PrintStream ps = new PrintStream( baos );

					HelpCommand helpCommand = new HelpCommand( new Usage( "neo4j-admin", commandLocator ), ps.println, commandLocator );
					helpCommand.Execute( "foobar" );

					assertEquals( string.Format( "usage: neo4j-admin foobar [--database=<name>]%n" + "%n" + "environment variables:%n" + "    NEO4J_CONF    Path to directory which contains neo4j.conf.%n" + "    NEO4J_DEBUG   Set to anything to enable debug output.%n" + "    NEO4J_HOME    Neo4j home directory.%n" + "    HEAP_SIZE     Set JVM maximum heap size during command execution.%n" + "                  Takes a number and a unit, for example 512m.%n" + "%n" + "This is a description of the foobar command.%n" + "%n" + "options:%n" + "  --database=<name>   Name of database. [default:" + GraphDatabaseSettings.DEFAULT_DATABASE_NAME + "]%n" ), baos.ToString() );
			  }
		 }

		 private static AdminCommand_Provider MockCommand( string name )
		 {
			  AdminCommand_Provider commandProvider = mock( typeof( AdminCommand_Provider ) );
			  when( commandProvider.Name() ).thenReturn(name);
			  when( commandProvider.CommandSection() ).thenReturn(AdminCommandSection.General());
			  return commandProvider;
		 }
	}

}
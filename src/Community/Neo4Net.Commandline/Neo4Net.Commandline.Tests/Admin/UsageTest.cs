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
namespace Neo4Net.CommandLine.Admin
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using InOrder = org.mockito.InOrder;
	using Mock = org.mockito.Mock;
	using MockitoAnnotations = org.mockito.MockitoAnnotations;


	using Arguments = Neo4Net.CommandLine.Args.Arguments;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class UsageTest
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
//ORIGINAL LINE: @Test void shouldPrintUsageForACommand()
		 internal virtual void ShouldPrintUsageForACommand()
		 {
			  // given
			  AdminCommand_Provider commandProvider = MockCommand( "bam", "A summary", AdminCommandSection.General() );
			  AdminCommand_Provider[] commands = new AdminCommand_Provider[]{ commandProvider };
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Usage usage = new Usage("neo4j-admin", new CannedLocator(commands));
			  Usage usage = new Usage( "neo4j-admin", new CannedLocator( commands ) );

			  // when
			  usage.PrintUsageForCommand( commandProvider, @out );

			  // then
			  InOrder ordered = inOrder( @out );
			  ordered.verify( @out ).accept( "usage: neo4j-admin bam " );
			  ordered.verify( @out ).accept( "" );
			  ordered.verify( @out ).accept( "environment variables:" );
			  ordered.verify( @out ).accept( "    NEO4J_CONF    Path to directory which contains neo4j.conf." );
			  ordered.verify( @out ).accept( "    NEO4J_DEBUG   Set to anything to enable debug output." );
			  ordered.verify( @out ).accept( "    NEO4J_HOME    Neo4j home directory." );
			  ordered.verify( @out ).accept( "    HEAP_SIZE     Set JVM maximum heap size during command execution." );
			  ordered.verify( @out ).accept( "                  Takes a number and a unit, for example 512m." );
			  ordered.verify( @out ).accept( "" );
			  ordered.verify( @out ).accept( "description" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPrintUsageWithConfiguration()
		 internal virtual void ShouldPrintUsageWithConfiguration()
		 {
			  AdminCommand_Provider[] commands = new AdminCommand_Provider[]{ MockCommand( "bam", "A summary", AdminCommandSection.General() ) };
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Usage usage = new Usage("neo4j-admin", new CannedLocator(commands));
			  Usage usage = new Usage( "neo4j-admin", new CannedLocator( commands ) );
			  usage.Print( @out );

			  InOrder ordered = inOrder( @out );
			  ordered.verify( @out ).accept( "usage: neo4j-admin <command>" );
			  ordered.verify( @out ).accept( "" );
			  ordered.verify( @out ).accept( "Manage your Neo4j instance." );
			  ordered.verify( @out ).accept( "" );

			  ordered.verify( @out ).accept( "environment variables:" );
			  ordered.verify( @out ).accept( "    NEO4J_CONF    Path to directory which contains neo4j.conf." );
			  ordered.verify( @out ).accept( "    NEO4J_DEBUG   Set to anything to enable debug output." );
			  ordered.verify( @out ).accept( "    NEO4J_HOME    Neo4j home directory." );
			  ordered.verify( @out ).accept( "    HEAP_SIZE     Set JVM maximum heap size during command execution." );
			  ordered.verify( @out ).accept( "                  Takes a number and a unit, for example 512m." );
			  ordered.verify( @out ).accept( "" );

			  ordered.verify( @out ).accept( "available commands:" );
			  ordered.verify( @out ).accept( "General" );
			  ordered.verify( @out ).accept( "    bam" );
			  ordered.verify( @out ).accept( "        A summary" );
			  ordered.verify( @out ).accept( "" );
			  ordered.verify( @out ).accept( "Use neo4j-admin help <command> for more details." );
			  ordered.verifyNoMoreInteractions();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void commandsUnderSameAdminCommandSectionPrintableSectionShouldAppearTogether()
		 internal virtual void CommandsUnderSameAdminCommandSectionPrintableSectionShouldAppearTogether()
		 {
			  AdminCommand_Provider[] commands = new AdminCommand_Provider[]{ MockCommand( "first-command", "first-command", AdminCommandSection.General() ), MockCommand("second-command", "second-command", new TestGeneralSection()) };
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Usage usage = new Usage("neo4j-admin", new CannedLocator(commands));
			  Usage usage = new Usage( "neo4j-admin", new CannedLocator( commands ) );
			  usage.Print( @out );

			  InOrder ordered = inOrder( @out );
			  ordered.verify( @out ).accept( "usage: neo4j-admin <command>" );
			  ordered.verify( @out ).accept( "" );
			  ordered.verify( @out ).accept( "Manage your Neo4j instance." );
			  ordered.verify( @out ).accept( "" );

			  ordered.verify( @out ).accept( "environment variables:" );
			  ordered.verify( @out ).accept( "    NEO4J_CONF    Path to directory which contains neo4j.conf." );
			  ordered.verify( @out ).accept( "    NEO4J_DEBUG   Set to anything to enable debug output." );
			  ordered.verify( @out ).accept( "    NEO4J_HOME    Neo4j home directory." );
			  ordered.verify( @out ).accept( "    HEAP_SIZE     Set JVM maximum heap size during command execution." );
			  ordered.verify( @out ).accept( "                  Takes a number and a unit, for example 512m." );
			  ordered.verify( @out ).accept( "" );

			  ordered.verify( @out ).accept( "available commands:" );
			  ordered.verify( @out ).accept( "General" );
			  ordered.verify( @out ).accept( "    first-command" );
			  ordered.verify( @out ).accept( "        first-command" );
			  ordered.verify( @out ).accept( "    second-command" );
			  ordered.verify( @out ).accept( "        second-command" );
			  ordered.verify( @out ).accept( "" );
			  ordered.verify( @out ).accept( "Use neo4j-admin help <command> for more details." );
			  ordered.verifyNoMoreInteractions();
		 }

		 private class TestGeneralSection : AdminCommandSection
		 {

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public String printable()
			  public override string Printable()
			  {
					return "General";
			  }
		 }

		 private static AdminCommand_Provider MockCommand( string name, string summary, AdminCommandSection section )
		 {
			  AdminCommand_Provider commandProvider = mock( typeof( AdminCommand_Provider ) );
			  when( commandProvider.Name() ).thenReturn(name);
			  when( commandProvider.Summary() ).thenReturn(summary);
			  when( commandProvider.AllArguments() ).thenReturn(Arguments.NO_ARGS);
			  when( commandProvider.PossibleArguments() ).thenReturn(Collections.singletonList(Arguments.NO_ARGS));
			  when( commandProvider.Description() ).thenReturn("description");
			  when( commandProvider.CommandSection() ).thenReturn(section);
			  return commandProvider;
		 }
	}

}
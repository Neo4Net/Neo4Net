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
	using InOrder = org.mockito.InOrder;
	using Mock = org.mockito.Mock;
	using MockitoAnnotations = org.mockito.MockitoAnnotations;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class AdminCommandSectionTest
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
//ORIGINAL LINE: @Test void shouldPrintUsageForAllCommandsAlphabetically()
		 internal virtual void ShouldPrintUsageForAllCommandsAlphabetically()
		 {
			  AdminCommandSection generalSection = AdminCommandSection.General();

			  IList<AdminCommand_Provider> providers = new IList<AdminCommand_Provider> { MockCommand( "restore", "Restore" ), MockCommand( "bam", "A summary" ), MockCommand( "zzzz-last-one", "Another summary" ) };
			  generalSection.PrintAllCommandsUnderSection( @out, providers );

			  InOrder ordered = inOrder( @out );
			  ordered.verify( @out ).accept( "" );
			  ordered.verify( @out ).accept( "General" );
			  ordered.verify( @out ).accept( "    bam" );
			  ordered.verify( @out ).accept( "        A summary" );
			  ordered.verify( @out ).accept( "    restore" );
			  ordered.verify( @out ).accept( "        Restore" );
			  ordered.verify( @out ).accept( "    zzzz-last-one" );
			  ordered.verify( @out ).accept( "        Another summary" );
			  ordered.verifyNoMoreInteractions();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void equalsUsingReflection()
		 internal virtual void EqualsUsingReflection()
		 {
			  assertEquals( AdminCommandSection.General(), new TestGeneralSection() );
			  assertNotEquals( AdminCommandSection.General(), new TestAnotherGeneralSection() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void hashCodeUsingReflection()
		 internal virtual void HashCodeUsingReflection()
		 {
			  TestGeneralSection testGeneralSection = new TestGeneralSection();
			  TestAnotherGeneralSection testAnotherGeneralSection = new TestAnotherGeneralSection();
			  Dictionary<AdminCommandSection, string> map = new Dictionary<AdminCommandSection, string>();
			  map[AdminCommandSection.General()] = "General-Original";
			  map[testGeneralSection] = "General-Test";
			  map[testAnotherGeneralSection] = "General-AnotherTest";

			  assertEquals( 2, map.Count );
			  assertEquals( "General-Test", map[AdminCommandSection.General()] );
			  assertEquals( "General-Test", map[testGeneralSection] );
			  assertEquals( "General-AnotherTest", map[testAnotherGeneralSection] );
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

		 private class TestAnotherGeneralSection : AdminCommandSection
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public String printable()
			  public override string Printable()
			  {
					return "Another Section";
			  }
		 }

		 private static AdminCommand_Provider MockCommand( string name, string summary )
		 {
			  AdminCommand_Provider commandProvider = mock( typeof( AdminCommand_Provider ) );
			  when( commandProvider.Name() ).thenReturn(name);
			  when( commandProvider.Summary() ).thenReturn(summary);
			  when( commandProvider.CommandSection() ).thenReturn(AdminCommandSection.General());
			  return commandProvider;
		 }
	}

}
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
namespace Neo4Net.Kernel.builtinprocs
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.arrayContaining;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class IndexSpecifierTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFormatAsCanonicalRepresentation()
		 public virtual void ShouldFormatAsCanonicalRepresentation()
		 {
			  assertThat( IndexSpecifier.ByPatternOrName( ":Person(name)" ).ToString(), @is(":Person(name)") );
			  assertThat( IndexSpecifier.ByPattern( ":Person(name)" ).ToString(), @is(":Person(name)") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseASimpleLabel()
		 public virtual void ShouldParseASimpleLabel()
		 {
			  assertThat( IndexSpecifier.ByPatternOrName( ":Person_23(name)" ).label(), @is("Person_23") );
			  assertThat( IndexSpecifier.ByPattern( ":Person_23(name)" ).label(), @is("Person_23") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseASimpleProperty()
		 public virtual void ShouldParseASimpleProperty()
		 {
			  assertThat( IndexSpecifier.ByPatternOrName( ":Person(a_Name_123)" ).properties(), @is(arrayContaining("a_Name_123")) );
			  assertThat( IndexSpecifier.ByPattern( ":Person(a_Name_123)" ).properties(), @is(arrayContaining("a_Name_123")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseTwoProperties()
		 public virtual void ShouldParseTwoProperties()
		 {
			  assertThat( IndexSpecifier.ByPatternOrName( ":Person(name, lastName)" ).properties(), @is(arrayContaining("name", "lastName")) );
			  assertThat( IndexSpecifier.ByPattern( ":Person(name, lastName)" ).properties(), @is(arrayContaining("name", "lastName")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseManyProperties()
		 public virtual void ShouldParseManyProperties()
		 {
			  assertThat( IndexSpecifier.ByPatternOrName( ":Person(1, 2, 3, 4, 5, 6)" ).properties(), @is(arrayContaining("1", "2", "3", "4", "5", "6")) );
			  assertThat( IndexSpecifier.ByPattern( ":Person(1, 2, 3, 4, 5, 6)" ).properties(), @is(arrayContaining("1", "2", "3", "4", "5", "6")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseManyPropertiesWithWhitespace()
		 public virtual void ShouldParseManyPropertiesWithWhitespace()
		 {
			  string specification = ":Person( 1 , 2   ,3   ,4  )";
			  assertThat( IndexSpecifier.ByPatternOrName( specification ).properties(), @is(arrayContaining("1", "2", "3", "4")) );
			  assertThat( IndexSpecifier.ByPattern( specification ).properties(), @is(arrayContaining("1", "2", "3", "4")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseOddProperties()
		 public virtual void ShouldParseOddProperties()
		 {
			  assertThat( IndexSpecifier.ByPatternOrName( ": Person(1,    2lskgj_LKHGS, `3sdlkhs,   df``sas;g`, 4, `  5  `, 6)" ).properties(), @is(arrayContaining("1", "2lskgj_LKHGS", "3sdlkhs,   df``sas;g", "4", "  5  ", "6")) );
			  assertThat( IndexSpecifier.ByPattern( ": Person(1,    2lskgj_LKHGS, `3sdlkhs,   df``sas;g`, 4, `  5  `, 6)" ).properties(), @is(arrayContaining("1", "2lskgj_LKHGS", "3sdlkhs,   df``sas;g", "4", "  5  ", "6")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseANastyLabel()
		 public virtual void ShouldParseANastyLabel()
		 {
			  assertThat( IndexSpecifier.ByPatternOrName( ":`:(!\"£$%^&*( )`(name)" ).label(), @is(":(!\"£$%^&*( )") );
			  assertThat( IndexSpecifier.ByPattern( ":`:(!\"£$%^&*( )`(name)" ).label(), @is(":(!\"£$%^&*( )") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseANastyProperty()
		 public virtual void ShouldParseANastyProperty()
		 {
			  assertThat( IndexSpecifier.ByPatternOrName( ":Person(`(:!\"£$%^&*( )`)" ).properties(), @is(arrayContaining("(:!\"£$%^&*( )")) );
			  assertThat( IndexSpecifier.ByPattern( ":Person(`(:!\"£$%^&*( )`)" ).properties(), @is(arrayContaining("(:!\"£$%^&*( )")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void specifiersThatDoNotBeginWithColonAreIndexNames()
		 public virtual void SpecifiersThatDoNotBeginWithColonAreIndexNames()
		 {
			  IndexSpecifier spec = IndexSpecifier.ByPatternOrName( "my_index" );
			  assertThat( spec.Name(), @is("my_index") );
			  assertNull( spec.Label() );
			  assertNull( spec.Properties() );

			  spec = IndexSpecifier.ByName( "my_index" );
			  assertThat( spec.Name(), @is("my_index") );
			  assertNull( spec.Label() );
			  assertNull( spec.Properties() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void patternSpecifiersHaveNoName()
		 public virtual void PatternSpecifiersHaveNoName()
		 {
			  IndexSpecifier spec = IndexSpecifier.ByPattern( ":Person(name)" );
			  assertNotNull( spec.Label() );
			  assertNotNull( spec.Properties() );
			  assertNull( spec.Name() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProduceAReasonableErrorIfTheSpecificationCantBeParsed()
		 public virtual void ShouldProduceAReasonableErrorIfTheSpecificationCantBeParsed()
		 {
			  try
			  {
					IndexSpecifier.ByPatternOrName( "just some rubbish" );
					fail( "expected exception" );
			  }
			  catch ( System.ArgumentException )
			  {
					//expected
			  }

			  try
			  {
					IndexSpecifier.ByPattern( "rubbish" );
					fail( "expected exception" );
			  }
			  catch ( System.ArgumentException )
			  {
					//expected
			  }

			  try
			  {
					IndexSpecifier.ByName( ":Person(name)" );
					fail( "expected exception" );
			  }
			  catch ( System.ArgumentException )
			  {
					//expected
			  }
		 }
	}

}
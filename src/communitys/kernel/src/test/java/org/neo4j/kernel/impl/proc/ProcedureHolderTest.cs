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
namespace Neo4Net.Kernel.impl.proc
{
	using Test = org.junit.Test;

	using QualifiedName = Neo4Net.@internal.Kernel.Api.procs.QualifiedName;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class ProcedureHolderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetProcedureFromHolder()
		 public virtual void ShouldGetProcedureFromHolder()
		 {
			  // given
			  ProcedureHolder<string> procHolder = new ProcedureHolder<string>();
			  QualifiedName qualifiedName = new QualifiedName( new string[0], "CaseSensitive" );
			  string item = "CaseSensitiveItem";
			  procHolder.Put( qualifiedName, item, false );

			  // then
			  assertThat( procHolder.Get( qualifiedName ), equalTo( item ) );
			  assertThat( procHolder.IdOf( qualifiedName ), equalTo( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void okToHaveProcsOnlyDifferByCase()
		 public virtual void OkToHaveProcsOnlyDifferByCase()
		 {
			  // given
			  ProcedureHolder<string> procHolder = new ProcedureHolder<string>();
			  procHolder.Put( new QualifiedName( new string[0], "CASESENSITIVE" ), "CASESENSITIVEItem", false );
			  procHolder.Put( new QualifiedName( new string[0], "CaseSensitive" ), "CaseSensitiveItem", false );

			  // then
			  assertThat( procHolder.Get( new QualifiedName( new string[0], "CASESENSITIVE" ) ), equalTo( "CASESENSITIVEItem" ) );
			  assertThat( procHolder.Get( new QualifiedName( new string[0], "CaseSensitive" ) ), equalTo( "CaseSensitiveItem" ) );
			  assertThat( procHolder.IdOf( new QualifiedName( new string[0], "CASESENSITIVE" ) ), equalTo( 0 ) );
			  assertThat( procHolder.IdOf( new QualifiedName( new string[0], "CaseSensitive" ) ), equalTo( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetCaseInsensitiveFromHolder()
		 public virtual void ShouldGetCaseInsensitiveFromHolder()
		 {
			  // given
			  ProcedureHolder<string> procHolder = new ProcedureHolder<string>();
			  QualifiedName qualifiedName = new QualifiedName( new string[0], "CaseInSensitive" );
			  string item = "CaseInSensitiveItem";
			  procHolder.Put( qualifiedName, item, true );

			  // then
			  QualifiedName lowerCaseName = new QualifiedName( new string[0], "caseinsensitive" );
			  assertThat( procHolder.Get( lowerCaseName ), equalTo( item ) );
			  assertThat( procHolder.IdOf( lowerCaseName ), equalTo( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canOverwriteFunctionAndChangeCaseSensitivity()
		 public virtual void CanOverwriteFunctionAndChangeCaseSensitivity()
		 {
			  // given
			  ProcedureHolder<string> procHolder = new ProcedureHolder<string>();
			  QualifiedName qualifiedName = new QualifiedName( new string[0], "CaseInSensitive" );
			  string item = "CaseInSensitiveItem";
			  procHolder.Put( qualifiedName, item, true );

			  // then
			  QualifiedName lowerCaseName = new QualifiedName( new string[0], "caseinsensitive" );
			  assertThat( procHolder.Get( lowerCaseName ), equalTo( item ) );
			  assertThat( procHolder.IdOf( lowerCaseName ), equalTo( 0 ) );

			  // and then
			  procHolder.Put( qualifiedName, item, false );
			  assertNull( procHolder.Get( lowerCaseName ) );
			  try
			  {
					procHolder.IdOf( lowerCaseName );
					fail( "Should have failed to find with lower case" );
			  }
			  catch ( NoSuchElementException )
			  {
					// expected
			  }
		 }
	}

}
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
namespace Neo4Net.Kernel.impl.util
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class ValidatorsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory directory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Directory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindFilesByRegex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindFilesByRegex()
		 {
			  // GIVEN
			  ExistenceOfFile( "abc" );
			  ExistenceOfFile( "bcd" );

			  // WHEN/THEN
			  AssertValid( "abc" );
			  AssertValid( "bcd" );
			  AssertValid( "ab." );
			  AssertValid( ".*bc" );
			  AssertNotValid( "abcd" );
			  AssertNotValid( ".*de.*" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldValidateInList()
		 public virtual void ShouldValidateInList()
		 {
			  try
			  {
					Validators.InList( new string[] { "foo", "bar", "baz" } ).validate( "qux" );
					fail( "Should have failed to find item in list." );
			  }
			  catch ( System.ArgumentException e )
			  {
					assertThat( e.Message, containsString( "'qux' found but must be one of: [foo, bar, baz]." ) );
			  }

			  try
			  {
					Validators.InList( new string[] { "foo", "bar", "baz" } ).validate( "bar" );
			  }
			  catch ( System.ArgumentException )
			  {
					fail( "Should have found item in list." );
			  }
		 }

		 private void AssertNotValid( string @string )
		 {
			  try
			  {
					Validate( @string );
					fail( "Should have failed" );
			  }
			  catch ( System.ArgumentException )
			  { // Good
			  }
		 }

		 private void AssertValid( string fileByName )
		 {
			  Validate( fileByName );
		 }

		 private void Validate( string fileByName )
		 {
			  Validators.RegexFileExists.validate( Directory.file( fileByName ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void existenceOfFile(String name) throws java.io.IOException
		 private void ExistenceOfFile( string name )
		 {
			  Directory.file( name ).createNewFile();
		 }
	}

}
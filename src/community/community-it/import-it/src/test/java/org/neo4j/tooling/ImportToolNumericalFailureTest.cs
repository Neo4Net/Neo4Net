using System;
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
namespace Neo4Net.Tooling
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameter = org.junit.runners.Parameterized.Parameter;
	using Parameters = org.junit.runners.Parameterized.Parameters;


	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using InputException = Neo4Net.@unsafe.Impl.Batchimport.input.InputException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.tooling.ImportToolTest.assertExceptionContains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.tooling.ImportToolTest.importTool;

	/// <summary>
	/// Tests that we fail correctly when given strings which can't be interpreted as numbers when configured to interpret
	/// them as such.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class ImportToolNumericalFailureTest
	public class ImportToolNumericalFailureTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters(name = "{index}: {0}, \"{1}\", \"{2}\"") public static java.util.List<Object[]> types()
		 public static IList<object[]> Types()
		 {
			  List<object[]> @params = new List<object[]>();

			  foreach ( string type in Arrays.asList( "int", "long", "short", "byte", "float", "double" ) )
			  {
					foreach ( string val in Arrays.asList( " 1 7 ", " -1 7 ", " - 1 ", "   ", "   -  ", "-", "1. 0", "1 .", ".", "1E 10", " . 1" ) )
					{
						 // Only include decimals for floating point
						 if ( val.Contains( "." ) && !( type.Equals( "float" ) || type.Equals( "double" ) ) )
						 {
							  continue;
						 }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String error;
						 string error;
						 if ( type.Equals( "float" ) || type.Equals( "double" ) )
						 {
							  error = "Not a number: \"" + val + "\"";
						 }
						 else
						 {
							  error = "Not an integer: \"" + val + "\"";
						 }

						 string[] args = new string[3];
						 args[0] = type;
						 args[1] = val;
						 args[2] = error;

						 @params.Add( args );
					}
			  }

			  return @params;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameter public String type;
		 public string Type;

		 [Parameter(value : 1)]
		 public string Val;

		 [Parameter(value : 2)]
		 public string ExpectedError;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.EmbeddedDatabaseRule dbRule = new Neo4Net.test.rule.EmbeddedDatabaseRule().startLazily();
		 public readonly EmbeddedDatabaseRule DbRule = new EmbeddedDatabaseRule().startLazily();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.SuppressOutput suppressOutput = Neo4Net.test.rule.SuppressOutput.suppress(Neo4Net.test.rule.SuppressOutput.System.values());
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppress( SuppressOutput.System.values() );

		 private int _dataIndex;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void test() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Test()
		 {
			  // GIVEN
			  File data = File( FileName( "whitespace.csv" ) );
			  using ( PrintStream writer = new PrintStream( data ) )
			  {
					writer.println( ":LABEL,adult:" + Type );
					writer.println( "PERSON," + Val );
			  }

			  try
			  {
					// WHEN
					importTool( "--into", DbRule.DatabaseDirAbsolutePath, "--quote", "'", "--nodes", data.AbsolutePath );
					// THEN
					fail( "Expected import to fail" );
			  }
			  catch ( Exception e )
			  {
					assertExceptionContains( e, ExpectedError, typeof( InputException ) );
			  }
		 }

		 private string FileName( string name )
		 {
			  return _dataIndex++ + "-" + name;
		 }

		 private File File( string localname )
		 {
			  return DbRule.databaseLayout().file(localname);
		 }
	}

}
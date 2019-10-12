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
namespace Org.Neo4j.Kernel.impl.transaction.log.pruning
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class ThresholdConfigValueTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseCorrectly()
		 public virtual void ShouldParseCorrectly()
		 {
			  ThresholdConfigParser.ThresholdConfigValue value = ThresholdConfigParser.Parse( "25 files" );
			  assertEquals( "files", value.Type );
			  assertEquals( 25, value.Value );

			  value = ThresholdConfigParser.Parse( "4g size" );
			  assertEquals( "size", value.Type );
			  assertEquals( 1L << 32, value.Value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionOnUnknownType()
		 public virtual void ShouldThrowExceptionOnUnknownType()
		 {
			  try
			  {
					ThresholdConfigParser.Parse( "more than one spaces is invalid" );
					fail( "Should not parse unknown types" );
			  }
			  catch ( System.ArgumentException )
			  {
					// good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNoPruningForTrue()
		 public virtual void ShouldReturnNoPruningForTrue()
		 {
			  ThresholdConfigParser.ThresholdConfigValue value = ThresholdConfigParser.Parse( "true" );
			  assertSame( ThresholdConfigParser.ThresholdConfigValue.NoPruning, value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnKeepOneEntryForFalse()
		 public virtual void ShouldReturnKeepOneEntryForFalse()
		 {
			  ThresholdConfigParser.ThresholdConfigValue value = ThresholdConfigParser.Parse( "false" );
			  assertEquals( "entries", value.Type );
			  assertEquals( 1, value.Value );
		 }
	}

}
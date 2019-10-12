using System;
using System.Text;

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
namespace Neo4Net.Kernel.impl.util.dbstructure
{
	using Test = org.junit.Test;

	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class DbStructureArgumentFormatterTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFormatNull()
		 public virtual void ShouldFormatNull()
		 {
			  assertEquals( "null", FormatArgument( null ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFormatInts()
		 public virtual void ShouldFormatInts()
		 {
			  assertEquals( "0", FormatArgument( 0 ) );
			  assertEquals( "1", FormatArgument( 1 ) );
			  assertEquals( "-1", FormatArgument( -1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFormatLongs()
		 public virtual void ShouldFormatLongs()
		 {
			  assertEquals( "0L", FormatArgument( 0L ) );
			  assertEquals( "-1L", FormatArgument( -1L ) );
			  assertEquals( "1L", FormatArgument( 1L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFormatDoubles()
		 public virtual void ShouldFormatDoubles()
		 {
			  assertEquals( "1.0d", FormatArgument( 1.0d ) );
			  assertEquals( "Double.NaN", FormatArgument( Double.NaN ) );
			  assertEquals( "Double.POSITIVE_INFINITY", FormatArgument( double.PositiveInfinity ) );
			  assertEquals( "Double.NEGATIVE_INFINITY", FormatArgument( double.NegativeInfinity ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFormatIndexDescriptors()
		 public virtual void ShouldFormatIndexDescriptors()
		 {
			  assertEquals( "IndexDescriptorFactory.forSchema( SchemaDescriptorFactory.forLabel( 23, 42 ) )", FormatArgument( TestIndexDescriptorFactory.forLabel( 23, 42 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFormatUniquenessConstraints()
		 public virtual void ShouldFormatUniquenessConstraints()
		 {
			  assertEquals( "ConstraintDescriptorFactory.uniqueForLabel( 23, 42 )", FormatArgument( ConstraintDescriptorFactory.uniqueForLabel( 23, 42 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFormatCompositeUniquenessConstraints()
		 public virtual void ShouldFormatCompositeUniquenessConstraints()
		 {
			  assertEquals( "ConstraintDescriptorFactory.uniqueForLabel( 23, 42, 43 )", FormatArgument( ConstraintDescriptorFactory.uniqueForLabel( 23, 42, 43 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFormatNodeKeyConstraints()
		 public virtual void ShouldFormatNodeKeyConstraints()
		 {
			  assertEquals( "ConstraintDescriptorFactory.nodeKeyForLabel( 23, 42, 43 )", FormatArgument( ConstraintDescriptorFactory.nodeKeyForLabel( 23, 42, 43 ) ) );
		 }

		 private string FormatArgument( object arg )
		 {
			  StringBuilder builder = new StringBuilder();
			  try
			  {
					DbStructureArgumentFormatter.Instance.formatArgument( builder, arg );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
			  return builder.ToString();
		 }
	}

}
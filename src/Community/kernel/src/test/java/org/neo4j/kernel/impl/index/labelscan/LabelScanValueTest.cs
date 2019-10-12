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
namespace Neo4Net.Kernel.impl.index.labelscan
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class LabelScanValueTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddBits()
		 public virtual void ShouldAddBits()
		 {
			  // GIVEN
			  LabelScanValue value = new LabelScanValue();
			  value.Bits = 0b0000__1000_0100__0010_0001;

			  // WHEN
			  LabelScanValue other = new LabelScanValue();
			  other.Bits = 0b1100__0100_0100__0100_0100;
			  value.Add( other );

			  // THEN
			  assertEquals( 0b1100__1100_0100__0110_0101, value.Bits );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveBits()
		 public virtual void ShouldRemoveBits()
		 {
			  // GIVEN
			  LabelScanValue value = new LabelScanValue();
			  value.Bits = 0b1100__1000_0100__0010_0001;

			  // WHEN
			  LabelScanValue other = new LabelScanValue();
			  other.Bits = 0b1000__0100_0100__0100_0100;
			  value.Remove( other );

			  // THEN
			  assertEquals( 0b0100__1000_0000__0010_0001, value.Bits );
		 }
	}

}
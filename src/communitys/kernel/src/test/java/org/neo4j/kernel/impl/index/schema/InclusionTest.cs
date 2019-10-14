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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class InclusionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void inclusionsMustBeOrderedLowerToHigher()
		 internal virtual void InclusionsMustBeOrderedLowerToHigher()
		 {
			  NativeIndexKey.Inclusion[] values = Enum.GetValues( typeof( NativeIndexKey.Inclusion ) );
			  assertEquals( 3, values.Length, "Unexpected number of inclusions" );
			  assertEquals( NativeIndexKey.Inclusion.Low, values[0] );
			  assertEquals( NativeIndexKey.Inclusion.Neutral, values[1] );
			  assertEquals( NativeIndexKey.Inclusion.High, values[2] );
		 }
	}

}
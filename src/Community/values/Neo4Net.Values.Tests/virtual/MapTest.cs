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
namespace Neo4Net.Values.@virtual
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.utils.AnyValueTestUtil.assertEqual;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValueTestUtil.map;

	internal class MapTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBeEqualToItself()
		 internal virtual void ShouldBeEqualToItself()
		 {
			  assertEqual( map( "1", false, "20", new short[]{ 4 } ), map( "1", false, "20", new short[]{ 4 } ) );

			  assertEqual( map( "1", 101L, "20", "yo" ), map( "1", 101L, "20", "yo" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCoerce()
		 internal virtual void ShouldCoerce()
		 {
			  assertEqual( map( "1", 1, "20", 'a' ), map( "1", 1.0, "20", "a" ) );

			  assertEqual( map( "1", new sbyte[]{ 1 }, "20", new string[]{ "x" } ), map( "1", new short[]{ 1 }, "20", new char[]{ 'x' } ) );

			  assertEqual( map( "1", new int[]{ 1 }, "20", new double[]{ 2.0 } ), map( "1", new float[]{ 1.0f }, "20", new float[]{ 2.0f } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRecurse()
		 internal virtual void ShouldRecurse()
		 {
			  assertEqual( map( "1", map( "2", map( "3", "hi" ) ) ), map( "1", map( "2", map( "3", "hi" ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRecurseAndCoerce()
		 internal virtual void ShouldRecurseAndCoerce()
		 {
			  assertEqual( map( "1", map( "2", map( "3", "x" ) ) ), map( "1", map( "2", map( "3", 'x' ) ) ) );

			  assertEqual( map( "1", map( "2", map( "3", 1.0 ) ) ), map( "1", map( "2", map( "3", 1 ) ) ) );
		 }
	}

}
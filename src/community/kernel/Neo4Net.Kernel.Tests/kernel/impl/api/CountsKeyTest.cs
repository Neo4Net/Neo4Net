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
namespace Neo4Net.Kernel.Impl.Api
{
	using Test = org.junit.Test;

	using CountsKey = Neo4Net.Kernel.impl.store.counts.keys.CountsKey;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.counts.keys.CountsKeyFactory.nodeKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.counts.keys.CountsKeyFactory.relationshipKey;

	public class CountsKeyTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSortNodeKeysBeforeRelationshipKeys()
		 public virtual void ShouldSortNodeKeysBeforeRelationshipKeys()
		 {
			  // given
			  CountsKey[] array = new CountsKey[] { relationshipKey( 13, 2, 21 ), relationshipKey( 17, 2, 21 ), relationshipKey( 21, 1, 13 ), nodeKey( 13 ), nodeKey( 17 ), nodeKey( 21 ) };

			  // when
			  Arrays.sort( array );

			  // then
			  assertEquals( Arrays.asList( nodeKey( 13 ), nodeKey( 17 ), nodeKey( 21 ), relationshipKey( 21, 1, 13 ), relationshipKey( 13, 2, 21 ), relationshipKey( 17, 2, 21 ) ), Arrays.asList( array ) );
		 }
	}

}
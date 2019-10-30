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
namespace Neo4Net.Kernel.Impl.Api
{
	using Test = org.junit.Test;


	using Iterables = Neo4Net.Collections.Helpers.Iterables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.counts.keys.CountsKeyFactory.nodeKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.counts.keys.CountsKeyFactory.relationshipKey;

	public class CountsRecordStateTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportDifferencesBetweenDifferentStates()
		 public virtual void ShouldReportDifferencesBetweenDifferentStates()
		 {
			  // given
			  CountsRecordState oracle = new CountsRecordState();
			  CountsRecordState victim = new CountsRecordState();
			  oracle.IncrementNodeCount( 17, 5 );
			  victim.IncrementNodeCount( 17, 3 );
			  oracle.IncrementNodeCount( 12, 9 );
			  victim.IncrementNodeCount( 12, 9 );
			  oracle.IncrementRelationshipCount( 1, 2, 3, 19 );
			  victim.IncrementRelationshipCount( 1, 2, 3, 22 );
			  oracle.IncrementRelationshipCount( 1, 4, 3, 25 );
			  victim.IncrementRelationshipCount( 1, 4, 3, 25 );

			  // when
			  ISet<CountsRecordState.Difference> differences = Iterables.asSet( oracle.Verify( victim ) );

			  // then
			  assertEquals(differences, asSet(new CountsRecordState.Difference(nodeKey(17), 0, 5, 0, 3), new CountsRecordState.Difference(relationshipKey(1, 2, 3), 0, 19, 0, 22)
			 ));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReportAnythingForEqualStates()
		 public virtual void ShouldNotReportAnythingForEqualStates()
		 {
			  // given
			  CountsRecordState oracle = new CountsRecordState();
			  CountsRecordState victim = new CountsRecordState();
			  oracle.IncrementNodeCount( 17, 5 );
			  victim.IncrementNodeCount( 17, 5 );
			  oracle.IncrementNodeCount( 12, 9 );
			  victim.IncrementNodeCount( 12, 9 );
			  oracle.IncrementRelationshipCount( 1, 4, 3, 25 );
			  victim.IncrementRelationshipCount( 1, 4, 3, 25 );

			  // when
			  IList<CountsRecordState.Difference> differences = oracle.Verify( victim );

			  // then
			  assertTrue( differences.ToString(), differences.Count == 0 );
		 }
	}

}
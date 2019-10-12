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
namespace Neo4Net.Kernel.impl.store.id
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class ReuseExcessBatchIdsOnRestartIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.EmbeddedDatabaseRule();
		 public readonly DatabaseRule Db = new EmbeddedDatabaseRule();

		 // Knowing that ids are grabbed in batches internally we only create one node and later assert
		 // that the excess ids that were only grabbed, but not used can be reused.
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReuseExcessBatchIdsWhichWerentUsedBeforeClose() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReuseExcessBatchIdsWhichWerentUsedBeforeClose()
		 {
			  // given
			  Node firstNode;
			  using ( Transaction tx = Db.beginTx() )
			  {
					firstNode = Db.createNode();
					tx.Success();
			  }

			  // when
			  Db.restartDatabase();

			  Node secondNode;
			  using ( Transaction tx = Db.beginTx() )
			  {
					secondNode = Db.createNode();
					tx.Success();
			  }

			  // then
			  assertEquals( firstNode.Id + 1, secondNode.Id );
		 }
	}

}
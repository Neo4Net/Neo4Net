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
namespace Neo4Net.GraphDb
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterables.count;

	public class FirstStartupIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.TestDirectory testDir = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeEmptyWhenFirstStarted()
		 public virtual void ShouldBeEmptyWhenFirstStarted()
		 {
			  // When
			  File storeDir = TestDir.absolutePath();
			  IGraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(storeDir);

			  // Then
			  using ( Transaction ignore = Db.beginTx() )
			  {
					assertEquals( 0, count( Db.AllNodes ) );
					assertEquals( 0, count( Db.AllRelationships ) );
					assertEquals( 0, count( Db.AllRelationshipTypes ) );
					assertEquals( 0, count( Db.AllLabels ) );
					assertEquals( 0, count( Db.AllPropertyKeys ) );
			  }

			  Db.shutdown();
		 }
	}

}
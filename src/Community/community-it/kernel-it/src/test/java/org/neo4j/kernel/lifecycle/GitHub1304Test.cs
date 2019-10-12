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
namespace Neo4Net.Kernel.Lifecycle
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using BatchInserter = Neo4Net.@unsafe.Batchinsert.BatchInserter;
	using BatchInserters = Neo4Net.@unsafe.Batchinsert.BatchInserters;

	public class GitHub1304Test
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.fs.DefaultFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenBatchInserterWhenArrayPropertyUpdated4TimesThenShouldNotFail() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void GivenBatchInserterWhenArrayPropertyUpdated4TimesThenShouldNotFail()
		 {
			  BatchInserter batchInserter = BatchInserters.inserter( TestDirectory.databaseDir(), FileSystemRule.get() );

			  long nodeId = batchInserter.createNode( Collections.emptyMap() );

			  for ( int i = 0; i < 4; i++ )
			  {
					batchInserter.SetNodeProperty( nodeId, "array", new sbyte[]{ 2, 3, 98, 1, 43, 50, 3, 33, 51, 55, 116, 16, 23, 56, 9, ( sbyte ) - 10, 1, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1 } );
			  }

			  batchInserter.GetNodeProperties( nodeId ); //fails here
			  batchInserter.Shutdown();
		 }
	}

}
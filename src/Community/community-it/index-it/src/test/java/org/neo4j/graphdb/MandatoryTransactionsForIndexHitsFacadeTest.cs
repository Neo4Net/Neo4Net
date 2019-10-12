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
namespace Neo4Net.Graphdb
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Neo4Net.Graphdb.index;
	using Neo4Net.Graphdb.index;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class MandatoryTransactionsForIndexHitsFacadeTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.ImpermanentDatabaseRule dbRule = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public ImpermanentDatabaseRule DbRule = new ImpermanentDatabaseRule();

		 private IndexHits<Node> _indexHits;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  Index<Node> index = CreateIndex();
			  _indexHits = QueryIndex( index );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMandateTransactionsForUsingIterator()
		 public virtual void ShouldMandateTransactionsForUsingIterator()
		 {
			  using ( ResourceIterator<Node> iterator = _indexHits.GetEnumerator() )
			  {
					try
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 iterator.hasNext();

						 fail( "Transactions are mandatory, also for reads" );
					}
					catch ( NotInTransactionException )
					{ // Expected
					}

					try
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 iterator.next();

						 fail( "Transactions are mandatory, also for reads" );
					}
					catch ( NotInTransactionException )
					{ // Expected
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMandateTransactionsForGetSingle()
		 public virtual void ShouldMandateTransactionsForGetSingle()
		 {
			  try
			  {
					_indexHits.Single;

					fail( "Transactions are mandatory, also for reads" );
			  }
			  catch ( NotInTransactionException )
			  { // Expected
			  }
		 }

		 private Index<Node> CreateIndex()
		 {
			  GraphDatabaseService graphDatabaseService = DbRule.GraphDatabaseAPI;
			  using ( Transaction transaction = graphDatabaseService.BeginTx() )
			  {
					Index<Node> index = graphDatabaseService.Index().forNodes("foo");
					transaction.Success();
					return index;
			  }
		 }

		 private IndexHits<Node> QueryIndex( Index<Node> index )
		 {
			  GraphDatabaseService graphDatabaseService = DbRule.GraphDatabaseAPI;
			  using ( Transaction ignored = graphDatabaseService.BeginTx() )
			  {
					IndexHits<Node> hits = index.get( "foo", 42 );
					hits.Close();
					return hits;
			  }
		 }
	}

}
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
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Neo4Net.GraphDb.index;
	using Neo4Net.GraphDb.index;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class MandatoryTransactionsForIndexHitsFacadeTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.ImpermanentDatabaseRule dbRule = new org.Neo4Net.test.rule.ImpermanentDatabaseRule();
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
			  IGraphDatabaseService IGraphDatabaseService = DbRule.GraphDatabaseAPI;
			  using ( Transaction transaction = IGraphDatabaseService.BeginTx() )
			  {
					Index<Node> index = IGraphDatabaseService.Index().forNodes("foo");
					transaction.Success();
					return index;
			  }
		 }

		 private IndexHits<Node> QueryIndex( Index<Node> index )
		 {
			  IGraphDatabaseService IGraphDatabaseService = DbRule.GraphDatabaseAPI;
			  using ( Transaction ignored = IGraphDatabaseService.BeginTx() )
			  {
					IndexHits<Node> hits = index.get( "foo", 42 );
					hits.Close();
					return hits;
			  }
		 }
	}

}
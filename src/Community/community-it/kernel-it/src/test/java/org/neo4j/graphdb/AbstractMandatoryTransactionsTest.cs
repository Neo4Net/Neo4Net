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
	using Rule = org.junit.Rule;

	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public abstract class AbstractMandatoryTransactionsTest<T>
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.EmbeddedDatabaseRule dbRule = new org.neo4j.test.rule.EmbeddedDatabaseRule();
		 public EmbeddedDatabaseRule DbRule = new EmbeddedDatabaseRule();

		 public virtual T ObtainEntity()
		 {
			  GraphDatabaseService graphDatabaseService = DbRule.GraphDatabaseAPI;

			  using ( Transaction tx = graphDatabaseService.BeginTx() )
			  {
					T result = ObtainEntityInTransaction( graphDatabaseService );
					tx.Success();

					return result;
			  }
		 }

		 public virtual void ObtainEntityInTerminatedTransaction( System.Action<T> f )
		 {
			  GraphDatabaseService graphDatabaseService = DbRule.GraphDatabaseAPI;

			  using ( Transaction tx = graphDatabaseService.BeginTx() )
			  {
					T result = ObtainEntityInTransaction( graphDatabaseService );
					tx.Terminate();

					f( result );
			  }
		 }

		 protected internal abstract T ObtainEntityInTransaction( GraphDatabaseService graphDatabaseService );

		 public static void AssertFacadeMethodsThrowNotInTransaction<T>( T entity, System.Action<T>[] methods )
		 {
			  foreach ( System.Action<T> method in methods )
			  {
					try
					{
						 method( entity );

						 fail( "Transactions are mandatory, also for reads: " + method );
					}
					catch ( NotInTransactionException )
					{
						 // awesome
					}
			  }
		 }

		 public virtual void AssertFacadeMethodsThrowAfterTerminate( System.Action<T>[] methods )
		 {
			  foreach ( system.Action<T> method in methods )
			  {
					ObtainEntityInTerminatedTransaction(entity =>
					{
					 try
					 {
						  method.accept( entity );

						  fail( "Transaction was terminated, yet not exception thrown in: " + method );
					 }
					 catch ( TransactionTerminatedException )
					 {
						  // awesome
					 }
					});
			  }
		 }
	}

}
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
namespace Neo4Net.Kernel.Impl.Api.integrationtest
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ConstraintViolationException = Neo4Net.GraphDb.ConstraintViolationException;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.Test;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;
	using Neo4Net.Test.rule.concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.rule.concurrent.OtherThreadRule.isWaiting;

	public class UniquenessConstraintValidationConcurrencyIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.DatabaseRule database = new org.Neo4Net.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule Database = new ImpermanentDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.concurrent.OtherThreadRule<Void> otherThread = new org.Neo4Net.test.rule.concurrent.OtherThreadRule<>();
		 public readonly OtherThreadRule<Void> OtherThread = new OtherThreadRule<Void>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowConcurrentCreationOfNonConflictingData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowConcurrentCreationOfNonConflictingData()
		 {
			  // given
			  Database.executeAndCommit( CreateUniquenessConstraint( "Label1", "key1" ) );

			  // when
			  Future<bool> created = Database.executeAndCommit(db =>
			  {
				Db.createNode( label( "Label1" ) ).setProperty( "key1", "value1" );
				return OtherThread.execute( CreateNode( db, "Label1", "key1", "value2" ) );
			  });

			  // then
			  assertTrue( "Node creation should succeed", created.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPreventConcurrentCreationOfConflictingData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPreventConcurrentCreationOfConflictingData()
		 {
			  // given
			  Database.executeAndCommit( CreateUniquenessConstraint( "Label1", "key1" ) );

			  // when
			  Future<bool> created = Database.executeAndCommit(db =>
			  {
				Db.createNode( label( "Label1" ) ).setProperty( "key1", "value1" );
				try
				{
					 return OtherThread.execute( CreateNode( db, "Label1", "key1", "value1" ) );
				}
				finally
				{
					 assertThat( OtherThread, Waiting );
				}
			  });

			  // then
			  assertFalse( "node creation should fail", created.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowOtherTransactionToCompleteIfFirstTransactionRollsBack() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowOtherTransactionToCompleteIfFirstTransactionRollsBack()
		 {
			  // given
			  Database.executeAndCommit( CreateUniquenessConstraint( "Label1", "key1" ) );

			  // when
			  Future<bool> created = Database.executeAndRollback(db =>
			  {
				Db.createNode( label( "Label1" ) ).setProperty( "key1", "value1" );
				try
				{
					 return OtherThread.execute( CreateNode( db, "Label1", "key1", "value1" ) );
				}
				finally
				{
					 assertThat( OtherThread, Waiting );
				}
			  });

			  // then
			  assertTrue( "Node creation should succeed", created.get() );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static System.Func<org.Neo4Net.graphdb.GraphDatabaseService, Void> createUniquenessConstraint(final String label, final String propertyKey)
		 private static System.Func<GraphDatabaseService, Void> CreateUniquenessConstraint( string label, string propertyKey )
		 {
			  return db =>
			  {
				Db.schema().constraintFor(label(label)).assertPropertyIsUnique(propertyKey).create();
				return null;
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.Neo4Net.test.OtherThreadExecutor.WorkerCommand<Void, bool> createNode(final org.Neo4Net.graphdb.GraphDatabaseService db, final String label, final String propertyKey, final Object propertyValue)
		 public static OtherThreadExecutor.WorkerCommand<Void, bool> CreateNode( IGraphDatabaseService db, string label, string propertyKey, object propertyValue )
		 {
			  return nothing =>
			  {
				try
				{
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.createNode( label( label ) ).setProperty( propertyKey, propertyValue );
   
						 tx.success();
						 return true;
					}
				}
				catch ( ConstraintViolationException )
				{
					 return false;
				}
			  };
		 }
	}

}
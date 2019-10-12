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
namespace Org.Neo4j.Kernel.Impl.Api.integrationtest
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ConstraintViolationException = Org.Neo4j.Graphdb.ConstraintViolationException;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Org.Neo4j.Test;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;
	using Org.Neo4j.Test.rule.concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.concurrent.OtherThreadRule.isWaiting;

	public class UniquenessConstraintValidationConcurrencyIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule database = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule Database = new ImpermanentDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.concurrent.OtherThreadRule<Void> otherThread = new org.neo4j.test.rule.concurrent.OtherThreadRule<>();
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
//ORIGINAL LINE: private static System.Func<org.neo4j.graphdb.GraphDatabaseService, Void> createUniquenessConstraint(final String label, final String propertyKey)
		 private static System.Func<GraphDatabaseService, Void> CreateUniquenessConstraint( string label, string propertyKey )
		 {
			  return db =>
			  {
				Db.schema().constraintFor(label(label)).assertPropertyIsUnique(propertyKey).create();
				return null;
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.neo4j.test.OtherThreadExecutor.WorkerCommand<Void, bool> createNode(final org.neo4j.graphdb.GraphDatabaseService db, final String label, final String propertyKey, final Object propertyValue)
		 public static OtherThreadExecutor.WorkerCommand<Void, bool> CreateNode( GraphDatabaseService db, string label, string propertyKey, object propertyValue )
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
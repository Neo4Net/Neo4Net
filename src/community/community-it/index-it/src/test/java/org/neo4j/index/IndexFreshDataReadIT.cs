using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Index
{
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Result = Neo4Net.GraphDb.Result;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.map;

	public class IndexFreshDataReadIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.EmbeddedDatabaseRule databaseRule = new Neo4Net.test.rule.EmbeddedDatabaseRule();
		 public EmbeddedDatabaseRule DatabaseRule = new EmbeddedDatabaseRule();

		 private ExecutorService _executor = Executors.newCachedThreadPool();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _executor.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readLatestIndexDataAfterUsingExhaustedNodeRelationshipIterator() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadLatestIndexDataAfterUsingExhaustedNodeRelationshipIterator()
		 {
			  using ( Transaction transaction = DatabaseRule.beginTx() )
			  {
					AddStaffMember( "Fry" );
					assertEquals( 1, CountStaff().intValue() );

					Node fry = DatabaseRule.getNodeById( 0 );
					IEnumerable<Relationship> fryRelationships = fry.Relationships;
					assertFalse( fryRelationships.GetEnumerator().hasNext() );

					AddStaffMember( "Lila" );
					assertEquals( 2, CountStaff().intValue() );

					AddStaffMember( "Bender" );
					assertEquals( 3, CountStaff().intValue() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void addStaffMember(String name) throws InterruptedException, java.util.concurrent.ExecutionException
		 private void AddStaffMember( string name )
		 {
			  _executor.submit( new CreateNamedNodeTask( this, name ) ).get();
		 }

		 private Number CountStaff()
		 {
			  using ( Result countResult = DatabaseRule.execute( "MATCH (n:staff) return count(n.name) as count" ) )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					return ( Number ) countResult.ColumnAs( "count" ).next();
			  }
		 }

		 private class CreateNamedNodeTask : ThreadStart
		 {
			 private readonly IndexFreshDataReadIT _outerInstance;

			  internal readonly string Name;

			  internal CreateNamedNodeTask( IndexFreshDataReadIT outerInstance, string name )
			  {
				  this._outerInstance = outerInstance;
					this.Name = name;
			  }

			  public override void Run()
			  {
					using ( Transaction transaction = outerInstance.DatabaseRule.beginTx() )
					{
						 outerInstance.DatabaseRule.execute( "CREATE (n:staff {name:{name}})", map( "name", Name ) );
						 transaction.Success();
					}
			  }
		 }
	}

}
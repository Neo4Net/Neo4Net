using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Org.Neo4j.Kernel
{
	using RandomStringUtils = org.apache.commons.lang3.RandomStringUtils;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using EmbeddedDatabaseRule = Org.Neo4j.Test.rule.EmbeddedDatabaseRule;
	using RepeatRule = Org.Neo4j.Test.rule.RepeatRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asSet;

	/// <summary>
	/// Token creation should be able to handle cases of concurrent token creation
	/// with different/same names. Short random interval (1-3) give a high chances of same token name in this test.
	/// <para>
	/// Newly created token should be visible only when token cache already have both mappings:
	/// "name -> id" and "id -> name" populated.
	/// Otherwise attempt to retrieve labels from newly created node can fail.
	/// </para>
	/// </summary>
	public class TokenCreationIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.EmbeddedDatabaseRule databaseRule = new org.neo4j.test.rule.EmbeddedDatabaseRule();
		 public readonly EmbeddedDatabaseRule DatabaseRule = new EmbeddedDatabaseRule();

		 private volatile bool _stop;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @RepeatRule.Repeat(times = 5) public void concurrentLabelTokenCreation() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConcurrentLabelTokenCreation()
		 {
			  int concurrentWorkers = 10;
			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( concurrentWorkers );
			  for ( int i = 0; i < concurrentWorkers; i++ )
			  {
					( new LabelCreator( this, DatabaseRule, latch ) ).Start();
			  }
			  LockSupport.parkNanos( TimeUnit.MILLISECONDS.toNanos( 500 ) );
			  _stop = true;
			  latch.await();
		 }

		 public virtual Label[] Labels
		 {
			 get
			 {
				  int randomLabelValue = ThreadLocalRandom.current().Next(2) + 1;
				  Label[] labels = new Label[randomLabelValue];
				  for ( int i = 0; i < labels.Length; i++ )
				  {
						labels[i] = Label.label( RandomStringUtils.randomAscii( randomLabelValue ) );
				  }
				  return labels;
			 }
		 }

		 private class LabelCreator : Thread
		 {
			 private readonly TokenCreationIT _outerInstance;

			  internal readonly GraphDatabaseService Database;
			  internal readonly System.Threading.CountdownEvent CreateLatch;

			  internal LabelCreator( TokenCreationIT outerInstance, GraphDatabaseService database, System.Threading.CountdownEvent createLatch )
			  {
				  this._outerInstance = outerInstance;
					this.Database = database;
					this.CreateLatch = createLatch;
			  }

			  public override void Run()
			  {
					try
					{
						 while ( !outerInstance.stop )
						 {

							  try
							  {
									  using ( Transaction transaction = Database.beginTx() )
									  {
										Label[] createdLabels = outerInstance.Labels;
										Node node = Database.createNode( createdLabels );
										IEnumerable<Label> nodeLabels = node.Labels;
										assertEquals( asSet( asList( createdLabels ) ), asSet( nodeLabels ) );
										transaction.Success();
									  }
							  }
							  catch ( Exception e )
							  {
									outerInstance.stop = true;
									throw e;
							  }
						 }
					}
					finally
					{
						 CreateLatch.Signal();
					}
			  }
		 }
	}

}
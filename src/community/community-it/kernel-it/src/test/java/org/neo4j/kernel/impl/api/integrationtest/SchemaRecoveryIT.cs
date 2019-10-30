using System;
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
namespace Neo4Net.Kernel.Impl.Api.integrationtest
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using ConstraintDefinition = Neo4Net.GraphDb.Schema.ConstraintDefinition;
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using Neo4Net.Test.subprocess;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;

	public class SchemaRecoveryIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void schemaTransactionsShouldSurviveRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SchemaTransactionsShouldSurviveRecovery()
		 {
			  // given
			  File storeDir = TestDirectory.absolutePath();
			  Process process = ( new CreateConstraintButDoNotShutDown() ).Start(storeDir);
			  process.WaitForSchemaTransactionCommitted();
			  SubProcess.kill( process );

			  // when
			  IGraphDatabaseService recoveredDatabase = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(storeDir);

			  // then
			  assertEquals( 1, Constraints( recoveredDatabase ).Count );
			  assertEquals( 1, Indexes( recoveredDatabase ).Count );

			  recoveredDatabase.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.TestDirectory testDirectory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private IList<ConstraintDefinition> Constraints( IGraphDatabaseService database )
		 {
			  using ( Transaction ignored = database.BeginTx() )
			  {
					return Iterables.asList( database.Schema().Constraints );
			  }
		 }

		 private IList<IndexDefinition> Indexes( IGraphDatabaseService database )
		 {
			  using ( Transaction ignored = database.BeginTx() )
			  {
					return Iterables.asList( database.Schema().Indexes );
			  }
		 }

		 public interface Process
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void waitForSchemaTransactionCommitted() throws InterruptedException;
			  void WaitForSchemaTransactionCommitted();
		 }

		 [Serializable]
		 internal class CreateConstraintButDoNotShutDown : SubProcess<Process, File>, Process
		 {
			  // Would use a CountDownLatch but fields of this class need to be serializable.
			  internal volatile bool Started;

			  protected internal override void Startup( File storeDir )
			  {
					GraphDatabaseService database = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(storeDir);
					using ( Transaction transaction = database.BeginTx() )
					{
						 database.Schema().constraintFor(label("User")).assertPropertyIsUnique("uuid").create();
						 transaction.Success();
					}
					Started = true;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void waitForSchemaTransactionCommitted() throws InterruptedException
			  public override void WaitForSchemaTransactionCommitted()
			  {
					while ( !Started )
					{
						 Thread.Sleep( 10 );
					}
			  }
		 }
	}

}
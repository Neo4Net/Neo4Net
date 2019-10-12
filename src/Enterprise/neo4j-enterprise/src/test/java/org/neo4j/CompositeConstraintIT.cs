/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ConsistencyCheckService = Neo4Net.Consistency.ConsistencyCheckService;
	using ConsistencyCheckTool = Neo4Net.Consistency.ConsistencyCheckTool;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using EnterpriseGraphDatabaseFactory = Neo4Net.Graphdb.factory.EnterpriseGraphDatabaseFactory;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class CompositeConstraintIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void compositeNodeKeyConstraintUpdate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CompositeNodeKeyConstraintUpdate()
		 {
			  GraphDatabaseService database = ( new EnterpriseGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(TestDirectory.storeDir()).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();

			  Label label = Label.label( "label" );

			  using ( Transaction transaction = database.BeginTx() )
			  {
					Node node = database.CreateNode( label );
					node.SetProperty( "b", ( short ) 3 );
					node.SetProperty( "a", new double[]{ 0.6, 0.4, 0.2 } );
					transaction.Success();
			  }

			  using ( Transaction transaction = database.BeginTx() )
			  {
					string query = format( "CREATE CONSTRAINT ON (n:%s) ASSERT (n.%s,n.%s) IS NODE KEY", label.Name(), "a", "b" );
					database.Execute( query );
					transaction.Success();
			  }

			  AwaitIndex( database );

			  using ( Transaction transaction = database.BeginTx() )
			  {
					Node node = database.CreateNode( label );
					node.SetProperty( "a", ( short ) 7 );
					node.SetProperty( "b", new double[]{ 0.7, 0.5, 0.3 } );
					transaction.Success();
			  }
			  database.Shutdown();

			  ConsistencyCheckService.Result consistencyCheckResult = CheckDbConsistency( TestDirectory.storeDir() );
			  assertTrue( "Database is consistent", consistencyCheckResult.Successful );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.consistency.ConsistencyCheckService.Result checkDbConsistency(java.io.File databaseDirectory) throws org.neo4j.consistency.ConsistencyCheckTool.ToolFailureException
		 private static ConsistencyCheckService.Result CheckDbConsistency( File databaseDirectory )
		 {
			  return ConsistencyCheckTool.runConsistencyCheckTool( new string[]{ databaseDirectory.AbsolutePath }, System.out, System.err );
		 }

		 private static void AwaitIndex( GraphDatabaseService database )
		 {
			  using ( Transaction tx = database.BeginTx() )
			  {
					database.Schema().awaitIndexesOnline(2, TimeUnit.MINUTES);
					tx.Success();
			  }
		 }
	}

}
/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ConsistencyCheckService = Neo4Net.Consistency.ConsistencyCheckService;
	using ConsistencyCheckTool = Neo4Net.Consistency.ConsistencyCheckTool;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using EnterpriseGraphDatabaseFactory = Neo4Net.GraphDb.factory.EnterpriseGraphDatabaseFactory;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class CompositeConstraintIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory testDirectory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.SuppressOutput suppressOutput = org.Neo4Net.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void compositeNodeKeyConstraintUpdate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CompositeNodeKeyConstraintUpdate()
		 {
			  IGraphDatabaseService database = ( new EnterpriseGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(TestDirectory.storeDir()).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();

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
//ORIGINAL LINE: private static org.Neo4Net.consistency.ConsistencyCheckService.Result checkDbConsistency(java.io.File databaseDirectory) throws org.Neo4Net.consistency.ConsistencyCheckTool.ToolFailureException
		 private static ConsistencyCheckService.Result CheckDbConsistency( File databaseDirectory )
		 {
			  return ConsistencyCheckTool.runConsistencyCheckTool( new string[]{ databaseDirectory.AbsolutePath }, System.out, System.err );
		 }

		 private static void AwaitIndex( IGraphDatabaseService database )
		 {
			  using ( Transaction tx = database.BeginTx() )
			  {
					database.Schema().awaitIndexesOnline(2, TimeUnit.MINUTES);
					tx.Success();
			  }
		 }
	}

}
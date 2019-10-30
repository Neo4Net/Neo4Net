using System;

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
namespace Neo4Net.Consistency
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using EnterpriseGraphDatabaseFactory = Neo4Net.GraphDb.factory.EnterpriseGraphDatabaseFactory;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using SchemaKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.SchemaKernelException;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using LabelSchemaDescriptor = Neo4Net.Kernel.api.schema.LabelSchemaDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class HalfCreatedConstraintIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory testDirectory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void uniqueIndexWithoutOwningConstraintIsIgnoredDuringCheck() throws ConsistencyCheckTool.ToolFailureException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UniqueIndexWithoutOwningConstraintIsIgnoredDuringCheck()
		 {
			  File databaseDir = TestDirectory.databaseDir();
			  Label marker = Label.label( "MARKER" );
			  string property = "property";

			  IGraphDatabaseService database = ( new EnterpriseGraphDatabaseFactory() ).newEmbeddedDatabase(databaseDir);
			  try
			  {
					CreateNodes( marker, property, database );
					AddIndex( database );
					WaitForIndexPopulationFailure( database );
			  }
			  catch ( SchemaKernelException e )
			  {
					Console.WriteLine( e.ToString() );
					Console.Write( e.StackTrace );
			  }
			  finally
			  {
					database.Shutdown();
			  }

			  ConsistencyCheckService.Result checkResult = ConsistencyCheckTool.RunConsistencyCheckTool( new string[]{ databaseDir.AbsolutePath }, EmptyPrintStream(), EmptyPrintStream() );
			  assertTrue( string.join( Environment.NewLine, Files.readAllLines( checkResult.ReportFile().toPath() ) ), checkResult.Successful );
		 }

		 private static void WaitForIndexPopulationFailure( IGraphDatabaseService database )
		 {
			  try
			  {
					  using ( Transaction ignored = database.BeginTx() )
					  {
						database.Schema().awaitIndexesOnline(10, TimeUnit.MINUTES);
						fail( "Unique index population should fail." );
					  }
			  }
			  catch ( System.InvalidOperationException )
			  {
					// TODO: Do we really need to verify the message since we know an IllegalStateException was caught? If so, this needs to be updated.
					// assertEquals( "Index entered a FAILED state. Please see database logs.", e.getMessage() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void addIndex(org.Neo4Net.graphdb.GraphDatabaseService database) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.SchemaKernelException
		 private static void AddIndex( IGraphDatabaseService database )
		 {
			  using ( Transaction transaction = database.BeginTx() )
			  {
					DependencyResolver resolver = ( ( GraphDatabaseAPI ) database ).DependencyResolver;
					ThreadToStatementContextBridge statementBridge = resolver.ProvideDependency( typeof( ThreadToStatementContextBridge ) ).get();
					KernelTransaction kernelTransaction = statementBridge.GetKernelTransactionBoundToThisThread( true );
					LabelSchemaDescriptor descriptor = SchemaDescriptorFactory.forLabel( 0, 0 );
					Config config = resolver.ResolveDependency( typeof( Config ) );
					kernelTransaction.IndexUniqueCreate( descriptor, config.Get( GraphDatabaseSettings.default_schema_provider ) );
					transaction.Success();
			  }
		 }

		 private static void CreateNodes( Label marker, string property, IGraphDatabaseService database )
		 {
			  using ( Transaction transaction = database.BeginTx() )
			  {
					for ( int i = 0; i < 10; i++ )
					{
						 Node node = database.CreateNode( marker );
						 node.SetProperty( property, "a" );
					}
					transaction.Success();
			  }
		 }

		 private static PrintStream EmptyPrintStream()
		 {
			  return new PrintStream( Neo4Net.Io.NullOutputStream.NullOutputStreamConflict );
		 }
	}

}
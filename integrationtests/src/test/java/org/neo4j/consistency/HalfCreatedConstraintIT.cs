﻿using System;

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
namespace Org.Neo4j.Consistency
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using EnterpriseGraphDatabaseFactory = Org.Neo4j.Graphdb.factory.EnterpriseGraphDatabaseFactory;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using SchemaKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.SchemaKernelException;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using LabelSchemaDescriptor = Org.Neo4j.Kernel.api.schema.LabelSchemaDescriptor;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class HalfCreatedConstraintIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void uniqueIndexWithoutOwningConstraintIsIgnoredDuringCheck() throws ConsistencyCheckTool.ToolFailureException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UniqueIndexWithoutOwningConstraintIsIgnoredDuringCheck()
		 {
			  File databaseDir = TestDirectory.databaseDir();
			  Label marker = Label.label( "MARKER" );
			  string property = "property";

			  GraphDatabaseService database = ( new EnterpriseGraphDatabaseFactory() ).newEmbeddedDatabase(databaseDir);
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

		 private static void WaitForIndexPopulationFailure( GraphDatabaseService database )
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
//ORIGINAL LINE: private static void addIndex(org.neo4j.graphdb.GraphDatabaseService database) throws org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException
		 private static void AddIndex( GraphDatabaseService database )
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

		 private static void CreateNodes( Label marker, string property, GraphDatabaseService database )
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
			  return new PrintStream( Org.Neo4j.Io.NullOutputStream.NullOutputStreamConflict );
		 }
	}

}
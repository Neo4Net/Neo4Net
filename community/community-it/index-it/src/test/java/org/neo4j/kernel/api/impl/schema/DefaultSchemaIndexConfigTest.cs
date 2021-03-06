﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Api.Impl.Schema
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseBuilder = Org.Neo4j.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using IndexReference = Org.Neo4j.@internal.Kernel.Api.IndexReference;
	using TokenRead = Org.Neo4j.@internal.Kernel.Api.TokenRead;
	using IndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using GenericNativeIndexProvider = Org.Neo4j.Kernel.Impl.Index.Schema.GenericNativeIndexProvider;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using TestLabels = Org.Neo4j.Test.TestLabels;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.default_schema_provider;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class DefaultSchemaIndexConfigTest
	public class DefaultSchemaIndexConfigTest
	{
		 private const string KEY = "key";
		 private const TestLabels LABEL = TestLabels.LABEL_ONE;
		 private static readonly GraphDatabaseBuilder _dbBuilder = new TestGraphDatabaseFactory().newImpermanentDatabaseBuilder();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.List<org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex> providers()
		 public static IList<GraphDatabaseSettings.SchemaIndex> Providers()
		 {
			  IList<GraphDatabaseSettings.SchemaIndex> providers = new List<GraphDatabaseSettings.SchemaIndex>( Arrays.asList( GraphDatabaseSettings.SchemaIndex.values() ) );
			  providers.Add( null ); // <-- to exercise the default option
			  return providers;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex provider;
		 public GraphDatabaseSettings.SchemaIndex Provider;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseConfiguredIndexProvider() throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUseConfiguredIndexProvider()
		 {
			  // given
			  GraphDatabaseService db = _dbBuilder.setConfig( default_schema_provider, Provider == null ? null : Provider.providerName() ).newGraphDatabase();
			  try
			  {
					// when
					CreateIndex( db );

					// then
					AssertIndexProvider( db, Provider == null ? GenericNativeIndexProvider.DESCRIPTOR.name() : Provider.providerName() );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertIndexProvider(org.neo4j.graphdb.GraphDatabaseService db, String expectedProviderIdentifier) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 private void AssertIndexProvider( GraphDatabaseService db, string expectedProviderIdentifier )
		 {
			  GraphDatabaseAPI graphDatabaseAPI = ( GraphDatabaseAPI ) db;
			  using ( Transaction tx = graphDatabaseAPI.BeginTx() )
			  {
					KernelTransaction ktx = graphDatabaseAPI.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) ).getKernelTransactionBoundToThisThread( true );
					TokenRead tokenRead = ktx.TokenRead();
					int labelId = tokenRead.NodeLabel( LABEL.name() );
					int propertyId = tokenRead.PropertyKey( KEY );
					IndexReference index = ktx.SchemaRead().index(labelId, propertyId);

					assertEquals( "expected IndexProvider.Descriptor", expectedProviderIdentifier, ( new IndexProviderDescriptor( index.ProviderKey(), index.ProviderVersion() ) ).name() );
					tx.Success();
			  }
		 }

		 private void CreateIndex( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(LABEL).on(KEY).create();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					tx.Success();
			  }
		 }
	}

}
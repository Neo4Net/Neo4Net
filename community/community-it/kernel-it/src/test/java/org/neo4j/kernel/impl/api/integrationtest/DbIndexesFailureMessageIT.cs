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
namespace Org.Neo4j.Kernel.Impl.Api.integrationtest
{
	using Test = org.junit.Test;


	using Org.Neo4j.Collection;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using MapUtil = Org.Neo4j.Helpers.Collection.MapUtil;
	using Transaction = Org.Neo4j.@internal.Kernel.Api.Transaction;
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using ProcedureCallContext = Org.Neo4j.@internal.Kernel.Api.procs.ProcedureCallContext;
	using LabelSchemaDescriptor = Org.Neo4j.Kernel.api.schema.LabelSchemaDescriptor;
	using FailingGenericNativeIndexProviderFactory = Org.Neo4j.Kernel.Impl.Index.Schema.FailingGenericNativeIndexProviderFactory;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.ProcedureSignature.procedureName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.LoginContext.AUTH_DISABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.FailingGenericNativeIndexProviderFactory.FailureType.POPULATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.TestGraphDatabaseFactory.INDEX_PROVIDERS_FILTER;

	public class DbIndexesFailureMessageIT : KernelIntegrationTest
	{
		 private AtomicBoolean _failNextIndexPopulation = new AtomicBoolean();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void listAllIndexesWithFailedIndex() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ListAllIndexesWithFailedIndex()
		 {
			  // Given
			  Transaction transaction = NewTransaction( AUTH_DISABLED );
			  int failedLabel = transaction.TokenWrite().labelGetOrCreateForName("Fail");
			  int propertyKeyId1 = transaction.TokenWrite().propertyKeyGetOrCreateForName("foo");
			  _failNextIndexPopulation.set( true );
			  LabelSchemaDescriptor descriptor = forLabel( failedLabel, propertyKeyId1 );
			  transaction.SchemaWrite().indexCreate(descriptor);
			  Commit();

			  //let indexes come online
			  try
			  {
					  using ( Org.Neo4j.Graphdb.Transaction ignored = Db.beginTx() )
					  {
						Db.schema().awaitIndexesOnline(2, MINUTES);
						fail( "Expected to fail when awaiting for index to come online" );
					  }
			  }
			  catch ( System.InvalidOperationException )
			  {
					// expected
			  }

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName("db", "indexes")).id(), new object[0], ProcedureCallContext.EMPTY);
			  assertTrue( stream.HasNext() );
			  object[] result = stream.Next();
			  assertFalse( stream.HasNext() );

			  // Then
			  assertEquals( "INDEX ON :Fail(foo)", result[0] );
			  assertEquals( "Unnamed index", result[1] );
			  assertEquals( Collections.singletonList( "Fail" ), result[2] );
			  assertEquals( Collections.singletonList( "foo" ), result[3] );
			  assertEquals( "FAILED", result[4] );
			  assertEquals( "node_label_property", result[5] );
			  assertEquals( 0.0, result[6] );
			  IDictionary<string, string> providerDescriptionMap = MapUtil.stringMap( "key", GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10.providerKey(), "version", GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10.providerVersion() );
			  assertEquals( providerDescriptionMap, result[7] );
			  assertEquals( IndexingService.getIndexId( descriptor ), result[8] );
			  assertThat( ( string ) result[9], containsString( "java.lang.RuntimeException: Fail on update during population" ) );

			  Commit();
		 }

		 protected internal override TestGraphDatabaseFactory CreateGraphDatabaseFactory()
		 {
			  return base.CreateGraphDatabaseFactory().removeKernelExtensions(INDEX_PROVIDERS_FILTER).addKernelExtension(new FailingGenericNativeIndexProviderFactory(POPULATION));
		 }
	}

}
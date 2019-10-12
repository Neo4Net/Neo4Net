using System;

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
namespace Neo4Net.@internal.Kernel.Api
{
	using Test = org.junit.Test;


	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using LabelSchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public abstract class LockingTestBase<G> : KernelAPIWriteTestBase<G> where G : KernelAPIWriteTestSupport
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBlockConstraintCreationOnUnrelatedPropertyWrite() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBlockConstraintCreationOnUnrelatedPropertyWrite()
		 {
			  int nodeProp;
			  int constraintProp;
			  int label;

			  // Given
			  using ( Transaction tx = beginTransaction() )
			  {
					nodeProp = tx.TokenWrite().propertyKeyGetOrCreateForName("nodeProp");
					constraintProp = tx.TokenWrite().propertyKeyGetOrCreateForName("constraintProp");
					label = tx.TokenWrite().labelGetOrCreateForName("label");
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					tx.SchemaWrite().uniquePropertyConstraintCreate(LabelDescriptor(label, constraintProp));
					tx.Success();
			  }

			  System.Threading.CountdownEvent createNodeLatch = new System.Threading.CountdownEvent( 1 );
			  System.Threading.CountdownEvent createConstraintLatch = new System.Threading.CountdownEvent( 1 );

			  // When & Then
			  ExecutorService executor = Executors.newFixedThreadPool( 2 );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> f1 = executor.submit(() ->
			  Future<object> f1 = executor.submit(() =>
			  {
			  try
			  {
				  using ( Transaction tx = beginTransaction() )
				  {
					  CreateNodeWithProperty( tx, nodeProp );
					  createNodeLatch.Signal();
					  assertTrue( createConstraintLatch.await( 5, TimeUnit.MINUTES ) );
					  tx.Success();
				  }
			  }
			  catch ( Exception e )
			  {
				  fail( "Create node failed: " + e );
			  }
			  finally
			  {
				  createNodeLatch.Signal();
			  }
			  });

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> f2 = executor.submit(() ->
			  Future<object> f2 = executor.submit(() =>
			  {
			  try
			  {
				  using ( Transaction tx = beginTransaction() )
				  {
					  assertTrue( createNodeLatch.await( 5, TimeUnit.MINUTES ) );
					  tx.SchemaWrite().uniquePropertyConstraintCreate(LabelDescriptor(label, constraintProp));
					  tx.Success();
				  }
			  }
			  catch ( KernelException e )
			  {
				  assertEquals( Status.Schema.ConstraintAlreadyExists, e.status() );
			  }
			  catch ( InterruptedException )
			  {
				  fail( "Interrupted during create constraint" );
			  }
			  finally
			  {
				  createConstraintLatch.Signal();
			  }
			  });

			  try
			  {
					f1.get();
					f2.get();
			  }
			  finally
			  {
					executor.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createNodeWithProperty(Transaction tx, int propId1) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private void CreateNodeWithProperty( Transaction tx, int propId1 )
		 {
			  long node = tx.DataWrite().nodeCreate();
			  tx.DataWrite().nodeSetProperty(node, propId1, Values.intValue(42));
		 }

		 protected internal abstract LabelSchemaDescriptor LabelDescriptor( int label, params int[] props );
	}

}
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.builtinprocs
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using Neo4Net.Collections;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Procedures = Neo4Net.Kernel.Api.Internal.Procedures;
	using SchemaWrite = Neo4Net.Kernel.Api.Internal.SchemaWrite;
	using Transaction = Neo4Net.Kernel.Api.Internal.Transaction;
	using ProcedureException = Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
	using ProcedureCallContext = Neo4Net.Kernel.Api.Internal.procs.ProcedureCallContext;
	using SchemaDescriptorFactory = Neo4Net.Kernel.Api.schema.SchemaDescriptorFactory;
	using AnonymousContext = Neo4Net.Kernel.Api.security.AnonymousContext;
	using KernelIntegrationTest = Neo4Net.Kernel.Impl.Api.integrationtest.KernelIntegrationTest;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsEqual.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.procs.ProcedureSignature.procedureName;

	public class SchemaProcedureIT : KernelIntegrationTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testEmptyGraph() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestEmptyGraph()
		 {
			  // Given the database is empty

			  // When
			  Procedures procs = procs();
			  RawIterator<object[], ProcedureException> stream = procs.procedureCallRead( procs.ProcedureGet( procedureName( "db", "schema" ) ).id(), new object[0], ProcedureCallContext.EMPTY );

			  // Then
			  assertThat(asList(stream), contains(equalTo(new object[]
			  {
				  new List<>(),
				  new List<>()
			  })));
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLabelIndex() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestLabelIndex()
		 {
			  // Given there is label with index and a constraint
			  Transaction transaction = NewTransaction( AnonymousContext.WriteToken() );
			  long nodeId = transaction.DataWrite().nodeCreate();
			  int labelId = transaction.TokenWrite().labelGetOrCreateForName("Person");
			  transaction.DataWrite().nodeAddLabel(nodeId, labelId);
			  int propertyIdName = transaction.TokenWrite().propertyKeyGetOrCreateForName("name");
			  int propertyIdAge = transaction.TokenWrite().propertyKeyGetOrCreateForName("age");
			  transaction.DataWrite().nodeSetProperty(nodeId, propertyIdName, Values.of("Emil"));
			  Commit();

			  SchemaWrite schemaOps = SchemaWriteInNewTransaction();
			  schemaOps.IndexCreate( SchemaDescriptorFactory.forLabel( labelId, propertyIdName ) );
			  schemaOps.UniquePropertyConstraintCreate( SchemaDescriptorFactory.forLabel( labelId, propertyIdAge ) );
			  Commit();

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName("db", "schema")).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  while ( stream.HasNext() )
			  {
					object[] next = stream.Next();
					assertEquals( 2, next.Length );
					List<Node> nodes = ( List<Node> ) next[0];
					assertEquals( 1, nodes.Count );
					assertThat( nodes[0].Labels, contains( equalTo( Label.label( "Person" ) ) ) );
					assertEquals( "Person", nodes[0].AllProperties["name"] );
					assertEquals( Collections.singletonList( "name" ), nodes[0].AllProperties["indexes"] );
					assertEquals( Collections.singletonList( "CONSTRAINT ON ( person:Person ) ASSERT person.age IS UNIQUE" ), nodes[0].AllProperties["constraints"] );
			  }
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelationShip() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestRelationShip()
		 {
			  // Given there ar
			  Transaction transaction = NewTransaction( AnonymousContext.WriteToken() );
			  long nodeIdPerson = transaction.DataWrite().nodeCreate();
			  int labelIdPerson = transaction.TokenWrite().labelGetOrCreateForName("Person");
			  transaction.DataWrite().nodeAddLabel(nodeIdPerson, labelIdPerson);
			  long nodeIdLocation = transaction.DataWrite().nodeCreate();
			  int labelIdLocation = transaction.TokenWrite().labelGetOrCreateForName("Location");
			  transaction.DataWrite().nodeAddLabel(nodeIdLocation, labelIdLocation);
			  int relationshipTypeId = transaction.TokenWrite().relationshipTypeGetOrCreateForName("LIVES_IN");
			  transaction.DataWrite().relationshipCreate(nodeIdPerson, relationshipTypeId, nodeIdLocation);
			  Commit();

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName("db", "schema")).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  while ( stream.HasNext() )
			  {
					object[] next = stream.Next();
					assertEquals( 2, next.Length );
					LinkedList<Relationship> relationships = ( LinkedList<Relationship> ) next[1];
					assertEquals( 1, relationships.Count );
					assertEquals( "LIVES_IN", relationships.get( 0 ).Type.name() );
					assertThat( relationships.get( 0 ).StartNode.Labels, contains( equalTo( Label.label( "Person" ) ) ) );
					assertThat( relationships.get( 0 ).EndNode.Labels, contains( equalTo( Label.label( "Location" ) ) ) );
			  }
			  Commit();
		 }
	}

}
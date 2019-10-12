using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.builtinprocs
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using Org.Neo4j.Collection;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Procedures = Org.Neo4j.@internal.Kernel.Api.Procedures;
	using SchemaWrite = Org.Neo4j.@internal.Kernel.Api.SchemaWrite;
	using Transaction = Org.Neo4j.@internal.Kernel.Api.Transaction;
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using ProcedureCallContext = Org.Neo4j.@internal.Kernel.Api.procs.ProcedureCallContext;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using AnonymousContext = Org.Neo4j.Kernel.api.security.AnonymousContext;
	using KernelIntegrationTest = Org.Neo4j.Kernel.Impl.Api.integrationtest.KernelIntegrationTest;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsEqual.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.ProcedureSignature.procedureName;

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
			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );
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
			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );
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
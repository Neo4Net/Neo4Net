using System;
using System.Collections.Generic;
using System.Diagnostics;

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
	using Test = org.junit.Test;


	using Neo4Net.Collections;
	using Transaction = Neo4Net.@internal.Kernel.Api.Transaction;
	using ProcedureException = Neo4Net.@internal.Kernel.Api.exceptions.ProcedureException;
	using ProcedureCallContext = Neo4Net.@internal.Kernel.Api.procs.ProcedureCallContext;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;
	using KernelIntegrationTest = Neo4Net.Kernel.Impl.Api.integrationtest.KernelIntegrationTest;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsEqual.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.ProcedureSignature.procedureName;

	public class BuiltInSchemaProceduresIT : KernelIntegrationTest
	{

		 private readonly string[] _nodesProcedureName = new string[] { "db", "schema", "nodeTypeProperties" };
		 private readonly string[] _relsProcedureName = new string[] { "db", "schema", "relTypeProperties" };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testWeirdLabelName() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestWeirdLabelName()
		 {
			  // Given

			  // Node1: (:`This:is_a:label` {color: "red"})

			  CreateNode( Arrays.asList( "`This:is_a:label`" ), Arrays.asList( "color" ), Arrays.asList( Values.stringValue( "red" ) ) );

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName(_nodesProcedureName)).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), containsInAnyOrder( equalTo( NodeEntry( ":``This:is_a:label``", Arrays.asList( "`This:is_a:label`" ), "color", Arrays.asList( "String" ), true ) ) ) );
	//        printStream( stream );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodePropertiesRegardlessOfCreationOrder1() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestNodePropertiesRegardlessOfCreationOrder1()
		 {
			  // Given

			  // Node1: (:A {color: "red", size: "M"})
			  // Node2: (:A {origin: "Kenya"})

			  CreateNode( Arrays.asList( "A" ), Arrays.asList( "color", "size" ), Arrays.asList( Values.stringValue( "red" ), Values.stringValue( "M" ) ) );
			  CreateNode( Arrays.asList( "A" ), Arrays.asList( "origin" ), Arrays.asList( Values.stringValue( "Kenya" ) ) );

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName(_nodesProcedureName)).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), containsInAnyOrder( equalTo( NodeEntry( ":`A`", Arrays.asList( "A" ), "color", Arrays.asList( "String" ), false ) ), equalTo( NodeEntry( ":`A`", Arrays.asList( "A" ), "size", Arrays.asList( "String" ), false ) ), equalTo( NodeEntry( ":`A`", Arrays.asList( "A" ), "origin", Arrays.asList( "String" ), false ) ) ) );
	//        printStream( stream );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodePropertiesRegardlessOfCreationOrder2() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestNodePropertiesRegardlessOfCreationOrder2()
		 {
			  // Given

			  // Node1: (:B {origin: "Kenya"})
			  // Node2 (:B {color: "red", size: "M"})

			  CreateNode( Arrays.asList( "B" ), Arrays.asList( "origin" ), Arrays.asList( Values.stringValue( "Kenya" ) ) );
			  CreateNode( Arrays.asList( "B" ), Arrays.asList( "color", "size" ), Arrays.asList( Values.stringValue( "red" ), Values.stringValue( "M" ) ) );

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName(_nodesProcedureName)).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), containsInAnyOrder( equalTo( NodeEntry( ":`B`", Arrays.asList( "B" ), "color", Arrays.asList( "String" ), false ) ), equalTo( NodeEntry( ":`B`", Arrays.asList( "B" ), "size", Arrays.asList( "String" ), false ) ), equalTo( NodeEntry( ":`B`", Arrays.asList( "B" ), "origin", Arrays.asList( "String" ), false ) ) ) );

	//        printStream( stream );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodePropertiesRegardlessOfCreationOrder3() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestNodePropertiesRegardlessOfCreationOrder3()
		 {
			  // Given

			  // Node1: (:C {color: "red", size: "M"})
			  // Node2: (:C {origin: "Kenya", active: true})

			  CreateNode( Arrays.asList( "C" ), Arrays.asList( "color", "size" ), Arrays.asList( Values.stringValue( "red" ), Values.stringValue( "M" ) ) );
			  CreateNode( Arrays.asList( "C" ), Arrays.asList( "origin", "active" ), Arrays.asList( Values.stringValue( "Kenya" ), Values.booleanValue( true ) ) );

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName(_nodesProcedureName)).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
					 assertThat( asList( stream ), containsInAnyOrder( equalTo( NodeEntry( ":`C`", Arrays.asList( "C" ), "color", Arrays.asList( "String" ), false ) ), equalTo( NodeEntry( ":`C`", Arrays.asList( "C" ), "size", Arrays.asList( "String" ), false ) ), equalTo( NodeEntry( ":`C`", Arrays.asList( "C" ), "origin", Arrays.asList( "String" ), false ) ), equalTo( NodeEntry( ":`C`", Arrays.asList( "C" ), "active", Arrays.asList( "Boolean" ), false ) ) ) );

	//        printStream( stream );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelsPropertiesRegardlessOfCreationOrder1() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestRelsPropertiesRegardlessOfCreationOrder1()
		 {
			  // Given

			  // Node1: (A)
			  // Rel1: (A)-[:R {color: "red", size: "M"}]->(A)
			  // Rel2: (A)-[:R {origin: "Kenya"}]->(A)

			  long emptyNode = CreateEmptyNode();
			  CreateRelationship( emptyNode, "R", emptyNode, Arrays.asList( "color", "size" ), Arrays.asList( Values.stringValue( "red" ), Values.stringValue( "M" ) ) );
			  CreateRelationship( emptyNode, "R", emptyNode, Arrays.asList( "origin" ), Arrays.asList( Values.stringValue( "Kenya" ) ) );

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName(_relsProcedureName)).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), containsInAnyOrder( equalTo( RelEntry( ":`R`", "color", Arrays.asList( "String" ), false ) ), equalTo( RelEntry( ":`R`", "size", Arrays.asList( "String" ), false ) ), equalTo( RelEntry( ":`R`", "origin", Arrays.asList( "String" ), false ) ) ) );
	//        printStream( stream );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelsPropertiesRegardlessOfCreationOrder2() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestRelsPropertiesRegardlessOfCreationOrder2()
		 {
			  // Given

			  // Node1: (A)
			  // Rel1: (A)-[:R {origin: "Kenya"}]->(A)
			  // Rel2: (A)-[:R {color: "red", size: "M"}]->(A)

			  long emptyNode = CreateEmptyNode();
			  CreateRelationship( emptyNode, "R", emptyNode, Arrays.asList( "origin" ), Arrays.asList( Values.stringValue( "Kenya" ) ) );
			  CreateRelationship( emptyNode, "R", emptyNode, Arrays.asList( "color", "size" ), Arrays.asList( Values.stringValue( "red" ), Values.stringValue( "M" ) ) );

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName(_relsProcedureName)).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), containsInAnyOrder( equalTo( RelEntry( ":`R`", "color", Arrays.asList( "String" ), false ) ), equalTo( RelEntry( ":`R`", "size", Arrays.asList( "String" ), false ) ), equalTo( RelEntry( ":`R`", "origin", Arrays.asList( "String" ), false ) ) ) );

	//        printStream( stream );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelsPropertiesRegardlessOfCreationOrder3() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestRelsPropertiesRegardlessOfCreationOrder3()
		 {
			  // Given

			  // Node1: (A)
			  // Rel1: (A)-[:R {color: "red", size: "M"}]->(A)
			  // Rel2: (A)-[:R {origin: "Kenya", active: true}}]->(A)

			  long emptyNode = CreateEmptyNode();
			  CreateRelationship( emptyNode, "R", emptyNode, Arrays.asList( "color", "size" ), Arrays.asList( Values.stringValue( "red" ), Values.stringValue( "M" ) ) );
			  CreateRelationship( emptyNode, "R", emptyNode, Arrays.asList( "origin", "active" ), Arrays.asList( Values.stringValue( "Kenya" ), Values.booleanValue( true ) ) );

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName(_relsProcedureName)).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), containsInAnyOrder( equalTo( RelEntry( ":`R`", "color", Arrays.asList( "String" ), false ) ), equalTo( RelEntry( ":`R`", "size", Arrays.asList( "String" ), false ) ), equalTo( RelEntry( ":`R`", "origin", Arrays.asList( "String" ), false ) ), equalTo( RelEntry( ":`R`", "active", Arrays.asList( "Boolean" ), false ) ) ) );

	//        printStream( stream );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodesShouldNotDependOnOrderOfCreationWithOverlap() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestNodesShouldNotDependOnOrderOfCreationWithOverlap()
		 {
			  // Given

			  // Node1: (:B {type:'B1})
			  // Node2: (:B {type:'B2', size: 5})

			  CreateNode( Arrays.asList( "B" ), Arrays.asList( "type" ), Arrays.asList( Values.stringValue( "B1" ) ) );
			  CreateNode( Arrays.asList( "B" ), Arrays.asList( "type", "size" ), Arrays.asList( Values.stringValue( "B2" ), Values.intValue( 5 ) ) );

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName(_nodesProcedureName)).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), containsInAnyOrder( equalTo( NodeEntry( ":`B`", Arrays.asList( "B" ), "type", Arrays.asList( "String" ), true ) ), equalTo( NodeEntry( ":`B`", Arrays.asList( "B" ), "size", Arrays.asList( "Integer" ), false ) ) ) );

	//         printStream( stream );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodesShouldNotDependOnOrderOfCreationWithOverlap2() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestNodesShouldNotDependOnOrderOfCreationWithOverlap2()
		 {
			  // Given

			  // Node1: (:B {type:'B2', size: 5})
			  // Node2: (:B {type:'B1})

			  CreateNode( Arrays.asList( "B" ), Arrays.asList( "type", "size" ), Arrays.asList( Values.stringValue( "B2" ), Values.intValue( 5 ) ) );
			  CreateNode( Arrays.asList( "B" ), Arrays.asList( "type" ), Arrays.asList( Values.stringValue( "B1" ) ) );

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName(_nodesProcedureName)).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), containsInAnyOrder( equalTo( NodeEntry( ":`B`", Arrays.asList( "B" ), "type", Arrays.asList( "String" ), true ) ), equalTo( NodeEntry( ":`B`", Arrays.asList( "B" ), "size", Arrays.asList( "Integer" ), false ) ) ) );

	//         printStream( stream );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelsShouldNotDependOnOrderOfCreationWithOverlap() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestRelsShouldNotDependOnOrderOfCreationWithOverlap()
		 {
			  // Given

			  // Node1: (n)
			  // Rel1: (n)-[:B {type:'B1}]->(n)
			  // Rel2: (n)-[:B {type:'B2', size: 5}]->(n)

			  long nodeId1 = CreateEmptyNode();
			  CreateRelationship( nodeId1, "B", nodeId1, Arrays.asList( "type" ), Arrays.asList( Values.stringValue( "B1" ) ) );
			  CreateRelationship( nodeId1, "B", nodeId1, Arrays.asList( "type", "size" ), Arrays.asList( Values.stringValue( "B1" ), Values.intValue( 5 ) ) );

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName(_relsProcedureName)).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), containsInAnyOrder( equalTo( RelEntry( ":`B`", "type", Arrays.asList( "String" ), true ) ), equalTo( RelEntry( ":`B`", "size", Arrays.asList( "Integer" ), false ) ) ) );

	//        printStream( stream );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelsShouldNotDependOnOrderOfCreationWithOverlap2() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestRelsShouldNotDependOnOrderOfCreationWithOverlap2()
		 {
			  // Given

			  // Node1: (n)
			  // Rel1: (n)-[:B {type:'B2', size: 5}]->(n)
			  // Rel2: (n)-[:B {type:'B1}]->(n)

			  long nodeId1 = CreateEmptyNode();
			  CreateRelationship( nodeId1, "B", nodeId1, Arrays.asList( "type", "size" ), Arrays.asList( Values.stringValue( "B1" ), Values.intValue( 5 ) ) );
			  CreateRelationship( nodeId1, "B", nodeId1, Arrays.asList( "type" ), Arrays.asList( Values.stringValue( "B1" ) ) );

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName(_relsProcedureName)).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), containsInAnyOrder( equalTo( RelEntry( ":`B`", "type", Arrays.asList( "String" ), true ) ), equalTo( RelEntry( ":`B`", "size", Arrays.asList( "Integer" ), false ) ) ) );

	//        printStream( stream );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testWithAllDifferentNodes() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestWithAllDifferentNodes()
		 {
			  // Given

			  // Node1: (:A:B {prop1:"Test", prop2:12})
			  // Node2: (:B {prop1:true})
			  // Node3: ()
			  // Node4: (:C {prop1: ["Test","Success"]}

			  CreateNode( Arrays.asList( "A", "B" ), Arrays.asList( "prop1", "prop2" ), Arrays.asList( Values.stringValue( "Test" ), Values.intValue( 12 ) ) );
			  CreateNode( Arrays.asList( "B" ), Arrays.asList( "prop1" ), Arrays.asList( Values.booleanValue( true ) ) );
			  CreateEmptyNode();
			  CreateNode( Arrays.asList( "C" ), Arrays.asList( "prop1" ), Arrays.asList( Values.stringArray( "Test", "Success" ) ) );

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName(_nodesProcedureName)).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), containsInAnyOrder( equalTo( NodeEntry( ":`A`:`B`", Arrays.asList( "A", "B" ), "prop1", Arrays.asList( "String" ), true ) ), equalTo( NodeEntry( ":`A`:`B`", Arrays.asList( "A", "B" ), "prop2", Arrays.asList( "Integer" ), true ) ), equalTo( NodeEntry( ":`B`", Arrays.asList( "B" ), "prop1", Arrays.asList( "Boolean" ), true ) ), equalTo( NodeEntry( ":`C`", Arrays.asList( "C" ), "prop1", Arrays.asList( "StringArray" ), true ) ), equalTo( NodeEntry( "", Arrays.asList(), null, null, false ) ) ) );

			  // printStream( stream );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testWithSimilarNodes() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestWithSimilarNodes()
		 {
			  // Given

			  // Node1: (:A {prop1:"Test"})
			  // Node2: (:A {prop1:"Test2"})

			  CreateNode( Arrays.asList( "A" ), Arrays.asList( "prop1" ), Arrays.asList( Values.stringValue( "Test" ) ) );
			  CreateNode( Arrays.asList( "A" ), Arrays.asList( "prop1" ), Arrays.asList( Values.stringValue( "Test2" ) ) );

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName(_nodesProcedureName)).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), contains( equalTo( NodeEntry( ":`A`", Arrays.asList( "A" ), "prop1", Arrays.asList( "String" ), true ) ) ) );

			  // printStream( stream );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testWithSimilarNodesHavingDifferentPropertyValueTypes() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestWithSimilarNodesHavingDifferentPropertyValueTypes()
		 {
			  // Given

			  // Node1: ({prop1:"Test", prop2: 12, prop3: true})
			  // Node2: ({prop1:"Test", prop2: 1.5, prop3: "Test"})
			  // Node3: ({prop1:"Test"})

			  CreateNode( Arrays.asList(), Arrays.asList("prop1", "prop2", "prop3"), Arrays.asList(Values.stringValue("Test"), Values.intValue(12), Values.booleanValue(true)) );
			  CreateNode( Arrays.asList(), Arrays.asList("prop1", "prop2", "prop3"), Arrays.asList(Values.stringValue("Test"), Values.floatValue(1.5f), Values.stringValue("Test")) );
			  CreateNode( Arrays.asList(), Arrays.asList("prop1"), Arrays.asList(Values.stringValue("Test")) );

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName(_nodesProcedureName)).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), containsInAnyOrder( equalTo( NodeEntry( "", Arrays.asList(), "prop1", Arrays.asList("String"), true ) ), equalTo(NodeEntry("", Arrays.asList(), "prop2", Arrays.asList("Integer", "Float"), false)), equalTo(NodeEntry("", Arrays.asList(), "prop3", Arrays.asList("String", "Boolean"), false)) ) );

			  // printStream( stream );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testWithSimilarNodesShouldNotDependOnOrderOfCreation() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestWithSimilarNodesShouldNotDependOnOrderOfCreation()
		 {
			  // Given

			  // Node1: ()
			  // Node2: ({prop1:"Test", prop2: 12, prop3: true})
			  // Node3: ({prop1:"Test", prop2: 1.5, prop3: "Test"})

			  CreateEmptyNode();
			  CreateNode( Arrays.asList(), Arrays.asList("prop1", "prop2", "prop3"), Arrays.asList(Values.stringValue("Test"), Values.intValue(12), Values.booleanValue(true)) );
			  CreateNode( Arrays.asList(), Arrays.asList("prop1", "prop2", "prop3"), Arrays.asList(Values.stringValue("Test"), Values.floatValue(1.5f), Values.stringValue("Test")) );

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName(_nodesProcedureName)).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), containsInAnyOrder( equalTo( NodeEntry( "", Arrays.asList(), "prop1", Arrays.asList("String"), false ) ), equalTo(NodeEntry("", Arrays.asList(), "prop2", Arrays.asList("Integer", "Float"), false)), equalTo(NodeEntry("", Arrays.asList(), "prop3", Arrays.asList("String", "Boolean"), false)) ) );

			  // printStream( stream );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testWithAllDifferentRelationships() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestWithAllDifferentRelationships()
		 {
			  // Given

			  // Node1: ()
			  // Rel1: (Node1)-[:R{prop1:"Test", prop2:12}]->(Node1)
			  // Rel2: (Node1)-[:X{prop1:true}]->(Node1)
			  // Rel3: (Node1)-[:Z{}]->(Node1)

			  long nodeId1 = CreateEmptyNode();
			  CreateRelationship( nodeId1, "R", nodeId1, Arrays.asList( "prop1", "prop2" ), Arrays.asList( Values.stringValue( "Test" ), Values.intValue( 12 ) ) );
			  CreateRelationship( nodeId1, "X", nodeId1, Arrays.asList( "prop1" ), Arrays.asList( Values.booleanValue( true ) ) );
			  CreateRelationship( nodeId1, "Z", nodeId1, Arrays.asList(), Arrays.asList() );

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName(_relsProcedureName)).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), containsInAnyOrder( equalTo( RelEntry( ":`R`", "prop1", Arrays.asList( "String" ), true ) ), equalTo( RelEntry( ":`R`", "prop2", Arrays.asList( "Integer" ), true ) ), equalTo( RelEntry( ":`X`", "prop1", Arrays.asList( "Boolean" ), true ) ), equalTo( RelEntry( ":`Z`", null, null, false ) ) ) );

			  // printStream( stream );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testWithSimilarRelationships() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestWithSimilarRelationships()
		 {
			  // Given

			  // Node1: ()
			  // Rel1: (node1)-[:R{prop1:"Test"}]->(node1)
			  // Rel2: (node1)-[:R{prop1:"Test2"}]->(node1)

			  long nodeId1 = CreateEmptyNode();
			  CreateRelationship( nodeId1, "R", nodeId1, Arrays.asList( "prop1" ), Arrays.asList( Values.stringValue( "Test" ) ) );
			  CreateRelationship( nodeId1, "R", nodeId1, Arrays.asList( "prop1" ), Arrays.asList( Values.stringValue( "Test2" ) ) );

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName(_relsProcedureName)).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), containsInAnyOrder( equalTo( RelEntry( ":`R`", "prop1", Arrays.asList( "String" ), true ) ) ) );

			  //printStream( stream );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSchemaWithRelationshipWithoutProperties() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestSchemaWithRelationshipWithoutProperties()
		 {
			  // Given

			  // Node1: ()
			  // Rel1: (node1)-[:R{prop1:"Test", prop2: 12, prop3: true}]->(node1)
			  // Rel2: (node1)-[:R]->(node1)

			  long nodeId1 = CreateEmptyNode();
			  CreateRelationship( nodeId1, "R", nodeId1, Arrays.asList( "prop1", "prop2", "prop3" ), Arrays.asList( Values.stringValue( "Test" ), Values.intValue( 12 ), Values.booleanValue( true ) ) );
			  CreateRelationship( nodeId1, "R", nodeId1, Arrays.asList(), Arrays.asList() );

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName(_relsProcedureName)).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), containsInAnyOrder( equalTo( RelEntry( ":`R`", "prop1", Arrays.asList( "String" ), false ) ), equalTo( RelEntry( ":`R`", "prop2", Arrays.asList( "Integer" ), false ) ), equalTo( RelEntry( ":`R`", "prop3", Arrays.asList( "Boolean" ), false ) ) ) );

			  //printStream( stream );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testWithSimilarRelationshipsHavingDifferentPropertyValueTypes() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestWithSimilarRelationshipsHavingDifferentPropertyValueTypes()
		 {
			  // Given

			  // Node1: ()
			  // Rel1: (node1)-[:R{prop1:"Test", prop2: 12, prop3: true}]->(node1)
			  // Rel2: (node1)-[:R{prop1:"Test", prop2: 1.5, prop3: "Test"}]->(node1)
			  // Rel3: (node1)-[:R{prop1:"Test"}]->(node1)

			  long nodeId1 = CreateEmptyNode();
			  CreateRelationship( nodeId1, "R", nodeId1, Arrays.asList( "prop1", "prop2", "prop3" ), Arrays.asList( Values.stringValue( "Test" ), Values.intValue( 12 ), Values.booleanValue( true ) ) );
			  CreateRelationship( nodeId1, "R", nodeId1, Arrays.asList( "prop1", "prop2", "prop3" ), Arrays.asList( Values.stringValue( "Test" ), Values.floatValue( 1.5f ), Values.stringValue( "Test" ) ) );
			  CreateRelationship( nodeId1, "R", nodeId1, Arrays.asList( "prop1" ), Arrays.asList( Values.stringValue( "Test" ) ) );

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName(_relsProcedureName)).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), containsInAnyOrder( equalTo( RelEntry( ":`R`", "prop1", Arrays.asList( "String" ), true ) ), equalTo( RelEntry( ":`R`", "prop2", Arrays.asList( "Integer", "Float" ), false ) ), equalTo( RelEntry( ":`R`", "prop3", Arrays.asList( "String", "Boolean" ), false ) ) ) );

			  //printStream( stream );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testWithSimilarRelationshipsShouldNotDependOnOrderOfCreation() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestWithSimilarRelationshipsShouldNotDependOnOrderOfCreation()
		 {
			  // This is basically the same as the test before but the empty rel is created first
			  // Given

			  // Node1: ()
			  // Rel1: (node1)-[:R]->(node1)
			  // Rel2: (node1)-[:R{prop1:"Test", prop2: 12, prop3: true}]->(node1)
			  // Rel3: (node1)-[:R{prop1:"Test", prop2: 1.5, prop3: "Test"}]->(node1)

			  long nodeId1 = CreateEmptyNode();
			  CreateRelationship( nodeId1, "R", nodeId1, Arrays.asList(), Arrays.asList() );
			  CreateRelationship( nodeId1, "R", nodeId1, Arrays.asList( "prop1", "prop2", "prop3" ), Arrays.asList( Values.stringValue( "Test" ), Values.intValue( 12 ), Values.booleanValue( true ) ) );
			  CreateRelationship( nodeId1, "R", nodeId1, Arrays.asList( "prop1", "prop2", "prop3" ), Arrays.asList( Values.stringValue( "Test" ), Values.floatValue( 1.5f ), Values.stringValue( "Test" ) ) );

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName(_relsProcedureName)).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), containsInAnyOrder( equalTo( RelEntry( ":`R`", "prop1", Arrays.asList( "String" ), false ) ), equalTo( RelEntry( ":`R`", "prop2", Arrays.asList( "Integer", "Float" ), false ) ), equalTo( RelEntry( ":`R`", "prop3", Arrays.asList( "String", "Boolean" ), false ) ) ) );

			  //printStream( stream );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testWithNullableProperties() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestWithNullableProperties()
		 {
			  // Given

			  // Node1: (:A{prop1:"Test", prop2: 12, prop3: true})
			  // Node2: (:A{prop1:"Test2", prop3: false})
			  // Node3: (:A{prop1:"Test3", prop2: 42})
			  // Node4: (:B{prop1:"Test4", prop2: 21})
			  // Node5: (:B)

			  CreateNode( Arrays.asList( "A" ), Arrays.asList( "prop1", "prop2", "prop3" ), Arrays.asList( Values.stringValue( "Test" ), Values.intValue( 12 ), Values.booleanValue( true ) ) );
			  CreateNode( Arrays.asList( "A" ), Arrays.asList( "prop1", "prop3" ), Arrays.asList( Values.stringValue( "Test2" ), Values.booleanValue( false ) ) );
			  CreateNode( Arrays.asList( "A" ), Arrays.asList( "prop1", "prop2" ), Arrays.asList( Values.stringValue( "Test3" ), Values.intValue( 42 ) ) );
			  CreateNode( Arrays.asList( "B" ), Arrays.asList( "prop1", "prop2" ), Arrays.asList( Values.stringValue( "Test4" ), Values.intValue( 21 ) ) );
			  CreateNode( Arrays.asList( "B" ), Arrays.asList(), Arrays.asList() );

			  // When
			  RawIterator<object[], ProcedureException> stream = Procs().procedureCallRead(Procs().procedureGet(procedureName(_nodesProcedureName)).id(), new object[0], ProcedureCallContext.EMPTY);

			  // Then
			  assertThat( asList( stream ), containsInAnyOrder( equalTo( NodeEntry( ":`A`", Arrays.asList( "A" ), "prop1", Arrays.asList( "String" ), true ) ), equalTo( NodeEntry( ":`A`", Arrays.asList( "A" ), "prop2", Arrays.asList( "Integer" ), false ) ), equalTo( NodeEntry( ":`A`", Arrays.asList( "A" ), "prop3", Arrays.asList( "Boolean" ), false ) ), equalTo( NodeEntry( ":`B`", Arrays.asList( "B" ), "prop1", Arrays.asList( "String" ), false ) ), equalTo( NodeEntry( ":`B`", Arrays.asList( "B" ), "prop2", Arrays.asList( "Integer" ), false ) ) ) );

			  //printStream( stream );
		 }

		 private object[] NodeEntry( string escapedLabels, IList<string> labels, string propertyName, IList<string> propertyValueTypes, bool? mandatory )
		 {
			  return new object[]{ escapedLabels, labels, propertyName, propertyValueTypes, mandatory };
		 }

		 private object[] RelEntry( string labelsOrRelType, string propertyName, IList<string> propertyValueTypes, bool? mandatory )
		 {
			  return new object[]{ labelsOrRelType, propertyName, propertyValueTypes, mandatory };
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long createEmptyNode() throws Throwable
		 private long CreateEmptyNode()
		 {
			  return CreateNode( Arrays.asList(), Arrays.asList(), Arrays.asList() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long createNode(java.util.List<String> labels, java.util.List<String> propKeys, java.util.List<org.neo4j.values.storable.Value> propValues) throws Throwable
		 private long CreateNode( IList<string> labels, IList<string> propKeys, IList<Value> propValues )
		 {
			  Debug.Assert( labels != null );
			  Debug.Assert( propKeys.Count == propValues.Count );

			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );
			  long nodeId = transaction.DataWrite().nodeCreate();

			  foreach ( string labelname in labels )
			  {
					int labelId = transaction.TokenWrite().labelGetOrCreateForName(labelname);
					transaction.DataWrite().nodeAddLabel(nodeId, labelId);
			  }

			  for ( int i = 0; i < propKeys.Count; i++ )
			  {
					string propKeyName = propKeys[i];
					Value propValue = propValues[i];
					int propKeyId = transaction.TokenWrite().propertyKeyGetOrCreateForName(propKeyName);
					transaction.DataWrite().nodeSetProperty(nodeId, propKeyId, propValue);
			  }
			  Commit();
			  return nodeId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createRelationship(long startNode, String type, long endNode, java.util.List<String> propKeys, java.util.List<org.neo4j.values.storable.Value> propValues) throws Throwable
		 private void CreateRelationship( long startNode, string type, long endNode, IList<string> propKeys, IList<Value> propValues )
		 {
			  Debug.Assert( !string.ReferenceEquals( type, null ) && !type.Equals( "" ) );
			  Debug.Assert( propKeys.Count == propValues.Count );

			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );

			  int typeId = transaction.TokenWrite().relationshipTypeGetOrCreateForName(type);
			  long relId = transaction.DataWrite().relationshipCreate(startNode, typeId, endNode);

			  for ( int i = 0; i < propKeys.Count; i++ )
			  {
					string propKeyName = propKeys[i];
					Value propValue = propValues[i];
					int propKeyId = transaction.TokenWrite().propertyKeyGetOrCreateForName(propKeyName);
					transaction.DataWrite().relationshipSetProperty(relId, propKeyId, propValue);
			  }
			  Commit();
		 }

		 /*
		   This method can be used to print to result stream to System.out -> Useful for debugging
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private void printStream(org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> stream) throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private void PrintStream( RawIterator<object[], ProcedureException> stream )
		 {
			  IEnumerator<object[]> iterator = asList( stream ).GetEnumerator();
			  while ( iterator.MoveNext() )
			  {
					object[] row = iterator.Current;
					foreach ( object column in row )
					{
						 Console.WriteLine( column );
					}
					Console.WriteLine();
			  }
		 }
	}

}
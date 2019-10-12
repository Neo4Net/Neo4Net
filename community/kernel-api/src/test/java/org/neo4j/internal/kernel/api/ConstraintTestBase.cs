using System.Collections.Generic;
using System.Diagnostics;

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
namespace Org.Neo4j.@internal.Kernel.Api
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using ConstraintDefinition = Org.Neo4j.Graphdb.schema.ConstraintDefinition;
	using ConstraintValidationException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.ConstraintValidationException;
	using LabelSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using ConstraintDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.intValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("Duplicates") public abstract class ConstraintTestBase<G extends KernelAPIWriteTestSupport> extends KernelAPIWriteTestBase<G>
	public abstract class ConstraintTestBase<G> : KernelAPIWriteTestBase<G> where G : KernelAPIWriteTestSupport
	{
		 protected internal abstract LabelSchemaDescriptor LabelSchemaDescriptor( int labelId, params int[] propertyIds );

		 protected internal abstract ConstraintDescriptor UniqueConstraintDescriptor( int labelId, params int[] propertyIds );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  using ( Org.Neo4j.Graphdb.Transaction tx = graphDb.beginTx() )
			  {
					foreach ( ConstraintDefinition definition in graphDb.schema().Constraints )
					{
						 definition.Drop();
					}
					tx.Success();
			  }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindConstraintsBySchema() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindConstraintsBySchema()
		 {
			  // GIVEN
			  AddConstraints( "FOO", "prop" );

			  using ( Transaction tx = beginTransaction() )
			  {
					int label = tx.TokenWrite().labelGetOrCreateForName("FOO");
					int prop = tx.TokenWrite().propertyKeyGetOrCreateForName("prop");
					LabelSchemaDescriptor descriptor = LabelSchemaDescriptor( label, prop );

					//WHEN
					IList<ConstraintDescriptor> constraints = new IList<ConstraintDescriptor> { tx.SchemaRead().constraintsGetForSchema(descriptor) };

					// THEN
					assertThat( constraints, hasSize( 1 ) );
					assertThat( constraints[0].Schema().PropertyId, equalTo(prop) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindConstraintsByLabel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindConstraintsByLabel()
		 {
			  // GIVEN
			  AddConstraints( "FOO", "prop1", "FOO", "prop2" );

			  using ( Transaction tx = beginTransaction() )
			  {
					int label = tx.TokenWrite().labelGetOrCreateForName("FOO");

					//WHEN
					IList<ConstraintDescriptor> constraints = new IList<ConstraintDescriptor> { tx.SchemaRead().constraintsGetForLabel(label) };

					// THEN
					assertThat( constraints, hasSize( 2 ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleCheckExistenceOfConstraints() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleCheckExistenceOfConstraints()
		 {
			  // GIVEN
			  using ( Org.Neo4j.Graphdb.Transaction tx = graphDb.beginTx() )
			  {

					graphDb.schema().constraintFor(label("FOO")).assertPropertyIsUnique("prop1").create();
					ConstraintDefinition dropped = graphDb.schema().constraintFor(label("FOO")).assertPropertyIsUnique("prop2").create();
					dropped.Drop();
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					int label = tx.tokenWrite().labelGetOrCreateForName("FOO");
					int prop1 = tx.tokenWrite().propertyKeyGetOrCreateForName("prop1");
					int prop2 = tx.tokenWrite().propertyKeyGetOrCreateForName("prop2");

					// THEN
					assertTrue( tx.schemaRead().constraintExists(UniqueConstraintDescriptor(label, prop1)) );
					assertFalse( tx.schemaRead().constraintExists(UniqueConstraintDescriptor(label, prop2)) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindAllConstraints() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindAllConstraints()
		 {
			  // GIVEN
			  AddConstraints( "FOO", "prop1", "BAR", "prop2", "BAZ", "prop3" );

			  using ( Transaction tx = beginTransaction() )
			  {
					//WHEN
					IList<ConstraintDescriptor> constraints = new IList<ConstraintDescriptor> { tx.SchemaRead().constraintsGetAll() };

					// THEN
					assertThat( constraints, hasSize( 3 ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCheckUniquenessWhenAddingLabel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCheckUniquenessWhenAddingLabel()
		 {
			  // GIVEN
			  long nodeConflicting, nodeNotConflicting;
			  AddConstraints( "FOO", "prop" );
			  using ( Org.Neo4j.Graphdb.Transaction tx = graphDb.beginTx() )
			  {
					Node conflict = graphDb.createNode();
					conflict.SetProperty( "prop", 1337 );
					nodeConflicting = conflict.Id;

					Node ok = graphDb.createNode();
					ok.SetProperty( "prop", 42 );
					nodeNotConflicting = ok.Id;

					//Existing node
					Node existing = graphDb.createNode();
					existing.AddLabel( Label.label( "FOO" ) );
					existing.SetProperty( "prop", 1337 );
					tx.Success();
			  }

			  int label;
			  using ( Transaction tx = beginTransaction() )
			  {
					label = tx.tokenWrite().labelGetOrCreateForName("FOO");

					//This is ok, since it will satisfy constraint
					assertTrue( tx.dataWrite().nodeAddLabel(nodeNotConflicting, label) );

					try
					{
						 tx.dataWrite().nodeAddLabel(nodeConflicting, label);
						 fail();
					}
					catch ( ConstraintValidationException )
					{
						 //ignore
					}
					tx.Success();
			  }

			  //Verify
			  using ( Transaction tx = beginTransaction(), NodeCursor nodeCursor = tx.cursors().allocateNodeCursor() )
			  {
					//Node without conflict
					tx.dataRead().singleNode(nodeNotConflicting, nodeCursor);
					assertTrue( nodeCursor.Next() );
					assertTrue( nodeCursor.Labels().contains(label) );
					//Node with conflict
					tx.dataRead().singleNode(nodeConflicting, nodeCursor);
					assertTrue( nodeCursor.Next() );
					assertFalse( nodeCursor.Labels().contains(label) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCheckUniquenessWhenAddingProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCheckUniquenessWhenAddingProperties()
		 {
			  // GIVEN
			  long nodeConflicting, nodeNotConflicting;
			  AddConstraints( "FOO", "prop" );
			  using ( Org.Neo4j.Graphdb.Transaction tx = graphDb.beginTx() )
			  {
					Node conflict = graphDb.createNode();
					conflict.AddLabel( Label.label( "FOO" ) );
					nodeConflicting = conflict.Id;

					Node ok = graphDb.createNode();
					ok.AddLabel( Label.label( "BAR" ) );
					nodeNotConflicting = ok.Id;

					//Existing node
					Node existing = graphDb.createNode();
					existing.AddLabel( Label.label( "FOO" ) );
					existing.SetProperty( "prop", 1337 );
					tx.Success();
			  }

			  int property;
			  using ( Transaction tx = beginTransaction() )
			  {
					property = tx.tokenWrite().propertyKeyGetOrCreateForName("prop");

					//This is ok, since it will satisfy constraint
					tx.dataWrite().nodeSetProperty(nodeNotConflicting, property, intValue(1337));

					try
					{
						 tx.dataWrite().nodeSetProperty(nodeConflicting, property, intValue(1337));
						 fail();
					}
					catch ( ConstraintValidationException )
					{
						 //ignore
					}
					tx.Success();
			  }

			  //Verify
			  using ( Transaction tx = beginTransaction(), NodeCursor nodeCursor = tx.cursors().allocateNodeCursor(), PropertyCursor propertyCursor = tx.cursors().allocatePropertyCursor() )
			  {
					//Node without conflict
					tx.dataRead().singleNode(nodeNotConflicting, nodeCursor);
					assertTrue( nodeCursor.Next() );
					nodeCursor.Properties( propertyCursor );
					assertTrue( HasKey( propertyCursor, property ) );
					//Node with conflict
					tx.dataRead().singleNode(nodeConflicting, nodeCursor);
					assertTrue( nodeCursor.Next() );
					nodeCursor.Properties( propertyCursor );
					assertFalse( HasKey( propertyCursor, property ) );
			  }
		 }

		 private bool HasKey( PropertyCursor propertyCursor, int key )
		 {
			  while ( propertyCursor.Next() )
			  {
					if ( propertyCursor.PropertyKey() == key )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 private void AddConstraints( params string[] labelProps )
		 {
			  Debug.Assert( labelProps.Length % 2 == 0 );

			  using ( Org.Neo4j.Graphdb.Transaction tx = graphDb.beginTx() )
			  {
					for ( int i = 0; i < labelProps.Length; i += 2 )
					{
						 graphDb.schema().constraintFor(label(labelProps[i])).assertPropertyIsUnique(labelProps[i + 1]).create();
					}
					tx.Success();
			  }
		 }
	}

}
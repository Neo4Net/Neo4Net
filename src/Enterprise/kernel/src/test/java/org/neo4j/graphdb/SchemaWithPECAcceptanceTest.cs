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
namespace Neo4Net.Graphdb
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using ConstraintDefinition = Neo4Net.Graphdb.schema.ConstraintDefinition;
	using IndexDefinition = Neo4Net.Graphdb.schema.IndexDefinition;
	using IndexDefinitionImpl = Neo4Net.Kernel.impl.coreapi.schema.IndexDefinitionImpl;
	using InternalSchemaActions = Neo4Net.Kernel.impl.coreapi.schema.InternalSchemaActions;
	using NodeKeyConstraintDefinition = Neo4Net.Kernel.impl.coreapi.schema.NodeKeyConstraintDefinition;
	using NodePropertyExistenceConstraintDefinition = Neo4Net.Kernel.impl.coreapi.schema.NodePropertyExistenceConstraintDefinition;
	using RelationshipPropertyExistenceConstraintDefinition = Neo4Net.Kernel.impl.coreapi.schema.RelationshipPropertyExistenceConstraintDefinition;
	using UniquenessConstraintDefinition = Neo4Net.Kernel.impl.coreapi.schema.UniquenessConstraintDefinition;
	using EnterpriseDatabaseRule = Neo4Net.Test.rule.EnterpriseDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.containsOnly;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.getConstraints;

	public class SchemaWithPECAcceptanceTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.EnterpriseDatabaseRule dbRule = new org.neo4j.test.rule.EnterpriseDatabaseRule();
		 public EnterpriseDatabaseRule DbRule = new EnterpriseDatabaseRule();

		 private GraphDatabaseService _db;
		 private Label _label = Labels.MyLabel;
		 private Label _label2 = Labels.MyOtherLabel;
		 private string _propertyKey = "my_property_key";
		 private string _propertyKey2 = "my_other_property";

		 private enum Labels
		 {
			  MyLabel,
			  MyOtherLabel
		 }

		 private enum Types
		 {
			  MyType,
			  MyOtherType
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void init()
		 public virtual void Init()
		 {
			  _db = DbRule.GraphDatabaseAPI;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateNodePropertyExistenceConstraint()
		 public virtual void ShouldCreateNodePropertyExistenceConstraint()
		 {
			  // When
			  ConstraintDefinition constraint = CreateNodePropertyExistenceConstraint( _label, _propertyKey );

			  // Then
			  assertThat( getConstraints( _db ), containsOnly( constraint ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateRelationshipPropertyExistenceConstraint()
		 public virtual void ShouldCreateRelationshipPropertyExistenceConstraint()
		 {
			  // When
			  ConstraintDefinition constraint = CreateRelationshipPropertyExistenceConstraint( Types.MyType, _propertyKey );

			  // Then
			  assertThat( getConstraints( _db ), containsOnly( constraint ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAddedConstraintsByLabel()
		 public virtual void ShouldListAddedConstraintsByLabel()
		 {
			  // GIVEN
			  ConstraintDefinition constraint1 = CreateUniquenessConstraint( _label, _propertyKey );
			  ConstraintDefinition constraint2 = CreateNodePropertyExistenceConstraint( _label, _propertyKey );
			  ConstraintDefinition constraint3 = CreateNodeKeyConstraint( _label, _propertyKey2 );
			  CreateNodeKeyConstraint( _label2, _propertyKey2 );
			  CreateNodePropertyExistenceConstraint( Labels.MyOtherLabel, _propertyKey );

			  // WHEN THEN
			  assertThat( getConstraints( _db, _label ), containsOnly( constraint1, constraint2, constraint3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAddedConstraintsByRelationshipType()
		 public virtual void ShouldListAddedConstraintsByRelationshipType()
		 {
			  // GIVEN
			  ConstraintDefinition constraint1 = CreateRelationshipPropertyExistenceConstraint( Types.MyType, _propertyKey );
			  CreateRelationshipPropertyExistenceConstraint( Types.MyOtherType, _propertyKey );

			  // WHEN THEN
			  assertThat( getConstraints( _db, Types.MyType ), containsOnly( constraint1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAddedConstraints()
		 public virtual void ShouldListAddedConstraints()
		 {
			  // GIVEN
			  ConstraintDefinition constraint1 = CreateUniquenessConstraint( _label, _propertyKey );
			  ConstraintDefinition constraint2 = CreateNodePropertyExistenceConstraint( _label, _propertyKey );
			  ConstraintDefinition constraint3 = CreateRelationshipPropertyExistenceConstraint( Types.MyType, _propertyKey );
			  ConstraintDefinition constraint4 = CreateNodeKeyConstraint( _label, _propertyKey2 );

			  // WHEN THEN
			  assertThat( getConstraints( _db ), containsOnly( constraint1, constraint2, constraint3, constraint4 ) );
		 }

		 private ConstraintDefinition CreateUniquenessConstraint( Label label, string propertyKey )
		 {
			  SchemaHelper.createUniquenessConstraint( _db, label, propertyKey );
			  SchemaHelper.awaitIndexes( _db );
			  InternalSchemaActions actions = mock( typeof( InternalSchemaActions ) );
			  IndexDefinition index = new IndexDefinitionImpl( actions, null, new Label[]{ label }, new string[]{ propertyKey }, true );
			  return new UniquenessConstraintDefinition( actions, index );
		 }

		 private ConstraintDefinition CreateNodeKeyConstraint( Label label, string propertyKey )
		 {
			  SchemaHelper.createNodeKeyConstraint( _db, label, propertyKey );
			  SchemaHelper.awaitIndexes( _db );
			  InternalSchemaActions actions = mock( typeof( InternalSchemaActions ) );
			  IndexDefinition index = new IndexDefinitionImpl( actions, null, new Label[]{ label }, new string[]{ propertyKey }, true );
			  return new NodeKeyConstraintDefinition( actions, index );
		 }

		 private ConstraintDefinition CreateNodePropertyExistenceConstraint( Label label, string propertyKey )
		 {
			  SchemaHelper.createNodePropertyExistenceConstraint( _db, label, propertyKey );
			  return new NodePropertyExistenceConstraintDefinition( mock( typeof( InternalSchemaActions ) ), label, new string[]{ propertyKey } );
		 }

		 private ConstraintDefinition CreateRelationshipPropertyExistenceConstraint( Types type, string propertyKey )
		 {
			  SchemaHelper.createRelPropertyExistenceConstraint( _db, type, propertyKey );
			  return new RelationshipPropertyExistenceConstraintDefinition( mock( typeof( InternalSchemaActions ) ), type, propertyKey );
		 }
	}

}
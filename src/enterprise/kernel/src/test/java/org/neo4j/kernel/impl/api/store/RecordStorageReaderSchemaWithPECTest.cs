using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Api.store
{
	using Test = org.junit.Test;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Iterators = Neo4Net.Collections.Helpers.Iterators;
	using ConstraintDescriptor = Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using RecordStorageReaderTestBase = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageReaderTestBase;
	using TestEnterpriseGraphDatabaseFactory = Neo4Net.Test.TestEnterpriseGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;

	public class RecordStorageReaderSchemaWithPECTest : RecordStorageReaderTestBase
	{
		 protected internal override IGraphDatabaseService CreateGraphDatabase()
		 {
			  return ( new TestEnterpriseGraphDatabaseFactory() ).newImpermanentDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllConstraints()
		 public virtual void ShouldListAllConstraints()
		 {
			  // Given
			  SchemaHelper.createUniquenessConstraint( Db, Label1, PropertyKey );
			  SchemaHelper.createUniquenessConstraint( Db, Label2, PropertyKey );
			  SchemaHelper.createNodeKeyConstraint( Db, Label1, OtherPropertyKey );
			  SchemaHelper.createNodeKeyConstraint( Db, Label2, OtherPropertyKey );

			  SchemaHelper.createNodePropertyExistenceConstraint( Db, Label2, PropertyKey );
			  SchemaHelper.createRelPropertyExistenceConstraint( Db, RelType1, PropertyKey );

			  SchemaHelper.awaitIndexes( Db );

			  // When
			  ISet<ConstraintDescriptor> constraints = asSet( StorageReader.constraintsGetAll() );

			  // Then
			  int labelId1 = LabelId( Label1 );
			  int labelId2 = LabelId( Label2 );
			  int relTypeId = RelationshipTypeId( RelType1 );
			  int propKeyId = PropertyKeyId( PropertyKey );
			  int propKeyId2 = PropertyKeyId( OtherPropertyKey );

			  assertThat( constraints, containsInAnyOrder( ConstraintDescriptorFactory.uniqueForLabel( labelId1, propKeyId ), ConstraintDescriptorFactory.uniqueForLabel( labelId2, propKeyId ), ConstraintDescriptorFactory.nodeKeyForLabel( labelId1, propKeyId2 ), ConstraintDescriptorFactory.nodeKeyForLabel( labelId2, propKeyId2 ), ConstraintDescriptorFactory.existsForLabel( labelId2, propKeyId ), ConstraintDescriptorFactory.existsForRelType( relTypeId, propKeyId ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllConstraintsForLabel()
		 public virtual void ShouldListAllConstraintsForLabel()
		 {
			  // Given
			  SchemaHelper.createNodePropertyExistenceConstraint( Db, Label1, PropertyKey );
			  SchemaHelper.createNodePropertyExistenceConstraint( Db, Label2, PropertyKey );

			  SchemaHelper.createUniquenessConstraint( Db, Label1, PropertyKey );
			  SchemaHelper.createNodeKeyConstraint( Db, Label1, OtherPropertyKey );
			  SchemaHelper.createNodeKeyConstraint( Db, Label2, OtherPropertyKey );

			  SchemaHelper.awaitIndexes( Db );

			  // When
			  ISet<ConstraintDescriptor> constraints = asSet( StorageReader.constraintsGetForLabel( LabelId( Label1 ) ) );

			  // Then
			  ISet<ConstraintDescriptor> expectedConstraints = asSet( UniqueConstraintDescriptor( Label1, PropertyKey ), NodeKeyConstraintDescriptor( Label1, OtherPropertyKey ), NodePropertyExistenceDescriptor( Label1, PropertyKey ) );

			  assertEquals( expectedConstraints, constraints );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllConstraintsForLabelAndProperty()
		 public virtual void ShouldListAllConstraintsForLabelAndProperty()
		 {
			  // Given
			  SchemaHelper.createUniquenessConstraint( Db, Label2, PropertyKey );
			  SchemaHelper.createUniquenessConstraint( Db, Label1, OtherPropertyKey );
			  SchemaHelper.createNodeKeyConstraint( Db, Label1, PropertyKey );
			  SchemaHelper.createNodeKeyConstraint( Db, Label2, OtherPropertyKey );

			  SchemaHelper.createNodePropertyExistenceConstraint( Db, Label1, PropertyKey );
			  SchemaHelper.createNodePropertyExistenceConstraint( Db, Label2, PropertyKey );

			  SchemaHelper.awaitIndexes( Db );

			  // When
			  ISet<ConstraintDescriptor> constraints = asSet( StorageReader.constraintsGetForSchema( SchemaDescriptorFactory.forLabel( LabelId( Label1 ), PropertyKeyId( PropertyKey ) ) ) );

			  // Then
			  ISet<ConstraintDescriptor> expected = asSet( NodeKeyConstraintDescriptor( Label1, PropertyKey ), NodePropertyExistenceDescriptor( Label1, PropertyKey ) );

			  assertEquals( expected, constraints );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllConstraintsForRelationshipType()
		 public virtual void ShouldListAllConstraintsForRelationshipType()
		 {
			  // Given
			  SchemaHelper.createRelPropertyExistenceConstraint( Db, RelType1, PropertyKey );
			  SchemaHelper.createRelPropertyExistenceConstraint( Db, RelType2, PropertyKey );
			  SchemaHelper.createRelPropertyExistenceConstraint( Db, RelType2, OtherPropertyKey );

			  // When
			  ISet<ConstraintDescriptor> constraints = asSet( StorageReader.constraintsGetForRelationshipType( RelationshipTypeId( RelType2 ) ) );

			  // Then
			  ISet<ConstraintDescriptor> expectedConstraints = Iterators.asSet( RelationshipPropertyExistenceDescriptor( RelType2, PropertyKey ), RelationshipPropertyExistenceDescriptor( RelType2, OtherPropertyKey ) );

			  assertEquals( expectedConstraints, constraints );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllConstraintsForRelationshipTypeAndProperty()
		 public virtual void ShouldListAllConstraintsForRelationshipTypeAndProperty()
		 {
			  // Given
			  SchemaHelper.createRelPropertyExistenceConstraint( Db, RelType1, PropertyKey );
			  SchemaHelper.createRelPropertyExistenceConstraint( Db, RelType1, OtherPropertyKey );

			  SchemaHelper.createRelPropertyExistenceConstraint( Db, RelType2, PropertyKey );
			  SchemaHelper.createRelPropertyExistenceConstraint( Db, RelType2, OtherPropertyKey );

			  // When
			  int relTypeId = RelationshipTypeId( RelType1 );
			  int propKeyId = PropertyKeyId( PropertyKey );
			  ISet<ConstraintDescriptor> constraints = asSet( StorageReader.constraintsGetForSchema( SchemaDescriptorFactory.forRelType( relTypeId, propKeyId ) ) );

			  // Then
			  ISet<ConstraintDescriptor> expectedConstraints = Iterators.asSet( RelationshipPropertyExistenceDescriptor( RelType1, PropertyKey ) );

			  assertEquals( expectedConstraints, constraints );
		 }

		 private ConstraintDescriptor UniqueConstraintDescriptor( Label label, string propertyKey )
		 {
			  int labelId = labelId( label );
			  int propKeyId = PropertyKeyId( propertyKey );
			  return ConstraintDescriptorFactory.uniqueForLabel( labelId, propKeyId );
		 }

		 private ConstraintDescriptor NodeKeyConstraintDescriptor( Label label, string propertyKey )
		 {
			  int labelId = labelId( label );
			  int propKeyId = PropertyKeyId( propertyKey );
			  return ConstraintDescriptorFactory.nodeKeyForLabel( labelId, propKeyId );
		 }

		 private ConstraintDescriptor NodePropertyExistenceDescriptor( Label label, string propertyKey )
		 {
			  int labelId = labelId( label );
			  int propKeyId = PropertyKeyId( propertyKey );
			  return ConstraintDescriptorFactory.existsForLabel( labelId, propKeyId );
		 }

		 private ConstraintDescriptor RelationshipPropertyExistenceDescriptor( RelationshipType relType, string propertyKey )
		 {
			  int relTypeId = RelationshipTypeId( relType );
			  int propKeyId = PropertyKeyId( propertyKey );
			  return ConstraintDescriptorFactory.existsForRelType( relTypeId, propKeyId );
		 }
	}

}
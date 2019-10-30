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
namespace Neo4Net.Kernel.api.schema.constraints
{
	using LabelSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaDescriptor;
	using RelationTypeSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.RelationTypeSchemaDescriptor;
	using Neo4Net.Kernel.Api.Internal.Schema;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using SchemaUtil = Neo4Net.Kernel.Api.Internal.Schema.SchemaUtil;

	public class ConstraintDescriptorFactory
	{
		 private ConstraintDescriptorFactory()
		 {
		 }

		 public static NodeExistenceConstraintDescriptor ExistsForLabel( int labelId, params int[] propertyIds )
		 {
			  return new NodeExistenceConstraintDescriptor( SchemaDescriptorFactory.forLabel( labelId, propertyIds ) );
		 }

		 public static RelExistenceConstraintDescriptor ExistsForRelType( int relTypeId, params int[] propertyIds )
		 {
			  return new RelExistenceConstraintDescriptor( SchemaDescriptorFactory.forRelType( relTypeId, propertyIds ) );
		 }

		 public static UniquenessConstraintDescriptor UniqueForLabel( int labelId, params int[] propertyIds )
		 {
			  return new UniquenessConstraintDescriptor( SchemaDescriptorFactory.forLabel( labelId, propertyIds ) );
		 }

		 public static NodeKeyConstraintDescriptor NodeKeyForLabel( int labelId, params int[] propertyIds )
		 {
			  return new NodeKeyConstraintDescriptor( SchemaDescriptorFactory.forLabel( labelId, propertyIds ) );
		 }

		 public static ConstraintDescriptor ExistsForSchema( SchemaDescriptor schema )
		 {
			  return Schema.computeWith( convertToExistenceConstraint );
		 }

		 public static NodeExistenceConstraintDescriptor ExistsForSchema( LabelSchemaDescriptor schema )
		 {
			  return new NodeExistenceConstraintDescriptor( schema );
		 }

		 public static RelExistenceConstraintDescriptor ExistsForSchema( RelationTypeSchemaDescriptor schema )
		 {
			  return new RelExistenceConstraintDescriptor( schema );
		 }

		 public static UniquenessConstraintDescriptor UniqueForSchema( SchemaDescriptor schema )
		 {
			  return Schema.computeWith( convertToUniquenessConstraint );
		 }

		 public static NodeKeyConstraintDescriptor NodeKeyForSchema( SchemaDescriptor schema )
		 {
			  return Schema.computeWith( convertToNodeKeyConstraint );
		 }

		 private static SchemaComputer<ConstraintDescriptor> convertToExistenceConstraint = new SchemaComputerAnonymousInnerClass();

		 private class SchemaComputerAnonymousInnerClass : SchemaComputer<ConstraintDescriptor>
		 {
			 public ConstraintDescriptor computeSpecific( LabelSchemaDescriptor schema )
			 {
				  return new NodeExistenceConstraintDescriptor( schema );
			 }

			 public ConstraintDescriptor computeSpecific( RelationTypeSchemaDescriptor schema )
			 {
				  return new RelExistenceConstraintDescriptor( schema );
			 }

			 public ConstraintDescriptor computeSpecific( SchemaDescriptor schema )
			 {
				  throw new System.NotSupportedException( format( "Cannot create existence constraint for schema '%s' of type %s", Schema.userDescription( SchemaUtil.idTokenNameLookup ), Schema.GetType().Name ) );
			 }
		 }

		 private static SchemaComputer<UniquenessConstraintDescriptor> convertToUniquenessConstraint = new SchemaComputerAnonymousInnerClass2();

		 private class SchemaComputerAnonymousInnerClass2 : SchemaComputer<UniquenessConstraintDescriptor>
		 {
			 public UniquenessConstraintDescriptor computeSpecific( LabelSchemaDescriptor schema )
			 {
				  return new UniquenessConstraintDescriptor( schema );
			 }

			 public UniquenessConstraintDescriptor computeSpecific( RelationTypeSchemaDescriptor schema )
			 {
				  throw new System.NotSupportedException( format( "Cannot create uniqueness constraint for schema '%s' of type %s", Schema.userDescription( SchemaUtil.idTokenNameLookup ), Schema.GetType().Name ) );
			 }

			 public UniquenessConstraintDescriptor computeSpecific( SchemaDescriptor schema )
			 {
				  throw new System.NotSupportedException( format( "Cannot create uniqueness constraint for schema '%s' of type %s", Schema.userDescription( SchemaUtil.idTokenNameLookup ), Schema.GetType().Name ) );
			 }
		 }

		 private static SchemaComputer<NodeKeyConstraintDescriptor> convertToNodeKeyConstraint = new SchemaComputerAnonymousInnerClass3();

		 private class SchemaComputerAnonymousInnerClass3 : SchemaComputer<NodeKeyConstraintDescriptor>
		 {
			 public NodeKeyConstraintDescriptor computeSpecific( LabelSchemaDescriptor schema )
			 {
				  return new NodeKeyConstraintDescriptor( schema );
			 }

			 public NodeKeyConstraintDescriptor computeSpecific( RelationTypeSchemaDescriptor schema )
			 {
				  throw new System.NotSupportedException( format( "Cannot create node key constraint for schema '%s' of type %s", Schema.userDescription( SchemaUtil.idTokenNameLookup ), Schema.GetType().Name ) );
			 }

			 public NodeKeyConstraintDescriptor computeSpecific( SchemaDescriptor schema )
			 {
				  throw new System.NotSupportedException( format( "Cannot create node key constraint for schema '%s' of type %s", Schema.userDescription( SchemaUtil.idTokenNameLookup ), Schema.GetType().Name ) );
			 }
		 }
	}

}
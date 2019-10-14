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
namespace Neo4Net.Kernel.api.schema
{
	using EntityType = Neo4Net.Storageengine.Api.EntityType;

	public class SchemaDescriptorFactory
	{
		 private SchemaDescriptorFactory()
		 {
		 }

		 public static LabelSchemaDescriptor ForLabel( int labelId, params int[] propertyIds )
		 {
			  ValidateLabelIds( labelId );
			  ValidatePropertyIds( propertyIds );
			  return new LabelSchemaDescriptor( labelId, propertyIds );
		 }

		 public static RelationTypeSchemaDescriptor ForRelType( int relTypeId, params int[] propertyIds )
		 {
			  ValidateRelationshipTypeIds( relTypeId );
			  ValidatePropertyIds( propertyIds );
			  return new RelationTypeSchemaDescriptor( relTypeId, propertyIds );
		 }

		 public static MultiTokenSchemaDescriptor MultiToken( int[] entityTokens, EntityType entityType, params int[] propertyIds )
		 {
			  ValidatePropertyIds( propertyIds );
			  switch ( entityType.innerEnumValue )
			  {
			  case EntityType.InnerEnum.NODE:
					ValidateLabelIds( entityTokens );
					break;
			  case EntityType.InnerEnum.RELATIONSHIP:
					ValidateRelationshipTypeIds( entityTokens );
					break;
			  default:
					throw new System.ArgumentException( "Cannot create schemadescriptor of type :" + entityType );
			  }
			  return new MultiTokenSchemaDescriptor( entityTokens, entityType, propertyIds );
		 }

		 private static void ValidatePropertyIds( int[] propertyIds )
		 {
			  foreach ( int propertyId in propertyIds )
			  {
					if ( StatementConstants.NO_SUCH_PROPERTY_KEY == propertyId )
					{
						 throw new System.ArgumentException( "Index schema descriptor can't be created for non existent property." );
					}
			  }
		 }

		 private static void ValidateRelationshipTypeIds( params int[] relTypes )
		 {
			  foreach ( int relType in relTypes )
			  {
					if ( StatementConstants.NO_SUCH_RELATIONSHIP_TYPE == relType )
					{
						 throw new System.ArgumentException( "Index schema descriptor can't be created for non existent relationship type." );
					}
			  }
		 }

		 private static void ValidateLabelIds( params int[] labelIds )
		 {
			  foreach ( int labelId in labelIds )
			  {
					if ( StatementConstants.NO_SUCH_LABEL == labelId )
					{
						 throw new System.ArgumentException( "Index schema descriptor can't be created for non existent label." );
					}
			  }
		 }
	}

}
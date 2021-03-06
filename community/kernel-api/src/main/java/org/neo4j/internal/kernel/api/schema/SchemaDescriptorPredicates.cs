﻿/*
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
namespace Org.Neo4j.@internal.Kernel.Api.schema
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;

	using EntityType = Org.Neo4j.Storageengine.Api.EntityType;

	public class SchemaDescriptorPredicates
	{
		 private SchemaDescriptorPredicates()
		 {
		 }

		 public static System.Predicate<T> HasLabel<T>( int labelId ) where T : SchemaDescriptorSupplier
		 {
			  return supplier =>
			  {
				SchemaDescriptor schema = supplier.schema();
				return Schema.entityType() == EntityType.NODE && ArrayUtils.contains(Schema.EntityTokenIds, labelId);
			  };
		 }

		 public static System.Predicate<T> HasRelType<T>( int relTypeId ) where T : SchemaDescriptorSupplier
		 {
			  return supplier =>
			  {
				SchemaDescriptor schema = supplier.schema();
				return Schema.entityType() == EntityType.RELATIONSHIP && ArrayUtils.contains(Schema.EntityTokenIds, relTypeId);
			  };
		 }

		 public static System.Predicate<T> HasProperty<T>( int propertyId ) where T : SchemaDescriptorSupplier
		 {
			  return supplier => HasProperty( supplier, propertyId );
		 }

		 public static bool HasLabel( SchemaDescriptorSupplier supplier, int labelId )
		 {
			  SchemaDescriptor schema = supplier.Schema();
			  return Schema.entityType() == EntityType.NODE && ArrayUtils.contains(Schema.EntityTokenIds, labelId);
		 }

		 public static bool HasRelType( SchemaDescriptorSupplier supplier, int relTypeId )
		 {
			  SchemaDescriptor schema = supplier.Schema();
			  return Schema.entityType() == EntityType.RELATIONSHIP && ArrayUtils.contains(Schema.EntityTokenIds, relTypeId);
		 }

		 public static bool HasProperty( SchemaDescriptorSupplier supplier, int propertyId )
		 {
			  int[] schemaProperties = supplier.Schema().PropertyIds;
			  foreach ( int schemaProp in schemaProperties )
			  {
					if ( schemaProp == propertyId )
					{
						 return true;
					}
			  }
			  return false;
		 }
	}

}
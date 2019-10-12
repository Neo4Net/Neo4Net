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
namespace Org.Neo4j.Kernel.api.schema
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;

	using TokenNameLookup = Org.Neo4j.@internal.Kernel.Api.TokenNameLookup;
	using Org.Neo4j.@internal.Kernel.Api.schema;
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using SchemaProcessor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaProcessor;
	using SchemaUtil = Org.Neo4j.@internal.Kernel.Api.schema.SchemaUtil;
	using ResourceTypes = Org.Neo4j.Kernel.impl.locking.ResourceTypes;
	using EntityType = Org.Neo4j.Storageengine.Api.EntityType;
	using ResourceType = Org.Neo4j.Storageengine.Api.@lock.ResourceType;

	public class RelationTypeSchemaDescriptor : Org.Neo4j.@internal.Kernel.Api.schema.RelationTypeSchemaDescriptor
	{
		 private readonly int _relTypeId;
		 private readonly int[] _propertyIds;

		 internal RelationTypeSchemaDescriptor( int relTypeId, params int[] propertyIds )
		 {
			  this._relTypeId = relTypeId;
			  this._propertyIds = propertyIds;
		 }

		 public override bool IsAffected( long[] entityTokenIds )
		 {
			  return ArrayUtils.contains( entityTokenIds, _relTypeId );
		 }

		 public override R ComputeWith<R>( SchemaComputer<R> processor )
		 {
			  return processor.ComputeSpecific( this );
		 }

		 public override void ProcessWith( SchemaProcessor processor )
		 {
			  processor.ProcessSpecific( this );
		 }

		 public override string UserDescription( TokenNameLookup tokenNameLookup )
		 {
			  return string.Format( "-[:{0}({1})]-", tokenNameLookup.RelationshipTypeGetName( _relTypeId ), SchemaUtil.niceProperties( tokenNameLookup, _propertyIds ) );
		 }

		 public virtual int RelTypeId
		 {
			 get
			 {
				  return _relTypeId;
			 }
		 }

		 public virtual int[] PropertyIds
		 {
			 get
			 {
				  return _propertyIds;
			 }
		 }

		 public virtual int[] EntityTokenIds
		 {
			 get
			 {
				  return new int[]{ _relTypeId };
			 }
		 }

		 public override int KeyId()
		 {
			  return RelTypeId;
		 }

		 public override ResourceType KeyType()
		 {
			  return ResourceTypes.RELATIONSHIP_TYPE;
		 }

		 public override EntityType EntityType()
		 {
			  return EntityType.RELATIONSHIP;
		 }

		 public override PropertySchemaType PropertySchemaType()
		 {
			  return PropertySchemaType.COMPLETE_ALL_TOKENS;
		 }

		 public override bool Equals( object o )
		 {
			  if ( o is RelationTypeSchemaDescriptor )
			  {
					RelationTypeSchemaDescriptor that = ( RelationTypeSchemaDescriptor )o;
					return _relTypeId == that.RelTypeId && Arrays.Equals( _propertyIds, that.PropertyIds );
			  }
			  return false;
		 }

		 public override int GetHashCode()
		 {
			  return Arrays.GetHashCode( _propertyIds ) + 31 * _relTypeId;
		 }

		 public override SchemaDescriptor Schema()
		 {
			  return this;
		 }
	}

}
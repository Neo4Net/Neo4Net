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
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;

	using TokenNameLookup = Neo4Net.Kernel.Api.Internal.TokenNameLookup;
	using Neo4Net.Kernel.Api.Internal.Schema;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using SchemaProcessor = Neo4Net.Kernel.Api.Internal.Schema.SchemaProcessor;
	using SchemaUtil = Neo4Net.Kernel.Api.Internal.Schema.SchemaUtil;
	using ResourceTypes = Neo4Net.Kernel.impl.locking.ResourceTypes;
	using EntityType = Neo4Net.Kernel.Api.StorageEngine.EntityType;
	using ResourceType = Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceType;

	public class RelationTypeSchemaDescriptor : Neo4Net.Kernel.Api.Internal.Schema.RelationTypeSchemaDescriptor
	{
		 private readonly int _relTypeId;
		 private readonly int[] _propertyIds;

		 internal RelationTypeSchemaDescriptor( int relTypeId, params int[] propertyIds )
		 {
			  this._relTypeId = relTypeId;
			  this._propertyIds = propertyIds;
		 }

		 public override bool IsAffected( long[] IEntityTokenIds )
		 {
			  return ArrayUtils.contains( IEntityTokenIds, _relTypeId );
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

		 public virtual int[] IEntityTokenIds
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
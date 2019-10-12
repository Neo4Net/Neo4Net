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
namespace Neo4Net.Kernel.api.schema
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;


	using TokenNameLookup = Neo4Net.@internal.Kernel.Api.TokenNameLookup;
	using Neo4Net.@internal.Kernel.Api.schema;
	using SchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor;
	using SchemaProcessor = Neo4Net.@internal.Kernel.Api.schema.SchemaProcessor;
	using SchemaUtil = Neo4Net.@internal.Kernel.Api.schema.SchemaUtil;
	using ResourceTypes = Neo4Net.Kernel.impl.locking.ResourceTypes;
	using EntityType = Neo4Net.Storageengine.Api.EntityType;
	using ResourceType = Neo4Net.Storageengine.Api.@lock.ResourceType;

	public class MultiTokenSchemaDescriptor : SchemaDescriptor
	{
		 private readonly int[] _entityTokens;
		 private readonly EntityType _entityType;
		 private readonly int[] _propertyIds;

		 internal MultiTokenSchemaDescriptor( int[] entityTokens, EntityType entityType, int[] propertyIds )
		 {
			  this._entityTokens = entityTokens;
			  this._entityType = entityType;
			  this._propertyIds = propertyIds;
		 }

		 public override bool IsAffected( long[] entityTokenIds )
		 {
			  foreach ( int id in _entityTokens )
			  {
					if ( ArrayUtils.contains( entityTokenIds, id ) )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 public override R ComputeWith<R>( SchemaComputer<R> computer )
		 {
			  return computer.ComputeSpecific( this );
		 }

		 public override void ProcessWith( SchemaProcessor processor )
		 {
			  processor.ProcessSpecific( this );
		 }

		 public override string ToString()
		 {
			  return "MultiTokenSchemaDescriptor[" + UserDescription( SchemaUtil.idTokenNameLookup ) + "]";
		 }

		 public override string UserDescription( TokenNameLookup tokenNameLookup )
		 {
			  return string.format( _entityType + ":%s(%s)", string.join( ", ", tokenNameLookup.EntityTokensGetNames( _entityType, _entityTokens ) ), SchemaUtil.niceProperties( tokenNameLookup, _propertyIds ) );
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
				  return _entityTokens;
			 }
		 }

		 public override int KeyId()
		 {
			  throw new System.NotSupportedException( this + " does not have a single keyId." );
		 }

		 public override ResourceType KeyType()
		 {
			  return _entityType == EntityType.NODE ? ResourceTypes.LABEL : ResourceTypes.RELATIONSHIP_TYPE;
		 }

		 public override EntityType EntityType()
		 {
			  return _entityType;
		 }

		 public override Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor_PropertySchemaType PropertySchemaType()
		 {
			  return Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor_PropertySchemaType.PartialAnyToken;
		 }

		 public override SchemaDescriptor Schema()
		 {
			  return this;
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( !( o is SchemaDescriptor ) )
			  {
					return false;
			  }
			  SchemaDescriptor that = ( SchemaDescriptor ) o;
			  return Arrays.Equals( _entityTokens, that.EntityTokenIds ) && _entityType == that.EntityType() && Arrays.Equals(_propertyIds, that.PropertyIds);
		 }

		 public override int GetHashCode()
		 {

			  int result = Objects.hash( _entityType );
			  result = 31 * result + Arrays.GetHashCode( _entityTokens );
			  result = 31 * result + Arrays.GetHashCode( _propertyIds );
			  return result;
		 }
	}

}
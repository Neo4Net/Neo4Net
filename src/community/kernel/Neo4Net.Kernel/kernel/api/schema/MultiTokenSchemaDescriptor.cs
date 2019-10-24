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

	public class MultiTokenSchemaDescriptor : SchemaDescriptor
	{
		 private readonly int[] _entityTokens;
		 private readonly EntityType _entityType;
		 private readonly int[] _propertyIds;

		 internal MultiTokenSchemaDescriptor( int[] IEntityTokens, EntityType EntityType, int[] propertyIds )
		 {
			  this._entityTokens = IEntityTokens;
			  this._entityType = EntityType;
			  this._propertyIds = propertyIds;
		 }

		 public override bool IsAffected( long[] IEntityTokenIds )
		 {
			  foreach ( int id in _entityTokens )
			  {
					if ( ArrayUtils.contains( IEntityTokenIds, id ) )
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

		 public virtual int[] IEntityTokenIds
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

		 public override Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor_PropertySchemaType PropertySchemaType()
		 {
			  return Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor_PropertySchemaType.PartialAnyToken;
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
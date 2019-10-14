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

	using TokenNameLookup = Neo4Net.@internal.Kernel.Api.TokenNameLookup;
	using Neo4Net.@internal.Kernel.Api.schema;
	using SchemaProcessor = Neo4Net.@internal.Kernel.Api.schema.SchemaProcessor;
	using SchemaUtil = Neo4Net.@internal.Kernel.Api.schema.SchemaUtil;
	using ResourceTypes = Neo4Net.Kernel.impl.locking.ResourceTypes;
	using EntityType = Neo4Net.Storageengine.Api.EntityType;
	using ResourceType = Neo4Net.Storageengine.Api.@lock.ResourceType;

	public class LabelSchemaDescriptor : Neo4Net.@internal.Kernel.Api.schema.LabelSchemaDescriptor
	{
		 private readonly int _labelId;
		 private readonly int[] _propertyIds;

		 internal LabelSchemaDescriptor( int labelId, params int[] propertyIds )
		 {
			  this._labelId = labelId;
			  this._propertyIds = propertyIds;
		 }

		 public override bool IsAffected( long[] entityTokenIds )
		 {
			  return ArrayUtils.contains( entityTokenIds, _labelId );
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
			  return string.Format( ":{0}({1})", tokenNameLookup.LabelGetName( _labelId ), SchemaUtil.niceProperties( tokenNameLookup, _propertyIds ) );
		 }

		 public virtual int LabelId
		 {
			 get
			 {
				  return _labelId;
			 }
		 }

		 public override int KeyId()
		 {
			  return LabelId;
		 }

		 public override ResourceType KeyType()
		 {
			  return ResourceTypes.LABEL;
		 }

		 public override EntityType EntityType()
		 {
			  return EntityType.NODE;
		 }

		 public override PropertySchemaType PropertySchemaType()
		 {
			  return PropertySchemaType.COMPLETE_ALL_TOKENS;
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
				  return new int[]{ _labelId };
			 }
		 }

		 public override bool Equals( object o )
		 {
			  if ( o is LabelSchemaDescriptor )
			  {
					LabelSchemaDescriptor that = ( LabelSchemaDescriptor )o;
					return _labelId == that.LabelId && Arrays.Equals( _propertyIds, that.PropertyIds );
			  }
			  return false;
		 }

		 public override int GetHashCode()
		 {
			  return Arrays.GetHashCode( _propertyIds ) + 31 * _labelId;
		 }

		 public override string ToString()
		 {
			  return "LabelSchemaDescriptor( " + UserDescription( SchemaUtil.idTokenNameLookup ) + " )";
		 }

		 public override LabelSchemaDescriptor Schema()
		 {
			  return this;
		 }
	}

}
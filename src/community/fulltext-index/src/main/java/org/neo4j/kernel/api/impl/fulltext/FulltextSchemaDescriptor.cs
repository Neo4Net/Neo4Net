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
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{

	using TokenNameLookup = Neo4Net.Kernel.Api.Internal.TokenNameLookup;
	using Neo4Net.Kernel.Api.Internal.schema;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptor;
	using SchemaProcessor = Neo4Net.Kernel.Api.Internal.schema.SchemaProcessor;
	using EntityType = Neo4Net.Kernel.Api.StorageEngine.EntityType;
	using ResourceType = Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceType;

	internal class FulltextSchemaDescriptor : SchemaDescriptor
	{
		 private readonly SchemaDescriptor _schema;
		 private readonly Properties _indexConfiguration;

		 internal FulltextSchemaDescriptor( SchemaDescriptor schema, Properties indexConfiguration )
		 {
			  this._schema = schema;
			  this._indexConfiguration = indexConfiguration;
		 }

		 public override bool IsAffected( long[] IEntityTokenIds )
		 {
			  return _schema.isAffected( IEntityTokenIds );
		 }

		 public override R ComputeWith<R>( SchemaComputer<R> computer )
		 {
			  return _schema.computeWith( computer );
		 }

		 public override void ProcessWith( SchemaProcessor processor )
		 {
			  _schema.processWith( processor );
		 }

		 public override string UserDescription( TokenNameLookup tokenNameLookup )
		 {
			  return _schema.userDescription( tokenNameLookup );
		 }

		 public virtual int[] PropertyIds
		 {
			 get
			 {
				  return _schema.PropertyIds;
			 }
		 }

		 public virtual int PropertyId
		 {
			 get
			 {
				  return _schema.PropertyId;
			 }
		 }

		 public virtual int[] IEntityTokenIds
		 {
			 get
			 {
				  return _schema.EntityTokenIds;
			 }
		 }

		 public override int KeyId()
		 {
			  return _schema.keyId();
		 }

		 public override ResourceType KeyType()
		 {
			  return _schema.keyType();
		 }

		 public override EntityType EntityType()
		 {
			  return _schema.entityType();
		 }

		 public override Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptor_PropertySchemaType PropertySchemaType()
		 {
			  return _schema.propertySchemaType();
		 }

		 public override SchemaDescriptor Schema()
		 {
			  return this;
		 }

		 public override int GetHashCode()
		 {
			  return _schema.GetHashCode();
		 }

		 public override bool Equals( object obj )
		 {
			  if ( obj is FulltextSchemaDescriptor )
			  {
					return _schema.Equals( ( ( FulltextSchemaDescriptor ) obj )._schema );
			  }
			  return _schema.Equals( obj );
		 }

		 internal virtual Properties IndexConfiguration
		 {
			 get
			 {
				  return _indexConfiguration;
			 }
		 }

		 internal virtual bool EventuallyConsistent
		 {
			 get
			 {
				  return bool.Parse( _indexConfiguration.getProperty( FulltextIndexSettings.INDEX_CONFIG_EVENTUALLY_CONSISTENT ) );
			 }
		 }
	}

}
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
namespace Org.Neo4j.Kernel.Api.Impl.Fulltext
{

	using TokenNameLookup = Org.Neo4j.@internal.Kernel.Api.TokenNameLookup;
	using Org.Neo4j.@internal.Kernel.Api.schema;
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using SchemaProcessor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaProcessor;
	using EntityType = Org.Neo4j.Storageengine.Api.EntityType;
	using ResourceType = Org.Neo4j.Storageengine.Api.@lock.ResourceType;

	internal class FulltextSchemaDescriptor : SchemaDescriptor
	{
		 private readonly SchemaDescriptor _schema;
		 private readonly Properties _indexConfiguration;

		 internal FulltextSchemaDescriptor( SchemaDescriptor schema, Properties indexConfiguration )
		 {
			  this._schema = schema;
			  this._indexConfiguration = indexConfiguration;
		 }

		 public override bool IsAffected( long[] entityTokenIds )
		 {
			  return _schema.isAffected( entityTokenIds );
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

		 public virtual int[] EntityTokenIds
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

		 public override Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor_PropertySchemaType PropertySchemaType()
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
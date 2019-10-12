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
namespace Neo4Net.Kernel.impl.cache
{
	using NamedToken = Neo4Net.@internal.Kernel.Api.NamedToken;
	using SchemaState = Neo4Net.Kernel.Impl.Api.SchemaState;
	using SchemaCache = Neo4Net.Kernel.Impl.Api.store.SchemaCache;
	using CacheAccessBackDoor = Neo4Net.Kernel.impl.core.CacheAccessBackDoor;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using SchemaRule = Neo4Net.Storageengine.Api.schema.SchemaRule;

	public class BridgingCacheAccess : CacheAccessBackDoor
	{
		 private readonly SchemaCache _schemaCache;
		 private readonly SchemaState _schemaState;
		 private readonly TokenHolders _tokenHolders;

		 public BridgingCacheAccess( SchemaCache schemaCache, SchemaState schemaState, TokenHolders tokenHolders )
		 {
			  this._schemaCache = schemaCache;
			  this._schemaState = schemaState;
			  this._tokenHolders = tokenHolders;
		 }

		 public override void AddSchemaRule( SchemaRule rule )
		 {
			  _schemaCache.addSchemaRule( rule );
		 }

		 public override void RemoveSchemaRuleFromCache( long id )
		 {
			  _schemaCache.removeSchemaRule( id );
			  _schemaState.clear();
		 }

		 public override void AddRelationshipTypeToken( NamedToken type )
		 {
			  _tokenHolders.relationshipTypeTokens().addToken(type);
		 }

		 public override void AddLabelToken( NamedToken label )
		 {
			  _tokenHolders.labelTokens().addToken(label);
		 }

		 public override void AddPropertyKeyToken( NamedToken propertyKey )
		 {
			  _tokenHolders.propertyKeyTokens().addToken(propertyKey);
		 }
	}

}
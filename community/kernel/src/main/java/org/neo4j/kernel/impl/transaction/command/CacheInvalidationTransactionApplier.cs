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
namespace Org.Neo4j.Kernel.impl.transaction.command
{
	using NamedToken = Org.Neo4j.@internal.Kernel.Api.NamedToken;
	using TransactionApplier = Org.Neo4j.Kernel.Impl.Api.TransactionApplier;
	using CacheAccessBackDoor = Org.Neo4j.Kernel.impl.core.CacheAccessBackDoor;
	using LabelTokenStore = Org.Neo4j.Kernel.impl.store.LabelTokenStore;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using PropertyKeyTokenStore = Org.Neo4j.Kernel.impl.store.PropertyKeyTokenStore;
	using RelationshipTypeTokenStore = Org.Neo4j.Kernel.impl.store.RelationshipTypeTokenStore;
	using LabelTokenCommand = Org.Neo4j.Kernel.impl.transaction.command.Command.LabelTokenCommand;
	using PropertyKeyTokenCommand = Org.Neo4j.Kernel.impl.transaction.command.Command.PropertyKeyTokenCommand;
	using RelationshipTypeTokenCommand = Org.Neo4j.Kernel.impl.transaction.command.Command.RelationshipTypeTokenCommand;

	public class CacheInvalidationTransactionApplier : Org.Neo4j.Kernel.Impl.Api.TransactionApplier_Adapter
	{
		 private readonly CacheAccessBackDoor _cacheAccess;
		 private readonly RelationshipTypeTokenStore _relationshipTypeTokenStore;
		 private readonly LabelTokenStore _labelTokenStore;
		 private readonly PropertyKeyTokenStore _propertyKeyTokenStore;

		 public CacheInvalidationTransactionApplier( NeoStores neoStores, CacheAccessBackDoor cacheAccess )
		 {
			  this._cacheAccess = cacheAccess;
			  this._relationshipTypeTokenStore = neoStores.RelationshipTypeTokenStore;
			  this._labelTokenStore = neoStores.LabelTokenStore;
			  this._propertyKeyTokenStore = neoStores.PropertyKeyTokenStore;
		 }

		 public override bool VisitRelationshipTypeTokenCommand( RelationshipTypeTokenCommand command )
		 {
			  NamedToken type = _relationshipTypeTokenStore.getToken( ( int ) command.Key );
			  _cacheAccess.addRelationshipTypeToken( type );

			  return false;
		 }

		 public override bool VisitLabelTokenCommand( LabelTokenCommand command )
		 {
			  NamedToken labelId = _labelTokenStore.getToken( ( int ) command.Key );
			  _cacheAccess.addLabelToken( labelId );

			  return false;
		 }

		 public override bool VisitPropertyKeyTokenCommand( PropertyKeyTokenCommand command )
		 {
			  NamedToken index = _propertyKeyTokenStore.getToken( ( int ) command.Key );
			  _cacheAccess.addPropertyKeyToken( index );

			  return false;
		 }

		 public override void Close()
		 {
			  // Nothing to close
		 }
	}

}
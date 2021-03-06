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
namespace Org.Neo4j.Kernel.Impl.Api
{
	using AuxiliaryTransactionState = Org.Neo4j.Kernel.api.txstate.auxiliary.AuxiliaryTransactionState;
	using AuxiliaryTransactionStateProvider = Org.Neo4j.Kernel.api.txstate.auxiliary.AuxiliaryTransactionStateProvider;
	using ExplicitIndexTransactionStateImpl = Org.Neo4j.Kernel.Impl.Api.state.ExplicitIndexTransactionStateImpl;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;

	public class ExplicitIndexTransactionStateProvider : AuxiliaryTransactionStateProvider
	{
		 public const string PROVIDER_KEY = "EXPLICIT INDEX TX STATE PROVIDER";

		 private readonly IndexConfigStore _indexConfigStore;
		 private readonly ExplicitIndexProvider _explicitIndexProviderLookup;

		 public ExplicitIndexTransactionStateProvider( IndexConfigStore indexConfigStore, ExplicitIndexProvider explicitIndexProviderLookup )
		 {
			  this._indexConfigStore = indexConfigStore;
			  this._explicitIndexProviderLookup = explicitIndexProviderLookup;
		 }

		 public virtual object IdentityKey
		 {
			 get
			 {
				  return PROVIDER_KEY;
			 }
		 }

		 public override AuxiliaryTransactionState CreateNewAuxiliaryTransactionState()
		 {
			  return new CachingExplicitIndexTransactionState( new ExplicitIndexTransactionStateImpl( _indexConfigStore, _explicitIndexProviderLookup ) );
		 }
	}

}
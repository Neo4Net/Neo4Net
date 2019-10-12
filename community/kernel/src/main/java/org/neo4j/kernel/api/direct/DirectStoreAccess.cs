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
namespace Org.Neo4j.Kernel.api.direct
{

	using LabelScanStore = Org.Neo4j.Kernel.api.labelscan.LabelScanStore;
	using IndexProviderMap = Org.Neo4j.Kernel.Impl.Api.index.IndexProviderMap;
	using TokenHolders = Org.Neo4j.Kernel.impl.core.TokenHolders;
	using StoreAccess = Org.Neo4j.Kernel.impl.store.StoreAccess;

	public class DirectStoreAccess : System.IDisposable
	{
		 private readonly StoreAccess _nativeStores;
		 private readonly LabelScanStore _labelScanStore;
		 private readonly IndexProviderMap _indexes;
		 private readonly TokenHolders _tokenHolders;

		 public DirectStoreAccess( StoreAccess nativeStores, LabelScanStore labelScanStore, IndexProviderMap indexes, TokenHolders tokenHolders )
		 {
			  this._nativeStores = nativeStores;
			  this._labelScanStore = labelScanStore;
			  this._indexes = indexes;
			  this._tokenHolders = tokenHolders;
		 }

		 public virtual StoreAccess NativeStores()
		 {
			  return _nativeStores;
		 }

		 public virtual LabelScanStore LabelScanStore()
		 {
			  return _labelScanStore;
		 }

		 public virtual IndexProviderMap Indexes()
		 {
			  return _indexes;
		 }

		 public virtual TokenHolders TokenHolders()
		 {
			  return _tokenHolders;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _nativeStores.close();
			  _labelScanStore.shutdown();
		 }
	}

}
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
namespace Org.Neo4j.Kernel.Impl.Api.index.sampling
{
	using TokenNameLookup = Org.Neo4j.@internal.Kernel.Api.TokenNameLookup;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	public class OnlineIndexSamplingJobFactory : IndexSamplingJobFactory
	{
		 private readonly IndexStoreView _storeView;
		 private readonly LogProvider _logProvider;
		 private readonly TokenNameLookup _nameLookup;

		 public OnlineIndexSamplingJobFactory( IndexStoreView storeView, TokenNameLookup nameLookup, LogProvider logProvider )
		 {
			  this._storeView = storeView;
			  this._logProvider = logProvider;
			  this._nameLookup = nameLookup;
		 }

		 public override IndexSamplingJob Create( long indexId, IndexProxy indexProxy )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String indexUserDescription = indexProxy.getDescriptor().userDescription(nameLookup);
			  string indexUserDescription = indexProxy.Descriptor.userDescription( _nameLookup );
			  return new OnlineIndexSamplingJob( indexId, indexProxy, _storeView, indexUserDescription, _logProvider );
		 }
	}

}
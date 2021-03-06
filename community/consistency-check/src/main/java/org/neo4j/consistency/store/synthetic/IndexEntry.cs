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
namespace Org.Neo4j.Consistency.store.synthetic
{
	using TokenNameLookup = Org.Neo4j.@internal.Kernel.Api.TokenNameLookup;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;

	/// <summary>
	/// Synthetic record type that stands in for a real record to fit in conveniently
	/// with consistency checking
	/// </summary>
	public class IndexEntry : AbstractBaseRecord
	{
		 private readonly StoreIndexDescriptor _indexDescriptor;
		 private readonly TokenNameLookup _tokenNameLookup;

		 public IndexEntry( StoreIndexDescriptor indexDescriptor, TokenNameLookup tokenNameLookup, long nodeId ) : base( nodeId )
		 {
			  this._indexDescriptor = indexDescriptor;
			  this._tokenNameLookup = tokenNameLookup;
			  InUse = true;
		 }

		 public override void Clear()
		 {
			  Initialize( false );
		 }

		 public override string ToString()
		 {
			  return format( "IndexEntry[nodeId=%d, index=%s]", Id, _indexDescriptor.ToString( _tokenNameLookup ) );
		 }
	}

}
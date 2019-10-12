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
namespace Neo4Net.Kernel.Impl.Api.index
{
	public class IndexCountsRemover
	{
		 private readonly IndexStoreView _storeView;
		 private readonly long _indexId;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public IndexCountsRemover(final IndexStoreView storeView, final long indexId)
		 public IndexCountsRemover( IndexStoreView storeView, long indexId )
		 {
			  this._storeView = storeView;
			  this._indexId = indexId;
		 }

		 public virtual void Remove()
		 {
			  _storeView.replaceIndexCounts( _indexId, 0, 0, 0 );
		 }
	}

}
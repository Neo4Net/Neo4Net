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
namespace Org.Neo4j.Kernel.Impl.Api.index.updater
{
	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Org.Neo4j.Kernel.Api.Index;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;

	public class UpdateCountingIndexUpdater : IndexUpdater
	{
		 private readonly IndexStoreView _storeView;
		 private readonly long _indexId;
		 private readonly IndexUpdater @delegate;
		 private long _updates;

		 public UpdateCountingIndexUpdater( IndexStoreView storeView, long indexId, IndexUpdater @delegate )
		 {
			  this._storeView = storeView;
			  this._indexId = indexId;
			  this.@delegate = @delegate;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void process(org.neo4j.kernel.api.index.IndexEntryUpdate<?> update) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void Process<T1>( IndexEntryUpdate<T1> update )
		 {
			  @delegate.Process( update );
			  _updates++;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void Close()
		 {
			  @delegate.Close();
			  _storeView.incrementIndexUpdates( _indexId, _updates );
		 }
	}

}
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
namespace Neo4Net.Consistency.checking.index
{
	using IndexCheck = Neo4Net.Consistency.checking.full.IndexCheck;
	using Neo4Net.Consistency.checking.full;
	using ConsistencyReporter = Neo4Net.Consistency.report.ConsistencyReporter;
	using IndexEntry = Neo4Net.Consistency.Store.Synthetic.IndexEntry;
	using TokenNameLookup = Neo4Net.Kernel.Api.Internal.TokenNameLookup;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;

	public class IndexEntryProcessor : Neo4Net.Consistency.checking.full.RecordProcessor_Adapter<long>
	{
		 private readonly ConsistencyReporter _reporter;
		 private readonly IndexCheck _indexCheck;
		 private readonly StoreIndexDescriptor _indexDescriptor;
		 private readonly TokenNameLookup _tokenNameLookup;

		 public IndexEntryProcessor( ConsistencyReporter reporter, IndexCheck indexCheck, StoreIndexDescriptor indexDescriptor, TokenNameLookup tokenNameLookup )
		 {
			  this._reporter = reporter;
			  this._indexCheck = indexCheck;
			  this._indexDescriptor = indexDescriptor;
			  this._tokenNameLookup = tokenNameLookup;
		 }

		 public override void Process( long? nodeId )
		 {
			  _reporter.forIndexEntry( new IndexEntry( _indexDescriptor, _tokenNameLookup, nodeId.Value ), _indexCheck );
		 }
	}

}
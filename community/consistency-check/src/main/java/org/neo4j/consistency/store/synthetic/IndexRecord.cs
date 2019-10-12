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
namespace Org.Neo4j.Consistency.store.synthetic
{
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;

	public class IndexRecord : AbstractBaseRecord
	{
		 private readonly StoreIndexDescriptor _indexRule;

		 public IndexRecord( StoreIndexDescriptor indexRule ) : base( indexRule.Id )
		 {
			  this._indexRule = indexRule;
		 }

		 public override void Clear()
		 {
			  Initialize( false );
		 }

		 public override string ToString()
		 {
			  return "Index[ " + _indexRule.ToString() + " ]";
		 }
	}

}
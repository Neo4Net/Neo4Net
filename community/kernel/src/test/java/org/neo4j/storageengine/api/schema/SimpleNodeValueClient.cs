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
namespace Org.Neo4j.Storageengine.Api.schema
{
	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using Value = Org.Neo4j.Values.Storable.Value;

	public class SimpleNodeValueClient : IndexProgressor_NodeValueClient
	{
		 public long Reference;
		 public Value[] Values;
		 private IndexProgressor _progressor;

		 public virtual bool Next()
		 {
			  if ( _progressor.next() )
			  {
					return true;
			  }
			  CloseProgressor();
			  return false;
		 }

		 public override void Initialize( IndexDescriptor descriptor, IndexProgressor progressor, IndexQuery[] query, IndexOrder indexOrder, bool needsValues )
		 {
			  this._progressor = progressor;
		 }

		 public override bool AcceptNode( long reference, params Value[] values )
		 {
			  this.Reference = reference;
			  this.Values = values;
			  return true;
		 }

		 public override bool NeedsValues()
		 {
			  return true;
		 }

		 private void CloseProgressor()
		 {
			  if ( _progressor != null )
			  {
					_progressor.close();
					_progressor = null;
			  }
		 }
	}

}
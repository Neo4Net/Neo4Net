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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using IndexOrder = Neo4Net.Kernel.Api.Internal.IndexOrder;
	using IndexQuery = Neo4Net.Kernel.Api.Internal.IndexQuery;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using IndexProgressor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexProgressor;
	using Value = Neo4Net.Values.Storable.Value;

	/// <summary>
	/// Simple NodeValueClient test utility.
	/// </summary>
	public class GatheringNodeValueClient : Neo4Net.Kernel.Api.StorageEngine.schema.IndexProgressor_NodeValueClient
	{
		 public long Reference;
		 public Value[] Values;
		 public IndexDescriptor Descriptor;
		 public IndexProgressor Progressor;
		 public IndexQuery[] Query;
		 public IndexOrder Order;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public bool NeedsValuesConflict;

		 public override void Initialize( IndexDescriptor descriptor, IndexProgressor progressor, IndexQuery[] query, IndexOrder order, bool needsValues )
		 {
			  this.Descriptor = descriptor;
			  this.Progressor = progressor;
			  this.Query = query;
			  this.Order = order;
			  this.NeedsValuesConflict = needsValues;
		 }

		 public override bool AcceptNode( long reference, params Value[] values )
		 {
			  this.Reference = reference;
			  this.Values = values;
			  return true;
		 }

		 public override bool NeedsValues()
		 {
			  return NeedsValuesConflict;
		 }
	}

}
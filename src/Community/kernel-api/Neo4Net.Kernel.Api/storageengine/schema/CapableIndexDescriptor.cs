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
namespace Neo4Net.Kernel.Api.StorageEngine.schema
{
	using IIndexCapability = Neo4Net.Kernel.Api.Internal.IIndexCapability;
	using IndexLimitation = Neo4Net.Kernel.Api.Internal.IndexLimitation;
	using IndexOrder = Neo4Net.Kernel.Api.Internal.IndexOrder;
	using IndexValueCapability = Neo4Net.Kernel.Api.Internal.IndexValueCapability;
	using ValueCategory = Neo4Net.Values.Storable.ValueCategory;

	/// <summary>
	/// A committed index with specified capabilities.
	/// </summary>
	public class CapableIndexDescriptor : StoreIndexDescriptor
	{
		 private readonly IIndexCapability _indexCapability;

		 public CapableIndexDescriptor( StoreIndexDescriptor indexDescriptor, IIndexCapability indexCapability ) : base( indexDescriptor )
		 {
			  this._indexCapability = indexCapability;
		 }

		 public override IndexOrder[] OrderCapability( params ValueCategory[] valueCategories )
		 {
			  return _indexCapability.orderCapability( valueCategories );
		 }

		 public override IndexValueCapability ValueCapability( params ValueCategory[] valueCategories )
		 {
			  return _indexCapability.valueCapability( valueCategories );
		 }

		 public override IndexLimitation[] Limitations()
		 {
			  return _indexCapability.limitations();
		 }

		 public override bool FulltextIndex
		 {
			 get
			 {
				  return _indexCapability.FulltextIndex;
			 }
		 }

		 public override bool EventuallyConsistent
		 {
			 get
			 {
				  return _indexCapability.EventuallyConsistent;
			 }
		 }
	}

}
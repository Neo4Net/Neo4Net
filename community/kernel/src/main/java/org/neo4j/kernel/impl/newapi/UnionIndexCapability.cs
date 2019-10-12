using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Impl.Newapi
{

	using IndexCapability = Org.Neo4j.@internal.Kernel.Api.IndexCapability;
	using IndexLimitation = Org.Neo4j.@internal.Kernel.Api.IndexLimitation;
	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexValueCapability = Org.Neo4j.@internal.Kernel.Api.IndexValueCapability;
	using ValueCategory = Org.Neo4j.Values.Storable.ValueCategory;

	/// <summary>
	/// Present the union of multiple index capabilities.
	/// If one of the internal capabilities has a capability, the union has the capability.
	/// </summary>
	public class UnionIndexCapability : IndexCapability
	{
		 private readonly IEnumerable<IndexCapability> _capabilities;
		 private readonly IndexLimitation[] _limitationsUnion;

		 protected internal UnionIndexCapability( IEnumerable<IndexCapability> capabilities )
		 {
			  this._capabilities = capabilities;
			  this._limitationsUnion = LimitationsUnion( capabilities );
		 }

		 public override IndexOrder[] OrderCapability( params ValueCategory[] valueCategories )
		 {
			  ISet<IndexOrder> orderCapability = new HashSet<IndexOrder>();
			  foreach ( IndexCapability capability in _capabilities )
			  {
					orderCapability.addAll( Arrays.asList( capability.OrderCapability( valueCategories ) ) );
			  }
			  return orderCapability.toArray( new IndexOrder[orderCapability.Count] );
		 }

		 public override IndexValueCapability ValueCapability( params ValueCategory[] valueCategories )
		 {
			  IndexValueCapability currentBest = IndexValueCapability.NO;
			  foreach ( IndexCapability capability in _capabilities )
			  {
					IndexValueCapability next = capability.ValueCapability( valueCategories );
					if ( next.compare( currentBest ) > 0 )
					{
						 currentBest = next;
					}
			  }
			  return currentBest;
		 }

		 public virtual bool FulltextIndex
		 {
			 get
			 {
				  return false;
			 }
		 }

		 public virtual bool EventuallyConsistent
		 {
			 get
			 {
				  return false;
			 }
		 }

		 public override IndexLimitation[] Limitations()
		 {
			  return _limitationsUnion;
		 }

		 private IndexLimitation[] LimitationsUnion( IEnumerable<IndexCapability> capabilities )
		 {
			  HashSet<IndexLimitation> union = new HashSet<IndexLimitation>();
			  foreach ( IndexCapability capability in capabilities )
			  {
					union.addAll( Arrays.asList( capability.Limitations() ) );
			  }
			  return union.toArray( new IndexLimitation[union.Count] );
		 }
	}

}
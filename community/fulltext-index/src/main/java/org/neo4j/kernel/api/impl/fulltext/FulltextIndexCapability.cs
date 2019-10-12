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
namespace Org.Neo4j.Kernel.Api.Impl.Fulltext
{
	using IndexCapability = Org.Neo4j.@internal.Kernel.Api.IndexCapability;
	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexValueCapability = Org.Neo4j.@internal.Kernel.Api.IndexValueCapability;
	using ValueCategory = Org.Neo4j.Values.Storable.ValueCategory;

	internal class FulltextIndexCapability : IndexCapability
	{
		 private readonly bool _isEventuallyConsistent;

		 internal FulltextIndexCapability( bool isEventuallyConsistent )
		 {
			  this._isEventuallyConsistent = isEventuallyConsistent;
		 }

		 public override IndexOrder[] OrderCapability( params ValueCategory[] valueCategories )
		 {
			  return Org.Neo4j.@internal.Kernel.Api.IndexCapability_Fields.OrderNone;
		 }

		 public override IndexValueCapability ValueCapability( params ValueCategory[] valueCategories )
		 {
			  return IndexValueCapability.NO;
		 }

		 public virtual bool FulltextIndex
		 {
			 get
			 {
				  return true;
			 }
		 }

		 public virtual bool EventuallyConsistent
		 {
			 get
			 {
				  return _isEventuallyConsistent;
			 }
		 }
	}

}
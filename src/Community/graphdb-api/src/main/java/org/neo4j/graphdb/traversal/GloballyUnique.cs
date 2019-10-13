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
namespace Neo4Net.Graphdb.traversal
{
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;

	internal class GloballyUnique : AbstractUniquenessFilter
	{
		 private readonly MutableLongSet _visited = new LongHashSet( 1 << 12 );

		 internal GloballyUnique( PrimitiveTypeFetcher type ) : base( type )
		 {
		 }

		 public override bool Check( TraversalBranch branch )
		 {
			  return _visited.add( Type.getId( branch ) );
		 }

		 public override bool CheckFull( Path path )
		 {
			  // Since this is for bidirectional uniqueness checks and
			  // uniqueness is enforced through the shared "visited" set
			  // this uniqueness contract is fulfilled automatically.
			  return true;
		 }
	}

}
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
namespace Neo4Net.Graphalgo.impl.path
{
	using Neo4Net.Graphalgo;
	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using TraversalMetadata = Neo4Net.Graphdb.traversal.TraversalMetadata;
	using Traverser = Neo4Net.Graphdb.traversal.Traverser;
	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using Neo4Net.Helpers.Collection;

	public abstract class TraversalPathFinder : PathFinder<Path>
	{
		 private Traverser _lastTraverser;

		 public override Path FindSinglePath( Node start, Node end )
		 {
			  return Iterables.firstOrNull( FindAllPaths( start, end ) );
		 }

		 protected internal virtual int? MaxResultCount()
		 {
			  return null;
		 }

		 public override IEnumerable<Path> FindAllPaths( Node start, Node end )
		 {
			  _lastTraverser = InstantiateTraverser( start, end );
			  int? maxResultCount = maxResultCount();
			  return maxResultCount != null ? new LimitingResourceIterable<Path>( _lastTraverser, maxResultCount.Value ) : _lastTraverser;
		 }

		 protected internal abstract Traverser InstantiateTraverser( Node start, Node end );

		 public override TraversalMetadata Metadata()
		 {
			  if ( _lastTraverser == null )
			  {
					throw new System.InvalidOperationException( "No traversal has been made" );
			  }
			  return _lastTraverser.metadata();
		 }
	}

}
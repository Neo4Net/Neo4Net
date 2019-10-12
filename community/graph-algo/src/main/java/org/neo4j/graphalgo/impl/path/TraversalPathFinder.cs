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
namespace Org.Neo4j.Graphalgo.impl.path
{
	using Org.Neo4j.Graphalgo;
	using Node = Org.Neo4j.Graphdb.Node;
	using Path = Org.Neo4j.Graphdb.Path;
	using TraversalMetadata = Org.Neo4j.Graphdb.traversal.TraversalMetadata;
	using Traverser = Org.Neo4j.Graphdb.traversal.Traverser;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Org.Neo4j.Helpers.Collection;

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
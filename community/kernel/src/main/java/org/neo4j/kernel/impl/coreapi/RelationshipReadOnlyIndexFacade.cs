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
namespace Org.Neo4j.Kernel.impl.coreapi
{
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Org.Neo4j.Graphdb.index;
	using ReadableRelationshipIndex = Org.Neo4j.Graphdb.index.ReadableRelationshipIndex;
	using RelationshipIndex = Org.Neo4j.Graphdb.index.RelationshipIndex;

	public class RelationshipReadOnlyIndexFacade : ReadOnlyIndexFacade<Relationship>, RelationshipIndex
	{
		 private readonly ReadableRelationshipIndex @delegate;

		 public RelationshipReadOnlyIndexFacade( ReadableRelationshipIndex @delegate ) : base( @delegate )
		 {
			  this.@delegate = @delegate;
		 }

		 public override IndexHits<Relationship> Get( string key, object valueOrNull, Node startNodeOrNull, Node endNodeOrNull )
		 {
			  return @delegate.Get( key, valueOrNull, startNodeOrNull, endNodeOrNull );
		 }

		 public override IndexHits<Relationship> Query( string key, object queryOrQueryObjectOrNull, Node startNodeOrNull, Node endNodeOrNull )
		 {
			  return @delegate.Query( key, queryOrQueryObjectOrNull, startNodeOrNull, endNodeOrNull );
		 }

		 public override IndexHits<Relationship> Query( object queryOrQueryObjectOrNull, Node startNodeOrNull, Node endNodeOrNull )
		 {
			  return @delegate.Query( queryOrQueryObjectOrNull, startNodeOrNull, endNodeOrNull );
		 }
	}

}
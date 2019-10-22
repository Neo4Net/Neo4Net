﻿/*
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
namespace Neo4Net.Kernel.impl.coreapi
{
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Neo4Net.GraphDb.index;
	using ReadableRelationshipIndex = Neo4Net.GraphDb.index.ReadableRelationshipIndex;
	using RelationshipIndex = Neo4Net.GraphDb.index.RelationshipIndex;

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
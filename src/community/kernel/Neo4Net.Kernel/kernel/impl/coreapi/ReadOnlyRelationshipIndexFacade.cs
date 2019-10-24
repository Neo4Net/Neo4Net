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
namespace Neo4Net.Kernel.impl.coreapi
{
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Neo4Net.GraphDb.Index;
	using RelationshipIndex = Neo4Net.GraphDb.Index.RelationshipIndex;

	/// <summary>
	/// Wraps an explicit index to prevent writes to it - exposing it as a read-only index.
	/// </summary>
	public class ReadOnlyRelationshipIndexFacade : ReadOnlyIndexFacade<Relationship>, RelationshipIndex
	{
		 private readonly RelationshipIndex @delegate;

		 public ReadOnlyRelationshipIndexFacade( RelationshipIndex @delegate ) : base( @delegate )
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
using System;

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
namespace Org.Neo4j.Cypher.@internal.javacompat
{
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Point = Org.Neo4j.Graphdb.spatial.Point;
	using EmbeddedProxySPI = Org.Neo4j.Kernel.impl.core.EmbeddedProxySPI;
	using Org.Neo4j.Kernel.impl.util;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;
	using Values = Org.Neo4j.Values.Storable.Values;

	public class ValueToObjectSerializer : BaseToObjectValueWriter<Exception>
	{
		 private readonly _embeddedProxySPI _embeddedProxySPI;
		 public ValueToObjectSerializer( embeddedProxySPI EmbeddedProxySPI ) : base()
		 {
			  this._embeddedProxySPI = embeddedProxySPI;
		 }

		 protected internal override Node NewNodeProxyById( long id )
		 {
			  return _embeddedProxySPI.newNodeProxy( id );
		 }

		 protected internal override Relationship NewRelationshipProxyById( long id )
		 {
			  return _embeddedProxySPI.newRelationshipProxy( id );
		 }

		 protected internal override Point NewPoint( CoordinateReferenceSystem crs, double[] coordinate )
		 {
			  return Values.pointValue( crs, coordinate );
		 }
	}

}
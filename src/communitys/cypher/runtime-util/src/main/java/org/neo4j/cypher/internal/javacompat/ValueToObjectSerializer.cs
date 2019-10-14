using System;

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
namespace Neo4Net.Cypher.@internal.javacompat
{
	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Point = Neo4Net.Graphdb.spatial.Point;
	using EmbeddedProxySPI = Neo4Net.Kernel.impl.core.EmbeddedProxySPI;
	using Neo4Net.Kernel.impl.util;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using Values = Neo4Net.Values.Storable.Values;

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
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
namespace Neo4Net.Graphalgo.impl.util
{

	using Neo4Net.Graphalgo;
	using Direction = Neo4Net.Graphdb.Direction;
	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Relationship = Neo4Net.Graphdb.Relationship;

	public class WeightedPathImpl : WeightedPath
	{
		 private readonly Path _path;
		 private readonly double _weight;

		 public WeightedPathImpl( CostEvaluator<double> costEvaluator, Path path )
		 {
			  this._path = path;
			  double cost = 0;
			  foreach ( Relationship relationship in path.Relationships() )
			  {
					cost += costEvaluator.GetCost( relationship, Direction.OUTGOING );
			  }
			  this._weight = cost;
		 }

		 public WeightedPathImpl( double weight, Path path )
		 {
			  this._path = path;
			  this._weight = weight;
		 }

		 public override double Weight()
		 {
			  return _weight;
		 }

		 public override Node StartNode()
		 {
			  return _path.startNode();
		 }

		 public override Node EndNode()
		 {
			  return _path.endNode();
		 }

		 public override Relationship LastRelationship()
		 {
			  return _path.lastRelationship();
		 }

		 public override int Length()
		 {
			  return _path.length();
		 }

		 public override IEnumerable<Node> Nodes()
		 {
			  return _path.nodes();
		 }

		 public override IEnumerable<Node> ReverseNodes()
		 {
			  return _path.reverseNodes();
		 }

		 public override IEnumerable<Relationship> Relationships()
		 {
			  return _path.relationships();
		 }

		 public override IEnumerable<Relationship> ReverseRelationships()
		 {
			  return _path.reverseRelationships();
		 }

		 public override string ToString()
		 {
			  return _path.ToString() + " weight:" + this._weight;
		 }

		 public override IEnumerator<PropertyContainer> Iterator()
		 {
			  return _path.GetEnumerator();
		 }

	}

}
using System.Collections.Generic;

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
namespace Neo4Net.GraphAlgo.Utils
{

	using Neo4Net.GraphAlgo;
	using Direction = Neo4Net.GraphDb.Direction;
	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;

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
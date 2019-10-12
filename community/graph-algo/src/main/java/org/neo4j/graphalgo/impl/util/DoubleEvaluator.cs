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
namespace Org.Neo4j.Graphalgo.impl.util
{
	using Org.Neo4j.Graphalgo;
	using Direction = Org.Neo4j.Graphdb.Direction;
	using Relationship = Org.Neo4j.Graphdb.Relationship;

	public class DoubleEvaluator : CostEvaluator<double>
	{
		 private string _costPropertyName;

		 public DoubleEvaluator( string costPropertyName ) : base()
		 {
			  this._costPropertyName = costPropertyName;
		 }

		 public override double? GetCost( Relationship relationship, Direction direction )
		 {
			  object costProp = relationship.GetProperty( _costPropertyName );
			  if ( costProp is Number )
			  {
					return ( ( Number ) costProp ).doubleValue();
			  }
			  else
			  {
					return double.Parse( costProp.ToString() );
			  }
		 }
	}

}
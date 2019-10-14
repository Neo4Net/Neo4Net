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
namespace Neo4Net.Graphalgo.impl.util
{
	using Neo4Net.Graphalgo;
	using Direction = Neo4Net.Graphdb.Direction;
	using Relationship = Neo4Net.Graphdb.Relationship;

	public class IntegerEvaluator : CostEvaluator<int>
	{
		 private string _costPropertyName;

		 public IntegerEvaluator( string costPropertyName ) : base()
		 {
			  this._costPropertyName = costPropertyName;
		 }

		 public override int? GetCost( Relationship relationship, Direction direction )
		 {
			  object costProp = relationship.GetProperty( _costPropertyName );
			  if ( costProp is Number )
			  {
					return ( ( Number ) costProp ).intValue();
			  }
			  else
			  {
					return int.Parse( costProp.ToString() );
			  }
		 }
	}

}
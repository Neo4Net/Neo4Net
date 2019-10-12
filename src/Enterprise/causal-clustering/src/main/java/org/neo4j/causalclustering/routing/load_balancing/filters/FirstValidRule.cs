using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.routing.load_balancing.filters
{

	/// <summary>
	/// Each chain of filters is considered a rule and they are evaluated in order. The result
	/// of the first rule to return a valid result (non-empty set) will be the final result.
	/// </summary>
	public class FirstValidRule<T> : Filter<T>
	{
		 private IList<FilterChain<T>> _rules;

		 public FirstValidRule( IList<FilterChain<T>> rules )
		 {
			  this._rules = rules;
		 }

		 public override ISet<T> Apply( ISet<T> input )
		 {
			  ISet<T> output = input;
			  foreach ( Filter<T> chain in _rules )
			  {
					output = chain( input );
					if ( output.Count > 0 )
					{
						 break;
					}
			  }
			  return output;
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: FirstValidRule<?> that = (FirstValidRule<?>) o;
			  FirstValidRule<object> that = ( FirstValidRule<object> ) o;
			  return Objects.Equals( _rules, that._rules );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _rules );
		 }

		 public override string ToString()
		 {
			  return "FirstValidRule{" +
						"rules=" + _rules +
						'}';
		 }
	}

}
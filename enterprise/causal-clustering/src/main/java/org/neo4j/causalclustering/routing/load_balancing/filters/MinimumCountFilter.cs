﻿using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.routing.load_balancing.filters
{

	/// <summary>
	/// Only returns a valid (non-empty) result if the minimum count is met.
	/// </summary>
	public class MinimumCountFilter<T> : Filter<T>
	{
		 private readonly int _minCount;

		 public MinimumCountFilter( int minCount )
		 {
			  this._minCount = minCount;
		 }

		 public override ISet<T> Apply( ISet<T> data )
		 {
			  return data.Count >= _minCount ? data : emptySet();
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
//ORIGINAL LINE: MinimumCountFilter<?> that = (MinimumCountFilter<?>) o;
			  MinimumCountFilter<object> that = ( MinimumCountFilter<object> ) o;
			  return _minCount == that._minCount;
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _minCount );
		 }

		 public override string ToString()
		 {
			  return "MinimumCountFilter{" +
						"minCount=" + _minCount +
						'}';
		 }
	}

}
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
namespace Neo4Net.Helpers
{
	[Obsolete]
	public class Reference<T>
	{
		 private T _t;

		 [Obsolete]
		 public Reference( T initial )
		 {
			  this._t = initial;
		 }

		 [Obsolete]
		 public virtual void Set( T t )
		 {
			  this._t = t;
		 }

		 [Obsolete]
		 public virtual T Get()
		 {
			  return _t;
		 }

		 public override string ToString()
		 {
			  return _t.ToString();
		 }

		 public override bool Equals( object obj )
		 {
			  return _t.Equals( obj );
		 }

		 public override int GetHashCode()
		 {
			  return _t.GetHashCode();
		 }
	}

}
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
namespace Org.Neo4j.causalclustering.catchup
{

	public abstract class Protocol<E> where E : Enum<E>
	{
		 private E _state;

		 protected internal Protocol( E initialValue )
		 {
			  this._state = initialValue;
		 }

		 public virtual void Expect( E state )
		 {
			  this._state = state;
		 }

		 public virtual bool IsExpecting( E state )
		 {
			  return this._state == state;
		 }

		 public virtual T Select<T>( IDictionary<E, T> map )
		 {
			  return map[_state];
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name + "{" + "state=" + _state + '}';
		 }
	}

}
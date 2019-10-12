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
namespace Org.Neo4j.Kernel.impl.util.statistics
{
	/// <summary>
	/// Used as a local counter, which manages its own counter as well as delegating changes to a global counter.
	/// </summary>
	public class LocalIntCounter : IntCounter
	{
		 private readonly IntCounter _global;

		 public LocalIntCounter( IntCounter globalCounter )
		 {
			  this._global = globalCounter;
		 }

		 public override void Increment()
		 {
			  base.Increment();
			  _global.increment();
		 }

		 public override void Decrement()
		 {
			  base.Decrement();
			  _global.decrement();
		 }

		 public override void Clear()
		 {
			  base.Clear();
		 }

		 public override void Add( int delta )
		 {
			  base.Add( delta );
			  _global.add( delta );
		 }

		 public override string ToString()
		 {
			  return "local:" + base.ToString() + ",global:" + _global.ToString();
		 }
	}

}
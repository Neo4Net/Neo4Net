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
namespace Neo4Net.Kernel.Lifecycle
{
	/// <summary>
	/// Convenient use of a <seealso cref="LifeSupport"/>, effectively making one or more <seealso cref="Lifecycle"/> look and feel
	/// like one <seealso cref="AutoCloseable"/>.
	/// </summary>
	public class Lifespan : AutoCloseable
	{
		 private readonly LifeSupport _life = new LifeSupport();

		 public Lifespan( params Lifecycle[] subjects )
		 {
			  foreach ( Lifecycle subject in subjects )
			  {
					_life.add( subject );
			  }
			  _life.start();
		 }

		 public virtual T Add<T>( T subject ) where T : Lifecycle
		 {
			  return _life.add( subject );
		 }

		 public override void Close()
		 {
			  _life.shutdown();
		 }
	}

}
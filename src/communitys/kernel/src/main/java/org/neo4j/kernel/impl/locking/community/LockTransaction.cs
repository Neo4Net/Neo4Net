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
namespace Neo4Net.Kernel.impl.locking.community
{

	/// <summary>
	/// A transaction used for the sole purpose of acquiring locks via the community lock manager. This exists solely to
	/// allow using the community lock manager with the <seealso cref="Locks"/> API.
	/// </summary>
	public class LockTransaction
	{
		 private static readonly AtomicInteger _ids = new AtomicInteger( 0 );

		 private readonly int _id = _ids.AndIncrement;

		 public virtual int Id
		 {
			 get
			 {
				  return _id;
			 }
		 }

		 public override string ToString()
		 {
			  return string.Format( "LockClient[{0:D}]", _id );
		 }
	}

}
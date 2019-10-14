using System.Threading;

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
namespace Neo4Net.@unsafe.Impl.Batchimport.executor
{

	/// <summary>
	/// Strategy for waiting a while, given a certain <seealso cref="System.Threading.Thread"/>.
	/// </summary>
	public interface ParkStrategy
	{
		 void Park( Thread thread );

		 void Unpark( Thread thread );
	}

	 public class ParkStrategy_Park : ParkStrategy
	 {
		  internal readonly long Nanos;

		  public ParkStrategy_Park( long time, TimeUnit unit )
		  {
				this.Nanos = NANOSECONDS.convert( time, unit );
		  }

		  public override void Park( Thread thread )
		  {
				LockSupport.parkNanos( Nanos );
		  }

		  public override void Unpark( Thread thread )
		  {
				LockSupport.unpark( thread );
		  }
	 }

}
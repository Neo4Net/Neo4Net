using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.locking
{

	internal class LockWorkerState
	{
		 internal readonly Locks Grabber;
		 internal readonly Locks_Client Client;
		 internal volatile bool DeadlockOnLastWait;
		 internal readonly IList<string> CompletedOperations = new List<string>();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal string DoingConflict;

		 internal LockWorkerState( Locks locks )
		 {
			  this.Grabber = locks;
			  this.Client = locks.NewClient();
		 }

		 public virtual void Doing( string doing )
		 {
			  this.DoingConflict = doing;
		 }

		 public virtual void Done()
		 {
			  this.CompletedOperations.Add( this.DoingConflict );
			  this.DoingConflict = null;
		 }
	}

}
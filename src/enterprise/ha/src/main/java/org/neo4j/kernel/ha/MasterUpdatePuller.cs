/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.ha
{
	/// <summary>
	/// Masters implementation of update puller that does nothing since master should not pull updates.
	/// </summary>
	public class MasterUpdatePuller : UpdatePuller
	{

		 public static readonly MasterUpdatePuller Instance = new MasterUpdatePuller();

		 private MasterUpdatePuller()
		 {
		 }

		 public override void Start()
		 {
			  // no-op
		 }

		 public override void Stop()
		 {
			  // no-op
		 }

		 public override void PullUpdates()
		 {
			  // no-op
		 }

		 public override bool TryPullUpdates()
		 {
			  return false;
		 }

		 public override void PullUpdates( UpdatePuller_Condition condition, bool assertPullerActive )
		 {
			  // no-op
		 }
	}

}
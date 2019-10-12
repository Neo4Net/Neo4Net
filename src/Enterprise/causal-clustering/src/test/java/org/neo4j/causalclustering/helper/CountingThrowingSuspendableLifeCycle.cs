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
namespace Neo4Net.causalclustering.helper
{
	using NullLog = Neo4Net.Logging.NullLog;

	public class CountingThrowingSuspendableLifeCycle : SuspendableLifeCycle
	{
		 public CountingThrowingSuspendableLifeCycle() : base(NullLog.Instance)
		 {
		 }

		 internal int Starts;
		 internal int Stops;
		 private bool _nextShouldFail;

		 internal virtual void SetFailMode()
		 {
			  _nextShouldFail = true;
		 }

		 internal virtual void SetSuccessMode()
		 {
			  _nextShouldFail = false;
		 }

		 protected internal override void Init0()
		 {
			  HandleMode();
		 }

		 protected internal override void Start0()
		 {
			  HandleMode();
			  Starts++;
		 }

		 protected internal override void Stop0()
		 {
			  HandleMode();
			  Stops++;
		 }

		 protected internal override void Shutdown0()
		 {
			  HandleMode();
		 }

		 private void HandleMode()
		 {
			  if ( _nextShouldFail )
			  {
					throw new System.InvalidOperationException( "Tragedy" );
			  }
		 }
	}

}
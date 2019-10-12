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
namespace Neo4Net.Kernel.availability
{
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

	/// <summary>
	/// At end of startup, wait for instance to become available for transactions.
	/// <para>
	/// This helps users who expect to be able to access the instance after
	/// the constructor is run.
	/// </para>
	/// </summary>
	public class StartupWaiter : LifecycleAdapter
	{
		 private readonly AvailabilityGuard _availabilityGuard;
		 private readonly long _timeout;

		 public StartupWaiter( AvailabilityGuard availabilityGuard, long timeout )
		 {
			  this._availabilityGuard = availabilityGuard;
			  this._timeout = timeout;
		 }

		 public override void Start()
		 {
			  _availabilityGuard.isAvailable( _timeout );
		 }
	}

}
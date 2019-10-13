/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Kernel.ha.cluster.modeswitch
{
	using Neo4Net.Kernel.ha;

	/// <summary>
	/// A component switcher deals with how internal services should react when changing
	/// between different cluster states, and allows them to switch to the new state
	/// that reflects that.
	/// </summary>
	/// @param <T> </param>
	public abstract class AbstractComponentSwitcher<T> : ComponentSwitcher
	{
		 private readonly DelegateInvocationHandler<T> @delegate;

		 protected internal AbstractComponentSwitcher( DelegateInvocationHandler<T> @delegate )
		 {
			  this.@delegate = @delegate;
		 }

		 protected internal abstract T MasterImpl { get; }

		 protected internal abstract T SlaveImpl { get; }

		 protected internal virtual T PendingImpl
		 {
			 get
			 {
				  return default( T );
			 }
		 }

		 public override void SwitchToMaster()
		 {
			  UpdateDelegate( MasterImpl );
		 }

		 public override void SwitchToSlave()
		 {
			  UpdateDelegate( SlaveImpl );
		 }

		 public override void SwitchToPending()
		 {
			  UpdateDelegate( PendingImpl );
		 }

		 private void UpdateDelegate( T newValue )
		 {
			  T oldDelegate = @delegate.setDelegate( newValue );
			  ShutdownOldDelegate( oldDelegate );
			  StartNewDelegate( newValue );
		 }

		 protected internal virtual void StartNewDelegate( T newValue )
		 {
			  // no-op by default
		 }

		 protected internal virtual void ShutdownOldDelegate( T oldDelegate )
		 {
			  // no-op by default
		 }
	}

}
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
namespace Neo4Net.Kernel.ha.cluster.modeswitch
{
	using Neo4Net.Kernel.ha;

	/// <summary>
	/// UpdatePullerSwitcher will provide different implementations of <seealso cref="UpdatePuller"/>
	/// depending on node mode (slave or master).
	/// </summary>
	/// <seealso cref= UpdatePuller </seealso>
	/// <seealso cref= SlaveUpdatePuller </seealso>
	public class UpdatePullerSwitcher : AbstractComponentSwitcher<UpdatePuller>
	{
		 private readonly PullerFactory _pullerFactory;

		 public UpdatePullerSwitcher( DelegateInvocationHandler<UpdatePuller> @delegate, PullerFactory pullerFactory ) : base( @delegate )
		 {
			  this._pullerFactory = pullerFactory;
		 }

		 protected internal override UpdatePuller MasterImpl
		 {
			 get
			 {
				  return MasterUpdatePuller.INSTANCE;
			 }
		 }

		 protected internal override UpdatePuller SlaveImpl
		 {
			 get
			 {
				  return _pullerFactory.createSlaveUpdatePuller();
			 }
		 }

		 protected internal override void ShutdownOldDelegate( UpdatePuller updatePuller )
		 {
			  if ( updatePuller != null )
			  {
					updatePuller.Stop();
			  }
		 }

		 protected internal override void StartNewDelegate( UpdatePuller updatePuller )
		 {
			  if ( updatePuller != null )
			  {
					updatePuller.Start();
			  }
		 }

	}

}
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
namespace Neo4Net.Kernel.ha.cluster
{

	using InstanceId = Neo4Net.cluster.InstanceId;

	/// <summary>
	/// Context used by the <seealso cref="HighAvailabilityMemberStateMachine"/>. Keeps track of what elections and previously
	/// available master this cluster member has seen.
	/// </summary>
	public class SimpleHighAvailabilityMemberContext : HighAvailabilityMemberContext
	{
		 private InstanceId _electedMasterId;
		 private URI _availableHaMasterId;
		 private readonly InstanceId _myId;
		 private bool _slaveOnly;

		 public SimpleHighAvailabilityMemberContext( InstanceId myId, bool slaveOnly )
		 {
			  this._myId = myId;
			  this._slaveOnly = slaveOnly;
		 }

		 public virtual InstanceId MyId
		 {
			 get
			 {
				  return _myId;
			 }
		 }

		 public virtual InstanceId ElectedMasterId
		 {
			 get
			 {
				  return _electedMasterId;
			 }
			 set
			 {
				  this._electedMasterId = value;
			 }
		 }


		 public virtual URI AvailableHaMaster
		 {
			 get
			 {
				  return _availableHaMasterId;
			 }
		 }

		 public virtual URI AvailableHaMasterId
		 {
			 set
			 {
				  this._availableHaMasterId = value;
			 }
		 }

		 public virtual bool SlaveOnly
		 {
			 get
			 {
				  return _slaveOnly;
			 }
		 }

		 public override string ToString()
		 {
			  return "SimpleHighAvailabilityMemberContext{" +
						 "electedMasterId=" + _electedMasterId +
						 ", availableHaMasterId=" + _availableHaMasterId +
						 ", myId=" + _myId +
						 ", slaveOnly=" + _slaveOnly +
						 '}';
		 }
	}

}
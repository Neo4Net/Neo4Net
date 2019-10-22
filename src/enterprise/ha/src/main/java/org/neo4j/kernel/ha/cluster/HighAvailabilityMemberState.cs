using System.Collections.Generic;

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
namespace Neo4Net.Kernel.ha.cluster
{

	using InstanceId = Neo4Net.cluster.InstanceId;

	/// <summary>
	/// This represents the different states that a cluster member
	/// can have internally.
	/// 
	/// Since transitioning to master or slave can take significant time those states are explicitly modeled.
	/// 
	/// Most common transitions:
	/// PENDING -> TO_MASTER -> MASTER
	/// PENDING -> TO_SLAVE -> SLAVE
	/// MASTER/SLAVE -> PENDING
	/// </summary>
	public abstract class HighAvailabilityMemberState
	{
		 /// <summary>
		 /// This state is the initial state, and is also the state used when leaving the cluster.
		 /// <para>
		 /// Here we are waiting for events that transitions this member either to becoming a master or slave.
		 /// </para>
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       PENDING { public HighAvailabilityMemberState masterIsElected(HighAvailabilityMemberContext context, org.Neo4Net.cluster.InstanceId masterId) { assert context.getAvailableHaMaster() == null; if(masterId.equals(context.getMyId()) && !context.isSlaveOnly()) { return TO_MASTER; } return PENDING; } public HighAvailabilityMemberState masterIsAvailable(HighAvailabilityMemberContext context, org.Neo4Net.cluster.InstanceId masterId, java.net.URI masterHaURI) { if(masterId.equals(context.getMyId())) { HighAvailabilityMemberState result = ILLEGAL; result.setErrorMessage("Received a MasterIsAvailable event for my InstanceId while in" + " PENDING state"); return result; } return TO_SLAVE; } public HighAvailabilityMemberState slaveIsAvailable(HighAvailabilityMemberContext context, org.Neo4Net.cluster.InstanceId slaveId, java.net.URI slaveUri) { if(slaveId.equals(context.getMyId())) { HighAvailabilityMemberState result = ILLEGAL; result.setErrorMessage("Cannot go from pending to slave"); return result; } return this; } public boolean isEligibleForElection() { return true; } public boolean isAccessAllowed() { return false; } },

		 /// <summary>
		 /// Member now knows that a master is available, and is transitioning itself to become a slave to that master.
		 /// It is performing the transition process here, and so is not yet available as a slave.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       TO_SLAVE { public HighAvailabilityMemberState masterIsElected(HighAvailabilityMemberContext context, org.Neo4Net.cluster.InstanceId masterId) { if(masterId.equals(context.getElectedMasterId())) { return this; } if(masterId.equals(context.getMyId())) { return TO_MASTER; } return PENDING; } public HighAvailabilityMemberState masterIsAvailable(HighAvailabilityMemberContext context, org.Neo4Net.cluster.InstanceId masterId, java.net.URI masterHaURI) { if(masterId.equals(context.getMyId())) { HighAvailabilityMemberState result = ILLEGAL; result.setErrorMessage("i (" + context.getMyId() + ") am trying to become a slave but " + "someone said i am available as master"); return result; } if(masterId.equals(context.getElectedMasterId())) { return this; } HighAvailabilityMemberState result = ILLEGAL; result.setErrorMessage("my (" + context.getMyId() + ") current master is " + context.getAvailableHaMaster() + " (elected as " + context.getElectedMasterId() + " but i got a " + "masterIsAvailable event for " + masterHaURI); return result; } public HighAvailabilityMemberState slaveIsAvailable(HighAvailabilityMemberContext context, org.Neo4Net.cluster.InstanceId slaveId, java.net.URI slaveUri) { if(slaveId.equals(context.getMyId())) { return SLAVE; } return this; } public boolean isEligibleForElection() { return false; } public boolean isAccessAllowed() { return false; } },

		 /// <summary>
		 /// The cluster member knows that it has been elected as master, and starts the transitioning process.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       TO_MASTER { public HighAvailabilityMemberState masterIsElected(HighAvailabilityMemberContext context, org.Neo4Net.cluster.InstanceId masterId) { assert context.getAvailableHaMaster() == null; if(masterId.equals(context.getMyId())) { return this; } return PENDING; } public HighAvailabilityMemberState masterIsAvailable(HighAvailabilityMemberContext context, org.Neo4Net.cluster.InstanceId masterId, java.net.URI masterHaURI) { if(masterId.equals(context.getMyId())) { return MASTER; } HighAvailabilityMemberState result = ILLEGAL; result.setErrorMessage("Received a MasterIsAvailable event for instance " + masterId + " while in TO_MASTER state"); return result; } public HighAvailabilityMemberState slaveIsAvailable(HighAvailabilityMemberContext context, org.Neo4Net.cluster.InstanceId slaveId, java.net.URI slaveUri) { if(slaveId.equals(context.getMyId())) { HighAvailabilityMemberState result = ILLEGAL; result.setErrorMessage("Cannot be transitioning to master and slave at the same time"); return result; } return this; } public boolean isEligibleForElection() { return true; } public boolean isAccessAllowed() { return false; } },

		 /// <summary>
		 /// Cluster member is available as master for other cluster members to use.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       MASTER { public HighAvailabilityMemberState masterIsElected(HighAvailabilityMemberContext context, org.Neo4Net.cluster.InstanceId masterId) { if(masterId.equals(context.getMyId())) { return this; } return PENDING; } public HighAvailabilityMemberState masterIsAvailable(HighAvailabilityMemberContext context, org.Neo4Net.cluster.InstanceId masterId, java.net.URI masterHaURI) { if(masterId.equals(context.getMyId())) { return this; } HighAvailabilityMemberState result = ILLEGAL; result.setErrorMessage("I, " + context.getMyId() + " got a masterIsAvailable for " + masterHaURI + " (id is " + masterId + " ) while in MASTER state. Probably missed a " + "MasterIsElected event."); return result; } public HighAvailabilityMemberState slaveIsAvailable(HighAvailabilityMemberContext context, org.Neo4Net.cluster.InstanceId slaveId, java.net.URI slaveUri) { if(slaveId.equals(context.getMyId())) { HighAvailabilityMemberState result = ILLEGAL; result.setErrorMessage("Cannot be master and transition to slave at the same time"); return result; } return this; } public boolean isEligibleForElection() { return true; } public boolean isAccessAllowed() { return true; } },

		 /// <summary>
		 /// Cluster member is ready as a slave
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       SLAVE { public HighAvailabilityMemberState masterIsElected(HighAvailabilityMemberContext context, org.Neo4Net.cluster.InstanceId masterId) { if(masterId.equals(context.getMyId())) { return TO_MASTER; } if(masterId.equals(context.getElectedMasterId())) { return this; } return PENDING; } public HighAvailabilityMemberState masterIsAvailable(HighAvailabilityMemberContext context, org.Neo4Net.cluster.InstanceId masterId, java.net.URI masterHaURI) { if(masterId.equals(context.getMyId())) { HighAvailabilityMemberState returnValue = ILLEGAL; returnValue.setErrorMessage("Cannot transition to MASTER directly from SLAVE state"); return returnValue; } else if(masterId.equals(context.getElectedMasterId())) { return this; } HighAvailabilityMemberState returnValue = ILLEGAL; returnValue.setErrorMessage("Received a MasterIsAvailable event for " + masterId + " which is different from the current master (" + context.getElectedMasterId() + ") while in the SLAVE state (probably missed a MasterIsElected event)"); return returnValue; } public HighAvailabilityMemberState slaveIsAvailable(HighAvailabilityMemberContext context, org.Neo4Net.cluster.InstanceId slaveId, java.net.URI slaveUri) { return this; } public boolean isEligibleForElection() { return true; } public boolean isAccessAllowed() { return true; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       ILLEGAL { public HighAvailabilityMemberState masterIsElected(HighAvailabilityMemberContext context, org.Neo4Net.cluster.InstanceId masterId) { throw new IllegalStateException("The ILLEGAL state is not meant to be used as a state, merely as an indicator that" + " something went wrong while handling a message and the state should be set to PENDING"); } public HighAvailabilityMemberState masterIsAvailable(HighAvailabilityMemberContext context, org.Neo4Net.cluster.InstanceId masterId, java.net.URI masterHaURI) { throw new IllegalStateException("The ILLEGAL state is not meant to be used as a state, merely as an indicator that" + " something went wrong while handling a message and the state should be set to PENDING"); } public HighAvailabilityMemberState slaveIsAvailable(HighAvailabilityMemberContext context, org.Neo4Net.cluster.InstanceId slaveId, java.net.URI slaveUri) { throw new IllegalStateException("The ILLEGAL state is not meant to be used as a state, merely as an indicator that" + " something went wrong while handling a message and the state should be set to PENDING"); } public boolean isEligibleForElection() { throw new IllegalStateException("The ILLEGAL state is not meant to be used as a state, merely as an indicator that" + " something went wrong while handling a message and the state should be set to PENDING"); } public boolean isAccessAllowed() { throw new IllegalStateException("The ILLEGAL state is not meant to be used as a state, merely as an indicator that" + " something went wrong while handling a message and the state should be set to PENDING"); } };

		 private static readonly IList<HighAvailabilityMemberState> valueList = new List<HighAvailabilityMemberState>();

		 static HighAvailabilityMemberState()
		 {
			 valueList.Add( PENDING );
			 valueList.Add( TO_SLAVE );
			 valueList.Add( TO_MASTER );
			 valueList.Add( MASTER );
			 valueList.Add( SLAVE );
			 valueList.Add( ILLEGAL );
		 }

		 public enum InnerEnum
		 {
			 PENDING,
			 TO_SLAVE,
			 TO_MASTER,
			 MASTER,
			 SLAVE,
			 ILLEGAL
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private HighAvailabilityMemberState( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 private string errorMessage = "";

		 public abstract HighAvailabilityMemberState masterIsElected( HighAvailabilityMemberContext context, Neo4Net.cluster.InstanceId masterId );

		 public abstract HighAvailabilityMemberState masterIsAvailable( HighAvailabilityMemberContext context, Neo4Net.cluster.InstanceId masterId, java.net.URI masterHaURI );

		 public abstract HighAvailabilityMemberState slaveIsAvailable( HighAvailabilityMemberContext context, Neo4Net.cluster.InstanceId slaveId, java.net.URI slaveUri );

		 /// <summary>
		 /// The purpose of this is that an instance cannot vote in an election while becoming a slave,
		 /// as it is copying stores.
		 /// </summary>
		 /// <returns> whether the instance is eligible or not </returns>
		 public abstract bool isEligibleForElection();

		 public abstract bool isAccessAllowed();

		 public static readonly HighAvailabilityMemberState public String errorMessage() { return errorMessage; } private void setErrorMessage(String message) { errorMessage = message; } = new HighAvailabilityMemberState("public String errorMessage() { return errorMessage; } private void setErrorMessage(String message) { errorMessage = message; }", InnerEnum.public String errorMessage() { return errorMessage; } private void setErrorMessage(String message) { errorMessage = message; });

		public static IList<HighAvailabilityMemberState> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static HighAvailabilityMemberState valueOf( string name )
		{
			foreach ( HighAvailabilityMemberState enumInstance in HighAvailabilityMemberState.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}
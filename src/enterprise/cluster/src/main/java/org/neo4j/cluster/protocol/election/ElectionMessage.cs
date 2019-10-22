﻿/*
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
namespace Neo4Net.cluster.protocol.election
{

	using MessageType = Neo4Net.cluster.com.message.MessageType;

	/// <summary>
	/// Messages used to implement the <seealso cref="ElectionState"/>
	/// </summary>
	public enum ElectionMessage
	{
		 Created,
		 Join,
		 Leave,
		 Demote,
		 PerformRoleElections,
		 Vote,
		 ElectionTimeout,
		 Voted

//JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
//		 public static class VotedData implements java.io.Serializable
	//	 {
	//		  private static final long serialVersionUID = 6115474263667086327L;
	//
	//		  private String role;
	//		  private InstanceId instanceId;
	//		  private ElectionCredentials voteCredentials;
	//
	//		  public VotedData(String role, InstanceId instanceId, ElectionCredentials electionCredentials)
	//		  {
	//				this.role = role;
	//				this.instanceId = instanceId;
	//				this.voteCredentials = electionCredentials;
	//		  }
	//
	//		  public String getRole()
	//		  {
	//				return role;
	//		  }
	//
	//		  public InstanceId getInstanceId()
	//		  {
	//				return instanceId;
	//		  }
	//
	//		  public ElectionCredentials getElectionCredentials()
	//		  {
	//				return voteCredentials;
	//		  }
	//
	//		  @@Override public String toString()
	//		  {
	//				return getClass().getSimpleName() + "[role:" + role + ", instance:" + instanceId + ", credentials:" + voteCredentials + ", " + getImplementationSpecificDetails() + "]";
	//		  }
	//
	//		  protected String getImplementationSpecificDetails()
	//		  {
	//				return "";
	//		  }
	//	 }

//JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
//		 public static class VersionedVotedData extends VotedData
	//	 {
	//		  private static final long serialVersionUID = -3795472557085578559L;
	//
	//		  private long version;
	//
	//		  public VersionedVotedData(String role, InstanceId instanceId, ElectionCredentials electionCredentials, long version)
	//		  {
	//				base(role, instanceId, electionCredentials);
	//				this.version = version;
	//		  }
	//
	//		  public long getVersion()
	//		  {
	//				return version;
	//		  }
	//
	//		  @@Override protected String getImplementationSpecificDetails()
	//		  {
	//				return "version: " + version;
	//		  }
	//	 }
	}

}
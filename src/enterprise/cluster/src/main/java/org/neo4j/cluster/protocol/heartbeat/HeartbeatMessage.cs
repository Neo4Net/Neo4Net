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
namespace Neo4Net.cluster.protocol.heartbeat
{

	using MessageType = Neo4Net.cluster.com.message.MessageType;

	/// <summary>
	/// Messages used by the <seealso cref="HeartbeatState"/> state machine.
	/// </summary>
	public enum HeartbeatMessage
	{
		 // Heartbeat API messages
		 AddHeartbeatListener,
		 RemoveHeartbeatListener,

		 // Protocol messages
		 Join,
		 Leave,
		 IAmAlive,
		 TimedOut,
		 SendHeartbeat,
		 ResetSendHeartbeat,
		 Suspicions

//JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
//		 public static class IAmAliveState implements java.io.Serializable
	//	 {
	//		  private static final long serialVersionUID = 6799806932628197123L;
	//
	//		  private InstanceId server;
	//
	//		  public IAmAliveState(InstanceId server)
	//		  {
	//				this.server = server;
	//		  }
	//
	//		  public InstanceId getServer()
	//		  {
	//				return server;
	//		  }
	//
	//		  @@Override public String toString()
	//		  {
	//				return "i_am_alive[" + server + "]";
	//		  }
	//
	//		  @@Override public boolean equals(Object o)
	//		  {
	//				if (this == o)
	//				{
	//					 return true;
	//				}
	//				if (o == null || getClass() != o.getClass())
	//				{
	//					 return false;
	//				}
	//
	//				IAmAliveState that = (IAmAliveState) o;
	//
	//				return server != null ? server.equals(that.server) : that.server == null;
	//		  }
	//
	//		  @@Override public int hashCode()
	//		  {
	//				return server != null ? server.hashCode() : 0;
	//		  }
	//	 }

//JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
//		 public static class SuspicionsState implements java.io.Serializable
	//	 {
	//
	//		  private static final long serialVersionUID = 3152836192116904427L;
	//
	//		  private Set<InstanceId> suspicions;
	//
	//		  public SuspicionsState(Set<InstanceId> suspicions)
	//		  {
	//				this.suspicions = suspicions;
	//		  }
	//
	//		  public Set<InstanceId> getSuspicions()
	//		  {
	//				return suspicions;
	//		  }
	//
	//		  @@Override public String toString()
	//		  {
	//				return "Suspicions:" + suspicions;
	//		  }
	//
	//		  @@Override public boolean equals(Object o)
	//		  {
	//				if (this == o)
	//				{
	//					 return true;
	//				}
	//				if (o == null || getClass() != o.getClass())
	//				{
	//					 return false;
	//				}
	//
	//				SuspicionsState that = (SuspicionsState) o;
	//
	//				return suspicions != null ? suspicions.equals(that.suspicions) : that.suspicions == null;
	//		  }
	//
	//		  @@Override public int hashCode()
	//		  {
	//				return suspicions != null ? suspicions.hashCode() : 0;
	//		  }
	//	 }
	}

}
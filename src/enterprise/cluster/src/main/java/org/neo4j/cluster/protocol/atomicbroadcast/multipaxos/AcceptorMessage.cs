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
namespace Neo4Net.cluster.protocol.atomicbroadcast.multipaxos
{

	using MessageType = Neo4Net.cluster.com.message.MessageType;

	/// <summary>
	/// Acceptor state machine messages
	/// </summary>
	public enum AcceptorMessage
	{
		 Failure,
		 Join,
		 Leave,
		 Prepare, // phase 1a/1b
		 Accept // phase 2a/2b - timeout if resulting learn is not fast enough

//JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
//		 public static class PrepareState implements java.io.Serializable
	//	 {
	//		  private static final long serialVersionUID = 7179066752672770593L;
	//
	//		  private long ballot;
	//
	//		  public PrepareState(long ballot)
	//		  {
	//				this.ballot = ballot;
	//		  }
	//
	//		  public long getBallot()
	//		  {
	//				return ballot;
	//		  }
	//
	//		  @@Override public String toString()
	//		  {
	//				return "PrepareState{" +
	//						  "ballot=" + ballot +
	//						  '}';
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
	//				PrepareState that = (PrepareState) o;
	//
	//				return ballot == that.ballot;
	//		  }
	//
	//		  @@Override public int hashCode()
	//		  {
	//				return (int)(ballot ^ (ballot >>> 32));
	//		  }
	//	 }

//JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
//		 public static class AcceptState implements java.io.Serializable
	//	 {
	//		  private static final long serialVersionUID = -5510569299948660967L;
	//
	//		  private long ballot;
	//		  private Object value;
	//
	//		  public AcceptState(long ballot, Object value)
	//		  {
	//				this.ballot = ballot;
	//				this.value = value;
	//		  }
	//
	//		  public long getBallot()
	//		  {
	//				return ballot;
	//		  }
	//
	//		  public Object getValue()
	//		  {
	//				return value;
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
	//				AcceptState that = (AcceptState) o;
	//
	//				if (ballot != that.ballot)
	//				{
	//					 return false;
	//				}
	//				return value != null ? value.equals(that.value) : that.value == null;
	//		  }
	//
	//		  @@Override public int hashCode()
	//		  {
	//				int result = (int)(ballot ^ (ballot >>> 32));
	//				result = 31 * result + (value != null ? value.hashCode() : 0);
	//				return result;
	//		  }
	//
	//		  @@Override public String toString()
	//		  {
	//				Object toStringValue = value;
	//				if (toStringValue instanceof Payload)
	//				{
	//					 try
	//					 {
	//						  toStringValue = new AtomicBroadcastSerializer(new ObjectStreamFactory(), new ObjectStreamFactory()).receive((Payload) toStringValue);
	//					 }
	//					 catch (Throwable e)
	//					 {
	//						  e.printStackTrace();
	//					 }
	//				}
	//
	//				return "AcceptState{" + "ballot=" + ballot + ", value=" + toStringValue + "}";
	//		  }
	//	 }
	}

}
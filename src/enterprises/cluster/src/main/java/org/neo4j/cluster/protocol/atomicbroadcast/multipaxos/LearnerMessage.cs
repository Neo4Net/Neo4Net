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
namespace Neo4Net.cluster.protocol.atomicbroadcast.multipaxos
{

	using MessageType = Neo4Net.cluster.com.message.MessageType;

	/// <summary>
	/// Learner state machine messages
	/// </summary>
	public enum LearnerMessage
	{
		 Join,
		 Leave,
		 Learn,
		 LearnRequest,
		 LearnFailed,
		 LearnTimedout,
		 CatchUp

//JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
//		 public static class LearnState implements java.io.Serializable
	//	 {
	//		  private static final long serialVersionUID = 3311287172384025589L;
	//
	//		  private Object value;
	//
	//		  public LearnState(Object value)
	//		  {
	//				this.value = value;
	//		  }
	//
	//		  public Object getValue()
	//		  {
	//				return value;
	//		  }
	//
	//		  @@Override public String toString()
	//		  {
	//				if (value instanceof Payload)
	//				{
	//					 try
	//					 {
	//						  ObjectStreamFactory streamFactory = new ObjectStreamFactory();
	//						  return new AtomicBroadcastSerializer(streamFactory, streamFactory).receive((Payload) value).toString();
	//					 }
	//					 catch (Throwable e)
	//					 {
	//						  return value.toString();
	//					 }
	//				}
	//				return value.toString();
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
	//				LearnState that = (LearnState) o;
	//
	//				return value != null ? value.equals(that.value) : that.value == null;
	//		  }
	//
	//		  @@Override public int hashCode()
	//		  {
	//				return value != null ? value.hashCode() : 0;
	//		  }
	//	 }

//JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
//		 public static class LearnRequestState implements java.io.Serializable
	//	 {
	//		  private static final long serialVersionUID = -2577225800895578365L;
	//
	//		  public LearnRequestState()
	//		  {
	//		  }
	//
	//		  @@Override public boolean equals(Object obj)
	//		  {
	//				return obj != null && getClass() == obj.getClass();
	//		  }
	//
	//		  @@Override public int hashCode()
	//		  {
	//				return 1;
	//		  }
	//
	//		  @@Override public String toString()
	//		  {
	//				return "Learn request";
	//		  }
	//	 }

//JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
//		 public static class LearnFailedState implements java.io.Serializable
	//	 {
	//		  private static final long serialVersionUID = -6587635550010611226L;
	//
	//		  public LearnFailedState()
	//		  {
	//		  }
	//
	//		  @@Override public String toString()
	//		  {
	//				return "Learn failed";
	//		  }
	//
	//		  @@Override public int hashCode()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public boolean equals(Object obj)
	//		  {
	//				return obj instanceof LearnFailedState;
	//		  }
	//	 }
	}

}
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
namespace Org.Neo4j.Kernel.impl.transaction.tracing
{
	/// <summary>
	/// Represents the process of turning the state of a committing transaction into a sequence of commands, and appending
	/// them to the transaction log.
	/// </summary>
	public interface LogAppendEvent : LogForceEvents, AutoCloseable
	{

		 /// <summary>
		 /// Mark the end of the process of appending a transaction to the transaction log.
		 /// </summary>
		 void Close();

		 /// <summary>
		 /// Note whether or not the log was rotated by the appending of this transaction to the log.
		 /// </summary>
		 bool LogRotated { set; }

		 /// <summary>
		 /// Begin a log rotation as part of this appending to the transaction log.
		 /// </summary>
		 LogRotateEvent BeginLogRotate();

		 /// <summary>
		 /// Begin serializing and writing out the commands for this transaction.
		 /// </summary>
		 SerializeTransactionEvent BeginSerializeTransaction();
	}

	public static class LogAppendEvent_Fields
	{
		 public static readonly LogAppendEvent Null = new Empty();
	}

	 public class LogAppendEvent_Empty : LogAppendEvent
	 {
		  public override void Close()
		  {
		  }

		  public virtual bool LogRotated
		  {
			  set
			  {
   
			  }
		  }

		  public override LogRotateEvent BeginLogRotate()
		  {
				return LogRotateEvent_Fields.Null;
		  }

		  public override SerializeTransactionEvent BeginSerializeTransaction()
		  {
				return SerializeTransactionEvent_Fields.Null;
		  }

		  public override LogForceWaitEvent BeginLogForceWait()
		  {
				return LogForceWaitEvent_Fields.Null;
		  }

		  public override LogForceEvent BeginLogForce()
		  {
				return LogForceEvent_Fields.Null;
		  }
	 }

}
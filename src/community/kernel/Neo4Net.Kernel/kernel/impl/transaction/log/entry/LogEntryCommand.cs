/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.impl.transaction.log.entry
{
	using StorageCommand = Neo4Net.Storageengine.Api.StorageCommand;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.entry.LogEntryByteCodes.COMMAND;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.entry.LogEntryVersion.CURRENT;

	public class LogEntryCommand : AbstractLogEntry
	{
		 private readonly StorageCommand _command;

		 public LogEntryCommand( StorageCommand command ) : this( CURRENT, command )
		 {
		 }

		 public LogEntryCommand( LogEntryVersion version, StorageCommand command ) : base( version, COMMAND )
		 {
			  this._command = command;
		 }

		 public virtual StorageCommand Command
		 {
			 get
			 {
				  return _command;
			 }
		 }

		 public override string ToString()
		 {
			  return "Command[" + _command + "]";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public <T extends LogEntry> T as()
		 public override T As<T>() where T : LogEntry
		 {
			  return ( T ) this;
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  LogEntryCommand command1 = ( LogEntryCommand ) o;
			  return _command.Equals( command1._command );
		 }

		 public override int GetHashCode()
		 {
			  return _command.GetHashCode();
		 }
	}

}
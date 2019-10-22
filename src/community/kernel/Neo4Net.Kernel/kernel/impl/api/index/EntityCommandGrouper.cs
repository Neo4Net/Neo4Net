using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Api.index
{

	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using NodeCommand = Neo4Net.Kernel.impl.transaction.command.Command.NodeCommand;
	using PropertyCommand = Neo4Net.Kernel.impl.transaction.command.Command.PropertyCommand;
	using RelationshipCommand = Neo4Net.Kernel.impl.transaction.command.Command.RelationshipCommand;

	/// <summary>
	/// Groups property commands by IEntity. The commands are provided from a list of transaction commands.
	/// Most IEntity updates include both the IEntity command as well as property commands, but sometimes
	/// only property commands for an IEntity exists in the list and this grouper handles both scenarios.
	/// Commands are appended to an array and then sorted before handed over for being processed.
	/// Hence one IEntity group can look like any of these combinations:
	/// <ul>
	///     <li>Entity command (<seealso cref="NodeCommand"/> or <seealso cref="RelationshipCommand"/> followed by zero or more <seealso cref="PropertyCommand property commands"/>
	///     for that IEntity</li>
	///     <li>zero or more <seealso cref="PropertyCommand property commands"/>, all for the same node</li>
	/// </ul>
	/// <para>
	/// Typical interaction goes like this:
	/// <ol>
	///     <li>All commands are added with <seealso cref="add(Command)"/></li>
	///     <li>Get a cursor to the sorted commands using <seealso cref="sortAndAccessGroups()"/></li>
	///     <li>Call <seealso cref="clear()"/> and use this instance again for another set of commands</li>
	/// </ol>
	/// </para>
	/// </summary>
	public class IEntityCommandGrouper<ENTITY> where IEntity : Neo4Net.Kernel.impl.transaction.command.Command
	{
		 /// <summary>
		 /// Enforces the order described on the class-level javadoc above.
		 /// </summary>
		 private readonly IComparer<Command> COMMAND_COMPARATOR = new ComparatorAnonymousInnerClass();

		 private class ComparatorAnonymousInnerClass : IComparer<Command>
		 {
			 public int compare( Command o1, Command o2 )
			 {
				  int IEntityIdComparison = Long.compare( IEntityId( o1 ), IEntityId( o2 ) );
				  return IEntityIdComparison != 0 ? IEntityIdComparison : Integer.compare( commandType( o1 ), commandType( o2 ) );
			 }

			 private long IEntityId( Command command )
			 {
				  if ( command.GetType() == outerInstance.entityCommandClass )
				  {
						return command.Key;
				  }
				  return ( ( Command.PropertyCommand ) command ).EntityId;
			 }

			 private int commandType( Command command )
			 {
				  return command.GetType() == outerInstance.entityCommandClass ? 0 : 1;
			 }
		 }

		 private readonly Type<ENTITY> _entityCommandClass;
		 private Command[] _commands;
		 private int _writeCursor;

		 public IEntityCommandGrouper( Type IEntityCommandClass, int sizeHint )
		 {
				 IEntityCommandClass = typeof( IEntity );
			  this._entityCommandClass = IEntityCommandClass;
			  this._commands = new Command[sizeHint];
		 }

		 public virtual void Add( Command command )
		 {
			  if ( _writeCursor == _commands.Length )
			  {
					_commands = Arrays.copyOf( _commands, _commands.Length * 2 );
			  }
			  _commands[_writeCursor++] = command;
		 }

		 public virtual Cursor SortAndAccessGroups()
		 {
			  Arrays.sort( _commands, 0, _writeCursor, COMMAND_COMPARATOR );
			  return new Cursor( this );
		 }

		 public virtual void Clear()
		 {
			  if ( _writeCursor > 1_000 )
			  {
					// Don't continue to hog large transactions
					Arrays.fill( _commands, 1_000, _writeCursor, null );
			  }
			  _writeCursor = 0;
		 }

		 /// <summary>
		 /// Interaction goes like this:
		 /// <ol>
		 ///     <li>Call <seealso cref="nextEntity()"/> to go to the next group, if any</li>
		 ///     <li>A group may or may not have the IEntity command, as accessed by <seealso cref="currentEntityCommand()"/>,
		 ///         either way the IEntity id is accessible using <seealso cref="currentEntityId()"/></li>
		 ///     <li>Call <seealso cref="nextProperty()"/> until it returns null, now all the <seealso cref="PropertyCommand"/> in this group have been accessed</li>
		 /// </ol>
		 /// </summary>
		 public class Cursor
		 {
			 private readonly IEntityCommandGrouper<ENTITY> _outerInstance;

			 public Cursor( IEntityCommandGrouper<ENTITY> outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal int ReadCursor;
			  internal long CurrentEntity;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal IEntity CurrentEntityCommandConflict;

			  public virtual bool NextEntity()
			  {
					if ( ReadCursor >= outerInstance.writeCursor )
					{
						 return false;
					}

					if ( outerInstance.commands[ReadCursor].GetType() == outerInstance.entityCommandClass )
					{
						 CurrentEntityCommandConflict = ( IEntity ) outerInstance.commands[ReadCursor++];
						 CurrentEntity = CurrentEntityCommandConflict.Key;
					}
					else
					{
						 Command.PropertyCommand firstPropertyCommand = ( Command.PropertyCommand ) outerInstance.commands[ReadCursor];
						 CurrentEntityCommandConflict = default( IEntity );
						 CurrentEntity = firstPropertyCommand.EntityId;
					}
					return true;
			  }

			  public virtual Command.PropertyCommand NextProperty()
			  {
					if ( ReadCursor < outerInstance.writeCursor )
					{
						 Command command = outerInstance.commands[ReadCursor];
						 if ( command is Command.PropertyCommand && ( ( Command.PropertyCommand ) command ).EntityId == CurrentEntity )
						 {
							  ReadCursor++;
							  return ( Command.PropertyCommand ) command;
						 }
					}
					return null;
			  }

			  public virtual long CurrentEntityId()
			  {
					return CurrentEntity;
			  }

			  public virtual IEntity CurrentEntityCommand()
			  {
					return CurrentEntityCommandConflict;
			  }
		 }
	}

}
﻿/*
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
namespace Neo4Net.Kernel.Impl.Api
{

	using AddNodeCommand = Neo4Net.Kernel.impl.index.IndexCommand.AddNodeCommand;
	using AddRelationshipCommand = Neo4Net.Kernel.impl.index.IndexCommand.AddRelationshipCommand;
	using CreateCommand = Neo4Net.Kernel.impl.index.IndexCommand.CreateCommand;
	using DeleteCommand = Neo4Net.Kernel.impl.index.IndexCommand.DeleteCommand;
	using RemoveCommand = Neo4Net.Kernel.impl.index.IndexCommand.RemoveCommand;
	using IndexDefineCommand = Neo4Net.Kernel.impl.index.IndexDefineCommand;
	using LabelTokenCommand = Neo4Net.Kernel.impl.transaction.command.Command.LabelTokenCommand;
	using NeoStoreCommand = Neo4Net.Kernel.impl.transaction.command.Command.NeoStoreCommand;
	using NodeCommand = Neo4Net.Kernel.impl.transaction.command.Command.NodeCommand;
	using NodeCountsCommand = Neo4Net.Kernel.impl.transaction.command.Command.NodeCountsCommand;
	using PropertyCommand = Neo4Net.Kernel.impl.transaction.command.Command.PropertyCommand;
	using PropertyKeyTokenCommand = Neo4Net.Kernel.impl.transaction.command.Command.PropertyKeyTokenCommand;
	using RelationshipCommand = Neo4Net.Kernel.impl.transaction.command.Command.RelationshipCommand;
	using RelationshipCountsCommand = Neo4Net.Kernel.impl.transaction.command.Command.RelationshipCountsCommand;
	using RelationshipGroupCommand = Neo4Net.Kernel.impl.transaction.command.Command.RelationshipGroupCommand;
	using RelationshipTypeTokenCommand = Neo4Net.Kernel.impl.transaction.command.Command.RelationshipTypeTokenCommand;
	using SchemaRuleCommand = Neo4Net.Kernel.impl.transaction.command.Command.SchemaRuleCommand;

	/// <summary>
	/// An interface for dealing with commands, either reading or writing them. See also <seealso cref="TransactionApplier"/>. The
	/// methods in this class should almost always return false, unless something went wrong.
	/// </summary>
	public interface ICommandVisitor
	{
		 // Store commands
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean visitNodeCommand(Neo4Net.kernel.impl.transaction.command.Command.NodeCommand command) throws java.io.IOException;
		 bool VisitNodeCommand( NodeCommand command );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean visitRelationshipCommand(Neo4Net.kernel.impl.transaction.command.Command.RelationshipCommand command) throws java.io.IOException;
		 bool VisitRelationshipCommand( RelationshipCommand command );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean visitPropertyCommand(Neo4Net.kernel.impl.transaction.command.Command.PropertyCommand command) throws java.io.IOException;
		 bool VisitPropertyCommand( PropertyCommand command );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean visitRelationshipGroupCommand(Neo4Net.kernel.impl.transaction.command.Command.RelationshipGroupCommand command) throws java.io.IOException;
		 bool VisitRelationshipGroupCommand( RelationshipGroupCommand command );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean visitRelationshipTypeTokenCommand(Neo4Net.kernel.impl.transaction.command.Command.RelationshipTypeTokenCommand command) throws java.io.IOException;
		 bool VisitRelationshipTypeTokenCommand( RelationshipTypeTokenCommand command );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean visitLabelTokenCommand(Neo4Net.kernel.impl.transaction.command.Command.LabelTokenCommand command) throws java.io.IOException;
		 bool VisitLabelTokenCommand( LabelTokenCommand command );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean visitPropertyKeyTokenCommand(Neo4Net.kernel.impl.transaction.command.Command.PropertyKeyTokenCommand command) throws java.io.IOException;
		 bool VisitPropertyKeyTokenCommand( PropertyKeyTokenCommand command );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean visitSchemaRuleCommand(Neo4Net.kernel.impl.transaction.command.Command.SchemaRuleCommand command) throws java.io.IOException;
		 bool VisitSchemaRuleCommand( SchemaRuleCommand command );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean visitNeoStoreCommand(Neo4Net.kernel.impl.transaction.command.Command.NeoStoreCommand command) throws java.io.IOException;
		 bool VisitNeoStoreCommand( NeoStoreCommand command );

		 // Index commands
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean visitIndexAddNodeCommand(Neo4Net.kernel.impl.index.IndexCommand.AddNodeCommand command) throws java.io.IOException;
		 bool VisitIndexAddNodeCommand( AddNodeCommand command );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean visitIndexAddRelationshipCommand(Neo4Net.kernel.impl.index.IndexCommand.AddRelationshipCommand command) throws java.io.IOException;
		 bool VisitIndexAddRelationshipCommand( AddRelationshipCommand command );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean visitIndexRemoveCommand(Neo4Net.kernel.impl.index.IndexCommand.RemoveCommand command) throws java.io.IOException;
		 bool VisitIndexRemoveCommand( RemoveCommand command );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean visitIndexDeleteCommand(Neo4Net.kernel.impl.index.IndexCommand.DeleteCommand command) throws java.io.IOException;
		 bool VisitIndexDeleteCommand( DeleteCommand command );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean visitIndexCreateCommand(Neo4Net.kernel.impl.index.IndexCommand.CreateCommand command) throws java.io.IOException;
		 bool VisitIndexCreateCommand( CreateCommand command );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean visitIndexDefineCommand(Neo4Net.kernel.impl.index.IndexDefineCommand command) throws java.io.IOException;
		 bool VisitIndexDefineCommand( IndexDefineCommand command );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean visitNodeCountsCommand(Neo4Net.kernel.impl.transaction.command.Command.NodeCountsCommand command) throws java.io.IOException;
		 bool VisitNodeCountsCommand( NodeCountsCommand command );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean visitRelationshipCountsCommand(Neo4Net.kernel.impl.transaction.command.Command.RelationshipCountsCommand command) throws java.io.IOException;
		 bool VisitRelationshipCountsCommand( RelationshipCountsCommand command );

		 /// <summary>
		 /// An empty implementation of a <seealso cref="CommandVisitor"/>. Allows you to implement only the methods you are
		 /// interested in. See also <seealso cref="TransactionApplier.Adapter"/> if need handle commands inside of a transaction, or
		 /// have a lock.
		 /// </summary>

		 /// <summary>
		 /// Wraps a given <seealso cref="CommandVisitor"/>, allowing you to do some extra operations before/after/instead of the
		 /// delegate executes.
		 /// </summary>
	}

	 public class CommandVisitor_Adapter : CommandVisitor
	 {

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitNodeCommand(Neo4Net.kernel.impl.transaction.command.Command.NodeCommand command) throws java.io.IOException
		  public override bool VisitNodeCommand( NodeCommand command )
		  {
				return false;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitRelationshipCommand(Neo4Net.kernel.impl.transaction.command.Command.RelationshipCommand command) throws java.io.IOException
		  public override bool VisitRelationshipCommand( RelationshipCommand command )
		  {
				return false;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitPropertyCommand(Neo4Net.kernel.impl.transaction.command.Command.PropertyCommand command) throws java.io.IOException
		  public override bool VisitPropertyCommand( PropertyCommand command )
		  {
				return false;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitRelationshipGroupCommand(Neo4Net.kernel.impl.transaction.command.Command.RelationshipGroupCommand command) throws java.io.IOException
		  public override bool VisitRelationshipGroupCommand( RelationshipGroupCommand command )
		  {
				return false;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitRelationshipTypeTokenCommand(Neo4Net.kernel.impl.transaction.command.Command.RelationshipTypeTokenCommand command) throws java.io.IOException
		  public override bool VisitRelationshipTypeTokenCommand( RelationshipTypeTokenCommand command )
		  {
				return false;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitLabelTokenCommand(Neo4Net.kernel.impl.transaction.command.Command.LabelTokenCommand command) throws java.io.IOException
		  public override bool VisitLabelTokenCommand( LabelTokenCommand command )
		  {
				return false;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitPropertyKeyTokenCommand(Neo4Net.kernel.impl.transaction.command.Command.PropertyKeyTokenCommand command) throws java.io.IOException
		  public override bool VisitPropertyKeyTokenCommand( PropertyKeyTokenCommand command )
		  {
				return false;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitSchemaRuleCommand(Neo4Net.kernel.impl.transaction.command.Command.SchemaRuleCommand command) throws java.io.IOException
		  public override bool VisitSchemaRuleCommand( SchemaRuleCommand command )
		  {
				return false;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitNeoStoreCommand(Neo4Net.kernel.impl.transaction.command.Command.NeoStoreCommand command) throws java.io.IOException
		  public override bool VisitNeoStoreCommand( NeoStoreCommand command )
		  {
				return false;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexAddNodeCommand(Neo4Net.kernel.impl.index.IndexCommand.AddNodeCommand command) throws java.io.IOException
		  public override bool VisitIndexAddNodeCommand( AddNodeCommand command )
		  {
				return false;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexAddRelationshipCommand(Neo4Net.kernel.impl.index.IndexCommand.AddRelationshipCommand command) throws java.io.IOException
		  public override bool VisitIndexAddRelationshipCommand( AddRelationshipCommand command )
		  {
				return false;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexRemoveCommand(Neo4Net.kernel.impl.index.IndexCommand.RemoveCommand command) throws java.io.IOException
		  public override bool VisitIndexRemoveCommand( RemoveCommand command )
		  {
				return false;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexDeleteCommand(Neo4Net.kernel.impl.index.IndexCommand.DeleteCommand command) throws java.io.IOException
		  public override bool VisitIndexDeleteCommand( DeleteCommand command )
		  {
				return false;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexCreateCommand(Neo4Net.kernel.impl.index.IndexCommand.CreateCommand command) throws java.io.IOException
		  public override bool VisitIndexCreateCommand( CreateCommand command )
		  {
				return false;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexDefineCommand(Neo4Net.kernel.impl.index.IndexDefineCommand command) throws java.io.IOException
		  public override bool VisitIndexDefineCommand( IndexDefineCommand command )
		  {
				return false;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitNodeCountsCommand(Neo4Net.kernel.impl.transaction.command.Command.NodeCountsCommand command) throws java.io.IOException
		  public override bool VisitNodeCountsCommand( NodeCountsCommand command )
		  {
				return false;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitRelationshipCountsCommand(Neo4Net.kernel.impl.transaction.command.Command.RelationshipCountsCommand command) throws java.io.IOException
		  public override bool VisitRelationshipCountsCommand( RelationshipCountsCommand command )
		  {
				return false;
		  }
	 }

	 public class CommandVisitor_Delegator : CommandVisitor
	 {
		  internal readonly CommandVisitor Delegate;

		  public CommandVisitor_Delegator( CommandVisitor @delegate )
		  {
				this.Delegate = @delegate;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitNodeCommand(Neo4Net.kernel.impl.transaction.command.Command.NodeCommand command) throws java.io.IOException
		  public override bool VisitNodeCommand( NodeCommand command )
		  {
				return Delegate.visitNodeCommand( command );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitRelationshipCommand(Neo4Net.kernel.impl.transaction.command.Command.RelationshipCommand command) throws java.io.IOException
		  public override bool VisitRelationshipCommand( RelationshipCommand command )
		  {
				return Delegate.visitRelationshipCommand( command );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitPropertyCommand(Neo4Net.kernel.impl.transaction.command.Command.PropertyCommand command) throws java.io.IOException
		  public override bool VisitPropertyCommand( PropertyCommand command )
		  {
				return Delegate.visitPropertyCommand( command );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitRelationshipGroupCommand(Neo4Net.kernel.impl.transaction.command.Command.RelationshipGroupCommand command) throws java.io.IOException
		  public override bool VisitRelationshipGroupCommand( RelationshipGroupCommand command )
		  {
				return Delegate.visitRelationshipGroupCommand( command );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitRelationshipTypeTokenCommand(Neo4Net.kernel.impl.transaction.command.Command.RelationshipTypeTokenCommand command) throws java.io.IOException
		  public override bool VisitRelationshipTypeTokenCommand( RelationshipTypeTokenCommand command )
		  {
				return Delegate.visitRelationshipTypeTokenCommand( command );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitLabelTokenCommand(Neo4Net.kernel.impl.transaction.command.Command.LabelTokenCommand command) throws java.io.IOException
		  public override bool VisitLabelTokenCommand( LabelTokenCommand command )
		  {
				return Delegate.visitLabelTokenCommand( command );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitPropertyKeyTokenCommand(Neo4Net.kernel.impl.transaction.command.Command.PropertyKeyTokenCommand command) throws java.io.IOException
		  public override bool VisitPropertyKeyTokenCommand( PropertyKeyTokenCommand command )
		  {
				return Delegate.visitPropertyKeyTokenCommand( command );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitSchemaRuleCommand(Neo4Net.kernel.impl.transaction.command.Command.SchemaRuleCommand command) throws java.io.IOException
		  public override bool VisitSchemaRuleCommand( SchemaRuleCommand command )
		  {
				return Delegate.visitSchemaRuleCommand( command );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitNeoStoreCommand(Neo4Net.kernel.impl.transaction.command.Command.NeoStoreCommand command) throws java.io.IOException
		  public override bool VisitNeoStoreCommand( NeoStoreCommand command )
		  {
				return Delegate.visitNeoStoreCommand( command );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexAddNodeCommand(Neo4Net.kernel.impl.index.IndexCommand.AddNodeCommand command) throws java.io.IOException
		  public override bool VisitIndexAddNodeCommand( AddNodeCommand command )
		  {
				return Delegate.visitIndexAddNodeCommand( command );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexAddRelationshipCommand(Neo4Net.kernel.impl.index.IndexCommand.AddRelationshipCommand command) throws java.io.IOException
		  public override bool VisitIndexAddRelationshipCommand( AddRelationshipCommand command )
		  {
				return Delegate.visitIndexAddRelationshipCommand( command );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexRemoveCommand(Neo4Net.kernel.impl.index.IndexCommand.RemoveCommand command) throws java.io.IOException
		  public override bool VisitIndexRemoveCommand( RemoveCommand command )
		  {
				return Delegate.visitIndexRemoveCommand( command );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexDeleteCommand(Neo4Net.kernel.impl.index.IndexCommand.DeleteCommand command) throws java.io.IOException
		  public override bool VisitIndexDeleteCommand( DeleteCommand command )
		  {
				return Delegate.visitIndexDeleteCommand( command );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexCreateCommand(Neo4Net.kernel.impl.index.IndexCommand.CreateCommand command) throws java.io.IOException
		  public override bool VisitIndexCreateCommand( CreateCommand command )
		  {
				return Delegate.visitIndexCreateCommand( command );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexDefineCommand(Neo4Net.kernel.impl.index.IndexDefineCommand command) throws java.io.IOException
		  public override bool VisitIndexDefineCommand( IndexDefineCommand command )
		  {
				return Delegate.visitIndexDefineCommand( command );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitNodeCountsCommand(Neo4Net.kernel.impl.transaction.command.Command.NodeCountsCommand command) throws java.io.IOException
		  public override bool VisitNodeCountsCommand( NodeCountsCommand command )
		  {
				return Delegate.visitNodeCountsCommand( command );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitRelationshipCountsCommand(Neo4Net.kernel.impl.transaction.command.Command.RelationshipCountsCommand command) throws java.io.IOException
		  public override bool VisitRelationshipCountsCommand( RelationshipCountsCommand command )
		  {
				return Delegate.visitRelationshipCountsCommand( command );
		  }
	 }

}
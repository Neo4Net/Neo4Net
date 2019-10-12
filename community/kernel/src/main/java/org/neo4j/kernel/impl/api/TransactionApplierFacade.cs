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
namespace Org.Neo4j.Kernel.Impl.Api
{

	using IndexCommand = Org.Neo4j.Kernel.impl.index.IndexCommand;
	using IndexDefineCommand = Org.Neo4j.Kernel.impl.index.IndexDefineCommand;
	using Command = Org.Neo4j.Kernel.impl.transaction.command.Command;
	using StorageCommand = Org.Neo4j.Storageengine.Api.StorageCommand;

	/// <summary>
	/// Wraps several <seealso cref="TransactionApplier"/>s. In this case, each individual visit-call will delegate to {@link
	/// #visit(StorageCommand)} instead, which will call each wrapped <seealso cref="TransactionApplier"/> in turn. In
	/// <seealso cref="close()"/>,
	/// the appliers are closed in reversed order.
	/// </summary>
	public class TransactionApplierFacade : TransactionApplier_Adapter
	{

		 internal readonly TransactionApplier[] Appliers;

		 public TransactionApplierFacade( params TransactionApplier[] appliers )
		 {
			  this.Appliers = appliers;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  // Need to close in reverse order or LuceneRecoveryIT can hang on database shutdown, when
			  // errors are thrown
			  for ( int i = Appliers.Length; i-- > 0; )
			  {
					Appliers[i].close();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visit(org.neo4j.storageengine.api.StorageCommand element) throws java.io.IOException
		 public override bool Visit( StorageCommand element )
		 {
			  foreach ( TransactionApplier applier in Appliers )
			  {
					if ( ( ( Command )element ).handle( applier ) )
					{
						 return true;
					}
			  }
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitNodeCommand(org.neo4j.kernel.impl.transaction.command.Command.NodeCommand command) throws java.io.IOException
		 public override bool VisitNodeCommand( Command.NodeCommand command )
		 {
			  return Visit( command );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitRelationshipCommand(org.neo4j.kernel.impl.transaction.command.Command.RelationshipCommand command) throws java.io.IOException
		 public override bool VisitRelationshipCommand( Command.RelationshipCommand command )
		 {
			  return Visit( command );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitPropertyCommand(org.neo4j.kernel.impl.transaction.command.Command.PropertyCommand command) throws java.io.IOException
		 public override bool VisitPropertyCommand( Command.PropertyCommand command )
		 {
			  return Visit( command );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitRelationshipGroupCommand(org.neo4j.kernel.impl.transaction.command.Command.RelationshipGroupCommand command) throws java.io.IOException
		 public override bool VisitRelationshipGroupCommand( Command.RelationshipGroupCommand command )
		 {
			  return Visit( command );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitRelationshipTypeTokenCommand(org.neo4j.kernel.impl.transaction.command.Command.RelationshipTypeTokenCommand command) throws java.io.IOException
		 public override bool VisitRelationshipTypeTokenCommand( Command.RelationshipTypeTokenCommand command )
		 {
			  return Visit( command );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitLabelTokenCommand(org.neo4j.kernel.impl.transaction.command.Command.LabelTokenCommand command) throws java.io.IOException
		 public override bool VisitLabelTokenCommand( Command.LabelTokenCommand command )
		 {
			  return Visit( command );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitPropertyKeyTokenCommand(org.neo4j.kernel.impl.transaction.command.Command.PropertyKeyTokenCommand command) throws java.io.IOException
		 public override bool VisitPropertyKeyTokenCommand( Command.PropertyKeyTokenCommand command )
		 {
			  return Visit( command );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitSchemaRuleCommand(org.neo4j.kernel.impl.transaction.command.Command.SchemaRuleCommand command) throws java.io.IOException
		 public override bool VisitSchemaRuleCommand( Command.SchemaRuleCommand command )
		 {
			  return Visit( command );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitNeoStoreCommand(org.neo4j.kernel.impl.transaction.command.Command.NeoStoreCommand command) throws java.io.IOException
		 public override bool VisitNeoStoreCommand( Command.NeoStoreCommand command )
		 {
			  return Visit( command );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexAddNodeCommand(org.neo4j.kernel.impl.index.IndexCommand.AddNodeCommand command) throws java.io.IOException
		 public override bool VisitIndexAddNodeCommand( IndexCommand.AddNodeCommand command )
		 {
			  return Visit( command );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexAddRelationshipCommand(org.neo4j.kernel.impl.index.IndexCommand.AddRelationshipCommand command) throws java.io.IOException
		 public override bool VisitIndexAddRelationshipCommand( IndexCommand.AddRelationshipCommand command )
		 {
			  return Visit( command );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexRemoveCommand(org.neo4j.kernel.impl.index.IndexCommand.RemoveCommand command) throws java.io.IOException
		 public override bool VisitIndexRemoveCommand( IndexCommand.RemoveCommand command )
		 {
			  return Visit( command );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexDeleteCommand(org.neo4j.kernel.impl.index.IndexCommand.DeleteCommand command) throws java.io.IOException
		 public override bool VisitIndexDeleteCommand( IndexCommand.DeleteCommand command )
		 {
			  return Visit( command );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexCreateCommand(org.neo4j.kernel.impl.index.IndexCommand.CreateCommand command) throws java.io.IOException
		 public override bool VisitIndexCreateCommand( IndexCommand.CreateCommand command )
		 {
			  return Visit( command );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitIndexDefineCommand(org.neo4j.kernel.impl.index.IndexDefineCommand command) throws java.io.IOException
		 public override bool VisitIndexDefineCommand( IndexDefineCommand command )
		 {
			  return Visit( command );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitNodeCountsCommand(org.neo4j.kernel.impl.transaction.command.Command.NodeCountsCommand command) throws java.io.IOException
		 public override bool VisitNodeCountsCommand( Command.NodeCountsCommand command )
		 {
			  return Visit( command );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitRelationshipCountsCommand(org.neo4j.kernel.impl.transaction.command.Command.RelationshipCountsCommand command) throws java.io.IOException
		 public override bool VisitRelationshipCountsCommand( Command.RelationshipCountsCommand command )
		 {
			  return Visit( command );
		 }
	}

}
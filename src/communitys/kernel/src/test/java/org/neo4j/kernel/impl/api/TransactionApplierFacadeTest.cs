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
namespace Neo4Net.Kernel.Impl.Api
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;

	using IndexCommand = Neo4Net.Kernel.impl.index.IndexCommand;
	using IndexDefineCommand = Neo4Net.Kernel.impl.index.IndexDefineCommand;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class TransactionApplierFacadeTest
	{
		 private TransactionApplierFacade _facade;
		 private TransactionApplier _txApplier1;
		 private TransactionApplier _txApplier2;
		 private TransactionApplier _txApplier3;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _txApplier1 = mock( typeof( TransactionApplier ) );
			  _txApplier2 = mock( typeof( TransactionApplier ) );
			  _txApplier3 = mock( typeof( TransactionApplier ) );

			  _facade = new TransactionApplierFacade( _txApplier1, _txApplier2, _txApplier3 );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testClose() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestClose()
		 {
			  // WHEN
			  _facade.close();

			  // THEN
			  InOrder inOrder = inOrder( _txApplier1, _txApplier2,_txApplier3 );

			  // Verify reverse order
			  inOrder.verify( _txApplier3 ).close();
			  inOrder.verify( _txApplier2 ).close();
			  inOrder.verify( _txApplier1 ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testVisit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestVisit()
		 {
			  Command cmd = mock( typeof( Command ) );

			  // WHEN
			  bool result = _facade.visit( cmd );

			  // THEN
			  InOrder inOrder = inOrder( cmd );

			  inOrder.verify( cmd ).handle( _txApplier1 );
			  inOrder.verify( cmd ).handle( _txApplier2 );
			  inOrder.verify( cmd ).handle( _txApplier3 );

			  inOrder.verifyNoMoreInteractions();

			  assertFalse( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testVisitNodeCommand() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestVisitNodeCommand()
		 {
			  Command.NodeCommand cmd = mock( typeof( Command.NodeCommand ) );
			  when( cmd.Handle( any( typeof( CommandVisitor ) ) ) ).thenCallRealMethod();

			  // WHEN
			  bool result = _facade.visitNodeCommand( cmd );

			  // THEN
			  InOrder inOrder = inOrder( _txApplier1, _txApplier2, _txApplier3 );

			  inOrder.verify( _txApplier1 ).visitNodeCommand( cmd );
			  inOrder.verify( _txApplier2 ).visitNodeCommand( cmd );
			  inOrder.verify( _txApplier3 ).visitNodeCommand( cmd );

			  inOrder.verifyNoMoreInteractions();

			  assertFalse( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testVisitRelationshipCommand() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestVisitRelationshipCommand()
		 {
			  Command.RelationshipCommand cmd = mock( typeof( Command.RelationshipCommand ) );
			  when( cmd.Handle( any( typeof( CommandVisitor ) ) ) ).thenCallRealMethod();

			  // WHEN
			  bool result = _facade.visitRelationshipCommand( cmd );

			  // THEN
			  InOrder inOrder = inOrder( _txApplier1, _txApplier2, _txApplier3 );

			  inOrder.verify( _txApplier1 ).visitRelationshipCommand( cmd );
			  inOrder.verify( _txApplier2 ).visitRelationshipCommand( cmd );
			  inOrder.verify( _txApplier3 ).visitRelationshipCommand( cmd );

			  inOrder.verifyNoMoreInteractions();

			  assertFalse( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testVisitPropertyCommand() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestVisitPropertyCommand()
		 {
			  Command.PropertyCommand cmd = mock( typeof( Command.PropertyCommand ) );
			  when( cmd.Handle( any( typeof( CommandVisitor ) ) ) ).thenCallRealMethod();

			  // WHEN
			  bool result = _facade.visitPropertyCommand( cmd );

			  // THEN
			  InOrder inOrder = inOrder( _txApplier1, _txApplier2, _txApplier3 );

			  inOrder.verify( _txApplier1 ).visitPropertyCommand( cmd );
			  inOrder.verify( _txApplier2 ).visitPropertyCommand( cmd );
			  inOrder.verify( _txApplier3 ).visitPropertyCommand( cmd );

			  inOrder.verifyNoMoreInteractions();

			  assertFalse( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testVisitRelationshipGroupCommand() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestVisitRelationshipGroupCommand()
		 {
			  Command.RelationshipGroupCommand cmd = mock( typeof( Command.RelationshipGroupCommand ) );
			  when( cmd.Handle( any( typeof( CommandVisitor ) ) ) ).thenCallRealMethod();

			  // WHEN
			  bool result = _facade.visitRelationshipGroupCommand( cmd );

			  // THEN
			  InOrder inOrder = inOrder( _txApplier1, _txApplier2, _txApplier3 );

			  inOrder.verify( _txApplier1 ).visitRelationshipGroupCommand( cmd );
			  inOrder.verify( _txApplier2 ).visitRelationshipGroupCommand( cmd );
			  inOrder.verify( _txApplier3 ).visitRelationshipGroupCommand( cmd );

			  inOrder.verifyNoMoreInteractions();

			  assertFalse( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testVisitRelationshipTypeTokenCommand() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestVisitRelationshipTypeTokenCommand()
		 {
			  Command.RelationshipTypeTokenCommand cmd = mock( typeof( Command.RelationshipTypeTokenCommand ) );
			  when( cmd.Handle( any( typeof( CommandVisitor ) ) ) ).thenCallRealMethod();

			  // WHEN
			  bool result = _facade.visitRelationshipTypeTokenCommand( cmd );

			  // THEN
			  InOrder inOrder = inOrder( _txApplier1, _txApplier2, _txApplier3 );

			  inOrder.verify( _txApplier1 ).visitRelationshipTypeTokenCommand( cmd );
			  inOrder.verify( _txApplier2 ).visitRelationshipTypeTokenCommand( cmd );
			  inOrder.verify( _txApplier3 ).visitRelationshipTypeTokenCommand( cmd );

			  inOrder.verifyNoMoreInteractions();

			  assertFalse( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testVisitLabelTokenCommand() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestVisitLabelTokenCommand()
		 {
			  Command.LabelTokenCommand cmd = mock( typeof( Command.LabelTokenCommand ) );
			  when( cmd.Handle( any( typeof( CommandVisitor ) ) ) ).thenCallRealMethod();

			  // WHEN
			  bool result = _facade.visitLabelTokenCommand( cmd );

			  // THEN
			  InOrder inOrder = inOrder( _txApplier1, _txApplier2, _txApplier3 );

			  inOrder.verify( _txApplier1 ).visitLabelTokenCommand( cmd );
			  inOrder.verify( _txApplier2 ).visitLabelTokenCommand( cmd );
			  inOrder.verify( _txApplier3 ).visitLabelTokenCommand( cmd );

			  inOrder.verifyNoMoreInteractions();

			  assertFalse( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testVisitPropertyKeyTokenCommand() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestVisitPropertyKeyTokenCommand()
		 {
			  // Make sure it just calls through to visit
			  Command.PropertyKeyTokenCommand cmd = mock( typeof( Command.PropertyKeyTokenCommand ) );
			  when( cmd.Handle( any( typeof( CommandVisitor ) ) ) ).thenCallRealMethod();

			  // WHEN
			  bool result = _facade.visitPropertyKeyTokenCommand( cmd );

			  // THEN
			  InOrder inOrder = inOrder( _txApplier1, _txApplier2, _txApplier3 );

			  inOrder.verify( _txApplier1 ).visitPropertyKeyTokenCommand( cmd );
			  inOrder.verify( _txApplier2 ).visitPropertyKeyTokenCommand( cmd );
			  inOrder.verify( _txApplier3 ).visitPropertyKeyTokenCommand( cmd );

			  inOrder.verifyNoMoreInteractions();

			  assertFalse( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testVisitSchemaRuleCommand() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestVisitSchemaRuleCommand()
		 {
	// Make sure it just calls through to visit
			  Command.SchemaRuleCommand cmd = mock( typeof( Command.SchemaRuleCommand ) );
			  when( cmd.Handle( any( typeof( CommandVisitor ) ) ) ).thenCallRealMethod();

			  // WHEN
			  bool result = _facade.visitSchemaRuleCommand( cmd );

			  // THEN
			  InOrder inOrder = inOrder( _txApplier1, _txApplier2, _txApplier3 );

			  inOrder.verify( _txApplier1 ).visitSchemaRuleCommand( cmd );
			  inOrder.verify( _txApplier2 ).visitSchemaRuleCommand( cmd );
			  inOrder.verify( _txApplier3 ).visitSchemaRuleCommand( cmd );

			  inOrder.verifyNoMoreInteractions();

			  assertFalse( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testVisitNeoStoreCommand() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestVisitNeoStoreCommand()
		 {
	// Make sure it just calls through to visit
			  Command.NeoStoreCommand cmd = mock( typeof( Command.NeoStoreCommand ) );
			  when( cmd.Handle( any( typeof( CommandVisitor ) ) ) ).thenCallRealMethod();

			  // WHEN
			  bool result = _facade.visitNeoStoreCommand( cmd );

			  // THEN
			  InOrder inOrder = inOrder( _txApplier1, _txApplier2, _txApplier3 );

			  inOrder.verify( _txApplier1 ).visitNeoStoreCommand( cmd );
			  inOrder.verify( _txApplier2 ).visitNeoStoreCommand( cmd );
			  inOrder.verify( _txApplier3 ).visitNeoStoreCommand( cmd );

			  inOrder.verifyNoMoreInteractions();

			  assertFalse( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testVisitIndexAddNodeCommand() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestVisitIndexAddNodeCommand()
		 {
			  IndexCommand.AddNodeCommand cmd = mock( typeof( IndexCommand.AddNodeCommand ) );
			  when( cmd.Handle( any( typeof( CommandVisitor ) ) ) ).thenCallRealMethod();

			  // WHEN
			  bool result = _facade.visitIndexAddNodeCommand( cmd );

			  // THEN
			  InOrder inOrder = inOrder( _txApplier1, _txApplier2, _txApplier3 );

			  inOrder.verify( _txApplier1 ).visitIndexAddNodeCommand( cmd );
			  inOrder.verify( _txApplier2 ).visitIndexAddNodeCommand( cmd );
			  inOrder.verify( _txApplier3 ).visitIndexAddNodeCommand( cmd );

			  inOrder.verifyNoMoreInteractions();

			  assertFalse( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testVisitIndexAddRelationshipCommand() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestVisitIndexAddRelationshipCommand()
		 {
			  IndexCommand.AddRelationshipCommand cmd = mock( typeof( IndexCommand.AddRelationshipCommand ) );
			  when( cmd.Handle( any( typeof( CommandVisitor ) ) ) ).thenCallRealMethod();

			  // WHEN
			  bool result = _facade.visitIndexAddRelationshipCommand( cmd );

			  // THEN
			  InOrder inOrder = inOrder( _txApplier1, _txApplier2, _txApplier3 );

			  inOrder.verify( _txApplier1 ).visitIndexAddRelationshipCommand( cmd );
			  inOrder.verify( _txApplier2 ).visitIndexAddRelationshipCommand( cmd );
			  inOrder.verify( _txApplier3 ).visitIndexAddRelationshipCommand( cmd );

			  inOrder.verifyNoMoreInteractions();

			  assertFalse( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testVisitIndexRemoveCommand() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestVisitIndexRemoveCommand()
		 {
			  IndexCommand.RemoveCommand cmd = mock( typeof( IndexCommand.RemoveCommand ) );
			  when( cmd.Handle( any( typeof( CommandVisitor ) ) ) ).thenCallRealMethod();

			  // WHEN
			  bool result = _facade.visitIndexRemoveCommand( cmd );

			  // THEN
			  InOrder inOrder = inOrder( _txApplier1, _txApplier2, _txApplier3 );

			  inOrder.verify( _txApplier1 ).visitIndexRemoveCommand( cmd );
			  inOrder.verify( _txApplier2 ).visitIndexRemoveCommand( cmd );
			  inOrder.verify( _txApplier3 ).visitIndexRemoveCommand( cmd );

			  inOrder.verifyNoMoreInteractions();

			  assertFalse( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testVisitIndexDeleteCommand() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestVisitIndexDeleteCommand()
		 {
			  IndexCommand.DeleteCommand cmd = mock( typeof( IndexCommand.DeleteCommand ) );
			  when( cmd.Handle( any( typeof( CommandVisitor ) ) ) ).thenCallRealMethod();

			  // WHEN
			  bool result = _facade.visitIndexDeleteCommand( cmd );

			  // THEN
			  InOrder inOrder = inOrder( _txApplier1, _txApplier2, _txApplier3 );

			  inOrder.verify( _txApplier1 ).visitIndexDeleteCommand( cmd );
			  inOrder.verify( _txApplier2 ).visitIndexDeleteCommand( cmd );
			  inOrder.verify( _txApplier3 ).visitIndexDeleteCommand( cmd );

			  inOrder.verifyNoMoreInteractions();

			  assertFalse( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testVisitIndexCreateCommand() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestVisitIndexCreateCommand()
		 {
			  IndexCommand.CreateCommand cmd = mock( typeof( IndexCommand.CreateCommand ) );
			  when( cmd.Handle( any( typeof( CommandVisitor ) ) ) ).thenCallRealMethod();

			  // WHEN
			  bool result = _facade.visitIndexCreateCommand( cmd );

			  // THEN
			  InOrder inOrder = inOrder( _txApplier1, _txApplier2, _txApplier3 );

			  inOrder.verify( _txApplier1 ).visitIndexCreateCommand( cmd );
			  inOrder.verify( _txApplier2 ).visitIndexCreateCommand( cmd );
			  inOrder.verify( _txApplier3 ).visitIndexCreateCommand( cmd );

			  inOrder.verifyNoMoreInteractions();

			  assertFalse( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testVisitIndexDefineCommand() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestVisitIndexDefineCommand()
		 {
			  IndexDefineCommand cmd = mock( typeof( IndexDefineCommand ) );
			  when( cmd.Handle( any( typeof( CommandVisitor ) ) ) ).thenCallRealMethod();

			  // WHEN
			  bool result = _facade.visitIndexDefineCommand( cmd );

			  // THEN
			  InOrder inOrder = inOrder( _txApplier1, _txApplier2, _txApplier3 );

			  inOrder.verify( _txApplier1 ).visitIndexDefineCommand( cmd );
			  inOrder.verify( _txApplier2 ).visitIndexDefineCommand( cmd );
			  inOrder.verify( _txApplier3 ).visitIndexDefineCommand( cmd );

			  inOrder.verifyNoMoreInteractions();

			  assertFalse( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testVisitNodeCountsCommand() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestVisitNodeCountsCommand()
		 {
			  Command.NodeCountsCommand cmd = mock( typeof( Command.NodeCountsCommand ) );
			  when( cmd.Handle( any( typeof( CommandVisitor ) ) ) ).thenCallRealMethod();

			  // WHEN
			  bool result = _facade.visitNodeCountsCommand( cmd );

			  // THEN
			  InOrder inOrder = inOrder( _txApplier1, _txApplier2, _txApplier3 );

			  inOrder.verify( _txApplier1 ).visitNodeCountsCommand( cmd );
			  inOrder.verify( _txApplier2 ).visitNodeCountsCommand( cmd );
			  inOrder.verify( _txApplier3 ).visitNodeCountsCommand( cmd );

			  inOrder.verifyNoMoreInteractions();

			  assertFalse( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testVisitRelationshipCountsCommand() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestVisitRelationshipCountsCommand()
		 {
			  Command.RelationshipCountsCommand cmd = mock( typeof( Command.RelationshipCountsCommand ) );
			  when( cmd.Handle( any( typeof( CommandVisitor ) ) ) ).thenCallRealMethod();

			  // WHEN
			  bool result = _facade.visitRelationshipCountsCommand( cmd );

			  // THEN
			  InOrder inOrder = inOrder( _txApplier1, _txApplier2, _txApplier3 );

			  inOrder.verify( _txApplier1 ).visitRelationshipCountsCommand( cmd );
			  inOrder.verify( _txApplier2 ).visitRelationshipCountsCommand( cmd );
			  inOrder.verify( _txApplier3 ).visitRelationshipCountsCommand( cmd );

			  inOrder.verifyNoMoreInteractions();

			  assertFalse( result );
		 }
	}

}
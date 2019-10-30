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
namespace Neo4Net.Kernel.Impl.Api.state
{
	using Test = org.junit.Test;


	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using IndexManager = Neo4Net.GraphDb.Index.IndexManager;
	using IndexCommand = Neo4Net.Kernel.impl.index.IndexCommand;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using IndexDefineCommand = Neo4Net.Kernel.impl.index.IndexDefineCommand;
	using IndexEntityType = Neo4Net.Kernel.impl.index.IndexEntityType;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using IndexImplementation = Neo4Net.Kernel.spi.explicitindex.IndexImplementation;
	using StorageCommand = Neo4Net.Kernel.Api.StorageEngine.StorageCommand;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ExplicitIndexTransactionStateImplTest
	{
		 private readonly IDictionary<string, string> _config = singletonMap( Neo4Net.GraphDb.Index.IndexManager_Fields.PROVIDER, "test" );
		 private readonly IndexImplementation _provider = mock( typeof( IndexImplementation ) );
		 private IndexConfigStore _indexConfigStore;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tracksNodeCommands()
		 public virtual void TracksNodeCommands()
		 {
			  ExplicitIndexTransactionStateImpl state = NewExplicitIndexTxState();

			  state.AddNode( "index1", 1, "key1", "value1" );
			  state.RemoveNode( "index1", 1, "key2", "value2" );
			  state.AddNode( "index1", 2, "key1", "value3" );
			  state.AddNode( "index1", 3, "key2", "value4" );
			  state.RemoveNode( "index2", 4, "key1", "value5" );

			  IndexDefineCommand indexDefinedCommand = new IndexDefineCommand();
			  indexDefinedCommand.GetOrAssignIndexNameId( "index1" );
			  indexDefinedCommand.GetOrAssignIndexNameId( "index2" );
			  indexDefinedCommand.GetOrAssignKeyId( "key1" );
			  indexDefinedCommand.GetOrAssignKeyId( "key2" );

			  ISet<Command> expectedCommands = new HashSet<Command>( Arrays.asList( indexDefinedCommand, AddNode( 1, 1, 1, "value1" ), RemoveNode( 1, 1, 2, "value2" ), AddNode( 1, 2, 1, "value3" ), AddNode( 1, 3, 2, "value4" ), RemoveNode( 2, 4, 1, "value5" ) ) );
			  assertEquals( expectedCommands, ExtractCommands( state ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tracksRelationshipCommands()
		 public virtual void TracksRelationshipCommands()
		 {
			  ExplicitIndexTransactionStateImpl state = NewExplicitIndexTxState();

			  state.RemoveRelationship( "index1", 1, "key1", "value1" );
			  state.AddRelationship( "index1", 1, "key2", "value2", 11, 11 );
			  state.RemoveRelationship( "index1", 2, "key1", "value3" );
			  state.AddRelationship( "index1", 3, "key2", "value4", 22, 22 );
			  state.AddRelationship( "index2", 4, "key1", "value5", 33, 33 );

			  IndexDefineCommand indexDefinedCommand = new IndexDefineCommand();
			  indexDefinedCommand.GetOrAssignIndexNameId( "index1" );
			  indexDefinedCommand.GetOrAssignIndexNameId( "index2" );
			  indexDefinedCommand.GetOrAssignKeyId( "key1" );
			  indexDefinedCommand.GetOrAssignKeyId( "key2" );

			  ISet<Command> expectedCommands = new HashSet<Command>( Arrays.asList( indexDefinedCommand, RemoveRelationship( 1, 1, 1, "value1" ), AddRelationship( 1, 1, 2, "value2", 11, 11 ), RemoveRelationship( 1, 2, 1, "value3" ), AddRelationship( 1, 3, 2, "value4", 22, 22 ), AddRelationship( 2, 4, 1, "value5", 33, 33 ) ) );
			  assertEquals( expectedCommands, ExtractCommands( state ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nodeIndexDeletionRemovesCommands()
		 public virtual void NodeIndexDeletionRemovesCommands()
		 {
			  ExplicitIndexTransactionStateImpl state = NewExplicitIndexTxState();

			  state.AddNode( "index", 1, "key", "value1" );
			  state.AddNode( "index", 2, "key", "value2" );
			  state.RemoveNode( "index", 3, "key", "value3" );

			  state.DeleteIndex( IndexEntityType.Node, "index" );

			  IndexDefineCommand indexDefinedCommand = new IndexDefineCommand();
			  indexDefinedCommand.GetOrAssignIndexNameId( "index" );
			  indexDefinedCommand.GetOrAssignKeyId( "key" );

			  IndexCommand.DeleteCommand delete = new IndexCommand.DeleteCommand();
			  delete.Init( 1, IndexEntityType.Node.id() );

			  ISet<Command> expectedCommands = new HashSet<Command>( Arrays.asList( indexDefinedCommand, delete ) );
			  assertEquals( expectedCommands, ExtractCommands( state ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void relationshipIndexDeletionRemovesCommands()
		 public virtual void RelationshipIndexDeletionRemovesCommands()
		 {
			  ExplicitIndexTransactionStateImpl state = NewExplicitIndexTxState();

			  state.RemoveRelationship( "index", 1, "key", "value1" );
			  state.AddRelationship( "index", 2, "key", "value2", 11, 11 );
			  state.AddRelationship( "index", 3, "key", "value3", 22, 22 );

			  state.DeleteIndex( IndexEntityType.Relationship, "index" );

			  IndexDefineCommand indexDefinedCommand = new IndexDefineCommand();
			  indexDefinedCommand.GetOrAssignIndexNameId( "index" );
			  indexDefinedCommand.GetOrAssignKeyId( "key" );

			  IndexCommand.DeleteCommand delete = new IndexCommand.DeleteCommand();
			  delete.Init( 1, IndexEntityType.Relationship.id() );

			  ISet<Command> expectedCommands = new HashSet<Command>( Arrays.asList( indexDefinedCommand, delete ) );
			  assertEquals( expectedCommands, ExtractCommands( state ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removalOfNodeIndexDoesNotClearRelationshipCommandsForRelationshipIndexWithSameName()
		 public virtual void RemovalOfNodeIndexDoesNotClearRelationshipCommandsForRelationshipIndexWithSameName()
		 {
			  ExplicitIndexTransactionStateImpl state = NewExplicitIndexTxState();

			  state.AddNode( "index", 1, "key", "value" );
			  state.AddRelationship( "index", 1, "key", "value", 11, 11 );
			  state.DeleteIndex( IndexEntityType.Node, "index" );

			  IndexDefineCommand indexDefinedCommand = new IndexDefineCommand();
			  indexDefinedCommand.GetOrAssignIndexNameId( "index" );
			  indexDefinedCommand.GetOrAssignKeyId( "key" );

			  IndexCommand.DeleteCommand delete = new IndexCommand.DeleteCommand();
			  delete.Init( 1, IndexEntityType.Node.id() );

			  ISet<Command> expectedCommands = new HashSet<Command>( Arrays.asList( indexDefinedCommand, delete, AddRelationship( 1, 1, 1, "value", 11, 11 ) ) );
			  assertEquals( expectedCommands, ExtractCommands( state ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removalOfRelationshipIndexDoesNotClearNodeCommandsForNodeIndexWithSameName()
		 public virtual void RemovalOfRelationshipIndexDoesNotClearNodeCommandsForNodeIndexWithSameName()
		 {
			  ExplicitIndexTransactionStateImpl state = NewExplicitIndexTxState();

			  state.AddNode( "index", 1, "key", "value" );
			  state.AddRelationship( "index", 1, "key", "value", 11, 11 );
			  state.DeleteIndex( IndexEntityType.Relationship, "index" );

			  IndexDefineCommand indexDefinedCommand = new IndexDefineCommand();
			  indexDefinedCommand.GetOrAssignIndexNameId( "index" );
			  indexDefinedCommand.GetOrAssignKeyId( "key" );

			  IndexCommand.DeleteCommand delete = new IndexCommand.DeleteCommand();
			  delete.Init( 1, IndexEntityType.Relationship.id() );

			  ISet<Command> expectedCommands = new HashSet<Command>( Arrays.asList( indexDefinedCommand, delete, AddNode( 1, 1, 1, "value" ) ) );
			  assertEquals( expectedCommands, ExtractCommands( state ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportIndexExists()
		 public virtual void ShouldReportIndexExists()
		 {
			  // given
			  ExplicitIndexTransactionStateImpl state = NewExplicitIndexTxState();

			  // when
			  bool nodeExists = state.CheckIndexExistence( IndexEntityType.Node, "name", null );
			  bool relExists = state.CheckIndexExistence( IndexEntityType.Relationship, "name", null );

			  // then
			  assertTrue( nodeExists );
			  assertTrue( relExists );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportIndexExistsWithMatchingConfiguration()
		 public virtual void ShouldReportIndexExistsWithMatchingConfiguration()
		 {
			  // given
			  ExplicitIndexTransactionStateImpl state = NewExplicitIndexTxState();
			  when( _provider.configMatches( anyMap(), anyMap() ) ).thenReturn(true);

			  // when
			  bool nodeExists = state.CheckIndexExistence( IndexEntityType.Node, "name", _config );
			  bool relExists = state.CheckIndexExistence( IndexEntityType.Node, "name", _config );

			  // then
			  assertTrue( nodeExists );
			  assertTrue( relExists );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOnIndexExistsWithMismatchingConfiguration()
		 public virtual void ShouldThrowOnIndexExistsWithMismatchingConfiguration()
		 {
			  // given
			  ExplicitIndexTransactionStateImpl state = NewExplicitIndexTxState();
			  when( _provider.configMatches( anyMap(), anyMap() ) ).thenReturn(false);

			  // when
			  try
			  {
					state.CheckIndexExistence( IndexEntityType.Node, "name", _config );
					fail( "Should've failed" );
			  }
			  catch ( System.ArgumentException )
			  { // then good
			  }
			  try
			  {
					state.CheckIndexExistence( IndexEntityType.Node, "name", _config );
					fail( "Should have failed" );
			  }
			  catch ( System.ArgumentException )
			  { // then good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportIndexDoesNotExist()
		 public virtual void ShouldReportIndexDoesNotExist()
		 {
			  // given
			  ExplicitIndexTransactionStateImpl state = NewExplicitIndexTxState();
			  when( _indexConfigStore.get( any( typeof( Type ) ), anyString() ) ).thenReturn(null);

			  // when
			  bool exists = state.CheckIndexExistence( IndexEntityType.Relationship, "name", null );

			  // then
			  assertFalse( exists );
		 }

		 private static ISet<StorageCommand> ExtractCommands( ExplicitIndexTransactionStateImpl state )
		 {
			  ISet<StorageCommand> commands = new HashSet<StorageCommand>();
			  state.ExtractCommands( commands );
			  return commands;
		 }

		 private static Command AddNode( int index, long id, int key, object value )
		 {
			  IndexCommand.AddNodeCommand command = new IndexCommand.AddNodeCommand();
			  command.Init( index, id, key, value );
			  return command;
		 }

		 private static Command AddRelationship( int index, long id, int key, object value, long startNode, long endNode )
		 {
			  IndexCommand.AddRelationshipCommand command = new IndexCommand.AddRelationshipCommand();
			  command.Init( index, id, key, value, startNode, endNode );
			  return command;
		 }

		 private static Command RemoveNode( int index, long id, int key, object value )
		 {
			  IndexCommand.RemoveCommand command = new IndexCommand.RemoveCommand();
			  command.Init( index, IndexEntityType.Node.id(), id, key, value );
			  return command;
		 }

		 private static Command RemoveRelationship( int index, long id, int key, object value )
		 {
			  IndexCommand.RemoveCommand command = new IndexCommand.RemoveCommand();
			  command.Init( index, IndexEntityType.Relationship.id(), id, key, value );
			  return command;
		 }

		 private ExplicitIndexTransactionStateImpl NewExplicitIndexTxState()
		 {
			  _indexConfigStore = mock( typeof( IndexConfigStore ) );
			  when( _indexConfigStore.get( eq( typeof( Node ) ), anyString() ) ).thenReturn(_config);
			  when( _indexConfigStore.get( eq( typeof( Relationship ) ), anyString() ) ).thenReturn(_config);

			  ExplicitIndexProvider explicitIndexProvider = mock( typeof( ExplicitIndexProvider ) );
			  when( explicitIndexProvider.GetProviderByName( anyString() ) ).thenReturn(_provider);

			  return new ExplicitIndexTransactionStateImpl( _indexConfigStore, explicitIndexProvider );
		 }
	}

}
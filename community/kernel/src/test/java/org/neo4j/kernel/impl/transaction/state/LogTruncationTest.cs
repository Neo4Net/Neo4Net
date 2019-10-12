using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.transaction.state
{
	using ObjectIntHashMap = org.eclipse.collections.impl.map.mutable.primitive.ObjectIntHashMap;
	using Test = org.junit.Test;


	using MapUtil = Org.Neo4j.Helpers.Collection.MapUtil;
	using TestIndexDescriptorFactory = Org.Neo4j.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using IndexCommand = Org.Neo4j.Kernel.impl.index.IndexCommand;
	using AddNodeCommand = Org.Neo4j.Kernel.impl.index.IndexCommand.AddNodeCommand;
	using AddRelationshipCommand = Org.Neo4j.Kernel.impl.index.IndexCommand.AddRelationshipCommand;
	using CreateCommand = Org.Neo4j.Kernel.impl.index.IndexCommand.CreateCommand;
	using DeleteCommand = Org.Neo4j.Kernel.impl.index.IndexCommand.DeleteCommand;
	using RemoveCommand = Org.Neo4j.Kernel.impl.index.IndexCommand.RemoveCommand;
	using IndexDefineCommand = Org.Neo4j.Kernel.impl.index.IndexDefineCommand;
	using IndexEntityType = Org.Neo4j.Kernel.impl.index.IndexEntityType;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using NeoStoreRecord = Org.Neo4j.Kernel.impl.store.record.NeoStoreRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;
	using Command = Org.Neo4j.Kernel.impl.transaction.command.Command;
	using NodeCountsCommand = Org.Neo4j.Kernel.impl.transaction.command.Command.NodeCountsCommand;
	using RelationshipCountsCommand = Org.Neo4j.Kernel.impl.transaction.command.Command.RelationshipCountsCommand;
	using InMemoryClosableChannel = Org.Neo4j.Kernel.impl.transaction.log.InMemoryClosableChannel;
	using PhysicalTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using ReadableClosablePositionAwareChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using LogEntry = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntry;
	using LogEntryCommand = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryCommand;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using LogEntryWriter = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryWriter;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using StorageCommand = Org.Neo4j.Storageengine.Api.StorageCommand;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.DynamicRecord.dynamicRecord;

	/// <summary>
	/// At any point, a power outage may stop us from writing to the log, which means that, at any point, all our commands
	/// need to be able to handle the log ending mid-way through reading it.
	/// </summary>
	public class LogTruncationTest
	{
		private bool InstanceFieldsInitialized = false;

		public LogTruncationTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_writer = new LogEntryWriter( _inMemoryChannel );
			NeoStoreRecord after = new NeoStoreRecord();
			after.NextProp = 42;
			_permutations[typeof( Command.NeoStoreCommand )] = new Command[] { new Command.NeoStoreCommand( new NeoStoreRecord(), after ) };
			_permutations[typeof( Command.NodeCommand )] = new Command[] { new Command.NodeCommand( new NodeRecord( 12L, false, 13L, 13L ), new NodeRecord( 0, false, 0, 0 ) ) };
			_permutations[typeof( Command.RelationshipCommand )] = new Command[] { new Command.RelationshipCommand( new RelationshipRecord( 1L ), new RelationshipRecord( 1L, 2L, 3L, 4 ) ) };
			_permutations[typeof( Command.PropertyCommand )] = new Command[] { new Command.PropertyCommand( new PropertyRecord( 1, new NodeRecord( 12L, false, 13L, 13 ) ), new PropertyRecord( 1, new NodeRecord( 12L, false, 13L, 13 ) ) ) };
			_permutations[typeof( Command.RelationshipGroupCommand )] = new Command[] { new Command.LabelTokenCommand( new LabelTokenRecord( 1 ), CreateLabelTokenRecord( 1 ) ) };
			_permutations[typeof( Command.SchemaRuleCommand )] = new Command[] { new Command.SchemaRuleCommand( singletonList( dynamicRecord( 1L, false, true, -1L, 1, "hello".GetBytes() ) ), singletonList(dynamicRecord(1L, true, true, -1L, 1, "hello".GetBytes())), TestIndexDescriptorFactory.forLabel(3, 4).withId(1) ) };
			_permutations[typeof( Command.RelationshipTypeTokenCommand )] = new Command[] { new Command.RelationshipTypeTokenCommand( new RelationshipTypeTokenRecord( 1 ), CreateRelationshipTypeTokenRecord( 1 ) ) };
			_permutations[typeof( Command.PropertyKeyTokenCommand )] = new Command[] { new Command.PropertyKeyTokenCommand( new PropertyKeyTokenRecord( 1 ), CreatePropertyKeyTokenRecord( 1 ) ) };
			_permutations[typeof( Command.LabelTokenCommand )] = new Command[] { new Command.LabelTokenCommand( new LabelTokenRecord( 1 ), CreateLabelTokenRecord( 1 ) ) };
         
			// Index commands
			IndexCommand.AddRelationshipCommand addRelationshipCommand = new IndexCommand.AddRelationshipCommand();
			addRelationshipCommand.Init( 1, 1L, 12345, "some value", 1, 1 );
			_permutations[typeof( IndexCommand.AddRelationshipCommand )] = new Command[] { addRelationshipCommand };
         
			IndexCommand.CreateCommand createCommand = new IndexCommand.CreateCommand();
			createCommand.Init( 1, IndexEntityType.Relationship.id(), MapUtil.stringMap("string1", "string 2") );
			_permutations[typeof( IndexCommand.CreateCommand )] = new Command[] { createCommand };
         
			IndexCommand.AddNodeCommand addCommand = new IndexCommand.AddNodeCommand();
			addCommand.Init( 1234, 122L, 2, "value" );
			_permutations[typeof( IndexCommand.AddNodeCommand )] = new Command[] { addCommand };
         
			IndexCommand.DeleteCommand deleteCommand = new IndexCommand.DeleteCommand();
			deleteCommand.Init( 1, IndexEntityType.Relationship.id() );
			_permutations[typeof( IndexCommand.DeleteCommand )] = new Command[] { deleteCommand };
         
			IndexCommand.RemoveCommand removeCommand = new IndexCommand.RemoveCommand();
			removeCommand.Init( 1, IndexEntityType.Node.id(), 126, (sbyte) 3, "the value" );
			_permutations[typeof( IndexCommand.RemoveCommand )] = new Command[] { removeCommand };
         
			IndexDefineCommand indexDefineCommand = new IndexDefineCommand();
			indexDefineCommand.Init( ObjectIntHashMap.newWithKeysValues( "string1", 45, "key1", 2 ), ObjectIntHashMap.newWithKeysValues( "string", 2 ) );
			_permutations[typeof( IndexDefineCommand )] = new Command[] { indexDefineCommand };
         
			// Counts commands
			_permutations[typeof( Command.NodeCountsCommand )] = new Command[]{ new Command.NodeCountsCommand( 42, 11 ) };
			_permutations[typeof( Command.RelationshipCountsCommand )] = new Command[]{ new Command.RelationshipCountsCommand( 17, 2, 13, -2 ) };
		}

		 private readonly InMemoryClosableChannel _inMemoryChannel = new InMemoryClosableChannel();
		 private readonly LogEntryReader<ReadableClosablePositionAwareChannel> _logEntryReader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
		 private LogEntryWriter _writer;
		 /// <summary>
		 /// Stores all known commands, and an arbitrary set of different permutations for them </summary>
		 private readonly IDictionary<Type, Command[]> _permutations = new Dictionary<Type, Command[]>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSerializationInFaceOfLogTruncation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestSerializationInFaceOfLogTruncation()
		 {
			  foreach ( Command cmd in EnumerateCommands() )
			  {
					AssertHandlesLogTruncation( cmd );
			  }
		 }

		 private IEnumerable<Command> EnumerateCommands()
		 {
			  // We use this reflection approach rather than just iterating over the permutation map to force developers
			  // writing new commands to add the new commands to this test. If you came here because of a test failure from
			  // missing commands, add all permutations you can think of of the command to the permutations map in the
			  // beginning of this class.
			  IList<Command> commands = new List<Command>();
			  foreach ( Type cmd in typeof( Command ).GetNestedTypes() )
			  {
					if ( cmd.IsAssignableFrom( typeof( Command ) ) )
					{
						 if ( _permutations.ContainsKey( cmd ) )
						 {
							  ( ( IList<Command> )commands ).AddRange( asList( _permutations[cmd] ) );
						 }
						 else if ( !isAbstract( cmd.Modifiers ) )
						 {
							  throw new AssertionError( "Unknown command type: " + cmd + ", please add missing instantiation to " + "test serialization of this command." );
						 }
					}
			  }
			  foreach ( Type cmd in typeof( IndexCommand ).GetNestedTypes() )
			  {
					if ( cmd.IsAssignableFrom( typeof( Command ) ) )
					{
						 if ( _permutations.ContainsKey( cmd ) )
						 {
							  ( ( IList<Command> )commands ).AddRange( asList( _permutations[cmd] ) );
						 }
						 else if ( !isAbstract( cmd.Modifiers ) )
						 {
							  throw new AssertionError( "Unknown command type: " + cmd + ", please add missing instantiation to " + "test serialization of this command." );
						 }
					}
			  }
			  return commands;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertHandlesLogTruncation(org.neo4j.kernel.impl.transaction.command.Command cmd) throws java.io.IOException
		 private void AssertHandlesLogTruncation( Command cmd )
		 {
			  _inMemoryChannel.reset();
			  _writer.serialize( new PhysicalTransactionRepresentation( singletonList( cmd ) ) );
			  int bytesSuccessfullyWritten = _inMemoryChannel.writerPosition();
			  try
			  {
					LogEntry logEntry = _logEntryReader.readLogEntry( _inMemoryChannel );
					StorageCommand command = ( ( LogEntryCommand ) logEntry ).Command;
					assertEquals( cmd, command );
			  }
			  catch ( Exception e )
			  {
					throw new AssertionError( "Failed to deserialize " + cmd.ToString() + ", because: ", e );
			  }
			  bytesSuccessfullyWritten--;
			  while ( bytesSuccessfullyWritten-- > 0 )
			  {
					_inMemoryChannel.reset();
					_writer.serialize( new PhysicalTransactionRepresentation( singletonList( cmd ) ) );
					_inMemoryChannel.truncateTo( bytesSuccessfullyWritten );
					LogEntry deserialized = _logEntryReader.readLogEntry( _inMemoryChannel );
					assertNull( "Deserialization did not detect log truncation!" + "Record: " + cmd + ", deserialized: " + deserialized, deserialized );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testInMemoryLogChannel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestInMemoryLogChannel()
		 {
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();
			  for ( int i = 0; i < 25; i++ )
			  {
					channel.PutInt( i );
			  }
			  for ( int i = 0; i < 25; i++ )
			  {
					assertEquals( i, channel.Int );
			  }
			  channel.Reset();
			  for ( long i = 0; i < 12; i++ )
			  {
					channel.PutLong( i );
			  }
			  for ( long i = 0; i < 12; i++ )
			  {
					assertEquals( i, channel.Long );
			  }
			  channel.Reset();
			  for ( long i = 0; i < 8; i++ )
			  {
					channel.PutLong( i );
					channel.PutInt( ( int ) i );
			  }
			  for ( long i = 0; i < 8; i++ )
			  {
					assertEquals( i, channel.Long );
					assertEquals( i, channel.Int );
			  }
			  channel.Dispose();
		 }

		 private LabelTokenRecord CreateLabelTokenRecord( int id )
		 {
			  LabelTokenRecord labelTokenRecord = new LabelTokenRecord( id );
			  labelTokenRecord.InUse = true;
			  labelTokenRecord.NameId = 333;
			  labelTokenRecord.AddNameRecord( new DynamicRecord( 43 ) );
			  return labelTokenRecord;
		 }

		 private RelationshipTypeTokenRecord CreateRelationshipTypeTokenRecord( int id )
		 {
			  RelationshipTypeTokenRecord relationshipTypeTokenRecord = new RelationshipTypeTokenRecord( id );
			  relationshipTypeTokenRecord.InUse = true;
			  relationshipTypeTokenRecord.NameId = 333;
			  relationshipTypeTokenRecord.AddNameRecord( new DynamicRecord( 43 ) );
			  return relationshipTypeTokenRecord;
		 }

		 private PropertyKeyTokenRecord CreatePropertyKeyTokenRecord( int id )
		 {
			  PropertyKeyTokenRecord propertyKeyTokenRecord = new PropertyKeyTokenRecord( id );
			  propertyKeyTokenRecord.InUse = true;
			  propertyKeyTokenRecord.NameId = 333;
			  propertyKeyTokenRecord.AddNameRecord( new DynamicRecord( 43 ) );
			  return propertyKeyTokenRecord;
		 }
	}

}
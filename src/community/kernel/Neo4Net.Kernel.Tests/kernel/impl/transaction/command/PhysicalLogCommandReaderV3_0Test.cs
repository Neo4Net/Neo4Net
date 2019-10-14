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
namespace Neo4Net.Kernel.impl.transaction.command
{
	using Test = org.junit.Test;

	using PropertyType = Neo4Net.Kernel.impl.store.PropertyType;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using NeoStoreRecord = Neo4Net.Kernel.Impl.Store.Records.NeoStoreRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using InMemoryClosableChannel = Neo4Net.Kernel.impl.transaction.log.InMemoryClosableChannel;
	using CommandReader = Neo4Net.Storageengine.Api.CommandReader;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class PhysicalLogCommandReaderV3_0Test
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadRelationshipCommand() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadRelationshipCommand()
		 {
			  // Given
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();
			  RelationshipRecord before = new RelationshipRecord( 42, -1, -1, -1 );
			  RelationshipRecord after = new RelationshipRecord( 42, true, 1, 2, 3, 4, 5, 6, 7, true, true );
			  ( new Command.RelationshipCommand( before, after ) ).Serialize( channel );

			  // When
			  PhysicalLogCommandReaderV3_0 reader = new PhysicalLogCommandReaderV3_0();
			  Command command = reader.Read( channel );
			  assertTrue( command is Command.RelationshipCommand );

			  Command.RelationshipCommand relationshipCommand = ( Command.RelationshipCommand ) command;

			  // Then
			  assertEquals( before, relationshipCommand.Before );
			  assertEquals( after, relationshipCommand.After );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readRelationshipCommandWithSecondaryUnit() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadRelationshipCommandWithSecondaryUnit()
		 {
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();
			  RelationshipRecord before = new RelationshipRecord( 42, true, 1, 2, 3, 4, 5, 6, 7, true, true );
			  before.RequiresSecondaryUnit = true;
			  before.SecondaryUnitId = 47;
			  RelationshipRecord after = new RelationshipRecord( 42, true, 1, 8, 3, 4, 5, 6, 7, true, true );
			  ( new Command.RelationshipCommand( before, after ) ).Serialize( channel );

			  PhysicalLogCommandReaderV3_0 reader = new PhysicalLogCommandReaderV3_0();
			  Command command = reader.Read( channel );
			  assertTrue( command is Command.RelationshipCommand );

			  Command.RelationshipCommand relationshipCommand = ( Command.RelationshipCommand ) command;
			  assertEquals( before, relationshipCommand.Before );
			  VerifySecondaryUnit( before, relationshipCommand.Before );
			  assertEquals( after, relationshipCommand.After );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readRelationshipCommandWithNonRequiredSecondaryUnit() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadRelationshipCommandWithNonRequiredSecondaryUnit()
		 {
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();
			  RelationshipRecord before = new RelationshipRecord( 42, true, 1, 2, 3, 4, 5, 6, 7, true, true );
			  before.RequiresSecondaryUnit = false;
			  before.SecondaryUnitId = 52;
			  RelationshipRecord after = new RelationshipRecord( 42, true, 1, 8, 3, 4, 5, 6, 7, true, true );
			  ( new Command.RelationshipCommand( before, after ) ).Serialize( channel );

			  PhysicalLogCommandReaderV3_0 reader = new PhysicalLogCommandReaderV3_0();
			  Command command = reader.Read( channel );
			  assertTrue( command is Command.RelationshipCommand );

			  Command.RelationshipCommand relationshipCommand = ( Command.RelationshipCommand ) command;
			  assertEquals( before, relationshipCommand.Before );
			  VerifySecondaryUnit( before, relationshipCommand.Before );
			  assertEquals( after, relationshipCommand.After );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readRelationshipCommandWithFixedReferenceFormat300() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadRelationshipCommandWithFixedReferenceFormat300()
		 {
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();
			  RelationshipRecord before = new RelationshipRecord( 42, true, 1, 2, 3, 4, 5, 6, 7, true, true );
			  before.UseFixedReferences = true;
			  RelationshipRecord after = new RelationshipRecord( 42, true, 1, 8, 3, 4, 5, 6, 7, true, true );
			  after.UseFixedReferences = true;
			  ( new Command.RelationshipCommand( before, after ) ).Serialize( channel );

			  PhysicalLogCommandReaderV3_0 reader = new PhysicalLogCommandReaderV3_0();
			  Command command = reader.Read( channel );
			  assertTrue( command is Command.RelationshipCommand );

			  Command.RelationshipCommand relationshipCommand = ( Command.RelationshipCommand ) command;
			  assertEquals( before, relationshipCommand.Before );
			  assertTrue( relationshipCommand.Before.UseFixedReferences );
			  assertEquals( after, relationshipCommand.After );
			  assertTrue( relationshipCommand.After.UseFixedReferences );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readRelationshipCommandWithFixedReferenceFormat302() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadRelationshipCommandWithFixedReferenceFormat302()
		 {
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();
			  RelationshipRecord before = new RelationshipRecord( 42, true, 1, 2, 3, 4, 5, 6, 7, true, true );
			  before.UseFixedReferences = true;
			  RelationshipRecord after = new RelationshipRecord( 42, true, 1, 8, 3, 4, 5, 6, 7, true, true );
			  after.UseFixedReferences = true;
			  ( new Command.RelationshipCommand( before, after ) ).Serialize( channel );

			  PhysicalLogCommandReaderV3_0_2 reader = new PhysicalLogCommandReaderV3_0_2();
			  Command command = reader.Read( channel );
			  assertTrue( command is Command.RelationshipCommand );

			  Command.RelationshipCommand relationshipCommand = ( Command.RelationshipCommand ) command;
			  assertEquals( before, relationshipCommand.Before );
			  assertTrue( relationshipCommand.Before.UseFixedReferences );
			  assertEquals( after, relationshipCommand.After );
			  assertTrue( relationshipCommand.After.UseFixedReferences );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadRelationshipGroupCommand() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadRelationshipGroupCommand()
		 {
			  // Given
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();
			  RelationshipGroupRecord before = new RelationshipGroupRecord( 42, 3 );
			  RelationshipGroupRecord after = new RelationshipGroupRecord( 42, 3, 4, 5, 6, 7, 8, true );
			  after.SetCreated();

			  ( new Command.RelationshipGroupCommand( before, after ) ).Serialize( channel );

			  // When
			  PhysicalLogCommandReaderV3_0 reader = new PhysicalLogCommandReaderV3_0();
			  Command command = reader.Read( channel );
			  assertTrue( command is Command.RelationshipGroupCommand );

			  Command.RelationshipGroupCommand relationshipGroupCommand = ( Command.RelationshipGroupCommand ) command;

			  // Then
			  assertEquals( before, relationshipGroupCommand.Before );
			  assertEquals( after, relationshipGroupCommand.After );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readRelationshipGroupCommandWithSecondaryUnit() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadRelationshipGroupCommandWithSecondaryUnit()
		 {
			  // Given
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();
			  RelationshipGroupRecord before = new RelationshipGroupRecord( 42, 3 );
			  RelationshipGroupRecord after = new RelationshipGroupRecord( 42, 3, 4, 5, 6, 7, 8, true );
			  after.RequiresSecondaryUnit = true;
			  after.SecondaryUnitId = 17;
			  after.SetCreated();

			  ( new Command.RelationshipGroupCommand( before, after ) ).Serialize( channel );

			  // When
			  PhysicalLogCommandReaderV3_0 reader = new PhysicalLogCommandReaderV3_0();
			  Command command = reader.Read( channel );
			  assertTrue( command is Command.RelationshipGroupCommand );

			  Command.RelationshipGroupCommand relationshipGroupCommand = ( Command.RelationshipGroupCommand ) command;

			  // Then
			  assertEquals( before, relationshipGroupCommand.Before );
			  assertEquals( after, relationshipGroupCommand.After );
			  VerifySecondaryUnit( after, relationshipGroupCommand.After );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readRelationshipGroupCommandWithNonRequiredSecondaryUnit() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadRelationshipGroupCommandWithNonRequiredSecondaryUnit()
		 {
			  // Given
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();
			  RelationshipGroupRecord before = new RelationshipGroupRecord( 42, 3 );
			  RelationshipGroupRecord after = new RelationshipGroupRecord( 42, 3, 4, 5, 6, 7, 8, true );
			  after.RequiresSecondaryUnit = false;
			  after.SecondaryUnitId = 17;
			  after.SetCreated();

			  ( new Command.RelationshipGroupCommand( before, after ) ).Serialize( channel );

			  // When
			  PhysicalLogCommandReaderV3_0 reader = new PhysicalLogCommandReaderV3_0();
			  Command command = reader.Read( channel );
			  assertTrue( command is Command.RelationshipGroupCommand );

			  Command.RelationshipGroupCommand relationshipGroupCommand = ( Command.RelationshipGroupCommand ) command;

			  // Then
			  assertEquals( before, relationshipGroupCommand.Before );
			  assertEquals( after, relationshipGroupCommand.After );
			  VerifySecondaryUnit( after, relationshipGroupCommand.After );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readRelationshipGroupCommandWithFixedReferenceFormat300() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadRelationshipGroupCommandWithFixedReferenceFormat300()
		 {
			  // Given
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();
			  RelationshipGroupRecord before = new RelationshipGroupRecord( 42, 3 );
			  before.UseFixedReferences = true;
			  RelationshipGroupRecord after = new RelationshipGroupRecord( 42, 3, 4, 5, 6, 7, 8, true );
			  after.UseFixedReferences = true;

			  ( new Command.RelationshipGroupCommand( before, after ) ).Serialize( channel );

			  // When
			  PhysicalLogCommandReaderV3_0 reader = new PhysicalLogCommandReaderV3_0();
			  Command command = reader.Read( channel );
			  assertTrue( command is Command.RelationshipGroupCommand );

			  Command.RelationshipGroupCommand relationshipGroupCommand = ( Command.RelationshipGroupCommand ) command;

			  // Then
			  assertEquals( before, relationshipGroupCommand.Before );
			  assertEquals( after, relationshipGroupCommand.After );
			  assertTrue( relationshipGroupCommand.Before.UseFixedReferences );
			  assertTrue( relationshipGroupCommand.After.UseFixedReferences );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readRelationshipGroupCommandWithFixedReferenceFormat302() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadRelationshipGroupCommandWithFixedReferenceFormat302()
		 {
			  // Given
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();
			  RelationshipGroupRecord before = new RelationshipGroupRecord( 42, 3 );
			  before.UseFixedReferences = true;
			  RelationshipGroupRecord after = new RelationshipGroupRecord( 42, 3, 4, 5, 6, 7, 8, true );
			  after.UseFixedReferences = true;

			  ( new Command.RelationshipGroupCommand( before, after ) ).Serialize( channel );

			  // When
			  PhysicalLogCommandReaderV3_0_2 reader = new PhysicalLogCommandReaderV3_0_2();
			  Command command = reader.Read( channel );
			  assertTrue( command is Command.RelationshipGroupCommand );

			  Command.RelationshipGroupCommand relationshipGroupCommand = ( Command.RelationshipGroupCommand ) command;

			  // Then
			  assertEquals( before, relationshipGroupCommand.Before );
			  assertEquals( after, relationshipGroupCommand.After );
			  assertTrue( relationshipGroupCommand.Before.UseFixedReferences );
			  assertTrue( relationshipGroupCommand.After.UseFixedReferences );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadNeoStoreCommand() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadNeoStoreCommand()
		 {
			  // Given
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();
			  NeoStoreRecord before = new NeoStoreRecord();
			  NeoStoreRecord after = new NeoStoreRecord();
			  after.NextProp = 42;

			  ( new Command.NeoStoreCommand( before, after ) ).Serialize( channel );

			  // When
			  PhysicalLogCommandReaderV3_0 reader = new PhysicalLogCommandReaderV3_0();
			  Command command = reader.Read( channel );
			  assertTrue( command is Command.NeoStoreCommand );

			  Command.NeoStoreCommand neoStoreCommand = ( Command.NeoStoreCommand ) command;

			  // Then
			  assertEquals( before.NextProp, neoStoreCommand.Before.NextProp );
			  assertEquals( after.NextProp, neoStoreCommand.After.NextProp );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nodeCommandWithFixedReferenceFormat300() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NodeCommandWithFixedReferenceFormat300()
		 {
			  // Given
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();
			  NodeRecord before = new NodeRecord( 42, true, false, 33, 99, 66 );
			  NodeRecord after = new NodeRecord( 42, true, false, 33, 99, 66 );
			  before.UseFixedReferences = true;
			  after.UseFixedReferences = true;

			  ( new Command.NodeCommand( before, after ) ).Serialize( channel );

			  // When
			  PhysicalLogCommandReaderV3_0 reader = new PhysicalLogCommandReaderV3_0();
			  Command command = reader.Read( channel );
			  assertTrue( command is Command.NodeCommand );

			  Command.NodeCommand nodeCommand = ( Command.NodeCommand ) command;

			  // Then
			  assertEquals( before, nodeCommand.Before );
			  assertEquals( after, nodeCommand.After );
			  assertTrue( nodeCommand.Before.UseFixedReferences );
			  assertTrue( nodeCommand.After.UseFixedReferences );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nodeCommandWithFixedReferenceFormat302() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NodeCommandWithFixedReferenceFormat302()
		 {
			  // Given
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();
			  NodeRecord before = new NodeRecord( 42, true, false, 33, 99, 66 );
			  NodeRecord after = new NodeRecord( 42, true, false, 33, 99, 66 );
			  before.UseFixedReferences = true;
			  after.UseFixedReferences = true;

			  ( new Command.NodeCommand( before, after ) ).Serialize( channel );

			  // When
			  PhysicalLogCommandReaderV3_0_2 reader = new PhysicalLogCommandReaderV3_0_2();
			  Command command = reader.Read( channel );
			  assertTrue( command is Command.NodeCommand );

			  Command.NodeCommand nodeCommand = ( Command.NodeCommand ) command;

			  // Then
			  assertEquals( before, nodeCommand.Before );
			  assertEquals( after, nodeCommand.After );
			  assertTrue( nodeCommand.Before.UseFixedReferences );
			  assertTrue( nodeCommand.After.UseFixedReferences );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readPropertyCommandWithSecondaryUnit() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadPropertyCommandWithSecondaryUnit()
		 {
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();
			  PropertyRecord before = new PropertyRecord( 1 );
			  PropertyRecord after = new PropertyRecord( 2 );
			  after.RequiresSecondaryUnit = true;
			  after.SecondaryUnitId = 78;

			  ( new Command.PropertyCommand( before, after ) ).Serialize( channel );

			  PhysicalLogCommandReaderV3_0 reader = new PhysicalLogCommandReaderV3_0();
			  Command command = reader.Read( channel );
			  assertTrue( command is Command.PropertyCommand );

			  Command.PropertyCommand neoStoreCommand = ( Command.PropertyCommand ) command;

			  // Then
			  assertEquals( before.NextProp, neoStoreCommand.Before.NextProp );
			  assertEquals( after.NextProp, neoStoreCommand.After.NextProp );
			  VerifySecondaryUnit( after, neoStoreCommand.After );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readPropertyCommandWithNonRequiredSecondaryUnit() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadPropertyCommandWithNonRequiredSecondaryUnit()
		 {
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();
			  PropertyRecord before = new PropertyRecord( 1 );
			  PropertyRecord after = new PropertyRecord( 2 );
			  after.RequiresSecondaryUnit = false;
			  after.SecondaryUnitId = 78;

			  ( new Command.PropertyCommand( before, after ) ).Serialize( channel );

			  PhysicalLogCommandReaderV3_0 reader = new PhysicalLogCommandReaderV3_0();
			  Command command = reader.Read( channel );
			  assertTrue( command is Command.PropertyCommand );

			  Command.PropertyCommand neoStoreCommand = ( Command.PropertyCommand ) command;

			  // Then
			  assertEquals( before.NextProp, neoStoreCommand.Before.NextProp );
			  assertEquals( after.NextProp, neoStoreCommand.After.NextProp );
			  VerifySecondaryUnit( after, neoStoreCommand.After );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readPropertyCommandWithFixedReferenceFormat300() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadPropertyCommandWithFixedReferenceFormat300()
		 {
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();
			  PropertyRecord before = new PropertyRecord( 1 );
			  PropertyRecord after = new PropertyRecord( 2 );
			  before.UseFixedReferences = true;
			  after.UseFixedReferences = true;

			  ( new Command.PropertyCommand( before, after ) ).Serialize( channel );

			  PhysicalLogCommandReaderV3_0 reader = new PhysicalLogCommandReaderV3_0();
			  Command command = reader.Read( channel );
			  assertTrue( command is Command.PropertyCommand );

			  Command.PropertyCommand neoStoreCommand = ( Command.PropertyCommand ) command;

			  // Then
			  assertEquals( before.NextProp, neoStoreCommand.Before.NextProp );
			  assertEquals( after.NextProp, neoStoreCommand.After.NextProp );
			  assertTrue( neoStoreCommand.Before.UseFixedReferences );
			  assertTrue( neoStoreCommand.After.UseFixedReferences );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readPropertyCommandWithFixedReferenceFormat302() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadPropertyCommandWithFixedReferenceFormat302()
		 {
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();
			  PropertyRecord before = new PropertyRecord( 1 );
			  PropertyRecord after = new PropertyRecord( 2 );
			  before.UseFixedReferences = true;
			  after.UseFixedReferences = true;

			  ( new Command.PropertyCommand( before, after ) ).Serialize( channel );

			  PhysicalLogCommandReaderV3_0_2 reader = new PhysicalLogCommandReaderV3_0_2();
			  Command command = reader.Read( channel );
			  assertTrue( command is Command.PropertyCommand );

			  Command.PropertyCommand neoStoreCommand = ( Command.PropertyCommand ) command;

			  // Then
			  assertEquals( before.NextProp, neoStoreCommand.Before.NextProp );
			  assertEquals( after.NextProp, neoStoreCommand.After.NextProp );
			  assertTrue( neoStoreCommand.Before.UseFixedReferences );
			  assertTrue( neoStoreCommand.After.UseFixedReferences );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadSomeCommands() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadSomeCommands()
		 {
			  // GIVEN
			  InMemoryClosableChannel channel = new InMemoryClosableChannel();
			  Commands.CreateNode( 0 ).serialize( channel );
			  Commands.CreateNode( 1 ).serialize( channel );
			  Commands.CreateRelationshipTypeToken( 0, 0 ).serialize( channel );
			  Commands.CreateRelationship( 0, 0, 1, 0 ).serialize( channel );
			  Commands.CreatePropertyKeyToken( 0, 0 ).serialize( channel );
			  Commands.CreateProperty( 0, PropertyType.SHORT_STRING, 0 ).serialize( channel );
			  CommandReader reader = new PhysicalLogCommandReaderV3_0();

			  // THEN
			  assertTrue( reader.Read( channel ) is Command.NodeCommand );
			  assertTrue( reader.Read( channel ) is Command.NodeCommand );
			  assertTrue( reader.Read( channel ) is Command.RelationshipTypeTokenCommand );
			  assertTrue( reader.Read( channel ) is Command.RelationshipCommand );
			  assertTrue( reader.Read( channel ) is Command.PropertyKeyTokenCommand );
			  assertTrue( reader.Read( channel ) is Command.PropertyCommand );
		 }

		 private void VerifySecondaryUnit<T>( T record, T commandRecord ) where T : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  assertEquals( "Secondary unit requirements should be the same", record.requiresSecondaryUnit(), commandRecord.requiresSecondaryUnit() );
			  assertEquals( "Secondary unit ids should be the same", record.SecondaryUnitId, commandRecord.SecondaryUnitId );
		 }
	}

}
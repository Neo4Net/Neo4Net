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
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;
	using ParameterizedTest = org.junit.jupiter.@params.ParameterizedTest;
	using EnumSource = org.junit.jupiter.@params.provider.EnumSource;


	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PrimitiveRecord = Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using Neo4Net.Kernel.impl.transaction.command.Command;
	using NodeCommand = Neo4Net.Kernel.impl.transaction.command.Command.NodeCommand;
	using RelationshipCommand = Neo4Net.Kernel.impl.transaction.command.Command.RelationshipCommand;
	using Inject = Neo4Net.Test.extension.Inject;
	using RandomExtension = Neo4Net.Test.extension.RandomExtension;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(RandomExtension.class) class IEntityCommandGrouperTest
	internal class IEntityCommandGrouperTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.test.rule.RandomRule random;
		 private RandomRule _random;

		 private long _nextPropertyId;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @EnumSource(Factory.class) void shouldHandleEmptyList(Factory factory)
		 internal virtual void ShouldHandleEmptyList( Factory factory )
		 {
			  // given
			  IEntityCommandGrouper grouper = new IEntityCommandGrouper<>( factory.command( 0 ).GetType(), 8 );

			  // when
			  IEntityCommandGrouper.Cursor cursor = grouper.sortAndAccessGroups();
			  bool hasNext = cursor.NextEntity();

			  // then
			  assertFalse( hasNext );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @EnumSource(Factory.class) void shouldSeeSingleGroupOfPropertiesWithEntity(Factory factory)
		 internal virtual void ShouldSeeSingleGroupOfPropertiesWithEntity( Factory factory )
		 {
			  // given
			  IEntityCommandGrouper grouper = new IEntityCommandGrouper<>( factory.command( 0 ).GetType(), 8 );
			  long IEntityId = 1;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.kernel.impl.transaction.command.Command.BaseCommand<? extends org.Neo4Net.kernel.impl.store.record.PrimitiveRecord> IEntity = factory.command(entityId);
			  Command.BaseCommand<PrimitiveRecord> IEntity = factory.command( IEntityId );
			  Command.PropertyCommand property1 = Property( IEntity.After );
			  Command.PropertyCommand property2 = Property( IEntity.After );
			  grouper.add( property1 );
			  grouper.add( property2 );
			  grouper.add( IEntity ); // <-- deliberately out-of-place
			  IEntityCommandGrouper.Cursor cursor = grouper.sortAndAccessGroups();

			  // when/then
			  AssertGroups( cursor, Group( IEntityId, IEntity, property1, property2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @EnumSource(Factory.class) void shouldSeeSingleGroupOfPropertiesWithoutEntity(Factory factory)
		 internal virtual void ShouldSeeSingleGroupOfPropertiesWithoutEntity( Factory factory )
		 {
			  // given
			  IEntityCommandGrouper grouper = new IEntityCommandGrouper<>( factory.command( 0 ).GetType(), 8 );
			  long IEntityId = 1;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.kernel.impl.transaction.command.Command.BaseCommand<? extends org.Neo4Net.kernel.impl.store.record.PrimitiveRecord> IEntity = factory.command(entityId);
			  Command.BaseCommand<PrimitiveRecord> IEntity = factory.command( IEntityId );
			  Command.PropertyCommand property1 = Property( IEntity.After );
			  Command.PropertyCommand property2 = Property( IEntity.After );
			  // intentionally DO NOT add the IEntity command
			  grouper.add( property1 );
			  grouper.add( property2 );
			  IEntityCommandGrouper.Cursor cursor = grouper.sortAndAccessGroups();

			  // when/then
			  AssertGroups( cursor, Group( IEntityId, null, property1, property2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @EnumSource(Factory.class) void shouldSeeMultipleGroupsSomeOfThemWithEntity(Factory factory)
		 internal virtual void ShouldSeeMultipleGroupsSomeOfThemWithEntity( Factory factory )
		 {
			  // given
			  IEntityCommandGrouper grouper = new IEntityCommandGrouper<>( factory.command( 0 ).GetType(), 64 );
			  Group[] groups = new Group[_random.Next( 10, 30 )];
			  for ( int IEntityId = 0; IEntityId < groups.Length; IEntityId++ )
			  {
					Command.BaseCommand IEntityCommand = _random.nextBoolean() ? factory.command(entityId) : null;
					groups[entityId] = new Group( IEntityId, IEntityCommand );
					if ( IEntityCommand != null )
					{
						 grouper.add( IEntityCommand ); // <-- storage transaction logs are sorted such that IEntity commands comes before property commands
					}
			  }
			  int totalNumberOfProperties = _random.Next( 10, 100 );
			  for ( int i = 0; i < totalNumberOfProperties; i++ )
			  {
					int IEntityId = _random.Next( groups.Length );
					Command.PropertyCommand property = property( factory.command( IEntityId ).After );
					groups[entityId].AddProperty( property );
					grouper.add( property );
			  }
			  // ^^^ OK so we've generated property commands for random entities in random order, let's sort them
			  IEntityCommandGrouper.Cursor cursor = grouper.sortAndAccessGroups();

			  // then
			  AssertGroups( cursor, groups );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @EnumSource(Factory.class) void shouldWorkOnADifferentSetOfCommandsAfterClear(Factory factory)
		 internal virtual void ShouldWorkOnADifferentSetOfCommandsAfterClear( Factory factory )
		 {
			  // given
			  IEntityCommandGrouper grouper = new IEntityCommandGrouper<>( factory.command( 0 ).GetType(), 16 );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.kernel.impl.transaction.command.Command.BaseCommand<? extends org.Neo4Net.kernel.impl.store.record.PrimitiveRecord> IEntity0 = factory.command(0);
			  Command.BaseCommand<PrimitiveRecord> IEntity0 = factory.command( 0 );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.kernel.impl.transaction.command.Command.BaseCommand<? extends org.Neo4Net.kernel.impl.store.record.PrimitiveRecord> IEntity1 = factory.command(1);
			  Command.BaseCommand<PrimitiveRecord> IEntity1 = factory.command( 1 );
			  grouper.add( IEntity0 );
			  grouper.add( IEntity1 );
			  grouper.add( Property( IEntity0.After ) );
			  grouper.add( Property( IEntity1.After ) );
			  grouper.clear();

			  // when
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.kernel.impl.transaction.command.Command.BaseCommand<? extends org.Neo4Net.kernel.impl.store.record.PrimitiveRecord> IEntity2 = factory.command(2);
			  Command.BaseCommand<PrimitiveRecord> IEntity2 = factory.command( 2 );
			  Command.PropertyCommand IEntityProperty = Property( IEntity2.After );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.kernel.impl.transaction.command.Command.BaseCommand<? extends org.Neo4Net.kernel.impl.store.record.PrimitiveRecord> IEntity3 = factory.command(3);
			  Command.BaseCommand<PrimitiveRecord> IEntity3 = factory.command( 3 );
			  grouper.add( IEntity2 );
			  grouper.add( IEntityProperty );
			  grouper.add( IEntity3 );

			  // then
			  AssertGroups( grouper.sortAndAccessGroups(), Group(entity2.Key, IEntity2, IEntityProperty), Group(entity3.Key, IEntity3) );
		 }

		 private void AssertGroups( IEntityCommandGrouper.Cursor cursor, params Group[] groups )
		 {
			  foreach ( Group group in groups )
			  {
					if ( group.Empty )
					{
						 continue;
					}
					assertTrue( cursor.NextEntity() );
					group.AssertGroup( cursor );
			  }
			  assertFalse( cursor.NextEntity() );
		 }

		 private Group Group<T1>( long IEntityId, Command.BaseCommand<T1> IEntityCommand, params Command.PropertyCommand[] properties ) where T1 : Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord
		 {
			  return new Group( IEntityId, IEntityCommand, properties );
		 }

		 private Command.PropertyCommand Property( PrimitiveRecord owner )
		 {
			  long propertyId = _nextPropertyId++;
			  return new Command.PropertyCommand( new PropertyRecord( propertyId, owner ), new PropertyRecord( propertyId, owner ) );
		 }

		 private class Group
		 {
			  internal readonly long IEntityId;
			  internal readonly Command IEntityCommand;
			  internal readonly ISet<Command.PropertyCommand> Properties = new HashSet<Command.PropertyCommand>();

			  internal Group( long IEntityId, Command IEntityCommand, params Command.PropertyCommand[] properties )
			  {
					this.EntityId = IEntityId;
					this.EntityCommand = IEntityCommand;
					this.Properties.addAll( Arrays.asList( properties ) );
			  }

			  internal virtual void AddProperty( Command.PropertyCommand property )
			  {
					Properties.Add( property );
			  }

			  internal virtual void AssertGroup( IEntityCommandGrouper.Cursor cursor )
			  {
					assertEquals( IEntityId, cursor.CurrentEntityId() );
					assertSame( IEntityCommand, cursor.CurrentEntityCommand() );
					ISet<Command.PropertyCommand> fromGrouper = new HashSet<Command.PropertyCommand>();
					while ( true )
					{
						 Command.PropertyCommand property = cursor.NextProperty();
						 if ( property == null )
						 {
							  break;
						 }
						 fromGrouper.Add( property );
					}
					assertEquals( fromGrouper, Properties );
			  }

			  internal virtual bool Empty
			  {
				  get
				  {
						return IEntityCommand == null && Properties.Count == 0;
				  }
			  }
		 }

		 private abstract class Factory
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           NODE { NodeCommand command(long value) { return new org.Neo4Net.kernel.impl.transaction.command.Command.NodeCommand(new org.Neo4Net.kernel.impl.store.record.NodeRecord(value), new org.Neo4Net.kernel.impl.store.record.NodeRecord(value)); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           RELATIONSHIP { RelationshipCommand command(long value) { return new org.Neo4Net.kernel.impl.transaction.command.Command.RelationshipCommand(new org.Neo4Net.kernel.impl.store.record.RelationshipRecord(value), new org.Neo4Net.kernel.impl.store.record.RelationshipRecord(value)); } };

			  private static readonly IList<Factory> valueList = new List<Factory>();

			  static Factory()
			  {
				  valueList.Add( NODE );
				  valueList.Add( RELATIONSHIP );
			  }

			  public enum InnerEnum
			  {
				  NODE,
				  RELATIONSHIP
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private Factory( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal abstract Neo4Net.Kernel.impl.transaction.command.Command.BaseCommand<JavaToDotNetGenericWildcard extends Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord> command( long id );

			 public static IList<Factory> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static Factory valueOf( string name )
			 {
				 foreach ( Factory enumInstance in Factory.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }
	}

}
﻿/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.tools.applytx
{
	using Arguments = io.airlift.airline.Arguments;
	using Cli = io.airlift.airline.Cli;
	using CliBuilder = io.airlift.airline.Cli.CliBuilder;


	using NamedToken = Org.Neo4j.@internal.Kernel.Api.NamedToken;
	using LabelTokenStore = Org.Neo4j.Kernel.impl.store.LabelTokenStore;
	using PropertyKeyTokenStore = Org.Neo4j.Kernel.impl.store.PropertyKeyTokenStore;
	using Org.Neo4j.Kernel.impl.store;
	using RelationshipTypeTokenStore = Org.Neo4j.Kernel.impl.store.RelationshipTypeTokenStore;
	using StoreAccess = Org.Neo4j.Kernel.impl.store.StoreAccess;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using Command = Org.Neo4j.tools.console.input.Command;
	using ConsoleInput = Org.Neo4j.tools.console.input.ConsoleInput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.NORMAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.tools.console.input.ConsoleUtil.airlineHelp;

	/// <summary>
	/// Able to dump records and record chains. Works as a <seealso cref="ConsoleInput"/> <seealso cref="Command"/>.
	/// </summary>
	public class DumpRecordsCommand : Command
	{
		 public const string NAME = "dump";

		 private interface Action
		 {
			  void Run( StoreAccess store, PrintStream @out );
		 }

		 private readonly Cli<Action> _cli;
		 private readonly System.Func<StoreAccess> _store;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public DumpRecordsCommand(System.Func<org.neo4j.kernel.impl.store.StoreAccess> store)
		 public DumpRecordsCommand( System.Func<StoreAccess> store )
		 {
			  this._store = store;
			  Cli.CliBuilder<Action> builder = Cli.builder<Action>( NAME ).withDescription( "Dump record contents" ).withCommands( typeof( DumpRelationshipTypes ), typeof( Help ) ).withDefaultCommand( typeof( Help ) );
			  builder.withGroup( "node" ).withCommands( typeof( DumpNodePropertyChain ), typeof( DumpNodeRelationshipChain ), typeof( Help ) ).withDefaultCommand( typeof( Help ) );
			  builder.withGroup( "relationship" ).withCommands( typeof( DumpRelationshipPropertyChain ), typeof( Help ) ).withDefaultCommand( typeof( Help ) );
			  builder.withGroup( "tokens" ).withCommands( typeof( DumpRelationshipTypes ), typeof( DumpLabels ), typeof( DumpPropertyKeys ), typeof( Help ) ).withDefaultCommand( typeof( Help ) );
			  this._cli = builder.build();
		 }

		 public override void Run( string[] args, PrintStream @out )
		 {
			  _cli.parse( args ).run( _store.get(), @out );
		 }

		 public override string ToString()
		 {
			  return airlineHelp( _cli );
		 }

		 internal abstract class DumpPropertyChain : Action
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Arguments(title = "id", description = "Entity id", required = true) public long id;
			  public long Id;

			  protected internal abstract long FirstPropId( StoreAccess access );

			  public override void Run( StoreAccess store, PrintStream @out )
			  {
					long propId = FirstPropId( store );
					RecordStore<PropertyRecord> propertyStore = store.PropertyStore;
					PropertyRecord record = propertyStore.NewRecord();
					while ( propId != Record.NO_NEXT_PROPERTY.intValue() )
					{
						 propertyStore.GetRecord( propId, record, NORMAL );
						 // We rely on this method having the side-effect of loading the property blocks:
						 record.NumberOfProperties();
						 @out.println( record );
						 propId = record.NextProp;
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @io.airlift.airline.Command(name = "properties", description = "Dump property chain for a node") public static class DumpNodePropertyChain extends DumpPropertyChain
		 public class DumpNodePropertyChain : DumpPropertyChain
		 {
			  protected internal override long FirstPropId( StoreAccess access )
			  {
					RecordStore<NodeRecord> nodeStore = access.NodeStore;
					return nodeStore.GetRecord( Id, nodeStore.NewRecord(), NORMAL ).NextProp;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @io.airlift.airline.Command(name = "properties", description = "Dump property chain for a relationship") public static class DumpRelationshipPropertyChain extends DumpPropertyChain
		 public class DumpRelationshipPropertyChain : DumpPropertyChain
		 {
			  protected internal override long FirstPropId( StoreAccess access )
			  {
					RecordStore<RelationshipRecord> relationshipStore = access.RelationshipStore;
					return relationshipStore.GetRecord( Id, relationshipStore.NewRecord(), NORMAL ).NextProp;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @io.airlift.airline.Command(name = "relationships", description = "Dump relationship chain for a node") public static class DumpNodeRelationshipChain implements Action
		 public class DumpNodeRelationshipChain : Action
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Arguments(description = "Node id", required = true) public long id;
			  public long Id;

			  public override void Run( StoreAccess store, PrintStream @out )
			  {
					RecordStore<NodeRecord> nodeStore = store.NodeStore;
					NodeRecord node = nodeStore.GetRecord( Id, nodeStore.NewRecord(), NORMAL );
					if ( node.Dense )
					{
						 RecordStore<RelationshipGroupRecord> relationshipGroupStore = store.RelationshipGroupStore;
						 RelationshipGroupRecord group = relationshipGroupStore.NewRecord();
						 relationshipGroupStore.GetRecord( node.NextRel, group, NORMAL );
						 do
						 {
							  @out.println( "group " + group );
							  @out.println( "out:" );
							  PrintRelChain( store, @out, group.FirstOut );
							  @out.println( "in:" );
							  PrintRelChain( store, @out, group.FirstIn );
							  @out.println( "loop:" );
							  PrintRelChain( store, @out, group.FirstLoop );
							  group = group.Next != -1 ? relationshipGroupStore.GetRecord( group.Next, group, NORMAL ) : null;
						 } while ( group != null );
					}
					else
					{
						 PrintRelChain( store, @out, node.NextRel );
					}
			  }

			  internal virtual void PrintRelChain( StoreAccess access, PrintStream @out, long firstRelId )
			  {
					for ( long rel = firstRelId; rel != Record.NO_NEXT_RELATIONSHIP.intValue(); )
					{
						 RecordStore<RelationshipRecord> relationshipStore = access.RelationshipStore;
						 RelationshipRecord record = relationshipStore.GetRecord( rel, relationshipStore.NewRecord(), NORMAL );
						 @out.println( rel + "\t" + record );
						 if ( record.FirstNode == Id )
						 {
							  rel = record.FirstNextRel;
						 }
						 else
						 {
							  rel = record.SecondNextRel;
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @io.airlift.airline.Command(name = "relationship-type", description = "Dump relationship type tokens") public static class DumpRelationshipTypes implements Action
		 public class DumpRelationshipTypes : Action
		 {
			  public override void Run( StoreAccess store, PrintStream @out )
			  {
					foreach ( NamedToken token in ( ( RelationshipTypeTokenStore ) store.RelationshipTypeTokenStore ).Tokens )
					{
						 @out.println( token );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @io.airlift.airline.Command(name = "label", description = "Dump label tokens") public static class DumpLabels implements Action
		 public class DumpLabels : Action
		 {
			  public override void Run( StoreAccess store, PrintStream @out )
			  {
					foreach ( NamedToken token in ( ( LabelTokenStore ) store.LabelTokenStore ).Tokens )
					{
						 @out.println( token );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @io.airlift.airline.Command(name = "property-key", description = "Dump property key tokens") public static class DumpPropertyKeys implements Action
		 public class DumpPropertyKeys : Action
		 {
			  public override void Run( StoreAccess store, PrintStream @out )
			  {
					foreach ( NamedToken token in ( ( PropertyKeyTokenStore ) store.PropertyKeyTokenStore ).Tokens )
					{
						 @out.println( token );
					}
			  }
		 }

		 public class Help : io.airlift.airline.Help, Action
		 {
			  public override void Run( StoreAccess store, PrintStream @out )
			  {
					run();
			  }
		 }
	}

}
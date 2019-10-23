﻿using System.Collections.Generic;

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
namespace Neo4Net.Consistency.checking
{

	using ConsistencyReport_RelationshipGroupConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport;
	using RecordAccess = Neo4Net.Consistency.store.RecordAccess;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;

	public class RelationshipGroupRecordCheck : RecordCheck<RelationshipGroupRecord, ConsistencyReport_RelationshipGroupConsistencyReport>
	{
		 private static readonly IList<RecordField<RelationshipGroupRecord, ConsistencyReport_RelationshipGroupConsistencyReport>> _fields;
		 static RelationshipGroupRecordCheck()
		 {
			  IList<RecordField<RelationshipGroupRecord, ConsistencyReport_RelationshipGroupConsistencyReport>> list = new List<RecordField<RelationshipGroupRecord, ConsistencyReport_RelationshipGroupConsistencyReport>>();
			  list.Add( RelationshipTypeField.RelationshipType );
			  list.Add( GroupField.Next );
			  list.Add( NodeField.Owner );
			  ( ( IList<RecordField<RelationshipGroupRecord, ConsistencyReport_RelationshipGroupConsistencyReport>> )list ).AddRange( asList( RelationshipField.values() ) );
			  _fields = Collections.unmodifiableList( list );
		 }

		 private sealed class NodeField : RecordField<RelationshipGroupRecord, RelationshipGroupConsistencyReport>, ComparativeRecordChecker<RelationshipGroupRecord, NodeRecord, RelationshipGroupConsistencyReport>
		 {
			  public static readonly NodeField Owner = new NodeField( "Owner", InnerEnum.Owner );

			  private static readonly IList<NodeField> valueList = new List<NodeField>();

			  static NodeField()
			  {
				  valueList.Add( Owner );
			  }

			  public enum InnerEnum
			  {
				  Owner
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private NodeField( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  public void CheckReference( Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord record, Neo4Net.Kernel.Impl.Store.Records.NodeRecord referred, CheckerEngine<Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport> engine, Neo4Net.Consistency.store.RecordAccess records )
			  {
					if ( !referred.InUse() )
					{
						 engine.Report().ownerNotInUse();
					}
			  }

			  public void CheckConsistency( Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord record, CheckerEngine<Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport> engine, Neo4Net.Consistency.store.RecordAccess records )
			  {
					if ( record.OwningNode < 0 )
					{
						 engine.Report().illegalOwner();
					}
					else
					{
						 engine.ComparativeCheck( records.Node( record.OwningNode ), this );
					}
			  }

			  public long ValueFrom( Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord record )
			  {
					return record.OwningNode;
			  }

			 public static IList<NodeField> values()
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

			 public static NodeField ValueOf( string name )
			 {
				 foreach ( NodeField enumInstance in NodeField.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private sealed class RelationshipTypeField : RecordField<RelationshipGroupRecord, RelationshipGroupConsistencyReport>, ComparativeRecordChecker<RelationshipGroupRecord, RelationshipTypeTokenRecord, RelationshipGroupConsistencyReport>
		 {
			  public static readonly RelationshipTypeField RelationshipType = new RelationshipTypeField( "RelationshipType", InnerEnum.RelationshipType );

			  private static readonly IList<RelationshipTypeField> valueList = new List<RelationshipTypeField>();

			  static RelationshipTypeField()
			  {
				  valueList.Add( RelationshipType );
			  }

			  public enum InnerEnum
			  {
				  RelationshipType
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private RelationshipTypeField( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  public void CheckConsistency( Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord record, CheckerEngine<Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport> engine, Neo4Net.Consistency.store.RecordAccess records )
			  {
					if ( record.Type < 0 )
					{
						 engine.Report().illegalRelationshipType();
					}
					else
					{
						 engine.ComparativeCheck( records.RelationshipType( record.Type ), this );
					}
			  }

			  public long ValueFrom( Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord record )
			  {
					return record.Type;
			  }

			  public void CheckReference( Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord record, Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord referred, CheckerEngine<Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport> engine, Neo4Net.Consistency.store.RecordAccess records )
			  {
					if ( !referred.InUse() )
					{
						 engine.Report().relationshipTypeNotInUse(referred);
					}
			  }

			 public static IList<RelationshipTypeField> values()
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

			 public static RelationshipTypeField ValueOf( string name )
			 {
				 foreach ( RelationshipTypeField enumInstance in RelationshipTypeField.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private sealed class GroupField : RecordField<RelationshipGroupRecord, RelationshipGroupConsistencyReport>, ComparativeRecordChecker<RelationshipGroupRecord, RelationshipGroupRecord, RelationshipGroupConsistencyReport>
		 {
			  public static readonly GroupField Next = new GroupField( "Next", InnerEnum.Next );

			  private static readonly IList<GroupField> valueList = new List<GroupField>();

			  static GroupField()
			  {
				  valueList.Add( Next );
			  }

			  public enum InnerEnum
			  {
				  Next
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private GroupField( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  public void CheckConsistency( Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord record, CheckerEngine<Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport> engine, Neo4Net.Consistency.store.RecordAccess records )
			  {
					if ( record.Next != Record.NO_NEXT_RELATIONSHIP.intValue() )
					{
						 engine.ComparativeCheck( records.RelationshipGroup( record.Next ), this );
					}
			  }

			  public long ValueFrom( Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord record )
			  {
					return record.Next;
			  }

			  public void CheckReference( Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord record, Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord referred, CheckerEngine<Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport> engine, Neo4Net.Consistency.store.RecordAccess records )
			  {
					if ( !referred.InUse() )
					{
						 engine.Report().nextGroupNotInUse();
					}
					else
					{
						 if ( record.Type >= referred.Type )
						 {
							  engine.Report().invalidTypeSortOrder();
						 }
						 if ( record.OwningNode != referred.OwningNode )
						 {
							  engine.Report().nextHasOtherOwner(referred);
						 }
					}
			  }

			 public static IList<GroupField> values()
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

			 public static GroupField ValueOf( string name )
			 {
				 foreach ( GroupField enumInstance in GroupField.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private abstract class RelationshipField : RecordField<RelationshipGroupRecord, RelationshipGroupConsistencyReport>, ComparativeRecordChecker<RelationshipGroupRecord, RelationshipRecord, RelationshipGroupConsistencyReport>
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           OUT { public long valueFrom(org.Neo4Net.kernel.impl.store.record.RelationshipGroupRecord record) { return record.getFirstOut(); } protected void relationshipNotInUse(org.Neo4Net.consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport report) { report.firstOutgoingRelationshipNotInUse(); } protected void relationshipNotFirstInChain(org.Neo4Net.consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport report) { report.firstOutgoingRelationshipNotFirstInChain(); } protected boolean isFirstInChain(org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship) { return relationship.isFirstInFirstChain(); } protected void relationshipOfOtherType(org.Neo4Net.consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport report) { report.firstOutgoingRelationshipOfOfOtherType(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           IN { public long valueFrom(org.Neo4Net.kernel.impl.store.record.RelationshipGroupRecord record) { return record.getFirstIn(); } protected void relationshipNotInUse(org.Neo4Net.consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport report) { report.firstIncomingRelationshipNotInUse(); } protected void relationshipNotFirstInChain(org.Neo4Net.consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport report) { report.firstIncomingRelationshipNotFirstInChain(); } protected boolean isFirstInChain(org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship) { return relationship.isFirstInSecondChain(); } protected void relationshipOfOtherType(org.Neo4Net.consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport report) { report.firstIncomingRelationshipOfOfOtherType(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           LOOP { public long valueFrom(org.Neo4Net.kernel.impl.store.record.RelationshipGroupRecord record) { return record.getFirstLoop(); } protected void relationshipNotInUse(org.Neo4Net.consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport report) { report.firstLoopRelationshipNotInUse(); } protected void relationshipNotFirstInChain(org.Neo4Net.consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport report) { report.firstLoopRelationshipNotFirstInChain(); } protected boolean isFirstInChain(org.Neo4Net.kernel.impl.store.record.RelationshipRecord relationship) { return relationship.isFirstInFirstChain() && relationship.isFirstInSecondChain(); } protected void relationshipOfOtherType(org.Neo4Net.consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport report) { report.firstLoopRelationshipOfOfOtherType(); } };

			  private static readonly IList<RelationshipField> valueList = new List<RelationshipField>();

			  static RelationshipField()
			  {
				  valueList.Add( OUT );
				  valueList.Add( IN );
				  valueList.Add( LOOP );
			  }

			  public enum InnerEnum
			  {
				  OUT,
				  IN,
				  LOOP
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private RelationshipField( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  public void checkReference( Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord record, Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord referred, CheckerEngine<Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport> engine, Neo4Net.Consistency.store.RecordAccess records )
			  {
				  if ( !referred.inUse() ) { relationshipNotInUse(engine.report()); } else
				  {
					  if ( !isFirstInChain( referred ) ) { relationshipNotFirstInChain( engine.report() ); } if (referred.getType() != record.getType()) { relationshipOfOtherType(engine.report()); }
				  }
			  }
			  protected internal abstract void relationshipOfOtherType( Neo4Net.Consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport report );

			  protected internal abstract void relationshipNotFirstInChain( Neo4Net.Consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport report );

			  protected internal abstract bool isFirstInChain( Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord referred );

			  protected internal abstract void relationshipNotInUse( Neo4Net.Consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport report );

			 public static IList<RelationshipField> values()
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

			 public static RelationshipField ValueOf( string name )
			 {
				 foreach ( RelationshipField enumInstance in RelationshipField.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 public override void Check( RelationshipGroupRecord record, CheckerEngine<RelationshipGroupRecord, ConsistencyReport_RelationshipGroupConsistencyReport> engine, RecordAccess records )
		 {
			  if ( !record.InUse() )
			  {
					return;
			  }
			  foreach ( RecordField<RelationshipGroupRecord, ConsistencyReport_RelationshipGroupConsistencyReport> field in _fields )
			  {
					field.CheckConsistency( record, engine, records );
			  }
		 }
	}

}
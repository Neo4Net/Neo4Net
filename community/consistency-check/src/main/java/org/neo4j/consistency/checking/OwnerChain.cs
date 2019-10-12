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
namespace Org.Neo4j.Consistency.checking
{
	using ConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport;
	using RecordAccess = Org.Neo4j.Consistency.store.RecordAccess;
	using Org.Neo4j.Consistency.store;
	using NeoStoreRecord = Org.Neo4j.Kernel.impl.store.record.NeoStoreRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PrimitiveRecord = Org.Neo4j.Kernel.impl.store.record.PrimitiveRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;

	// TODO: it would be great if this also checked for cyclic chains. (we would also need cycle checking for full check, and for relationships)
	public abstract class OwnerChain : ComparativeRecordChecker<PropertyRecord, PropertyRecord, ConsistencyReport.PropertyConsistencyReport>
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       NEW { RecordReference<org.neo4j.kernel.impl.store.record.PropertyRecord> property(org.neo4j.consistency.store.RecordAccess records, long id) { return records.property(id); } RecordReference<org.neo4j.kernel.impl.store.record.NodeRecord> node(org.neo4j.consistency.store.RecordAccess records, long id) { return records.node(id); } RecordReference<org.neo4j.kernel.impl.store.record.RelationshipRecord> relationship(org.neo4j.consistency.store.RecordAccess records, long id) { return records.relationship(id); } RecordReference<org.neo4j.kernel.impl.store.record.NeoStoreRecord> graph(org.neo4j.consistency.store.RecordAccess records) { return records.graph(); } void wrongOwner(org.neo4j.consistency.report.ConsistencyReport_PropertyConsistencyReport report) { report.ownerDoesNotReferenceBack(); } };

		 private static readonly IList<OwnerChain> valueList = new List<OwnerChain>();

		 static OwnerChain()
		 {
			 valueList.Add( NEW );
		 }

		 public enum InnerEnum
		 {
			 NEW
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private OwnerChain( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 private readonly ComparativeRecordChecker<Org.Neo4j.Kernel.impl.store.record.PropertyRecord, Org.Neo4j.Kernel.impl.store.record.PrimitiveRecord, Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport> OWNER_CHECK = ( record, owner, engine, records ) =>
		 {
					 if ( !owner.inUse() && !record.inUse() )
					 {
						  return;
					 }
					 if ( !owner.inUse() || Record.NO_NEXT_PROPERTY.@is(owner.NextProp) )
					 {
						  WrongOwner( engine.report() );
					 }
					 else if ( owner.NextProp != record.Id )
					 {
						  engine.comparativeCheck( Property( records, owner.NextProp ), OwnerChain.this );
					 }
		 };

		 public void CheckReference( Org.Neo4j.Kernel.impl.store.record.PropertyRecord record, Org.Neo4j.Kernel.impl.store.record.PropertyRecord property, CheckerEngine<Org.Neo4j.Kernel.impl.store.record.PropertyRecord, Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport> engine, Org.Neo4j.Consistency.store.RecordAccess records )
		 {
			  if ( record.Id != property.Id )
			  {
					if ( !property.InUse() || Record.NO_NEXT_PROPERTY.@is(property.NextProp) )
					{
						 WrongOwner( engine.Report() );
					}
					else if ( property.NextProp != record.Id )
					{
						 engine.ComparativeCheck( property( records, property.NextProp ), this );
					}
			  }
		 }

		 public void Check( Org.Neo4j.Kernel.impl.store.record.PropertyRecord record, CheckerEngine<Org.Neo4j.Kernel.impl.store.record.PropertyRecord, Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport> engine, Org.Neo4j.Consistency.store.RecordAccess records )
		 {
			  engine.ComparativeCheck( OwnerOf( record, records ), _ownerCheck );
		 }

		 private Org.Neo4j.Consistency.store.RecordReference<JavaToDotNetGenericWildcard extends Org.Neo4j.Kernel.impl.store.record.PrimitiveRecord> OwnerOf( Org.Neo4j.Kernel.impl.store.record.PropertyRecord record, Org.Neo4j.Consistency.store.RecordAccess records )
		 {
			  if ( record.NodeId != -1 )
			  {
					return Node( records, record.NodeId );
			  }
			  else if ( record.RelId != -1 )
			  {
					return Relationship( records, record.RelId );
			  }
			  else
			  {
					return Graph( records );
			  }
		 }

		 internal abstract Org.Neo4j.Consistency.store.RecordReference<Org.Neo4j.Kernel.impl.store.record.PropertyRecord> property( Org.Neo4j.Consistency.store.RecordAccess records, long id );

		 internal abstract Org.Neo4j.Consistency.store.RecordReference<Org.Neo4j.Kernel.impl.store.record.NodeRecord> node( Org.Neo4j.Consistency.store.RecordAccess records, long id );

		 internal abstract Org.Neo4j.Consistency.store.RecordReference<Org.Neo4j.Kernel.impl.store.record.RelationshipRecord> relationship( Org.Neo4j.Consistency.store.RecordAccess records, long id );

		 internal abstract Org.Neo4j.Consistency.store.RecordReference<Org.Neo4j.Kernel.impl.store.record.NeoStoreRecord> graph( Org.Neo4j.Consistency.store.RecordAccess records );

		 internal abstract void wrongOwner( Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport report );

		public static IList<OwnerChain> values()
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

		public static OwnerChain valueOf( string name )
		{
			foreach ( OwnerChain enumInstance in OwnerChain.valueList )
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
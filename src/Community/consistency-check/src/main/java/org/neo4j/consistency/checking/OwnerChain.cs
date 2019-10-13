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
namespace Neo4Net.Consistency.checking
{
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using RecordAccess = Neo4Net.Consistency.store.RecordAccess;
	using Neo4Net.Consistency.store;
	using NeoStoreRecord = Neo4Net.Kernel.impl.store.record.NeoStoreRecord;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using PrimitiveRecord = Neo4Net.Kernel.impl.store.record.PrimitiveRecord;
	using PropertyRecord = Neo4Net.Kernel.impl.store.record.PropertyRecord;
	using Record = Neo4Net.Kernel.impl.store.record.Record;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;

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

		 private readonly ComparativeRecordChecker<Neo4Net.Kernel.impl.store.record.PropertyRecord, Neo4Net.Kernel.impl.store.record.PrimitiveRecord, Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport> OWNER_CHECK = ( record, owner, engine, records ) =>
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

		 public void CheckReference( Neo4Net.Kernel.impl.store.record.PropertyRecord record, Neo4Net.Kernel.impl.store.record.PropertyRecord property, CheckerEngine<Neo4Net.Kernel.impl.store.record.PropertyRecord, Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport> engine, Neo4Net.Consistency.store.RecordAccess records )
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

		 public void Check( Neo4Net.Kernel.impl.store.record.PropertyRecord record, CheckerEngine<Neo4Net.Kernel.impl.store.record.PropertyRecord, Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport> engine, Neo4Net.Consistency.store.RecordAccess records )
		 {
			  engine.ComparativeCheck( OwnerOf( record, records ), _ownerCheck );
		 }

		 private Neo4Net.Consistency.store.RecordReference<JavaToDotNetGenericWildcard extends Neo4Net.Kernel.impl.store.record.PrimitiveRecord> OwnerOf( Neo4Net.Kernel.impl.store.record.PropertyRecord record, Neo4Net.Consistency.store.RecordAccess records )
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

		 internal abstract Neo4Net.Consistency.store.RecordReference<Neo4Net.Kernel.impl.store.record.PropertyRecord> property( Neo4Net.Consistency.store.RecordAccess records, long id );

		 internal abstract Neo4Net.Consistency.store.RecordReference<Neo4Net.Kernel.impl.store.record.NodeRecord> node( Neo4Net.Consistency.store.RecordAccess records, long id );

		 internal abstract Neo4Net.Consistency.store.RecordReference<Neo4Net.Kernel.impl.store.record.RelationshipRecord> relationship( Neo4Net.Consistency.store.RecordAccess records, long id );

		 internal abstract Neo4Net.Consistency.store.RecordReference<Neo4Net.Kernel.impl.store.record.NeoStoreRecord> graph( Neo4Net.Consistency.store.RecordAccess records );

		 internal abstract void wrongOwner( Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport report );

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
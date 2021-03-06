﻿using System;
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
namespace Org.Neo4j.Consistency.checking.full
{
	using IntIterator = org.eclipse.collections.api.iterator.IntIterator;
	using MutableIntObjectMap = org.eclipse.collections.api.map.primitive.MutableIntObjectMap;
	using MutableIntSet = org.eclipse.collections.api.set.primitive.MutableIntSet;
	using IntObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.IntObjectHashMap;
	using IntHashSet = org.eclipse.collections.impl.set.mutable.primitive.IntHashSet;


	using ConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport;
	using ConsistencyReporter = Org.Neo4j.Consistency.report.ConsistencyReporter;
	using LabelSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using RelationTypeSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.RelationTypeSchemaDescriptor;
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using SchemaProcessor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaProcessor;
	using SchemaStorage = Org.Neo4j.Kernel.impl.store.SchemaStorage;
	using StoreAccess = Org.Neo4j.Kernel.impl.store.StoreAccess;
	using ConstraintRule = Org.Neo4j.Kernel.impl.store.record.ConstraintRule;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PrimitiveRecord = Org.Neo4j.Kernel.impl.store.record.PrimitiveRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Numbers.safeCastLongToInt;

	public class MandatoryProperties
	{
		 private readonly MutableIntObjectMap<int[]> _nodes = new IntObjectHashMap<int[]>();
		 private readonly MutableIntObjectMap<int[]> _relationships = new IntObjectHashMap<int[]>();
		 private readonly StoreAccess _storeAccess;

		 public MandatoryProperties( StoreAccess storeAccess )
		 {
			  this._storeAccess = storeAccess;
			  SchemaStorage schemaStorage = new SchemaStorage( storeAccess.SchemaStore );
			  foreach ( ConstraintRule rule in ConstraintsIgnoringMalformed( schemaStorage ) )
			  {
					if ( rule.ConstraintDescriptor.enforcesPropertyExistence() )
					{
						 rule.Schema().processWith(constraintRecorder);
					}
			  }
		 }

		 private SchemaProcessor constraintRecorder = new SchemaProcessorAnonymousInnerClass();

		 private class SchemaProcessorAnonymousInnerClass : SchemaProcessor
		 {
			 public void processSpecific( LabelSchemaDescriptor schema )
			 {
				  foreach ( int propertyId in Schema.PropertyIds )
				  {
						RecordConstraint( Schema.LabelId, propertyId, outerInstance.nodes );
				  }
			 }

			 public void processSpecific( RelationTypeSchemaDescriptor schema )
			 {
				  foreach ( int propertyId in Schema.PropertyIds )
				  {
						RecordConstraint( Schema.RelTypeId, propertyId, outerInstance.relationships );
				  }
			 }

			 public void processSpecific( SchemaDescriptor schema )
			 {
				  throw new System.InvalidOperationException( "General SchemaDescriptors cannot support constraints" );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public System.Func<org.neo4j.kernel.impl.store.record.NodeRecord,Check<org.neo4j.kernel.impl.store.record.NodeRecord,org.neo4j.consistency.report.ConsistencyReport_NodeConsistencyReport>> forNodes(final org.neo4j.consistency.report.ConsistencyReporter reporter)
		 public virtual System.Func<NodeRecord, Check<NodeRecord, Org.Neo4j.Consistency.report.ConsistencyReport_NodeConsistencyReport>> ForNodes( ConsistencyReporter reporter )
		 {
			  return node =>
			  {
				MutableIntSet keys = null;
				foreach ( long labelId in NodeLabelReader.GetListOfLabels( node, _storeAccess.NodeDynamicLabelStore ) )
				{
					 // labelId _is_ actually an int. A technical detail in the store format has these come in a long[]
					 int[] propertyKeys = _nodes.get( safeCastLongToInt( labelId ) );
					 if ( propertyKeys != null )
					 {
						  if ( keys == null )
						  {
								keys = new IntHashSet( 16 );
						  }
						  foreach ( int key in propertyKeys )
						  {
								keys.add( key );
						  }
					 }
				}
				return keys != null ? new RealCheck<Org.Neo4j.Kernel.impl.store.record.NodeRecord, Check<Org.Neo4j.Kernel.impl.store.record.NodeRecord, Org.Neo4j.Consistency.report.ConsistencyReport_NodeConsistencyReport>>( node, typeof( ConsistencyReport.NodeConsistencyReport ), reporter, RecordType.Node, keys ) : MandatoryProperties.NoCheck();
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public System.Func<org.neo4j.kernel.impl.store.record.RelationshipRecord,Check<org.neo4j.kernel.impl.store.record.RelationshipRecord,org.neo4j.consistency.report.ConsistencyReport_RelationshipConsistencyReport>> forRelationships(final org.neo4j.consistency.report.ConsistencyReporter reporter)
		 public virtual System.Func<RelationshipRecord, Check<RelationshipRecord, Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipConsistencyReport>> ForRelationships( ConsistencyReporter reporter )
		 {
			  return relationship =>
			  {
				int[] propertyKeys = _relationships.get( relationship.Type );
				if ( propertyKeys != null )
				{
					 MutableIntSet keys = new IntHashSet( propertyKeys.Length );
					 foreach ( int key in propertyKeys )
					 {
						  keys.add( key );
					 }
					 return new RealCheck<Org.Neo4j.Kernel.impl.store.record.RelationshipRecord, Check<Org.Neo4j.Kernel.impl.store.record.RelationshipRecord, Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipConsistencyReport>>( relationship, typeof( ConsistencyReport.RelationshipConsistencyReport ), reporter, RecordType.Relationship, keys );
				}
				return NoCheck();
			  };
		 }

		 private IEnumerable<ConstraintRule> ConstraintsIgnoringMalformed( SchemaStorage schemaStorage )
		 {
			  return schemaStorage.constraintsGetAllIgnoreMalformed;
		 }

		 private static void RecordConstraint( int labelOrRelType, int propertyKey, MutableIntObjectMap<int[]> storage )
		 {
			  int[] propertyKeys = storage.get( labelOrRelType );
			  if ( propertyKeys == null )
			  {
					propertyKeys = new int[]{ propertyKey };
			  }
			  else
			  {
					propertyKeys = Arrays.copyOf( propertyKeys, propertyKeys.Length + 1 );
					propertyKeys[propertyKeys.Length - 1] = propertyKey;
			  }
			  storage.put( labelOrRelType, propertyKeys );
		 }

		 public interface Check<RECORD, REPORT> : AutoCloseable where RECORD : Org.Neo4j.Kernel.impl.store.record.PrimitiveRecord where REPORT : Org.Neo4j.Consistency.report.ConsistencyReport_PrimitiveConsistencyReport
		 {
			  void Receive( int[] keys );

			  void Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static <RECORD extends org.neo4j.kernel.impl.store.record.PrimitiveRecord, REPORT extends org.neo4j.consistency.report.ConsistencyReport_PrimitiveConsistencyReport> Check<RECORD,REPORT> noCheck()
		 private static Check<RECORD, REPORT> NoCheck<RECORD, REPORT>() where RECORD : Org.Neo4j.Kernel.impl.store.record.PrimitiveRecord where REPORT : Org.Neo4j.Consistency.report.ConsistencyReport_PrimitiveConsistencyReport
		 {
			  return NONE;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") private static final Check NONE = new Check()
		 private static readonly Check NONE = new CheckAnonymousInnerClass();

		 private class CheckAnonymousInnerClass : Check
		 {
			 public void receive( int[] keys )
			 {
			 }

			 public void close()
			 {
			 }

			 public override string ToString()
			 {
				  return "NONE";
			 }
		 }

		 private class RealCheck<RECORD, REPORT> : Check<RECORD, REPORT> where RECORD : Org.Neo4j.Kernel.impl.store.record.PrimitiveRecord where REPORT : Org.Neo4j.Consistency.report.ConsistencyReport_PrimitiveConsistencyReport
		 {
			  internal readonly RECORD Record;
			  internal readonly MutableIntSet MandatoryKeys;
			  internal readonly Type<REPORT> ReportClass;
			  internal readonly ConsistencyReporter Reporter;
			  internal readonly RecordType RecordType;

			  internal RealCheck( RECORD record, Type reportClass, ConsistencyReporter reporter, RecordType recordType, MutableIntSet mandatoryKeys )
			  {
					  reportClass = typeof( REPORT );
					this.Record = record;
					this.ReportClass = reportClass;
					this.Reporter = reporter;
					this.RecordType = recordType;
					this.MandatoryKeys = mandatoryKeys;
			  }

			  public override void Receive( int[] keys )
			  {
					foreach ( int key in keys )
					{
						 MandatoryKeys.remove( key );
					}
			  }

			  public override void Close()
			  {
					if ( !MandatoryKeys.Empty )
					{
						 for ( IntIterator key = MandatoryKeys.intIterator(); key.hasNext(); )
						 {
							  Reporter.report( Record, ReportClass, RecordType ).missingMandatoryProperty( key.next() );
						 }
					}
			  }

			  public override string ToString()
			  {
					return "Mandatory properties: " + MandatoryKeys;
			  }
		 }
	}

}
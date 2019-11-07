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
namespace Neo4Net.Consistency.checking.full
{
	using IntIterator = org.eclipse.collections.api.iterator.IntIterator;
	using MutableIntObjectMap = org.eclipse.collections.api.map.primitive.MutableIntObjectMap;
	using MutableIntSet = org.eclipse.collections.api.set.primitive.MutableIntSet;
	using IntObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.IntObjectHashMap;
	using IntHashSet = org.eclipse.collections.impl.set.mutable.primitive.IntHashSet;


	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using ConsistencyReporter = Neo4Net.Consistency.report.ConsistencyReporter;
	using LabelSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaDescriptor;
	using RelationTypeSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.RelationTypeSchemaDescriptor;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using SchemaProcessor = Neo4Net.Kernel.Api.Internal.Schema.SchemaProcessor;
	using SchemaStorage = Neo4Net.Kernel.impl.store.SchemaStorage;
	using StoreAccess = Neo4Net.Kernel.impl.store.StoreAccess;
	using ConstraintRule = Neo4Net.Kernel.Impl.Store.Records.ConstraintRule;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PrimitiveRecord = Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.Numbers.safeCastLongToInt;

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
//ORIGINAL LINE: public System.Func<Neo4Net.kernel.impl.store.record.NodeRecord,Check<Neo4Net.kernel.impl.store.record.NodeRecord,Neo4Net.consistency.report.ConsistencyReport_NodeConsistencyReport>> forNodes(final Neo4Net.consistency.report.ConsistencyReporter reporter)
		 public virtual System.Func<NodeRecord, Check<NodeRecord, Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport>> ForNodes( ConsistencyReporter reporter )
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
				return keys != null ? new RealCheck<Neo4Net.Kernel.Impl.Store.Records.NodeRecord, Check<Neo4Net.Kernel.Impl.Store.Records.NodeRecord, Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport>>( node, typeof( ConsistencyReport.NodeConsistencyReport ), reporter, RecordType.Node, keys ) : MandatoryProperties.NoCheck();
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public System.Func<Neo4Net.kernel.impl.store.record.RelationshipRecord,Check<Neo4Net.kernel.impl.store.record.RelationshipRecord,Neo4Net.consistency.report.ConsistencyReport_RelationshipConsistencyReport>> forRelationships(final Neo4Net.consistency.report.ConsistencyReporter reporter)
		 public virtual System.Func<RelationshipRecord, Check<RelationshipRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport>> ForRelationships( ConsistencyReporter reporter )
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
					 return new RealCheck<Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord, Check<Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport>>( relationship, typeof( ConsistencyReport.RelationshipConsistencyReport ), reporter, RecordType.Relationship, keys );
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

		 public interface ICheck<RECORD, REPORT> : IDisposable where RECORD : Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord where REPORT : Neo4Net.Consistency.report.ConsistencyReport_PrimitiveConsistencyReport
		 {
			  void Receive( int[] keys );

			  void Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static <RECORD extends Neo4Net.kernel.impl.store.record.PrimitiveRecord, REPORT extends Neo4Net.consistency.report.ConsistencyReport_PrimitiveConsistencyReport> Check<RECORD,REPORT> noCheck()
		 private static Check<RECORD, REPORT> NoCheck<RECORD, REPORT>() where RECORD : Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord where REPORT : Neo4Net.Consistency.report.ConsistencyReport_PrimitiveConsistencyReport
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

		 private class RealCheck<RECORD, REPORT> : Check<RECORD, REPORT> where RECORD : Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord where REPORT : Neo4Net.Consistency.report.ConsistencyReport_PrimitiveConsistencyReport
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
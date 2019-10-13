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
namespace Neo4Net.Consistency.report
{

	using Neo4Net.Consistency.checking;
	using Neo4Net.Consistency.checking;
	using Neo4Net.Consistency.checking;
	using Neo4Net.Consistency.store;
	using RecordAccess = Neo4Net.Consistency.store.RecordAccess;
	using Neo4Net.Consistency.store;
	using CountsEntry = Neo4Net.Consistency.store.synthetic.CountsEntry;
	using IndexEntry = Neo4Net.Consistency.store.synthetic.IndexEntry;
	using LabelScanDocument = Neo4Net.Consistency.store.synthetic.LabelScanDocument;
	using DocumentedUtils = Neo4Net.Kernel.Impl.Annotations.DocumentedUtils;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.impl.store.record.LabelTokenRecord;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.impl.store.record.PropertyRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.impl.store.record.RelationshipTypeTokenRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Exceptions.stringify;

	public class ConsistencyReporter : ConsistencyReport_Reporter
	{
		 private static readonly ProxyFactory<ConsistencyReport_SchemaConsistencyReport> _schemaReport = ProxyFactory.Create( typeof( ConsistencyReport_SchemaConsistencyReport ) );
		 private static readonly ProxyFactory<ConsistencyReport_NodeConsistencyReport> _nodeReport = ProxyFactory.Create( typeof( ConsistencyReport_NodeConsistencyReport ) );
		 private static readonly ProxyFactory<ConsistencyReport_RelationshipConsistencyReport> _relationshipReport = ProxyFactory.Create( typeof( ConsistencyReport_RelationshipConsistencyReport ) );
		 private static readonly ProxyFactory<ConsistencyReport_PropertyConsistencyReport> _propertyReport = ProxyFactory.Create( typeof( ConsistencyReport_PropertyConsistencyReport ) );
		 private static readonly ProxyFactory<ConsistencyReport_RelationshipTypeConsistencyReport> _relationshipTypeReport = ProxyFactory.Create( typeof( ConsistencyReport_RelationshipTypeConsistencyReport ) );
		 private static readonly ProxyFactory<ConsistencyReport_LabelTokenConsistencyReport> _labelKeyReport = ProxyFactory.Create( typeof( ConsistencyReport_LabelTokenConsistencyReport ) );
		 private static readonly ProxyFactory<ConsistencyReport_PropertyKeyTokenConsistencyReport> _propertyKeyReport = ProxyFactory.Create( typeof( ConsistencyReport_PropertyKeyTokenConsistencyReport ) );
		 private static readonly ProxyFactory<ConsistencyReport_DynamicConsistencyReport> _dynamicReport = ProxyFactory.Create( typeof( ConsistencyReport_DynamicConsistencyReport ) );
		 private static readonly ProxyFactory<ConsistencyReport_DynamicLabelConsistencyReport> _dynamicLabelReport = ProxyFactory.Create( typeof( ConsistencyReport_DynamicLabelConsistencyReport ) );
		 private static readonly ProxyFactory<ConsistencyReport_LabelScanConsistencyReport> _labelScanReport = ProxyFactory.Create( typeof( ConsistencyReport_LabelScanConsistencyReport ) );
		 private static readonly ProxyFactory<ConsistencyReport_IndexConsistencyReport> _index = ProxyFactory.Create( typeof( ConsistencyReport_IndexConsistencyReport ) );
		 private static readonly ProxyFactory<ConsistencyReport_RelationshipGroupConsistencyReport> _relationshipGroupReport = ProxyFactory.Create( typeof( ConsistencyReport_RelationshipGroupConsistencyReport ) );
		 private static readonly ProxyFactory<ConsistencyReport_CountsConsistencyReport> _countsReport = ProxyFactory.Create( typeof( ConsistencyReport_CountsConsistencyReport ) );

		 private readonly RecordAccess _records;
		 private readonly InconsistencyReport _report;
		 private readonly Monitor _monitor;

		 public interface Monitor
		 {
			  void Reported( Type report, string method, string message );
		 }

		 public static readonly Monitor NoMonitor = ( _report, method, message ) =>
		 {
		 };

		 public ConsistencyReporter( RecordAccess records, InconsistencyReport report ) : this( records, report, NoMonitor )
		 {
		 }

		 public ConsistencyReporter( RecordAccess records, InconsistencyReport report, Monitor monitor )
		 {
			  this._records = records;
			  this._report = report;
			  this._monitor = monitor;
		 }

		 private void Dispatch<RECORD, REPORT>( RecordType type, ProxyFactory<REPORT> factory, RECORD record, RecordCheck<RECORD, REPORT> checker ) where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord where REPORT : ConsistencyReport
		 {
			  ReportInvocationHandler<RECORD, REPORT> handler = new ReportHandler<RECORD, REPORT>( _report, factory, type, _records, record, _monitor );
			  try
			  {
					checker.Check( record, handler, _records );
			  }
			  catch ( Exception e )
			  {
					// This is a rare event and exposing the stack trace is a good idea, otherwise we
					// can only see that something went wrong, not at all what.
					handler.ReportConflict.error( type, record, "Failed to check record: " + stringify( e ), new object[0] );
			  }
			  handler.UpdateSummary();
		 }

		 internal static void DispatchReference( CheckerEngine engine, ComparativeRecordChecker checker, AbstractBaseRecord referenced, RecordAccess records )
		 {
			  ReportInvocationHandler handler = ( ReportInvocationHandler ) engine;
			  handler.checkReference( engine, checker, referenced, records );
			  handler.updateSummary();
		 }

		 internal static string PendingCheckToString( CheckerEngine engine, ComparativeRecordChecker checker )
		 {
			  ReportInvocationHandler handler = ( ReportInvocationHandler ) engine;
			  return handler.pendingCheckToString( checker );
		 }

		 internal static void DispatchChangeReference( CheckerEngine engine, ComparativeRecordChecker checker, AbstractBaseRecord oldReferenced, AbstractBaseRecord newReferenced, RecordAccess records )
		 {
			  ReportInvocationHandler handler = ( ReportInvocationHandler ) engine;
			  handler.checkDiffReference( engine, checker, oldReferenced, newReferenced, records );
			  handler.updateSummary();
		 }

		 internal static void DispatchSkip( CheckerEngine engine )
		 {
			  ( ( ReportInvocationHandler ) engine ).updateSummary();
		 }

		 public virtual REPORT Report<RECORD, REPORT>( RECORD record, Type cls, RecordType recordType ) where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord where REPORT : ConsistencyReport
		 {
				 cls = typeof( REPORT );
			  ProxyFactory<REPORT> proxyFactory = ProxyFactory.Get( cls );
			  ReportInvocationHandler<RECORD, REPORT> handler = new ReportHandlerAnonymousInnerClass( this, _report, proxyFactory, recordType, _records, record, _monitor );
			  return handler.Report();
		 }

		 private class ReportHandlerAnonymousInnerClass : ReportHandler<RECORD, REPORT>
		 {
			 private readonly ConsistencyReporter _outerInstance;

			 public ReportHandlerAnonymousInnerClass( ConsistencyReporter outerInstance, Neo4Net.Consistency.report.InconsistencyReport report, Neo4Net.Consistency.report.ConsistencyReporter.ProxyFactory<REPORT> proxyFactory, RecordType recordType, RecordAccess records, AbstractBaseRecord record, Neo4Net.Consistency.report.ConsistencyReporter.Monitor monitor ) : base( report, proxyFactory, recordType, records, record, monitor )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void inconsistencyReported()
			 {
				  updateSummary();
			 }
		 }

		 public virtual FormattingDocumentedHandler FormattingHandler( RecordType type )
		 {
			  return new FormattingDocumentedHandler( _report, type );
		 }

		 public class FormattingDocumentedHandler : InvocationHandler
		 {
			  internal readonly InconsistencyReport Report;
			  internal readonly RecordType Type;
			  internal int Errors;
			  internal int Warnings;

			  internal FormattingDocumentedHandler( InconsistencyReport report, RecordType type )
			  {
					this.Report = report;
					this.Type = type;
			  }

			  public override object Invoke( object proxy, System.Reflection.MethodInfo method, object[] args )
			  {
					string message = DocumentedUtils.extractFormattedMessage( method, args );
					if ( method.getAnnotation( typeof( ConsistencyReport_Warning ) ) == null )
					{
						 Errors++;
						 Report.error( message );
					}
					else
					{
						 Warnings++;
						 Report.warning( message );
					}
					return null;
			  }

			  public virtual void UpdateSummary()
			  {
					Report.updateSummary( Type, Errors, Warnings );
			  }
		 }

		 public abstract class ReportInvocationHandler <RECORD, REPORT> : CheckerEngine<RECORD, REPORT>, InvocationHandler where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord where REPORT : ConsistencyReport
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly InconsistencyReport ReportConflict;
			  internal readonly ProxyFactory<REPORT> Factory;
			  internal readonly RecordType Type;
			  internal short Errors;
			  internal short Warnings;
			  internal short References = 1;
			  internal readonly RecordAccess Records;
			  internal readonly Monitor Monitor;

			  internal ReportInvocationHandler( InconsistencyReport report, ProxyFactory<REPORT> factory, RecordType type, RecordAccess records, Monitor monitor )
			  {
					this.ReportConflict = report;
					this.Factory = factory;
					this.Type = type;
					this.Records = records;
					this.Monitor = monitor;
			  }

			  internal virtual void UpdateSummary()
			  {
				  lock ( this )
				  {
						if ( --References == 0 )
						{
							 ReportConflict.updateSummary( Type, Errors, Warnings );
						}
				  }
			  }

			  internal virtual string PendingCheckToString( ComparativeRecordChecker checker )
			  {
					string checkName;
					try
					{
						 if ( checker.GetType().GetMethod("toString").DeclaringClass == typeof(object) )
						 {
							  checkName = checker.GetType().Name;
							  if ( checkName.Length == 0 )
							  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
									checkName = checker.GetType().FullName;
							  }
						 }
						 else
						 {
							  checkName = checker.ToString();
						 }
					}
					catch ( NoSuchMethodException )
					{
						 checkName = checker.ToString();
					}
					return string.Format( "ReferenceCheck{{{0}[{1}]/{2}}}", Type, RecordId(), checkName );
			  }

			  internal abstract long RecordId();

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <REFERRED extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord> void comparativeCheck(org.neo4j.consistency.store.RecordReference<REFERRED> reference, org.neo4j.consistency.checking.ComparativeRecordChecker<RECORD, ? super REFERRED, REPORT> checker)
			  public override void ComparativeCheck<REFERRED, T1>( RecordReference<REFERRED> reference, ComparativeRecordChecker<T1> checker ) where REFERRED : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
			  {
					References++;
					reference.Dispatch( new PendingReferenceCheck<REFERRED>( this, checker ) );
			  }

			  public override REPORT Report()
			  {
					return Factory.create( this );
			  }

			  /// <summary>
			  /// Invoked when an inconsistency is encountered.
			  /// </summary>
			  /// <param name="args"> array of the items referenced from this record with which it is inconsistent. </param>
			  public override object Invoke( object proxy, System.Reflection.MethodInfo method, object[] args )
			  {
					string message = DocumentedUtils.extractMessage( method );
					if ( method.getAnnotation( typeof( ConsistencyReport_Warning ) ) == null )
					{
						 Errors++;
						 args = GetRealRecords( args );
						 LogError( message, args );
					}
					else
					{
						 Warnings++;
						 args = GetRealRecords( args );
						 LogWarning( message, args );
					}
					Monitor.reported( Factory.type(), method.Name, message );
					InconsistencyReported();
					return null;
			  }

			  protected internal virtual void InconsistencyReported()
			  {
			  }

			  internal virtual object[] GetRealRecords( object[] args )
			  {
					if ( args == null )
					{
						 return args;
					}
					for ( int i = 0; i < args.Length; i++ )
					{
						 // We use "created" flag here. Consistency checking code revolves around records and so
						 // even in scenarios where records are built from other sources, f.ex half-and-purpose-built from cache,
						 // this flag is used to signal that the real record needs to be read in order to be used as a general
						 // purpose record.
						 if ( args[i] is AbstractBaseRecord && ( ( AbstractBaseRecord ) args[i] ).Created )
						 { // get the real record
							  if ( args[i] is NodeRecord )
							  {
									args[i] = ( ( DirectRecordReference<NodeRecord> ) Records.node( ( ( NodeRecord ) args[i] ).Id ) ).record();
							  }
							  else if ( args[i] is RelationshipRecord )
							  {
									args[i] = ( ( DirectRecordReference<RelationshipRecord> ) Records.relationship( ( ( RelationshipRecord ) args[i] ).Id ) ).record();
							  }
						 }
					}
					return args;
			  }

			  protected internal abstract void LogError( string message, object[] args );

			  protected internal abstract void LogWarning( string message, object[] args );

			  internal abstract void CheckReference( CheckerEngine engine, ComparativeRecordChecker checker, AbstractBaseRecord referenced, RecordAccess records );

			  internal abstract void CheckDiffReference( CheckerEngine engine, ComparativeRecordChecker checker, AbstractBaseRecord oldReferenced, AbstractBaseRecord newReferenced, RecordAccess records );
		 }

		 public class ReportHandler <RECORD, REPORT> : ReportInvocationHandler<RECORD, REPORT> where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord where REPORT : ConsistencyReport
		 {
			  internal readonly AbstractBaseRecord Record;

			  public ReportHandler( InconsistencyReport report, ProxyFactory<REPORT> factory, RecordType type, RecordAccess records, AbstractBaseRecord record, Monitor monitor ) : base( report, factory, type, records, monitor )
			  {
					this.Record = record;
			  }

			  internal override long RecordId()
			  {
					return Record.Id;
			  }

			  protected internal override void LogError( string message, object[] args )
			  {
					_outerInstance.report.error( type, Record, message, args );
			  }

			  protected internal override void LogWarning( string message, object[] args )
			  {
					_outerInstance.report.warning( type, Record, message, args );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") void checkReference(org.neo4j.consistency.checking.CheckerEngine engine, org.neo4j.consistency.checking.ComparativeRecordChecker checker, org.neo4j.kernel.impl.store.record.AbstractBaseRecord referenced, org.neo4j.consistency.store.RecordAccess records)
			  internal override void CheckReference( CheckerEngine engine, ComparativeRecordChecker checker, AbstractBaseRecord referenced, RecordAccess records )
			  {
					checker.checkReference( Record, referenced, this, records );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") void checkDiffReference(org.neo4j.consistency.checking.CheckerEngine engine, org.neo4j.consistency.checking.ComparativeRecordChecker checker, org.neo4j.kernel.impl.store.record.AbstractBaseRecord oldReferenced, org.neo4j.kernel.impl.store.record.AbstractBaseRecord newReferenced, org.neo4j.consistency.store.RecordAccess records)
			  internal override void CheckDiffReference( CheckerEngine engine, ComparativeRecordChecker checker, AbstractBaseRecord oldReferenced, AbstractBaseRecord newReferenced, RecordAccess records )
			  {
					checker.checkReference( Record, newReferenced, this, records );
			  }
		 }

		 public override void ForSchema( DynamicRecord schema, RecordCheck<DynamicRecord, ConsistencyReport_SchemaConsistencyReport> checker )
		 {
			  Dispatch( RecordType.SCHEMA, _schemaReport, schema, checker );
		 }

		 public override void ForNode( NodeRecord node, RecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport> checker )
		 {
			  Dispatch( RecordType.NODE, _nodeReport, node, checker );
		 }

		 public override void ForRelationship( RelationshipRecord relationship, RecordCheck<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport> checker )
		 {
			  Dispatch( RecordType.RELATIONSHIP, _relationshipReport, relationship, checker );
		 }

		 public override void ForProperty( PropertyRecord property, RecordCheck<PropertyRecord, ConsistencyReport_PropertyConsistencyReport> checker )
		 {
			  Dispatch( RecordType.PROPERTY, _propertyReport, property, checker );
		 }

		 public override void ForRelationshipTypeName( RelationshipTypeTokenRecord relationshipTypeTokenRecord, RecordCheck<RelationshipTypeTokenRecord, ConsistencyReport_RelationshipTypeConsistencyReport> checker )
		 {
			  Dispatch( RecordType.RELATIONSHIP_TYPE, _relationshipTypeReport, relationshipTypeTokenRecord, checker );
		 }

		 public override void ForLabelName( LabelTokenRecord label, RecordCheck<LabelTokenRecord, ConsistencyReport_LabelTokenConsistencyReport> checker )
		 {
			  Dispatch( RecordType.LABEL, _labelKeyReport, label, checker );
		 }

		 public override void ForNodeLabelScan( LabelScanDocument document, RecordCheck<LabelScanDocument, ConsistencyReport_LabelScanConsistencyReport> checker )
		 {
			  Dispatch( RecordType.LABEL_SCAN_DOCUMENT, _labelScanReport, document, checker );
		 }

		 public override void ForIndexEntry( IndexEntry entry, RecordCheck<IndexEntry, ConsistencyReport_IndexConsistencyReport> checker )
		 {
			  Dispatch( RecordType.INDEX, _index, entry, checker );
		 }

		 public override void ForPropertyKey( PropertyKeyTokenRecord key, RecordCheck<PropertyKeyTokenRecord, ConsistencyReport_PropertyKeyTokenConsistencyReport> checker )
		 {
			  Dispatch( RecordType.PROPERTY_KEY, _propertyKeyReport, key, checker );
		 }

		 public override void ForDynamicBlock( RecordType type, DynamicRecord record, RecordCheck<DynamicRecord, ConsistencyReport_DynamicConsistencyReport> checker )
		 {
			  Dispatch( type, _dynamicReport, record, checker );
		 }

		 public override void ForDynamicLabelBlock( RecordType type, DynamicRecord record, RecordCheck<DynamicRecord, ConsistencyReport_DynamicLabelConsistencyReport> checker )
		 {
			  Dispatch( type, _dynamicLabelReport, record, checker );
		 }

		 public override void ForRelationshipGroup( RelationshipGroupRecord record, RecordCheck<RelationshipGroupRecord, ConsistencyReport_RelationshipGroupConsistencyReport> checker )
		 {
			  Dispatch( RecordType.RELATIONSHIP_GROUP, _relationshipGroupReport, record, checker );
		 }

		 public override void ForCounts( CountsEntry countsEntry, RecordCheck<CountsEntry, ConsistencyReport_CountsConsistencyReport> checker )
		 {
			  Dispatch( RecordType.COUNTS, _countsReport, countsEntry, checker );
		 }

		 public class ProxyFactory<T>
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static java.util.Map<Class,ProxyFactory<?>> instances = new java.util.HashMap<>();
			  internal static IDictionary<Type, ProxyFactory<object>> Instances = new Dictionary<Type, ProxyFactory<object>>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private Constructor<? extends T> constructor;
			  internal System.Reflection.ConstructorInfo<T> Constructor;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Type<T> TypeConflict;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") static <T> ProxyFactory<T> get(Class<T> cls)
			  internal static ProxyFactory<T> Get<T>( Type cls )
			  {
					  cls = typeof( T );
					return ( ProxyFactory<T> ) Instances[cls];
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") ProxyFactory(Class<T> type) throws LinkageError
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  internal ProxyFactory( Type type )
			  {
					  type = typeof( T );
					this.TypeConflict = type;
					try
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: this.constructor = (Constructor<? extends T>) Proxy.getProxyClass(ConsistencyReporter.class.getClassLoader(), type).getConstructor(InvocationHandler.class);
						 this.Constructor = ( System.Reflection.ConstructorInfo<T> ) Proxy.getProxyClass( typeof( ConsistencyReporter ).ClassLoader, type ).getConstructor( typeof( InvocationHandler ) );
						 Instances[type] = this;
					}
					catch ( NoSuchMethodException e )
					{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
						 throw new LinkageError( "Cannot access Proxy constructor for " + type.FullName, e );
					}
			  }

			  public override string ToString()
			  {
					return this.GetType().Name + asList(Constructor.DeclaringClass.Interfaces);
			  }

			  internal virtual Type Type()
			  {
					return TypeConflict;
			  }

			  public virtual T Create( InvocationHandler handler )
			  {
					try
					{
						 return Constructor.newInstance( handler );
					}
					catch ( Exception e )
					{
						 throw new LinkageError( "Failed to create proxy instance", e );
					}
			  }

			  public static ProxyFactory<T> Create<T>( Type type )
			  {
					  type = typeof( T );
					return new ProxyFactory<T>( type );
			  }
		 }
	}

}
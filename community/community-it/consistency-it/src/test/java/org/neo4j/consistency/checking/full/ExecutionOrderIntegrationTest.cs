﻿using System;
using System.Collections.Generic;
using System.Text;

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
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using InvocationOnMock = org.mockito.invocation.InvocationOnMock;
	using Answer = org.mockito.stubbing.Answer;


	using Org.Neo4j.Consistency.checking;
	using Org.Neo4j.Consistency.checking;
	using Org.Neo4j.Consistency.checking;
	using Org.Neo4j.Consistency.checking;
	using CacheAccess = Org.Neo4j.Consistency.checking.cache.CacheAccess;
	using DefaultCacheAccess = Org.Neo4j.Consistency.checking.cache.DefaultCacheAccess;
	using ConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport;
	using ConsistencyReport_LabelTokenConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_LabelTokenConsistencyReport;
	using ConsistencyReport_NeoStoreConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_NeoStoreConsistencyReport;
	using ConsistencyReport_NodeConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_NodeConsistencyReport;
	using ConsistencyReport_PropertyConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_PropertyConsistencyReport;
	using ConsistencyReport_PropertyKeyTokenConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport;
	using ConsistencyReport_RelationshipConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipConsistencyReport;
	using ConsistencyReport_RelationshipGroupConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport;
	using ConsistencyReport_RelationshipTypeConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipTypeConsistencyReport;
	using ConsistencySummaryStatistics = Org.Neo4j.Consistency.report.ConsistencySummaryStatistics;
	using InconsistencyLogger = Org.Neo4j.Consistency.report.InconsistencyLogger;
	using InconsistencyReport = Org.Neo4j.Consistency.report.InconsistencyReport;
	using Org.Neo4j.Consistency.report;
	using DefaultCounts = Org.Neo4j.Consistency.statistics.DefaultCounts;
	using Statistics = Org.Neo4j.Consistency.statistics.Statistics;
	using RecordAccess = Org.Neo4j.Consistency.store.RecordAccess;
	using Org.Neo4j.Consistency.store;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using ProgressMonitorFactory = Org.Neo4j.Helpers.progress.ProgressMonitorFactory;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using StoreAccess = Org.Neo4j.Kernel.impl.store.StoreAccess;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using NeoStoreRecord = Org.Neo4j.Kernel.impl.store.record.NeoStoreRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PrimitiveRecord = Org.Neo4j.Kernel.impl.store.record.PrimitiveRecord;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.withSettings;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.ConsistencyCheckService.defaultConsistencyCheckThreadsNumber;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.report.ConsistencyReporter.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.Property.property;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.Property.set;

	public class ExecutionOrderIntegrationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.consistency.checking.GraphStoreFixture fixture = new org.neo4j.consistency.checking.GraphStoreFixture(getRecordFormatName())
		 public GraphStoreFixture fixture = new GraphStoreFixtureAnonymousInnerClass( RecordFormatName );

		 private class GraphStoreFixtureAnonymousInnerClass : GraphStoreFixture
		 {
			 public GraphStoreFixtureAnonymousInnerClass( string getRecordFormatName ) : base( getRecordFormatName )
			 {
			 }

			 protected internal override void generateInitialData( GraphDatabaseService graphDb )
			 {
				  // TODO: create bigger sample graph here
				  using ( Org.Neo4j.Graphdb.Transaction tx = graphDb.BeginTx() )
				  {
						Node node1 = set( graphDb.CreateNode( label( "Foo" ) ) );
						Node node2 = set( graphDb.CreateNode( label( "Foo" ) ), property( "key", "value" ) );
						node1.CreateRelationshipTo( node2, RelationshipType.withName( "C" ) );
						tx.Success();
				  }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunChecksInSingleThreadedPass() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunChecksInSingleThreadedPass()
		 {
			  // given
			  StoreAccess store = fixture.directStoreAccess().nativeStores();
			  int threads = defaultConsistencyCheckThreadsNumber();
			  CacheAccess cacheAccess = new DefaultCacheAccess( new DefaultCounts( threads ), threads );
			  RecordAccess access = FullCheck.RecordAccess( store, cacheAccess );

			  FullCheck singlePass = new FullCheck( TuningConfiguration, ProgressMonitorFactory.NONE, Statistics.NONE, threads, true );

			  ConsistencySummaryStatistics singlePassSummary = new ConsistencySummaryStatistics();
			  InconsistencyLogger logger = mock( typeof( InconsistencyLogger ) );
			  InvocationLog singlePassChecks = new InvocationLog();

			  // when
			  singlePass.Execute( fixture.directStoreAccess(), new LogDecorator(singlePassChecks), access, new InconsistencyReport(logger, singlePassSummary), cacheAccess, NO_MONITOR );

			  // then
			  verifyZeroInteractions( logger );
			  assertEquals( "Expected no inconsistencies in single pass.", 0, singlePassSummary.TotalInconsistencyCount );
		 }

		 private Config TuningConfiguration
		 {
			 get
			 {
				  return Config.defaults( stringMap( GraphDatabaseSettings.pagecache_memory.name(), "8m", GraphDatabaseSettings.record_format.name(), RecordFormatName ) );
			 }
		 }

		 protected internal virtual string RecordFormatName
		 {
			 get
			 {
				  return StringUtils.EMPTY;
			 }
		 }

		 private class InvocationLog
		 {
			  internal readonly IDictionary<string, Exception> Data = new Dictionary<string, Exception>();
			  internal readonly IDictionary<string, int> Duplicates = new Dictionary<string, int>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("ThrowableResultOfMethodCallIgnored") void log(org.neo4j.consistency.report.PendingReferenceCheck<?> check, org.mockito.invocation.InvocationOnMock invocation)
			  internal virtual void Log<T1>( PendingReferenceCheck<T1> check, InvocationOnMock invocation )
			  {
					System.Reflection.MethodInfo method = invocation.Method;
					if ( typeof( object ) == method.DeclaringClass && "finalize".Equals( method.Name ) )
					{
						 /* skip invocations to finalize - they are not of interest to us,
						  * and GC is not predictable enough to reliably trace this. */
						 return;
					}
					StringBuilder entry = ( new StringBuilder( method.Name ) ).Append( '(' );
					entry.Append( check );
					foreach ( object arg in invocation.Arguments )
					{
						 if ( arg is AbstractBaseRecord )
						 {
							  AbstractBaseRecord record = ( AbstractBaseRecord ) arg;
							  entry.Append( ',' ).Append( record.GetType().Name ).Append('[').Append(record.Id).Append(']');
						 }
					}
					string message = entry.Append( ')' ).ToString();
					if ( null != Data[message] = new Exception( message ) )
					{
						 int? cur = Duplicates[message];
						 if ( cur == null )
						 {
							  cur = 1;
						 }
						 Duplicates[message] = cur + 1;
					}
			  }
		 }

		 private class LogDecorator : CheckDecorator
		 {
			  internal readonly InvocationLog Log;

			  internal LogDecorator( InvocationLog log )
			  {
					this.Log = log;
			  }

			  public override void Prepare()
			  {
			  }

			  internal virtual OwningRecordCheck<REC, REP> Logging<REC, REP>( RecordCheck<REC, REP> checker ) where REC : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord where REP : Org.Neo4j.Consistency.report.ConsistencyReport
			  {
					return new LoggingChecker<REC, REP>( checker, Log );
			  }

			  public override OwningRecordCheck<NeoStoreRecord, ConsistencyReport_NeoStoreConsistencyReport> DecorateNeoStoreChecker( OwningRecordCheck<NeoStoreRecord, ConsistencyReport_NeoStoreConsistencyReport> checker )
			  {
					return Logging( checker );
			  }

			  public override OwningRecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport> DecorateNodeChecker( OwningRecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport> checker )
			  {
					return Logging( checker );
			  }

			  public override OwningRecordCheck<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport> DecorateRelationshipChecker( OwningRecordCheck<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport> checker )
			  {
					return Logging( checker );
			  }

			  public override RecordCheck<PropertyRecord, ConsistencyReport_PropertyConsistencyReport> DecoratePropertyChecker( RecordCheck<PropertyRecord, ConsistencyReport_PropertyConsistencyReport> checker )
			  {
					return Logging( checker );
			  }

			  public override RecordCheck<PropertyKeyTokenRecord, ConsistencyReport_PropertyKeyTokenConsistencyReport> DecoratePropertyKeyTokenChecker( RecordCheck<PropertyKeyTokenRecord, ConsistencyReport_PropertyKeyTokenConsistencyReport> checker )
			  {
					return Logging( checker );
			  }

			  public override RecordCheck<RelationshipTypeTokenRecord, ConsistencyReport_RelationshipTypeConsistencyReport> DecorateRelationshipTypeTokenChecker( RecordCheck<RelationshipTypeTokenRecord, ConsistencyReport_RelationshipTypeConsistencyReport> checker )
			  {
					return Logging( checker );
			  }

			  public override RecordCheck<LabelTokenRecord, ConsistencyReport_LabelTokenConsistencyReport> DecorateLabelTokenChecker( RecordCheck<LabelTokenRecord, ConsistencyReport_LabelTokenConsistencyReport> checker )
			  {
					return Logging( checker );
			  }

			  public override RecordCheck<RelationshipGroupRecord, ConsistencyReport_RelationshipGroupConsistencyReport> DecorateRelationshipGroupChecker( RecordCheck<RelationshipGroupRecord, ConsistencyReport_RelationshipGroupConsistencyReport> checker )
			  {
					return Logging( checker );
			  }
		 }

		 private class LoggingChecker<REC, REP> : OwningRecordCheck<REC, REP> where REC : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord where REP : Org.Neo4j.Consistency.report.ConsistencyReport
		 {
			  internal readonly RecordCheck<REC, REP> Checker;
			  internal readonly InvocationLog Log;

			  internal LoggingChecker( RecordCheck<REC, REP> checker, InvocationLog log )
			  {
					this.Checker = checker;
					this.Log = log;
			  }

			  public override void Check( REC record, CheckerEngine<REC, REP> engine, RecordAccess records )
			  {
					Checker.check( record, engine, new ComparativeLogging( records, Log ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public org.neo4j.consistency.checking.ComparativeRecordChecker<REC,org.neo4j.kernel.impl.store.record.PrimitiveRecord,REP> ownerCheck()
			  public override ComparativeRecordChecker<REC, PrimitiveRecord, REP> OwnerCheck()
			  {
					if ( Checker is OwningRecordCheck )
					{
						 return ( ( OwningRecordCheck ) Checker ).ownerCheck();
					}

					throw new System.NotSupportedException();
			  }
		 }

		 private class LoggingReference<T> : RecordReference<T> where T : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  internal readonly RecordReference<T> Reference;
			  internal readonly InvocationLog Log;

			  internal LoggingReference( RecordReference<T> reference, InvocationLog log )
			  {
					this.Reference = reference;
					this.Log = log;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public void dispatch(org.neo4j.consistency.report.PendingReferenceCheck<T> reporter)
			  public override void Dispatch( PendingReferenceCheck<T> reporter )
			  {
					Reference.dispatch( mock( ( Type<PendingReferenceCheck<T>> ) reporter.GetType(), withSettings().spiedInstance(reporter).defaultAnswer(new ReporterSpy<>(Reference, reporter, Log)) ) );
			  }
		 }

		 private class ReporterSpy<T> : Answer<object> where T : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  internal readonly RecordReference<T> Reference;
			  internal readonly PendingReferenceCheck<T> Reporter;
			  internal readonly InvocationLog Log;

			  internal ReporterSpy( RecordReference<T> reference, PendingReferenceCheck<T> reporter, InvocationLog log )
			  {
					this.Reference = reference;
					this.Reporter = reporter;
					this.Log = log;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object answer(org.mockito.invocation.InvocationOnMock invocation) throws Throwable
			  public override object Answer( InvocationOnMock invocation )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: if (!(reference instanceof org.neo4j.consistency.store.RecordReference_SkippingReference<?>))
					if ( !( Reference is Org.Neo4j.Consistency.store.RecordReference_SkippingReference<object> ) )
					{
						 Log.log( Reporter, invocation );
					}
					return invocation.callRealMethod();
			  }
		 }

		 private class ComparativeLogging : RecordAccess
		 {
			  internal readonly RecordAccess Access;
			  internal readonly InvocationLog Log;

			  internal ComparativeLogging( RecordAccess access, InvocationLog log )
			  {
					this.Access = access;
					this.Log = log;
			  }

			  internal virtual LoggingReference<T> Logging<T>( RecordReference<T> actual ) where T : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
			  {
					return new LoggingReference<T>( actual, Log );
			  }

			  public override RecordReference<DynamicRecord> Schema( long id )
			  {
					return Logging( Access.schema( id ) );
			  }

			  public override RecordReference<NodeRecord> Node( long id )
			  {
					return Logging( Access.node( id ) );
			  }

			  public override RecordReference<RelationshipRecord> Relationship( long id )
			  {
					return Logging( Access.relationship( id ) );
			  }

			  public override RecordReference<RelationshipGroupRecord> RelationshipGroup( long id )
			  {
					return Logging( Access.relationshipGroup( id ) );
			  }

			  public override RecordReference<PropertyRecord> Property( long id )
			  {
					return Logging( Access.property( id ) );
			  }

			  public override RecordReference<RelationshipTypeTokenRecord> RelationshipType( int id )
			  {
					return Logging( Access.relationshipType( id ) );
			  }

			  public override RecordReference<PropertyKeyTokenRecord> PropertyKey( int id )
			  {
					return Logging( Access.propertyKey( id ) );
			  }

			  public override RecordReference<DynamicRecord> String( long id )
			  {
					return Logging( Access.@string( id ) );
			  }

			  public override RecordReference<DynamicRecord> Array( long id )
			  {
					return Logging( Access.array( id ) );
			  }

			  public override RecordReference<DynamicRecord> RelationshipTypeName( int id )
			  {
					return Logging( Access.relationshipTypeName( id ) );
			  }

			  public override RecordReference<DynamicRecord> NodeLabels( long id )
			  {
					return Logging( Access.nodeLabels( id ) );
			  }

			  public override RecordReference<LabelTokenRecord> Label( int id )
			  {
					return Logging( Access.label( id ) );
			  }

			  public override RecordReference<DynamicRecord> LabelName( int id )
			  {
					return Logging( Access.labelName( id ) );
			  }

			  public override RecordReference<DynamicRecord> PropertyKeyName( int id )
			  {
					return Logging( Access.propertyKeyName( id ) );
			  }

			  public override RecordReference<NeoStoreRecord> Graph()
			  {
					return Logging( Access.graph() );
			  }

			  public override bool ShouldCheck( long id, MultiPassStore store )
			  {
					return Access.shouldCheck( id, store );
			  }

			  public override CacheAccess CacheAccess()
			  {
					return Access.cacheAccess();
			  }

			  public override IEnumerator<PropertyRecord> RawPropertyChain( long firstId )
			  {
					return Access.rawPropertyChain( firstId );
			  }
		 }
	}

}
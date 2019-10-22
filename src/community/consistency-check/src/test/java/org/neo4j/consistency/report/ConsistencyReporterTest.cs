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
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using TestName = org.junit.rules.TestName;
	using TestRule = org.junit.rules.TestRule;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Suite = org.junit.runners.Suite;
	using Statement = org.junit.runners.model.Statement;
	using ArgumentCaptor = org.mockito.ArgumentCaptor;
	using ArgumentMatchers = org.mockito.ArgumentMatchers;
	using InvocationOnMock = org.mockito.invocation.InvocationOnMock;
	using Answer = org.mockito.stubbing.Answer;


	using Neo4Net.Consistency.checking;
	using Neo4Net.Consistency.checking;
	using Neo4Net.Consistency.checking;
	using RecordAccess = Neo4Net.Consistency.store.RecordAccess;
	using Neo4Net.Consistency.store;
	using CountsEntry = Neo4Net.Consistency.store.synthetic.CountsEntry;
	using IndexEntry = Neo4Net.Consistency.store.synthetic.IndexEntry;
	using LabelScanDocument = Neo4Net.Consistency.store.synthetic.LabelScanDocument;
	using IndexProviderDescriptor = Neo4Net.Internal.Kernel.Api.schema.IndexProviderDescriptor;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using NodeLabelRange = Neo4Net.Kernel.api.labelscan.NodeLabelRange;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using NeoStoreRecord = Neo4Net.Kernel.Impl.Store.Records.NeoStoreRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyBlock = Neo4Net.Kernel.Impl.Store.Records.PropertyBlock;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;
	using IndexDescriptorFactory = Neo4Net.Storageengine.Api.schema.IndexDescriptorFactory;
	using SchemaRule = Neo4Net.Storageengine.Api.schema.SchemaRule;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.hamcrest.MockitoHamcrest.argThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.consistency.report.ConsistencyReporter.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.schema.SchemaUtil.idTokenNameLookup;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.counts.keys.CountsKeyFactory.nodeKey;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Suite.class) @Suite.SuiteClasses({ConsistencyReporterTest.TestAllReportMessages.class, ConsistencyReporterTest.TestReportLifecycle.class}) public class ConsistencyReporterTest
	public class ConsistencyReporterTest
	{
		 public class TestReportLifecycle
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.TestName testName = new org.junit.rules.TestName();
			  public readonly TestName TestName = new TestName();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSummarizeStatisticsAfterCheck()
			  public virtual void ShouldSummarizeStatisticsAfterCheck()
			  {
					// given
					ConsistencySummaryStatistics summary = mock( typeof( ConsistencySummaryStatistics ) );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.Neo4Net.consistency.store.RecordAccess records = mock(org.Neo4Net.consistency.store.RecordAccess.class);
					RecordAccess records = mock( typeof( RecordAccess ) );
					ConsistencyReporter.ReportHandler handler = new ConsistencyReporter.ReportHandler( new InconsistencyReport( mock( typeof( InconsistencyLogger ) ), summary ), mock( typeof( ConsistencyReporter.ProxyFactory ) ), RecordType.PROPERTY, records, new PropertyRecord( 0 ), NO_MONITOR );

					// when
					handler.updateSummary();

					// then
					verify( summary ).update( RecordType.PROPERTY, 0, 0 );
					verifyNoMoreInteractions( summary );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @SuppressWarnings("unchecked") public void shouldOnlySummarizeStatisticsWhenAllReferencesAreChecked()
			  public virtual void ShouldOnlySummarizeStatisticsWhenAllReferencesAreChecked()
			  {
					// given
					ConsistencySummaryStatistics summary = mock( typeof( ConsistencySummaryStatistics ) );
					RecordAccess records = mock( typeof( RecordAccess ) );
					ConsistencyReporter.ReportHandler handler = new ConsistencyReporter.ReportHandler( new InconsistencyReport( mock( typeof( InconsistencyLogger ) ), summary ), mock( typeof( ConsistencyReporter.ProxyFactory ) ), RecordType.PROPERTY, records, new PropertyRecord( 0 ), NO_MONITOR );

					RecordReference<PropertyRecord> reference = mock( typeof( RecordReference ) );
					ComparativeRecordChecker<PropertyRecord, PropertyRecord, ConsistencyReport_PropertyConsistencyReport> checker = mock( typeof( ComparativeRecordChecker ) );

					handler.comparativeCheck( reference, checker );
					ArgumentCaptor<PendingReferenceCheck<PropertyRecord>> captor = ( ArgumentCaptor ) ArgumentCaptor.forClass( typeof( PendingReferenceCheck ) );
					verify( reference ).dispatch( captor.capture() );
					PendingReferenceCheck pendingRefCheck = captor.Value;

					// when
					handler.updateSummary();

					// then
					verifyZeroInteractions( summary );

					// when
					pendingRefCheck.skip();

					// then
					verify( summary ).update( RecordType.PROPERTY, 0, 0 );
					verifyNoMoreInteractions( summary );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeStackTraceInUnexpectedCheckException()
			  public virtual void ShouldIncludeStackTraceInUnexpectedCheckException()
			  {
					// GIVEN
					ConsistencySummaryStatistics summary = mock( typeof( ConsistencySummaryStatistics ) );
					RecordAccess records = mock( typeof( RecordAccess ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<String> loggedError = new java.util.concurrent.atomic.AtomicReference<>();
					AtomicReference<string> loggedError = new AtomicReference<string>();
					InconsistencyLogger logger = new InconsistencyLoggerAnonymousInnerClass( this, loggedError );
					InconsistencyReport inconsistencyReport = new InconsistencyReport( logger, summary );
					ConsistencyReporter reporter = new ConsistencyReporter( records, inconsistencyReport );
					NodeRecord node = new NodeRecord( 10 );
					RecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport> checker = mock( typeof( RecordCheck ) );
					Exception exception = new Exception( "My specific exception" );
					doThrow( exception ).when( checker ).check( any( typeof( NodeRecord ) ), any( typeof( CheckerEngine ) ), any( typeof( RecordAccess ) ) );

					// WHEN
					reporter.ForNode( node, checker );

					// THEN
					assertNotNull( loggedError.get() );
					string error = loggedError.get();
					assertThat( error, containsString( "at " ) );
					assertThat( error, containsString( TestName.MethodName ) );
			  }

			  private class InconsistencyLoggerAnonymousInnerClass : InconsistencyLogger
			  {
				  private readonly TestReportLifecycle _outerInstance;

				  private AtomicReference<string> _loggedError;

				  public InconsistencyLoggerAnonymousInnerClass( TestReportLifecycle outerInstance, AtomicReference<string> loggedError )
				  {
					  this.outerInstance = outerInstance;
					  this._loggedError = loggedError;
				  }

				  public void error( RecordType recordType, AbstractBaseRecord record, string message, object[] args )
				  {
						assertTrue( _loggedError.compareAndSet( null, message ) );
				  }

				  public void error( RecordType recordType, AbstractBaseRecord oldRecord, AbstractBaseRecord newRecord, string message, object[] args )
				  {
						assertTrue( _loggedError.compareAndSet( null, message ) );
				  }

				  public void error( string message )
				  {
						assertTrue( _loggedError.compareAndSet( null, message ) );
				  }

				  public void warning( RecordType recordType, AbstractBaseRecord record, string message, object[] args )
				  {
				  }

				  public void warning( RecordType recordType, AbstractBaseRecord oldRecord, AbstractBaseRecord newRecord, string message, object[] args )
				  {
				  }

				  public void warning( string message )
				  {
				  }
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public static class TestAllReportMessages implements org.mockito.stubbing.Answer
		 public class TestAllReportMessages : Answer
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @SuppressWarnings("unchecked") public void shouldLogInconsistency() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldLogInconsistency()
			  {
					// given
					InconsistencyReport report = mock( typeof( InconsistencyReport ) );
					ConsistencyReport_Reporter reporter = new ConsistencyReporter( mock( typeof( RecordAccess ) ), report );

					// when
					ReportMethod.invoke( reporter, Parameters( ReportMethod ) );

					// then
					if ( Method.getAnnotation( typeof( ConsistencyReport_Warning ) ) == null )
					{
						 if ( ReportMethod.Name.EndsWith( "Change" ) )
						 {
							  verify( report ).error( any( typeof( RecordType ) ), any( typeof( AbstractBaseRecord ) ), any( typeof( AbstractBaseRecord ) ), argThat( HasExpectedFormat() ), any(typeof(object[])) );
						 }
						 else
						 {
							  verify( report ).error( any( typeof( RecordType ) ), any( typeof( AbstractBaseRecord ) ), argThat( HasExpectedFormat() ), NullSafeAny() );
						 }
					}
					else
					{
						 if ( ReportMethod.Name.EndsWith( "Change" ) )
						 {
							  verify( report ).warning( any( typeof( RecordType ) ), any( typeof( AbstractBaseRecord ) ), any( typeof( AbstractBaseRecord ) ), argThat( HasExpectedFormat() ), any(typeof(object[])) );
						 }
						 else
						 {
							  verify( report ).warning( any( typeof( RecordType ) ), any( typeof( AbstractBaseRecord ) ), argThat( HasExpectedFormat() ), NullSafeAny() );
						 }
					}
			  }

			  internal readonly System.Reflection.MethodInfo ReportMethod;
			  internal readonly System.Reflection.MethodInfo Method;

			  public TestAllReportMessages( System.Reflection.MethodInfo reportMethod, System.Reflection.MethodInfo method )
			  {
					this.ReportMethod = reportMethod;
					this.Method = method;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{1}") public static java.util.List<Object[]> methods()
			  public static IList<object[]> Methods()
			  {
					List<object[]> methods = new List<object[]>();
					foreach ( System.Reflection.MethodInfo reporterMethod in typeof( ConsistencyReport_Reporter ).GetMethods() )
					{
						 Type[] parameterTypes = reporterMethod.GenericParameterTypes;
						 ParameterizedType checkerParameter = ( ParameterizedType ) parameterTypes[parameterTypes.Length - 1];
						 Type reportType = ( Type ) checkerParameter.ActualTypeArguments[1];
						 foreach ( System.Reflection.MethodInfo method in reportType.GetMethods() )
						 {
							  methods.Add( new object[]{ reporterMethod, method } );
						 }
					}
					return methods;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.TestRule logFailure = (super, description) -> new org.junit.runners.model.Statement()
			  public readonly TestRule logFailure = ( @base, description ) => new StatementAnonymousInnerClass();

			  private class StatementAnonymousInnerClass : Statement
			  {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void evaluate() throws Throwable
				  public override void evaluate()
				  {
						try
						{
							 @base.evaluate();
						}
						catch ( Exception failure )
						{
							 Console.Error.WriteLine( "Failure in " + outerInstance + ": " + failure );
							 throw failure;
						}
				  }
			  }

			  public override string ToString()
			  {
					return format( "report.%s( %s{ reporter.%s(); } )", ReportMethod.Name, SignatureOf( ReportMethod ), Method.Name );
			  }

			  internal static string SignatureOf( System.Reflection.MethodInfo reportMethod )
			  {
					if ( reportMethod.ParameterTypes.length == 2 )
					{
						 return "record, RecordCheck( reporter )";
					}
					else
					{
						 return "oldRecord, newRecord, RecordCheck( reporter )";
					}
			  }

			  internal virtual object[] Parameters( System.Reflection.MethodInfo method )
			  {
					Type[] parameterTypes = method.ParameterTypes;
					object[] parameters = new object[parameterTypes.Length];
					for ( int i = 0; i < parameters.Length; i++ )
					{
						 parameters[i] = Parameter( parameterTypes[i] );
					}
					return parameters;
			  }

			  internal virtual object Parameter( Type type )
			  {
					if ( type == typeof( RecordType ) )
					{
						 return RecordType.STRING_PROPERTY;
					}
					if ( type == typeof( RecordCheck ) )
					{
						 return MockChecker();
					}
					if ( type == typeof( NodeRecord ) )
					{
						 return new NodeRecord( 0, false, 1, 2 );
					}
					if ( type == typeof( RelationshipRecord ) )
					{
						 return new RelationshipRecord( 0, 1, 2, 3 );
					}
					if ( type == typeof( PropertyRecord ) )
					{
						 return new PropertyRecord( 0 );
					}
					if ( type == typeof( PropertyKeyTokenRecord ) )
					{
						 return new PropertyKeyTokenRecord( 0 );
					}
					if ( type == typeof( PropertyBlock ) )
					{
						 return new PropertyBlock();
					}
					if ( type == typeof( RelationshipTypeTokenRecord ) )
					{
						 return new RelationshipTypeTokenRecord( 0 );
					}
					if ( type == typeof( LabelTokenRecord ) )
					{
						 return new LabelTokenRecord( 0 );
					}
					if ( type == typeof( DynamicRecord ) )
					{
						 return new DynamicRecord( 0 );
					}
					if ( type == typeof( NeoStoreRecord ) )
					{
						 return new NeoStoreRecord();
					}
					if ( type == typeof( LabelScanDocument ) )
					{
						 return new LabelScanDocument( new NodeLabelRange( 0, new long[][] {} ) );
					}
					if ( type == typeof( IndexEntry ) )
					{
						 return new IndexEntry( IndexDescriptorFactory.forSchema( SchemaDescriptorFactory.forLabel( 1, 1 ) ).withId( 1L ), idTokenNameLookup, 0 );
					}
					if ( type == typeof( CountsEntry ) )
					{
						 return new CountsEntry( nodeKey( 7 ), 42 );
					}
					if ( type == typeof( Neo4Net.Storageengine.Api.schema.SchemaRule_Kind ) )
					{
						 return Neo4Net.Storageengine.Api.schema.SchemaRule_Kind.IndexRule;
					}
					if ( type == typeof( StoreIndexDescriptor ) )
					{
						 return IndexDescriptorFactory.forSchema( forLabel( 2, 3 ), IndexProviderDescriptor.UNDECIDED ).withId( 1 );
					}
					if ( type == typeof( SchemaRule ) )
					{
						 return SimpleSchemaRule();
					}
					if ( type == typeof( RelationshipGroupRecord ) )
					{
						 return new RelationshipGroupRecord( 0, 1 );
					}
					if ( type == typeof( long ) )
					{
						 return 12L;
					}
					if ( type == typeof( object ) )
					{
						 return "object";
					}
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					throw new System.ArgumentException( format( "Don't know how to provide parameter of type %s", type.FullName ) );
			  }

			  internal static SchemaRule SimpleSchemaRule()
			  {
					return new SchemaRuleAnonymousInnerClass();
			  }

			  private class SchemaRuleAnonymousInnerClass : SchemaRule
			  {
				  public long Id
				  {
					  get
					  {
							return 0;
					  }
				  }

				  public string Name
				  {
					  get
					  {
							return null;
					  }
				  }

				  public SchemaDescriptor schema()
				  {
						return null;
				  }
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private org.Neo4Net.consistency.checking.RecordCheck mockChecker()
			  internal virtual RecordCheck MockChecker()
			  {
					RecordCheck checker = mock( typeof( RecordCheck ) );
					doAnswer( this ).when( checker ).check( any( typeof( AbstractBaseRecord ) ), any( typeof( CheckerEngine ) ), any( typeof( RecordAccess ) ) );
					return checker;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object answer(org.mockito.invocation.InvocationOnMock invocation) throws Throwable
			  public override object Answer( InvocationOnMock invocation )
			  {
					object[] arguments = invocation.Arguments;
					ConsistencyReport report = ( ( CheckerEngine )arguments[arguments.Length - 2] ).report();
					try
					{
						 return Method.invoke( report, Parameters( Method ) );
					}
					catch ( System.ArgumentException ex )
					{
						 throw new System.ArgumentException( format( "%s.%s#%s(...)", report, Method.DeclaringClass.SimpleName, Method.Name ), ex );
					}
			  }
		 }

		 private static T[] NullSafeAny<T>()
		 {
			  return ArgumentMatchers.argThat( argument => true );
		 }

		 private static Matcher<string> HasExpectedFormat()
		 {
			  return new TypeSafeMatcherAnonymousInnerClass();
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<string>
		 {
			 public override bool matchesSafely( string item )
			 {
				  return item.Trim().Split(" ", true).length > 1;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "message of valid format" );
			 }
		 }
	}

}
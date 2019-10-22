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
namespace Neo4Net.Helpers.progress
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using TestName = org.junit.rules.TestName;
	using TestRule = org.junit.rules.TestRule;
	using Description = org.junit.runner.Description;
	using Statement = org.junit.runners.model.Statement;
	using InOrder = org.mockito.InOrder;
	using Mockito = org.mockito.Mockito;


	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.lineSeparator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doNothing;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ProgressMonitorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expected = org.junit.rules.ExpectedException.none();
		 public ExpectedException Expected = ExpectedException.none();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.TestName testName = new org.junit.rules.TestName();
		 public readonly TestName TestName = new TestName();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.SuppressOutput suppressOutput = org.Neo4Net.test.rule.SuppressOutput.suppressAll();
		 public SuppressOutput SuppressOutput = SuppressOutput.suppressAll();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final SingleIndicator factory = new SingleIndicator();
		 public readonly SingleIndicator Factory = new SingleIndicator();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportProgressInTheSpecifiedIntervals()
		 public virtual void ShouldReportProgressInTheSpecifiedIntervals()
		 {
			  // given
			  Indicator indicator = IndicatorMock();
			  ProgressListener progressListener = Factory.mock( indicator, 10 ).singlePart( TestName.MethodName, 16 );

			  // when
			  progressListener.Started();
			  for ( int i = 0; i < 16; i++ )
			  {
					progressListener.Add( 1 );
			  }
			  progressListener.Done();

			  // then
			  InOrder order = inOrder( indicator );
			  order.verify( indicator ).startProcess( 16 );
			  for ( int i = 0; i < 10; i++ )
			  {
					order.verify( indicator ).progress( i, i + 1 );
			  }
			  order.verify( indicator ).completeProcess();
			  order.verifyNoMoreInteractions();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAggregateProgressFromMultipleProcesses()
		 public virtual void ShouldAggregateProgressFromMultipleProcesses()
		 {
			  // given
			  Indicator indicator = IndicatorMock();
			  ProgressMonitorFactory.MultiPartBuilder builder = Factory.mock( indicator, 10 ).multipleParts( TestName.MethodName );
			  ProgressListener first = builder.ProgressForPart( "first", 5 );
			  ProgressListener other = builder.ProgressForPart( "other", 5 );
			  builder.Build();
			  InOrder order = inOrder( indicator );
			  order.verify( indicator ).startProcess( 10 );
			  order.verifyNoMoreInteractions();

			  // when
			  first.Started();
			  for ( int i = 0; i < 5; i++ )
			  {
					first.Add( 1 );
			  }
			  first.Done();

			  // then
			  order.verify( indicator ).startPart( "first", 5 );
			  for ( int i = 0; i < 5; i++ )
			  {
					order.verify( indicator ).progress( i, i + 1 );
			  }
			  order.verify( indicator ).completePart( "first" );
			  order.verifyNoMoreInteractions();

			  // when
			  other.Started();
			  for ( int i = 0; i < 5; i++ )
			  {
					other.Add( 1 );
			  }
			  other.Done();

			  // then
			  order.verify( indicator ).startPart( "other", 5 );
			  for ( int i = 5; i < 10; i++ )
			  {
					order.verify( indicator ).progress( i, i + 1 );
			  }
			  order.verify( indicator ).completePart( "other" );
			  order.verify( indicator ).completeProcess();
			  order.verifyNoMoreInteractions();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowAddingPartsAfterCompletingMultiPartBuilder()
		 public virtual void ShouldNotAllowAddingPartsAfterCompletingMultiPartBuilder()
		 {
			  ProgressMonitorFactory.MultiPartBuilder builder = Factory.mock( IndicatorMock(), 10 ).multipleParts(TestName.MethodName);
			  builder.ProgressForPart( "first", 10 );
			  builder.Build();

			  Expected.expect( typeof( System.InvalidOperationException ) );
			  Expected.expectMessage( "Builder has been completed." );
			  builder.ProgressForPart( "other", 10 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowAddingMultiplePartsWithSameIdentifier()
		 public virtual void ShouldNotAllowAddingMultiplePartsWithSameIdentifier()
		 {
			  ProgressMonitorFactory.MultiPartBuilder builder = Mockito.mock( typeof( ProgressMonitorFactory ) ).multipleParts( TestName.MethodName );
			  builder.ProgressForPart( "first", 10 );

			  Expected.expect( typeof( System.ArgumentException ) );
			  Expected.expectMessage( "Part 'first' has already been defined." );
			  builder.ProgressForPart( "first", 10 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartProcessAutomaticallyIfNotDoneBefore()
		 public virtual void ShouldStartProcessAutomaticallyIfNotDoneBefore()
		 {
			  // given
			  Indicator indicator = IndicatorMock();
			  ProgressListener progressListener = Factory.mock( indicator, 10 ).singlePart( TestName.MethodName, 16 );

			  // when
			  for ( int i = 0; i < 16; i++ )
			  {
					progressListener.Add( 1 );
			  }
			  progressListener.Done();

			  // then
			  InOrder order = inOrder( indicator );
			  order.verify( indicator, times( 1 ) ).startProcess( 16 );
			  for ( int i = 0; i < 10; i++ )
			  {
					order.verify( indicator ).progress( i, i + 1 );
			  }
			  order.verify( indicator ).completeProcess();
			  order.verifyNoMoreInteractions();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartMultiPartProcessAutomaticallyIfNotDoneBefore()
		 public virtual void ShouldStartMultiPartProcessAutomaticallyIfNotDoneBefore()
		 {
			  // given
			  Indicator indicator = IndicatorMock();
			  ProgressMonitorFactory.MultiPartBuilder builder = Factory.mock( indicator, 10 ).multipleParts( TestName.MethodName );
			  ProgressListener first = builder.ProgressForPart( "first", 5 );
			  ProgressListener other = builder.ProgressForPart( "other", 5 );
			  builder.Build();
			  InOrder order = inOrder( indicator );
			  order.verify( indicator ).startProcess( 10 );
			  order.verifyNoMoreInteractions();

			  // when
			  for ( int i = 0; i < 5; i++ )
			  {
					first.Add( 1 );
			  }
			  first.Done();

			  // then
			  order.verify( indicator ).startPart( "first", 5 );
			  for ( int i = 0; i < 5; i++ )
			  {
					order.verify( indicator ).progress( i, i + 1 );
			  }
			  order.verify( indicator ).completePart( "first" );
			  order.verifyNoMoreInteractions();

			  // when
			  for ( int i = 0; i < 5; i++ )
			  {
					other.Add( 1 );
			  }
			  other.Done();

			  // then
			  order.verify( indicator ).startPart( "other", 5 );
			  for ( int i = 5; i < 10; i++ )
			  {
					order.verify( indicator ).progress( i, i + 1 );
			  }
			  order.verify( indicator ).completePart( "other" );
			  order.verify( indicator ).completeProcess();
			  order.verifyNoMoreInteractions();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCompleteMultiPartProgressWithNoPartsImmediately()
		 public virtual void ShouldCompleteMultiPartProgressWithNoPartsImmediately()
		 {
			  // given
			  Indicator indicator = IndicatorMock();
			  ProgressMonitorFactory.MultiPartBuilder builder = Factory.mock( indicator, 10 ).multipleParts( TestName.MethodName );

			  // when
			  builder.Build();

			  // then
			  InOrder order = inOrder( indicator );
			  order.verify( indicator ).startProcess( 0 );
			  order.verify( indicator ).progress( 0, 10 );
			  order.verify( indicator ).completeProcess();
			  order.verifyNoMoreInteractions();
		 }

		 private static Indicator IndicatorMock()
		 {
			  Indicator indicator = mock( typeof( Indicator ), Mockito.CALLS_REAL_METHODS );
			  doNothing().when(indicator).progress(anyInt(), anyInt());
			  return indicator;
		 }

		 private static readonly string _expectedTextualOutput;

		 static ProgressMonitorTest()
		 {
			  StringWriter expectedTextualOutput = new StringWriter();
			  for ( int i = 0; i < 10; )
			  {
					for ( int j = 0; j < 20; j++ )
					{
						 expectedTextualOutput.write( '.' );
					}
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: expectedTextualOutput.write(String.format(" %3d%%%n", (++i) * 10));
					expectedTextualOutput.write( string.Format( " %3d%%%n", ( ++i ) * 10 ) );
			  }
			  _expectedTextualOutput = expectedTextualOutput.ToString();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrintADotEveryHalfPercentAndFullPercentageEveryTenPercentWithTextualIndicator() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPrintADotEveryHalfPercentAndFullPercentageEveryTenPercentWithTextualIndicator()
		 {
			  // given
			  MemoryStream stream = new MemoryStream();
			  ProgressListener progressListener = ProgressMonitorFactory.Textual( stream ).singlePart( TestName.MethodName, 1000 );

			  // when
			  for ( int i = 0; i < 1000; i++ )
			  {
					progressListener.Add( 1 );
			  }

			  // then
			  assertEquals( TestName.MethodName + lineSeparator() + _expectedTextualOutput, stream.ToString(Charset.defaultCharset().name()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrintADotEveryHalfPercentAndFullPercentageEveryTenPercentEvenWhenStepResolutionIsLower()
		 public virtual void ShouldPrintADotEveryHalfPercentAndFullPercentageEveryTenPercentEvenWhenStepResolutionIsLower()
		 {
			  // given
			  StringWriter writer = new StringWriter();
			  ProgressListener progressListener = ProgressMonitorFactory.Textual( writer ).singlePart( TestName.MethodName, 50 );

			  // when
			  for ( int i = 0; i < 50; i++ )
			  {
					progressListener.Add( 1 );
			  }

			  // then
			  assertEquals( TestName.MethodName + lineSeparator() + _expectedTextualOutput, writer.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowStartingAPartBeforeCompletionOfMultiPartBuilder()
		 public virtual void ShouldAllowStartingAPartBeforeCompletionOfMultiPartBuilder()
		 {
			  // given
			  Indicator indicator = mock( typeof( Indicator ) );
			  ProgressMonitorFactory.MultiPartBuilder builder = Factory.mock( indicator, 10 ).multipleParts( TestName.MethodName );
			  ProgressListener part1 = builder.ProgressForPart( "part1", 1 );
			  ProgressListener part2 = builder.ProgressForPart( "part2", 1 );

			  // when
			  part1.Add( 1 );
			  builder.Build();
			  part2.Add( 1 );
			  part1.Done();
			  part2.Done();

			  // then
			  InOrder order = inOrder( indicator );
			  order.verify( indicator ).startPart( "part1", 1 );
			  order.verify( indicator ).startProcess( 2 );
			  order.verify( indicator ).startPart( "part2", 1 );
			  order.verify( indicator ).completePart( "part1" );
			  order.verify( indicator ).completePart( "part2" );
			  order.verify( indicator ).completeProcess();
		 }

		 private class SingleIndicator : TestRule
		 {
			  internal virtual ProgressMonitorFactory Mock( Indicator indicatorMock, int indicatorSteps )
			  {
					when( indicatorMock.ReportResolution() ).thenReturn(indicatorSteps);
					ProgressMonitorFactory factory = Mockito.mock( typeof( ProgressMonitorFactory ) );
					when( factory.NewIndicator( any( typeof( string ) ) ) ).thenReturn( indicatorMock );
					FactoryMocks[factory] = false;
					return factory;
			  }

			  internal readonly IDictionary<ProgressMonitorFactory, bool> FactoryMocks = new Dictionary<ProgressMonitorFactory, bool>();

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.junit.runners.model.Statement apply(final org.junit.runners.model.Statement super, org.junit.runner.Description description)
			  public override Statement Apply( Statement @base, Description description )
			  {
					return new StatementAnonymousInnerClass( this, @base );
			  }

			  private class StatementAnonymousInnerClass : Statement
			  {
				  private readonly SingleIndicator _outerInstance;

				  private Statement @base;

				  public StatementAnonymousInnerClass( SingleIndicator outerInstance, Statement @base )
				  {
					  this.outerInstance = outerInstance;
					  this.@base = @base;
				  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void evaluate() throws Throwable
				  public override void evaluate()
				  {
						@base.evaluate();
						foreach ( KeyValuePair<ProgressMonitorFactory, bool> factoryMock in _outerInstance.factoryMocks.SetOfKeyValuePairs() )
						{
							 verify( factoryMock.Key, times( 1 ) ).newIndicator( any( typeof( string ) ) );
						}
				  }
			  }
		 }
	}

}
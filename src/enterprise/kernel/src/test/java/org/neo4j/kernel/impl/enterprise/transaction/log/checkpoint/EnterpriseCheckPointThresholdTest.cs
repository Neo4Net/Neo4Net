/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.impl.enterprise.transaction.log.checkpoint
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using CheckPointThreshold = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointThreshold;
	using CheckPointThresholdTestSupport = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointThresholdTestSupport;
	using LogPruning = Neo4Net.Kernel.impl.transaction.log.pruning.LogPruning;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class EnterpriseCheckPointThresholdTest : CheckPointThresholdTestSupport
	{
		 private bool _haveLogsToPrune;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before @Override public void setUp()
		 public override void SetUp()
		 {
			  base.SetUp();
			  LogPruning = new LogPruningAnonymousInnerClass( this );
		 }

		 private class LogPruningAnonymousInnerClass : LogPruning
		 {
			 private readonly EnterpriseCheckPointThresholdTest _outerInstance;

			 public LogPruningAnonymousInnerClass( EnterpriseCheckPointThresholdTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public void pruneLogs( long currentVersion )
			 {
				  fail( "Check point threshold must never call out to prune logs directly." );
			 }

			 public bool mightHaveLogsToPrune()
			 {
				  return _outerInstance.haveLogsToPrune;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void checkPointIsNeededIfWeMightHaveLogsToPrune()
		 public virtual void CheckPointIsNeededIfWeMightHaveLogsToPrune()
		 {
			  WithPolicy( "volumetric" );
			  _haveLogsToPrune = true;
			  CheckPointThreshold threshold = CreateThreshold();
			  threshold.Initialize( 2 );
			  assertTrue( threshold.IsCheckPointingNeeded( 2, Triggered ) );
			  VerifyTriggered( "log pruning" );
			  VerifyNoMoreTriggers();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void checkPointIsInitiallyNotNeededIfWeHaveNoLogsToPrune()
		 public virtual void CheckPointIsInitiallyNotNeededIfWeHaveNoLogsToPrune()
		 {
			  WithPolicy( "volumetric" );
			  _haveLogsToPrune = false;
			  CheckPointThreshold threshold = CreateThreshold();
			  threshold.Initialize( 2 );
			  assertFalse( threshold.IsCheckPointingNeeded( 2, NotTriggered ) );
			  VerifyNoMoreTriggers();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("ConstantConditions") @Test public void continuousPolicyMustTriggerCheckPointsAfterAnyWriteTransaction()
		 public virtual void ContinuousPolicyMustTriggerCheckPointsAfterAnyWriteTransaction()
		 {
			  WithPolicy( "continuous" );
			  CheckPointThreshold threshold = CreateThreshold();
			  threshold.Initialize( 2 );

			  assertThat( threshold.CheckFrequencyMillis(), lessThan(Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointThreshold_Fields.DefaultCheckingFrequencyMillis) );

			  assertFalse( threshold.IsCheckPointingNeeded( 2, Triggered ) );
			  threshold.CheckPointHappened( 3 );
			  assertFalse( threshold.IsCheckPointingNeeded( 3, Triggered ) );
			  assertTrue( threshold.IsCheckPointingNeeded( 4, Triggered ) );
			  VerifyTriggered( "continuous" );
			  VerifyNoMoreTriggers();
		 }
	}

}
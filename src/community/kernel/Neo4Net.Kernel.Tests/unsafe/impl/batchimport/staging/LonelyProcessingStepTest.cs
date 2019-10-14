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
namespace Neo4Net.@unsafe.Impl.Batchimport.staging
{
	using ClassRule = org.junit.ClassRule;
	using Test = org.junit.Test;


	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class LonelyProcessingStepTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static org.neo4j.test.rule.SuppressOutput mute = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public static SuppressOutput Mute = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 10_000) public void issuePanicBeforeCompletionOnError() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IssuePanicBeforeCompletionOnError()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<Step<?>> stepsPipeline = new java.util.ArrayList<>();
			  IList<Step<object>> stepsPipeline = new List<Step<object>>();
			  FaultyLonelyProcessingStepTest faultyStep = new FaultyLonelyProcessingStepTest( this, stepsPipeline );
			  stepsPipeline.Add( faultyStep );

			  faultyStep.Receive( 1, null );
			  faultyStep.AwaitCompleted();

			  assertTrue( faultyStep.EndOfUpstreamCalled );
			  assertTrue( "On upstream end step should be already on panic in case of exception", faultyStep.PanicOnEndUpstream );
			  assertTrue( faultyStep.Panic );
			  assertFalse( faultyStep.StillWorking() );
			  assertTrue( faultyStep.Completed );
		 }

		 private class FaultyLonelyProcessingStepTest : LonelyProcessingStep
		 {
			 private readonly LonelyProcessingStepTest _outerInstance;

			  internal volatile bool EndOfUpstreamCalled;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal volatile bool PanicOnEndUpstreamConflict;

			  internal FaultyLonelyProcessingStepTest<T1>( LonelyProcessingStepTest outerInstance, IList<T1> pipeLine ) : base( new StageExecution( "Faulty", null, Configuration.DEFAULT, pipeLine, 0 ), "Faulty", Configuration.DEFAULT )
			  {
				  this._outerInstance = outerInstance;
			  }

			  protected internal override void Process()
			  {
					throw new Exception( "Process exception" );
			  }

			  public override void EndOfUpstream()
			  {
					EndOfUpstreamCalled = true;
					PanicOnEndUpstreamConflict = Panic;
					base.EndOfUpstream();
			  }

			  internal virtual bool PanicOnEndUpstream
			  {
				  get
				  {
						return PanicOnEndUpstreamConflict;
				  }
			  }
		 }
	}

}
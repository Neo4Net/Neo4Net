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
namespace Neo4Net.@unsafe.Impl.Batchimport.staging
{
	using Test = org.junit.Test;


	using Neo4Net.Helpers.Collection;
	using Keys = Neo4Net.@unsafe.Impl.Batchimport.stats.Keys;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.Configuration.DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.staging.ControlledStep.stepWithAverageOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.staging.Step_Fields.ORDER_SEND_DOWNSTREAM;

	public class StageExecutionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOrderStepsAscending()
		 public virtual void ShouldOrderStepsAscending()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<Step<?>> steps = new java.util.ArrayList<>();
			  ICollection<Step<object>> steps = new List<Step<object>>();
			  steps.Add( stepWithAverageOf( "step1", 0, 10 ) );
			  steps.Add( stepWithAverageOf( "step2", 0, 5 ) );
			  steps.Add( stepWithAverageOf( "step3", 0, 30 ) );
			  StageExecution execution = new StageExecution( "Test", null, DEFAULT, steps, ORDER_SEND_DOWNSTREAM );

			  // WHEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<org.neo4j.helpers.collection.Pair<Step<?>,float>> ordered = execution.stepsOrderedBy(org.neo4j.unsafe.impl.batchimport.stats.Keys.avg_processing_time, true).iterator();
			  IEnumerator<Pair<Step<object>, float>> ordered = execution.StepsOrderedBy( Keys.avg_processing_time, true ).GetEnumerator();

			  // THEN
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.helpers.collection.Pair<Step<?>,float> fastest = ordered.next();
			  Pair<Step<object>, float> fastest = ordered.next();
			  assertEquals( 1f / 2f, fastest.Other(), 0f );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.helpers.collection.Pair<Step<?>,float> faster = ordered.next();
			  Pair<Step<object>, float> faster = ordered.next();
			  assertEquals( 1f / 3f, faster.Other(), 0f );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.helpers.collection.Pair<Step<?>,float> fast = ordered.next();
			  Pair<Step<object>, float> fast = ordered.next();
			  assertEquals( 1f, fast.Other(), 0f );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( ordered.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOrderStepsDescending()
		 public virtual void ShouldOrderStepsDescending()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<Step<?>> steps = new java.util.ArrayList<>();
			  ICollection<Step<object>> steps = new List<Step<object>>();
			  steps.Add( stepWithAverageOf( "step1", 0, 10 ) );
			  steps.Add( stepWithAverageOf( "step2", 0, 5 ) );
			  steps.Add( stepWithAverageOf( "step3", 0, 30 ) );
			  steps.Add( stepWithAverageOf( "step4", 0, 5 ) );
			  StageExecution execution = new StageExecution( "Test", null, DEFAULT, steps, ORDER_SEND_DOWNSTREAM );

			  // WHEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<org.neo4j.helpers.collection.Pair<Step<?>,float>> ordered = execution.stepsOrderedBy(org.neo4j.unsafe.impl.batchimport.stats.Keys.avg_processing_time, false).iterator();
			  IEnumerator<Pair<Step<object>, float>> ordered = execution.StepsOrderedBy( Keys.avg_processing_time, false ).GetEnumerator();

			  // THEN
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.helpers.collection.Pair<Step<?>,float> slowest = ordered.next();
			  Pair<Step<object>, float> slowest = ordered.next();
			  assertEquals( 3f, slowest.Other(), 0f );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.helpers.collection.Pair<Step<?>,float> slower = ordered.next();
			  Pair<Step<object>, float> slower = ordered.next();
			  assertEquals( 2f, slower.Other(), 0f );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.helpers.collection.Pair<Step<?>,float> slow = ordered.next();
			  Pair<Step<object>, float> slow = ordered.next();
			  assertEquals( 1f, slow.Other(), 0f );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.helpers.collection.Pair<Step<?>,float> alsoSlow = ordered.next();
			  Pair<Step<object>, float> alsoSlow = ordered.next();
			  assertEquals( 1f, alsoSlow.Other(), 0f );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( ordered.hasNext() );
		 }
	}

}
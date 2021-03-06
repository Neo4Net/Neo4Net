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
namespace Org.Neo4j.Helpers.Collection
{
	using Test = org.junit.jupiter.api.Test;


	using Org.Neo4j.Function;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;

	internal class IterablesTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void safeForAllShouldConsumeAllSubjectsRegardlessOfSuccess()
		 internal virtual void SafeForAllShouldConsumeAllSubjectsRegardlessOfSuccess()
		 {
			  // given
			  IList<string> seenSubjects = new List<string>();
			  IList<string> failedSubjects = new List<string>();
			  ThrowingConsumer<string, Exception> consumer = new ThrowingConsumerAnonymousInnerClass( this, seenSubjects, failedSubjects );
			  IEnumerable<string> subjects = asList( "1", "2", "3", "4", "5" );

			  // when
			  try
			  {
					Iterables.SafeForAll( consumer, subjects );
					fail( "Should have thrown exception" );
			  }
			  catch ( Exception e )
			  {
					// then good
					assertEquals( subjects, seenSubjects );
					IEnumerator<string> failed = failedSubjects.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( failed.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( e.Message, failed.next() );
					foreach ( Exception suppressed in e.Suppressed )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 assertTrue( failed.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 assertEquals( suppressed.Message, failed.next() );
					}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( failed.hasNext() );
			  }
		 }

		 private class ThrowingConsumerAnonymousInnerClass : ThrowingConsumer<string, Exception>
		 {
			 private readonly IterablesTest _outerInstance;

			 private IList<string> _seenSubjects;
			 private IList<string> _failedSubjects;

			 public ThrowingConsumerAnonymousInnerClass( IterablesTest outerInstance, IList<string> seenSubjects, IList<string> failedSubjects )
			 {
				 this.outerInstance = outerInstance;
				 this._seenSubjects = seenSubjects;
				 this._failedSubjects = failedSubjects;
			 }

			 public void accept( string s )
			 {
				  _seenSubjects.Add( s );

				  // Fail every other
				  if ( _seenSubjects.Count % 2 == 1 )
				  {
						_failedSubjects.Add( s );
						throw new Exception( s );
				  }
			 }
		 }
	}

}
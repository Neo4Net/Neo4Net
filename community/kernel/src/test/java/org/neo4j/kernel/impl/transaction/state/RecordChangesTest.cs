﻿/*
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
namespace Org.Neo4j.Kernel.impl.transaction.state
{
	using Test = org.junit.Test;

	using IntCounter = Org.Neo4j.Kernel.impl.util.statistics.IntCounter;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class RecordChangesTest
	{
		 private readonly RecordAccess_Loader<object, object> loader = new RecordAccess_LoaderAnonymousInnerClass();

		 private class RecordAccess_LoaderAnonymousInnerClass : RecordAccess_Loader<object, object>
		 {
			 public object newUnused( long o, object additionalData )
			 {
				  return o;
			 }

			 public object load( long o, object additionalData )
			 {
				  return o;
			 }

			 public void ensureHeavy( object o )
			 {

			 }

			 public object clone( object o )
			 {
				  return o.ToString();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountChanges()
		 public virtual void ShouldCountChanges()
		 {
			  // Given
			  RecordChanges<object, object> change = new RecordChanges<object, object>( loader, new IntCounter() );

			  // When
			  change.GetOrLoad( 1, null ).forChangingData();
			  change.GetOrLoad( 1, null ).forChangingData();
			  change.GetOrLoad( 2, null ).forChangingData();
			  change.GetOrLoad( 3, null ).forReadingData();

			  // Then
			  assertThat( change.ChangeSize(), equalTo(2) );
		 }
	}

}
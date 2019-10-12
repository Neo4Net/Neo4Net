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
namespace Org.Neo4j.Kernel.impl.transaction.log.entry
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class LogEntryVersionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expect = org.junit.rules.ExpectedException.none();
		 public ExpectedException Expect = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToSelectAnyVersion()
		 public virtual void ShouldBeAbleToSelectAnyVersion()
		 {
			  foreach ( LogEntryVersion version in LogEntryVersion.values() )
			  {
					// GIVEN
					sbyte code = version.byteCode();

					// WHEN
					LogEntryVersion selectedVersion = LogEntryVersion.byVersion( code );

					// THEN
					assertEquals( version, selectedVersion );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnAboutOldLogVersion()
		 public virtual void ShouldWarnAboutOldLogVersion()
		 {
			  Expect.expect( typeof( UnsupportedLogVersionException ) );
			  LogEntryVersion.byVersion( ( sbyte ) - 4 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWarnAboutNewerLogVersion()
		 public virtual void ShouldWarnAboutNewerLogVersion()
		 {
			  Expect.expect( typeof( UnsupportedLogVersionException ) );
			  LogEntryVersion.byVersion( ( sbyte ) - 42 ); // unused for now
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void moreRecent()
		 public virtual void MoreRecent()
		 {
			  assertTrue( LogEntryVersion.moreRecentVersionExists( LogEntryVersion.V2_3 ) );
			  assertTrue( LogEntryVersion.moreRecentVersionExists( LogEntryVersion.V3_0 ) );
			  assertTrue( LogEntryVersion.moreRecentVersionExists( LogEntryVersion.V2_3_5 ) );
			  assertTrue( LogEntryVersion.moreRecentVersionExists( LogEntryVersion.V3_0_2 ) );
			  assertFalse( LogEntryVersion.moreRecentVersionExists( LogEntryVersion.V3_0_10 ) );
		 }
	}

}
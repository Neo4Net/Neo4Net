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
namespace Neo4Net.Kernel.Impl.Api
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;

	public class DatabaseSchemaStateTest
	{
		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider();
		 private DatabaseSchemaState _stateStore;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void should_apply_updates_correctly()
		 public virtual void ShouldApplyUpdatesCorrectly()
		 {
			  // GIVEN
			  _stateStore.put( "key", "created_value" );

			  // WHEN
			  string result = _stateStore.get( "key" );

			  // THEN
			  assertEquals( "created_value", result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void should_flush()
		 public virtual void ShouldFlush()
		 {
			  // GIVEN
			  _stateStore.put( "key", "created_value" );

			  // WHEN
			  _stateStore.clear();

			  // THEN
			  string result = _stateStore.get( "key" );
			  assertNull( result );

			  // AND ALSO
			  _logProvider.assertExactly( inLog( typeof( DatabaseSchemaState ) ).debug( "Schema state store has been cleared." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  this._stateStore = new DatabaseSchemaState( _logProvider );
		 }
	}

}
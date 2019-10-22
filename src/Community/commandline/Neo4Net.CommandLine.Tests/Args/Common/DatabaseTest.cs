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
namespace Neo4Net.CommandLine.Args.Common
{
	using Test = org.junit.jupiter.api.Test;


	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Args = Neo4Net.Helpers.Args;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;

	internal class DatabaseTest
	{
		 private Database _arg = new Database();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void parseDatabaseShouldThrowOnPath()
		 internal virtual void ParseDatabaseShouldThrowOnPath()
		 {
			  Path path = Paths.get( "data", "databases", GraphDatabaseSettings.DEFAULT_DATABASE_NAME );
			  System.ArgumentException exception = assertThrows( typeof( System.ArgumentException ), () => _arg.parse(Args.parse("--database=" + path)) );
			  assertEquals( "'database' should be a name but you seem to have specified a path: " + path, exception.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void parseDatabaseName()
		 internal virtual void ParseDatabaseName()
		 {
			  assertEquals( "bob.db", _arg.parse( Args.parse( "--database=bob.db" ) ) );
		 }
	}

}
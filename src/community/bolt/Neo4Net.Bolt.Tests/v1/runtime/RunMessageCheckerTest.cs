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
namespace Neo4Net.Bolt.v1.runtime
{
	using ParameterizedTest = org.junit.jupiter.@params.ParameterizedTest;
	using ValueSource = org.junit.jupiter.@params.provider.ValueSource;

	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.runtime.RunMessageChecker.isBegin;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.runtime.RunMessageChecker.isCommit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.runtime.RunMessageChecker.isRollback;

	internal class RunMessageCheckerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(strings = {"begin", "BEGIN", "   begin   ", "   BeGiN ;   ", " begin     ;"}) void shouldCheckBegin(String statement)
		 internal virtual void ShouldCheckBegin( string statement )
		 {
			  assertTrue( isBegin( new RunMessage( statement ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(strings = {"commit", "COMMIT", "   commit   ", "   CoMmIt ;   ", " commiT     ;"}) void shouldCheckCommit(String statement)
		 internal virtual void ShouldCheckCommit( string statement )
		 {
			  assertTrue( isCommit( new RunMessage( statement ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(strings = {"rollback", "ROLLBACK", "   rollback   ", "   RoLlBaCk ;   ", " Rollback     ;"}) void shouldCheckRollback(String statement)
		 internal virtual void ShouldCheckRollback( string statement )
		 {
			  assertTrue( isRollback( new RunMessage( statement ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(strings = {"RETURN 1", "CREATE ()", "MATCH (n) RETURN n", "RETURN 'Hello World!'"}) void shouldCheckStatement(String statement)
		 internal virtual void ShouldCheckStatement( string statement )
		 {
			  assertFalse( isBegin( new RunMessage( statement ) ) );
			  assertFalse( isCommit( new RunMessage( statement ) ) );
			  assertFalse( isRollback( new RunMessage( statement ) ) );
		 }
	}

}
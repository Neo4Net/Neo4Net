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

	using Command = Org.Neo4j.Kernel.impl.transaction.command.Command;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsEqual.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.command.Command.Mode.fromRecordState;

	public class TestCommandMode
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInferCorrectModes()
		 public virtual void ShouldInferCorrectModes()
		 {
			  assertThat( fromRecordState( true, true ), equalTo( Command.Mode.CREATE ) );
			  assertThat( fromRecordState( false, true ), equalTo( Command.Mode.UPDATE ) );

			  assertThat( fromRecordState( false, false ), equalTo( Command.Mode.DELETE ) );
			  assertThat( fromRecordState( true, false ), equalTo( Command.Mode.DELETE ) );
		 }

	}

}